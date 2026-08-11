using System.Threading.Tasks;
using SFA.DAS.ProviderRelationships.Messages.Events;

namespace SFA.DAS.Reservations.Domain.ProviderPermissions
{
    public interface IProviderPermissionService
    {
        Task AddProviderPermission(UpdatedPermissionsEvent updateEvent);
        Task ReconcileForLevyStatusChange(long accountId, bool isLevy);
    }
}
