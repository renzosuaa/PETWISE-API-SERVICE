using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PetWise_API.Contracts.Analytics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static PetWise_API.Contracts.Analytics.UserDashboardResponseController;

namespace PetWise_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly Supabase.Client _client;

        public AnalyticsController(Supabase.Client client)
        {
            _client = client;
        }

  
        [HttpGet("Pet/{pet_id:int}/activity-health")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PetActivityHealthAnalyticsResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPetActivityAndHealthAnalytics(int pet_id)
        {
            if (pet_id <= 0)
                return BadRequest(new { message = "Pet ID must be a positive integer." });

            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "target_pet_id", pet_id }
                };

                // Execute the pet-specific database function
                var response = await _client.Rpc("get_pet_activity_and_health_stats", parameters);

                if (response == null || string.IsNullOrWhiteSpace(response.Content))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new { message = "Empty response received from the analytics engine." });
                }

                var analyticsData = JsonConvert.DeserializeObject<PetActivityHealthAnalyticsResponse>(response.Content);
                return Ok(analyticsData);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Database analytics runtime failure.", error = ex.Message });
            }
        }

        [HttpGet("User/{user_id:Guid}/dashboard-summary")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDashboardAnalyticsResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserDashboardAnalytics(Guid user_id)
        {
            if (user_id == Guid.Empty)
                return BadRequest(new { message = "A valid user ID is required." });

            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "target_user_id", user_id.ToString() }
                };

                // Call the user-level aggregation stored procedure
                var response = await _client.Rpc("get_user_all_pets_stats", parameters);

                if (response == null || string.IsNullOrWhiteSpace(response.Content))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new { message = "Empty response received from the database engine." });
                }

                var dashboardData = JsonConvert.DeserializeObject<UserDashboardAnalyticsResponse>(response.Content);
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Database analytics runtime failure.", error = ex.Message });
            }
        }
    }
}