using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetWise_API.Contracts.Pet;
using PetWise_API.Models;

namespace PetWise_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PetController : ControllerBase
    {
        private readonly string _anonKey;
        private readonly Supabase.Client _client;

        public PetController(Supabase.Client client, IConfiguration configuration)
        {
            _client = client;
            _anonKey = configuration["Supabase:AnonKey"]!;
        }

        #region POST
        [HttpPost("/Pet/")]
        public async Task<IActionResult> CreateUser(CreatePetRequest request)
        {
            try
            {
                // VALIDATION (NEW - REQUIRED because breed/weight are strict now)
                if (string.IsNullOrWhiteSpace(request.breed))
                    return BadRequest(new { message = "Breed is required." });

                if (request.weight <= 0)
                    return BadRequest(new { message = "Weight must be greater than 0." });

                var pet = new Pet
                {
                    name = request.name,
                    species = request.species,
                    birthday = request.birthday,
                    user_id = request.user_id,
                    sex = request.sex,
                    breed = request.breed,
                    weight = request.weight,
                    created_at = DateTime.UtcNow,
                };

                var response = await _client.From<Pet>().Insert(pet);

                var newPet = response.Models.FirstOrDefault();

                if (newPet == null)
                    return StatusCode(500, new { message = "Failed to create pet." });

                return Ok(new { newPet.pet_id });
            }
            catch (Postgrest.Exceptions.PostgrestException ex) when (ex.Message.Contains("violates foreign key constraint"))
            {
                return Conflict(new { message = "User ID doesn't exist." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error.", error = ex.Message });
            }
        }
        #endregion

        #region GET SINGLE PET
        [HttpGet("/Pet/{pet_id}")]
        public async Task<IActionResult> GetPet(int pet_id)
        {
            try
            {
                var response = await _client.From<Pet>()
                                            .Where(p => p.pet_id == pet_id)
                                            .Get();

                var pet = response.Models.FirstOrDefault();

                if (pet == null)
                    return NotFound(new { message = "Pet not found." });

                return Ok(new PetResponse
                {
                    pet_id = pet.pet_id,
                    name = pet.name,
                    species = pet.species,
                    birthday = pet.birthday,
                    sex = pet.sex,
                    breed = pet.breed ?? "",
                    weight = pet.weight,
                    created_at = pet.created_at,
                    user_id = pet.user_id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving pet.", error = ex.Message });
            }
        }
        #endregion

        #region GET BY USER
        [HttpGet("/Pet")]
        public async Task<IActionResult> GetPetsByUser([FromQuery] Guid user_id)
        {
            try
            {
                var response = await _client.From<Pet>()
                    .Filter("user_id", Postgrest.Constants.Operator.Equals, user_id.ToString())
                    .Get();

                var pets = response.Models;

                if (pets == null || !pets.Any())
                    return NotFound(new { message = "No pets found for this user." });

                var result = pets.Select(p => new PetResponse
                {
                    pet_id = p.pet_id,
                    name = p.name,
                    species = p.species,
                    birthday = p.birthday,
                    sex = p.sex,
                    created_at = p.created_at,
                    user_id = p.user_id,
                    breed = p.breed ?? "",
                    weight = p.weight
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving pets.", error = ex.Message });
            }
        }
        #endregion

        #region UPDATE
        [HttpPatch("/Pet/{pet_id}")]
        public async Task<IActionResult> PatchPet(int pet_id, [FromBody] UpdatePetRequest request)
        {
            try
            {
                var existingResponse = await _client.From<Pet>()
                                                    .Where(p => p.pet_id == pet_id)
                                                    .Get();

                var existing = existingResponse.Models.FirstOrDefault();

                if (existing == null)
                    return NotFound(new { message = "Pet not found." });

                if (!string.IsNullOrEmpty(request.name))
                    existing.name = request.name;

                if (!string.IsNullOrEmpty(request.species))
                    existing.species = request.species;

                if (request.birthday.HasValue)
                    existing.birthday = request.birthday.Value;

                if (!string.IsNullOrEmpty(request.breed))
                    existing.breed = request.breed;

                if (request.weight.HasValue)
                {
                    if (request.weight <= 0)
                        return BadRequest(new { message = "Invalid weight." });

                    existing.weight = request.weight.Value;
                }

                if (!string.IsNullOrEmpty(request.sex))
                    existing.sex = request.sex;

                var response = await _client.From<Pet>()
                                            .Where(p => p.pet_id == pet_id)
                                            .Update(existing);

                var updated = response.Models.FirstOrDefault();

                if (updated == null)
                    return StatusCode(500, new { message = "Update failed." });

                return Ok(new PetResponse
                {
                    pet_id = updated.pet_id,
                    name = updated.name,
                    species = updated.species,
                    birthday = updated.birthday,
                    sex = updated.sex,
                    breed = updated.breed ?? "",
                    weight = updated.weight,
                    created_at = updated.created_at,
                    user_id = updated.user_id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating pet.", error = ex.Message });
            }
        }
        #endregion
    }
}