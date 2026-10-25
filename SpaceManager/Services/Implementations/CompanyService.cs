using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SpaceManager.Common;
using SpaceManager.Data;
using SpaceManager.DTOs.Companies;
using SpaceManager.Mappings;
using SpaceManager.Models;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Services.Implementations;

/// <summary>
/// Implements corporate account management, tenant isolation, and member assignments.
/// </summary>
public class CompanyService : ICompanyService
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    public CompanyService(AppDbContext context,UserManager<ApplicationUser> userManager) 
    {
        _context = context;
        _userManager = userManager;
    }
    public async Task<IEnumerable<CompanyResponseDto>> GetAllAsync()
    {
        var companies = await _context.Companies.AsNoTracking().ToListAsync();
        return companies.Select(c => c.ToResponseDto());
    }
    public async Task<ServiceResult<CompanyResponseDto>> GetByIdAsync(int id, CurrentUserContext caller)
    {
        if (!caller.IsAdmin && caller.CompanyId != id)
        {
            return ServiceResult<CompanyResponseDto>.Forbidden();
        }

        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
        if (company == null)
        {
            return ServiceResult<CompanyResponseDto>.Failure("Company not found.");
        }

        return ServiceResult<CompanyResponseDto>.Success(company.ToResponseDto());
    }
    public async Task<ServiceResult<CompanyResponseDto>> UpdateAsync(int id, UpdateCompanyDto dto)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
        if (company == null)
        {
            return ServiceResult<CompanyResponseDto>.Failure("Company not found.");
        }
        var duplicateCompany = await _context.Companies.AnyAsync(c => c.Id != id && (c.Name == dto.Name || c.TaxRegistrationNumber == dto.TaxRegistrationNumber));
        if (duplicateCompany)
        {
            return ServiceResult<CompanyResponseDto>.Failure("A company with the same name or tax registration number already exists.");
        }

        company.Name = dto.Name;
        company.TaxRegistrationNumber = dto.TaxRegistrationNumber;
        company.BillingAddress = dto.BillingAddress;
        company.ContactEmail = dto.ContactEmail;
        await _context.SaveChangesAsync();
        return ServiceResult<CompanyResponseDto>.Success(company.ToResponseDto());
    }
    public async Task<ServiceResult<CompanyResponseDto>> CreateAsync(CreateCompanyDto dto)
    {
        bool alreadyExists = await _context.Companies
            .AnyAsync(c => c.Name == dto.Name || c.TaxRegistrationNumber == dto.TaxRegistrationNumber);
        if (alreadyExists) { 
            return ServiceResult<CompanyResponseDto>.Failure("A company with the same name or tax registration number already exists.");
        }
        ApplicationUser? companyAdmin = null;
        if (dto.AdminEmail != null)
        {
            companyAdmin = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.AdminEmail);
            if (companyAdmin == null)
            {
                return ServiceResult<CompanyResponseDto>.Failure("Admin user not found.");
            }

            if (companyAdmin.CompanyId != null)
            {
                return ServiceResult<CompanyResponseDto>.Failure("Admin user is already assigned to a company.");
            }

        }
        

        var company = dto.ToEntity();
        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

       if (companyAdmin != null)
        {
            await _userManager.AddToRoleAsync(companyAdmin, "CompanyAdmin");
            companyAdmin.CompanyId = company.Id;
            await _context.SaveChangesAsync();
        }


        return ServiceResult<CompanyResponseDto>.Success(company.ToResponseDto());
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        bool isAsinnedToAnyUsers = await _context.Users.AnyAsync(u => u.CompanyId == id);
        if(isAsinnedToAnyUsers )
        {
            return ServiceResult.Failure("Company is assigned to users and cannot be deleted.");
        }
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
        if (company == null)
        {
            return ServiceResult.Failure("Company not found.");
        }
        _context.Companies.Remove(company);
        await _context.SaveChangesAsync();
        return ServiceResult.Success();
    }


    public async Task<ServiceResult> AddMemberAsync(int companyId, string email, CurrentUserContext caller)
    {
        if (!caller.CanManageCompany(companyId)) { return ServiceResult.Forbidden(); }
        var company  = await _context.Companies.FirstOrDefaultAsync(c => c.Id == companyId);
        if (company == null)
        {
            return ServiceResult.Failure("Company not found.");
        }
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return ServiceResult.Failure("User not found.");
        }
        if (user.CompanyId != null)
        {
            return ServiceResult.Failure("User is already assigned to a company.");
        }
        user.CompanyId = companyId;
        await _context.SaveChangesAsync();
        return ServiceResult.Success();
    }

   

    public async Task<ServiceResult<IEnumerable<CompanyMemberResponseDto>>> GetMembersAsync(int companyId, CurrentUserContext caller)
    {
        if (!caller.CanManageCompany(companyId)) { return ServiceResult<IEnumerable<CompanyMemberResponseDto>>.Forbidden(); }
        var members = await _context.Users.AsNoTracking()
            .Where(u => u.CompanyId == companyId)
            
            .ToListAsync();
        
        return ServiceResult<IEnumerable<CompanyMemberResponseDto>>.Success(members.Select(u => u.ToMemberDto()));
    }

    public async Task<ServiceResult> RemoveMemberAsync(int companyId, string userId, CurrentUserContext caller)
    {
        if (!caller.CanManageCompany(companyId)) { return ServiceResult.Forbidden(); }
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.CompanyId == companyId);
        if (user == null)
        {
            return ServiceResult.Failure("User not found in the specified company.");
        }
        user.CompanyId = null;
        await _context.SaveChangesAsync();
        return ServiceResult.Success();
    }
    public async  Task<ServiceResult> SetCompanyAdminAsync(int companyId, string email)
    { 
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email );
        if (user == null) {
           return ServiceResult.Failure("User not found.");
        }
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == companyId);
        if (company == null)
        {
            return ServiceResult.Failure("Company not found.");
        }
        if (user.CompanyId == null)
        {
            user.CompanyId = companyId;
            await _context.SaveChangesAsync();
        }
        if (user.CompanyId != companyId) { 
            return ServiceResult.Failure("User is assigned to a different company.");
        }
        await _userManager.AddToRoleAsync(user, "CompanyAdmin");
        return ServiceResult.Success();
    }


}

