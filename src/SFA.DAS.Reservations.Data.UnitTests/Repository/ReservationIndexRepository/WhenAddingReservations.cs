﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nest;
using NUnit.Framework;
using SFA.DAS.Reservations.Data.Registry;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Data.UnitTests.Repository.ReservationIndexRepository
{
    public class WhenAddingReservations
    {
        private Mock<IElasticClient> _clientMock;
        private Mock<IIndexRegistry> _registryMock;
        private Data.Repository.ReservationIndexRepository _repository;

        [SetUp]
        public void Init()
        {
            _clientMock = new Mock<IElasticClient>();
            _registryMock = new Mock<IIndexRegistry>();
            _repository = new Data.Repository.ReservationIndexRepository(_clientMock.Object, _registryMock.Object);
        }

        [Test]
        public async Task ThenWillIndexManyReservationAtOnce()
        {
            //Arrange
            var reservations = new List<ReservationIndex>
            {
                new ReservationIndex {Id = Guid.NewGuid(), Status = 1},
                new ReservationIndex {Id = Guid.NewGuid(), Status = 1}
            };

            //Act
            await _repository.Add(reservations);

            //Assert
            _clientMock.Verify(c => c.BulkAsync(It.IsAny<BulkRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ThenWillIndexASingleReservation()
        {
            //Arrange
            var reservation = new ReservationIndex {Id = Guid.NewGuid(), Status = 1};

            //Act
            await _repository.Add(reservation);

            //Assert
            _clientMock.Verify(c => c.IndexAsync(It.Is<IndexRequest<ReservationIndex>>(r => r.Document.Equals(reservation)),It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
