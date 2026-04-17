using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetWise_API.Contracts.User;
using PetWise_API.Models;
using User = PetWise_API.Models.User;

namespace PetWise_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly Supabase.Client _client;
        private readonly string _anonKey;

        public UserController(Supabase.Client client, IConfiguration configuration)
        {
            _client = client;
            _anonKey = configuration["Supabase:AnonKey"]!;
        }

        #region GET
        [HttpGet("/User/{user_id}")]
        public async Task<IActionResult> GetUser(Guid user_id)
        {
            var token = Request.Headers["Authorization"]
                               .ToString()
                               .Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
                return Unauthorized("No token provided.");

            _client.Postgrest.GetHeaders = () => new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {token}" },
                    { "apikey", _anonKey }
                };

            var response = await _client.From<User>()
                                        .Where(u => u.user_id == user_id)
                                        .Get();

            var user = response.Models.FirstOrDefault();
            if (user == null) return NotFound();

            return Ok(new UserResponse
            {
                user_id = user.user_id,
                first_name = user.first_name,
                last_name = user.last_name,
                email = user.email,
                nickname = user.nickname,
                created_at = user.created_at
            });
        }
        #endregion

        #region UPDATE

        [HttpPatch("/User/{user_id}")]
        public async Task<IActionResult> PatchUser(Guid user_id, [FromBody] UpdateUserRequest request)
        {
            var token = Request.Headers["Authorization"]
                               .ToString()
                               .Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
                return Unauthorized("No token provided.");

            _client.Postgrest.GetHeaders = () => new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {token}" },
                    { "apikey", _anonKey }
                };
           
            var existingResponse = await _client.From<User>()
                                                .Where(u => u.user_id == user_id)
                                                .Single();

            var existing = existingResponse;

            if (existing == null)
                return NotFound();

            
            if (request.first_name != null)
                existing.first_name = request.first_name;

            if (request.last_name != null)
                existing.last_name = request.last_name;
            if (request.nickname != null)
                existing.nickname = request.nickname;



            var response = await _client.From<User>()
                                        .Where(u => u.user_id == user_id)
                                        .Update(existing);

            var updatedUser = response.Models.FirstOrDefault();

            if (updatedUser == null)
                return NotFound();

            return Ok(new UserResponse
            {
                user_id = updatedUser.user_id,
                first_name = updatedUser.first_name,
                last_name = updatedUser.last_name,
                nickname = updatedUser.nickname,
                email = updatedUser.email,
                created_at = updatedUser.created_at
            });
        }

        #endregion


    }
}