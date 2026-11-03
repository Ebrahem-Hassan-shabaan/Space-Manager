using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SpaceManager.Common;
using SpaceManager.Data;
using SpaceManager.DTOs.Notifications;
using SpaceManager.DTOs.Payments;
using SpaceManager.DTOs.Subscriptions;
using SpaceManager.Enums;
using SpaceManager.Mappings;
using SpaceManager.Models;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Services.Implementations;

/// <summary>
/// Implements subscription tier management, self-service checkouts via payment gateways, and member privilege enforcement.
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly AppDbContext _db;
    private readonly IPaymentService _paymentService;
    private readonly INotificationService _notificationService;

    public SubscriptionService(
        AppDbContext db,
        IPaymentService paymentService,
        INotificationService notificationService)
    {
        _db = db;
        _paymentService = paymentService;
        _notificationService = notificationService;
    }

    public async Task<ServiceResult<List<SubscriptionPlanResponseDto>>> GetAllActivePlansAsync()
    {
        var plans = await _db.SubscriptionPlans
            .AsNoTracking()
            .Include(p => p.WorkspaceType)
            .Include(p => p.UserSubscriptions)
            .Where(p => p.IsActive)
            .OrderBy(p => p.MonthlyPrice)
            .ToListAsync();

        return ServiceResult<List<SubscriptionPlanResponseDto>>.Success(plans.Select(p => p.ToResponseDto()).ToList());
    }

    public async Task<ServiceResult<List<SubscriptionPlanResponseDto>>> GetAllPlansAdminAsync(CurrentUserContext caller)
    {
        if (!caller.IsAdmin)
            return ServiceResult<List<SubscriptionPlanResponseDto>>.Forbidden();

        var plans = await _db.SubscriptionPlans
            .AsNoTracking()
            .Include(p => p.WorkspaceType)
            .Include(p => p.UserSubscriptions)
            .OrderByDescending(p => p.IsActive)
            .ThenBy(p => p.MonthlyPrice)
            .ToListAsync();

        return ServiceResult<List<SubscriptionPlanResponseDto>>.Success(plans.Select(p => p.ToResponseDto()).ToList());
    }

    public async Task<ServiceResult<SubscriptionPlanResponseDto>> GetPlanByIdAsync(int id)
    {
        var plan = await _db.SubscriptionPlans
            .AsNoTracking()
            .Include(p => p.WorkspaceType)
            .Include(p => p.UserSubscriptions)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plan == null)
            return ServiceResult<SubscriptionPlanResponseDto>.Failure($"Subscription plan #{id} not found.");

        return ServiceResult<SubscriptionPlanResponseDto>.Success(plan.ToResponseDto());
    }

    public async Task<ServiceResult<SubscriptionPlanResponseDto>> CreatePlanAsync(CreateSubscriptionPlanDto dto, CurrentUserContext caller)
    {
        if (!caller.IsAdmin)
            return ServiceResult<SubscriptionPlanResponseDto>.Forbidden("Only platform administrators can create membership plans.");

        var nameExists = await _db.SubscriptionPlans.AnyAsync(p => p.Name.ToLower() == dto.Name.Trim().ToLower());
        if (nameExists)
            return ServiceResult<SubscriptionPlanResponseDto>.Failure($"A subscription plan named '{dto.Name}' already exists.");

        if (dto.WorkspaceTypeId.HasValue)
        {
            var typeExists = await _db.WorkspaceTypes.AnyAsync(t => t.Id == dto.WorkspaceTypeId.Value);
            if (!typeExists)
                return ServiceResult<SubscriptionPlanResponseDto>.Failure($"Workspace classification #{dto.WorkspaceTypeId.Value} does not exist.");
        }

        var plan = dto.ToEntity();
        _db.SubscriptionPlans.Add(plan);
        await _db.SaveChangesAsync();

        if (plan.WorkspaceTypeId.HasValue)
            plan.WorkspaceType = await _db.WorkspaceTypes.FindAsync(plan.WorkspaceTypeId.Value);

        return ServiceResult<SubscriptionPlanResponseDto>.Success(plan.ToResponseDto());
    }

    public async Task<ServiceResult<SubscriptionPlanResponseDto>> UpdatePlanAsync(int id, UpdateSubscriptionPlanDto dto, CurrentUserContext caller)
    {
        if (!caller.IsAdmin)
            return ServiceResult<SubscriptionPlanResponseDto>.Forbidden("Only platform administrators can modify membership plans.");

        var plan = await _db.SubscriptionPlans.Include(p => p.WorkspaceType).FirstOrDefaultAsync(p => p.Id == id);
        if (plan == null)
            return ServiceResult<SubscriptionPlanResponseDto>.Failure($"Subscription plan #{id} not found.");

        if (!string.IsNullOrWhiteSpace(dto.Name) && !string.Equals(plan.Name, dto.Name.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            var nameConflict = await _db.SubscriptionPlans.AnyAsync(p => p.Id != id && p.Name.ToLower() == dto.Name.Trim().ToLower());
            if (nameConflict)
                return ServiceResult<SubscriptionPlanResponseDto>.Failure($"Another plan named '{dto.Name}' already exists.");

            plan.Name = dto.Name.Trim();
        }

        if (dto.Description != null)
            plan.Description = dto.Description.Trim();

        if (dto.MonthlyPrice.HasValue)
            plan.MonthlyPrice = dto.MonthlyPrice.Value;

        if (dto.DurationMonths.HasValue)
            plan.DurationMonths = dto.DurationMonths.Value;

        if (dto.MaxBookingsPerMonth.HasValue)
            plan.MaxBookingsPerMonth = dto.MaxBookingsPerMonth.Value;

        if (dto.WorkspaceTypeId.HasValue)
        {
            var typeExists = await _db.WorkspaceTypes.AnyAsync(t => t.Id == dto.WorkspaceTypeId.Value);
            if (!typeExists)
                return ServiceResult<SubscriptionPlanResponseDto>.Failure($"Workspace classification #{dto.WorkspaceTypeId.Value} does not exist.");

            plan.WorkspaceTypeId = dto.WorkspaceTypeId.Value;
        }

        if (dto.IsActive.HasValue)
            plan.IsActive = dto.IsActive.Value;

        await _db.SaveChangesAsync();
        return ServiceResult<SubscriptionPlanResponseDto>.Success(plan.ToResponseDto());
    }

    public async Task<ServiceResult> DeactivatePlanAsync(int id, CurrentUserContext caller)
    {
        if (!caller.IsAdmin)
            return ServiceResult.Forbidden("Only platform administrators can deactivate membership plans.");

        var plan = await _db.SubscriptionPlans.FindAsync(id);
        if (plan == null)
            return ServiceResult.Failure($"Subscription plan #{id} not found.");

        plan.IsActive = false;
        await _db.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<UserSubscriptionResponseDto>> SubscribeAsync(SubscribeRequestDto request, CurrentUserContext caller)
    {
        if (string.IsNullOrWhiteSpace(caller.UserId))
            return ServiceResult<UserSubscriptionResponseDto>.Forbidden();

        // 1. Check existing active subscription
        var existingActive = await _db.UserSubscriptions
            .Include(s => s.SubscriptionPlan)
            .FirstOrDefaultAsync(s => s.UserId == caller.UserId && s.Status == SubscriptionStatus.Active && s.EndDate >= DateTime.UtcNow);

        if (existingActive != null)
            return ServiceResult<UserSubscriptionResponseDto>.Failure(
                $"You already have an active subscription to '{existingActive.SubscriptionPlan.Name}' valid until {existingActive.EndDate:MMM dd, yyyy}.");

        // 2. Load plan
        var plan = await _db.SubscriptionPlans.Include(p => p.WorkspaceType).FirstOrDefaultAsync(p => p.Id == request.PlanId);
        if (plan == null || !plan.IsActive)
            return ServiceResult<UserSubscriptionResponseDto>.Failure("The selected subscription plan is currently unavailable.");

        // Rule: CompanyBalance payments require affiliation with a corporate account
        if (request.PaymentMethod == PaymentMethod.CompanyBalance && !caller.CompanyId.HasValue && !caller.IsCompanyAdmin)
        {
            return ServiceResult<UserSubscriptionResponseDto>.Forbidden("Corporate Balance payment is only available for accounts affiliated with a corporate organization.");
        }

        // 3. Validate payment credentials before issuing invoice
        string token;
        if (request.PaymentMethod == PaymentMethod.CreditCard || request.PaymentMethod == PaymentMethod.DebitCard)
        {
            var rawToken = !string.IsNullOrWhiteSpace(request.PaymentToken)
                ? request.PaymentToken.Trim()
                : (!string.IsNullOrWhiteSpace(request.CardNumber) ? request.CardNumber.Trim() : null);

            if (string.IsNullOrWhiteSpace(rawToken))
            {
                return ServiceResult<UserSubscriptionResponseDto>.Failure(
                    "A valid card number or payment token is required for credit/debit card transactions.");
            }

            token = rawToken;
        }
        else if (request.PaymentMethod == PaymentMethod.CompanyBalance)
        {
            token = !string.IsNullOrWhiteSpace(request.PaymentToken)
                ? request.PaymentToken.Trim()
                : $"COMP-BAL-{Guid.NewGuid():N}"[..16].ToUpperInvariant();
        }
        else
        {
            token = !string.IsNullOrWhiteSpace(request.PaymentToken)
                ? request.PaymentToken.Trim()
                : $"{request.PaymentMethod.ToString().ToUpperInvariant()}-{Guid.NewGuid():N}"[..16].ToUpperInvariant();
        }

        // 4. Issue subscription invoice
        var invoice = new Invoice
        {
            UserId = caller.UserId,
            Amount = plan.MonthlyPrice,
            IssuedDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddMinutes(15),
            Status = InvoiceStatus.Issued
        };
        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();

        // 5. Process payment transaction via payment rails
        var paymentResult = await _paymentService.ProcessAsync(
            new ProcessPaymentDto(invoice.Id, plan.MonthlyPrice, request.PaymentMethod, token),
            caller);

        if (!paymentResult.Succeeded)
        {
            invoice.Status = InvoiceStatus.Cancelled;
            await _db.SaveChangesAsync();
            return ServiceResult<UserSubscriptionResponseDto>.Failure($"Payment failed: {paymentResult.Error}");
        }

        // 5. Activate user subscription
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddMonths(plan.DurationMonths);

        var subscription = new UserSubscription
        {
            UserId = caller.UserId,
            SubscriptionPlanId = plan.Id,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true,
            IsAutoRenew = request.IsAutoRenew,
            Status = SubscriptionStatus.Active
        };

        _db.UserSubscriptions.Add(subscription);
        await _db.SaveChangesAsync();

        subscription.SubscriptionPlan = plan;

        // 6. Notify subscriber
        await _notificationService.SendNotificationAsync(new CreateNotificationDto
        {
            UserId = caller.UserId,
            Title = "Subscription Activated",
            Message = $"Welcome to {plan.Name}! Your membership is active through {endDate:MMM dd, yyyy}.",
            ActionUrl = "/subscriptions/my"
        });

        return ServiceResult<UserSubscriptionResponseDto>.Success(subscription.ToResponseDto(0));
    }

    public async Task<ServiceResult<UserSubscriptionResponseDto>> AdminAssignSubscriptionAsync(AdminAssignSubscriptionDto dto, CurrentUserContext caller)
    {
        if (!caller.IsAdmin)
            return ServiceResult<UserSubscriptionResponseDto>.Forbidden("Only administrators can manually assign subscriptions.");

        var targetUser = await _db.Users.FindAsync(dto.UserId);
        if (targetUser == null)
            return ServiceResult<UserSubscriptionResponseDto>.Failure($"Target user #{dto.UserId} not found.");

        var plan = await _db.SubscriptionPlans.Include(p => p.WorkspaceType).FirstOrDefaultAsync(p => p.Id == dto.PlanId);
        if (plan == null)
            return ServiceResult<UserSubscriptionResponseDto>.Failure($"Subscription plan #{dto.PlanId} not found.");

        // Expire or supersede any prior active subscription
        var priorActive = await _db.UserSubscriptions
            .Where(s => s.UserId == dto.UserId && s.Status == SubscriptionStatus.Active)
            .ToListAsync();

        foreach (var sub in priorActive)
        {
            sub.Status = SubscriptionStatus.Cancelled;
            sub.IsActive = false;
        }

        // Generate paid invoice if not complimentary
        if (!dto.IsComplimentary)
        {
            var invoice = new Invoice
            {
                UserId = dto.UserId,
                Amount = plan.MonthlyPrice,
                IssuedDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow,
                Status = InvoiceStatus.Paid
            };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            var payment = new Payment
            {
                InvoiceId = invoice.Id,
                Amount = plan.MonthlyPrice,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = dto.PaymentMethod,
                TransactionReference = $"ADMIN-SUB-{Guid.NewGuid():N}".Substring(0, 24).ToUpperInvariant()
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();
        }

        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddMonths(plan.DurationMonths);

        var newSubscription = new UserSubscription
        {
            UserId = dto.UserId,
            SubscriptionPlanId = plan.Id,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true,
            IsAutoRenew = dto.IsAutoRenew,
            Status = SubscriptionStatus.Active
        };

        _db.UserSubscriptions.Add(newSubscription);
        await _db.SaveChangesAsync();

        newSubscription.SubscriptionPlan = plan;

        await _notificationService.SendNotificationAsync(new CreateNotificationDto
        {
            UserId = dto.UserId,
            Title = "Membership Granted",
            Message = $"An administrator has activated your {plan.Name} subscription valid through {endDate:MMM dd, yyyy}.",
            ActionUrl = "/subscriptions/my"
        });

        return ServiceResult<UserSubscriptionResponseDto>.Success(newSubscription.ToResponseDto(0));
    }

    public async Task<ServiceResult<UserSubscriptionResponseDto?>> GetMySubscriptionAsync(CurrentUserContext caller)
    {
        if (string.IsNullOrWhiteSpace(caller.UserId))
            return ServiceResult<UserSubscriptionResponseDto?>.Forbidden();

        return await GetActiveSubscriptionForUserAsync(caller.UserId);
    }

    public async Task<ServiceResult<UserSubscriptionResponseDto?>> GetActiveSubscriptionForUserAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return ServiceResult<UserSubscriptionResponseDto?>.Success(null);

        var subscription = await _db.UserSubscriptions
            .AsNoTracking()
            .Include(s => s.SubscriptionPlan)
                .ThenInclude(p => p.WorkspaceType)
            .Where(s => s.UserId == userId && s.Status == SubscriptionStatus.Active && s.EndDate >= DateTime.UtcNow)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync();

        if (subscription == null)
            return ServiceResult<UserSubscriptionResponseDto?>.Success(null);

        var firstDayOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var bookingsQuery = _db.WorkspaceBookings
            .Where(b => b.UserId == userId &&
                        b.CreatedAt >= firstDayOfMonth &&
                        b.Status != BookingStatus.Cancelled &&
                        b.Status != BookingStatus.PendingPayment);

        if (subscription.SubscriptionPlan.WorkspaceTypeId.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.Workspace.WorkspaceTypeId == subscription.SubscriptionPlan.WorkspaceTypeId.Value);
        }

        var bookingsCount = await bookingsQuery.CountAsync();

        return ServiceResult<UserSubscriptionResponseDto?>.Success(subscription.ToResponseDto(bookingsCount));
    }

    public async Task<ServiceResult> CancelSubscriptionAsync(CurrentUserContext caller)
    {
        if (string.IsNullOrWhiteSpace(caller.UserId))
            return ServiceResult.Forbidden();

        var subscription = await _db.UserSubscriptions
            .Include(s => s.SubscriptionPlan)
            .FirstOrDefaultAsync(s => s.UserId == caller.UserId && s.Status == SubscriptionStatus.Active && s.EndDate >= DateTime.UtcNow);

        if (subscription == null)
            return ServiceResult.Failure("No active subscription found to cancel.");

        subscription.Status = SubscriptionStatus.Cancelled;
        subscription.IsAutoRenew = false;
        await _db.SaveChangesAsync();

        await _notificationService.SendNotificationAsync(new CreateNotificationDto
        {
            UserId = caller.UserId,
            Title = "Subscription Cancelled",
            Message = $"Your {subscription.SubscriptionPlan.Name} subscription has been cancelled. You retain membership access until {subscription.EndDate:MMM dd, yyyy}.",
            ActionUrl = "/subscriptions/my"
        });

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ToggleAutoRenewAsync(bool enable, CurrentUserContext caller)
    {
        if (string.IsNullOrWhiteSpace(caller.UserId))
            return ServiceResult.Forbidden();

        var subscription = await _db.UserSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == caller.UserId && s.Status == SubscriptionStatus.Active && s.EndDate >= DateTime.UtcNow);

        if (subscription == null)
            return ServiceResult.Failure("No active subscription found.");

        subscription.IsAutoRenew = enable;
        await _db.SaveChangesAsync();

        return ServiceResult.Success();
    }
}
