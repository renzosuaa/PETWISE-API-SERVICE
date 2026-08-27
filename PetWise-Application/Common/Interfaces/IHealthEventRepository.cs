using PetWise_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Application.Common.Interfaces
{
    public interface IHealthEventRepository
    {
        Task<HealthEvent?> CreateAsync(HealthEvent healthEvent, CancellationToken cancellationToken = default);
        Task<HealthEvent?> GetByIdAsync(int eventId, CancellationToken cancellationToken = default);
        Task<IEnumerable<HealthEvent>> GetByPetIdAsync(int petId, CancellationToken cancellationToken = default);
        Task<bool> CompleteAsync(int eventId, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int eventId, CancellationToken cancellationToken = default);
    }
}
