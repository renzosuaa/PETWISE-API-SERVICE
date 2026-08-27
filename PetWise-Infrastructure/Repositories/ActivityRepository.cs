using PetWise_Application.Common.Exceptions;
using PetWise_Application.Common.Interfaces;
using PetWise_Domain.Entities;
using PetWise_Infrastructure.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Infrastructure.Repositories
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly Supabase.Client _client;

        public ActivityRepository(Supabase.Client client)
        {
            _client = client;
        }

        public async Task<Activity?> CreateAsync(Activity activity, CancellationToken cancellationToken = default)
        {
            try
            {
                var model = MapToModel(activity);
                var response = await _client.From<ActivityModel>().Insert(model, cancellationToken: cancellationToken);
                var newModel = response.Models.FirstOrDefault();

                return newModel != null ? MapToDomain(newModel) : null;
            }
            catch (Postgrest.Exceptions.PostgrestException ex) when (ex.Message.Contains("violates foreign key constraint"))
            {
                throw new ConflictException("The provided Pet ID does not exist.");
            }
        }

        public async Task<Activity?> GetByIdAsync(int activityId, CancellationToken cancellationToken = default)
        {
            var response = await _client.From<ActivityModel>()
                .Where(a => a.activity_id == activityId)
                .Get(cancellationToken);

            var model = response.Models.FirstOrDefault();
            return model != null ? MapToDomain(model) : null;
        }

        public async Task<IEnumerable<Activity>> GetByPetIdAsync(int petId, CancellationToken cancellationToken = default)
        {
            var response = await _client.From<ActivityModel>()
                .Where(a => a.pet_id == petId)
                .Get(cancellationToken);

            return response.Models.Select(MapToDomain);
        }

        public async Task<Activity?> UpdateAsync(Activity activity, CancellationToken cancellationToken = default)
        {
            var model = MapToModel(activity);
            var response = await _client.From<ActivityModel>()
                .Where(a => a.activity_id == activity.Id)
                .Update(model, cancellationToken: cancellationToken);

            var updatedModel = response.Models.FirstOrDefault();
            return updatedModel != null ? MapToDomain(updatedModel) : null;
        }

        public async Task<bool> DeleteAsync(int activityId, CancellationToken cancellationToken = default)
        {
            var response = await _client.From<ActivityModel>()
                .Where(a => a.activity_id == activityId)
                .Get(cancellationToken);

            if (!response.Models.Any()) return false;

            await _client.From<ActivityModel>()
                .Where(a => a.activity_id == activityId)
                .Delete(cancellationToken: cancellationToken);

            return true;
        }

        private static Activity MapToDomain(ActivityModel model)
        {
            return new Activity
            {
                Id = model.activity_id,
                PetId = model.pet_id,
                Title = model.title,
                Description = model.description,
                TimeScheduled = model.time_scheduled,
                Recurrence = model.recurrence,
                IsActive = model.is_active,
                CreatedAt = model.created_at
            };
        }

        private static ActivityModel MapToModel(Activity activity)
        {
            return new ActivityModel
            {
                activity_id = activity.Id,
                pet_id = activity.PetId,
                title = activity.Title,
                description = activity.Description,
                time_scheduled = activity.TimeScheduled,
                recurrence = activity.Recurrence,
                is_active = activity.IsActive,
                created_at = activity.CreatedAt
            };
        }
    }
}
