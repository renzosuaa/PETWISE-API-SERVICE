using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetWise_Application.Common.Interfaces;
using PetWise_API.Contracts.User;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PetWise.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    #region GET
    [HttpGet("{user_id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUser(Guid user_id, CancellationToken cancellationToken)
    {
        if (user_id == Guid.Empty)
            return BadRequest(new { message = "A valid user ID is required." });

        try
        {
            var user = await _userRepository.GetByIdAsync(user_id, cancellationToken);

            if (user == null)
                return NotFound(new { message = $"No user found with ID {user_id}." });

            return Ok(new UserResponse
            {
                user_id = user.Id,
                first_name = user.FirstName,
                last_name = user.LastName,
                email = user.Email,
                image_url = user.ImageUrl,
                nickname = user.Nickname,
                created_at = user.CreatedAt,
                has_completed_setup = user.HasCompletedSetup
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching user.", error = ex.Message });
        }
    }
    #endregion

    #region UPDATE
    [HttpPatch("{user_id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PatchUser(Guid user_id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (user_id == Guid.Empty)
            return BadRequest(new { message = "A valid user ID is required." });

        if (request == null)
            return BadRequest(new { message = "Request body is required." });

        if (string.IsNullOrWhiteSpace(request.first_name) &&
            string.IsNullOrWhiteSpace(request.last_name) &&
            string.IsNullOrWhiteSpace(request.nickname) &&
            string.IsNullOrWhiteSpace(request.image_url) &&
            request.has_completed_setup == null)
        {
            return UnprocessableEntity(new { message = "At least one field must be provided to update." });
        }

        try
        {
            var existingUser = await _userRepository.GetByIdAsync(user_id, cancellationToken);

            if (existingUser == null)
                return NotFound(new { message = $"No user found with ID {user_id}." });

            if (!string.IsNullOrEmpty(request.first_name))
                existingUser.FirstName = request.first_name;

            if (!string.IsNullOrEmpty(request.last_name))
                existingUser.LastName = request.last_name;

            if (!string.IsNullOrEmpty(request.image_url))
                existingUser.ImageUrl = request.image_url;

            if (!string.IsNullOrEmpty(request.nickname))
                existingUser.Nickname = request.nickname;

            if (request.has_completed_setup.HasValue)
                existingUser.HasCompletedSetup = request.has_completed_setup.Value;

            var updatedUser = await _userRepository.UpdateAsync(existingUser, cancellationToken);

            if (updatedUser == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Update failed." });

            return Ok(new UserResponse
            {
                user_id = updatedUser.Id,
                first_name = updatedUser.FirstName,
                last_name = updatedUser.LastName,
                nickname = updatedUser.Nickname,
                email = updatedUser.Email,
                image_url = updatedUser.ImageUrl,
                created_at = updatedUser.CreatedAt,
                has_completed_setup = updatedUser.HasCompletedSetup
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating user.", error = ex.Message });
        }
    }
    #endregion
}