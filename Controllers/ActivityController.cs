using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetWise_API.Contracts.Activity;
using PetWise_API.Models;

namespace PetWise_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly Supabase.Client _client;
        public ActivityController(Supabase.Client client)
        {
            _client = client;
        }

        #region POST
        [HttpPost("/Activity")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateActivity([FromBody] CreateActivityRequest request)
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
                    description = request.description,
                    pet_id = request.pet_id,
                    recurrence = request.recurrence,
                    time_scheduled = request.time_scheduled,
                    is_active = true, // Default to true for new activities
                    title = request.title,
                    created_at = DateTime.UtcNow
                };

                var response = await _client.From<Activity>().Insert(activity);
                var newActivity = response.Models.FirstOrDefault();

                if (newActivity == null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create activity." });

                // Standard practice is to return the created object or its ID with a 201 status
                return StatusCode(StatusCodes.Status201Created, new { newActivity.activity_id });
            }
            catch (Postgrest.Exceptions.PostgrestException ex) when (ex.Message.Contains("violates foreign key constraint"))
            {
                return Conflict(new { message = "The provided Pet ID does not exist." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error creating activity.", error = ex.Message });
            }
        }
        #endregion

        #region GET
        [HttpGet("/Activity/{activity_id:int}")]
        public async Task<IActionResult> GetActivity(int activity_id)
        {
            if (activity_id <= 0)
                return BadRequest(new { message = "Activity ID must be a positive integer." });

            try
            {
                var response = await _client.From<Activity>()
                                            .Where(a => a.activity_id == activity_id)
                                            .Get();

                var activity = response.Models.FirstOrDefault();
                if (activity == null)
                    return NotFound(new { message = $"No activity found with ID {activity_id}." });

                return Ok(new ActivityResponse
                {
                    activity_id = activity.activity_id,
                    pet_id = activity.pet_id,
                    recurrence = activity.recurrence,
                    created_at = activity.created_at,
                    title = activity.title,
                    description = activity.description,
                    time_scheduled = activity.time_scheduled,
                    is_active = activity.is_active
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving activity.", error = ex.Message });
            }
        }
        #endregion

        #region PATCH
        [HttpPatch("/Activity/{activity_id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PatchActivity(int activity_id, [FromBody] UpdateActivityRequest request)
        {
            if (activity_id <= 0)
                return BadRequest(new { message = "Activity ID must be a positive integer." });

            if (request == null)
                return BadRequest(new { message = "Request body is required." });

            try
            {
                var existingResponse = await _client.From<Activity>()
                                                     .Where(a => a.activity_id == activity_id)
                                                     .Get();

                var existing = existingResponse.Models.FirstOrDefault();

                if (existing == null)
                    return NotFound(new { message = $"No activity found with ID {activity_id}." });

                // Apply updates
                if (request.title != null) existing.title = request.title;
                if (request.description != null) existing.description = request.description;
                if (request.time_scheduled.HasValue) existing.time_scheduled = request.time_scheduled.Value;
                if (request.recurrence != null) existing.recurrence = request.recurrence;
                if (request.is_active.HasValue) existing.is_active = request.is_active.Value;

                var response = await _client.From<Activity>()
                                             .Where(a => a.activity_id == activity_id)
                                             .Update(existing);

                var updated = response.Models.FirstOrDefault();

                if (updated == null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Update failed." });

                return Ok(new ActivityResponse
                {
                    activity_id = updated.activity_id,
                    pet_id = updated.pet_id,
                    title = updated.title,
                    description = updated.description,
                    time_scheduled = updated.time_scheduled,
                    recurrence = updated.recurrence,
                    is_active = updated.is_active,
                    created_at = updated.created_at
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error updating activity.", error = ex.Message });
            }
        }
        #endregion

        #region DELETE
        [HttpDelete("/Activity/{activity_id:int}")]
        public async Task<IActionResult> DeleteActivity(int activity_id)
        {
            if (activity_id <= 0)
                return BadRequest(new { message = "Activity ID must be a positive integer." });

            try
            {
                var existing = await _client.From<Activity>()
                                .Where(a => a.activity_id == activity_id)
                                .Get();

                if (!existing.Models.Any())
                    return NotFound(new { message = $"Activity with ID {activity_id} not found." });

                await _client.From<Activity>()
                             .Where(a => a.activity_id == activity_id)
                             .Delete();

                return Ok(new { message = $"Activity {activity_id} deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting activity.", error = ex.Message });
            }
        }
        #endregion
    }
}