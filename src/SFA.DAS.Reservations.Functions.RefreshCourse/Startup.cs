﻿using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.Reservations.Application.RefreshCourses.Handlers;
using SFA.DAS.Reservations.Application.RefreshCourses.Services;
using SFA.DAS.Reservations.Data;
using SFA.DAS.Reservations.Data.Repository;
using SFA.DAS.Reservations.Domain.Configuration;
using SFA.DAS.Reservations.Domain.Infrastructure;
using SFA.DAS.Reservations.Domain.RefreshCourse;
using SFA.DAS.Reservations.Functions.RefreshCourse;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.DependencyInjection;
using SFA.DAS.Reservations.Infrastructure.Logging;

[assembly: WebJobsStartup(typeof(Startup))]

namespace SFA.DAS.Reservations.Functions.RefreshCourse
{
    internal class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddExecutionContextBinding();
            builder.AddDependencyInjection<ServiceProviderBuilder>();
        }
    }

    internal class ServiceProviderBuilder : IServiceProviderBuilder
    {
        private readonly ILoggerFactory _loggerFactory;
        public IConfiguration Configuration { get; }

        public ServiceProviderBuilder(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _loggerFactory = loggerFactory;

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", true)
                .AddEnvironmentVariables()
                .AddAzureTableStorageConfiguration(
                    configuration["ConfigurationStorageConnectionString"],
                    configuration["ConfigNames"].Split(','),
                    configuration["EnvironmentName"],
                    configuration["Version"]
                )
                .Build();

            Configuration = config;
        }

        public IServiceProvider Build()
        {
            var services = new ServiceCollection();

            services.Configure<ReservationsJobs>(Configuration.GetSection("ReservationsJobs"));
            services.AddSingleton(cfg => cfg.GetService<IOptions<ReservationsJobs>>().Value);

            var serviceProvider = services.BuildServiceProvider();

            var config = serviceProvider.GetService<ReservationsJobs>();

            var nLogConfiguration = new NLogConfiguration();

            services.AddLogging((options) =>
            {
                options.AddConfiguration(Configuration.GetSection("Logging"));
                options.SetMinimumLevel(LogLevel.Information);
                
                options.AddConsole();
                options.AddDebug();
                nLogConfiguration.ConfigureNLog(Configuration);
                options.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
            });

            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

            services.AddSingleton(_ => _loggerFactory.CreateLogger(LogCategories.CreateFunctionUserCategory("Common")));

            services.AddTransient<IStandardApiClient>(x => new StandardApiClient(config.ApprenticeshipBaseUrl));
            services.AddTransient<IFrameworkApiClient>(x => new FrameworkApiClient(config.ApprenticeshipBaseUrl));

            services.AddTransient<IApprenticeshipCourseService, ApprenticeshipCoursesService>();
            services.AddTransient<ICourseService, CourseService>();

            services.AddTransient<IGetCoursesHandler, GetCoursesHandler>();
            services.AddTransient<IStoreCourseHandler, StoreCourseHandler>();

            services.AddDbContext<ReservationsDataContext>(options => options.UseSqlServer(config.ConnectionString));
            services.AddScoped<IReservationsDataContext, ReservationsDataContext>(provider =>
                provider.GetService<ReservationsDataContext>());

            services.AddTransient<ICourseRepository, CourseRepository>();

            return services.BuildServiceProvider();
        }
    }
}