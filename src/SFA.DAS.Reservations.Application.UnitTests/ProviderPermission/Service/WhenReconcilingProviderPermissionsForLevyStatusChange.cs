using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.ProviderPermissions.Service;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.ProviderPermissions;

namespace SFA.DAS.Reservations.Application.UnitTests.ProviderPermission.Service;

public class WhenReconcilingProviderPermissionsForLevyStatusChange
{
    private Mock<IProviderPermissionRepository> _repo;
    private Mock<IReservationService> _reservationService;
    private ProviderPermissionService _service;

    [SetUp]
    public void Arrange()
    {
        _repo = new Mock<IProviderPermissionRepository>();
        _reservationService = new Mock<IReservationService>();
        _service = new ProviderPermissionService(
            _repo.Object,
            Mock.Of<ILogger<ProviderPermissionService>>(),
            _reservationService.Object);
    }

    [Test]
    public async Task Then_NonLevy_Upserts_CreateCohort_Permissions_And_Adds_To_Search_Index()
    {
        const long accountId = 42;
        var permissions = new List<Domain.Entities.ProviderPermission>
        {
            new() { AccountId = accountId, AccountLegalEntityId = 100, ProviderId = 10001, CanCreateCohort = true },
            new() { AccountId = accountId, AccountLegalEntityId = 101, ProviderId = 10002, CanCreateCohort = false },
            new() { AccountId = accountId, AccountLegalEntityId = 102, ProviderId = 10003, CanCreateCohort = true }
        };
        _repo.Setup(x => x.GetAllForAccount(accountId)).Returns(permissions);

        await _service.ReconcileForLevyStatusChange(accountId, false);

        _repo.Verify(x => x.Add(It.Is<Domain.Entities.ProviderPermission>(p =>
            p.AccountId == accountId && p.AccountLegalEntityId == 100 && p.ProviderId == 10001 && p.CanCreateCohort)), Times.Once);
        _repo.Verify(x => x.Add(It.Is<Domain.Entities.ProviderPermission>(p =>
            p.AccountId == accountId && p.AccountLegalEntityId == 102 && p.ProviderId == 10003 && p.CanCreateCohort)), Times.Once);
        _repo.Verify(x => x.Add(It.Is<Domain.Entities.ProviderPermission>(p => !p.CanCreateCohort)), Times.Never);

        _reservationService.Verify(x => x.AddProviderToSearchIndex(10001, 100), Times.Once);
        _reservationService.Verify(x => x.AddProviderToSearchIndex(10003, 102), Times.Once);
        _reservationService.Verify(x => x.DeleteProviderFromSearchIndex(It.IsAny<uint>(), It.IsAny<long>()), Times.Never);
    }

    [Test]
    public async Task Then_Levy_Upserts_CreateCohort_Permissions_And_Deletes_From_Search_Index()
    {
        const long accountId = 42;
        var permissions = new List<Domain.Entities.ProviderPermission>
        {
            new() { AccountId = accountId, AccountLegalEntityId = 100, ProviderId = 10001, CanCreateCohort = true },
            new() { AccountId = accountId, AccountLegalEntityId = 101, ProviderId = 10002, CanCreateCohort = false }
        };
        _repo.Setup(x => x.GetAllForAccount(accountId)).Returns(permissions);

        await _service.ReconcileForLevyStatusChange(accountId, true);

        _repo.Verify(x => x.Add(It.Is<Domain.Entities.ProviderPermission>(p =>
            p.AccountId == accountId && p.AccountLegalEntityId == 100 && p.ProviderId == 10001 && p.CanCreateCohort)), Times.Once);
        _reservationService.Verify(x => x.DeleteProviderFromSearchIndex(10001, 100), Times.Once);
        _reservationService.Verify(x => x.AddProviderToSearchIndex(It.IsAny<uint>(), It.IsAny<long>()), Times.Never);
    }

    [Test]
    public async Task Then_Does_Nothing_When_Account_Has_No_CreateCohort_Permissions()
    {
        const long accountId = 42;
        _repo.Setup(x => x.GetAllForAccount(accountId)).Returns(new List<Domain.Entities.ProviderPermission>
        {
            new() { AccountId = accountId, AccountLegalEntityId = 101, ProviderId = 10002, CanCreateCohort = false }
        });

        await _service.ReconcileForLevyStatusChange(accountId, false);

        _repo.Verify(x => x.Add(It.IsAny<Domain.Entities.ProviderPermission>()), Times.Never);
        _reservationService.Verify(x => x.AddProviderToSearchIndex(It.IsAny<uint>(), It.IsAny<long>()), Times.Never);
        _reservationService.Verify(x => x.DeleteProviderFromSearchIndex(It.IsAny<uint>(), It.IsAny<long>()), Times.Never);
    }
}
