using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.Reservations.Domain.AccountLegalEntities;
using SFA.DAS.Reservations.Domain.Accounts;

namespace SFA.DAS.Reservations.Application.AccountLegalEntities.Handlers
{
    public class ApprenticeshipEmployerTypeChangeHandler(
        IAccountsService accountsService,
        ILogger<ApprenticeshipEmployerTypeChangeHandler> logger)
        : IApprenticeshipEmployerTypeChangeHandler
    {
        public async Task Handle(ApprenticeshipEmployerTypeChangeEvent apprenticeshipEmployerTypeChangeEvent)
        {
            if (apprenticeshipEmployerTypeChangeEvent.ApprenticeshipEmployerType == ApprenticeshipEmployerType.Unknown)
            {
                return;
            }

            var isLevy = apprenticeshipEmployerTypeChangeEvent.ApprenticeshipEmployerType == ApprenticeshipEmployerType.Levy;

            try
            {
                await accountsService.UpdateLevyStatus(apprenticeshipEmployerTypeChangeEvent.AccountId, isLevy);
            }
            catch (DbUpdateException e)
            {
                logger.LogWarning("Could not update levy status for account {AccountId}", apprenticeshipEmployerTypeChangeEvent.AccountId, e);
                throw;
            }
        }
    }
}
