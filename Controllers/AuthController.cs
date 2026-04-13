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
                var session = await _client.Auth.SignUp(request.email, request.password);

                if (session.User == null)
                    return BadRequest(new { message = "Could not create auth user." });

                var supabaseUserId = session.User.Id;

                // Only set session if tokens are present
                if (!string.IsNullOrEmpty(session.AccessToken) && !string.IsNullOrEmpty(session.RefreshToken))
                {
                    await _client.Auth.SetSession(session.AccessToken, session.RefreshToken);

                    var user = new User
                    {
                        user_id = supabaseUserId,
                        first_name = request.first_name,
                        last_name = request.last_name,
                        email = request.email,
                        created_at = DateTime.UtcNow,
                    };

                    await _client.From<User>().Insert(user);

                    return Ok(new AuthResponse
                    {
                        user_id = user.user_id,
                        email = user.email,
                        access_token = session.AccessToken
                    });
                }

                // No tokens: likely confirmable signup — do not attempt authenticated DB writes
                return Ok(new
                {
                    user_id = supabaseUserId,
                    email = request.email,
                    message = "Sign-up created. Please confirm your email before signing in."
                });
            }
            catch (Postgrest.Exceptions.PostgrestException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        // Diagnostic SignIn — run once to capture session details and DB error info
        [HttpPost("/Auth/Signin")]
        public async Task<IActionResult> SignIn(SigninRequest request)
        {
            try
            {
                var session = await _client.Auth.SignIn(request.email, request.password);

                if (session.User == null)
                    return Unauthorized(new { message = "Invalid credentials." });

                // Log session info for debugging
                Console.WriteLine($"[DEBUG] SignIn: UserId={session.User.Id}");
                Console.WriteLine($"[DEBUG] AccessToken present={!string.IsNullOrEmpty(session.AccessToken)} Length={(session.AccessToken ?? "").Length}");
                Console.WriteLine($"[DEBUG] RefreshToken present={!string.IsNullOrEmpty(session.RefreshToken)} Length={(session.RefreshToken ?? "").Length}");

                if (!string.IsNullOrEmpty(session.AccessToken) && !string.IsNullOrEmpty(session.RefreshToken))
                {
                    await _client.Auth.SetSession(session.AccessToken, session.RefreshToken);
                    Console.WriteLine("[DEBUG] SetSession completed.");
                }
                else
                {
                    Console.WriteLine("[DEBUG] No tokens returned from SignIn.");
                    return StatusCode(500, new { message = "Sign-in succeeded but no session tokens issued." });
                }

                // Test an authenticated read (small query) and capture any Postgrest error details
                try
                {
                    var user = (await _client.From<User>()
                                            .Filter("user_id", Postgrest.Constants.Operator.Equals, session.User.Id)
                                            .Get())
                                            .Models.FirstOrDefault();

                    if (user == null)
                    {
                        Console.WriteLine("[DEBUG] Authenticated query returned no profile row.");
                        return NotFound(new { message = "User profile not found." });
                    }

                    return Ok(new AuthResponse
                    {
                        user_id = user.user_id,
                        email = user.email,
                        access_token = session.AccessToken
                    });
                }
                catch (Postgrest.Exceptions.PostgrestException pex)
                {
                    // Log Postgrest error details
                    Console.WriteLine($"[DEBUG] PostgrestException: Status={pex.StatusCode} Message={pex.Message}");
                    return StatusCode(pex.StatusCode == 0 ? 500 : (int)pex.StatusCode, new { message = pex.Message });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] SignIn exception: {ex}");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
