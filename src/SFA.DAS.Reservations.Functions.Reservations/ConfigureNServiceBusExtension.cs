using System.Net;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using SFA.DAS.Notifications.Messages.Commands;
using SFA.DAS.Reservations.Infrastructure.NServiceBus;

namespace SFA.DAS.Reservations.Functions.Reservations;

public static class ConfigureNServiceBusExtension
{
    public static IHostBuilder ConfigureNServiceBus(this IHostBuilder hostBuilder, string endpointName)
    {
        hostBuilder.UseNServiceBus((config, endpointConfiguration) =>
        {
            endpointConfiguration.AdvancedConfiguration.EnableInstallers();
            endpointConfiguration.AdvancedConfiguration.SendFailedMessagesTo($"{endpointName}-error");
            endpointConfiguration.AdvancedConfiguration.UseMessageConventions();
            endpointConfiguration.AdvancedConfiguration.UseNewtonsoftJsonSerializer();

            endpointConfiguration.Routing.RouteToEndpoint(typeof(SendEmailCommand), "SFA.DAS.Notifications.MessageHandlers");

            var decodedLicence = WebUtility.HtmlDecode(config["ReservationsJobs:NServiceBusLicense"]);
            if(!string.IsNullOrWhiteSpace(decodedLicence)) endpointConfiguration.AdvancedConfiguration.License(decodedLicence);
        });

        return hostBuilder;
    }
}