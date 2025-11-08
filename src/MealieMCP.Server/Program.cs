using MealieMCP.Server.Config;
using MealieMCP.Server.Tools;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using System;
using System.IO;
using System.Net.Http.Headers;

namespace MealieMCP.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add configuration from environment variables with optional prefix
            builder.Configuration.AddEnvironmentVariables("MealiMcp__");

            // Ensure log directory exists for the File sink
            try
            {
                var logDir = Path.Combine(builder.Environment.ContentRootPath, "Logs");
                Directory.CreateDirectory(logDir);
            }
            catch
            {
                // ignore directory creation errors, Serilog will attempt to create files itself
            }

            // Configure Serilog from configuration
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            builder.Host.UseSerilog();

            builder.Services.Configure<MealieSettings>(builder.Configuration.GetSection("MealieSettings"));
            builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<MealieSettings>>().Value);

            builder.Services.AddKiotaHandlers();

            // Configure HttpClient for Mealie with Polly policies
            builder.Services.AddHttpClient("MealieClient", (sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<MealieSettings>>().Value;
                if (Uri.TryCreate(settings.Url, UriKind.Absolute, out var baseUri))
                {
                    client.BaseAddress = baseUri;
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy()).AttachKiotaHandlers();

            builder.Services.AddTransient<MealiClientFactory>();
            builder.Services.AddTransient(sp => sp.GetRequiredService<MealiClientFactory>().GetClient());

            builder.Services.AddControllers();
            builder.Services.AddMcpServer().WithHttpTransport().WithTools<ReceipeTools>();


            var app = builder.Build();

            app.UseSerilogRequestLogging();

            app.UseCors(cors => cors
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .SetIsOriginAllowed(_ => true));

            app.MapMcp();

            app.UseAuthorization();


            app.MapControllers();

            try
            {
                Log.Information("Starting up");
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            // Exponential backoff retry for transient errors
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(new[] {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(5)
                }, onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    Log.Warning("Delaying for {Delay}ms, then making retry {Retry}", timespan.TotalMilliseconds, retryAttempt);
                });
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30), onBreak: (outcome, timespan) =>
                {
                    Log.Warning("Circuit breaker opened for {Duration} due to {Error}", timespan, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                }, onReset: () => Log.Information("Circuit breaker reset"));
        }
    }
}
