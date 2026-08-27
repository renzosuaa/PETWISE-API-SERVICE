using PetWise_Application.Common.Exceptions;
using PetWise_Application.Common.Interfaces;
using PetWise_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Infrastructure.Services
{
    public class HealthEventService : IHealthEventService
    {
        private readonly IHealthEventRepository _repository;

        public HealthEventService(IHealthEventRepository repository)
        {
            _repository = repository;
        }

        public async Task<HealthEvent?> CreateHealthEventAsync(HealthEvent healthEvent, CancellationToken cancellationToken = default)
        {
            if (healthEvent.PetId <= 0)
                throw new ValidationException("Pet Id doesn't exist");

            if (string.IsNullOrWhiteSpace(healthEvent.EventName))
                throw new ValidationException("Event name is required.");

            return await _repository.CreateAsync(healthEvent, cancellationToken);
        }

        public async Task<HealthEvent?> GetHealthEventByIdAsync(int eventId, CancellationToken cancellationToken = default)
        {
            if (eventId <= 0)
                throw new ValidationException("Event ID must be a positive integer.");

            var healthEvent = await _repository.GetByIdAsync(eventId, cancellationToken);
            if (healthEvent == null)
                throw new NotFoundException("Health Event not found.");

            return healthEvent;
        }

        public async Task<IEnumerable<HealthEvent>> GetHealthEventsByPetIdAsync(int petId, CancellationToken cancellationToken = default)
        {
            if (petId <= 0)
                throw new ValidationException("Pet ID must be a positive integer.");

            var events = await _repository.GetByPetIdAsync(petId, cancellationToken);
            if (!events.GetEnumerator().MoveNext())
                throw new NotFoundException("No Health Events found for this pet.");

            return events;
        }

        public async Task<bool> CompleteHealthEventAsync(int eventId, CancellationToken cancellationToken = default)
        {
            if (eventId <= 0)
                throw new ValidationException("Event ID must be a positive integer.");

            return await _repository.CompleteAsync(eventId, cancellationToken);
        }

        public async Task<bool> DeleteHealthEventAsync(int eventId, CancellationToken cancellationToken = default)
        {
            if (eventId <= 0)
                throw new ValidationException("Event ID must be a positive integer.");

            var success = await _repository.DeleteAsync(eventId, cancellationToken);
            if (!success)
                throw new NotFoundException("Health Event not found.");

            return success;
        }
    }
}
