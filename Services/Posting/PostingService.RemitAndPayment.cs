using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Telebill.Dto.Posting;
using Telebill.Models;

namespace Telebill.Services.Posting;

public partial class PostingService
{
    public async Task<RemitRefDto> CreateRemitRefAsync(CreateRemitRefRequestDto dto, int currentUserID)
    {
        if (!await repo.PayerExistsAsync(dto.PayerID))
        {
            throw new ArgumentException("PayerID does not exist");
        }

        if (dto.BatchID.HasValue && !await repo.BatchExistsAsync(dto.BatchID.Value))
        {
            throw new ArgumentException("BatchID does not exist");
        }

        if (string.IsNullOrWhiteSpace(dto.PayloadUri))
        {
            throw new ArgumentException("PayloadUri is required");
        }

        var entity = new RemitRef
        {
            PayerId = dto.PayerID,
            BatchId = dto.BatchID,
            PayloadUri = dto.PayloadUri,
            ReceivedDate = dto.ReceivedDate.ToDateTime(TimeOnly.MinValue),
            Status = "Loaded"
        };

        entity = await repo.CreateRemitRefAsync(entity);

        await repo.WriteAuditLogAsync(currentUserID, "CREATE_REMIT_REF", $"RemitRef:{entity.RemitId}",
            JsonSerializer.Serialize(new { payerID = dto.PayerID, batchID = dto.BatchID, receivedDate = dto.ReceivedDate }));

        var payer = await repo.GetPayerByIdAsync(dto.PayerID);

        return new RemitRefDto
        {
            RemitID = entity.RemitId,
            PayerID = entity.PayerId ?? dto.PayerID,
            PayerName = payer?.Name ?? string.Empty,
            BatchID = entity.BatchId,
            PayloadUri = entity.PayloadUri ?? string.Empty,
            ReceivedDate = dto.ReceivedDate,
            Status = entity.Status ?? "Loaded"
        };
    }

    public async Task<RemitRefListResponseDto> GetRemitRefsAsync(int? payerID, string? status, DateOnly? dateFrom, DateOnly? dateTo, int page, int pageSize)
    {
        pageSize = Math.Min(pageSize, 100);
        var (items, total) = await repo.GetRemitRefsPagedAsync(payerID, status, dateFrom, dateTo, page, pageSize);
        return new RemitRefListResponseDto
        {
            TotalCount = total,
            RemitRefs = items.Select(r => new RemitRefDto
            {
                RemitID = r.RemitId,
                PayerID = r.PayerId ?? 0,
                PayerName = r.Payer?.Name ?? string.Empty,
                BatchID = r.BatchId,
                PayloadUri = r.PayloadUri ?? string.Empty,
                ReceivedDate = DateOnly.FromDateTime(r.ReceivedDate ?? DateTime.UtcNow),
                Status = r.Status ?? string.Empty
            }).ToList()
        };
    }

    public async Task<RemitRefDto> GetRemitRefByIdAsync(int remitID)
    {
        var r = await repo.GetRemitRefByIdAsync(remitID);
        if (r == null) throw new KeyNotFoundException("Remit not found");

        return new RemitRefDto
        {
            RemitID = r.RemitId,
            PayerID = r.PayerId ?? 0,
            PayerName = r.Payer?.Name ?? string.Empty,
            BatchID = r.BatchId,
            PayloadUri = r.PayloadUri ?? string.Empty,
            ReceivedDate = DateOnly.FromDateTime(r.ReceivedDate ?? DateTime.UtcNow),
            Status = r.Status ?? string.Empty
        };
    }

    public async Task<RemitRefDto> UpdateRemitRefStatusAsync(int remitID, UpdateRemitRefStatusRequestDto dto, int currentUserID)
    {
        var r = await repo.GetRemitRefByIdAsync(remitID);
        if (r == null) throw new KeyNotFoundException("Remit not found");

        if (!IsValidRemitStatus(dto.Status))
        {
            throw new ArgumentException("invalid status value");
        }

        r.Status = dto.Status;
        await repo.UpdateRemitRefAsync(r);

        await repo.WriteAuditLogAsync(currentUserID, "UPDATE_REMIT_STATUS", $"RemitRef:{remitID}",
            JsonSerializer.Serialize(new { newStatus = dto.Status }));

        return await GetRemitRefByIdAsync(remitID);
    }

