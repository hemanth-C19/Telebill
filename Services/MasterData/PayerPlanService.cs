using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Dto.MasterData;
using Telebill.Models;
using Telebill.Repositories.MasterData;

namespace Telebill.Services.MasterData;

public class PayerPlanService : IPayerPlanService
{
    private readonly IPayerRepository _payerRepo;
    private readonly IPayerPlanRepository _planRepo;

    public PayerPlanService(IPayerRepository payerRepo, IPayerPlanRepository planRepo)
    {
        _payerRepo = payerRepo;
        _planRepo = planRepo;
    }

    public async Task<IEnumerable<PayerPlan>> GetPlansByPayerIdAsync(int payerId)
    {
        if (!await _payerRepo.ExistsAsync(payerId))
            throw new KeyNotFoundException($"Payer {payerId} not found.");

        return await _planRepo.GetByPayerIdAsync(payerId);
    }

    public async Task<IEnumerable<PlanNamesDTO>> GetPlanNamesByPayerIdAsync(int payerId)
    {
        if (!await _payerRepo.ExistsAsync(payerId))
            throw new KeyNotFoundException($"Payer {payerId} not found.");

        var plans = await _planRepo.GetActiveByPayerIdAsync(payerId);
        return plans.Select(p => new PlanNamesDTO { PlanId = p.PlanId, PlanName = p.PlanName });
    }

    public async Task AddPlanAsync(PayerPlanDTO plan)
    {
        if (!plan.PayerId.HasValue)
            throw new ArgumentException("PayerId is required.");
        if (string.IsNullOrWhiteSpace(plan.PlanName))
            throw new ArgumentException("PlanName is required.");

        if (!await _payerRepo.ExistsAsync(plan.PayerId.Value))
            throw new KeyNotFoundException($"Payer {plan.PayerId.Value} not found.");

        var entity = new PayerPlan
        {
            PayerId = plan.PayerId,
            PlanName = plan.PlanName,
            NetworkType = plan.NetworkType,
            Posdefault = plan.Posdefault,
            TelehealthModifiersJson = plan.TelehealthModifiersJson,
            Status = plan.Status
        };

        await _planRepo.AddAsync(entity);
    }

    public async Task UpdatePlanAsync(PayerPlanDTO plan)
    {
        if (!plan.PlanId.HasValue)
            throw new ArgumentException("PlanId is required for update.");
        if (string.IsNullOrWhiteSpace(plan.PlanName))
            throw new ArgumentException("PlanName is required.");

        var existing = await _planRepo.GetPayerPlanByIdAsync(plan.PlanId.Value);
        if (existing == null)
            throw new KeyNotFoundException($"Plan {plan.PlanId.Value} not found.");

        // Keep payer link consistent; allow changing payer only if provided and exists.
        if (plan.PayerId.HasValue)
        {
            if (!await _payerRepo.ExistsAsync(plan.PayerId.Value))
                throw new KeyNotFoundException($"Payer {plan.PayerId.Value} not found.");
            existing.PayerId = plan.PayerId;
        }

        existing.PlanName = plan.PlanName;
        existing.NetworkType = plan.NetworkType;
        existing.Posdefault = plan.Posdefault;
        existing.TelehealthModifiersJson = plan.TelehealthModifiersJson;
        existing.Status = plan.Status;

        await _planRepo.UpdateAsync(existing);
    }

    public async Task DeletePlanAsync(int planId)
    {
        var existing = await _planRepo.GetPayerPlanByIdAsync(planId);
        if (existing == null)
            throw new KeyNotFoundException($"Plan {planId} not found.");

        await _planRepo.DeleteAsync(existing);
    }
}

