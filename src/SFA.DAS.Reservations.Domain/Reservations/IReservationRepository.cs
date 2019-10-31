﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Domain.Reservations
{
    public interface IReservationRepository
    {
        Task SaveStatus(Guid reservationId, ReservationStatus status);
        IEnumerable<Entities.Reservation> GetAllNonLevyForAccountLegalEntity(long accountLegalEntityId);
    }
}
