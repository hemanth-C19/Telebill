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
    public async Task<PatientBalanceListResponseDto> GetPatientBalancesAsync(PatientBalanceFilterParams filters)
    {
        filters.PageSize = Math.Min(filters.PageSize, 100);
        var (items, total) = await _repo.GetPatientBalancesPagedAsync(filters);
        var mapped = items.Select(MapBalance).ToList();
        return new PatientBalanceListResponseDto
        {
            TotalCount = total,
            TotalAmountDue = mapped.Sum(b => b.AmountDue),
            Balances = mapped
        };
    }

    public Task<AgingSummaryDto> GetAgingSummaryAsync()
    {
        return _repo.GetAgingSummaryAsync();
    }

    public async Task<PatientBalanceDto> GetPatientBalanceByIdAsync(int balanceID)
    {
        var b = await _repo.GetPatientBalanceByIdAsync(balanceID);
        if (b == null) throw new KeyNotFoundException("balance not found");
        return MapBalance(b);
    }

    public async Task<PatientBalanceListResponseDto> GetBalancesByPatientAsync(int patientID)
    {
        var patient = await _repo.GetPatientByIdAsync(patientID);
        if (patient == null) throw new KeyNotFoundException("patient not found");

        var items = await _repo.GetPatientBalancesByPatientAsync(patientID);
        var mapped = items.Select(MapBalance).ToList();
        return new PatientBalanceListResponseDto
        {
            TotalCount = mapped.Count,
            TotalAmountDue = mapped.Sum(b => b.AmountDue),
            Balances = mapped
        };
    }

    public async Task<PatientBalanceDto> UpdatePatientBalanceStatusAsync(int balanceID, UpdatePatientBalanceStatusRequestDto dto, int currentUserID)
    {
        var b = await _repo.GetPatientBalanceByIdAsync(balanceID);
        if (b == null) throw new KeyNotFoundException("balance not found");

        if (!string.Equals(dto.Status, "Paid", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(dto.Status, "WrittenOff", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("invalid status value");
        }

        var prev = b.Status;
        b.Status = dto.Status;
        if (string.Equals(dto.Status, "Paid", StringComparison.OrdinalIgnoreCase))
        {
            b.AmountDue = 0m;
        }

        await _repo.UpdatePatientBalanceAsync(b);

        return MapBalance(b);
    }

    public async Task<AgingBatchJobResultDto> RunAgingBucketJobAsync(string? schedulerKey)
    {
        var configured = _config["SchedulerKey"];
        if (!string.IsNullOrWhiteSpace(configured) && !string.Equals(configured, schedulerKey))
        {
            throw new UnauthorizedAccessException("Invalid scheduler key");
        }

        var started = DateTime.UtcNow;
        var openBalances = await _repo.GetAllOpenBalancesAsync();
        int updated = 0;

        foreach (var b in openBalances)
        {
            if (b.ClaimId == null) continue;
            var firstPostDate = await _repo.GetFirstPostingDateForClaimAsync(b.ClaimId.Value) ?? started;
            var days = (DateTime.UtcNow - firstPostDate).Days;
            var newBucket = days <= 30 ? "0-30" :
                days <= 60 ? "31-60" :
                days <= 90 ? "61-90" : "90+";

            if (b.AgingBucket != newBucket)
            {
                b.AgingBucket = newBucket;
                updated++;
            }
        }

        await _repo.SaveAllPatientBalancesAsync(openBalances);

        var completed = DateTime.UtcNow;
        return new AgingBatchJobResultDto
        {
            JobID = Guid.NewGuid().ToString("N"),
            RunAt = completed,
            BalancesUpdated = updated,
            DurationSeconds = (completed - started).TotalSeconds
        };
    }

    public async Task<StatementDto> GenerateStatementAsync(GenerateStatementRequestDto dto, int currentUserID)
    {
        var patient = await _repo.GetPatientByIdAsync(dto.PatientID);
        if (patient == null) throw new KeyNotFoundException("patient not found");
        if (dto.PeriodEnd < dto.PeriodStart) throw new ArgumentException("PeriodEnd must be after PeriodStart");

        var balances = (await _repo.GetPatientBalancesByPatientAsync(dto.PatientID))
            .Where(b => b.Status == "Open" && (b.AmountDue ?? 0m) > 0m)
            .ToList();

        if (balances.Count == 0) throw new ArgumentException("No open balances found for this patient");

        if (await _repo.StatementExistsForPeriodAsync(dto.PatientID, dto.PeriodStart, dto.PeriodEnd))
        {
            throw new InvalidOperationException("A statement already exists for this patient and period");
        }

        var lineItems = new List<StatementLineItemDto>();
        decimal totalBilled = 0m;
        decimal totalInsurancePaid = 0m;
        decimal totalPatientDue = 0m;

        foreach (var bal in balances)
        {
            var claim = await _repo.GetClaimByIdAsync(bal.ClaimId ?? 0);
            if (claim == null) continue;
            var enc = claim.EncounterId.HasValue ? await _repo.GetEncounterByIdAsync(claim.EncounterId.Value) : null;
            var serviceDate = enc == null ? DateOnly.FromDateTime(DateTime.UtcNow) : DateOnly.FromDateTime(enc.EncounterDateTime);

            var lines = await _repo.GetActiveClaimLinesByClaimAsync(claim.ClaimId);
            var posts = (await _repo.GetPaymentPostsByClaimAsync(claim.ClaimId)).Where(p => p.Status == "Active").ToList();

            foreach (var line in lines)
            {
                var post = posts.FirstOrDefault(p => p.ClaimLineId == line.ClaimLineId);
                if (post == null) continue;
                var adjustments = DeserializeAdjustments(post.AdjustmentJson);
                var coTotal = adjustments.Where(a => a.Group.Equals("CO", StringComparison.OrdinalIgnoreCase)).Sum(a => a.Amount);
                var prTotal = adjustments.Where(a => a.Group.Equals("PR", StringComparison.OrdinalIgnoreCase)).Sum(a => a.Amount);

                lineItems.Add(new StatementLineItemDto
                {
                    ClaimID = claim.ClaimId,
                    ServiceDate = serviceDate,
                    CptHcpcs = line.CptHcpcs ?? string.Empty,
                    Billed = line.ChargeAmount ?? 0m,
                    InsurancePaid = post.AmountPaid ?? 0m,
                    Adjustment = coTotal,
                    PatientDue = prTotal
                });

                totalBilled += line.ChargeAmount ?? 0m;
                totalInsurancePaid += post.AmountPaid ?? 0m;
                totalPatientDue += prTotal;
            }
        }

        var amountDue = balances.Sum(b => b.AmountDue ?? 0m);
        var summaryObj = new { lineItems, totalBilled, totalInsurancePaid, totalPatientDue };
        var summaryJson = JsonSerializer.Serialize(summaryObj);

        var statement = new Statement
        {
            PatientId = dto.PatientID,
            PeriodStart = dto.PeriodStart,
            PeriodEnd = dto.PeriodEnd,
            GeneratedDate = DateTime.UtcNow,
            AmountDue = amountDue,
            SummaryJson = summaryJson,
            Status = "Open"
        };

        statement = await _repo.CreateStatementAsync(statement);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var bal in balances)
        {
            bal.LastStatementDate = today;
        }
        await _repo.SaveAllPatientBalancesAsync(balances);

        var frontDeskUsers = await _repo.GetUsersByRoleAsync("FrontDesk");
        foreach (var u in frontDeskUsers)
        {
            await _repo.CreateNotificationAsync(u.UserId,
                $"Statement generated for Patient #{dto.PatientID}: ${amountDue} due for period {dto.PeriodStart} to {dto.PeriodEnd}.",
                "Statement");
        }

        return MapStatement(statement, patient, lineItems);
    }

    public async Task<StatementBatchResultDto> GenerateStatementBatchAsync(GenerateStatementBatchRequestDto dto)
    {
        var configured = _config["SchedulerKey"];
        if (!string.IsNullOrWhiteSpace(configured) && !string.Equals(configured, dto.SchedulerKey))
        {
            throw new UnauthorizedAccessException("Invalid scheduler key");
        }

        var started = DateTime.UtcNow;
        int statementsGenerated = 0;
        int patientsProcessed = 0;
        decimal totalAmountBilled = 0m;

        var patientIds = await _repo.GetDistinctPatientIDsWithOpenBalancesAsync();
        foreach (var patientId in patientIds)
        {
            try
            {
                var result = await GenerateStatementAsync(new GenerateStatementRequestDto
                {
                    PatientID = patientId,
                    PeriodStart = dto.PeriodStart,
                    PeriodEnd = dto.PeriodEnd
                }, 0);

                statementsGenerated++;
                totalAmountBilled += result.AmountDue;
            }
            catch (InvalidOperationException)
            {
                // statement already exists - skip
            }

            patientsProcessed++;
        }

        var completed = DateTime.UtcNow;

        var notifyUsers = (await _repo.GetUsersByRoleAsync("AR")).Concat(await _repo.GetUsersByRoleAsync("FrontDesk")).DistinctBy(u => u.UserId).ToList();
        foreach (var u in notifyUsers)
        {
            await _repo.CreateNotificationAsync(u.UserId,
                $"Statement batch complete: {statementsGenerated} statements generated for {patientsProcessed} patients. Total: ${totalAmountBilled}",
                "Statement");
        }

        return new StatementBatchResultDto
        {
            JobID = Guid.NewGuid().ToString("N"),
            StartedAt = started,
            CompletedAt = completed,
            StatementsGenerated = statementsGenerated,
            PatientsProcessed = patientsProcessed,
            TotalAmountBilled = totalAmountBilled,
            DurationSeconds = (completed - started).TotalSeconds
        };
    }

    public async Task<StatementListResponseDto> GetStatementsAsync(int? patientID, string? status, DateOnly? dateFrom, DateOnly? dateTo, int page, int pageSize)
    {
        pageSize = Math.Min(pageSize, 100);
        var (items, total) = await _repo.GetStatementsPagedAsync(patientID, status, dateFrom, dateTo, page, pageSize);
        return new StatementListResponseDto
        {
            TotalCount = total,
            Statements = items.Select(s =>
            {
                var patient = s.Patient;
                var lineItems = DeserializeStatementLineItems(s.SummaryJson);
                return MapStatement(s, patient, lineItems);
            }).ToList()
        };
    }

    public async Task<StatementDto> GetStatementByIdAsync(int statementID)
    {
        var s = await _repo.GetStatementByIdAsync(statementID);
        if (s == null) throw new KeyNotFoundException("statement not found");
        var lineItems = DeserializeStatementLineItems(s.SummaryJson);
        return MapStatement(s, s.Patient, lineItems);
    }

    public async Task<StatementDto> UpdateStatementStatusAsync(int statementID, UpdateStatementStatusRequestDto dto, int currentUserID)
    {
        var s = await _repo.GetStatementByIdAsync(statementID);
        if (s == null) throw new KeyNotFoundException("statement not found");
        s.Status = dto.Status;
        await _repo.UpdateStatementAsync(s);
        return await GetStatementByIdAsync(statementID);
    }

    private static List<StatementLineItemDto> DeserializeStatementLineItems(string? summaryJson)
    {
        if (string.IsNullOrWhiteSpace(summaryJson)) return new List<StatementLineItemDto>();
        try
        {
            using var doc = JsonDocument.Parse(summaryJson);
            if (!doc.RootElement.TryGetProperty("lineItems", out var items) || items.ValueKind != JsonValueKind.Array)
            {
                return new List<StatementLineItemDto>();
            }

            var list = new List<StatementLineItemDto>();
            foreach (var item in items.EnumerateArray())
            {
                list.Add(new StatementLineItemDto
                {
                    ClaimID = item.GetProperty("ClaimID").GetInt32(),
                    ServiceDate = DateOnly.Parse(item.GetProperty("ServiceDate").GetString() ?? DateTime.UtcNow.ToString("yyyy-MM-dd")),
                    CptHcpcs = item.GetProperty("CptHcpcs").GetString() ?? string.Empty,
                    Billed = item.GetProperty("Billed").GetDecimal(),
                    InsurancePaid = item.GetProperty("InsurancePaid").GetDecimal(),
                    Adjustment = item.GetProperty("Adjustment").GetDecimal(),
                    PatientDue = item.GetProperty("PatientDue").GetDecimal()
                });
            }
            return list;
        }
        catch
        {
        }

        return new List<StatementLineItemDto>();
    }

    private static PatientBalanceDto MapBalance(PatientBalance b)
    {
        return new PatientBalanceDto
        {
            BalanceID = b.BalanceId,
            PatientID = b.PatientId ?? 0,
            PatientName = b.Patient?.Name ?? string.Empty,
            MRN = b.Patient?.Mrn ?? string.Empty,
            ClaimID = b.ClaimId ?? 0,
            AmountDue = b.AmountDue ?? 0m,
            AgingBucket = b.AgingBucket ?? string.Empty,
            LastStatementDate = b.LastStatementDate,
            Status = b.Status ?? string.Empty
        };
    }

    private static StatementDto MapStatement(Statement s, Patient? patient, List<StatementLineItemDto> items)
    {
        return new StatementDto
        {
            StatementID = s.StatementId,
            PatientID = s.PatientId ?? 0,
            PatientName = patient?.Name ?? string.Empty,
            MRN = patient?.Mrn ?? string.Empty,
            PeriodStart = s.PeriodStart ?? DateOnly.FromDateTime(DateTime.UtcNow),
            PeriodEnd = s.PeriodEnd ?? DateOnly.FromDateTime(DateTime.UtcNow),
            GeneratedDate = s.GeneratedDate ?? DateTime.MinValue,
            AmountDue = s.AmountDue ?? 0m,
            LineItems = items,
            Status = s.Status ?? string.Empty
        };
    }
}

