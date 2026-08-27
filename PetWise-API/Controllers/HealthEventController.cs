using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetWise.Domain.Entities;
using PetWise_API.Contracts.HealthEvent;
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
public class HealthEventController : ControllerBase
{
    private readonly IHealthEventService _service;

    public HealthEventController(IHealthEventService service)
    {
        _service = service;
    }

    #region POST
    [HttpPost("/HealthEvent")]
    public async Task<IActionResult> CreateHealthEvent([FromBody] CreateHealthEvent request, CancellationToken cancellationToken)
    {
        try
        {
            var healthEvent = new HealthEvent
            {
                CreatedAt = DateTime.UtcNow,
                EventDate = request.event_date,
                EventName = request.event_name,
                IsCompleted = false,
                PetId = request.pet_id,
                Type = request.type
            };

            var newEvent = await _service.CreateHealthEventAsync(healthEvent, cancellationToken);
            if (newEvent == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create health event." });

            return Ok(newEvent.Id);
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    #endregion

    #region GET
    [HttpGet("/HealthEvent/{event_id:int}")]
    public async Task<IActionResult> GetHealthEvent(int event_id, CancellationToken cancellationToken)
    {
        try
        {
            var healthEvent = await _service.GetHealthEventByIdAsync(event_id, cancellationToken);

            var dto = new HealthEventResponse
            {
                pet_id = healthEvent.PetId,
                type = healthEvent.Type,
                event_date = healthEvent.EventDate,
                event_name = healthEvent.EventName,
                is_completed = healthEvent.IsCompleted,
                created_at = healthEvent.CreatedAt,
                event_id = healthEvent.Id
            };

            return Ok(dto);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("/HealthEvent")]
    public async Task<IActionResult> GetPetsByUser([FromQuery] int pet_id, CancellationToken cancellationToken)
    {
        try
        {
            var healthEvents = await _service.GetHealthEventsByPetIdAsync(pet_id, cancellationToken);

            var healthEventDtos = healthEvents.Select(p => new HealthEventResponse
            {
                pet_id = p.PetId,
                type = p.Type,
                event_date = p.EventDate,
                event_name = p.EventName,
                is_completed = p.IsCompleted,
                created_at = p.CreatedAt,
                event_id = p.Id
            }).ToList();

            return Ok(healthEventDtos);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
    #endregion

    #region PATCH
    [HttpPatch("/HealthEvent/{event_id:int}/complete")]
    public async Task<IActionResult> CompleteActivity(int event_id, CancellationToken cancellationToken)
    {
        try
        {
            await _service.CompleteHealthEventAsync(event_id, cancellationToken);
            return Ok(new { message = "Health marked as completed." });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }
    #endregion

    #region DELETE
    [HttpDelete("/HealthEvent/{event_id:int}")]
    public async Task<IActionResult> DeleteHealth(int event_id, CancellationToken cancellationToken)
    {
        try
        {
            await _service.DeleteHealthEventAsync(event_id, cancellationToken);
            return Ok(new { message = $"Activity {event_id} deleted successfully." });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
    #endregion
}