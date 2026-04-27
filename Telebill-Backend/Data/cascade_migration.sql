-- ============================================================
-- TeleBill – Cascade Delete / Set Null Migration
-- Run this ONCE against your TeleBill SQL Server database.
-- Open in SSMS, connect to the TeleBill DB, and Execute.
-- ============================================================

-- ── PATIENT → owned children (CASCADE DELETE) ────────────────

-- Coverage.PatientID
ALTER TABLE [Coverage] DROP CONSTRAINT [FK__Coverage__Patien__787EE5A0];
ALTER TABLE [Coverage] ADD CONSTRAINT [FK__Coverage__Patien__787EE5A0]
    FOREIGN KEY ([PatientID]) REFERENCES [Patient]([PatientID]) ON DELETE CASCADE;

-- Encounter.PatientID
ALTER TABLE [Encounter] DROP CONSTRAINT [FK__Encounter__Patie__01142BA1];
ALTER TABLE [Encounter] ADD CONSTRAINT [FK__Encounter__Patie__01142BA1]
    FOREIGN KEY ([PatientID]) REFERENCES [Patient]([PatientID]) ON DELETE CASCADE;

-- Claim.PatientID  (primary ownership — EncounterId path left as NO ACTION)
ALTER TABLE [Claim] DROP CONSTRAINT [FK__Claim__PatientID__1BC821DD];
ALTER TABLE [Claim] ADD CONSTRAINT [FK__Claim__PatientID__1BC821DD]
    FOREIGN KEY ([PatientID]) REFERENCES [Patient]([PatientID]) ON DELETE CASCADE;

-- PatientBalance.PatientID  (primary ownership — ClaimId path left as NO ACTION)
ALTER TABLE [PatientBalance] DROP CONSTRAINT [FK__PatientBa__Patie__4C6B5938];
ALTER TABLE [PatientBalance] ADD CONSTRAINT [FK__PatientBa__Patie__4C6B5938]
    FOREIGN KEY ([PatientID]) REFERENCES [Patient]([PatientID]) ON DELETE CASCADE;

-- Statement.PatientID
ALTER TABLE [Statement] DROP CONSTRAINT [FK__Statement__Patie__503BEA1C];
ALTER TABLE [Statement] ADD CONSTRAINT [FK__Statement__Patie__503BEA1C]
    FOREIGN KEY ([PatientID]) REFERENCES [Patient]([PatientID]) ON DELETE CASCADE;

-- ── COVERAGE → owned children ────────────────────────────────

ALTER TABLE [EligibilityRef] DROP CONSTRAINT [FK__Eligibili__Cover__7D439ABD];
ALTER TABLE [EligibilityRef] ADD CONSTRAINT [FK__Eligibili__Cover__7D439ABD]
    FOREIGN KEY ([CoverageID]) REFERENCES [Coverage]([CoverageID]) ON DELETE CASCADE;

-- ── ENCOUNTER → owned children ───────────────────────────────

ALTER TABLE [ChargeLine] DROP CONSTRAINT [FK__ChargeLin__Encou__06CD04F7];
ALTER TABLE [ChargeLine] ADD CONSTRAINT [FK__ChargeLin__Encou__06CD04F7]
    FOREIGN KEY ([EncounterID]) REFERENCES [Encounter]([EncounterID]) ON DELETE CASCADE;

ALTER TABLE [Diagnosis] DROP CONSTRAINT [FK__Diagnosis__Encou__10566F31];
ALTER TABLE [Diagnosis] ADD CONSTRAINT [FK__Diagnosis__Encou__10566F31]
    FOREIGN KEY ([EncounterID]) REFERENCES [Encounter]([EncounterID]) ON DELETE CASCADE;

ALTER TABLE [Attestation] DROP CONSTRAINT [FK__Attestati__Encou__0B91BA14];
ALTER TABLE [Attestation] ADD CONSTRAINT [FK__Attestati__Encou__0B91BA14]
    FOREIGN KEY ([EncounterID]) REFERENCES [Encounter]([EncounterID]) ON DELETE CASCADE;

