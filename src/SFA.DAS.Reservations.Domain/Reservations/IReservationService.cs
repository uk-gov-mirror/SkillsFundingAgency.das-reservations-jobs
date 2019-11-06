﻿using System;
using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Domain.Reservations
{
    public interface IReservationService
    {
        Task UpdateReservationStatus(Guid reservationId, ReservationStatus status);
        Task RefreshReservationIndex();
        Task AddReservationToReservationsIndex(Reservation reservation);
        Task DeleteProviderFromSearchIndex(uint ukPrn, long accountLegalEntityId);
    }
}
