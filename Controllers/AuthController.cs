using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using PetWise_API.Contracts.Auth;
using PetWise_API.Models;
using Postgrest;
using ForgotPasswordRequest = PetWise_API.Contracts.Auth.ForgotPasswordRequest;

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
        public async Task<IActionResult> SignUp(SignUpRequest request)
        {
            try
            {
                var session = await _client.Auth.SignUp(request.email, request.password);

                if (session.User == null)
                    return BadRequest(new { message = "Failed to create user." });

                if (!Guid.TryParse(session.User.Id, out var userId))
                    return StatusCode(500, new { message = "Invalid user ID format." });

                var user = new User
                {
                    user_id = userId,
                    email = request.email,
                    created_at = DateTime.UtcNow
                };

                await _client.From<User>().Insert(user);

                return Ok(new
                {
                    user.user_id,
                    user.email,
                    message = "Signup successful. Please verify your email."
                });
            }
            catch (Postgrest.Exceptions.PostgrestException ex)
            {
                return StatusCode(500, new { message = "Database error.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error.", error = ex.Message });
            }
        }


        [HttpPost("/Auth/Signin")]
        public async Task<IActionResult> SignIn(SigninRequest request)
        {
            try
            {
                var session = await _client.Auth.SignIn(request.email, request.password);

                if (session.User == null || string.IsNullOrEmpty(session.AccessToken))
                    return Unauthorized(new { message = "Invalid email or password." });

                if (!Guid.TryParse(session.User.Id, out var userId))
                    return StatusCode(500, new { message = "Invalid user ID format." });

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
                return StatusCode(500, new { message = "Database error.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error.", error = ex.Message });
            }
        }

        [HttpPost("/Auth/ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.email))
                return BadRequest(new { message = "Email is required." });

            try
            {
                await _client.Auth.ResetPasswordForEmail(request.email);

                
                return Ok(new
                {
                    message = "If the email exists, a reset link has been sent."
                });
            }
            catch (Exception)
            {
                // Don't leak errors or email existence
                return Ok(new
                {
                    message = "If the email exists, a reset link has been sent."
                });
            }
        }
        #endregion
    }
}