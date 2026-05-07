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
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePet([FromBody] CreatePetRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.name))
                return BadRequest(new { message = "Pet name is required." });

            if (string.IsNullOrWhiteSpace(request.species))
                return BadRequest(new { message = "Species is required." });

            if (string.IsNullOrWhiteSpace(request.breed))
                return BadRequest(new { message = "Breed is required." });

            if (request.weight <= 0)
                return UnprocessableEntity(new { message = "Weight must be greater than 0." });

            if (request.user_id == Guid.Empty)
                return BadRequest(new { message = "A valid user ID is required." });

            try
            {
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
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create pet." });

                return StatusCode(StatusCodes.Status201Created, new { newPet.pet_id });
            }
            catch (Postgrest.Exceptions.PostgrestException ex) when (ex.Message.Contains("violates foreign key constraint"))
            {
                return Conflict(new { message = "The provided user ID does not exist." });
            }
            catch (Postgrest.Exceptions.PostgrestException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Database error.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unexpected error.", error = ex.Message });
            }
        }
        #endregion

        #region GET SINGLE PET
        [HttpGet("/Pet/{pet_id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPet(int pet_id)
        {
            if (pet_id <= 0)
                return BadRequest(new { message = "Pet ID must be a positive integer." });

            try
            {
                var response = await _client.From<Pet>()
                                            .Where(p => p.pet_id == pet_id)
                                            .Get();

                var pet = response.Models.FirstOrDefault();

                if (pet == null)
                    return NotFound(new { message = $"No pet found with ID {pet_id}." });

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
            catch (Postgrest.Exceptions.PostgrestException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Database error.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving pet.", error = ex.Message });
            }
        }
        #endregion

        #region GET BY USER
        [HttpGet("/Pet")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPetsByUser([FromQuery] Guid user_id)
        {
            if (user_id == Guid.Empty)
                return BadRequest(new { message = "A valid user ID is required." });

            try
            {
                var response = await _client.From<Pet>()
                    .Filter("user_id", Postgrest.Constants.Operator.Equals, user_id.ToString())
                    .Filter("is_deleted", Postgrest.Constants.Operator.Equals, "false") 
                    .Get();

                var pets = response.Models;

                if (pets == null || !pets.Any())
                    return NoContent();

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
            catch (Postgrest.Exceptions.PostgrestException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Database error.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving pets.", error = ex.Message });
            }
        }
        #endregion

        #region UPDATE
        [HttpPatch("/Pet/{pet_id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PatchPet(int pet_id, [FromBody] UpdatePetRequest request)
        {
            if (pet_id <= 0)
                return BadRequest(new { message = "Pet ID must be a positive integer." });

            // Reject empty PATCH body — nothing to update
            if (request == null)
                return BadRequest(new { message = "Request body is required." });

            if (request.weight.HasValue && request.weight <= 0)
                return UnprocessableEntity(new { message = "Weight must be greater than 0." });

            if (!string.IsNullOrEmpty(request.sex) &&
                request.sex != "Male" && request.sex != "Female")
                return UnprocessableEntity(new { message = "Sex must be 'Male' or 'Female'." });

            try
            {
                var existingResponse = await _client.From<Pet>()
                                                    .Where(p => p.pet_id == pet_id)
                                                    .Get();

                var existing = existingResponse.Models.FirstOrDefault();

                if (existing == null)
                    return NotFound(new { message = $"No pet found with ID {pet_id}." });

                if (!string.IsNullOrEmpty(request.name))
                    existing.name = request.name;

                if (!string.IsNullOrEmpty(request.species))
                    existing.species = request.species;

                if (request.birthday.HasValue)
                    existing.birthday = request.birthday.Value;

                if (!string.IsNullOrEmpty(request.breed))
                    existing.breed = request.breed;

                if (request.weight.HasValue)
                    existing.weight = request.weight.Value;

                if (!string.IsNullOrEmpty(request.sex))
                    existing.sex = request.sex;

                var response = await _client.From<Pet>()
                                            .Where(p => p.pet_id == pet_id)
                                            .Update(existing);

                var updated = response.Models.FirstOrDefault();

                if (updated == null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Update failed." });

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
            catch (Postgrest.Exceptions.PostgrestException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Database error.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error updating pet.", error = ex.Message });
            }
        }
        #endregion

        #region DELETE
        [HttpDelete("/Pet/{pet_id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SoftDeletePet(int pet_id)
        {
            if (pet_id <= 0)
                return BadRequest(new { message = "Pet ID must be a positive integer." });

            try
            {
                var existingResponse = await _client.From<Pet>()
                                                    .Where(p => p.pet_id == pet_id)
                                                    .Get();

                var existing = existingResponse.Models.FirstOrDefault();

               
                if (existing == null || existing.is_deleted == true)
                    return NotFound(new { message = $"No active pet found with ID {pet_id}." });

               
                existing.is_deleted = true;

                await _client.From<Pet>()
                             .Where(p => p.pet_id == pet_id)
                             .Set(p => p.is_deleted, true)
                             .Update();

                return Ok(new { message = $"Pet with ID {pet_id} has been deactivated (soft-deleted)." });
            }
            catch (Postgrest.Exceptions.PostgrestException ex)
            {
                
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Database error.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unexpected error.", error = ex.Message });
            }
        }
        #endregion
    }
}