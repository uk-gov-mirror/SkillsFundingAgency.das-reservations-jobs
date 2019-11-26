﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Data;
using SFA.DAS.Reservations.Domain.Reservations;
using TechTalk.SpecFlow;

namespace SFA.DAS.Reservations.Functions.Reservations.AcceptanceTests.Steps
{
    [Binding]
    public class ApprenticeshipDeletedSteps : StepsBase
    {
        public ApprenticeshipDeletedSteps(TestServiceProvider serviceProvider, TestData testData) : base(serviceProvider, testData)
        {
        }

        [Given(@"I have a (.*) reservation")]
        public void GivenIHaveAReservation(string reservationStatus)
        {
            TestData.ReservationId = Guid.NewGuid();

            Enum.TryParse(reservationStatus, true, out ReservationStatus status);

            var dbContext = Services.GetService<ReservationsDataContext>();

            var reservation = new Domain.Entities.Reservation
            {
                AccountId = 1,
                AccountLegalEntityId = TestData.AccountLegalEntity.AccountLegalEntityId,
                AccountLegalEntityName = TestData.AccountLegalEntity.AccountLegalEntityName,
                CourseId = TestData.Course.CourseId,
                CreatedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(2),
                IsLevyAccount = false,
                Status = (short)status,
                StartDate = DateTime.UtcNow.AddMonths(1),
                Id = TestData.ReservationId,
                UserId = Guid.NewGuid()
            };

            dbContext.Reservations.Add(reservation);
            dbContext.SaveChanges();
        }

        [When(@"a delete apprenticeship event is triggered")]
        public void WhenADeleteApprenticeshipEventIsTriggered()
        {
            var handler = Services.GetService<IApprenticeshipDeletedHandler>();
            handler.Handle(TestData.ReservationId).Wait();
        }
        
        [Then(@"the reservation status will be pending")]
        public void ThenTheReservationStatusWillBePending()
        {
            var dbContext = Services.GetService<ReservationsDataContext>();
            var reservation = dbContext.Reservations.Find(TestData.ReservationId);
            Assert.AreEqual(ReservationStatus.Pending, (ReservationStatus)reservation.Status);
            var reservationIndexRepository = Services.GetService<IReservationIndexRepository>();
            var mock = Mock.Get(reservationIndexRepository);
            mock.Verify(x => x.SaveReservationStatus(TestData.ReservationId, ReservationStatus.Pending), Times.Once);
        }
        
        [Then(@"the reservation does not cause a re-queue")]
        public void ThenTheReservationDoesNotCauseARe_Queue()
        {
            ScenarioContext.Current.Pending();
        }
    }
}
