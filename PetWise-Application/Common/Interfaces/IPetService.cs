using PetWise_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Application.Common.Interfaces
{
    public interface IPetService
    {
        Task<Pet?> CreatePetAsync(Pet pet, CancellationToken cancellationToken = default);
        Task<Pet?> GetPetByIdAsync(int petId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Pet>> GetPetsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Pet?> UpdatePetAsync(Pet pet, CancellationToken cancellationToken = default);
        Task<bool> SoftDeletePetAsync(int petId, CancellationToken cancellationToken = default);
    }
}
