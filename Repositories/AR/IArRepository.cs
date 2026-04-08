using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.AR;
using Telebill.Models;

namespace Telebill.Repositories.AR;

public interface IArRepository
{
    // ── DENIAL ───────────────────────────────────────────────────
    Task<List<Denial>> GetDenialsAsync(ArWorklistFilterParams filters);
    Task<Denial?> GetDenialByIdAsync(int denialId);
    Task<List<Denial>> GetDenialsByClaimIdAsync(int claimId);
    Task UpdateDenialAsync(Denial denial);
    Task<Denial> AddDenialAsync(Denial denial);

    // ── CLAIM ─────────────────────────────────────────────────────
    Task<Claim?> GetClaimByIdAsync(int claimId);
    Task<List<ClaimLine>> GetClaimLinesByClaimIdAsync(int claimId);
    Task UpdateClaimStatusAsync(int claimId, string newStatus);

    // ── PAYMENT POST ─────────────────────────────────────────────
    Task<List<PaymentPost>> GetPaymentPostsByClaimIdAsync(int claimId);

    // ── SUBMISSION HISTORY ────────────────────────────────────────
    Task<List<SubmissionRef>> GetSubmissionRefsByClaimIdAsync(int claimId);

    // ── ATTACHMENT ────────────────────────────────────────────────
    Task<List<AttachmentRef>> GetAttachmentsByClaimIdAsync(int claimId);
    Task<AttachmentRef> AddAttachmentAsync(AttachmentRef attachment);

    // ── FEE SCHEDULE ──────────────────────────────────────────────
    Task<FeeSchedule?> GetFeeScheduleAsync(int planId, string cptHcpcs,
                                           string? modifierCombo, DateOnly serviceDate);

    // ── CROSS-MODULE READS ────────────────────────────────────────
    Task<Encounter?> GetEncounterByClaimIdAsync(int claimId);
    Task<Patient?> GetPatientByIdAsync(int patientId);
    Task<Payer?> GetPayerByPlanIdAsync(int planId);
    Task<PayerPlan?> GetPayerPlanByIdAsync(int planId);
    Task<List<Claim>> GetPartiallyPaidClaimsAsync();
    Task<List<User>> GetUsersByRoleAsync(string role);

    // ── DASHBOARD ─────────────────────────────────────────────────
    Task<List<Denial>> GetAllOpenDenialsAsync();
    Task<int> GetTotalClaimsSubmittedByPayerAsync(int payerId);

    // ── SAVE ──────────────────────────────────────────────────────
    Task SaveChangesAsync();
}

