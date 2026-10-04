-- Uydurma bir kullanicinin v17 verisi: iki gelir tutari, bir kredi, bir kart, kapanmis ve acik donem plani.
-- Yetim satirlar (karsi kaydi olmayan cocuklar) eski semada yabanci anahtar olmadigi icin bilerek var.
INSERT INTO settings (Id, SalaryDay, MonthlyLivingBudget, ProjectionStartingSavings, ProjectionAnchorDate,
    CreditCardCarryInterestRate, DeficitFinancingInterestRate, PaymentAssignmentMode, SchemaVersion,
    DevelopmentSeedVersion, GamificationEnabled, DevelopmentSeedEnabled, TrackingStartedDate, PaymentReminderMode)
VALUES (1, 15, 12000, 50000, '2026-09-15', 4.25, 5.5, 0, 17, 0, 0, 0, NULL, 2);

INSERT INTO salary_schedule (Id, NetAmount, EffectiveFrom, Note) VALUES
    ('00000000-0000-4000-8000-000000000a01', 40000, '2026-01-01', ''),
    ('00000000-0000-4000-8000-000000000a02', 50000, '2026-07-01', 'Zam');
INSERT INTO other_incomes (Id, Amount, ExactDate, Description) VALUES
    ('00000000-0000-4000-8000-000000000b01', 7500, '2026-10-02', 'Ikramiye');

INSERT INTO loans (Id, Name, Bank, MonthlyInstallment, PaymentDay, StartDate, EndDate, InstallmentCount, RemainingDebt,
    EarlyClosureAmount, IsActive, FinalPaymentAmount, EarlyClosureAmountAsOf, Kind) VALUES
    ('00000000-0000-4000-8000-000000000c01', 'Ihtiyac Kredisi', 'Is Bankasi', 5000, 20, '2026-10-20', '2027-09-20', 12, 55000,
     52000, 1, 5000, '2026-09-15', 0);
INSERT INTO loan_prepayments (Id, LoanId, Date, Mode, PrincipalAmount) VALUES
    ('00000000-0000-4000-8000-000000000c11', '00000000-0000-4000-8000-000000000c01', '2026-09-01', 0, 3000),
    ('00000000-0000-4000-8000-000000000c12', '00000000-0000-4000-8000-00000000dead', '2026-09-02', 0, 999);

INSERT INTO credit_cards (Id, Name, Bank, "Limit", CurrentTotalDebt, LastStatementDebt, LastStatementRemaining,
    CurrentCycleSpending, StatementClosingDay, PaymentDueDay, MinimumPaymentRate, PaymentMode, ManualPaymentAmount,
    CarriedBalance, UnbilledSpending, BalanceAsOfDate, StatementModelVersion, PaymentStrategy, FixedPaymentAmount,
    ProjectionFallbackStrategy, ProjectionFallbackFixedAmount, KnownNextStatementDate, KnownNextDueDate) VALUES
    ('00000000-0000-4000-8000-000000000d01', 'Bonus', 'Garanti', 60000, 9000, 6000, 6000,
     3000, 5, 15, 0.4, 0, 0, 6000, 3000, '2026-09-15', 2, 0, 0, 0, 0, '2026-10-05', '2026-10-15');
INSERT INTO card_installments (Id, CreditCardId, Description, DueDate, Amount) VALUES
    ('00000000-0000-4000-8000-000000000d11', '00000000-0000-4000-8000-000000000d01', 'Telefon', '2026-09-10', 1200),
    ('00000000-0000-4000-8000-000000000d12', '00000000-0000-4000-8000-00000000dead', 'Yetim', '2026-09-11', 1);

INSERT INTO financial_snapshots (Id, SnapshotDate, ProjectionAnchorDate, NextReviewDate, ProjectionStartingSavings,
    SalaryDay, PreviousSnapshotId, Source, IsCurrent, CreatedAtUtc, Note) VALUES
    ('00000000-0000-4000-8000-000000000e01', '2026-09-15', '2026-09-15', '2026-10-15', 50000, 15, NULL, 0, 1,
     '2026-09-15T08:00:00.0000000+00:00', '');

