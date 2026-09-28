using Mizan.Application.Abstractions;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class ObligationManagementServiceTests
{
    private readonly InMemoryLoanRepository _loanRepository = new();
    private readonly InMemoryTemporaryPaymentPlanRepository _paymentPlanRepository = new();
    private readonly InMemoryPlannedLargeExpenseRepository _largeExpenseRepository = new();
    private readonly FakePlanChangeRecorder _changeRecorder = new();
    private readonly TestClock _clock = new(
        new DateOnly(2026, 9, 25),
        new DateTimeOffset(2026, 9, 25, 10, 0, 0, TimeSpan.Zero));
    private readonly LoanPayoffService _loanPayoffService;

    public ObligationManagementServiceTests()
    {
        var scheduleCalculator = new LoanScheduleCalculator();
        var amortizationCalculator = new LoanAmortizationCalculator(scheduleCalculator);
        var scheduleBuilder = new LoanPaymentScheduleBuilder(scheduleCalculator, amortizationCalculator);
        _loanPayoffService = new LoanPayoffService(
            _clock, amortizationCalculator, scheduleBuilder, new LoanPrepaymentValidator(amortizationCalculator, scheduleBuilder));
    }

    private ObligationManagementService CreateSut() =>
        new(
            _loanRepository,
            _paymentPlanRepository,
            _largeExpenseRepository,
            _loanPayoffService,
            _changeRecorder);

    [Fact]
    public void Constructor_NullArguments_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ObligationManagementService(null!, _paymentPlanRepository, _largeExpenseRepository, _loanPayoffService, _changeRecorder));
        Assert.Throws<ArgumentNullException>(() =>
            new ObligationManagementService(_loanRepository, null!, _largeExpenseRepository, _loanPayoffService, _changeRecorder));
        Assert.Throws<ArgumentNullException>(() =>
            new ObligationManagementService(_loanRepository, _paymentPlanRepository, null!, _loanPayoffService, _changeRecorder));
        Assert.Throws<ArgumentNullException>(() =>
            new ObligationManagementService(_loanRepository, _paymentPlanRepository, _largeExpenseRepository, null!, _changeRecorder));
        Assert.Throws<ArgumentNullException>(() =>
            new ObligationManagementService(_loanRepository, _paymentPlanRepository, _largeExpenseRepository, _loanPayoffService, null!));
    }

    [Fact]
    public async Task SaveLoanAsync_ValidLoan_SavesAndTriggersChange()
    {
        var sut = CreateSut();
        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            Name = "İhtiyaç Kredisi",
            MonthlyPayment = 5_000m,
            PaymentDay = 15,
            NextPaymentDate = new DateOnly(2026, 10, 15),
            RemainingInstallmentCount = 12
        };

        await sut.SaveLoanAsync(loan, []);

        var savedLoans = await _loanRepository.GetLoansAsync();
        Assert.Single(savedLoans);
        Assert.Equal(loan.Id, savedLoans[0].Id);
        Assert.Single(_changeRecorder.RecordedTriggers);
        Assert.Equal("Kredi planı değişti", _changeRecorder.RecordedTriggers[0]);
    }

    [Fact]
    public async Task SaveLoanAsync_WithEarlyClosureAmount_PreparesForSaveAndUpdatesRemainingDebt()
    {
        var sut = CreateSut();
        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            Name = "Konut Kredisi",
            MonthlyPayment = 10_000m,
            PaymentDay = 15,
            NextPaymentDate = new DateOnly(2026, 10, 15),
            RemainingInstallmentCount = 10,
            EarlyClosureAmount = 80_000m,
            EarlyClosureAmountAsOf = new DateOnly(2026, 9, 25)
        };

        await sut.SaveLoanAsync(loan, []);

        var savedLoans = await _loanRepository.GetLoansAsync();
        Assert.Single(savedLoans);
        Assert.NotNull(savedLoans[0].RemainingDebt);
        Assert.True(savedLoans[0].RemainingDebt > 0m);
        Assert.Single(_changeRecorder.RecordedTriggers);
        Assert.Equal("Kredi planı değişti", _changeRecorder.RecordedTriggers[0]);
    }

    [Fact]
    public async Task PreviewLoan_KaydetmedenKaydinKurallariylaCozer()
    {
        var sut = CreateSut();
        var loan = TenInstallmentLoan() with { EarlyClosureAmount = 80_000m };

        var overview = sut.PreviewLoan(loan);

        Assert.NotNull(overview);
        Assert.Equal(LoanRateSource.BankQuote, overview.Analysis.Amortization!.Source);
        Assert.Equal(new DateOnly(2026, 9, 25), overview.Loan.EarlyClosureAmountAsOf);
        Assert.Empty(await _loanRepository.GetLoansAsync());
        Assert.Empty(_changeRecorder.RecordedTriggers);
    }

    [Fact]
    public void PreviewLoan_KayitReddedecekse_NullDoner()
    {
        var sut = CreateSut();

        var overview = sut.PreviewLoan(TenInstallmentLoan() with { RemainingDebt = 80_000m, EarlyClosureAmount = 150_000m });

        Assert.Null(overview);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-500)]
    public async Task SaveLoanAsync_MonthlyPaymentZeroOrNegative_ThrowsInvalidOperationException(decimal payment)
    {
        var sut = CreateSut();
        var loan = new Loan
        {
            MonthlyPayment = payment,
            PaymentDay = 15,
            NextPaymentDate = new DateOnly(2026, 10, 15),
            RemainingInstallmentCount = 12
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SaveLoanAsync(loan, []));
        Assert.Empty(await _loanRepository.GetLoansAsync());
        Assert.Empty(_changeRecorder.RecordedTriggers);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SaveLoanAsync_RemainingInstallmentCountLessThanOne_ThrowsInvalidOperationException(int count)
    {
        var sut = CreateSut();
        var loan = new Loan
        {
            MonthlyPayment = 5_000m,
            PaymentDay = 15,
            NextPaymentDate = new DateOnly(2026, 10, 15),
            RemainingInstallmentCount = count
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SaveLoanAsync(loan, []));
        Assert.Empty(await _loanRepository.GetLoansAsync());
        Assert.Empty(_changeRecorder.RecordedTriggers);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    public async Task SaveLoanAsync_InvalidPaymentDay_ThrowsArgumentOutOfRangeException(int day)
    {
        var sut = CreateSut();
        var loan = new Loan
        {
            MonthlyPayment = 5_000m,
            PaymentDay = day,
            NextPaymentDate = new DateOnly(2026, 10, 15),
            RemainingInstallmentCount = 12
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sut.SaveLoanAsync(loan, []));
        Assert.Empty(await _loanRepository.GetLoansAsync());
        Assert.Empty(_changeRecorder.RecordedTriggers);
    }

    [Fact]
    public async Task DeleteLoanAsync_ValidId_DeletesAndTriggersChange()
    {
        var sut = CreateSut();
        var loanId = Guid.NewGuid();
        await _loanRepository.UpsertLoanAsync(new Loan
        {
            Id = loanId,
            MonthlyPayment = 5_000m,
            PaymentDay = 15,
            NextPaymentDate = new DateOnly(2026, 10, 15),
            RemainingInstallmentCount = 12
        });

        await sut.DeleteLoanAsync(loanId);

        Assert.Empty(await _loanRepository.GetLoansAsync());
        Assert.Single(_changeRecorder.RecordedTriggers);
        Assert.Equal("Kredi planı değişti", _changeRecorder.RecordedTriggers[0]);
    }

    [Fact]
    public async Task SavePaymentPlanAsync_ValidPlan_NormalizesSavesAndTriggersChange()
    {
        var sut = CreateSut();
        var planId = Guid.NewGuid();
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Mobilya Taksiti",
            Kind = PaymentPlanKind.Installment,
            Installments =
            [
                new TemporaryPaymentInstallment
                {
                    PlanId = Guid.Empty, // Normalize edilmeli
                    DueDate = new DateOnly(2026, 11, 20),
                    Amount = 3_000m
                },
                new TemporaryPaymentInstallment
                {
                    PlanId = Guid.Empty,
                    DueDate = new DateOnly(2026, 10, 20),
                    Amount = 3_000m
                }
            ]
        };

        await sut.SavePaymentPlanAsync(plan);

        var savedPlans = await _paymentPlanRepository.GetPaymentPlansAsync();
        Assert.Single(savedPlans);
        var saved = savedPlans[0];
        Assert.Equal(planId, saved.Id);
        Assert.All(saved.Installments, i => Assert.Equal(planId, i.PlanId));
        Assert.Equal(new DateOnly(2026, 10, 20), saved.Installments[0].DueDate);
        Assert.Equal(new DateOnly(2026, 11, 20), saved.Installments[1].DueDate);
        Assert.Single(_changeRecorder.RecordedTriggers);
        Assert.Equal("Planlı ödeme değişti", _changeRecorder.RecordedTriggers[0]);
    }

    [Fact]
    public async Task SavePaymentPlanAsync_EmptyInstallments_ThrowsInvalidOperationException()
    {
        var sut = CreateSut();
        var plan = new TemporaryPaymentPlan
        {
            Id = Guid.NewGuid(),
            Name = "Boş Plan",
            Installments = []
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SavePaymentPlanAsync(plan));
        Assert.Empty(await _paymentPlanRepository.GetPaymentPlansAsync());
        Assert.Empty(_changeRecorder.RecordedTriggers);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task SavePaymentPlanAsync_InstallmentWithZeroOrNegativeAmount_ThrowsInvalidOperationException(decimal amount)
    {
        var sut = CreateSut();
        var plan = new TemporaryPaymentPlan
        {
            Id = Guid.NewGuid(),
            Name = "Geçersiz Plan",
            Installments =
            [
                new TemporaryPaymentInstallment
                {
                    DueDate = new DateOnly(2026, 10, 20),
                    Amount = amount
                }
            ]
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SavePaymentPlanAsync(plan));
        Assert.Empty(await _paymentPlanRepository.GetPaymentPlansAsync());
        Assert.Empty(_changeRecorder.RecordedTriggers);
    }

    [Fact]
    public async Task DeletePaymentPlanAsync_ValidId_DeletesAndTriggersChange()
    {
        var sut = CreateSut();
        var planId = Guid.NewGuid();
        await _paymentPlanRepository.UpsertPaymentPlanAsync(new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Silinecek Plan",
            Installments =
            [
                new TemporaryPaymentInstallment
                {
                    PlanId = planId,
                    DueDate = new DateOnly(2026, 10, 20),
                    Amount = 1_000m
                }
            ]
        });

        await sut.DeletePaymentPlanAsync(planId);

        Assert.Empty(await _paymentPlanRepository.GetPaymentPlansAsync());
        Assert.Single(_changeRecorder.RecordedTriggers);
        Assert.Equal("Planlı ödeme değişti", _changeRecorder.RecordedTriggers[0]);
    }

    [Fact]
    public async Task SavePlannedLargeExpenseAsync_ValidExpense_SavesAndTriggersChange()
    {
        var sut = CreateSut();
        var expense = new PlannedLargeExpense
        {
            Id = Guid.NewGuid(),
            Name = "Sigorta Primi",
            Amount = 15_000m,
            ExactDate = new DateOnly(2026, 12, 1)
        };

        await sut.SavePlannedLargeExpenseAsync(expense);

        var saved = await _largeExpenseRepository.GetPlannedLargeExpensesAsync();
        Assert.Single(saved);
        Assert.Equal(expense.Id, saved[0].Id);
        Assert.Single(_changeRecorder.RecordedTriggers);
        Assert.Equal("Büyük ödeme planı değişti", _changeRecorder.RecordedTriggers[0]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-500)]
    public async Task SavePlannedLargeExpenseAsync_ZeroOrNegativeAmount_ThrowsInvalidOperationException(decimal amount)
    {
        var sut = CreateSut();
        var expense = new PlannedLargeExpense
        {
            Id = Guid.NewGuid(),
            Name = "Geçersiz Harcama",
            Amount = amount,
            ExactDate = new DateOnly(2026, 12, 1)
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SavePlannedLargeExpenseAsync(expense));
        Assert.Empty(await _largeExpenseRepository.GetPlannedLargeExpensesAsync());
        Assert.Empty(_changeRecorder.RecordedTriggers);
    }

    [Fact]
    public async Task DeletePlannedLargeExpenseAsync_ValidId_DeletesAndTriggersChange()
    {
        var sut = CreateSut();
        var id = Guid.NewGuid();
        await _largeExpenseRepository.UpsertPlannedLargeExpenseAsync(new PlannedLargeExpense
        {
            Id = id,
            Name = "Silinecek Harcama",
            Amount = 10_000m,
            ExactDate = new DateOnly(2026, 12, 1)
        });

        await sut.DeletePlannedLargeExpenseAsync(id);

        Assert.Empty(await _largeExpenseRepository.GetPlannedLargeExpensesAsync());
        Assert.Single(_changeRecorder.RecordedTriggers);
        Assert.Equal("Büyük ödeme planı değişti", _changeRecorder.RecordedTriggers[0]);
    }

    private static Loan TenInstallmentLoan() => new()
    {
        Name = "Konut Kredisi",
        MonthlyPayment = 10_000m,
        PaymentDay = 15,
        NextPaymentDate = new DateOnly(2026, 10, 15),
        RemainingInstallmentCount = 10
    };

    private sealed class FakePlanChangeRecorder : IPlanChangeRecorder
    {
        public List<string> RecordedTriggers { get; } = [];

        public Task RecordChangeAsync(string trigger, CancellationToken cancellationToken = default)
        {
            RecordedTriggers.Add(trigger);
            return Task.CompletedTask;
        }
    }

    private sealed class TestClock(DateOnly today, DateTimeOffset utcNow) : IClock
    {
        public DateOnly Today => today;
        public DateTimeOffset UtcNow => utcNow;
    }
}
