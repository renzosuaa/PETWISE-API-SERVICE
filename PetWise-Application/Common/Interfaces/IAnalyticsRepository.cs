using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Application.Common.Interfaces
{
    public interface IAnalyticsRepository
    {
        Task<string?> GetPetActivityAndHealthStatsAsync(int petId, CancellationToken cancellationToken = default);
        Task<string?> GetUserAllPetsStatsAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