ALTER TABLE [CodingLock] DROP CONSTRAINT [FK__CodingLoc__Encou__151B244E];
ALTER TABLE [CodingLock] ADD CONSTRAINT [FK__CodingLoc__Encou__151B244E]
    FOREIGN KEY ([EncounterID]) REFERENCES [Encounter]([EncounterID]) ON DELETE CASCADE;

-- ── CLAIM → owned children ───────────────────────────────────

ALTER TABLE [ClaimLine] DROP CONSTRAINT [FK__ClaimLine__Claim__2180FB33];
ALTER TABLE [ClaimLine] ADD CONSTRAINT [FK__ClaimLine__Claim__2180FB33]
    FOREIGN KEY ([ClaimID]) REFERENCES [Claim]([ClaimID]) ON DELETE CASCADE;

ALTER TABLE [AttachmentRef] DROP CONSTRAINT [FK__Attachmen__Claim__3587F3E0];
ALTER TABLE [AttachmentRef] ADD CONSTRAINT [FK__Attachmen__Claim__3587F3E0]
    FOREIGN KEY ([ClaimID]) REFERENCES [Claim]([ClaimID]) ON DELETE CASCADE;

-- Denial.ClaimID  (primary; ClaimLine→Denial left as NO ACTION to avoid multi-path error)
ALTER TABLE [Denial] DROP CONSTRAINT [FK__Denial__ClaimID__55009F39];
ALTER TABLE [Denial] ADD CONSTRAINT [FK__Denial__ClaimID__55009F39]
    FOREIGN KEY ([ClaimID]) REFERENCES [Claim]([ClaimID]) ON DELETE CASCADE;

-- PaymentPost.ClaimID  (primary; ClaimLine→PaymentPost left as NO ACTION)
ALTER TABLE [PaymentPost] DROP CONSTRAINT [FK__PaymentPo__Claim__46B27FE2];
ALTER TABLE [PaymentPost] ADD CONSTRAINT [FK__PaymentPo__Claim__46B27FE2]
    FOREIGN KEY ([ClaimID]) REFERENCES [Claim]([ClaimID]) ON DELETE CASCADE;

ALTER TABLE [PriorAuth] DROP CONSTRAINT [FK__PriorAuth__Claim__31B762FC];
ALTER TABLE [PriorAuth] ADD CONSTRAINT [FK__PriorAuth__Claim__31B762FC]
    FOREIGN KEY ([ClaimID]) REFERENCES [Claim]([ClaimID]) ON DELETE CASCADE;

-- ScrubIssue.ClaimID  (primary; ClaimLine→ScrubIssue left as NO ACTION)
ALTER TABLE [ScrubIssue] DROP CONSTRAINT [FK__ScrubIssu__Claim__2739D489];
ALTER TABLE [ScrubIssue] ADD CONSTRAINT [FK__ScrubIssu__Claim__2739D489]
    FOREIGN KEY ([ClaimID]) REFERENCES [Claim]([ClaimID]) ON DELETE CASCADE;

ALTER TABLE [SubmissionRef] DROP CONSTRAINT [FK__Submissio__Claim__3E1D39E1];
ALTER TABLE [SubmissionRef] ADD CONSTRAINT [FK__Submissio__Claim__3E1D39E1]
    FOREIGN KEY ([ClaimID]) REFERENCES [Claim]([ClaimID]) ON DELETE CASCADE;

ALTER TABLE [X12_837P_Ref] DROP CONSTRAINT [FK__X12_837P___Claim__2DE6D218];
ALTER TABLE [X12_837P_Ref] ADD CONSTRAINT [FK__X12_837P___Claim__2DE6D218]
    FOREIGN KEY ([ClaimID]) REFERENCES [Claim]([ClaimID]) ON DELETE CASCADE;

