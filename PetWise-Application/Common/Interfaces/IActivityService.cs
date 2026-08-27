using PetWise_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Application.Common.Interfaces
{
    public interface IActivityService
    {
        Task<Activity?> CreateActivityAsync(Activity activity, CancellationToken cancellationToken = default);
        Task<Activity?> GetActivityByIdAsync(int activityId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Activity>> GetActivitiesByPetIdAsync(int petId, CancellationToken cancellationToken = default);
        Task<Activity?> UpdateActivityAsync(Activity activity, CancellationToken cancellationToken = default);
        Task<bool> DeleteActivityAsync(int activityId, CancellationToken cancellationToken = default);
    }
}
