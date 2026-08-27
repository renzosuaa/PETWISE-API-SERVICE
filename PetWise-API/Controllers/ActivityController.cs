using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetWise.Domain.Entities;
using PetWise_API.Contracts.Activity;
using PetWise_Application.Common.Exceptions;
using PetWise_Application.Common.Interfaces;
using PetWise_Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PetWise_API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ActivityController : ControllerBase
{
    private readonly IActivityService _activityService;

    public ActivityController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    #region POST
    [HttpPost("/Activity")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateActivity([FromBody] CreateActivityRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest(new { message = "Request body is required." });

        if (string.IsNullOrWhiteSpace(request.title))
            return BadRequest(new { message = "Activity title is required." });

        if (request.pet_id <= 0)
            return BadRequest(new { message = "A valid Pet ID is required." });

        try
        {
            var activity = new Activity
            {
                Title = request.title,
                Description = request.description,
                PetId = request.pet_id,
                Recurrence = request.recurrence,
                TimeScheduled = request.time_scheduled,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var newActivity = await _activityService.CreateActivityAsync(activity, cancellationToken);

            if (newActivity == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create activity." });

            return StatusCode(StatusCodes.Status201Created, new { activity_id = newActivity.Id });
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error creating activity.", error = ex.Message });
        }
    }
    #endregion

    #region GET
    [HttpGet("/Activity/{activity_id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActivity(int activity_id, CancellationToken cancellationToken)
    {
        try
        {
            var activity = await _activityService.GetActivityByIdAsync(activity_id, cancellationToken);

            return Ok(new ActivityResponse
            {
                activity_id = activity.Id,
                pet_id = activity.PetId,
                title = activity.Title,
                description = activity.Description,
                time_scheduled = activity.TimeScheduled,
                recurrence = activity.Recurrence,
                is_active = activity.IsActive,
                created_at = activity.CreatedAt
            });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving activity.", error = ex.Message });
        }
    }

    [HttpGet("/Activity/Pet/{pet_id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActivitiesByPet(int pet_id, CancellationToken cancellationToken)
    {
        try
        {
            var activities = await _activityService.GetActivitiesByPetIdAsync(pet_id, cancellationToken);

            if (!activities.Any())
                return NoContent();

            var result = activities.Select(a => new ActivityResponse
            {
                activity_id = a.Id,
                pet_id = a.PetId,
                title = a.Title,
                description = a.Description,
                time_scheduled = a.TimeScheduled,
                recurrence = a.Recurrence,
                is_active = a.IsActive,
                created_at = a.CreatedAt
            });

            return Ok(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving activities for the specified pet.", error = ex.Message });
        }
    }
    #endregion

    #region PATCH
    [HttpPatch("/Activity/{activity_id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PatchActivity(int activity_id, [FromBody] UpdateActivityRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest(new { message = "Request body is required." });

        try
        {
            var existing = await _activityService.GetActivityByIdAsync(activity_id, cancellationToken);

            if (request.title != null) existing.Title = request.title;
            if (request.description != null) existing.Description = request.description;
            if (request.time_scheduled.HasValue) existing.TimeScheduled = request.time_scheduled.Value;
            if (request.recurrence != null) existing.Recurrence = request.recurrence;
            if (request.is_active.HasValue) existing.IsActive = request.is_active.Value;

            var updated = await _activityService.UpdateActivityAsync(existing, cancellationToken);

            if (updated == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Update failed." });

            return Ok(new ActivityResponse
            {
                activity_id = updated.Id,
                pet_id = updated.PetId,
                title = updated.Title,
                description = updated.Description,
                time_scheduled = updated.TimeScheduled,
                recurrence = updated.Recurrence,
                is_active = updated.IsActive,
                created_at = updated.CreatedAt
            });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error updating activity.", error = ex.Message });
        }
    }
    #endregion

    #region DELETE
    [HttpDelete("/Activity/{activity_id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteActivity(int activity_id, CancellationToken cancellationToken)
    {
        try
        {
            await _activityService.DeleteActivityAsync(activity_id, cancellationToken);
            return Ok(new { message = $"Activity {activity_id} deleted successfully." });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error deleting activity.", error = ex.Message });
        }
    }
    #endregion
}