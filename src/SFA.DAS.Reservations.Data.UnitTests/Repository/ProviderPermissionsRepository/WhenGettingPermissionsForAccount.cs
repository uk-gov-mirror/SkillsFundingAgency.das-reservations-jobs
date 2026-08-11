using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Data.UnitTests.DatabaseMock;
using SFA.DAS.Reservations.Domain.Entities;

namespace SFA.DAS.Reservations.Data.UnitTests.Repository.ProviderPermissionsRepository;

public class WhenGettingPermissionsForAccount
{
    [Test]
    public void Then_Returns_All_Permissions_For_The_Account()
    {
        var permissions = new List<ProviderPermission>
        {
            new() { AccountId = 1, AccountLegalEntityId = 1, ProviderId = 1, CanCreateCohort = true },
            new() { AccountId = 1, AccountLegalEntityId = 2, ProviderId = 2, CanCreateCohort = false },
            new() { AccountId = 2, AccountLegalEntityId = 3, ProviderId = 1, CanCreateCohort = true }
        };

        var dataContext = new Mock<IReservationsDataContext>();
        dataContext.Setup(x => x.ProviderPermissions).ReturnsDbSet(permissions);
        var repository = new Data.Repository.ProviderPermissionRepository(dataContext.Object);

        var actual = repository.GetAllForAccount(1).ToList();

        actual.Should().BeEquivalentTo(permissions.Where(p => p.AccountId == 1));
    }
}
