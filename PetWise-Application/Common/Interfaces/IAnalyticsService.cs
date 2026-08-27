using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Application.Common.Interfaces
{
    public interface IAnalyticsService
    {
        Task<string> GetPetActivityAndHealthStatsAsync(int petId, CancellationToken cancellationToken = default);
        Task<string> GetUserDashboardAnalyticsAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
