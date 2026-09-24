using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

/// <summary>
/// <see cref="FinancialInstrumentReconciliationService"/> sınıfının dönem kapanışında
/// fiili ödemeleri kredilere, kartlara, vadeli planlara ve büyük harcamalara doğru uyguladığını
/// ve invariant'ları (I4, I11, I13, I17) koruduğunu doğrulayan test kalkanı.
/// </summary>
public sealed class FinancialInstrumentReconciliationServiceTests
{
    private readonly FinancialInstrumentReconciliationService _sut;
    private static readonly DateOnly NewAnchor = new(2026, 9, 20);

    public FinancialInstrumentReconciliationServiceTests()
    {
        var scheduleCalculator = new LoanScheduleCalculator();
        var loanCalculator = new LoanAmortizationCalculator(scheduleCalculator);
        var loanScheduleBuilder = new LoanPaymentScheduleBuilder(scheduleCalculator, loanCalculator);
        var cardStatementCalculator = new CreditCardStatementCalculator();
        var cardReconciler = new CreditCardActualPaymentReconciler();
        var loanReconciler = new LoanInstrumentReconciler(loanCalculator, loanScheduleBuilder);

        _sut = new FinancialInstrumentReconciliationService(
            cardReconciler,
            cardStatementCalculator,
            loanReconciler);
    }

    [Fact]
    public void Apply_WhenLoanInstallmentPaid_DecreasesRemainingCount_AndReducesOnlyPrincipalPortion()
    {
        var loan = CreateSampleLoan(
            remainingDebt: 100_000m,
            monthlyPayment: 10_000m,
            remainingInstallments: 12,
            paymentDay: 15,
            nextPaymentDate: new DateOnly(2026, 9, 15));

        var plan = CreatePlan(loans: [loan]);
        var line = CreatePaymentLine(loan.Id, PlanPaymentSourceType.Loan, new DateOnly(2026, 9, 15), 10_000m);
        var actual = CreateActualPayment(line.Id, PlanPaymentSourceType.Loan, 10_000m, ActualPaymentStatus.Paid);

        var result = _sut.Apply(plan, [line], [actual], NewAnchor);

        var updatedLoan = Assert.Single(result.Loans);
        Assert.Equal(11, updatedLoan.RemainingInstallmentCount);
        Assert.True(updatedLoan.IsActive);
        Assert.Equal(new DateOnly(2026, 10, 15), updatedLoan.NextPaymentDate);
        // I17: Taksitin tamamı (10.000) değil, yalnızca anapara payı düşmeli. Kalan anapara 90.000'den büyük olmalı!
        Assert.NotNull(updatedLoan.RemainingDebt);
        Assert.True(updatedLoan.RemainingDebt.Value > 90_000m, "Taksitin tamamı değil yalnız anapara payı düşmeli.");
        Assert.True(updatedLoan.RemainingDebt.Value < 100_000m, "Anapara azalmış olmalı.");
    }

    [Fact]
    public void Apply_WhenLoanInstallmentUnpaid_CarriesNextPaymentDateToNewAnchor()
    {
        var loan = CreateSampleLoan(
            remainingDebt: 100_000m,
            monthlyPayment: 10_000m,
            remainingInstallments: 12,
            paymentDay: 15,
            nextPaymentDate: new DateOnly(2026, 9, 15));

        var plan = CreatePlan(loans: [loan]);
        var line = CreatePaymentLine(loan.Id, PlanPaymentSourceType.Loan, new DateOnly(2026, 9, 15), 10_000m);
        var actual = CreateActualPayment(line.Id, PlanPaymentSourceType.Loan, 0m, ActualPaymentStatus.Unpaid);

        var result = _sut.Apply(plan, [line], [actual], NewAnchor);

        var updatedLoan = Assert.Single(result.Loans);
        Assert.Equal(12, updatedLoan.RemainingInstallmentCount);
        Assert.Equal(100_000m, updatedLoan.RemainingDebt);
        Assert.True(updatedLoan.IsActive);
        // I4: Ödenmeyen kredi yeni dönemin ilk gününe (NewAnchor) devredilir.
        Assert.Equal(NewAnchor, updatedLoan.NextPaymentDate);
    }