-- Kapanmis donem: gelir donem basinda yatiyor (StrategyUsed 0).
-- Acik donem: gelir donem sonunda yatiyor (StrategyUsed 1).
-- Geliri sifir olan plan: gelir satiri turetilmez. Yetim plan: anlik goruntusu yok.
INSERT INTO period_plan_snapshots (Id, FinancialSnapshotId, PeriodStart, PeriodEnd, ReviewAvailableFrom, CreatedAtUtc,
    StrategyUsed, PaymentWindowStart, PaymentWindowEnd, OpeningSavings, PlannedIncome, PlannedLoanPayments,
    PlannedCardPayments, PlannedTemporaryPayments, PlannedInstallmentPayments, PlannedOtherScheduledPayments,
    PlannedMandatoryPayments, PlannedLivingBudget, PlannedLargeExpenses, PlannedCardInterest, PlannedDeficitInterest,
    PlannedEndingSavings) VALUES
    ('00000000-0000-4000-8000-000000000f01', '00000000-0000-4000-8000-000000000e01', '2026-08-15', '2026-09-15',
     '2026-09-15', '2026-08-15T08:00:00.0000000+00:00', 0, '2026-08-15', '2026-09-15', 40000, 40000, 5000, 6000, 0, 0, 0,
     11000, 12000, 0, 0, 0, 57000),
    ('00000000-0000-4000-8000-000000000f02', '00000000-0000-4000-8000-000000000e01', '2026-09-15', '2026-10-15',
     '2026-10-15', '2026-09-15T08:00:00.0000000+00:00', 1, '2026-09-15', '2026-10-15', 50000, 50000, 5000, 6000, 0, 0, 0,
     11000, 12000, 0, 0, 0, 77000),
    ('00000000-0000-4000-8000-000000000f03', '00000000-0000-4000-8000-000000000e01', '2026-10-15', '2026-11-15',
     '2026-11-15', '2026-10-15T08:00:00.0000000+00:00', 0, '2026-10-15', '2026-11-15', 0, 0, 0, 0, 0, 0, 0,
     0, 0, 0, 0, 0, 0),
    ('00000000-0000-4000-8000-000000000f04', '00000000-0000-4000-8000-00000000dead', '2026-07-15', '2026-08-15',
     '2026-08-15', '2026-07-15T08:00:00.0000000+00:00', 0, '2026-07-15', '2026-08-15', 1, 1, 0, 0, 0, 0, 0,
     0, 0, 0, 0, 0, 1);

INSERT INTO period_plan_payment_lines (Id, PeriodPlanSnapshotId, SourceEntityId, SourceType, Name, PlannedDate,
    PlannedAmount, IsEstimate, Detail) VALUES
    ('00000000-0000-4000-8000-000000001001', '00000000-0000-4000-8000-000000000f02', '00000000-0000-4000-8000-000000000c01', 1,
     'Ihtiyac Kredisi', '2026-10-20', 5000, 0, ''),
    ('00000000-0000-4000-8000-000000001002', '00000000-0000-4000-8000-000000000f01', '00000000-0000-4000-8000-000000000c01', 1,
     'Ihtiyac Kredisi', '2026-09-20', 5000, 0, '');

INSERT INTO period_plan_revisions (Id, PeriodPlanSnapshotId, RevisionNumber, CreatedAtUtc, "Trigger", StrategyUsed,
    PlannedIncome, PlannedLoanPayments, PlannedCardPayments, PlannedTemporaryPayments, PlannedInstallmentPayments,
    PlannedOtherScheduledPayments, PlannedMandatoryPayments, PlannedLivingBudget, PlannedLargeExpenses,
    PlannedCardInterest, PlannedDeficitInterest, PlannedInterest, PlannedEndingSavings, Note) VALUES
    ('00000000-0000-4000-8000-000000001101', '00000000-0000-4000-8000-000000000f02', 1, '2026-09-20T08:00:00.0000000+00:00',
     'Kredi', 1, 50000, 5000, 6000, 0, 0, 0, 11000, 12000, 0, 0, 0, 0, 77000, '');
INSERT INTO period_plan_revision_payment_lines (Id, PeriodPlanRevisionId, SourceEntityId, SourceType, Name, PlannedDate,
    PlannedAmount, IsEstimate, Detail) VALUES
    ('00000000-0000-4000-8000-000000001111', '00000000-0000-4000-8000-000000001101', '00000000-0000-4000-8000-000000000c01', 1,
     'Ihtiyac Kredisi', '2026-10-20', 5000, 0, '');

