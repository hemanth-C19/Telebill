using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telebill.Migrations
{
    /// <inheritdoc />
    public partial class initialmigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BillingReport",
                columns: table => new
                {
                    ReportID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Scope = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MetricsJSON = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeneratedDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BillingR__D5BD48E515F988CD", x => x.ReportID);
                });

            migrationBuilder.CreateTable(
                name: "Patient",
                columns: table => new
                {
                    PatientID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MRN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DOB = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AddressJSON = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Patient__970EC3462DC0E31F", x => x.PatientID);
                });

            migrationBuilder.CreateTable(
                name: "Payer",
                columns: table => new
                {
                    PayerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PayerCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ClearinghouseCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContactInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Payer__0ADBE847890AAAF4", x => x.PayerID);
                });

            migrationBuilder.CreateTable(
                name: "Provider",
                columns: table => new
                {
                    ProviderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NPI = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Taxonomy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TelehealthEnrolled = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    ContactInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Provider__B54C689DBA7F593C", x => x.ProviderID);
                });

            migrationBuilder.CreateTable(
                name: "ScrubRule",
                columns: table => new
                {
                    RuleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ExpressionJSON = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ScrubRul__110458C2261FCA16", x => x.RuleID);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionBatch",
                columns: table => new
                {
                    BatchID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    ItemCount = table.Column<int>(type: "int", nullable: true),
                    TotalCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Submissi__5D55CE384DE7D73B", x => x.BatchID);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__User__1788CCAC4D9BD9F6", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "Statement",
                columns: table => new
                {
                    StatementID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientID = table.Column<int>(type: "int", nullable: true),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: true),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: true),
                    GeneratedDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    AmountDue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SummaryJSON = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Open")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Statemen__2B7E04222CBFCE1A", x => x.StatementID);
                    table.ForeignKey(
                        name: "FK__Statement__Patie__503BEA1C",
                        column: x => x.PatientID,
                        principalTable: "Patient",
                        principalColumn: "PatientID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayerPlan",
                columns: table => new
                {
                    PlanID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayerID = table.Column<int>(type: "int", nullable: true),
                    PlanName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NetworkType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    POSDefault = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true, defaultValue: "02"),
                    TelehealthModifiersJSON = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PayerPla__755C22D73A52AAAE", x => x.PlanID);
                    table.ForeignKey(
                        name: "FK__PayerPlan__Payer__6C190EBB",
                        column: x => x.PayerID,
                        principalTable: "Payer",
                        principalColumn: "PayerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Encounter",
                columns: table => new
                {
                    EncounterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientID = table.Column<int>(type: "int", nullable: true),
                    ProviderID = table.Column<int>(type: "int", nullable: true),
                    EncounterDateTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    VisitType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    POS = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true, defaultValue: "02"),
                    DocumentationURI = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "Open")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Encounte__4278DD165DCAE1F0", x => x.EncounterID);
                    table.ForeignKey(
                        name: "FK__Encounter__Patie__01142BA1",
                        column: x => x.PatientID,
                        principalTable: "Patient",
                        principalColumn: "PatientID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Encounter__Provi__02084FDA",
                        column: x => x.ProviderID,
                        principalTable: "Provider",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RemitRef",
                columns: table => new
                {
                    RemitID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayerID = table.Column<int>(type: "int", nullable: true),
                    BatchID = table.Column<int>(type: "int", nullable: true),
                    PayloadURI = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RemitRef__A1F3A787298AB3D2", x => x.RemitID);
                    table.ForeignKey(
                        name: "FK__RemitRef__BatchI__42E1EEFE",
                        column: x => x.BatchID,
                        principalTable: "SubmissionBatch",
                        principalColumn: "BatchID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK__RemitRef__PayerI__41EDCAC5",
                        column: x => x.PayerID,
                        principalTable: "Payer",
                        principalColumn: "PayerID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    AuditID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Resource = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AuditLog__A17F23B896C55F9D", x => x.AuditID);
                    table.ForeignKey(
                        name: "FK__AuditLog__UserID__60A75C0F",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    NotificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Unread"),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__20CF2E32EC8CF003", x => x.NotificationID);
                    table.ForeignKey(
                        name: "FK__Notificat__UserI__6442E2C9",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Coverage",
                columns: table => new
                {
                    CoverageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientID = table.Column<int>(type: "int", nullable: true),
                    PlanID = table.Column<int>(type: "int", nullable: true),
                    MemberID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GroupNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Coverage__45403D9BDA4E59D8", x => x.CoverageID);
                    table.ForeignKey(
                        name: "FK__Coverage__Patien__787EE5A0",
                        column: x => x.PatientID,
                        principalTable: "Patient",
                        principalColumn: "PatientID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Coverage__PlanID__797309D9",
                        column: x => x.PlanID,
                        principalTable: "PayerPlan",
                        principalColumn: "PlanID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FeeSchedule",
                columns: table => new
                {
                    FeeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanID = table.Column<int>(type: "int", nullable: true),
                    CPT_HCPCS = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ModifierCombo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AllowedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FeeSched__B387B209AAE4E531", x => x.FeeID);
                    table.ForeignKey(
                        name: "FK__FeeSchedu__PlanI__70DDC3D8",
                        column: x => x.PlanID,
                        principalTable: "PayerPlan",
                        principalColumn: "PlanID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attestation",
                columns: table => new
                {
                    AttestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EncounterID = table.Column<int>(type: "int", nullable: true),
                    ProviderID = table.Column<int>(type: "int", nullable: true),
                    AttestText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttestDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Attestat__23D3AEC482C75DC7", x => x.AttestID);
                    table.ForeignKey(
                        name: "FK__Attestati__Encou__0B91BA14",
                        column: x => x.EncounterID,
                        principalTable: "Encounter",
                        principalColumn: "EncounterID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Attestati__Provi__0C85DE4D",
                        column: x => x.ProviderID,
                        principalTable: "Provider",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ChargeLine",
                columns: table => new
                {
                    ChargeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EncounterID = table.Column<int>(type: "int", nullable: true),
                    CPT_HCPCS = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Modifiers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Units = table.Column<int>(type: "int", nullable: true, defaultValue: 1),
                    ChargeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RevenueCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Draft")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ChargeLi__17FC363B48345F36", x => x.ChargeID);
                    table.ForeignKey(
                        name: "FK__ChargeLin__Encou__06CD04F7",
                        column: x => x.EncounterID,
                        principalTable: "Encounter",
                        principalColumn: "EncounterID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Claim",
                columns: table => new
                {
                    ClaimID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EncounterID = table.Column<int>(type: "int", nullable: true),
                    PatientID = table.Column<int>(type: "int", nullable: true),
                    PlanID = table.Column<int>(type: "int", nullable: true),
                    SubscriberRel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TotalCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ClaimStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "Draft"),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Claim__EF2E13BB55C3AABF", x => x.ClaimID);
                    table.ForeignKey(
                        name: "FK__Claim__Encounter__1AD3FDA4",
                        column: x => x.EncounterID,
                        principalTable: "Encounter",
                        principalColumn: "EncounterID");
                    table.ForeignKey(
                        name: "FK__Claim__PatientID__1BC821DD",
                        column: x => x.PatientID,
                        principalTable: "Patient",
                        principalColumn: "PatientID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Claim__PlanID__1CBC4616",
                        column: x => x.PlanID,
                        principalTable: "PayerPlan",
                        principalColumn: "PlanID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CodingLock",
                columns: table => new
                {
                    CodingLockID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EncounterID = table.Column<int>(type: "int", nullable: true),
                    CoderID = table.Column<int>(type: "int", nullable: true),
                    LockedDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Locked")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CodingLo__A4770B1493213E96", x => x.CodingLockID);
                    table.ForeignKey(
                        name: "FK__CodingLoc__Coder__160F4887",
                        column: x => x.CoderID,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK__CodingLoc__Encou__151B244E",
                        column: x => x.EncounterID,
                        principalTable: "Encounter",
                        principalColumn: "EncounterID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Diagnosis",
                columns: table => new
                {
                    DxID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EncounterID = table.Column<int>(type: "int", nullable: true),
                    ICD10Code = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: true, defaultValue: 1),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Diagnosi__35BD562841D9DEA9", x => x.DxID);
                    table.ForeignKey(
                        name: "FK__Diagnosis__Encou__10566F31",
                        column: x => x.EncounterID,
                        principalTable: "Encounter",
                        principalColumn: "EncounterID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EligibilityRef",
                columns: table => new
                {
                    EligibilityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoverageID = table.Column<int>(type: "int", nullable: true),
                    RequestPayloadURI = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    ResponsePayloadURI = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    CheckedDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Result = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Eligibil__962AC9DB2BD2E0E8", x => x.EligibilityID);
                    table.ForeignKey(
                        name: "FK__Eligibili__Cover__7D439ABD",
                        column: x => x.CoverageID,
                        principalTable: "Coverage",
                        principalColumn: "CoverageID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ARWorkitem",
                columns: table => new
                {
                    WorkID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimID = table.Column<int>(type: "int", nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AssignedTo = table.Column<int>(type: "int", nullable: true),
                    NextActionDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Open")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ARWorkit__2DE6D215F3736CC5", x => x.WorkID);
                    table.ForeignKey(
                        name: "FK__ARWorkite__Assig__5D95E53A",
                        column: x => x.AssignedTo,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK__ARWorkite__Claim__5CA1C101",
                        column: x => x.ClaimID,
                        principalTable: "Claim",
                        principalColumn: "ClaimID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttachmentRef",
                columns: table => new
                {
                    AttachID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimID = table.Column<int>(type: "int", nullable: true),
                    FileType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FileURI = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedBy = table.Column<int>(type: "int", nullable: true),
                    UploadedDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Attachme__F517FC15AF512FE3", x => x.AttachID);
                    table.ForeignKey(
                        name: "FK__Attachmen__Claim__3587F3E0",
                        column: x => x.ClaimID,
                        principalTable: "Claim",
                        principalColumn: "ClaimID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Attachmen__Uploa__367C1819",
                        column: x => x.UploadedBy,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ClaimLine",
                columns: table => new
                {
                    ClaimLineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimID = table.Column<int>(type: "int", nullable: true),
                    LineNo = table.Column<int>(type: "int", nullable: true),
                    CPT_HCPCS = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Modifiers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Units = table.Column<int>(type: "int", nullable: true),
                    ChargeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DxPointers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    POS = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    LineStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ClaimLin__54106B81B1830998", x => x.ClaimLineID);
                    table.ForeignKey(
                        name: "FK__ClaimLine__Claim__2180FB33",
                        column: x => x.ClaimID,
                        principalTable: "Claim",
                        principalColumn: "ClaimID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientBalance",
                columns: table => new
                {
                    BalanceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientID = table.Column<int>(type: "int", nullable: true),
                    ClaimID = table.Column<int>(type: "int", nullable: true),
                    AmountDue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AgingBucket = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LastStatementDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PatientB__A760D59E91F73F7B", x => x.BalanceID);
                    table.ForeignKey(
                        name: "FK__PatientBa__Claim__4D5F7D71",
                        column: x => x.ClaimID,
                        principalTable: "Claim",
                        principalColumn: "ClaimID");
                    table.ForeignKey(
                        name: "FK__PatientBa__Patie__4C6B5938",
                        column: x => x.PatientID,
                        principalTable: "Patient",
                        principalColumn: "PatientID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PriorAuth",
                columns: table => new
                {
                    PAID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimID = table.Column<int>(type: "int", nullable: true),
                    PlanID = table.Column<int>(type: "int", nullable: true),
                    AuthNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RequestedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ApprovedFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    ApprovedTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PriorAut__5986FD6D44EA84EE", x => x.PAID);
                    table.ForeignKey(
                        name: "FK__PriorAuth__Claim__31B762FC",
                        column: x => x.ClaimID,
                        principalTable: "Claim",
                        principalColumn: "ClaimID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__PriorAuth__PlanI__32AB8735",
                        column: x => x.PlanID,
                        principalTable: "PayerPlan",
                        principalColumn: "PlanID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionRef",
                columns: table => new
                {
                    SubmitID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchID = table.Column<int>(type: "int", nullable: true),
                    ClaimID = table.Column<int>(type: "int", nullable: true),
                    ClearinghouseID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CorrelationID = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SubmitDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    AckType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AckStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AckDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Submissi__421A8E1ED53F8917", x => x.SubmitID);
                    table.ForeignKey(
                        name: "FK__Submissio__Batch__3D2915A8",
                        column: x => x.BatchID,
                        principalTable: "SubmissionBatch",
                        principalColumn: "BatchID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Submissio__Claim__3E1D39E1",
                        column: x => x.ClaimID,
                        principalTable: "Claim",
                        principalColumn: "ClaimID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "X12_837P_Ref",
                columns: table => new
                {
                    X12ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimID = table.Column<int>(type: "int", nullable: true),
                    PayloadURI = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    GeneratedDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Version = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__X12_837P__68BEC4B72FC858AE", x => x.X12ID);
                    table.ForeignKey(
                        name: "FK__X12_837P___Claim__2DE6D218",
                        column: x => x.ClaimID,
                        principalTable: "Claim",
                        principalColumn: "ClaimID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Denial",
                columns: table => new
                {
                    DenialID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimID = table.Column<int>(type: "int", nullable: true),
                    ClaimLineID = table.Column<int>(type: "int", nullable: true),
                    ReasonCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RemarkCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DenialDate = table.Column<DateOnly>(type: "date", nullable: true),
                    AmountDenied = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Denial__82E6F05CDEFC9B9C", x => x.DenialID);
                    table.ForeignKey(
                        name: "FK__Denial__ClaimID__55009F39",
                        column: x => x.ClaimID,
                        principalTable: "Claim",
                        principalColumn: "ClaimID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Denial__ClaimLin__55F4C372",
                        column: x => x.ClaimLineID,
                        principalTable: "ClaimLine",
                        principalColumn: "ClaimLineID");
                });

            migrationBuilder.CreateTable(
                name: "PaymentPost",
                columns: table => new
                {
                    PaymentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimID = table.Column<int>(type: "int", nullable: true),
                    ClaimLineID = table.Column<int>(type: "int", nullable: true),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AdjustmentJSON = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostedDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    PostedBy = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PaymentP__9B556A5879177ED2", x => x.PaymentID);
                    table.ForeignKey(
                        name: "FK__PaymentPo__Claim__46B27FE2",
                        column: x => x.ClaimID,
                        principalTable: "Claim",
                        principalColumn: "ClaimID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__PaymentPo__Claim__47A6A41B",
                        column: x => x.ClaimLineID,
                        principalTable: "ClaimLine",
                        principalColumn: "ClaimLineID");
                    table.ForeignKey(
                        name: "FK__PaymentPo__Poste__498EEC8D",
                        column: x => x.PostedBy,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ScrubIssue",
                columns: table => new
                {
                    IssueID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimID = table.Column<int>(type: "int", nullable: true),
                    ClaimLineID = table.Column<int>(type: "int", nullable: true),
                    RuleID = table.Column<int>(type: "int", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetectedDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Open")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ScrubIss__6C86162459062E5F", x => x.IssueID);
                    table.ForeignKey(
                        name: "FK__ScrubIssu__Claim__2739D489",
                        column: x => x.ClaimID,
                        principalTable: "Claim",
                        principalColumn: "ClaimID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__ScrubIssu__Claim__282DF8C2",
                        column: x => x.ClaimLineID,
                        principalTable: "ClaimLine",
                        principalColumn: "ClaimLineID");
                    table.ForeignKey(
                        name: "FK__ScrubIssu__RuleI__29221CFB",
                        column: x => x.RuleID,
                        principalTable: "ScrubRule",
                        principalColumn: "RuleID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Appeal",
                columns: table => new
                {
                    AppealID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DenialID = table.Column<int>(type: "int", nullable: true),
                    SubmittedDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AttachmentURI = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Outcome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OutcomeDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Appeal__BB684E107BD435EE", x => x.AppealID);
                    table.ForeignKey(
                        name: "FK__Appeal__DenialID__58D1301D",
                        column: x => x.DenialID,
                        principalTable: "Denial",
                        principalColumn: "DenialID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appeal_DenialID",
                table: "Appeal",
                column: "DenialID");

            migrationBuilder.CreateIndex(
                name: "IX_ARWorkitem_AssignedTo",
                table: "ARWorkitem",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_ARWorkitem_ClaimID",
                table: "ARWorkitem",
                column: "ClaimID");

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentRef_ClaimID",
                table: "AttachmentRef",
                column: "ClaimID");

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentRef_UploadedBy",
                table: "AttachmentRef",
                column: "UploadedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Attestation_EncounterID",
                table: "Attestation",
                column: "EncounterID");

            migrationBuilder.CreateIndex(
                name: "IX_Attestation_ProviderID",
                table: "Attestation",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_UserID",
                table: "AuditLog",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ChargeLine_EncounterID",
                table: "ChargeLine",
                column: "EncounterID");

            migrationBuilder.CreateIndex(
                name: "IX_Claim_EncounterID",
                table: "Claim",
                column: "EncounterID");

            migrationBuilder.CreateIndex(
                name: "IX_Claim_PatientID",
                table: "Claim",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_Claim_PlanID",
                table: "Claim",
                column: "PlanID");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimLine_ClaimID",
                table: "ClaimLine",
                column: "ClaimID");

            migrationBuilder.CreateIndex(
                name: "IX_CodingLock_CoderID",
                table: "CodingLock",
                column: "CoderID");

            migrationBuilder.CreateIndex(
                name: "IX_CodingLock_EncounterID",
                table: "CodingLock",
                column: "EncounterID");

            migrationBuilder.CreateIndex(
                name: "IX_Coverage_PatientID",
                table: "Coverage",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_Coverage_PlanID",
                table: "Coverage",
                column: "PlanID");

            migrationBuilder.CreateIndex(
                name: "IX_Denial_ClaimID",
                table: "Denial",
                column: "ClaimID");

            migrationBuilder.CreateIndex(
                name: "IX_Denial_ClaimLineID",
                table: "Denial",
                column: "ClaimLineID");

            migrationBuilder.CreateIndex(
                name: "IX_Diagnosis_EncounterID",
                table: "Diagnosis",
                column: "EncounterID");

            migrationBuilder.CreateIndex(
                name: "IX_EligibilityRef_CoverageID",
                table: "EligibilityRef",
                column: "CoverageID");

            migrationBuilder.CreateIndex(
                name: "IX_Encounter_PatientID",
                table: "Encounter",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_Encounter_ProviderID",
                table: "Encounter",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_FeeSchedule_PlanID",
                table: "FeeSchedule",
                column: "PlanID");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserID",
                table: "Notification",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "UQ__Patient__C790FDB489FE611A",
                table: "Patient",
                column: "MRN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientBalance_ClaimID",
                table: "PatientBalance",
                column: "ClaimID");

            migrationBuilder.CreateIndex(
                name: "IX_PatientBalance_PatientID",
                table: "PatientBalance",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_PayerPlan_PayerID",
                table: "PayerPlan",
                column: "PayerID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentPost_ClaimID",
                table: "PaymentPost",
                column: "ClaimID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentPost_ClaimLineID",
                table: "PaymentPost",
                column: "ClaimLineID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentPost_PostedBy",
                table: "PaymentPost",
                column: "PostedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PriorAuth_ClaimID",
                table: "PriorAuth",
                column: "ClaimID");

            migrationBuilder.CreateIndex(
                name: "IX_PriorAuth_PlanID",
                table: "PriorAuth",
                column: "PlanID");

            migrationBuilder.CreateIndex(
                name: "UQ__Provider__C7DE9A1288CE189A",
                table: "Provider",
                column: "NPI",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RemitRef_BatchID",
                table: "RemitRef",
                column: "BatchID");

            migrationBuilder.CreateIndex(
                name: "IX_RemitRef_PayerID",
                table: "RemitRef",
                column: "PayerID");

            migrationBuilder.CreateIndex(
                name: "IX_ScrubIssue_ClaimID",
                table: "ScrubIssue",
                column: "ClaimID");

            migrationBuilder.CreateIndex(
                name: "IX_ScrubIssue_ClaimLineID",
                table: "ScrubIssue",
                column: "ClaimLineID");

            migrationBuilder.CreateIndex(
                name: "IX_ScrubIssue_RuleID",
                table: "ScrubIssue",
                column: "RuleID");

            migrationBuilder.CreateIndex(
                name: "IX_Statement_PatientID",
                table: "Statement",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionRef_BatchID",
                table: "SubmissionRef",
                column: "BatchID");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionRef_ClaimID",
                table: "SubmissionRef",
                column: "ClaimID");

            migrationBuilder.CreateIndex(
                name: "UQ__User__A9D105340ED77520",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_X12_837P_Ref_ClaimID",
                table: "X12_837P_Ref",
                column: "ClaimID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appeal");

            migrationBuilder.DropTable(
                name: "ARWorkitem");

            migrationBuilder.DropTable(
                name: "AttachmentRef");

            migrationBuilder.DropTable(
                name: "Attestation");

            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "BillingReport");

            migrationBuilder.DropTable(
                name: "ChargeLine");

            migrationBuilder.DropTable(
                name: "CodingLock");

            migrationBuilder.DropTable(
                name: "Diagnosis");

            migrationBuilder.DropTable(
                name: "EligibilityRef");

            migrationBuilder.DropTable(
                name: "FeeSchedule");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "PatientBalance");

            migrationBuilder.DropTable(
                name: "PaymentPost");

            migrationBuilder.DropTable(
                name: "PriorAuth");

            migrationBuilder.DropTable(
                name: "RemitRef");

            migrationBuilder.DropTable(
                name: "ScrubIssue");

            migrationBuilder.DropTable(
                name: "Statement");

            migrationBuilder.DropTable(
                name: "SubmissionRef");

            migrationBuilder.DropTable(
                name: "X12_837P_Ref");

            migrationBuilder.DropTable(
                name: "Denial");

            migrationBuilder.DropTable(
                name: "Coverage");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "ScrubRule");

            migrationBuilder.DropTable(
                name: "SubmissionBatch");

            migrationBuilder.DropTable(
                name: "ClaimLine");

            migrationBuilder.DropTable(
                name: "Claim");

            migrationBuilder.DropTable(
                name: "Encounter");

            migrationBuilder.DropTable(
                name: "PayerPlan");

            migrationBuilder.DropTable(
                name: "Patient");

            migrationBuilder.DropTable(
                name: "Provider");

            migrationBuilder.DropTable(
                name: "Payer");
        }
    }
}
