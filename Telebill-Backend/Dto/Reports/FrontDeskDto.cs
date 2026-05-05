namespace Telebill.Dto.Reports;

public record FrontDeskSummaryDto(
    int TotalPatients,
    int TodayEncounters,
    int UnbilledEncounters,
    int RejectedClaims,
    int PendingBatches,
    int OutstandingStatements,
    decimal OutstandingAmount
);
