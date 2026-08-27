using PetWise.Infrastructure.Persistence.Models;
using PetWise_Application.Common.Exceptions;
using PetWise_Application.Common.Interfaces;
using PetWise_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using static Postgrest.Constants;

namespace PetWise_Infrastructure.Repositories
{
    public class HealthEventRepository : IHealthEventRepository
    {
        private readonly Supabase.Client _client;

        public HealthEventRepository(Supabase.Client client)
        {
            _client = client;
        }

        public async Task<HealthEvent?> CreateAsync(HealthEvent healthEvent, CancellationToken cancellationToken = default)
        {
            try
            {
                var model = MapToModel(healthEvent);
                var response = await _client.From<HealthEventModel>().Insert(model, cancellationToken: cancellationToken);
                var newModel = response.Models.FirstOrDefault();

                return newModel != null ? MapToDomain(newModel) : null;
            }
            catch (Postgrest.Exceptions.PostgrestException ex) when (ex.Message.Contains("violates foreign key constraint"))
            {
                throw new ConflictException("Pet Id doesn't exist");
            }
        }

        public async Task<HealthEvent?> GetByIdAsync(int eventId, CancellationToken cancellationToken = default)
        {
            var response = await _client.From<HealthEventModel>()
                .Where(h => h.event_id == eventId)
                .Get(cancellationToken);

            var model = response.Models.FirstOrDefault();
            return model != null ? MapToDomain(model) : null;
        }

        public async Task<IEnumerable<HealthEvent>> GetByPetIdAsync(int petId, CancellationToken cancellationToken = default)
        {
            var response = await _client.From<HealthEventModel>()
                .Filter("pet_id", Operator.Equals, petId)
                .Get(cancellationToken);

            return response.Models.Select(MapToDomain);
        }

        public async Task<bool> CompleteAsync(int eventId, CancellationToken cancellationToken = default)
        {
            await _client.From<HealthEventModel>()
                .Where(h => h.event_id == eventId)
                .Set(h => h.is_completed, true)
                .Update(cancellationToken: cancellationToken);

            return true;
        }

        public async Task<bool> DeleteAsync(int eventId, CancellationToken cancellationToken = default)
        {
            var response = await _client.From<HealthEventModel>()
                .Where(h => h.event_id == eventId)
                .Get(cancellationToken);

            if (!response.Models.Any()) return false;

            await _client.From<HealthEventModel>()
                .Where(h => h.event_id == eventId)
                .Delete(cancellationToken: cancellationToken);

            return true;
        }

        private static HealthEvent MapToDomain(HealthEventModel model)
        {
            return new HealthEvent
            {
                Id = model.event_id,
                PetId = model.pet_id,
                Type = model.type,
                EventName = model.event_name,
                EventDate = model.event_date,
                IsCompleted = model.is_completed,
                CreatedAt = model.created_at
            };
        }

        private static HealthEventModel MapToModel(HealthEvent healthEvent)
        {
            return new HealthEventModel
            {
                event_id = healthEvent.Id,
                pet_id = healthEvent.PetId,
                type = healthEvent.Type,
                event_name = healthEvent.EventName,
                event_date = healthEvent.EventDate,
                is_completed = healthEvent.IsCompleted,
                created_at = healthEvent.CreatedAt
            };
        }
    }
}
