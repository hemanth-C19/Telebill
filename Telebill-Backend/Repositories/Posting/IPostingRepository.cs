using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.Posting;
using Telebill.Models;

namespace Telebill.Repositories.Posting;

public interface IPostingRepository
{
    // RemitRef
    Task<RemitRef?> GetRemitRefByIdAsync(int remitID);
    Task<(List<RemitRef> items, int totalCount)> GetRemitRefsPagedAsync(
        int? payerID, string? status, DateOnly? dateFrom, DateOnly? dateTo, int page, int pageSize);
    Task<RemitRef> CreateRemitRefAsync(RemitRef entity);
    Task UpdateRemitRefAsync(RemitRef entity);

    // PaymentPost
    Task<PaymentPost?> GetPaymentPostByIdAsync(int paymentID);
    Task<List<PaymentPost>> GetPaymentPostsByClaimAsync(int claimID);
    Task<bool> ActivePostExistsForLineAsync(int claimID, int? claimLineID);
    Task<PaymentPost> CreatePaymentPostAsync(PaymentPost entity);
    Task UpdatePaymentPostAsync(PaymentPost entity);

    // PatientBalance
    Task<PatientBalance?> GetPatientBalanceByIdAsync(int balanceID);
    Task<PatientBalance?> GetPatientBalanceByClaimAsync(int claimID);
    Task<List<PatientBalance>> GetPatientBalancesByPatientAsync(int patientID);
    Task<(List<PatientBalance> items, int totalCount)> GetPatientBalancesPagedAsync(PatientBalanceFilterParams filters);
    Task<AgingSummaryDto> GetAgingSummaryAsync();
    Task<List<PatientBalance>> GetAllOpenBalancesAsync();
    Task<PatientBalance> CreatePatientBalanceAsync(PatientBalance entity);
    Task UpdatePatientBalanceAsync(PatientBalance entity);
    Task SaveAllPatientBalancesAsync(List<PatientBalance> entities);

    // Statement
    Task<Statement?> GetStatementByIdAsync(int statementID);
    Task<bool> StatementExistsForPeriodAsync(int patientID, DateOnly periodStart, DateOnly periodEnd);
    Task<(List<Statement> items, int totalCount)> GetStatementsPagedAsync(
        int? patientID, string? status, DateOnly? dateFrom, DateOnly? dateTo, int page, int pageSize);
    Task<Statement> CreateStatementAsync(Statement entity);
    Task UpdateStatementAsync(Statement entity);

    // Cross-module reads
    Task<Claim?> GetClaimByIdAsync(int claimID);
    Task<List<ClaimLine>> GetActiveClaimLinesByClaimAsync(int claimID);
    Task<ClaimLine?> GetClaimLineByIdAsync(int claimLineID);
    Task UpdateClaimStatusAsync(int claimID, string newStatus);
    Task<Encounter?> GetEncounterByIdAsync(int encounterID);
    Task<Patient?> GetPatientByIdAsync(int patientID);
    Task<bool> PayerExistsAsync(int payerID);
    Task<bool> BatchExistsAsync(int batchID);
    Task<Payer?> GetPayerByIdAsync(int payerID);
    Task<int> GetClaimCountForBatchAsync(int batchID);
    Task<DateTime?> GetFirstPostingDateForClaimAsync(int claimID);

    // Module 10 write
    Task<Denial> CreateDenialAsync(Denial entity);

    Task<List<User>> GetUsersByRoleAsync(string role);
    Task<List<int>> GetDistinctPatientIDsWithOpenBalancesAsync();
    Task WriteAuditLogAsync(int userID, string action, string resource, string? metadata);
    Task CreateNotificationAsync(int userID, string message, string category);
}

