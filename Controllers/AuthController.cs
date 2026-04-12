using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using PetWise_API.Contracts.Auth;
using PetWise_API.Models;

namespace PetWise_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly Supabase.Client _client;
        public AuthController(Supabase.Client client)
        {
            _client = client;
        }

        // Signing In
        [HttpPost("/Auth/Signup")]
        public async Task<IActionResult> SignUp(SignUpRequest request)
        {
            try
            {
                // 1️⃣ Create Supabase Auth user
                var session = await _client.Auth.SignUp(request.email, request.password);

                if (session.User == null)
                    return BadRequest(new { message = "Could not create auth user." });

                var supabaseUserId = session.User.Id;

                // 2️⃣ Insert into User table
                var user = new User
                {
                    first_name = request.first_name,
                    last_name = request.last_name,
                    email = request.email,
                    created_at = DateTime.UtcNow,
                    supabase_user_id = supabaseUserId
                };

                await _client.From<User>().Insert(user);

                return Ok(new AuthResponse
                {
                    user_id = user.user_id,
                    email = user.email,
                    access_token = session.AccessToken
                });
            }
            catch (Postgrest.Exceptions.PostgrestException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [HttpPost("/Auth/Signin")]
        public async Task<IActionResult> SignIn(SigninRequest request)
        {
            try
            {
                var session = await _client.Auth.SignIn(request.email, request.password);

                if (session.User == null)
                    return Unauthorized(new { message = "Invalid credentials." });

                // Optional: fetch User table info
                var user = (await _client.From<User>()
                                        .Filter("supabase_user_id", Postgrest.Constants.Operator.Equals, session.User.Id)
                                        .Get())
                                        .Models.FirstOrDefault();

                if (user == null)
                    return NotFound(new { message = "User profile not found." });

                return Ok(new AuthResponse
                {
                    user_id = user.user_id,
                    email = user.email,
                    access_token = session.AccessToken
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
