namespace CustomersApi.Infrastructure.Observability;

using System.Diagnostics;
using System.Reflection;
using CustomersModule.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

public static class ObservabilityExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddCustomersApiObservability()
        {
            string serviceName = "CustomersApi";
            
            // Read configuration
            IConfiguration config = builder.Configuration;
            List<string> excludePaths = config.GetSection("ObservabilityFilter:ExcludePaths").Get<List<string>>() ?? [];
            List<string> excludePathsUnlessError = config.GetSection("ObservabilityFilter:ExcludePathsUnlessError").Get<List<string>>() ?? [];

            // Configure Serilog from configuration
            LoggerConfiguration logConfig = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", serviceName)
                .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
                .Filter.ByExcluding(logEvent =>
                {
                    // Exclude configured paths
                    if (logEvent.Properties.TryGetValue("RequestPath", out LogEventPropertyValue? requestPath))
                    {
                        string path = requestPath.ToString().Trim('"');
                        
                        // Always exclude these paths
                        if (excludePaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                        {
                            return true;
                        }

                        // Exclude unless error
                        if (excludePathsUnlessError.Contains(path) && logEvent.Level < LogEventLevel.Error)
                        {
                            return true;
                        }
                    }

                    return false;
                });

            Log.Logger = logConfig.CreateLogger();
            builder.Host.UseSerilog();

            // Configure OpenTelemetry
            string serviceVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
            string jaegerEndpoint = config["Observability:Jaeger:Endpoint"] 
                ?? "http://jaeger-collector.monitoring.svc.cluster.local:4317";

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService(serviceName, serviceVersion: serviceVersion)
                    .AddAttributes([
                        new KeyValuePair<string, object>("deployment.environment", builder.Environment.EnvironmentName),
                        new KeyValuePair<string, object>("service.namespace", "CustomersApi")
                    ]))
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = httpContext =>
                        {
                            // Don't trace excluded paths
                            string path = httpContext.Request.Path.Value ?? string.Empty;
                            return !excludePaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)) &&
                                   !excludePathsUnlessError.Contains(path);
                        };
                        options.EnrichWithHttpRequest = (activity, request) =>
                        {
                            activity.SetTag("http.request_id", request.HttpContext.TraceIdentifier);
                            activity.SetTag("http.client_ip", request.HttpContext.Connection.RemoteIpAddress?.ToString());
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddSource(serviceName)
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(jaegerEndpoint);
                    }))
                .WithMetrics(metrics => metrics
                    .AddMeter("Microsoft.EntityFrameworkCore")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(serviceName)
                    .AddPrometheusExporter());

            // Add health checks
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<CustomersDbContext>("database");

            return builder;
        }
    }

    extension(WebApplication app)
    {
        public WebApplication UseCustomersApiObservability()
        {
            // Add Serilog HTTP request logging
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            });

            // Map health check endpoints for Kubernetes
            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false // No checks, just returns 200 if app is running
            });

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => true // All checks must pass
            });

            // Expose Prometheus metrics at /metrics
            app.MapPrometheusScrapingEndpoint();

            return app;
        }
    }
}
