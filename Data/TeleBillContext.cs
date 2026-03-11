using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Telebill.Models;

public partial class TeleBillContext : DbContext
{
    public TeleBillContext()
    {
    }

    public TeleBillContext(DbContextOptions<TeleBillContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appeal> Appeals { get; set; }

    public virtual DbSet<Arworkitem> Arworkitems { get; set; }

    public virtual DbSet<AttachmentRef> AttachmentRefs { get; set; }

    public virtual DbSet<Attestation> Attestations { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<BillingReport> BillingReports { get; set; }

    public virtual DbSet<ChargeLine> ChargeLines { get; set; }

    public virtual DbSet<Claim> Claims { get; set; }

    public virtual DbSet<ClaimLine> ClaimLines { get; set; }

    public virtual DbSet<CodingLock> CodingLocks { get; set; }

    public virtual DbSet<Coverage> Coverages { get; set; }

    public virtual DbSet<Denial> Denials { get; set; }

    public virtual DbSet<Diagnosis> Diagnoses { get; set; }

    public virtual DbSet<EligibilityRef> EligibilityRefs { get; set; }

    public virtual DbSet<Encounter> Encounters { get; set; }

    public virtual DbSet<FeeSchedule> FeeSchedules { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<PatientBalance> PatientBalances { get; set; }

    public virtual DbSet<Payer> Payers { get; set; }

    public virtual DbSet<PayerPlan> PayerPlans { get; set; }

    public virtual DbSet<PaymentPost> PaymentPosts { get; set; }

    public virtual DbSet<PriorAuth> PriorAuths { get; set; }

    public virtual DbSet<Provider> Providers { get; set; }

    public virtual DbSet<RemitRef> RemitRefs { get; set; }

    public virtual DbSet<ScrubIssue> ScrubIssues { get; set; }

    public virtual DbSet<ScrubRule> ScrubRules { get; set; }

    public virtual DbSet<Statement> Statements { get; set; }

    public virtual DbSet<SubmissionBatch> SubmissionBatches { get; set; }

    public virtual DbSet<SubmissionRef> SubmissionRefs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<X12837pRef> X12837pRefs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
// #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LTIN718457\\SQLEXPRESS;Initial Catalog=TeleBill;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;Integrated Security=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appeal>(entity =>
        {
            entity.HasKey(e => e.AppealId).HasName("PK__Appeal__BB684E107BD435EE");

            entity.ToTable("Appeal");

            entity.Property(e => e.AppealId).HasColumnName("AppealID");
            entity.Property(e => e.AttachmentUri)
                .HasMaxLength(2048)
                .HasColumnName("AttachmentURI");
            entity.Property(e => e.DenialId).HasColumnName("DenialID");
            entity.Property(e => e.Method).HasMaxLength(50);
            entity.Property(e => e.Outcome).HasMaxLength(50);
            entity.Property(e => e.SubmittedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Denial).WithMany(p => p.Appeals)
                .HasForeignKey(d => d.DenialId)
                .HasConstraintName("FK__Appeal__DenialID__58D1301D");
        });

        modelBuilder.Entity<Arworkitem>(entity =>
        {
            entity.HasKey(e => e.WorkId).HasName("PK__ARWorkit__2DE6D215F3736CC5");

            entity.ToTable("ARWorkitem");

            entity.Property(e => e.WorkId).HasColumnName("WorkID");
            entity.Property(e => e.ClaimId).HasColumnName("ClaimID");
            entity.Property(e => e.Priority).HasMaxLength(20);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Open");

            entity.HasOne(d => d.AssignedToNavigation).WithMany(p => p.Arworkitems)
                .HasForeignKey(d => d.AssignedTo)
                .HasConstraintName("FK__ARWorkite__Assig__5D95E53A");

            entity.HasOne(d => d.Claim).WithMany(p => p.Arworkitems)
                .HasForeignKey(d => d.ClaimId)
                .HasConstraintName("FK__ARWorkite__Claim__5CA1C101");
        });

        modelBuilder.Entity<AttachmentRef>(entity =>
        {
            entity.HasKey(e => e.AttachId).HasName("PK__Attachme__F517FC15AF512FE3");

            entity.ToTable("AttachmentRef");

            entity.Property(e => e.AttachId).HasColumnName("AttachID");
            entity.Property(e => e.ClaimId).HasColumnName("ClaimID");
            entity.Property(e => e.FileType).HasMaxLength(50);
            entity.Property(e => e.FileUri)
                .HasMaxLength(2048)
                .HasColumnName("FileURI");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.UploadedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Claim).WithMany(p => p.AttachmentRefs)
                .HasForeignKey(d => d.ClaimId)
                .HasConstraintName("FK__Attachmen__Claim__3587F3E0");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.AttachmentRefs)
                .HasForeignKey(d => d.UploadedBy)
                .HasConstraintName("FK__Attachmen__Uploa__367C1819");
        });