    public async Task<PostingResultDto> CreatePaymentPostAsync(CreatePaymentPostRequestDto dto, int currentUserID)
    {
        var claim = await repo.GetClaimByIdAsync(dto.ClaimID);
        if (claim == null) throw new KeyNotFoundException("Claim not found");

        var oldStatus = claim.ClaimStatus ?? string.Empty;
        if (!string.Equals(oldStatus, "Accepted", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(oldStatus, "PartiallyPaid", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Payments can only be posted on Accepted or PartiallyPaid claims");
        }

        if (dto.AmountPaid < 0m) throw new ArgumentException("AmountPaid cannot be negative");

        decimal referenceAmount;
        ClaimLine? claimLine = null;
        if (dto.ClaimLineID.HasValue)
        {
            claimLine = await repo.GetClaimLineByIdAsync(dto.ClaimLineID.Value);
            if (claimLine == null) throw new KeyNotFoundException("Claim line not found");
            if (claimLine.ClaimId != dto.ClaimID) throw new ArgumentException("ClaimLine does not belong to this claim");
            referenceAmount = claimLine.ChargeAmount ?? 0m;
        }
        else
        {
            referenceAmount = claim.TotalCharge ?? 0m;
        }

        var totalAdjustments = dto.Adjustments.Sum(a => a.Amount);
        var difference = Math.Abs(referenceAmount - (dto.AmountPaid + totalAdjustments));
        if (difference > 0.01m)
        {
            throw new ArgumentException($"Financial equation does not balance. ChargeAmount ({referenceAmount}) must equal AmountPaid ({dto.AmountPaid}) + Adjustments ({totalAdjustments}). Difference: {difference}");
        }

        if (await repo.ActivePostExistsForLineAsync(dto.ClaimID, dto.ClaimLineID))
        {
            throw new InvalidOperationException("An active payment post already exists for this claim line. Void it before reposting.");
        }

        var adjustmentJson = JsonSerializer.Serialize(dto.Adjustments);
        var post = new PaymentPost
        {
            ClaimId = dto.ClaimID,
            ClaimLineId = dto.ClaimLineID,
            AmountPaid = dto.AmountPaid,
            AdjustmentJson = adjustmentJson,
            PostedDate = DateTime.UtcNow,
            PostedBy = currentUserID,
            Status = "Active"
        };

        post = await repo.CreatePaymentPostAsync(post);

        var (newClaimStatus, totalPaid, totalCharge, denialCreated) = await RecalculateClaimStatusAsync(dto.ClaimID);
        var balance = await RecalculatePatientBalanceAsync(dto.ClaimID);

        await repo.WriteAuditLogAsync(currentUserID, "POST_PAYMENT", $"PaymentPost:{post.PaymentId}",
            JsonSerializer.Serialize(new { claimID = dto.ClaimID, claimLineID = dto.ClaimLineID, amountPaid = dto.AmountPaid, totalAdjustments }));

        var createdPostDto = await MapPaymentPostDto(post);

        return new PostingResultDto
        {
            ClaimID = dto.ClaimID,
            PreviousClaimStatus = oldStatus,
            NewClaimStatus = newClaimStatus,
            TotalPaid = totalPaid,
            TotalCharge = totalCharge,
            TotalPatientResponsibility = balance.AmountDue,
            PatientBalance = balance,
            DenialCreated = denialCreated,
            CreatedPost = createdPostDto
        };
    }

    public async Task<ClaimPaymentSummaryDto> GetPaymentPostsByClaimAsync(int claimID)
    {
        var claim = await repo.GetClaimByIdAsync(claimID);
        if (claim == null) throw new KeyNotFoundException("claim not found");

        var posts = await repo.GetPaymentPostsByClaimAsync(claimID);
        var mapped = new List<PaymentPostDto>();

        decimal totalPaid = 0m;
        decimal totalCo = 0m;
        decimal totalPr = 0m;

        foreach (var p in posts)
        {
            var dto = await MapPaymentPostDto(p);
            mapped.Add(dto);

            if (string.Equals(p.Status, "Active", StringComparison.OrdinalIgnoreCase))
            {
                totalPaid += p.AmountPaid ?? 0m;
                totalCo += dto.Adjustments.Where(a => a.Group.Equals("CO", StringComparison.OrdinalIgnoreCase)).Sum(a => a.Amount);
                totalPr += dto.Adjustments.Where(a => a.Group.Equals("PR", StringComparison.OrdinalIgnoreCase)).Sum(a => a.Amount);
            }
        }

        return new ClaimPaymentSummaryDto
        {
            ClaimID = claimID,
            ClaimStatus = claim.ClaimStatus ?? string.Empty,
            TotalCharge = claim.TotalCharge ?? 0m,
            TotalPaid = totalPaid,
            TotalContractualAdjustment = totalCo,
            TotalPatientResponsibility = totalPr,
            PaymentPosts = mapped
        };
    }

    public async Task<PaymentPostDto> GetPaymentPostByIdAsync(int paymentID)
    {
        var post = await repo.GetPaymentPostByIdAsync(paymentID);
        if (post == null) throw new KeyNotFoundException("payment not found");
        return await MapPaymentPostDto(post);
    }

    public async Task<PostingResultDto> VoidPaymentPostAsync(int paymentID, VoidPaymentPostRequestDto dto, int currentUserID)
    {
        var post = await repo.GetPaymentPostByIdAsync(paymentID);
        if (post == null) throw new KeyNotFoundException("payment not found");

        if (string.Equals(post.Status, "Voided", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Payment post is already voided");
        }

        post.Status = "Voided";
        await repo.UpdatePaymentPostAsync(post);

        var claim = await repo.GetClaimByIdAsync(post.ClaimId ?? 0);
        var oldStatus = claim?.ClaimStatus ?? string.Empty;

        var (newClaimStatus, totalPaid, totalCharge, denialCreated) = await RecalculateClaimStatusAsync(post.ClaimId ?? 0);
        var balance = await RecalculatePatientBalanceAsync(post.ClaimId ?? 0);

        await repo.WriteAuditLogAsync(currentUserID, "VOID_PAYMENT", $"PaymentPost:{paymentID}",
            JsonSerializer.Serialize(new { claimID = post.ClaimId, claimLineID = post.ClaimLineId, amountPaid = post.AmountPaid ?? 0m, reason = dto.Reason }));

        return new PostingResultDto
        {
            ClaimID = post.ClaimId ?? 0,
            PreviousClaimStatus = oldStatus,
            NewClaimStatus = newClaimStatus,
            TotalPaid = totalPaid,
            TotalCharge = totalCharge,
            TotalPatientResponsibility = balance.AmountDue,
            PatientBalance = balance,
            DenialCreated = denialCreated,
            CreatedPost = await MapPaymentPostDto(post)
        };
    }

    private async Task<(string newStatus, decimal totalPaid, decimal totalCharge, bool denialCreated)> RecalculateClaimStatusAsync(int claimID)
    {
        var claim = await repo.GetClaimByIdAsync(claimID);
        if (claim == null) throw new KeyNotFoundException("Claim not found");

        var lines = await repo.GetActiveClaimLinesByClaimAsync(claimID);
        var posts = (await repo.GetPaymentPostsByClaimAsync(claimID)).Where(p => p.Status == "Active").ToList();

        var totalPaid = posts.Sum(p => p.AmountPaid ?? 0m);
        var totalCharge = lines.Sum(l => l.ChargeAmount ?? 0m);

        bool hasDenialCo = posts.Any(p => DeserializeAdjustments(p.AdjustmentJson)
            .Any(a => a.Group.Equals("CO", StringComparison.OrdinalIgnoreCase) && DenialCarcCodes.Contains(a.Carc)));

        bool isDenial = totalPaid == 0m && hasDenialCo;
        var newStatus = isDenial ? "Denied" :
            totalPaid >= totalCharge && totalPaid > 0m ? "Paid" :
            totalPaid > 0m ? "PartiallyPaid" : "PartiallyPaid";

        await repo.UpdateClaimStatusAsync(claimID, newStatus);

        bool denialCreated = false;
        if (isDenial)
        {
            denialCreated = await CreateDenialRecordsAsync(claimID, lines, posts);
        }

        return (newStatus, totalPaid, totalCharge, denialCreated);
    }

    private async Task<PatientBalanceDto> RecalculatePatientBalanceAsync(int claimID)
    {
        var claim = await repo.GetClaimByIdAsync(claimID);
        if (claim == null) throw new KeyNotFoundException("Claim not found");

        var posts = (await repo.GetPaymentPostsByClaimAsync(claimID)).Where(p => p.Status == "Active").ToList();
        decimal totalPr = 0m;
        foreach (var p in posts)
        {
            var adjustments = DeserializeAdjustments(p.AdjustmentJson);
            totalPr += adjustments.Where(a => a.Group.Equals("PR", StringComparison.OrdinalIgnoreCase)).Sum(a => a.Amount);
        }

        var balance = await repo.GetPatientBalanceByClaimAsync(claimID);
        if (balance != null)
        {
            balance.AmountDue = totalPr;
            balance.Status = totalPr == 0m ? "Paid" : "Open";
            await repo.UpdatePatientBalanceAsync(balance);
        }
        else if (totalPr > 0m)
        {
            balance = new PatientBalance
            {
                PatientId = claim.PatientId,
                ClaimId = claimID,
                AmountDue = totalPr,
                AgingBucket = "0-30",
                LastStatementDate = null,
                Status = "Open"
            };
            balance = await repo.CreatePatientBalanceAsync(balance);
        }
        else
        {
            balance = new PatientBalance
            {
                PatientId = claim.PatientId,
                ClaimId = claimID,
                AmountDue = 0m,
                AgingBucket = "0-30",
                Status = "Paid"
            };
        }

        var patient = claim.PatientId.HasValue ? await repo.GetPatientByIdAsync(claim.PatientId.Value) : null;
        balance.Patient = patient;

        return MapBalance(balance);
    }

    private async Task<bool> CreateDenialRecordsAsync(int claimID, List<ClaimLine> lines, List<PaymentPost> posts)
    {
        var claim = await repo.GetClaimByIdAsync(claimID);
        if (claim == null) throw new KeyNotFoundException("Claim not found");

        var reasonCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int denialCount = 0;

        foreach (var post in posts)
        {
            var adjustments = DeserializeAdjustments(post.AdjustmentJson);
            foreach (var adj in adjustments.Where(a => a.Group.Equals("CO", StringComparison.OrdinalIgnoreCase)))
            {
                var amountDenied = 0m;
                if (post.ClaimLineId.HasValue)
                {
                    var line = lines.FirstOrDefault(l => l.ClaimLineId == post.ClaimLineId.Value);
                    amountDenied = line?.ChargeAmount ?? 0m;
                }
                else
                {
                    amountDenied = claim.TotalCharge ?? 0m;
                }

                await repo.CreateDenialAsync(new Denial
                {
                    ClaimId = claimID,
                    ClaimLineId = post.ClaimLineId,
                    ReasonCode = adj.Carc,
                    RemarkCode = null,
                    DenialDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    AmountDenied = amountDenied,
                    Status = "Open"
                });

                reasonCodes.Add(adj.Carc);
                denialCount++;
            }
        }

        if (denialCount > 0)
        {
            var arUsers = await repo.GetUsersByRoleAsync("AR");
            foreach (var u in arUsers)
            {
                await repo.CreateNotificationAsync(u.UserId,
                    $"Claim #{claimID} has been denied (CARC {string.Join(",", reasonCodes)}). Please review and file an appeal if applicable.",
                    "Denial");
            }

            await repo.WriteAuditLogAsync(0, "CREATE_DENIAL_FROM_POSTING", $"Claim:{claimID}",
                JsonSerializer.Serialize(new { denialCount, reasonCodes = reasonCodes.ToList() }));
        }

        return denialCount > 0;
    }

    private static bool IsValidRemitStatus(string status)
    {
        return status == "Loaded" || status == "Posted" || status == "Failed";
    }

    private static List<AdjustmentEntryDto> DeserializeAdjustments(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<AdjustmentEntryDto>();
        try
        {
            return JsonSerializer.Deserialize<List<AdjustmentEntryDto>>(json) ?? new List<AdjustmentEntryDto>();
        }
        catch
        {
            return new List<AdjustmentEntryDto>();
        }
    }

    private async Task<PaymentPostDto> MapPaymentPostDto(PaymentPost post)
    {
        var adjustments = DeserializeAdjustments(post.AdjustmentJson);
        var totalAdjusted = adjustments.Sum(a => a.Amount);
        var pr = adjustments.Where(a => a.Group.Equals("PR", StringComparison.OrdinalIgnoreCase)).Sum(a => a.Amount);

        int? lineNo = null;
        string? cpt = null;
        decimal chargeAmt = 0m;
        if (post.ClaimLineId.HasValue)
        {
            var line = await repo.GetClaimLineByIdAsync(post.ClaimLineId.Value);
            lineNo = line?.LineNo;
            cpt = line?.CptHcpcs;
            chargeAmt = line?.ChargeAmount ?? 0m;
        }
        else
        {
            var claim = await repo.GetClaimByIdAsync(post.ClaimId ?? 0);
            chargeAmt = claim?.TotalCharge ?? 0m;
        }

        var user = post.PostedBy.HasValue ? (await repo.GetUsersByRoleAsync("AR")).FirstOrDefault(u => u.UserId == post.PostedBy.Value) : null;

        return new PaymentPostDto
        {
            PaymentID = post.PaymentId,
            ClaimID = post.ClaimId ?? 0,
            ClaimLineID = post.ClaimLineId,
            LineNo = lineNo,
            CptHcpcs = cpt,
            ChargeAmount = chargeAmt,
            AmountPaid = post.AmountPaid ?? 0m,
            Adjustments = adjustments,
            TotalAdjusted = totalAdjusted,
            PatientResponsibility = pr,
            PostedDate = post.PostedDate ?? DateTime.MinValue,
            PostedBy = post.PostedBy ?? 0,
            PostedByName = post.PostedByNavigation?.Name ?? user?.Name ?? string.Empty,
            Status = post.Status ?? string.Empty
        };
    }
}

