using PetWise_Application.Common.Exceptions;
using PetWise_Application.Common.Interfaces;
using PetWise_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Infrastructure.Services
{
    public class PetService: IPetService
    {
        private readonly IPetRepository _petRepository;

        public PetService(IPetRepository petRepository)
        {
            _petRepository = petRepository;
        }

        public async Task<Pet?> CreatePetAsync(Pet pet, CancellationToken cancellationToken = default)
        {
            if (pet.Weight <= 0)
                throw new ValidationException("Weight must be greater than 0.");

            return await _petRepository.CreateAsync(pet, cancellationToken);
        
        }

        public async Task<Pet?> GetPetByIdAsync(int petId, CancellationToken cancellationToken = default)
        {
            if (petId <= 0)
                throw new ValidationException("Pet ID must be a positive integer.");

            var pet = await _petRepository.GetByIdAsync(petId, cancellationToken);
            if (pet == null)
                throw new NotFoundException($"No pet found with ID {petId}.");

            return pet;
        }

        public async Task<IEnumerable<Pet>> GetPetsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ValidationException("A valid user ID is required.");

            return await _petRepository.GetByUserIdAsync(userId, cancellationToken);
        }

        public async Task<Pet?> UpdatePetAsync(Pet pet, CancellationToken cancellationToken = default)
        {
            var existingPet = await _petRepository.GetByIdAsync(pet.Id, cancellationToken);
            if (existingPet == null)
                throw new NotFoundException($"No pet found with ID {pet.Id}.");

            return await _petRepository.UpdateAsync(pet, cancellationToken);
        }

        public async Task<bool> SoftDeletePetAsync(int petId, CancellationToken cancellationToken = default)
        {
            if (petId <= 0)
                throw new ValidationException("Pet ID must be a positive integer.");

            var success = await _petRepository.SoftDeleteAsync(petId, cancellationToken);
            if (!success)
                throw new NotFoundException($"No active pet found with ID {petId}.");

            return success;
        }
    }
}
