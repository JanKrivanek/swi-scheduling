using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SolarWinds.Messaging.Abstractions;
using TenantHelper;

namespace WorkInserter
{
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

            await Task.WhenAll(tenantIds.Select(id => base.MessageBus.PublishAsync<long>(Topic, id, cancellationToken)))
                .ConfigureAwait(false);
        }
    }
}