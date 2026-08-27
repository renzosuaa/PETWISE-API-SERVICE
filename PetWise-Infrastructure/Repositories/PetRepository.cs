using PetWise_Application.Common.Interfaces;
using PetWise_Application.Common.Exceptions;
using PetWise_Domain.Entities;
using PetWise_Infrastructure.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static Postgrest.Constants;

namespace PetWise_Infrastructure.Repositories
{
    public class PetRepository : IPetRepository
    {
        private readonly Supabase.Client _client;

        public PetRepository(Supabase.Client client)
        {
            _client = client;
        }

        public async Task<Pet?> CreateAsync(Pet pet, CancellationToken cancellationToken = default)
        {
            try
            {
                var model = MapToModel(pet);
                var response = await _client.From<PetModel>().Insert(model, cancellationToken: cancellationToken);
                var newModel = response.Models.FirstOrDefault();

                return newModel != null ? MapToDomain(newModel) : null;
            }
            catch (Postgrest.Exceptions.PostgrestException ex) when (ex.Message.Contains("violates foreign key constraint"))
            {
                throw new ConflictException("The provided user ID does not exist.");
            }
        }

        public async Task<Pet?> GetByIdAsync(int petId, CancellationToken cancellationToken = default)
        {
            var response = await _client.From<PetModel>()
                .Where(p => p.pet_id == petId)
                .Get(cancellationToken);

            var model = response.Models.FirstOrDefault();
            return model != null ? MapToDomain(model) : null;
        }

        public async Task<IEnumerable<Pet>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var response = await _client.From<PetModel>()
                .Filter("user_id", Operator.Equals, userId.ToString())
                .Filter("is_deleted", Operator.Equals, "false")
                .Get(cancellationToken);

            return response.Models.Select(MapToDomain);
        }

        public async Task<Pet?> UpdateAsync(Pet pet, CancellationToken cancellationToken = default)
        {
            var model = MapToModel(pet);
            var response = await _client.From<PetModel>()
                .Where(p => p.pet_id == pet.Id)
                .Update(model, cancellationToken: cancellationToken);

            var updatedModel = response.Models.FirstOrDefault();
            return updatedModel != null ? MapToDomain(updatedModel) : null;
        }

        public async Task<bool> SoftDeleteAsync(int petId, CancellationToken cancellationToken = default)
        {
            var response = await _client.From<PetModel>()
                .Where(p => p.pet_id == petId)
                .Get(cancellationToken);

            var existing = response.Models.FirstOrDefault();
            if (existing == null || existing.is_deleted) return false;

            await _client.From<PetModel>()
                .Where(p => p.pet_id == petId)
                .Set(p => p.is_deleted, true)
                .Update(cancellationToken: cancellationToken);

            return true;
        }

        private static Pet MapToDomain(PetModel model)
        {
            return new Pet
            {
                Id = model.pet_id,
                Name = model.name,
                Species = model.species,
                Breed = model.breed ?? string.Empty,
                ImageUrl = model.image_url ?? string.Empty,
                Weight = model.weight,
                Sex = model.sex,
                Birthday = model.birthday,
                CreatedAt = model.created_at,
                UserId = model.user_id,
                IsDeleted = model.is_deleted
            };
        }

        private static PetModel MapToModel(Pet pet)
        {
            return new PetModel
            {
                pet_id = pet.Id,
                name = pet.Name,
                species = pet.Species,
                breed = pet.Breed,
                image_url = pet.ImageUrl,
                weight = pet.Weight,
                sex = pet.Sex,
                birthday = pet.Birthday,
                created_at = pet.CreatedAt,
                user_id = pet.UserId,
                is_deleted = pet.IsDeleted
            };
        }
    }
    
}
