using PetWise_Application.Common.Exceptions;
using PetWise_Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Infrastructure.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsRepository _analyticsRepository;

        public AnalyticsService(IAnalyticsRepository analyticsRepository)
        {
            _analyticsRepository = analyticsRepository;
        }

        public async Task<string> GetPetActivityAndHealthStatsAsync(int petId, CancellationToken cancellationToken = default)
        {
            if (petId <= 0)
                throw new ValidationException("Pet ID must be a positive integer.");

            var jsonResponse = await _analyticsRepository.GetPetActivityAndHealthStatsAsync(petId, cancellationToken);

            if (string.IsNullOrWhiteSpace(jsonResponse))
                throw new ApplicationException("Empty response received from the analytics engine.");

            return jsonResponse;
        }

        public async Task<string> GetUserDashboardAnalyticsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ValidationException("A valid user ID is required.");

            var jsonResponse = await _analyticsRepository.GetUserAllPetsStatsAsync(userId, cancellationToken);

            if (string.IsNullOrWhiteSpace(jsonResponse))
                throw new ApplicationException("Empty response received from the database engine.");

            return jsonResponse;
        }
    }
}
