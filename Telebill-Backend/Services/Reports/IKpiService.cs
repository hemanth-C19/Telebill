using System.Collections.Generic;
using Telebill.Dto.Reports;
using Telebill.Models;

namespace Telebill.Services.Reports;

public interface IKpiService
{
    // Pure computation — no DB calls. All raw data is passed in.
    KpiResultDto Compute(
        KpiFilterParams filters,
        string? scopeName,
        List<Claim> claims,
        List<ScrubIssue> scrubIssues,
        List<SubmissionRef> submissionRefs,
        List<PaymentPost> paymentPosts,
        List<Encounter> encounters,
        List<Denial> denials);
}

