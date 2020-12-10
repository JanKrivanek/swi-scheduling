using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SolarWinds.ActivityContext.Abstractions;
using SolarWinds.ActivityContext.AspNet;
using SolarWinds.Messaging.Abstractions;
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
                .AddJsonFile("appsettings.json"); //or what ever file you have the settings
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
            serviceCollection.AddScoped<SchedulerQueueSettings>(_ => SchedulerQueueSettings.DefaultSettingsForElasticMQ);
            serviceCollection.AddScoped<ITenantHelper, TenantHelper.TenantHelper>();
            serviceCollection.AddScoped<TenantsProducer>();

            return serviceCollection.BuildServiceProvider();
        }

    }

    public class TenantsProducer: BusUserBase
    {
        private readonly ITenantHelper tenantHelper;

        public TenantsProducer(SchedulerQueueSettings settings, ILogger<TenantsProducer> logger, ITenantHelper tenantHelper) 
            : base(settings, logger)
        {
            this.tenantHelper = tenantHelper;
        }

        protected override void OnStart(IMessageBus messageBus)
        {
            InsertWork().Wait();
        }

        private async Task InsertWork()
        {
            CancellationToken cancellationToken = CancellationToken.None;
            var tenantIds = await tenantHelper.GetAllTenantIds(cancellationToken).ConfigureAwait(false);

            await Task.WhenAll(tenantIds.Select(id => base.MessageBus.PublishAsync(Topic, id, cancellationToken)))
                .ConfigureAwait(false);
        }
    }

    //public class Test
    //{
    //    private ISlingAuthUser slingAuthUser;
    //    private IActivityImpersonationProvider impersonationProvider;

    //    public Test(ISlingAuthUser slingAuthUser, IActivityImpersonationProvider impersonationProvider)
    //    {
    //        this.slingAuthUser = slingAuthUser;
    //        this.impersonationProvider = impersonationProvider;
    //    }

    //    public async Task DoWork(CancellationToken token)
    //    {
    //        await impersonationProvider.RunAsService(DoWorkInternal, ActivityConfiguration.Default, token);

    //        await impersonationProvider.RunAsTenant(0, WhatToDoPerTenant,
    //            ActivityConfiguration.Default,
    //            token);
    //    }

    //    private async Task DoWorkInternal(CancellationToken token)
    //    {
    //        var tentInfos = await slingAuthUser.GetAllTenantsAsync(token);
    //        foreach (var tenantInfo in tentInfos)
    //        {
    //            Console.WriteLine(tenantInfo.TenantId);

    //            //await impersonationProvider.RunAsTenant(tenantInfo.TenantId, WhatToDoPerTenant,
    //            //    ActivityConfiguration.Default,
    //            //    token);

    //        }
    //    }

    //    private async Task WhatToDoPerTenant(CancellationToken token)
    //    {


    //        Console.WriteLine("Current tenant Id from ambient context: {0}",
    //            AuthenticationContext.Current.Slingshot.TenantId);
    //    }

    //    public void Foo()
    //    {
            
    //    }
    //}

    //public class Consumer
    //{
    //    private IActivityImpersonationProvider impersonationProvider;

    //    private void ConsumeNext(int tenantId, CancellationToken token)
    //    {
    //        impersonationProvider.RunAsTenant(tenantId, WhatToDo, ActivityConfiguration.Default, token);
    //    }

    //    private async Task WhatToDo(CancellationToken token)
    //    {

    //    }
    //}
}