ALTER TABLE [ARWorkitem] DROP CONSTRAINT [FK__ARWorkite__Claim__5CA1C101];
ALTER TABLE [ARWorkitem] ADD CONSTRAINT [FK__ARWorkite__Claim__5CA1C101]
    FOREIGN KEY ([ClaimID]) REFERENCES [Claim]([ClaimID]) ON DELETE CASCADE;

-- ── DENIAL → owned children ──────────────────────────────────

ALTER TABLE [Appeal] DROP CONSTRAINT [FK__Appeal__DenialID__58D1301D];
ALTER TABLE [Appeal] ADD CONSTRAINT [FK__Appeal__DenialID__58D1301D]
    FOREIGN KEY ([DenialID]) REFERENCES [Denial]([DenialID]) ON DELETE CASCADE;

-- ── PAYER → PayerPlan → FeeSchedule ─────────────────────────

ALTER TABLE [PayerPlan] DROP CONSTRAINT [FK__PayerPlan__Payer__6C190EBB];
ALTER TABLE [PayerPlan] ADD CONSTRAINT [FK__PayerPlan__Payer__6C190EBB]
    FOREIGN KEY ([PayerID]) REFERENCES [Payer]([PayerID]) ON DELETE CASCADE;

ALTER TABLE [FeeSchedule] DROP CONSTRAINT [FK__FeeSchedu__PlanI__70DDC3D8];
ALTER TABLE [FeeSchedule] ADD CONSTRAINT [FK__FeeSchedu__PlanI__70DDC3D8]
    FOREIGN KEY ([PlanID]) REFERENCES [PayerPlan]([PlanID]) ON DELETE CASCADE;

-- ── BATCH → owned children ───────────────────────────────────

ALTER TABLE [SubmissionRef] DROP CONSTRAINT [FK__Submissio__Batch__3D2915A8];
ALTER TABLE [SubmissionRef] ADD CONSTRAINT [FK__Submissio__Batch__3D2915A8]
    FOREIGN KEY ([BatchID]) REFERENCES [SubmissionBatch]([BatchID]) ON DELETE CASCADE;

-- ── USER → Notification (CASCADE) ────────────────────────────

ALTER TABLE [Notification] DROP CONSTRAINT [FK__Notificat__UserI__6442E2C9];
ALTER TABLE [Notification] ADD CONSTRAINT [FK__Notificat__UserI__6442E2C9]
    FOREIGN KEY ([UserID]) REFERENCES [User]([UserID]) ON DELETE CASCADE;

-- ── USER → soft references (SET NULL — preserve records, clear user link) ──

ALTER TABLE [AuditLog] DROP CONSTRAINT [FK__AuditLog__UserID__60A75C0F];
ALTER TABLE [AuditLog] ADD CONSTRAINT [FK__AuditLog__UserID__60A75C0F]
    FOREIGN KEY ([UserID]) REFERENCES [User]([UserID]) ON DELETE SET NULL;

ALTER TABLE [AttachmentRef] DROP CONSTRAINT [FK__Attachmen__Uploa__367C1819];
ALTER TABLE [AttachmentRef] ADD CONSTRAINT [FK__Attachmen__Uploa__367C1819]
    FOREIGN KEY ([UploadedBy]) REFERENCES [User]([UserID]) ON DELETE SET NULL;

ALTER TABLE [PaymentPost] DROP CONSTRAINT [FK__PaymentPo__Poste__498EEC8D];
ALTER TABLE [PaymentPost] ADD CONSTRAINT [FK__PaymentPo__Poste__498EEC8D]
    FOREIGN KEY ([PostedBy]) REFERENCES [User]([UserID]) ON DELETE SET NULL;

