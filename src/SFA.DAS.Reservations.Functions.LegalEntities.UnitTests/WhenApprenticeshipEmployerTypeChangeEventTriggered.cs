using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.Reservations.Domain.AccountLegalEntities;
using SFA.DAS.Reservations.Functions.LegalEntities.Functions;

namespace SFA.DAS.Reservations.Functions.LegalEntities.UnitTests
{
    [TestFixture]
    public class WhenApprenticeshipEmployerTypeChangeEventTriggered
    {
        [Test]
        public async Task ThenMessageIsHandled()
        {
            var message = new ApprenticeshipEmployerTypeChangeEvent
            {
                AccountId = 1234345,
                ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy,
                Created = DateTime.Now.AddDays(-1)
            };
            var handler = new Mock<IApprenticeshipEmployerTypeChangeHandler>();
            var logger = new Mock<ILogger<ApprenticeshipEmployerTypeChangeEvent>>();
            var sut = new HandleApprenticeshipEmployerTypeChangeEvent(handler.Object, logger.Object);

            await sut.Handle(message, Mock.Of<IMessageHandlerContext>());

            handler.Verify(
                x => x.Handle(It.Is<ApprenticeshipEmployerTypeChangeEvent>(e =>
                    e.AccountId == message.AccountId &&
                    e.ApprenticeshipEmployerType == message.ApprenticeshipEmployerType &&
                    e.Created == message.Created)),
                Times.Once);
        }
    }
}
