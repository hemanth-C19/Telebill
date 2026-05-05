using System;
using System.Threading.Tasks;
using Telebill.Dto.Posting;

namespace Telebill.Services.Posting;

public interface IPostingService
{
    Task<RemitRefDto> CreateRemitRefAsync(CreateRemitRefRequestDto dto, int currentUserID);
    Task<RemitRefListResponseDto> GetRemitRefsAsync(int? payerID, string? status, DateOnly? dateFrom, DateOnly? dateTo, int page, int pageSize);
    Task<RemitRefDto> GetRemitRefByIdAsync(int remitID);
    Task<RemitRefDto> UpdateRemitRefStatusAsync(int remitID, UpdateRemitRefStatusRequestDto dto, int currentUserID);

    Task<PostingResultDto> CreatePaymentPostAsync(CreatePaymentPostRequestDto dto, int currentUserID);
    Task<ClaimPaymentSummaryDto> GetPaymentPostsByClaimAsync(int claimID);
    Task<PaymentPostDto> GetPaymentPostByIdAsync(int paymentID);
    Task<PostingResultDto> VoidPaymentPostAsync(int paymentID, VoidPaymentPostRequestDto dto, int currentUserID);

    Task<PatientBalanceListResponseDto> GetPatientBalancesAsync(PatientBalanceFilterParams filters);
    Task<AgingSummaryDto> GetAgingSummaryAsync();
    Task<PatientBalanceDto> GetPatientBalanceByIdAsync(int balanceID);
    Task<PatientBalanceListResponseDto> GetBalancesByPatientAsync(int patientID);
    Task<PatientBalanceDto> UpdatePatientBalanceStatusAsync(int balanceID, UpdatePatientBalanceStatusRequestDto dto, int currentUserID);
    Task<AgingBatchJobResultDto> RunAgingBucketJobAsync(string? schedulerKey);

    Task<StatementDto> GenerateStatementAsync(GenerateStatementRequestDto dto, int currentUserID);
    Task<StatementBatchResultDto> GenerateStatementBatchAsync(GenerateStatementBatchRequestDto dto);
    Task<StatementListResponseDto> GetStatementsAsync(int? patientID, string? status, DateOnly? dateFrom, DateOnly? dateTo, int page, int pageSize);
    Task<StatementDto> GetStatementByIdAsync(int statementID);
    Task<StatementDto> UpdateStatementStatusAsync(int statementID, UpdateStatementStatusRequestDto dto, int currentUserID);
}

