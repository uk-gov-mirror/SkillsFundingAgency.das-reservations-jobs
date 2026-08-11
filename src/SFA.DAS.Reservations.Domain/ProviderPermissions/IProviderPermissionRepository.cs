using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Entities;

namespace SFA.DAS.Reservations.Domain.ProviderPermissions
{
    public interface IProviderPermissionRepository
    {
        Task Add(ProviderPermission permission);
        IEnumerable<ProviderPermission> GetAllWithCreateCohortPermission();
        IEnumerable<ProviderPermission> GetAllForAccountLegalEntity(long accountLegalEntityId);
        IEnumerable<ProviderPermission> GetAllForAccount(long accountId);
    }
}
