using PetWise_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Application.Common.Interfaces
{
    public interface IHealthEventService
    {
        Task<HealthEvent?> CreateHealthEventAsync(HealthEvent healthEvent, CancellationToken cancellationToken = default);
        Task<HealthEvent?> GetHealthEventByIdAsync(int eventId, CancellationToken cancellationToken = default);
        Task<IEnumerable<HealthEvent>> GetHealthEventsByPetIdAsync(int petId, CancellationToken cancellationToken = default);
        Task<bool> CompleteHealthEventAsync(int eventId, CancellationToken cancellationToken = default);
        Task<bool> DeleteHealthEventAsync(int eventId, CancellationToken cancellationToken = default);
    }
}
