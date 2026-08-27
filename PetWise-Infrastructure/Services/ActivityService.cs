using PetWise_Application.Common.Exceptions;
using PetWise_Application.Common.Interfaces;
using PetWise_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Infrastructure.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IActivityRepository _activityRepository;

        public ActivityService(IActivityRepository activityRepository)
        {
            _activityRepository = activityRepository;
        }

        public async Task<Activity?> CreateActivityAsync(Activity activity, CancellationToken cancellationToken = default)
        {
            if (activity.PetId <= 0)
                throw new ValidationException("A valid Pet ID is required.");

            if (string.IsNullOrWhiteSpace(activity.Title))
                throw new ValidationException("Activity title is required.");

            return await _activityRepository.CreateAsync(activity, cancellationToken);
        }

        public async Task<Activity?> GetActivityByIdAsync(int activityId, CancellationToken cancellationToken = default)
        {
            if (activityId <= 0)
                throw new ValidationException("Activity ID must be a positive integer.");

            var activity = await _activityRepository.GetByIdAsync(activityId, cancellationToken);
            if (activity == null)
                throw new NotFoundException($"No activity found with ID {activityId}.");

            return activity;
        }

        public async Task<IEnumerable<Activity>> GetActivitiesByPetIdAsync(int petId, CancellationToken cancellationToken = default)
        {
            if (petId <= 0)
                throw new ValidationException("Pet ID must be a positive integer.");

            return await _activityRepository.GetByPetIdAsync(petId, cancellationToken);
        }

        public async Task<Activity?> UpdateActivityAsync(Activity activity, CancellationToken cancellationToken = default)
        {
            var existing = await _activityRepository.GetByIdAsync(activity.Id, cancellationToken);
            if (existing == null)
                throw new NotFoundException($"No activity found with ID {activity.Id}.");

            return await _activityRepository.UpdateAsync(activity, cancellationToken);
        }

        public async Task<bool> DeleteActivityAsync(int activityId, CancellationToken cancellationToken = default)
        {
            if (activityId <= 0)
                throw new ValidationException("Activity ID must be a positive integer.");

            var success = await _activityRepository.DeleteAsync(activityId, cancellationToken);
            if (!success)
                throw new NotFoundException($"Activity with ID {activityId} not found.");

            return success;
        }
    }
}
