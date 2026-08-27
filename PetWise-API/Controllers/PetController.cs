using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetWise_Application.Common.Exceptions;
using PetWise_Application.Common.Interfaces;
using PetWise_Domain.Entities;
using PetWise_API.Contracts.Pet;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PetWise_API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PetController : ControllerBase
{
    private readonly IPetService _petService;

    public PetController(IPetService petService)
    {
        _petService = petService;
    }

    #region POST
    [HttpPost("/Pet")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreatePet([FromBody] CreatePetRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.name))
            return BadRequest(new { message = "Pet name is required." });

        if (string.IsNullOrWhiteSpace(request.species))
            return BadRequest(new { message = "Species is required." });

        if (string.IsNullOrWhiteSpace(request.breed))
            return BadRequest(new { message = "Breed is required." });

        if (string.IsNullOrWhiteSpace(request.image_url))
            return BadRequest(new { message = "Image URL is required." });

        if (request.weight <= 0)
            return UnprocessableEntity(new { message = "Weight must be greater than 0." });

        if (request.user_id == Guid.Empty)
            return BadRequest(new { message = "A valid user ID is required." });

        try
        {
            var pet = new Pet
            {
                Name = request.name,
                Species = request.species,
                Breed = request.breed,
                ImageUrl = request.image_url,
                Weight = request.weight,
                Sex = request.sex,
                Birthday = request.birthday,
                UserId = request.user_id,
                CreatedAt = DateTime.UtcNow
            };

            var newPet = await _petService.CreatePetAsync(pet, cancellationToken);

            if (newPet == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create pet." });

            return StatusCode(StatusCodes.Status201Created, new { pet_id = newPet.Id });
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
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unexpected error.", error = ex.Message });
        }
    }
    #endregion

    #region GET SINGLE PET
    [HttpGet("/Pet/{pet_id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPet(int pet_id, CancellationToken cancellationToken)
    {
        try
        {
            var pet = await _petService.GetPetByIdAsync(pet_id, cancellationToken);

            return Ok(new PetResponse
            {
                pet_id = pet.Id,
                name = pet.Name,
                species = pet.Species,
                birthday = (DateTime)pet.Birthday,
                sex = pet.Sex,
                breed = pet.Breed,
                weight = (float)pet.Weight,
                created_at = pet.CreatedAt,
                user_id = pet.UserId,
                image_url = pet.ImageUrl
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
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving pet.", error = ex.Message });
        }
    }
    #endregion

    #region GET BY USER
    [HttpGet("/Pet")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPetsByUser([FromQuery] Guid user_id, CancellationToken cancellationToken)
    {
        try
        {
            var pets = await _petService.GetPetsByUserIdAsync(user_id, cancellationToken);

            if (!pets.Any())
                return NoContent();

            var result = pets.Select(p => new PetResponse
            {
                pet_id = p.Id,
                name = p.Name,
                species = p.Species,
                birthday = (DateTime)p.Birthday,
                sex = p.Sex,
                created_at = p.CreatedAt,
                user_id = p.UserId,
                image_url = p.ImageUrl,
                breed = p.Breed,
                weight = (float)p.Weight
            });

            return Ok(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving pets.", error = ex.Message });
        }
    }
    #endregion

    #region UPDATE
    [HttpPatch("/Pet/{pet_id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PatchPet(int pet_id, [FromBody] UpdatePetRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest(new { message = "Request body is required." });

        if (request.weight.HasValue && request.weight <= 0)
            return UnprocessableEntity(new { message = "Weight must be greater than 0." });

        if (!string.IsNullOrEmpty(request.sex) && request.sex != "Male" && request.sex != "Female")
            return UnprocessableEntity(new { message = "Sex must be 'Male' or 'Female'." });

        if (request.name != null && string.IsNullOrWhiteSpace(request.name))
            return BadRequest(new { message = "Pet name cannot be empty." });

        if (request.image_url != null && string.IsNullOrWhiteSpace(request.image_url))
            return BadRequest(new { message = "Image URL cannot be empty." });

        try
        {
            var existingPet = await _petService.GetPetByIdAsync(pet_id, cancellationToken);

            if (!string.IsNullOrEmpty(request.name)) existingPet.Name = request.name;
            if (!string.IsNullOrEmpty(request.species)) existingPet.Species = request.species;
            if (request.birthday.HasValue) existingPet.Birthday = request.birthday.Value;
            if (!string.IsNullOrEmpty(request.breed)) existingPet.Breed = request.breed;
            if (request.weight.HasValue) existingPet.Weight = request.weight.Value;
            if (!string.IsNullOrEmpty(request.sex)) existingPet.Sex = request.sex;
            if (!string.IsNullOrEmpty(request.image_url)) existingPet.ImageUrl = request.image_url;

            var updatedPet = await _petService.UpdatePetAsync(existingPet, cancellationToken);

            if (updatedPet == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Update failed." });

            return Ok(new PetResponse
            {
                pet_id = updatedPet.Id,
                name = updatedPet.Name,
                species = updatedPet.Species,
                birthday = (DateTime)updatedPet.Birthday,
                sex = updatedPet.Sex,
                breed = updatedPet.Breed,
                image_url = updatedPet.ImageUrl,
                weight = (float)updatedPet.Weight,
                created_at = updatedPet.CreatedAt,
                user_id = updatedPet.UserId
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
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error updating pet.", error = ex.Message });
        }
    }
    #endregion

    #region DELETE
    [HttpDelete("/Pet/{pet_id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SoftDeletePet(int pet_id, CancellationToken cancellationToken)
    {
        try
        {
            await _petService.SoftDeletePetAsync(pet_id, cancellationToken);
            return Ok(new { message = $"Pet with ID {pet_id} has been deactivated (soft-deleted)." });
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
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unexpected error.", error = ex.Message });
        }
    }
    #endregion
}