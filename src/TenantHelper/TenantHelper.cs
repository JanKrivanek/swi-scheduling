using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SolarWinds.SlingAuth.Client;
using SolarWinds.TenantAffinity.Abstractions;

namespace TenantHelper
{
    public interface ITenantHelper
    {
        Task<IEnumerable<long>> GetAllTenantIds(CancellationToken cancellationToken);

        Task<TRetval> RunAsTenant<TRetval>(
            long tenantId,
            Func<CancellationToken, Task<TRetval>> activity,
            ActivityConfiguration configuration = null,
            CancellationToken cancellationToken = default(CancellationToken));
    }

    public class TenantHelper : ITenantHelper
    {
        private readonly ISlingAuthUser slingAuthUser;
        private readonly IActivityImpersonationProvider impersonationProvider;

        public TenantHelper(ISlingAuthUser slingAuthUser, IActivityImpersonationProvider impersonationProvider)
        {
            this.slingAuthUser = slingAuthUser;
            this.impersonationProvider = impersonationProvider;
        }

        public async Task<IEnumerable<long>> GetAllTenantIds(CancellationToken cancellationToken)
        {
            var tenantInfos = await impersonationProvider.RunAsService(slingAuthUser.GetAllTenantsAsync,
                ActivityConfiguration.Default, cancellationToken).ConfigureAwait(false);
            return tenantInfos.Select(ti => ti.TenantId);
        }

        public Task<TRetval> RunAsTenant<TRetval>(
            long tenantId,
            Func<CancellationToken, Task<TRetval>> activity,
            ActivityConfiguration configuration = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return impersonationProvider.RunAsTenant(tenantId, activity, configuration, cancellationToken);
        }
    }
}
