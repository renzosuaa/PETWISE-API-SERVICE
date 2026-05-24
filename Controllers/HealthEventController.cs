using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetWise_API.Contracts.Activity;
using PetWise_API.Contracts.HealthEvent;
using PetWise_API.Models;

namespace PetWise_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class HealthEventController : ControllerBase
    {
        private readonly Supabase.Client _client;
        public HealthEventController(Supabase.Client client)
        {
            _client = client;
        }

        #region POST

        [HttpPost("/HealthEvent")]
        public async Task<IActionResult> CreateHealthEvent(CreateHealthEvent request)
        {
            try
            {
                var healthEvent = new HealthEvent
                {
                    created_at = DateTime.UtcNow,
                    event_date = request.event_date,
                    event_name = request.event_name,
                    is_completed = false,
                    pet_id = request.pet_id,
                    type = request.type
                };

                var response = await _client.From<HealthEvent>().Insert(healthEvent);

                var newEvent = response.Models.First();

                return Ok(newEvent.event_id);

            }
            catch (Postgrest.Exceptions.PostgrestException ex) when (ex.Message.Contains("violates foreign key constraint"))
            {
                
                return Conflict(new { message = "Pet Id doesn't exist" });
            }
        }
        #endregion

        #region GET

        [HttpGet("/HealthEvent/{event_id}")]
        public async Task<IActionResult> GetHealthEvent(int event_id)
        {
            var response = await _client.From<HealthEvent>()
                                        .Where(h => h.event_id == event_id)
                                        .Get();

            var healthEvent = response.Models.FirstOrDefault();
            if (healthEvent == null)
                return NotFound();

            var dto = new HealthEventResponse
            {
                pet_id = healthEvent.pet_id,
                type = healthEvent.type,
                event_date = healthEvent.event_date,
                event_name = healthEvent.event_name,
                is_completed = healthEvent.is_completed,
                created_at = healthEvent.created_at,
                event_id = healthEvent.event_id
            };

            return Ok(dto);
        }

        // GET /Health?pet_id=2
        [HttpGet("/HealthEvent")]
        public async Task<IActionResult> GetPetsByUser([FromQuery] int pet_id)
        {
            
            var response = await _client.From<HealthEvent>()
                                        .Filter("pet_id", Postgrest.Constants.Operator.Equals, pet_id)
                                        .Get();

            var healthEvents = response.Models;

            if (!healthEvents.Any())
                return NotFound(new { message = "No Health Events found for this pet." });

            
            var healthEventDtos = healthEvents.Select(p => new HealthEventResponse
            {
                pet_id = p.pet_id,
                type = p.type,
                event_date = p.event_date,
                event_name = p.event_name,
                is_completed = p.is_completed,
                created_at = p.created_at,
                event_id = p.event_id

            }).ToList();

            return Ok(healthEventDtos);
        }

        #endregion

        #region PATCH

        [HttpPatch("/HealthEvent/{event_id}/complete")]
        public async Task<IActionResult> CompleteActivity(int event_id)
        {
            try
            {
                await _client.From<HealthEvent>()
                             .Where(h => h.event_id == event_id)
                             .Set(h => h.is_completed, true)
                             .Update();

                return Ok(new { message = "Health marked as completed." });
            }
            catch (Postgrest.Exceptions.PostgrestException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        #endregion

        #region DELETE

        [HttpDelete("/HealthEvent/{event_id}")]
        public async Task<IActionResult> DeleteHealth(int event_id)
        {
            var existing = await _client.From<HealthEvent>()
                            .Where(h => h.event_id == event_id)
                            .Get();

            if (!existing.Models.Any())
                return NotFound(new { message = "Health Event not found." });

            await _client.From<HealthEvent>()
                         .Where(h => h.event_id == event_id)
                         .Delete();

            return Ok(new { message = $"Activity {event_id} deleted successfully." });
        }

        #endregion

    }
}
