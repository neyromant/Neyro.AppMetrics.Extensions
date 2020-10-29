using System;
using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.Reporting.InfluxDB;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ExampleApp
{
    public class Program
    {
        private static IMetricsRoot Metrics { get; set; }
        public static void Main(string[] args)
        {
            Metrics = AppMetrics.CreateDefaultBuilder()
                .Report.ToInfluxDb(options =>
                {
                    options.InfluxDb = new InfluxDbOptions
                    {
                        BaseUri = new Uri("http://localhost:8086"),
                        Database = "Test"
                    };
                })
                .Build();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureServices(services => services.AddHostedService(sp =>
                        new Neyro.AppMetrics.Extensions.EventCountersCollector(
                            sp.GetRequiredService<IMetricsRoot>(),
                            new Neyro.AppMetrics.Extensions.EventCountersCollectorOptions
                            {
                                RefreshIntervalSec = 5,
                                EnabledSources = new[] {"System.Runtime" },
                                SetTagsFromMetadata = true
                            }
                        )))
                        .ConfigureMetrics(Metrics)
                        .UseMetrics(); ;
                });
    }
}