    [Fact]
    public void Apply_WhenLastLoanInstallmentPaid_ClosesLoanAndSetsDebtToZero()
    {
        var loan = CreateSampleLoan(
            remainingDebt: 5_000m,
            monthlyPayment: 5_000m,
            remainingInstallments: 1,
            paymentDay: 15,
            nextPaymentDate: new DateOnly(2026, 9, 15));

        var plan = CreatePlan(loans: [loan]);
        var line = CreatePaymentLine(loan.Id, PlanPaymentSourceType.Loan, new DateOnly(2026, 9, 15), 5_000m);
        var actual = CreateActualPayment(line.Id, PlanPaymentSourceType.Loan, 5_000m, ActualPaymentStatus.Paid);

        var result = _sut.Apply(plan, [line], [actual], NewAnchor);

        var updatedLoan = Assert.Single(result.Loans);
        Assert.Equal(0, updatedLoan.RemainingInstallmentCount);
        Assert.Equal(0m, updatedLoan.RemainingDebt);
        Assert.False(updatedLoan.IsActive);
    }

    [Fact]
    public void Apply_WhenLoanPrepaymentPaid_ReplaysLoanAndConsumesPrepayment()
    {
        var loan = CreateSampleLoan(
            remainingDebt: 100_000m,
            monthlyPayment: 10_000m,
            remainingInstallments: 12,
            paymentDay: 15,
            nextPaymentDate: new DateOnly(2026, 9, 15));

        var prepayment = new LoanPrepayment
        {
            Id = Guid.NewGuid(),
            LoanId = loan.Id,
            Date = new DateOnly(2026, 9, 10),
            Mode = LoanPrepaymentMode.FullClosure,
            PrincipalAmount = null
        };

        var plan = CreatePlan(loans: [loan], prepayments: [prepayment]);
        var line = CreatePaymentLine(prepayment.Id, PlanPaymentSourceType.Loan, new DateOnly(2026, 9, 10), 105_000m);
        var actual = CreateActualPayment(line.Id, PlanPaymentSourceType.Loan, 105_000m, ActualPaymentStatus.Paid);

        var result = _sut.Apply(plan, [line], [actual], NewAnchor);

        var updatedLoan = Assert.Single(result.Loans);
        Assert.False(updatedLoan.IsActive);
        Assert.Equal(0, updatedLoan.RemainingInstallmentCount);
        Assert.Contains(prepayment.Id, result.RemovedLoanPrepaymentIds);
    }

    [Fact]
    public void Apply_WhenLoanPrepaymentUnpaid_CancelsPrepaymentAndLeavesLoanUnchanged()
    {
        var loan = CreateSampleLoan(
            remainingDebt: 100_000m,
            monthlyPayment: 10_000m,
            remainingInstallments: 12,
            paymentDay: 15,
            nextPaymentDate: new DateOnly(2026, 9, 15));

        var prepayment = new LoanPrepayment
        {
            Id = Guid.NewGuid(),
            LoanId = loan.Id,
            Date = new DateOnly(2026, 9, 10),
            Mode = LoanPrepaymentMode.FullClosure,
            PrincipalAmount = null
        };

        var plan = CreatePlan(loans: [loan], prepayments: [prepayment]);
        var line = CreatePaymentLine(prepayment.Id, PlanPaymentSourceType.Loan, new DateOnly(2026, 9, 10), 105_000m);
        var actual = CreateActualPayment(line.Id, PlanPaymentSourceType.Loan, 0m, ActualPaymentStatus.Unpaid);

        var result = _sut.Apply(plan, [line], [actual], NewAnchor);

        var updatedLoan = Assert.Single(result.Loans);
        Assert.True(updatedLoan.IsActive);
        Assert.Equal(12, updatedLoan.RemainingInstallmentCount);
        Assert.Contains(prepayment.Id, result.RemovedLoanPrepaymentIds);
    }

    [Fact]
    public void Apply_WhenPrepaymentIsStale_RemovesItEvenIfNotInPaymentLines()
    {
        var loan = CreateSampleLoan();
        var stalePrepayment = new LoanPrepayment
        {
            Id = Guid.NewGuid(),
            LoanId = loan.Id,
            Date = NewAnchor.AddDays(-2),
            Mode = LoanPrepaymentMode.FullClosure
        };

        var plan = CreatePlan(loans: [loan], prepayments: [stalePrepayment]);

        var result = _sut.Apply(plan, [], [], NewAnchor);

        Assert.Contains(stalePrepayment.Id, result.RemovedLoanPrepaymentIds);
    }