INSERT INTO period_actuals (Id, PeriodPlanSnapshotId, SourceFinancialSnapshotId, ResultFinancialSnapshotId, PeriodStart,
    PeriodEnd, FinalizedAtUtc, ActualIncome, ActualLoanPayments, ActualCardPayments, ActualTemporaryPayments,
    ActualInstallmentPayments, ActualOtherScheduledPayments, ActualLargeExpenses, ActualMandatoryPayments,
    ActualLivingSpend, ActualInterest, UnplannedIncome, UnplannedPayments, DerivedEndingSavings, ConfirmedEndingSavings,
    ReconciliationAdjustment, ComparisonSummary, Note) VALUES
    ('00000000-0000-4000-8000-000000001201', '00000000-0000-4000-8000-000000000f01', '00000000-0000-4000-8000-000000000e01',
     '00000000-0000-4000-8000-000000000e01', '2026-08-15', '2026-09-15', '2026-09-15T09:00:00.0000000+00:00',
     40000, 5000, 6000, 0, 0, 0, 0, 11000, 11500, 0, 0, 0, 57500, 57500, 0, '', '');
INSERT INTO actual_payments (Id, PeriodActualId, PeriodPlanPaymentLineId, SourceEntityId, SourceType, Name, PlannedDate,
    PlannedAmount, ActualPaymentDate, ActualAmount, Status, Note) VALUES
    ('00000000-0000-4000-8000-000000001211', '00000000-0000-4000-8000-000000001201', '00000000-0000-4000-8000-000000001002', '00000000-0000-4000-8000-000000000c01',
     1, 'Ihtiyac Kredisi', '2026-09-20', 5000, '2026-09-20', 5000, 1, '');
INSERT INTO actual_flows (Id, PeriodActualId, Type, Name, Category, Date, Amount) VALUES
    ('00000000-0000-4000-8000-000000001221', '00000000-0000-4000-8000-000000001201', 1, 'Market', 'Gida', '2026-09-01', 800);
INSERT INTO actual_living_breakdowns (Id, PeriodActualId, Category, Amount) VALUES
    ('00000000-0000-4000-8000-000000001231', '00000000-0000-4000-8000-000000001201', 'Gida', 4000);

-- Acik donemin bakiye gozlemi ve bir odeme isareti.
INSERT INTO period_observations (Id, PeriodPlanSnapshotId, ObservedOn, ObservedBalance, ObservedLivingSpend, Note,
    CreatedAtUtc, UpdatedAtUtc) VALUES
    ('00000000-0000-4000-8000-000000001301', '00000000-0000-4000-8000-000000000f02', '2026-09-25', 48000, 2000, '',
     '2026-09-25T10:00:00.0000000+00:00', '2026-09-25T10:00:00.0000000+00:00');
INSERT INTO period_observation_payments (Id, PeriodObservationId, PeriodPlanPaymentLineId, Status, ActualAmount,
    ActualPaymentDate, Note) VALUES
    ('00000000-0000-4000-8000-000000001311', '00000000-0000-4000-8000-000000001301', '00000000-0000-4000-8000-000000001001',
     1, 5000, '2026-09-20', '');

INSERT INTO simulation_drafts (Id, Name, CreatedAt, UpdatedAt) VALUES
    ('00000000-0000-4000-8000-000000001401', 'Deneme', '2026-09-20T10:00:00.0000000+00:00', '2026-09-20T10:00:00.0000000+00:00');
INSERT INTO simulation_draft_conditions (Id, DraftId, Position, IsEnabled, Type, Name, Amount, StartDate, PaymentCount,
    AppliesToAllStatements) VALUES
    ('00000000-0000-4000-8000-000000001411', '00000000-0000-4000-8000-000000001401', 0, 1, 1, 'Yeni harcama', 1000,
     '2026-10-01', 1, 0);
INSERT INTO payment_reminder_responses (DueKey, Name, DueDate, Amount, Kind, AnsweredAt, SnoozedUntil) VALUES
    ('kredi-2026-09-20', 'Ihtiyac Kredisi', '2026-09-20', 5000, 1, '2026-09-19T09:00:00.0000000+00:00', NULL);
