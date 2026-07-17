using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.Reservations.Domain.AccountLegalEntities;

namespace SFA.DAS.Reservations.Functions.LegalEntities.Functions;

public class HandleApprenticeshipEmployerTypeChangeEvent(
    IApprenticeshipEmployerTypeChangeHandler handler,
    ILogger<ApprenticeshipEmployerTypeChangeEvent> log) : IHandleMessages<ApprenticeshipEmployerTypeChangeEvent>
{
    public async Task Handle(ApprenticeshipEmployerTypeChangeEvent message, IMessageHandlerContext context)
    {
        log.LogInformation($"NServiceBus ApprenticeshipEmployerTypeChangeEvent trigger function started execution at: {DateTime.Now} for ${nameof(message.AccountId)}:${message.AccountId}");
        await handler.Handle(message);
        log.LogInformation($"NServiceBus ApprenticeshipEmployerTypeChangeEvent trigger function finished execution at: {DateTime.Now} for ${nameof(message.AccountId)}:${message.AccountId}");
    }
}
