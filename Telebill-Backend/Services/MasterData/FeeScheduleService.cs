using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.MasterData;
using Telebill.Models;
using Telebill.Repositories.MasterData;

namespace Telebill.Services.MasterData;

public class FeeScheduleService : IFeeScheduleService
{
    private readonly IFeeScheduleRepository _feeRepo;

    public FeeScheduleService(IFeeScheduleRepository feeRepo)
    {
        _feeRepo = feeRepo;
    }

    public async Task<IEnumerable<FeeSchedule>> GetFeesByPlanIdAsync(int planId)
    {
        if (!await _feeRepo.PlanExistsAsync(planId))
            throw new KeyNotFoundException($"Plan {planId} not found.");

        return await _feeRepo.GetByPlanIdAsync(planId);
    }

    public async Task AddFeeAsync(AddFeeDTO fee)
    {
        if (!fee.PlanId.HasValue)
            throw new ArgumentException("PlanId is required.");
        if (string.IsNullOrWhiteSpace(fee.CptHcpcs))
            throw new ArgumentException("CptHcpcs is required.");

        if (!await _feeRepo.PlanExistsAsync(fee.PlanId.Value))
            throw new KeyNotFoundException($"Plan {fee.PlanId.Value} not found.");

        var entity = new FeeSchedule
        {
            PlanId = fee.PlanId,
            CptHcpcs = fee.CptHcpcs,
            ModifierCombo = fee.ModifierCombo,
            AllowedAmount = fee.AllowedAmount,
            EffectiveFrom = fee.EffectiveFrom,
            EffectiveTo = fee.EffectiveTo,
            Status = fee.Status
        };

        await _feeRepo.AddAsync(entity);
    }

    public async Task UpdateFeeAsync(UpdateFeeDTO fee)
    {
        if (!fee.FeeId.HasValue)
            throw new ArgumentException("FeeId is required for update.");
        if (string.IsNullOrWhiteSpace(fee.CptHcpcs))
            throw new ArgumentException("CptHcpcs is required.");

        var existing = await _feeRepo.GetFeeByIdAsync(fee.FeeId.Value);
        if (existing == null)
            throw new KeyNotFoundException($"Fee {fee.FeeId.Value} not found.");

        if (fee.PlanId.HasValue)
        {
            if (!await _feeRepo.PlanExistsAsync(fee.PlanId.Value))
                throw new KeyNotFoundException($"Plan {fee.PlanId.Value} not found.");
            existing.PlanId = fee.PlanId;
        }

        existing.CptHcpcs = fee.CptHcpcs;
        existing.ModifierCombo = fee.ModifierCombo;
        existing.AllowedAmount = fee.AllowedAmount;
        existing.EffectiveFrom = fee.EffectiveFrom;
        existing.EffectiveTo = fee.EffectiveTo;
        existing.Status = fee.Status;

        await _feeRepo.UpdateAsync(existing);
    }

    public async Task DeleteFeeAsync(int feeId)
    {
        var existing = await _feeRepo.GetFeeByIdAsync(feeId);
        if (existing == null)
            throw new KeyNotFoundException($"Fee {feeId} not found.");

        await _feeRepo.DeleteAsync(existing);
    }
}

