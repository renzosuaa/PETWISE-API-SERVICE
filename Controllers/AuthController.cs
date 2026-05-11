using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetWise_API.Contracts.Auth;
using PetWise_API.Models;
using Postgrest;

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

        #region POST

        [HttpPost("/Auth/Signup")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.email) || string.IsNullOrWhiteSpace(request.password))
                return BadRequest(new { message = "Email and password are required." });

            try
            {
                var session = await _client.Auth.SignUp(request.email, request.password);

                if (session?.User == null)
                    return BadRequest(new { message = "Failed to create user." });

                
                return StatusCode(StatusCodes.Status201Created, new
                {
                    user_id = session.User.Id,
                    email = session.User.Email,
                    message = "Signup successful. Please verify your email."
                });
            }
            catch (Supabase.Gotrue.Exceptions.GotrueException ex) when (ex.Message.Contains("already registered"))
            {
                return Conflict(new { message = "An account with this email already exists." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error.", error = ex.Message });
            }
        }


        [HttpPost("/Auth/Signin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SignIn([FromBody] SigninRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.email) || string.IsNullOrWhiteSpace(request.password))
                return BadRequest(new { message = "Email and password are required." });

            try
            {
                var session = await _client.Auth.SignIn(request.email, request.password);

                if (session?.User == null || string.IsNullOrEmpty(session.AccessToken))
                    return Unauthorized(new { message = "Invalid email or password." });

                if (!Guid.TryParse(session.User.Id, out var userId))
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Invalid user ID format returned from auth provider." });

                await _client.Auth.SetSession(session.AccessToken, session.RefreshToken);

                var response = await _client.From<User>()
                    .Filter("user_id", Postgrest.Constants.Operator.Equals, userId.ToString())
                    .Get();

                var user = response.Models.FirstOrDefault();

                if (user == null)
                    return NotFound(new { message = "User profile not found." });

                return Ok(new AuthResponse
                {
                    user_id = user.user_id,
                    email = user.email,
                    access_token = session.AccessToken
                });
            }
            catch (Postgrest.Exceptions.PostgrestException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Database error.", error = ex.Message });
            }
            catch (Supabase.Gotrue.Exceptions.GotrueException ex) when (ex.Message.Contains("Invalid login credentials"))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unexpected error.", error = ex.Message });
            }
        }


        [HttpPost("/Auth/ChangePassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.email) ||
                string.IsNullOrWhiteSpace(request.current_password) ||
                string.IsNullOrWhiteSpace(request.new_password))
            {
                return BadRequest(new { message = "All fields are required." });
            }

            if (request.current_password == request.new_password)
                return UnprocessableEntity(new { message = "New password must be different from the current password." });

            if (request.new_password.Length < 8)
                return UnprocessableEntity(new { message = "New password must be at least 8 characters long." });

            try
            {
                var session = await _client.Auth.SignIn(request.email, request.current_password);

                if (session?.User == null)
                    return Unauthorized(new { message = "Current password is incorrect." });

                await _client.Auth.SetSession(session.AccessToken, session.RefreshToken);

                var updatedUser = await _client.Auth.Update(new Supabase.Gotrue.UserAttributes
                {
                    Password = request.new_password
                });

                if (updatedUser == null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to update password." });

                return Ok(new { message = "Password changed successfully." });
            }
            catch (Supabase.Gotrue.Exceptions.GotrueException ex) when (ex.Message.Contains("Invalid login credentials"))
            {
                return Unauthorized(new { message = "Current password is incorrect." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error updating password.", error = ex.Message });
            }
        }


        [HttpPost("/Auth/ForgotPassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.email))
                return BadRequest(new { message = "Email is required." });

            
            if (!request.email.Contains('@') || !request.email.Contains('.'))
                return BadRequest(new { message = "Invalid email format." });

            try
            {
                await _client.Auth.ResetPasswordForEmail(request.email);

                return Ok(new { message = "If the email exists, a reset link has been sent." });
            }
            catch (Supabase.Gotrue.Exceptions.GotrueException ex) when (ex.Message.Contains("rate limit") || ex.Message.Contains("too many"))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, new { message = "Too many requests. Please wait before trying again." });
            }
            catch (Exception)
            {
                
                return Ok(new { message = "If the email exists, a reset link has been sent." });
            }
        }

        #endregion

        #region GoogleAuth
        [HttpPost("/Auth/GoogleSignIn")]
        public async Task<IActionResult> GoogleSignIn([FromBody] GoogleSignInRequest request)
        {
            try
            {
                // 1. Use Supabase to verify the Google ID Token
                var session = await _client.Auth.SignInWithIdToken(Supabase.Gotrue.Constants.Provider.Google, request.idToken);

                if (session?.User == null || string.IsNullOrEmpty(session.AccessToken))
                    return Unauthorized(new { message = "Google authentication failed." });

                var userId = Guid.Parse(session.User.Id);

                // 2. Sync with your local User table (same as your Signup logic)
                var response = await _client.From<User>().Filter("user_id", Postgrest.Constants.Operator.Equals, userId.ToString()).Get();
                var user = response.Models.FirstOrDefault();

                if (user == null)
                {
                    user = new User { user_id = userId, email = session.User.Email, created_at = DateTime.UtcNow };
                    await _client.From<User>().Insert(user);
                }

                // 3. Return the exact same AuthResponse your Flutter app expects
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
        #endregion
    }
}