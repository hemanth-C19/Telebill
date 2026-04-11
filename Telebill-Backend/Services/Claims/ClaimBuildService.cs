using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Telebill.Dto;
using Telebill.Models;
using Telebill.Repositories.Claims;

namespace Services;

public class ClaimBuildService(IClaimRepository repo, IClaimScrubService scrubService) : IClaimBuildService
{
    public async Task<BuildClaimResponseDto> BuildClaimAsync(BuildClaimRequestDto dto)
    {
        var encounter = await repo.GetEncounterByIdAsync(dto.EncounterID);
        if (encounter == null)
        {
            throw new ArgumentException("Encounter not found");
        }

        if (!string.Equals(encounter.Status, "Finalized", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Encounter is not Finalized");
        }

        var codingLock = await repo.GetActiveCodingLockAsync(dto.EncounterID);
        if (codingLock == null)
        {
            throw new ArgumentException("No active coding lock found");
        }

        if (await repo.ExistsForEncounterAsync(dto.EncounterID))
        {
            throw new InvalidOperationException("Claim already exists for this encounter");
        }

        var chargeLines = await repo.GetFinalizedChargeLinesByEncounterAsync(dto.EncounterID);
        if (chargeLines.Count == 0)
        {
            throw new ArgumentException("No finalized charge lines found");
        }

        var diagnoses = await repo.GetActiveDiagnosesByEncounterAsync(dto.EncounterID);

        Coverage? coverage = null;
        PayerPlan? plan = null;
        if (encounter.PatientId.HasValue)
        {
            coverage = await repo.GetActiveCoverageForEncounterAsync(
                encounter.PatientId.Value,
                encounter.EncounterDateTime);

            if (coverage != null && coverage.PlanId.HasValue)
            {
                plan = await repo.GetPayerPlanByIdAsync(coverage.PlanId.Value);
            }
        }

        var claim = new Claim
        {
            EncounterId = dto.EncounterID,
            PatientId = encounter.PatientId,
            PlanId = coverage?.PlanId,
            SubscriberRel = "Self",
            TotalCharge = chargeLines.Sum(c => c.ChargeAmount ?? 0m),
            ClaimStatus = "Draft",
            CreatedDate = DateTime.UtcNow
        };

        claim = await repo.CreateAsync(claim);

        var claimLines = new List<ClaimLine>();
        var lineNo = 1;

        foreach (var cl in chargeLines.OrderBy(c => c.ChargeId))
        {
            FeeSchedule? fee = null;
            if (coverage?.PlanId != null)
            {
                fee = await repo.GetFeeScheduleAsync(
                    coverage.PlanId.Value,
                    cl.CptHcpcs ?? string.Empty,
                    cl.Modifiers,
                    encounter.EncounterDateTime);
            }

            var dxPointers = new List<int> { 1 };
            var dxPointersJson = JsonSerializer.Serialize(dxPointers);

            claimLines.Add(new ClaimLine
            {
                ClaimId = claim.ClaimId,
                LineNo = lineNo++,
                CptHcpcs = cl.CptHcpcs,
                Modifiers = cl.Modifiers,
                Units = cl.Units,
                ChargeAmount = cl.ChargeAmount,
                DxPointers = dxPointersJson,
                Pos = encounter.Pos,
                LineStatus = "Active"
            });
        }

        await repo.CreateLinesAsync(claimLines);

        var scrubResult = await scrubService.ScrubClaimAsync(claim.ClaimId);

        var response = new BuildClaimResponseDto
        {
            ClaimID = claim.ClaimId,
            EncounterID = dto.EncounterID,
            ClaimStatus = scrubResult?.ClaimStatus ?? claim.ClaimStatus ?? string.Empty,
            TotalCharge = claim.TotalCharge ?? 0m,
            ScrubTriggered = scrubResult != null
        };

        var lines = await repo.GetLinesByClaimIDAsync(claim.ClaimId);
        foreach (var line in lines)
        {
            var modifiers = ClaimJsonHelper.SafeDeserializeStringList(line.Modifiers);
            var dxPointers = ClaimJsonHelper.SafeDeserializeIntList(line.DxPointers);

            response.ClaimLines.Add(new ClaimLineDto
            {
                ClaimLineID = line.ClaimLineId,
                LineNo = line.LineNo ?? 0,
                CPT_HCPCS = line.CptHcpcs ?? string.Empty,
                Modifiers = modifiers,
                Units = line.Units ?? 0,
                ChargeAmount = line.ChargeAmount ?? 0m,
                DxPointers = dxPointers,
                POS = line.Pos ?? string.Empty,
                LineStatus = line.LineStatus ?? string.Empty
            });
        }

        return response;
    }

    public async Task TriggerClaimBuildFromEncounterAsync(int encounterID)
    {
        var dto = new BuildClaimRequestDto { EncounterID = encounterID };
        await BuildClaimAsync(dto);
    }
}

