-- Eski uygulamanin (com.coinflow.mobile) sema v17 veritabaninin DDL dokumu; yalniz sema, kullanici verisi yok.
CREATE TABLE IF NOT EXISTS "salary_schedule" (
"Id" varchar primary key not null ,
"NetAmount" float ,
"EffectiveFrom" varchar ,
"Note" varchar );
CREATE TABLE IF NOT EXISTS "other_incomes" (
"Id" varchar primary key not null ,
"Amount" float ,
"ExactDate" varchar ,
"Description" varchar );
CREATE TABLE IF NOT EXISTS "loans" (
"Id" varchar primary key not null ,
"Name" varchar ,
"Bank" varchar ,
"MonthlyInstallment" float ,
"PaymentDay" integer ,
"StartDate" varchar ,
"EndDate" varchar ,
"InstallmentCount" integer ,
"RemainingDebt" float ,
"EarlyClosureAmount" float ,
"IsActive" integer , "FinalPaymentAmount" float, "EarlyClosureAmountAsOf" varchar, "Kind" integer);
CREATE TABLE IF NOT EXISTS "payment_plans" (
"Id" varchar primary key not null ,
"Name" varchar ,
"Kind" integer ,
"OriginalAmount" float ,
"TotalRepaymentAmount" float );
CREATE TABLE IF NOT EXISTS "payment_installments" (
"Id" varchar primary key not null ,
"PlanId" varchar ,
"DueDate" varchar ,
"Amount" float ,
"IsPaid" integer );
CREATE TABLE IF NOT EXISTS "credit_cards" (
"Id" varchar primary key not null ,
"Name" varchar ,
"Bank" varchar ,
"Limit" float ,
"CurrentTotalDebt" float ,
"LastStatementDebt" float ,
"LastStatementRemaining" float ,
"CurrentCycleSpending" float ,
"StatementClosingDay" integer ,
"PaymentDueDay" integer ,
"MinimumPaymentRate" float ,
"PaymentMode" integer ,
"ManualPaymentAmount" float ,
"CarriedBalance" float ,
"UnbilledSpending" float ,
"BalanceAsOfDate" varchar ,
"StatementModelVersion" integer ,
"PaymentStrategy" integer ,
"FixedPaymentAmount" float ,
"ProjectionFallbackStrategy" integer ,
"ProjectionFallbackFixedAmount" float ,
"KnownNextStatementDate" varchar ,
"KnownNextDueDate" varchar );
CREATE TABLE IF NOT EXISTS "card_installments" (
"Id" varchar primary key not null ,
"CreditCardId" varchar ,
"Description" varchar ,
"DueDate" varchar ,
"Amount" float );
CREATE TABLE IF NOT EXISTS "credit_card_payment_plans" (
"Id" varchar primary key not null ,
"CreditCardId" varchar ,
"DueDate" varchar ,
"PlannedPaymentAmount" float ,
"PaymentType" integer ,
"Amount" float );
CREATE TABLE IF NOT EXISTS "credit_card_payment_preferences" (
"Id" varchar primary key not null ,
"CreditCardId" varchar ,
"Mode" integer ,
"CustomAmount" float ,
"EffectiveFromStatementDate" varchar ,
"CreatedAt" varchar ,
"Note" varchar );
CREATE TABLE IF NOT EXISTS "credit_card_statements" (
"Id" varchar primary key not null ,
"CreditCardId" varchar ,
"StatementDate" varchar ,
"DueDate" varchar ,
"StatementAmount" float ,
"MinimumPaymentAmount" float ,
"NextStatementDate" varchar ,
"NextDueDate" varchar ,
"Source" integer ,
"SourceDocumentFingerprint" varchar ,
"ImportedAt" varchar ,
"CreatedAt" varchar ,
"UpdatedAt" varchar ,
"CurrentPaymentMode" integer ,
"CurrentPaymentCustomAmount" float );
CREATE TABLE IF NOT EXISTS "planned_large_expenses" (
"Id" varchar primary key not null ,
"Name" varchar ,
"Amount" float ,
"ExactDate" varchar ,
"Note" varchar ,
"Status" integer );
CREATE TABLE IF NOT EXISTS "settings" (
"Id" integer primary key not null ,
"SalaryDay" integer ,
"MonthlyLivingBudget" float ,
"ProjectionStartingSavings" float ,
"ProjectionAnchorDate" varchar ,
"CreditCardCarryInterestRate" float ,
"DeficitFinancingInterestRate" float ,
"PaymentAssignmentMode" integer ,
"SchemaVersion" integer ,
"DevelopmentSeedVersion" integer ,
"GamificationEnabled" integer ,
"DevelopmentSeedEnabled" integer ,
"TrackingStartedDate" varchar , "PaymentReminderMode" integer);
CREATE TABLE IF NOT EXISTS "payment_assignment_strategies" (
"Id" varchar primary key not null ,
"Mode" integer ,
"EffectiveFromSalaryDate" varchar ,
"CreatedAt" varchar ,
"Note" varchar );
CREATE TABLE IF NOT EXISTS "financial_snapshots" (
"Id" varchar primary key not null ,
"SnapshotDate" varchar ,
"ProjectionAnchorDate" varchar ,
"NextReviewDate" varchar ,
"ProjectionStartingSavings" float ,
"SalaryDay" integer ,
"PreviousSnapshotId" varchar ,
"Source" integer ,
"IsCurrent" integer ,
"CreatedAtUtc" varchar ,
"Note" varchar );
CREATE TABLE IF NOT EXISTS "period_plan_snapshots" (
"Id" varchar primary key not null ,
"FinancialSnapshotId" varchar ,
"PeriodStart" varchar ,
"PeriodEnd" varchar ,
"ReviewAvailableFrom" varchar ,
"CreatedAtUtc" varchar ,
"StrategyUsed" integer ,
"PaymentWindowStart" varchar ,
"PaymentWindowEnd" varchar ,
"OpeningSavings" float ,
"PlannedIncome" float ,
"PlannedLoanPayments" float ,
"PlannedCardPayments" float ,
"PlannedTemporaryPayments" float ,
"PlannedInstallmentPayments" float ,
"PlannedOtherScheduledPayments" float ,
"PlannedMandatoryPayments" float ,
"PlannedLivingBudget" float ,
"PlannedLargeExpenses" float ,
"PlannedCardInterest" float ,
"PlannedDeficitInterest" float ,
"PlannedEndingSavings" float );
CREATE TABLE IF NOT EXISTS "period_plan_payment_lines" (
"Id" varchar primary key not null ,
"PeriodPlanSnapshotId" varchar ,
"SourceEntityId" varchar ,
"SourceType" integer ,
"Name" varchar ,
"PlannedDate" varchar ,
"PlannedAmount" float ,
"IsEstimate" integer ,
"Detail" varchar );
CREATE TABLE IF NOT EXISTS "period_plan_revisions" (
"Id" varchar primary key not null ,
"PeriodPlanSnapshotId" varchar ,
"RevisionNumber" integer ,
"CreatedAtUtc" varchar ,
"Trigger" varchar ,
"StrategyUsed" integer ,
"PlannedIncome" float ,
"PlannedLoanPayments" float ,
"PlannedCardPayments" float ,
"PlannedTemporaryPayments" float ,
"PlannedInstallmentPayments" float ,
"PlannedOtherScheduledPayments" float ,
"PlannedMandatoryPayments" float ,
"PlannedLivingBudget" float ,
"PlannedLargeExpenses" float ,
"PlannedCardInterest" float ,
"PlannedDeficitInterest" float ,
"PlannedInterest" float ,
"PlannedEndingSavings" float ,
"Note" varchar );
CREATE TABLE IF NOT EXISTS "period_plan_revision_payment_lines" (
"Id" varchar primary key not null ,
"PeriodPlanRevisionId" varchar ,
"SourceEntityId" varchar ,
"SourceType" integer ,
"Name" varchar ,
"PlannedDate" varchar ,
"PlannedAmount" float ,
"IsEstimate" integer ,
"Detail" varchar );
CREATE TABLE IF NOT EXISTS "period_actuals" (
"Id" varchar primary key not null ,
"PeriodPlanSnapshotId" varchar ,
"SourceFinancialSnapshotId" varchar ,
"ResultFinancialSnapshotId" varchar ,
"PeriodStart" varchar ,
"PeriodEnd" varchar ,
"FinalizedAtUtc" varchar ,
"ActualIncome" float ,
"ActualLoanPayments" float ,
"ActualCardPayments" float ,
"ActualTemporaryPayments" float ,
"ActualInstallmentPayments" float ,
"ActualOtherScheduledPayments" float ,
"ActualLargeExpenses" float ,
"ActualMandatoryPayments" float ,
"ActualLivingSpend" float ,
"ActualInterest" float ,
"UnplannedIncome" float ,
"UnplannedPayments" float ,
"DerivedEndingSavings" float ,
"ConfirmedEndingSavings" float ,
"ReconciliationAdjustment" float ,
"ComparisonSummary" varchar ,
"Note" varchar );
CREATE TABLE IF NOT EXISTS "actual_payments" (
"Id" varchar primary key not null ,
"PeriodActualId" varchar ,
"PeriodPlanPaymentLineId" varchar ,
"SourceEntityId" varchar ,
"SourceType" integer ,
"Name" varchar ,
"PlannedDate" varchar ,
"PlannedAmount" float ,
"ActualPaymentDate" varchar ,
"ActualAmount" float ,
"Status" integer ,
"Note" varchar );
CREATE TABLE IF NOT EXISTS "actual_flows" (
"Id" varchar primary key not null ,
"PeriodActualId" varchar ,
"Type" integer ,
"Name" varchar ,
"Category" varchar ,
"Date" varchar ,
"Amount" float );
CREATE TABLE IF NOT EXISTS "actual_living_breakdowns" (
"Id" varchar primary key not null ,
"PeriodActualId" varchar ,
"Category" varchar ,
"Amount" float );
CREATE TABLE IF NOT EXISTS "simulation_drafts" (
"Id" varchar primary key not null ,
"Name" varchar ,
"CreatedAt" varchar ,
"UpdatedAt" varchar );
CREATE TABLE IF NOT EXISTS "simulation_draft_conditions" (
"Id" varchar primary key not null ,
"DraftId" varchar ,
"Position" integer ,
"IsEnabled" integer ,
"Type" integer ,
"Name" varchar ,
"Amount" float ,
"StartDate" varchar ,
"PaymentCount" integer ,
"FirstPaymentDate" varchar ,
"CreditCardId" varchar ,
"TotalRepaymentAmount" float ,
"NewPaymentAssignmentMode" integer ,
"EffectiveSalaryDate" varchar ,
"CardPaymentType" integer ,
"AppliesToAllStatements" integer , "LoanId" varchar, "PrepaymentMode" integer);
CREATE TABLE IF NOT EXISTS "period_observations" (
"Id" varchar primary key not null ,
"PeriodPlanSnapshotId" varchar ,
"ObservedOn" varchar ,
"ObservedBalance" float ,
"ObservedLivingSpend" float ,
"Note" varchar ,
"CreatedAtUtc" varchar ,
"UpdatedAtUtc" varchar );
CREATE TABLE IF NOT EXISTS "period_observation_payments" (
"Id" varchar primary key not null ,
"PeriodObservationId" varchar ,
"PeriodPlanPaymentLineId" varchar ,
"Status" integer ,
"ActualAmount" float ,
"ActualPaymentDate" varchar ,
"Note" varchar );
CREATE TABLE IF NOT EXISTS "period_observation_flows" (
"Id" varchar primary key not null ,
"PeriodObservationId" varchar ,
"Type" integer ,
"Name" varchar ,
"Category" varchar ,
"Date" varchar ,
"Amount" float );
CREATE TABLE IF NOT EXISTS "loan_prepayments" (
"Id" varchar primary key not null ,
"LoanId" varchar ,
"Date" varchar ,
"Mode" integer ,
"PrincipalAmount" float );
CREATE TABLE IF NOT EXISTS "payment_reminder_responses" (
"DueKey" varchar primary key not null ,
"Name" varchar ,
"DueDate" varchar ,
"Amount" float ,
"Kind" integer ,
"AnsweredAt" varchar ,
"SnoozedUntil" varchar );
CREATE INDEX "salary_schedule_EffectiveFrom" on "salary_schedule"("EffectiveFrom");
CREATE INDEX "other_incomes_ExactDate" on "other_incomes"("ExactDate");
CREATE INDEX "payment_installments_PlanId" on "payment_installments"("PlanId");
CREATE INDEX "payment_installments_DueDate" on "payment_installments"("DueDate");
CREATE INDEX "card_installments_CreditCardId" on "card_installments"("CreditCardId");
CREATE INDEX "card_installments_DueDate" on "card_installments"("DueDate");
CREATE INDEX "credit_card_payment_plans_CreditCardId" on "credit_card_payment_plans"("CreditCardId");
CREATE INDEX "credit_card_payment_plans_DueDate" on "credit_card_payment_plans"("DueDate");
CREATE INDEX "credit_card_payment_preferences_CreditCardId" on "credit_card_payment_preferences"("CreditCardId");
CREATE INDEX "credit_card_payment_preferences_EffectiveFromStatementDate" on "credit_card_payment_preferences"("EffectiveFromStatementDate");
CREATE INDEX "credit_card_statements_CreditCardId" on "credit_card_statements"("CreditCardId");
CREATE INDEX "credit_card_statements_StatementDate" on "credit_card_statements"("StatementDate");
CREATE INDEX "planned_large_expenses_ExactDate" on "planned_large_expenses"("ExactDate");
CREATE UNIQUE INDEX "payment_assignment_strategies_EffectiveFromSalaryDate" on "payment_assignment_strategies"("EffectiveFromSalaryDate");
CREATE INDEX "financial_snapshots_SnapshotDate" on "financial_snapshots"("SnapshotDate");
CREATE INDEX "financial_snapshots_NextReviewDate" on "financial_snapshots"("NextReviewDate");
CREATE INDEX "financial_snapshots_IsCurrent" on "financial_snapshots"("IsCurrent");
CREATE INDEX "period_plan_snapshots_FinancialSnapshotId" on "period_plan_snapshots"("FinancialSnapshotId");
CREATE INDEX "period_plan_snapshots_PeriodStart" on "period_plan_snapshots"("PeriodStart");
CREATE INDEX "period_plan_snapshots_ReviewAvailableFrom" on "period_plan_snapshots"("ReviewAvailableFrom");
CREATE INDEX "period_plan_payment_lines_PeriodPlanSnapshotId" on "period_plan_payment_lines"("PeriodPlanSnapshotId");
CREATE INDEX "period_plan_revisions_PeriodPlanSnapshotId" on "period_plan_revisions"("PeriodPlanSnapshotId");
CREATE INDEX "period_plan_revision_payment_lines_PeriodPlanRevisionId" on "period_plan_revision_payment_lines"("PeriodPlanRevisionId");
CREATE UNIQUE INDEX "period_actuals_PeriodPlanSnapshotId" on "period_actuals"("PeriodPlanSnapshotId");
CREATE INDEX "period_actuals_PeriodStart" on "period_actuals"("PeriodStart");
CREATE INDEX "actual_payments_PeriodActualId" on "actual_payments"("PeriodActualId");
CREATE INDEX "actual_flows_PeriodActualId" on "actual_flows"("PeriodActualId");
CREATE INDEX "actual_living_breakdowns_PeriodActualId" on "actual_living_breakdowns"("PeriodActualId");
CREATE INDEX "simulation_draft_conditions_DraftId" on "simulation_draft_conditions"("DraftId");
CREATE INDEX "period_observations_PeriodPlanSnapshotId" on "period_observations"("PeriodPlanSnapshotId");
CREATE INDEX "period_observation_payments_PeriodObservationId" on "period_observation_payments"("PeriodObservationId");
CREATE INDEX "period_observation_flows_PeriodObservationId" on "period_observation_flows"("PeriodObservationId");
CREATE INDEX "loan_prepayments_LoanId" on "loan_prepayments"("LoanId");
