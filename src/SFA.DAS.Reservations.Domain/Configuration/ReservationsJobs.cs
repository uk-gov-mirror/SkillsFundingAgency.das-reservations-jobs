﻿namespace SFA.DAS.Reservations.Domain.Configuration
{
    public class ReservationsJobs
    {
        public string ApprenticeshipBaseUrl { get; set; }
        public string ConnectionString { get; set; }
        public string NServiceBusConnectionString { get; set; }
        public string AzureWebJobsStorage { get; set; }
        public string ElasticSearchUsername { get; set; }
        public string ElasticSearchPassword { get; set; }
        public string ElasticSearchServerUrl { get; set; }
        public string ReservationCreatedEmailTemplateId { get; set; }
    }
}
