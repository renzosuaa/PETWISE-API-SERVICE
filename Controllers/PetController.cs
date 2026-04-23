using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetWise_API.Contracts.Pet;
using PetWise_API.Contracts.User;
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
                var pet = new Pet
                {
                    name = request.name,
                    species = request.species,
                    birthday = request.birthday,
                    user_id = request.user_id,
                    sex = request.sex,
                    created_at = DateTime.UtcNow,
                };

                var response = await _client.From<Pet>().Insert(pet);

                var newPet = response.Models.FirstOrDefault();

                if (newPet == null)
                    return StatusCode(500, new { message = "Failed to create pet." });

                return Ok(new { pet_id = newPet.pet_id });
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

        #region GET
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
                    created_at = pet.created_at,
                    user_id = pet.user_id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving pet.", error = ex.Message });
            }
        }

       
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
                    user_id = p.user_id
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