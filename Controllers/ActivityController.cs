using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetWise_API.Contracts.Activity;
using PetWise_API.Contracts.Pet;
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
        public async Task<IActionResult> CreateActivity(CreateActivityRequest request)
        {
            try
            {
                var activity = new Activity
                {
                    description = request.description,
                    pet_id = request.pet_id,
                    recurrence = request.recurrence,
                    scheduled_date = request.scheduled_date,
                    is_completed = false,
                    title = request.title,
                    created_at = DateTime.UtcNow
                };

                var response = await _client.From<Activity>().Insert(activity);

                var newActivity = response.Models.First();

                return Ok(newActivity.activity_id);

            }
            catch (Postgrest.Exceptions.PostgrestException ex) when (ex.Message.Contains("violates foreign key constraint"))
            {
                // Will be triggered if the user_id provided doesn't exist in the users table
                return Conflict(new { message = "Pet Id doesn't exist" });
            }
        }

        #endregion

        #region GET
        [HttpGet("/Activity/{activity_id}")]
        public async Task<IActionResult> GetActivity(int activity_id)
        {
            var response = await _client.From<Activity>()
                                        .Where(a => a.activity_id == activity_id)
                                        .Get();

            var activity = response.Models.FirstOrDefault();
            if (activity == null)
                return NotFound();

            var dto = new ActivityResponse
            {
                activity_id = activity_id,
                pet_id = activity.pet_id,
                recurrence = activity.recurrence,
                created_at = activity.created_at,
                title = activity.title,
                description = activity.description,
                scheduled_date = activity.scheduled_date,
                is_completed = activity.is_completed
            };

            return Ok(dto);
        }

        // GET /Activity?pet_id=2
        [HttpGet("/Activity")]
        public async Task<IActionResult> GetPetsByUser([FromQuery] int pet_id)
        {
            // 1️⃣ Use .Filter() to translate query param for PostgREST
            var response = await _client.From<Activity>()
                                        .Filter("pet_id", Postgrest.Constants.Operator.Equals, pet_id)
                                        .Get();

            var activities = response.Models;

            if (!activities.Any())
                return NotFound(new { message = "No Activities found for this pet." });

            // 2️⃣ Map to DTOs
            var activityDtos = activities.Select(p => new ActivityResponse
            {
                activity_id = p.activity_id,
                pet_id = p.pet_id,
                created_at = p.created_at,
                title = p.title,
                description = p.description,
                scheduled_date = p.scheduled_date,
                is_completed = p.is_completed,
                recurrence = p.recurrence

            }).ToList();

            return Ok(activityDtos);
        }
        #endregion

        #region DELETE

        [HttpDelete("/Activity/{activity_id}")]
        public async Task<IActionResult> DeleteActivity(int activity_id)
        {
            var existing = await _client.From<Activity>()
                            .Where(a => a.activity_id == activity_id)
                            .Get();

            if (!existing.Models.Any())
                return NotFound(new { message = "Activity not found." });

            await _client.From<Activity>()
                         .Where(a => a.activity_id == activity_id)
                         .Delete();

            return Ok(new { message = $"Activity {activity_id} deleted successfully." });
        }

        #endregion

        #region PATCH

        [HttpPatch("/Activity/{activity_id}/complete")]
        public async Task<IActionResult> CompleteActivity(int activity_id)
        {
            try
            {
                await _client.From<Activity>()
                             .Where(a => a.activity_id == activity_id)
                             .Set(a => a.is_completed, true)
                             .Update();

                return Ok(new { message = "Activity marked as completed." });
            }
            catch (Postgrest.Exceptions.PostgrestException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        #endregion
    }
}









