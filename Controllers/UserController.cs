using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetWise_API.Contracts.User;
using PetWise_API.Models;
using User = PetWise_API.Models.User;

namespace PetWise_API.Controllers
{
    [Authorize]
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

        private string? ExtractToken()
        {
            var token = Request.Headers["Authorization"]
                               .ToString()
                               .Replace("Bearer ", "")
                               .Trim();

            return string.IsNullOrEmpty(token) ? null : token;
        }

        private void SetAuthHeaders(string token)
        {
            _client.Postgrest.GetHeaders = () => new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {token}" },
                { "apikey", _anonKey }
            };
        }

        #region GET
        [HttpGet("/User/{user_id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUser(Guid user_id)
        {
            if (user_id == Guid.Empty)
                return BadRequest(new { message = "A valid user ID is required." });

            var token = ExtractToken();
            if (token == null)
                return Unauthorized(new { message = "No token provided." });

            try
            {
                SetAuthHeaders(token);

                var response = await _client.From<User>()
                                            .Where(u => u.user_id == user_id)
                                            .Get();

                var user = response.Models.FirstOrDefault();

                if (user == null)
                    return NotFound(new { message = $"No user found with ID {user_id}." });

                return Ok(new UserResponse
                {
                    user_id = user.user_id,
                    first_name = user.first_name,
                    last_name = user.last_name,
                    email = user.email,
                    image_url = user.image_url,
                    nickname = user.nickname,
                    created_at = user.created_at,
                    has_completed_setup = user.has_completed_setup
                });
            }
            catch (Postgrest.Exceptions.PostgrestException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Database error.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching user.", error = ex.Message });
            }
        }
        #endregion

        #region UPDATE
        [HttpPatch("/User/{user_id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PatchUser(Guid user_id, [FromBody] UpdateUserRequest request)
        {
            if (user_id == Guid.Empty)
                return BadRequest(new { message = "A valid user ID is required." });

            if (request == null)
                return BadRequest(new { message = "Request body is required." });

            // Reject if all fields are null — nothing to update
            if (string.IsNullOrWhiteSpace(request.first_name) &&
                string.IsNullOrWhiteSpace(request.last_name) &&
                string.IsNullOrWhiteSpace(request.nickname) &&
                string.IsNullOrWhiteSpace(request.image_url) &&
                request.has_completed_setup == null)
                return UnprocessableEntity(new { message = "At least one field must be provided to update." });

            var token = ExtractToken();
            if (token == null)
                return Unauthorized(new { message = "No token provided." });

            try
            {
                SetAuthHeaders(token);

                var existingResponse = await _client.From<User>()
                                                    .Where(u => u.user_id == user_id)
                                                    .Get();

                var existing = existingResponse.Models.FirstOrDefault();

                if (existing == null)
                    return NotFound(new { message = $"No user found with ID {user_id}." });

                if (!string.IsNullOrEmpty(request.first_name))
                    existing.first_name = request.first_name;

                if (!string.IsNullOrEmpty(request.last_name))
                    existing.last_name = request.last_name;

                if (!string.IsNullOrEmpty(request.image_url))
                    existing.image_url = request.image_url;

                if (!string.IsNullOrEmpty(request.nickname))
                    existing.nickname = request.nickname;

                if (request.has_completed_setup.HasValue)
                    existing.has_completed_setup = request.has_completed_setup.Value;

                var response = await _client.From<User>()
                                            .Where(u => u.user_id == user_id)
                                            .Update(existing);

                var updatedUser = response.Models.FirstOrDefault();

                if (updatedUser == null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Update failed." });

                return Ok(new UserResponse
                {
                    user_id = updatedUser.user_id,
                    first_name = updatedUser.first_name,
                    last_name = updatedUser.last_name,
                    nickname = updatedUser.nickname,
                    email = updatedUser.email,
                    image_url = updatedUser.image_url,
                    created_at = updatedUser.created_at,
                    has_completed_setup = updatedUser.has_completed_setup
                });
            }
            catch (Postgrest.Exceptions.PostgrestException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Database error.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating user.", error = ex.Message });
            }
        }
        #endregion
    }
}