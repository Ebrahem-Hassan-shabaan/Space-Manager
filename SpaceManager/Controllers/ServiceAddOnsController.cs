using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpaceManager.DTOs.ServiceAddOns;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Controllers;

/// <summary>
/// Provides API endpoints for managing the bookable service add-ons catalog and calculating pricing.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServiceAddOnsController : ControllerBase
{
    private readonly IAddOnService _serviceAddOnsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceAddOnsController"/> class.
    /// </summary>
    /// <param name="serviceAddOnsService">The service add-on business service instance.</param>
    public ServiceAddOnsController(IAddOnService serviceAddOnsService)
    {
        _serviceAddOnsService = serviceAddOnsService;
    }

    /// <summary>
    /// Retrieves all service add-ons. Administrators see all add-ons; regular users see only currently available items.
    /// </summary>
    /// <returns>A collection of service add-on response DTOs.</returns>
    /// <response code="200">Returns the list of service add-ons.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ServiceAddOnResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        bool? onlyAvailable = User.IsInRole("Admin") ? null : true;
        var addOns = await _serviceAddOnsService.GetAllAsync(onlyAvailable);
        return Ok(addOns);
    }

    /// <summary>
    /// Retrieves details of a specific service add-on by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the service add-on.</param>
    /// <returns>The service add-on details DTO if found.</returns>
    /// <response code="200">Returns the requested service add-on details.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="404">If no service add-on exists with the specified ID.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ServiceAddOnResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var addOn = await _serviceAddOnsService.GetByIdAsync(id);
        if (addOn == null)
        {
            return NotFound(new { message = $"Service add-on with ID {id} was not found." });
        }
        return Ok(addOn);
    }

    /// <summary>
    /// Creates a new service add-on in the catalog. (Admin restricted)
    /// </summary>
    /// <param name="dto">The creation payload.</param>
    /// <returns>The newly created service add-on DTO with its location header.</returns>
    /// <response code="201">Returns the newly created service add-on.</response>
    /// <response code="400">If the input validation fails.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="403">If the caller is not an administrator.</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceAddOnResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateServiceAddOnDto dto)
    {
        var createdAddOn = await _serviceAddOnsService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = createdAddOn.Id }, createdAddOn);
    }

    /// <summary>
    /// Updates an existing service add-on by its unique identifier. (Admin restricted)
    /// </summary>
    /// <param name="id">The unique identifier of the service add-on to update.</param>
    /// <param name="dto">The update payload.</param>
    /// <returns>The updated service add-on details DTO.</returns>
    /// <response code="200">Returns the updated service add-on details.</response>
    /// <response code="400">If the input validation fails.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="403">If the caller is not an administrator.</response>
    /// <response code="404">If no service add-on exists with the specified ID.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceAddOnResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceAddOnDto dto)
    {
        var updatedAddOn = await _serviceAddOnsService.UpdateAsync(id, dto);
        if (updatedAddOn == null)
        {
            return NotFound(new { message = $"Service add-on with ID {id} was not found." });
        }
        return Ok(updatedAddOn);
    }

    /// <summary>
    /// Deletes a service add-on by its unique identifier. (Admin restricted)
    /// </summary>
    /// <param name="id">The unique identifier of the service add-on to delete.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the service add-on was successfully deleted.</response>
    /// <response code="400">If the service add-on is associated with existing bookings.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    /// <response code="403">If the caller is not an administrator.</response>
    /// <response code="404">If no service add-on exists with the specified ID.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _serviceAddOnsService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Service add-on with ID {id} was not found." });
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Calculates the total cost for a collection of requested add-ons.
    /// </summary>
    /// <param name="addOns">The collection of requested add-on selections and quantities.</param>
    /// <returns>The calculated total cost object.</returns>
    /// <response code="200">Returns the calculated total cost.</response>
    /// <response code="400">If any requested service is invalid or unavailable.</response>
    /// <response code="401">If the request is unauthenticated.</response>
    [HttpPost("calculate-cost")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CalculateCost([FromBody] IEnumerable<BookingAddOnRequestDto> addOns)
    {
        try
        {
            var totalCost = await _serviceAddOnsService.CalculateTotalCostAsync(addOns);
            return Ok(new { totalCost });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
