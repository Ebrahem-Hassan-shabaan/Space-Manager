using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpaceManager.DTOs.Companies;
using SpaceManager.Services.Interfaces;
using SpaceManager.Common;

namespace SpaceManager.Controllers;

/// <summary>
/// Provides administrative and tenant endpoints for managing corporate companies, roster memberships, and administrators.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompaniesController"/> class.
    /// </summary>
    /// <param name="companyService">The company service responsible for business logic execution.</param>
    public CompaniesController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    /// <summary>
    /// Retrieves all registered corporate accounts. Restricted to platform administrators.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var companies = await _companyService.GetAllAsync();
        return Ok(companies);
    }

    /// <summary>
    /// Retrieves detailed information for a specific company by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the company.</param>
    [Authorize(Roles = "Admin,CompanyAdmin")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var caller = User.GetUserContext();
        var result = await _companyService.GetByIdAsync(id, caller);
        if (!result.Succeeded)
        {
            return result.IsForbidden
                ? Forbid()
                : NotFound(new { message = result.Error });
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Registers a new corporate company account. Restricted to platform administrators.
    /// </summary>
    /// <param name="dto">The company registration payload.</param>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCompanyDto dto)
    {
        var result = await _companyService.CreateAsync(dto);
        if (!result.Succeeded)
        {
            return BadRequest(result.Error);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Updates details of an existing company. Restricted to platform administrators.
    /// </summary>
    /// <param name="id">The unique identifier of the company to update.</param>
    /// <param name="dto">The updated company attributes.</param>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCompanyDto dto)
    {
        var result = await _companyService.UpdateAsync(id, dto);
        if (!result.Succeeded)
        {
            return result.Error == "Company not found."
                ? NotFound(new { message = result.Error })
                : BadRequest(new { message = result.Error });
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Deletes a corporate company record. Restricted to platform administrators.
    /// </summary>
    /// <param name="id">The unique identifier of the company to remove.</param>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _companyService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            return result.Error == "Company not found."
                ? NotFound(new { message = result.Error })
                : BadRequest(new { message = result.Error });
        }
        return NoContent();
    }

    /// <summary>
    /// Retrieves the list of member accounts affiliated with a company.
    /// </summary>
    /// <param name="companyId">The unique identifier of the company.</param>
    [Authorize(Roles = "Admin,CompanyAdmin")]
    [HttpGet("{companyId:int}/members")]
    public async Task<IActionResult> GetMembers(int companyId)
    {
        var caller = User.GetUserContext();
        var result = await _companyService.GetMembersAsync(companyId, caller);
        if (!result.Succeeded)
        {
            return Forbid();
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Enrolls an existing user into a company by their email address.
    /// </summary>
    /// <param name="companyId">The unique identifier of the company.</param>
    /// <param name="dto">The member enrollment payload.</param>
    [Authorize(Roles = "Admin,CompanyAdmin")]
    [HttpPost("{companyId:int}/members")]
    public async Task<IActionResult> AddMember(int companyId, [FromBody] AddCompanyMemberDto dto)
    {
        var caller = User.GetUserContext();
        var result = await _companyService.AddMemberAsync(companyId, dto.Email, caller);
        if (!result.Succeeded)
        {
            return result.Error switch
            {
                "Company not found." => NotFound(new { message = result.Error }),
                "User not found." => NotFound(new { message = result.Error }),
                "User is already assigned to a company." => BadRequest(result.Error),
                _ => Forbid()
            };
        }
        return Ok();
    }

    /// <summary>
    /// Removes a member from a corporate company roster.
    /// </summary>
    /// <param name="companyId">The unique identifier of the company.</param>
    /// <param name="userId">The unique identifier of the member to detach.</param>
    [Authorize(Roles = "Admin,CompanyAdmin")]
    [HttpDelete("{companyId:int}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(int companyId, string userId)
    {
        var caller = User.GetUserContext();
        var result = await _companyService.RemoveMemberAsync(companyId, userId, caller);
        if (!result.Succeeded)
        {
            return result.Error switch
            {
                "Company not found." => NotFound(new { message = result.Error }),
                "User not found." => NotFound(new { message = result.Error }),
                "User not found in the specified company." => BadRequest(result.Error),
                _ => Forbid()
            };
        }
        return NoContent();
    }

    /// <summary>
    /// Designates an enrolled company member as a Company Administrator. Restricted to platform administrators.
    /// </summary>
    /// <param name="companyId">The unique identifier of the company.</param>
    /// <param name="dto">The target user email payload.</param>
    [Authorize(Roles = "Admin")]
    [HttpPut("{companyId:int}/admin")]
    public async Task<IActionResult> SetCompanyAdmin(int companyId, [FromBody] AssignCompanyAdminDto dto)
    {
        var result = await _companyService.SetCompanyAdminAsync(companyId, dto.Email);
        if (!result.Succeeded)
        {
            return result.Error switch
            {
                "Company not found." => NotFound(new { message = result.Error }),
                "User not found." => NotFound(new { message = result.Error }),
                _ => BadRequest(new { message = result.Error })
            };
        }
        return Ok(new { message = "Company admin assigned successfully." });
    }
}
