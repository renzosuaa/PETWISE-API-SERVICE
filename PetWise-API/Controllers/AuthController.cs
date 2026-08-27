using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetWise_Application.Common.Exceptions;
using PetWise_Application.Common.Interfaces;
using PetWise_API.Contracts.Auth; 
using System.Threading;
using System.Threading.Tasks;

namespace PetWise.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("/Auth/Signup")]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.email) || string.IsNullOrWhiteSpace(request.password))
            return BadRequest(new { message = "Email and password are required." });

        try
        {
            var result = await _authService.SignUpAsync(request.email, request.password, cancellationToken);
            if (result == null)
                return BadRequest(new { message = "Failed to create user." });

            return StatusCode(StatusCodes.Status201Created, new
            {
                user_id = result.UserId,
                email = result.Email,
                message = "Signup successful. Please verify your email."
            });
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("/Auth/Signin")]
    public async Task<IActionResult> SignIn([FromBody] SigninRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.email) || string.IsNullOrWhiteSpace(request.password))
            return BadRequest(new { message = "Email and password are required." });

        try
        {
            var result = await _authService.SignInAsync(request.email, request.password, cancellationToken);
            if (result == null)
                return Unauthorized(new { message = "Invalid email or password." });

            return Ok(new AuthResponse
            {
                user_id = result.UserId,
                email = result.Email,
                access_token = result.AccessToken
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("/Auth/ChangePassword")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.email) ||
            string.IsNullOrWhiteSpace(request.current_password) ||
            string.IsNullOrWhiteSpace(request.new_password))
        {
            return BadRequest(new { message = "All fields are required." });
        }

        if (request.current_password == request.new_password)
            return UnprocessableEntity(new { message = "New password must be different from current password." });

        if (request.new_password.Length < 8)
            return UnprocessableEntity(new { message = "New password must be at least 8 characters long." });

        var success = await _authService.ChangePasswordAsync(request.email, request.current_password, request.new_password, cancellationToken);
        if (!success)
            return Unauthorized(new { message = "Current password is incorrect." });

        return Ok(new { message = "Password changed successfully." });
    }

    [HttpPost("/Auth/ForgotPassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.email) || !request.email.Contains('@'))
            return BadRequest(new { message = "Valid email is required." });

        try
        {
            await _authService.SendPasswordResetEmailAsync(request.email, cancellationToken);
            return Ok(new { message = "If the email exists, a reset link has been sent." });
        }
        catch (TooManyRequestsException ex)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new { message = ex.Message });
        }
    }

    [HttpPost("/Auth/GoogleSignIn")]
    public async Task<IActionResult> GoogleSignIn([FromBody] GoogleSignInRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.GoogleSignInAsync(request.idToken, cancellationToken);
        if (result == null)
            return Unauthorized(new { message = "Google authentication failed." });

        return Ok(new AuthResponse
        {
            user_id = result.UserId,
            email = result.Email,
            access_token = result.AccessToken
        });
    }
}