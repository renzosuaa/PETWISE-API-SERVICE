using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PetWise_Infrastructure.Repositories;
using PetWise_Application.Common.Interfaces;
using PetWise_Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPetRepository, PetRepository>();
            services.AddScoped<IActivityRepository, ActivityRepository>();
            services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
            services.AddScoped<IHealthEventRepository, HealthEventRepository>();

            // Register Services
            services.AddScoped<IAuthService, SupabaseAuthService>();
            services.AddScoped<IPetService, PetService>();
            services.AddScoped<IActivityService, ActivityService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();
            services.AddScoped<IHealthEventService, HealthEventService>();

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IPetService, PetService>();
            services.AddScoped<IActivityService, ActivityService>();
            services.AddScoped<IHealthEventService, HealthEventService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();

            return services;
        }
    }
}
