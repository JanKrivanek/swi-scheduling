using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SolarWinds.ActivityContext.Abstractions;
using SolarWinds.ActivityContext.AspNet;
using SolarWinds.SlingAuth.Client;
using SolarWinds.SlingAuth.Client.AspNet;
//using SolarWinds.SlingAuth.Client.GrpcCore;
using SolarWinds.Swicus.Client;
using SolarWinds.Swinx.Client;
using SolarWinds.TenantAffinity;
using SolarWinds.TenantAffinity.Abstractions;
using SolarWinds.Tracing.Abstractions;
using TenantHelper;

namespace WorkInserter
{
    internal class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static async Task Main(string[] args)
        {
            var container = SetupDi();

            var serviceScopeFactory = container.GetRequiredService<IServiceScopeFactory>();
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var producer = scope.ServiceProvider.GetService<TenantsProducer>();

                producer.Start();
                producer.Dispose();
            }
        }

        private static IServiceProvider SetupDi()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(opt =>
            {
                opt.AddConsole(copt => { copt.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fffffff"; });
                opt.SetMinimumLevel(LogLevel.Trace);
            });

            var builder = new ConfigurationBuilder()
                //.SetBasePath("path here") //<--You would need to set the path
                .AddJsonFile("appsettings.json") //or what ever file you have the settings
                .AddEnvironmentVariables()
                ;

            IConfiguration configuration = builder.Build();

            serviceCollection.AddScoped<IConfiguration>(_ => configuration);

            //serviceCollection.ConfigureSwinxClient();
            //serviceCollection.ConfigureSwicusClient();
            serviceCollection.AddSingleton<IMemoryCache, MemoryCache>();
            serviceCollection.AddActivityContextServices();
            serviceCollection.ConfigureSwinxClient();
            serviceCollection.ConfigureSwicusClient();
            serviceCollection.AddSlingAuth();
            serviceCollection.AddTenantAffinity();
            //I'm lazy to make this working - just binding to instance directly below
            //serviceCollection.Configure<SchedulerQueueSettings>(o => configuration.GetSection("SchedulerQueueSettings").Bind(o));
            serviceCollection.AddScoped<SchedulerQueueSettings>(_ => configuration.GetSection("SchedulerQueueSettings").Get<SchedulerQueueSettings>());
            serviceCollection.AddScoped<ITenantHelper, TenantHelper.TenantHelper>();
            serviceCollection.AddScoped<TenantsProducer>();

            return serviceCollection.BuildServiceProvider();
        }

    }
}
