using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Telebill.Dto.AR;
using Telebill.Models;
using Telebill.Repositories.AR;

namespace Telebill.Services.AR;

public class UnderpaymentService : IUnderpaymentService
{
    private readonly IArRepository _repo;

    public UnderpaymentService(IArRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<UnderpaymentItemDto>> GetUnderpaymentWorklistAsync()
    {
        var partialClaims = await _repo.GetPartiallyPaidClaimsAsync();
        var result = new List<UnderpaymentItemDto>();

        foreach (var claim in partialClaims)
        {
            if (!claim.ClaimId.Equals(claim.ClaimId))
            {
                continue;
            }

            var enc = await _repo.GetEncounterByClaimIdAsync(claim.ClaimId);
            var patient = claim.PatientId.HasValue
                ? await _repo.GetPatientByIdAsync(claim.PatientId.Value)
                : null;

            PayerPlan? plan = claim.PlanId.HasValue
                ? await _repo.GetPayerPlanByIdAsync(claim.PlanId.Value)
                : null;

            Payer? payer = plan != null
                ? await _repo.GetPayerByPlanIdAsync(plan.PlanId)
                : null;

            var lines = await _repo.GetClaimLinesByClaimIdAsync(claim.ClaimId);
            var payments = await _repo.GetPaymentPostsByClaimIdAsync(claim.ClaimId);

            var lineItems = new List<LineUnderpaymentDto>();
            decimal totalAllowed = 0m;
            decimal totalPaid = 0m;

            foreach (var line in lines)
            {
                var serviceDate = enc != null
                    ? DateOnly.FromDateTime(enc.EncounterDateTime)
                    : DateOnly.FromDateTime(DateTime.Today);

                var modList = new List<string>();
                if (!string.IsNullOrWhiteSpace(line.Modifiers))
                {
                    try
                    {
                        modList = JsonSerializer.Deserialize<List<string>>(line.Modifiers!) ?? new List<string>();
                    }
                    catch
                    {
                    }
                }

                var modCombo = modList.FirstOrDefault();

                FeeSchedule? fee = null;
                if (claim.PlanId.HasValue)
                {
                    fee = await _repo.GetFeeScheduleAsync(claim.PlanId.Value, line.CptHcpcs ?? string.Empty, modCombo, serviceDate);
                }

                var linePayment = payments
                    .Where(p => p.ClaimLineId == line.ClaimLineId)
                    .Sum(p => p.AmountPaid ?? 0m);

                decimal? variance = fee?.AllowedAmount != null
                    ? fee.AllowedAmount - linePayment
                    : null;

                totalAllowed += fee?.AllowedAmount ?? 0m;
                totalPaid += linePayment;

                lineItems.Add(new LineUnderpaymentDto
                {
                    ClaimLineId = line.ClaimLineId,
                    LineNo = line.LineNo ?? 0,
                    CptHcpcs = line.CptHcpcs,
                    Modifiers = line.Modifiers,
                    ChargeAmount = line.ChargeAmount ?? 0m,
                    AmountPaid = linePayment,
                    AllowedAmount = fee?.AllowedAmount,
                    Variance = variance,
                    IsPotentialUnderpayment = variance.HasValue && variance.Value > 0.01m
                });
            }

            var underpaymentAmt = totalAllowed - totalPaid;
            if (underpaymentAmt > 0.01m)
            {
                result.Add(new UnderpaymentItemDto
                {
                    ClaimId = claim.ClaimId,
                    PatientName = patient?.Name,
                    PayerName = payer?.Name,
                    PlanName = plan?.PlanName,
                    EncounterDateTime = enc?.EncounterDateTime ?? DateTime.MinValue,
                    TotalCharge = claim.TotalCharge ?? 0m,
                    TotalPaid = totalPaid,
                    TotalAllowed = totalAllowed,
                    UnderpaymentAmount = underpaymentAmt,
                    ClaimStatus = claim.ClaimStatus,
                    Lines = lineItems
                });
            }
        }

        return result
            .OrderByDescending(x => x.UnderpaymentAmount)
            .ToList();
    }

    public async Task<(bool success, string error)> FlagUnderpaymentAsync(FlagUnderpaymentDto dto, int userId)
    {
        var claim = await _repo.GetClaimByIdAsync(dto.ClaimId);
        if (claim == null)
        {
            return (false, "Claim not found");
        }

        if (!string.Equals(claim.ClaimStatus, "PartiallyPaid", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Only PartiallyPaid claims can be flagged as underpayments");
        }

        var existing = await _repo.GetDenialsByClaimIdAsync(dto.ClaimId);
        if (existing.Any(d => d.ReasonCode == "UNDERPAYMENT" && d.Status == "Open"))
        {
            return (false, "An underpayment dispute already exists for this claim");
        }

        var denial = new Denial
        {
            ClaimId = dto.ClaimId,
            ClaimLineId = null,
            ReasonCode = "UNDERPAYMENT",
            RemarkCode = null,
            DenialDate = DateOnly.FromDateTime(DateTime.Today),
            AmountDenied = 0m,
            Status = "Open"
        };

        await _repo.AddDenialAsync(denial);

        return (true, string.Empty);
    }
}
