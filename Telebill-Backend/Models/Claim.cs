using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class Claim
{
    public int ClaimId { get; set; }

    public int? EncounterId { get; set; }

    public int? PatientId { get; set; }

    public int? PlanId { get; set; }

    public string? SubscriberRel { get; set; }

    public decimal? TotalCharge { get; set; }

    public string? ClaimStatus { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<Arworkitem> Arworkitems { get; set; } = new List<Arworkitem>();

    public virtual ICollection<AttachmentRef> AttachmentRefs { get; set; } = new List<AttachmentRef>();

    public virtual ICollection<ClaimLine> ClaimLines { get; set; } = new List<ClaimLine>();

    public virtual ICollection<Denial> Denials { get; set; } = new List<Denial>();

    public virtual Encounter? Encounter { get; set; }

    public virtual Patient? Patient { get; set; }

    public virtual ICollection<PatientBalance> PatientBalances { get; set; } = new List<PatientBalance>();

    public virtual ICollection<PaymentPost> PaymentPosts { get; set; } = new List<PaymentPost>();

    public virtual PayerPlan? Plan { get; set; }

    public virtual ICollection<PriorAuth> PriorAuths { get; set; } = new List<PriorAuth>();

    public virtual ICollection<ScrubIssue> ScrubIssues { get; set; } = new List<ScrubIssue>();

    public virtual ICollection<SubmissionRef> SubmissionRefs { get; set; } = new List<SubmissionRef>();

    public virtual ICollection<X12837pRef> X12837pRefs { get; set; } = new List<X12837pRef>();
}
