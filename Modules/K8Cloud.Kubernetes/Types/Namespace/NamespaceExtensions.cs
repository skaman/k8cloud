using AutoMapper;
using HotChocolate;
using HotChocolate.Types;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Contracts.Kubernetes.Enums;
using K8Cloud.Kubernetes.Extensions;
using K8Cloud.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace K8Cloud.Kubernetes.Types.Namespace;

[ExtendObjectType(typeof(NamespaceResource))]
internal class NamespaceExtensions
{
    /// <summary>
    /// Namespace status.
    /// </summary>
    /// <param name="namespaceRecord">Namespace record.</param>
    /// <param name="dbContext">Database context.</param>
    /// <param name="mapper">Mapper.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<SyncInfo> GetSyncInfo(
        [Parent] NamespaceResource namespaceRecord,
        K8CloudDbContext dbContext,
        [Service] IMapper mapper,
        CancellationToken cancellationToken
    )
    {
        var syntState = await dbContext
            .NamespaceSyncStatesReadOnly()
            .SingleOrDefaultAsync(c => c.CorrelationId == namespaceRecord.Id, cancellationToken)
            .ConfigureAwait(false);

        if (syntState == null)
        {
            return new SyncInfo { Status = SyncStatus.Unknown, ErrorStatus = null };
        }

        return mapper.Map<SyncInfo>(syntState);
    }
}