        modelBuilder.Entity<Attestation>(entity =>
        {
            entity.HasKey(e => e.AttestId).HasName("PK__Attestat__23D3AEC482C75DC7");

            entity.ToTable("Attestation");

            entity.Property(e => e.AttestId).HasColumnName("AttestID");
            entity.Property(e => e.AttestDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EncounterId).HasColumnName("EncounterID");
            entity.Property(e => e.ProviderId).HasColumnName("ProviderID");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Encounter).WithMany(p => p.Attestations)
                .HasForeignKey(d => d.EncounterId)
                .HasConstraintName("FK__Attestati__Encou__0B91BA14");

            entity.HasOne(d => d.Provider).WithMany(p => p.Attestations)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("FK__Attestati__Provi__0C85DE4D");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditId).HasName("PK__AuditLog__A17F23B896C55F9D");

            entity.ToTable("AuditLog");

            entity.Property(e => e.AuditId).HasColumnName("AuditID");
            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.Resource).HasMaxLength(255);
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__AuditLog__UserID__60A75C0F");
        });

        modelBuilder.Entity<BillingReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__BillingR__D5BD48E515F988CD");

            entity.ToTable("BillingReport");

            entity.Property(e => e.ReportId).HasColumnName("ReportID");
            entity.Property(e => e.GeneratedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MetricsJson).HasColumnName("MetricsJSON");
            entity.Property(e => e.Scope).HasMaxLength(255);
        });

        modelBuilder.Entity<ChargeLine>(entity =>
        {
            entity.HasKey(e => e.ChargeId).HasName("PK__ChargeLi__17FC363B48345F36");

            entity.ToTable("ChargeLine");

            entity.Property(e => e.ChargeId).HasColumnName("ChargeID");
            entity.Property(e => e.ChargeAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CptHcpcs)
                .HasMaxLength(10)
                .HasColumnName("CPT_HCPCS");
            entity.Property(e => e.EncounterId).HasColumnName("EncounterID");
            entity.Property(e => e.RevenueCode).HasMaxLength(10);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Draft");
            entity.Property(e => e.Units).HasDefaultValue(1);

            entity.HasOne(d => d.Encounter).WithMany(p => p.ChargeLines)
                .HasForeignKey(d => d.EncounterId)
                .HasConstraintName("FK__ChargeLin__Encou__06CD04F7");
        });

        modelBuilder.Entity<Claim>(entity =>
        {
            entity.HasKey(e => e.ClaimId).HasName("PK__Claim__EF2E13BB55C3AABF");

            entity.ToTable("Claim");

            entity.Property(e => e.ClaimId).HasColumnName("ClaimID");
            entity.Property(e => e.ClaimStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Draft");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EncounterId).HasColumnName("EncounterID");
            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.PlanId).HasColumnName("PlanID");
            entity.Property(e => e.SubscriberRel).HasMaxLength(50);
            entity.Property(e => e.TotalCharge).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Encounter).WithMany(p => p.Claims)
                .HasForeignKey(d => d.EncounterId)
                .HasConstraintName("FK__Claim__Encounter__1AD3FDA4");

            entity.HasOne(d => d.Patient).WithMany(p => p.Claims)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK__Claim__PatientID__1BC821DD");

            entity.HasOne(d => d.Plan).WithMany(p => p.Claims)
                .HasForeignKey(d => d.PlanId)
                .HasConstraintName("FK__Claim__PlanID__1CBC4616");
        });

        modelBuilder.Entity<ClaimLine>(entity =>
        {
            entity.HasKey(e => e.ClaimLineId).HasName("PK__ClaimLin__54106B81B1830998");

            entity.ToTable("ClaimLine");

            entity.Property(e => e.ClaimLineId).HasColumnName("ClaimLineID");
            entity.Property(e => e.ChargeAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ClaimId).HasColumnName("ClaimID");
            entity.Property(e => e.CptHcpcs)
                .HasMaxLength(10)
                .HasColumnName("CPT_HCPCS");
            entity.Property(e => e.LineStatus).HasMaxLength(20);
            entity.Property(e => e.Pos)
                .HasMaxLength(5)
                .HasColumnName("POS");

            entity.HasOne(d => d.Claim).WithMany(p => p.ClaimLines)
                .HasForeignKey(d => d.ClaimId)
                .HasConstraintName("FK__ClaimLine__Claim__2180FB33");
        });

        modelBuilder.Entity<CodingLock>(entity =>
        {
            entity.HasKey(e => e.CodingLockId).HasName("PK__CodingLo__A4770B1493213E96");

            entity.ToTable("CodingLock");

            entity.Property(e => e.CodingLockId).HasColumnName("CodingLockID");
            entity.Property(e => e.CoderId).HasColumnName("CoderID");
            entity.Property(e => e.EncounterId).HasColumnName("EncounterID");
            entity.Property(e => e.LockedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Locked");

            entity.HasOne(d => d.Coder).WithMany(p => p.CodingLocks)
                .HasForeignKey(d => d.CoderId)
                .HasConstraintName("FK__CodingLoc__Coder__160F4887");

            entity.HasOne(d => d.Encounter).WithMany(p => p.CodingLocks)
                .HasForeignKey(d => d.EncounterId)
                .HasConstraintName("FK__CodingLoc__Encou__151B244E");
        });

        modelBuilder.Entity<Coverage>(entity =>
        {
            entity.HasKey(e => e.CoverageId).HasName("PK__Coverage__45403D9BDA4E59D8");

            entity.ToTable("Coverage");

            entity.Property(e => e.CoverageId).HasColumnName("CoverageID");
            entity.Property(e => e.GroupNumber).HasMaxLength(50);
            entity.Property(e => e.MemberId)
                .HasMaxLength(50)
                .HasColumnName("MemberID");
            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.PlanId).HasColumnName("PlanID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.Patient).WithMany(p => p.Coverages)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK__Coverage__Patien__787EE5A0");

            entity.HasOne(d => d.Plan).WithMany(p => p.Coverages)
                .HasForeignKey(d => d.PlanId)
                .HasConstraintName("FK__Coverage__PlanID__797309D9");
        });

        modelBuilder.Entity<Denial>(entity =>
        {
            entity.HasKey(e => e.DenialId).HasName("PK__Denial__82E6F05CDEFC9B9C");

            entity.ToTable("Denial");

            entity.Property(e => e.DenialId).HasColumnName("DenialID");
            entity.Property(e => e.AmountDenied).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ClaimId).HasColumnName("ClaimID");
            entity.Property(e => e.ClaimLineId).HasColumnName("ClaimLineID");
            entity.Property(e => e.ReasonCode).HasMaxLength(20);
            entity.Property(e => e.RemarkCode).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Claim).WithMany(p => p.Denials)
                .HasForeignKey(d => d.ClaimId)
                .HasConstraintName("FK__Denial__ClaimID__55009F39");

            entity.HasOne(d => d.ClaimLine).WithMany(p => p.Denials)
                .HasForeignKey(d => d.ClaimLineId)
                .HasConstraintName("FK__Denial__ClaimLin__55F4C372");
        });

        modelBuilder.Entity<Diagnosis>(entity =>
        {
            entity.HasKey(e => e.DxId).HasName("PK__Diagnosi__35BD562841D9DEA9");

            entity.ToTable("Diagnosis");

            entity.Property(e => e.DxId).HasColumnName("DxID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.EncounterId).HasColumnName("EncounterID");
            entity.Property(e => e.Icd10code)
                .HasMaxLength(15)
                .HasColumnName("ICD10Code");
            entity.Property(e => e.Sequence).HasDefaultValue(1);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.Encounter).WithMany(p => p.Diagnoses)
                .HasForeignKey(d => d.EncounterId)
                .HasConstraintName("FK__Diagnosis__Encou__10566F31");
        });

        modelBuilder.Entity<EligibilityRef>(entity =>
        {
            entity.HasKey(e => e.EligibilityId).HasName("PK__Eligibil__962AC9DB2BD2E0E8");

            entity.ToTable("EligibilityRef");

            entity.Property(e => e.EligibilityId).HasColumnName("EligibilityID");
            entity.Property(e => e.CheckedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CoverageId).HasColumnName("CoverageID");
            entity.Property(e => e.RequestPayloadUri)
                .HasMaxLength(2048)
                .HasColumnName("RequestPayloadURI");
            entity.Property(e => e.ResponsePayloadUri)
                .HasMaxLength(2048)
                .HasColumnName("ResponsePayloadURI");
            entity.Property(e => e.Result).HasMaxLength(50);

            entity.HasOne(d => d.Coverage).WithMany(p => p.EligibilityRefs)
                .HasForeignKey(d => d.CoverageId)
                .HasConstraintName("FK__Eligibili__Cover__7D439ABD");
        });

        modelBuilder.Entity<Encounter>(entity =>
        {
            entity.HasKey(e => e.EncounterId).HasName("PK__Encounte__4278DD165DCAE1F0");

            entity.ToTable("Encounter");

            entity.Property(e => e.EncounterId).HasColumnName("EncounterID");
            entity.Property(e => e.DocumentationUri)
                .HasMaxLength(2048)
                .HasColumnName("DocumentationURI");
            entity.Property(e => e.EncounterDateTime).HasColumnType("datetime");
            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.Pos)
                .HasMaxLength(5)
                .HasDefaultValue("02")
                .HasColumnName("POS");
            entity.Property(e => e.ProviderId).HasColumnName("ProviderID");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Open");
            entity.Property(e => e.VisitType).HasMaxLength(50);

            entity.HasOne(d => d.Patient).WithMany(p => p.Encounters)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK__Encounter__Patie__01142BA1");

            entity.HasOne(d => d.Provider).WithMany(p => p.Encounters)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("FK__Encounter__Provi__02084FDA");
        });

        modelBuilder.Entity<FeeSchedule>(entity =>
        {
            entity.HasKey(e => e.FeeId).HasName("PK__FeeSched__B387B209AAE4E531");

            entity.ToTable("FeeSchedule");

            entity.Property(e => e.FeeId).HasColumnName("FeeID");
            entity.Property(e => e.AllowedAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CptHcpcs)
                .HasMaxLength(10)
                .HasColumnName("CPT_HCPCS");
            entity.Property(e => e.ModifierCombo).HasMaxLength(50);
            entity.Property(e => e.PlanId).HasColumnName("PlanID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.Plan).WithMany(p => p.FeeSchedules)
                .HasForeignKey(d => d.PlanId)
                .HasConstraintName("FK__FeeSchedu__PlanI__70DDC3D8");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E32EC8CF003");

            entity.ToTable("Notification");

            entity.Property(e => e.NotificationId).HasColumnName("NotificationID");
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Unread");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Notificat__UserI__6442E2C9");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("PK__Patient__970EC3462DC0E31F");

            entity.ToTable("Patient");

            entity.HasIndex(e => e.Mrn, "UQ__Patient__C790FDB489FE611A").IsUnique();

            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.AddressJson).HasColumnName("AddressJSON");
            entity.Property(e => e.Dob).HasColumnName("DOB");
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.Mrn)
                .HasMaxLength(50)
                .HasColumnName("MRN");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
        });

        modelBuilder.Entity<PatientBalance>(entity =>
        {
            entity.HasKey(e => e.BalanceId).HasName("PK__PatientB__A760D59E91F73F7B");

            entity.ToTable("PatientBalance");

            entity.Property(e => e.BalanceId).HasColumnName("BalanceID");
            entity.Property(e => e.AgingBucket).HasMaxLength(20);
            entity.Property(e => e.AmountDue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ClaimId).HasColumnName("ClaimID");
            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Claim).WithMany(p => p.PatientBalances)
                .HasForeignKey(d => d.ClaimId)
                .HasConstraintName("FK__PatientBa__Claim__4D5F7D71");

            entity.HasOne(d => d.Patient).WithMany(p => p.PatientBalances)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK__PatientBa__Patie__4C6B5938");
        });

        modelBuilder.Entity<Payer>(entity =>
        {
            entity.HasKey(e => e.PayerId).HasName("PK__Payer__0ADBE847890AAAF4");

            entity.ToTable("Payer");

            entity.Property(e => e.PayerId).HasColumnName("PayerID");
            entity.Property(e => e.ClearinghouseCode).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.PayerCode).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
        });

        modelBuilder.Entity<PayerPlan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PK__PayerPla__755C22D73A52AAAE");

            entity.ToTable("PayerPlan");

            entity.Property(e => e.PlanId).HasColumnName("PlanID");
            entity.Property(e => e.NetworkType).HasMaxLength(50);
            entity.Property(e => e.PayerId).HasColumnName("PayerID");
            entity.Property(e => e.PlanName).HasMaxLength(255);
            entity.Property(e => e.Posdefault)
                .HasMaxLength(5)
                .HasDefaultValue("02")
                .HasColumnName("POSDefault");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.TelehealthModifiersJson).HasColumnName("TelehealthModifiersJSON");

            entity.HasOne(d => d.Payer).WithMany(p => p.PayerPlans)
                .HasForeignKey(d => d.PayerId)
                .HasConstraintName("FK__PayerPlan__Payer__6C190EBB");
        });

        modelBuilder.Entity<PaymentPost>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__PaymentP__9B556A5879177ED2");

            entity.ToTable("PaymentPost");

            entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
            entity.Property(e => e.AdjustmentJson).HasColumnName("AdjustmentJSON");
            entity.Property(e => e.AmountPaid).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ClaimId).HasColumnName("ClaimID");
            entity.Property(e => e.ClaimLineId).HasColumnName("ClaimLineID");
            entity.Property(e => e.PostedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Claim).WithMany(p => p.PaymentPosts)
                .HasForeignKey(d => d.ClaimId)
                .HasConstraintName("FK__PaymentPo__Claim__46B27FE2");

            entity.HasOne(d => d.ClaimLine).WithMany(p => p.PaymentPosts)
                .HasForeignKey(d => d.ClaimLineId)
                .HasConstraintName("FK__PaymentPo__Claim__47A6A41B");

            entity.HasOne(d => d.PostedByNavigation).WithMany(p => p.PaymentPosts)
                .HasForeignKey(d => d.PostedBy)
                .HasConstraintName("FK__PaymentPo__Poste__498EEC8D");
        });

        modelBuilder.Entity<PriorAuth>(entity =>
        {
            entity.HasKey(e => e.Paid).HasName("PK__PriorAut__5986FD6D44EA84EE");

            entity.ToTable("PriorAuth");

            entity.Property(e => e.Paid).HasColumnName("PAID");
            entity.Property(e => e.AuthNumber).HasMaxLength(100);
            entity.Property(e => e.ClaimId).HasColumnName("ClaimID");
            entity.Property(e => e.PlanId).HasColumnName("PlanID");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Claim).WithMany(p => p.PriorAuths)
                .HasForeignKey(d => d.ClaimId)
                .HasConstraintName("FK__PriorAuth__Claim__31B762FC");

            entity.HasOne(d => d.Plan).WithMany(p => p.PriorAuths)
                .HasForeignKey(d => d.PlanId)
                .HasConstraintName("FK__PriorAuth__PlanI__32AB8735");
        });

        modelBuilder.Entity<Provider>(entity =>
        {
            entity.HasKey(e => e.ProviderId).HasName("PK__Provider__B54C689DBA7F593C");

            entity.ToTable("Provider");

            entity.HasIndex(e => e.Npi, "UQ__Provider__C7DE9A1288CE189A").IsUnique();

            entity.Property(e => e.ProviderId).HasColumnName("ProviderID");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Npi)
                .HasMaxLength(10)
                .HasColumnName("NPI");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.Taxonomy).HasMaxLength(100);
            entity.Property(e => e.TelehealthEnrolled).HasDefaultValue(false);
        });

        modelBuilder.Entity<RemitRef>(entity =>
        {
            entity.HasKey(e => e.RemitId).HasName("PK__RemitRef__A1F3A787298AB3D2");

            entity.ToTable("RemitRef");

            entity.Property(e => e.RemitId).HasColumnName("RemitID");
            entity.Property(e => e.BatchId).HasColumnName("BatchID");
            entity.Property(e => e.PayerId).HasColumnName("PayerID");
            entity.Property(e => e.PayloadUri)
                .HasMaxLength(2048)
                .HasColumnName("PayloadURI");
            entity.Property(e => e.ReceivedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Batch).WithMany(p => p.RemitRefs)
                .HasForeignKey(d => d.BatchId)
                .HasConstraintName("FK__RemitRef__BatchI__42E1EEFE");

            entity.HasOne(d => d.Payer).WithMany(p => p.RemitRefs)
                .HasForeignKey(d => d.PayerId)
                .HasConstraintName("FK__RemitRef__PayerI__41EDCAC5");
        });

        modelBuilder.Entity<ScrubIssue>(entity =>
        {
            entity.HasKey(e => e.IssueId).HasName("PK__ScrubIss__6C86162459062E5F");

            entity.ToTable("ScrubIssue");

            entity.Property(e => e.IssueId).HasColumnName("IssueID");
            entity.Property(e => e.ClaimId).HasColumnName("ClaimID");
            entity.Property(e => e.ClaimLineId).HasColumnName("ClaimLineID");
            entity.Property(e => e.DetectedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RuleId).HasColumnName("RuleID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Open");

            entity.HasOne(d => d.Claim).WithMany(p => p.ScrubIssues)
                .HasForeignKey(d => d.ClaimId)
                .HasConstraintName("FK__ScrubIssu__Claim__2739D489");

            entity.HasOne(d => d.ClaimLine).WithMany(p => p.ScrubIssues)
                .HasForeignKey(d => d.ClaimLineId)
                .HasConstraintName("FK__ScrubIssu__Claim__282DF8C2");

            entity.HasOne(d => d.Rule).WithMany(p => p.ScrubIssues)
                .HasForeignKey(d => d.RuleId)
                .HasConstraintName("FK__ScrubIssu__RuleI__29221CFB");
        });

        modelBuilder.Entity<ScrubRule>(entity =>
        {
            entity.HasKey(e => e.RuleId).HasName("PK__ScrubRul__110458C2261FCA16");

            entity.ToTable("ScrubRule");

            entity.Property(e => e.RuleId).HasColumnName("RuleID");
            entity.Property(e => e.ExpressionJson).HasColumnName("ExpressionJSON");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Severity).HasMaxLength(20);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
        });

        modelBuilder.Entity<Statement>(entity =>
        {
            entity.HasKey(e => e.StatementId).HasName("PK__Statemen__2B7E04222CBFCE1A");

            entity.ToTable("Statement");

            entity.Property(e => e.StatementId).HasColumnName("StatementID");
            entity.Property(e => e.AmountDue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GeneratedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Open");
            entity.Property(e => e.SummaryJson).HasColumnName("SummaryJSON");

            entity.HasOne(d => d.Patient).WithMany(p => p.Statements)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK__Statement__Patie__503BEA1C");
        });

        modelBuilder.Entity<SubmissionBatch>(entity =>
        {
            entity.HasKey(e => e.BatchId).HasName("PK__Submissi__5D55CE384DE7D73B");

            entity.ToTable("SubmissionBatch");

            entity.Property(e => e.BatchId).HasColumnName("BatchID");
            entity.Property(e => e.BatchDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TotalCharge).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<SubmissionRef>(entity =>
        {
            entity.HasKey(e => e.SubmitId).HasName("PK__Submissi__421A8E1ED53F8917");

            entity.ToTable("SubmissionRef");

            entity.Property(e => e.SubmitId).HasColumnName("SubmitID");
            entity.Property(e => e.AckDate).HasColumnType("datetime");
            entity.Property(e => e.AckStatus).HasMaxLength(20);
            entity.Property(e => e.AckType).HasMaxLength(20);
            entity.Property(e => e.BatchId).HasColumnName("BatchID");
            entity.Property(e => e.ClaimId).HasColumnName("ClaimID");
            entity.Property(e => e.ClearinghouseId)
                .HasMaxLength(50)
                .HasColumnName("ClearinghouseID");
            entity.Property(e => e.CorrelationId)
                .HasMaxLength(100)
                .HasColumnName("CorrelationID");
            entity.Property(e => e.SubmitDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Batch).WithMany(p => p.SubmissionRefs)
                .HasForeignKey(d => d.BatchId)
                .HasConstraintName("FK__Submissio__Batch__3D2915A8");

            entity.HasOne(d => d.Claim).WithMany(p => p.SubmissionRefs)
                .HasForeignKey(d => d.ClaimId)
                .HasConstraintName("FK__Submissio__Claim__3E1D39E1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CCAC4D9BD9F6");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__A9D105340ED77520").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
        });

        modelBuilder.Entity<X12837pRef>(entity =>
        {
            entity.HasKey(e => e.X12id).HasName("PK__X12_837P__68BEC4B72FC858AE");

            entity.ToTable("X12_837P_Ref");

            entity.Property(e => e.X12id).HasColumnName("X12ID");
            entity.Property(e => e.ClaimId).HasColumnName("ClaimID");
            entity.Property(e => e.GeneratedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PayloadUri)
                .HasMaxLength(2048)
                .HasColumnName("PayloadURI");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Version).HasMaxLength(20);

            entity.HasOne(d => d.Claim).WithMany(p => p.X12837pRefs)
                .HasForeignKey(d => d.ClaimId)
                .HasConstraintName("FK__X12_837P___Claim__2DE6D218");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
