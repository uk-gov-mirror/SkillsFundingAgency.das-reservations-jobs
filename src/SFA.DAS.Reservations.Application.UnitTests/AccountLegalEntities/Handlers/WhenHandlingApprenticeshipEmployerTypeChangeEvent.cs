using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.Reservations.Application.AccountLegalEntities.Handlers;
using SFA.DAS.Reservations.Domain.Accounts;
using SFA.DAS.Reservations.Domain.ProviderPermissions;

namespace SFA.DAS.Reservations.Application.UnitTests.AccountLegalEntities.Handlers;

public class WhenHandlingApprenticeshipEmployerTypeChangeEvent
{
    private Mock<IAccountsService> _service;
    private Mock<IProviderPermissionService> _providerPermissionService;
    private ApprenticeshipEmployerTypeChangeHandler _handler;

    [SetUp]
    public void Arrange()
    {
        _service = new Mock<IAccountsService>();
        _providerPermissionService = new Mock<IProviderPermissionService>();
        _handler = new ApprenticeshipEmployerTypeChangeHandler(
            _service.Object,
            _providerPermissionService.Object,
            Mock.Of<ILogger<ApprenticeshipEmployerTypeChangeHandler>>());
    }

    [Test]
    public async Task Then_Levy_Type_Updates_Status_To_True_And_Reconciles_Permissions()
    {
        var message = new ApprenticeshipEmployerTypeChangeEvent
        {
            AccountId = 5,
            ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy,
            Created = DateTime.Now
        };

        await _handler.Handle(message);

        _service.Verify(x => x.UpdateLevyStatus(message.AccountId, true), Times.Once);
        _providerPermissionService.Verify(x => x.ReconcileForLevyStatusChange(message.AccountId, true), Times.Once);
    }

    [Test]
    public async Task Then_NonLevy_Type_Updates_Status_To_False_And_Reconciles_Permissions()
    {
        var message = new ApprenticeshipEmployerTypeChangeEvent
        {
            AccountId = 5,
            ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
            Created = DateTime.Now
        };

        await _handler.Handle(message);

        _service.Verify(x => x.UpdateLevyStatus(message.AccountId, false), Times.Once);
        _providerPermissionService.Verify(x => x.ReconcileForLevyStatusChange(message.AccountId, false), Times.Once);
    }

    [Test]
    public async Task Then_Unknown_Type_Is_Ignored()
    {
        var message = new ApprenticeshipEmployerTypeChangeEvent
        {
            AccountId = 5,
            ApprenticeshipEmployerType = ApprenticeshipEmployerType.Unknown,
            Created = DateTime.Now
        };

        await _handler.Handle(message);

        _service.Verify(x => x.UpdateLevyStatus(It.IsAny<long>(), It.IsAny<bool>()), Times.Never);
        _providerPermissionService.Verify(x => x.ReconcileForLevyStatusChange(It.IsAny<long>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void Then_Will_Throw_Exception_If_Database_Update_Fails()
    {
        var message = new ApprenticeshipEmployerTypeChangeEvent
        {
            AccountId = 5,
            ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy,
            Created = DateTime.Now
        };

        _service.Setup(x => x.UpdateLevyStatus(It.IsAny<long>(), It.IsAny<bool>()))
            .ThrowsAsync(new DbUpdateException("Failed", (Exception)null));

        var action = () => _handler.Handle(message);
        action.Should().ThrowAsync<DbUpdateException>();
        _providerPermissionService.Verify(x => x.ReconcileForLevyStatusChange(It.IsAny<long>(), It.IsAny<bool>()), Times.Never);
    }
}
