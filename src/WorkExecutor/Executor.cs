using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SolarWinds.ActivityContext.Abstractions;
using SolarWinds.MessageBus.Sqs;
using SolarWinds.Messaging.Abstractions;
using SolarWinds.Messaging.Utils;
using TenantHelper;

namespace WorkExecutor
{
    public class Executor: BusUserBase
    {
        private readonly ITenantHelper tenantHelper;

        public Executor(SchedulerQueueSettings settings, ILogger<Executor> logger, ITenantHelper tenantHelper)
        :base(settings, logger)
        {
            this.tenantHelper = tenantHelper;
        }

        protected override void OnStart(IMessageBus messageBus)
        {
            messageBus.Subscribe<long>(Topic, OnMsgAction);
        }

        private void OnMsgAction(long tenantId, IAck ack)
        {
            tenantHelper.RunAsTenant(tenantId, RunAsTenantInternal).Wait();
            ack.Ack();
        }

        private async Task<bool> RunAsTenantInternal(CancellationToken cancellationToken)
        {
            Console.WriteLine("Current tenant Id from ambient context: {0}",
                AuthenticationContext.Current.Slingshot.TenantId);

            return true;
        }
    }
}