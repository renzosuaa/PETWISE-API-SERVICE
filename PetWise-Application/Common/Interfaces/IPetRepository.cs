using PetWise_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Application.Common.Interfaces
{
    public interface IPetRepository
    {
        Task<Pet?> CreateAsync(Pet pet, CancellationToken cancellationToken = default);
        Task<Pet?> GetByIdAsync(int petId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Pet>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Pet?> UpdateAsync(Pet pet, CancellationToken cancellationToken = default);
        Task<bool> SoftDeleteAsync(int petId, CancellationToken cancellationToken = default);
    }
}