    [Fact]
    public void Apply_WhenTemporaryPaymentPlanInstallmentPaid_MarksInstallmentPaid()
    {
        var planId = Guid.NewGuid();
        var installmentId = Guid.NewGuid();
        var installment = new TemporaryPaymentInstallment
        {
            Id = installmentId,
            PlanId = planId,
            DueDate = new DateOnly(2026, 9, 12),
            Amount = 2_500m,
            IsPaid = false
        };
        var paymentPlan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Mobilya Taksiti",
            Installments = [installment]
        };

        var plan = CreatePlan(paymentPlans: [paymentPlan]);
        var line = CreatePaymentLine(installmentId, PlanPaymentSourceType.TemporaryPayment, new DateOnly(2026, 9, 12), 2_500m);
        var actual = CreateActualPayment(line.Id, PlanPaymentSourceType.TemporaryPayment, 2_500m, ActualPaymentStatus.Paid);

        var result = _sut.Apply(plan, [line], [actual], NewAnchor);

        var updatedPlan = Assert.Single(result.PaymentPlans);
        var updatedInstallment = Assert.Single(updatedPlan.Installments);
        Assert.True(updatedInstallment.IsPaid);
    }

    [Fact]
    public void Apply_WhenTemporaryPaymentPlanInstallmentUnpaid_CarriesDueDateToNewAnchor()
    {
        var planId = Guid.NewGuid();
        var installmentId = Guid.NewGuid();
        var installment = new TemporaryPaymentInstallment
        {
            Id = installmentId,
            PlanId = planId,
            DueDate = new DateOnly(2026, 9, 12),
            Amount = 2_500m,
            IsPaid = false
        };
        var paymentPlan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Mobilya Taksiti",
            Installments = [installment]
        };

        var plan = CreatePlan(paymentPlans: [paymentPlan]);
        var line = CreatePaymentLine(installmentId, PlanPaymentSourceType.TemporaryPayment, new DateOnly(2026, 9, 12), 2_500m);
        var actual = CreateActualPayment(line.Id, PlanPaymentSourceType.TemporaryPayment, 0m, ActualPaymentStatus.Unpaid);

        var result = _sut.Apply(plan, [line], [actual], NewAnchor);

        var updatedPlan = Assert.Single(result.PaymentPlans);
        var updatedInstallment = Assert.Single(updatedPlan.Installments);
        Assert.False(updatedInstallment.IsPaid);
        Assert.Equal(NewAnchor, updatedInstallment.DueDate);
    }

    [Fact]
    public void Apply_WhenCreditCardPaid_UpdatesCarriedBalanceAndCleansCurrentStatement()
    {
        var cardId = Guid.NewGuid();
        var dueDate = new DateOnly(2026, 9, 7);
        var card = new CreditCard
        {
            Id = cardId,
            Name = "Bonus",
            StatementClosingDay = 27,
            PaymentDueDay = 7,
            Limit = 100_000m,
            CarriedBalance = 20_000m,
            BalanceAsOfDate = new DateOnly(2026, 8, 28),
            CurrentStatement = new CreditCardStatement
            {
                StatementDate = new DateOnly(2026, 8, 27),
                DueDate = dueDate,
                StatementAmount = 30_000m,
                MinimumPaymentAmount = 6_000m
            }
        };

        var plan = CreatePlan(cards: [card]);
        var line = CreatePaymentLine(cardId, PlanPaymentSourceType.CreditCard, dueDate, 30_000m);
        var actual = CreateActualPayment(line.Id, PlanPaymentSourceType.CreditCard, 30_000m, ActualPaymentStatus.Paid);

        var result = _sut.Apply(plan, [line], [actual], NewAnchor);

        var updatedCard = Assert.Single(result.CreditCards);
        Assert.Equal(0m, updatedCard.CarriedBalance);
        Assert.Null(updatedCard.CurrentStatement);
    }

    [Fact]
    public void Apply_WhenPlannedLargeExpensePaid_MarksStatusCompleted()
    {
        var expense = new PlannedLargeExpense
        {
            Id = Guid.NewGuid(),
            Name = "Beyaz Eşya",
            Amount = 45_000m,
            ExactDate = new DateOnly(2026, 9, 14),
            Status = PlannedExpenseStatus.Planned
        };

        var plan = CreatePlan(expenses: [expense]);
        var line = CreatePaymentLine(expense.Id, PlanPaymentSourceType.PlannedLargeExpense, new DateOnly(2026, 9, 14), 45_000m);
        var actual = CreateActualPayment(line.Id, PlanPaymentSourceType.PlannedLargeExpense, 45_000m, ActualPaymentStatus.Paid);

        var result = _sut.Apply(plan, [line], [actual], NewAnchor);

        var updatedExpense = Assert.Single(result.LargeExpenses);
        Assert.Equal(PlannedExpenseStatus.Completed, updatedExpense.Status);
    }

    [Fact]
    public void Apply_WhenPlannedLargeExpenseUnpaid_CarriesDateToNewAnchor()
    {
        var expense = new PlannedLargeExpense
        {
            Id = Guid.NewGuid(),
            Name = "Beyaz Eşya",
            Amount = 45_000m,
            ExactDate = new DateOnly(2026, 9, 14),
            Status = PlannedExpenseStatus.Planned
        };

        var plan = CreatePlan(expenses: [expense]);
        var line = CreatePaymentLine(expense.Id, PlanPaymentSourceType.PlannedLargeExpense, new DateOnly(2026, 9, 14), 45_000m);
        var actual = CreateActualPayment(line.Id, PlanPaymentSourceType.PlannedLargeExpense, 0m, ActualPaymentStatus.Unpaid);

        var result = _sut.Apply(plan, [line], [actual], NewAnchor);

        var updatedExpense = Assert.Single(result.LargeExpenses);
        Assert.Equal(PlannedExpenseStatus.Planned, updatedExpense.Status);
        Assert.Equal(NewAnchor, updatedExpense.ExactDate);
    }

    [Fact]
    public void Apply_WhenSameDayHasInstallmentAndPrepayment_ProcessesInstallmentFirst()
    {
        var loan = CreateSampleLoan(
            remainingDebt: 100_000m,
            monthlyPayment: 10_000m,
            remainingInstallments: 12,
            paymentDay: 15,
            nextPaymentDate: new DateOnly(2026, 9, 15));

        var prepayment = new LoanPrepayment
        {
            Id = Guid.NewGuid(),
            LoanId = loan.Id,
            Date = new DateOnly(2026, 9, 15),
            Mode = LoanPrepaymentMode.FullClosure,
            PrincipalAmount = null
        };

        var plan = CreatePlan(loans: [loan], prepayments: [prepayment]);
        var installmentLine = CreatePaymentLine(loan.Id, PlanPaymentSourceType.Loan, new DateOnly(2026, 9, 15), 10_000m);
        var prepaymentLine = CreatePaymentLine(prepayment.Id, PlanPaymentSourceType.Loan, new DateOnly(2026, 9, 15), 95_000m);

        // actualPayments listesinde bilerek önce erken ödeme, sonra taksit veriyoruz;
        // servis bunu PlannedDate ve prepayment mantığı ile doğru sıralamalı!
        var actualPrepayment = CreateActualPayment(prepaymentLine.Id, PlanPaymentSourceType.Loan, 95_000m, ActualPaymentStatus.Paid);
        var actualInstallment = CreateActualPayment(installmentLine.Id, PlanPaymentSourceType.Loan, 10_000m, ActualPaymentStatus.Paid);

        var result = _sut.Apply(plan, [installmentLine, prepaymentLine], [actualPrepayment, actualInstallment], NewAnchor);

        var updatedLoan = Assert.Single(result.Loans);
        Assert.False(updatedLoan.IsActive);
        Assert.Equal(0, updatedLoan.RemainingInstallmentCount);
        Assert.Contains(prepayment.Id, result.RemovedLoanPrepaymentIds);
    }

    [Fact]
    public void Apply_WhenLoanHasStaleEarlyClosureQuote_ClearsClosureQuote()
    {
        var loan = CreateSampleLoan(
            remainingDebt: 100_000m,
            monthlyPayment: 10_000m,
            remainingInstallments: 12,
            paymentDay: 15,
            nextPaymentDate: new DateOnly(2026, 9, 15)) with
        {
            EarlyClosureAmount = 85_000m,
            EarlyClosureAmountAsOf = new DateOnly(2026, 8, 1) // 8. ayın 1'i, PreviousDueDate olan 15 Ağustos'tan eski!
        };

        var plan = CreatePlan(loans: [loan]);
        var line = CreatePaymentLine(loan.Id, PlanPaymentSourceType.Loan, new DateOnly(2026, 9, 15), 10_000m);
        var actual = CreateActualPayment(line.Id, PlanPaymentSourceType.Loan, 10_000m, ActualPaymentStatus.Paid);

        var result = _sut.Apply(plan, [line], [actual], NewAnchor);

        var updatedLoan = Assert.Single(result.Loans);
        Assert.Null(updatedLoan.EarlyClosureAmount);
        Assert.Null(updatedLoan.EarlyClosureAmountAsOf);
    }

    [Fact]
    public void Apply_WhenArgumentsNull_ThrowsArgumentNullException()
    {
        var plan = CreatePlan();
        Assert.Throws<ArgumentNullException>(() => _sut.Apply(null!, [], [], NewAnchor));
        Assert.Throws<ArgumentNullException>(() => _sut.Apply(plan, null!, [], NewAnchor));
        Assert.Throws<ArgumentNullException>(() => _sut.Apply(plan, [], null!, NewAnchor));
    }

    [Fact]
    public void Apply_WhenPaymentLineNotFound_ThrowsInvalidOperationException()
    {
        var plan = CreatePlan();
        var missingLineId = Guid.NewGuid();
        var actual = CreateActualPayment(missingLineId, PlanPaymentSourceType.Loan, 1_000m, ActualPaymentStatus.Paid);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            _sut.Apply(plan, [], [actual], NewAnchor));

        Assert.Equal("Planlanan ödeme satırı bulunamadı.", ex.Message);
    }

    private static Loan CreateSampleLoan(
        decimal remainingDebt = 100_000m,
        decimal monthlyPayment = 10_000m,
        int remainingInstallments = 12,
        int paymentDay = 15,
        DateOnly? nextPaymentDate = null) => new()
    {
        Id = Guid.NewGuid(),
        Bank = "Garanti BBVA",
        Name = "İhtiyaç Kredisi",
        Kind = LoanKind.Consumer,
        MonthlyPayment = monthlyPayment,
        RemainingInstallmentCount = remainingInstallments,
        RemainingDebt = remainingDebt,
        PaymentDay = paymentDay,
        NextPaymentDate = nextPaymentDate ?? new DateOnly(2026, 9, 15),
        IsActive = true
    };

    private static FinancialPlan CreatePlan(
        IReadOnlyList<Loan>? loans = null,
        IReadOnlyList<TemporaryPaymentPlan>? paymentPlans = null,
        IReadOnlyList<CreditCard>? cards = null,
        IReadOnlyList<PlannedLargeExpense>? expenses = null,
        IReadOnlyList<LoanPrepayment>? prepayments = null) => new()
    {
        Settings = new UserSettings
        {
            PeriodAnchor = new PeriodAnchor(20),
            ProjectionOpeningBalance = 50_000m,
            ProjectionAnchorDate = new DateOnly(2026, 8, 20),
            CreditCardCarryInterestRate = 0.0425m
        },
        RecurringIncomes =
        [
            new RecurringIncome
            {
                Id = Guid.NewGuid(),
                Name = "Maaş",
                PaymentDay = 20,
                IsActive = true
            }
        ],
        IncomeHistories = [],
        AdHocIncomes = [],
        Loans = loans ?? [],
        CreditCards = cards ?? [],
        PaymentPlans = paymentPlans ?? [],
        PlannedLargeExpenses = expenses ?? [],
        LoanPrepayments = prepayments ?? []
    };

    private static PeriodPlanPaymentLine CreatePaymentLine(
        Guid sourceEntityId,
        PlanPaymentSourceType sourceType,
        DateOnly plannedDate,
        decimal plannedAmount) => new()
    {
        Id = Guid.NewGuid(),
        PeriodPlanSnapshotId = Guid.NewGuid(),
        SourceEntityId = sourceEntityId,
        SourceType = sourceType,
        Name = "Test Kalemi",
        PlannedDate = plannedDate,
        PlannedAmount = plannedAmount
    };

    private static ActualPayment CreateActualPayment(
        Guid lineId,
        PlanPaymentSourceType sourceType,
        decimal actualAmount,
        ActualPaymentStatus status) => new()
    {
        Id = Guid.NewGuid(),
        PeriodPlanPaymentLineId = lineId,
        SourceType = sourceType,
        ActualAmount = actualAmount,
        Status = status,
        ActualPaymentDate = NewAnchor
    };
}
