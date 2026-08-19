using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Entities;
using SFA.DAS.Reservations.Domain.ProviderPermissions;

namespace SFA.DAS.Reservations.Data.Repository
{
    public class ProviderPermissionRepository(IReservationsDataContext dataContext) : IProviderPermissionRepository
    {
        public IEnumerable<ProviderPermission> GetAllWithCreateCohortPermission()
        {
            return dataContext.ProviderPermissions
                .Where(c => c.CanCreateCohort);
        }

        public IEnumerable<ProviderPermission> GetAllForAccountLegalEntity(long accountLegalEntityId)
        {
            return dataContext.ProviderPermissions
                .Where(permission => permission.AccountLegalEntityId == accountLegalEntityId)
                .Where(permission => permission.CanCreateCohort);
        }

        public IEnumerable<ProviderPermission> GetAllForAccount(long accountId)
        {
            return dataContext.ProviderPermissions
                .Where(permission => permission.AccountId == accountId);
        }

        public async Task Add(ProviderPermission permission)
        {
            var existingPermission = await dataContext.ProviderPermissions.FindAsync(permission.AccountId,
                permission.AccountLegalEntityId, permission.ProviderId);

            if (existingPermission == null)
            {
                await dataContext.ProviderPermissions.AddAsync(permission);
            }
            else
            {
                existingPermission.CanCreateCohort = permission.CanCreateCohort;

            }

            dataContext.SaveChanges();
        }
    }
}