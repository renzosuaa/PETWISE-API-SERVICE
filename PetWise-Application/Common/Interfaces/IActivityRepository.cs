using PetWise_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Application.Common.Interfaces
{
    public interface IActivityRepository
    {
        Task<Activity?> CreateAsync(Activity activity, CancellationToken cancellationToken = default);
        Task<Activity?> GetByIdAsync(int activityId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Activity>> GetByPetIdAsync(int petId, CancellationToken cancellationToken = default);
        Task<Activity?> UpdateAsync(Activity activity, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int activityId, CancellationToken cancellationToken = default);
    }
}
