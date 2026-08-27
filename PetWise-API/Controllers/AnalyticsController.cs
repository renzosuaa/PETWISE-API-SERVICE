using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PetWise_API.Contracts.Analytics;
using PetWise_Application.Common.Exceptions;
using PetWise_Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using static PetWise_API.Contracts.Analytics.UserDashboardResponseController;

namespace PetWise_API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("Pet/{pet_id:int}/activity-health")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PetActivityHealthAnalyticsResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPetActivityAndHealthAnalytics(int pet_id, CancellationToken cancellationToken)
    {
        try
        {
            var jsonContent = await _analyticsService.GetPetActivityAndHealthStatsAsync(pet_id, cancellationToken);
            var analyticsData = JsonConvert.DeserializeObject<PetActivityHealthAnalyticsResponse>(jsonContent);

            return Ok(analyticsData);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
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
    public async Task<IActionResult> GetUserDashboardAnalytics(Guid user_id, CancellationToken cancellationToken)
    {
        try
        {
            var jsonContent = await _analyticsService.GetUserDashboardAnalyticsAsync(user_id, cancellationToken);
            var dashboardData = JsonConvert.DeserializeObject<UserDashboardAnalyticsResponse>(jsonContent);

            return Ok(dashboardData);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Database analytics runtime failure.", error = ex.Message });
        }
    }
}