ALTER TABLE [ARWorkitem] DROP CONSTRAINT [FK__ARWorkite__Assig__5D95E53A];
ALTER TABLE [ARWorkitem] ADD CONSTRAINT [FK__ARWorkite__Assig__5D95E53A]
    FOREIGN KEY ([AssignedTo]) REFERENCES [User]([UserID]) ON DELETE SET NULL;

ALTER TABLE [CodingLock] DROP CONSTRAINT [FK__CodingLoc__Coder__160F4887];
ALTER TABLE [CodingLock] ADD CONSTRAINT [FK__CodingLoc__Coder__160F4887]
    FOREIGN KEY ([CoderID]) REFERENCES [User]([UserID]) ON DELETE SET NULL;

-- ── PROVIDER → soft references (SET NULL — preserve clinical history) ──

ALTER TABLE [Encounter] DROP CONSTRAINT [FK__Encounter__Provi__02084FDA];
ALTER TABLE [Encounter] ADD CONSTRAINT [FK__Encounter__Provi__02084FDA]
    FOREIGN KEY ([ProviderID]) REFERENCES [Provider]([ProviderID]) ON DELETE SET NULL;

ALTER TABLE [Attestation] DROP CONSTRAINT [FK__Attestati__Provi__0C85DE4D];
ALTER TABLE [Attestation] ADD CONSTRAINT [FK__Attestati__Provi__0C85DE4D]
    FOREIGN KEY ([ProviderID]) REFERENCES [Provider]([ProviderID]) ON DELETE SET NULL;

-- ── Plan → soft references (SET NULL — keep clinical records) ──

ALTER TABLE [Coverage] DROP CONSTRAINT [FK__Coverage__PlanID__797309D9];
ALTER TABLE [Coverage] ADD CONSTRAINT [FK__Coverage__PlanID__797309D9]
    FOREIGN KEY ([PlanID]) REFERENCES [PayerPlan]([PlanID]) ON DELETE SET NULL;

ALTER TABLE [Claim] DROP CONSTRAINT [FK__Claim__PlanID__1CBC4616];
ALTER TABLE [Claim] ADD CONSTRAINT [FK__Claim__PlanID__1CBC4616]
    FOREIGN KEY ([PlanID]) REFERENCES [PayerPlan]([PlanID]) ON DELETE SET NULL;

ALTER TABLE [PriorAuth] DROP CONSTRAINT [FK__PriorAuth__PlanI__32AB8735];
ALTER TABLE [PriorAuth] ADD CONSTRAINT [FK__PriorAuth__PlanI__32AB8735]
    FOREIGN KEY ([PlanID]) REFERENCES [PayerPlan]([PlanID]) ON DELETE SET NULL;

-- ── BATCH / PAYER → RemitRef (SET NULL — keep remit records) ──

ALTER TABLE [RemitRef] DROP CONSTRAINT [FK__RemitRef__BatchI__42E1EEFE];
ALTER TABLE [RemitRef] ADD CONSTRAINT [FK__RemitRef__BatchI__42E1EEFE]
    FOREIGN KEY ([BatchID]) REFERENCES [SubmissionBatch]([BatchID]) ON DELETE SET NULL;

ALTER TABLE [RemitRef] DROP CONSTRAINT [FK__RemitRef__PayerI__41EDCAC5];
ALTER TABLE [RemitRef] ADD CONSTRAINT [FK__RemitRef__PayerI__41EDCAC5]
    FOREIGN KEY ([PayerID]) REFERENCES [Payer]([PayerID]) ON DELETE SET NULL;

-- ── ScrubRule → ScrubIssue (SET NULL — keep issue record if rule deleted) ──

ALTER TABLE [ScrubIssue] DROP CONSTRAINT [FK__ScrubIssu__RuleI__29221CFB];
ALTER TABLE [ScrubIssue] ADD CONSTRAINT [FK__ScrubIssu__RuleI__29221CFB]
    FOREIGN KEY ([RuleID]) REFERENCES [ScrubRule]([RuleID]) ON DELETE SET NULL;
