using System;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SolarWinds.ActivityContext.AspNet;
using SolarWinds.SlingAuth.Client.AspNet;
using SolarWinds.Swicus.Client;
using SolarWinds.Swinx.Client;
using SolarWinds.TenantAffinity;
using SolarWinds.UniversalMessageFormat.Parser;
using TenantHelper;

namespace WorkExecutor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Continuous work executor running");

            IServiceProvider container = SetupDi();
            var serviceScopeFactory = container.GetRequiredService<IServiceScopeFactory>();
            CancellationToken ct = CancellationToken.None;
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var executor = scope.ServiceProvider.GetService<Executor>();
                executor.Start();
                
                //just indefinitely wait - in reality service would wait on exit signal
                ct.WaitHandle.WaitOne();
                //Console.ReadKey();
                executor.Dispose();
            }
        }

        static IServiceProvider SetupDi()
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
                .AddEnvironmentVariables();
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
            serviceCollection.AddScoped<Executor>();

            return serviceCollection.BuildServiceProvider();
        }
    }
}
