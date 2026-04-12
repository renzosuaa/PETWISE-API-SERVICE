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
        private readonly Supabase.Client _client;
        public PetController(Supabase.Client client)
        {
            _client = client;
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

                var newPet = response.Models.First();

                return Ok(newPet.pet_id);

            }
            catch (Postgrest.Exceptions.PostgrestException ex) when (ex.Message.Contains("violates foreign key constraint"))
            {
                // Will be triggered if the user_id provided doesn't exist in the users table
                return Conflict(new { message = "User Id doesn't exist" });
            }

        }
        #endregion

        #region GET
        [HttpGet("/Pet/{pet_id}")]
        public async Task<IActionResult> GetPet(int pet_id)
        {
            var response = await _client.From<Pet>()
                                        .Where(p => p.pet_id == pet_id)
                                        .Get();

            var pet = response.Models.FirstOrDefault();
            if (pet == null)
                return NotFound();

            var dto = new PetResponse
            {
                pet_id = pet_id,
                name = pet.name,
                species = pet.species,
                birthday = pet.birthday,
                sex = pet.sex,
                created_at = pet.created_at,
                user_id = pet.user_id
            };

            return Ok(dto);
        }

        // GET /Pet?user_id=2
        [HttpGet("/Pet")]
        public async Task<IActionResult> GetPetsByUser([FromQuery] int user_id)
        {
            // 1️⃣ Use .Filter() to translate query param for PostgREST
            var response = await _client.From<Pet>()
                                        .Filter("user_id", Postgrest.Constants.Operator.Equals, user_id)
                                        .Get();

            var pets = response.Models;

            if (!pets.Any())
                return NotFound(new { message = "No pets found for this user." });

            // 2️⃣ Map to DTOs
            var petDtos = pets.Select(p => new PetResponse
            {
                pet_id = p.pet_id,
                name = p.name,
                species = p.species,
                birthday = p.birthday,
                sex = p.sex,
                created_at = p.created_at,
                user_id = p.user_id
            }).ToList();

            return Ok(petDtos);
        }

        #endregion
    }
}