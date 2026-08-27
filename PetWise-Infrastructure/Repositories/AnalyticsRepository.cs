using PetWise_Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Infrastructure.Repositories
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly Supabase.Client _client;

        public AnalyticsRepository(Supabase.Client client)
        {
            _client = client;
        }

        public async Task<string?> GetPetActivityAndHealthStatsAsync(int petId, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, object>
        {
            { "target_pet_id", petId }
        };

            var response = await _client.Rpc("get_pet_activity_and_health_stats", parameters);
            return response?.Content;
        }

        public async Task<string?> GetUserAllPetsStatsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, object>
        {
            { "target_user_id", userId.ToString() }
        };

            var response = await _client.Rpc("get_user_all_pets_stats", parameters);
            return response?.Content;
        }
    }
}
