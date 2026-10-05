using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class SimulationPlanApplierTests
{
    private readonly InMemoryLoanRepository _loanRepository = new();
    private readonly InMemoryTemporaryPaymentPlanRepository _paymentPlanRepository = new();
    private readonly InMemoryCreditCardRepository _creditCardRepository = new();
    private readonly InMemoryPlannedLargeExpenseRepository _largeExpenseRepository = new();
    private readonly InMemoryRecurringIncomeRepository _recurringIncomeRepository = new();
    private readonly InMemoryAdHocIncomeRepository _adHocIncomeRepository = new();
    private readonly FakePlanChangeRecorder _changeRecorder = new();

    private readonly ScenarioPlanBuilder _planBuilder;

    public SimulationPlanApplierTests()
    {
        var loanSchedule = new LoanScheduleCalculator();
        var loanAmortization = new LoanAmortizationCalculator(loanSchedule);
        var loanScheduleBuilder = new LoanPaymentScheduleBuilder(loanSchedule, loanAmortization);
        var loanValidator = new LoanPrepaymentValidator(loanAmortization, loanScheduleBuilder);
        var installments = new InstallmentScheduleCalculator();
        _planBuilder = new ScenarioPlanBuilder(installments, loanValidator);
    }

    private SimulationPlanApplier CreateSut(TestSimulationBatchWriter? batchWriter = null)
    {
        var writer = batchWriter ?? new TestSimulationBatchWriter(
            _largeExpenseRepository,
            _paymentPlanRepository,
            _creditCardRepository,
            _loanRepository,
            _adHocIncomeRepository,
            _recurringIncomeRepository);

        return new SimulationPlanApplier(
            _planBuilder,
            writer,
            _changeRecorder);
    }

    [Fact]
    public void Constructor_NullArguments_ThrowsArgumentNullException()
    {
        var writer = new TestSimulationBatchWriter(
            _largeExpenseRepository,
            _paymentPlanRepository,
            _creditCardRepository,
            _loanRepository,
            _adHocIncomeRepository,
            _recurringIncomeRepository);

        Assert.Throws<ArgumentNullException>(() =>
            new SimulationPlanApplier(null!, writer, _changeRecorder));
        Assert.Throws<ArgumentNullException>(() =>
            new SimulationPlanApplier(_planBuilder, null!, _changeRecorder));
        Assert.Throws<ArgumentNullException>(() =>
            new SimulationPlanApplier(_planBuilder, writer, null!));
    }

    [Fact]
    public async Task ApplyAsync_BatchWriterFails_DoesNotRecordChange()
    {
        var batchWriter = new TestSimulationBatchWriter(
            _largeExpenseRepository,
            _paymentPlanRepository,
            _creditCardRepository,
            _loanRepository,
            _adHocIncomeRepository,
            _recurringIncomeRepository)
        {
            ShouldThrow = true
        };
        var sut = CreateSut(batchWriter);
        var plan = CreateBasePlan();
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.CashPurchase,
            Name = "Tadilat",
            Amount = 50_000m,
            StartDate = new DateOnly(2026, 10, 15),
            ScenarioId = Guid.NewGuid()
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.ApplyAsync(plan, [request], "trigger"));
        Assert.Empty(_changeRecorder.RecordedTriggers);
    }

    private sealed class TestSimulationBatchWriter(
        InMemoryPlannedLargeExpenseRepository largeExpenseRepo,
        InMemoryTemporaryPaymentPlanRepository paymentPlanRepo,
        InMemoryCreditCardRepository creditCardRepo,
        InMemoryLoanRepository loanRepo,
        InMemoryAdHocIncomeRepository adHocIncomeRepo,
        InMemoryRecurringIncomeRepository recurringIncomeRepo) : ISimulationBatchWriter
    {
        public SimulationPersistenceBatch? LastBatch { get; private set; }
        public bool ShouldThrow { get; set; }

        public async Task WriteBatchAsync(SimulationPersistenceBatch batch, CancellationToken cancellationToken = default)
        {
            if (ShouldThrow)
            {
                throw new InvalidOperationException("Atomic write failed");
            }

            LastBatch = batch;
            foreach (var expense in batch.LargeExpenses)
            {
                await largeExpenseRepo.UpsertPlannedLargeExpenseAsync(expense, cancellationToken);
            }

            foreach (var plan in batch.PaymentPlans)
            {
                await paymentPlanRepo.UpsertPaymentPlanAsync(plan, cancellationToken);
            }

            foreach (var card in batch.CreditCards)
            {
                await creditCardRepo.UpsertCreditCardAsync(card, cancellationToken);
            }

            foreach (var prepay in batch.LoanPrepayments)
            {
                await loanRepo.UpsertLoanPrepaymentAsync(prepay, cancellationToken);
            }

            foreach (var income in batch.AdHocIncomes)
            {
                await adHocIncomeRepo.UpsertAdHocIncomeAsync(income, cancellationToken);
            }

            foreach (var history in batch.IncomeHistories)
            {
                await recurringIncomeRepo.UpsertIncomeAmountHistoryAsync(history, cancellationToken);
            }
        }
    }

    [Fact]
    public async Task ApplyAsync_EmptyScenarioId_ThrowsInvalidOperationException()
    {
        var sut = CreateSut();
        var plan = CreateBasePlan();
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.CashPurchase,
            Name = "Tadilat",
            Amount = 10_000m,
            StartDate = new DateOnly(2026, 10, 1),
            ScenarioId = Guid.Empty
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.ApplyAsync(plan, [request], "trigger"));
    }

    [Fact]
    public async Task ApplyAsync_CashPurchase_SavesLargeExpenseAndTriggersChange()
    {
        var sut = CreateSut();
        var plan = CreateBasePlan();
        var scenarioId = Guid.NewGuid();
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.CashPurchase,
            Name = "Tadilat",
            Amount = 50_000m,
            StartDate = new DateOnly(2026, 10, 15),
            ScenarioId = scenarioId
        };

        var result = await sut.ApplyAsync(plan, [request], "Simülasyon uygulandı");

        Assert.False(result.AlreadyApplied);
        Assert.Equal(SimulationApplyDestination.Payments, result.Destination);
        Assert.Equal(scenarioId, result.EntityId);
        Assert.Equal("Plan finans planına eklendi.", result.Message);

        var expenses = await _largeExpenseRepository.GetPlannedLargeExpensesAsync();
        Assert.Single(expenses);
        Assert.Equal(scenarioId, expenses[0].Id);
        Assert.Equal(50_000m, expenses[0].Amount);
        Assert.Single(_changeRecorder.RecordedTriggers);
        Assert.Equal("Simülasyon uygulandı", _changeRecorder.RecordedTriggers[0]);
    }

    [Fact]
    public async Task ApplyAsync_AlreadyApplied_ReturnsAlreadyAppliedWithoutWriting()
    {
        var sut = CreateSut();
        var scenarioId = Guid.NewGuid();
        var plan = CreateBasePlan() with
        {
            PlannedLargeExpenses =
            [
                new PlannedLargeExpense
                {
                    Id = scenarioId,
                    Name = "Tadilat",
                    Amount = 50_000m,
                    ExactDate = new DateOnly(2026, 10, 15)
                }
            ]
        };

        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.CashPurchase,
            Name = "Tadilat",
            Amount = 50_000m,
            StartDate = new DateOnly(2026, 10, 15),
            ScenarioId = scenarioId
        };

        var result = await sut.ApplyAsync(plan, [request], "trigger");

        Assert.True(result.AlreadyApplied);
        Assert.Equal("Plan daha önce finans planına eklendi.", result.Message);
        Assert.Empty(_changeRecorder.RecordedTriggers);
    }

    [Fact]
    public async Task ApplyAsync_PartiallyApplied_ThrowsInvalidOperationException()
    {
        var sut = CreateSut();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var plan = CreateBasePlan() with
        {
            PlannedLargeExpenses =
            [
                new PlannedLargeExpense
                {
                    Id = id1,
                    Name = "Tadilat",
                    Amount = 50_000m,
                    ExactDate = new DateOnly(2026, 10, 15)
                }
            ]
        };

        var req1 = new SimulationRequest
        {
            Type = SimulationScenarioType.CashPurchase,
            Name = "Tadilat",
            Amount = 50_000m,
            StartDate = new DateOnly(2026, 10, 15),
            ScenarioId = id1
        };
        var req2 = new SimulationRequest
        {
            Type = SimulationScenarioType.CashPurchase,
            Name = "Mobilya",
            Amount = 30_000m,
            StartDate = new DateOnly(2026, 11, 15),
            ScenarioId = id2
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.ApplyAsync(plan, [req1, req2], "trigger"));
    }

    [Fact]
    public async Task ApplyAsync_IncomeChangeConflict_ThrowsInvalidOperationException()
    {
        var sut = CreateSut();
        var streamId = Guid.NewGuid();
        var plan = CreateBasePlan() with
        {
            IncomeHistories =
            [
                new IncomeAmountHistory
                {
                    RecurringIncomeId = streamId,
                    Amount = 60_000m,
                    EffectiveDate = new DateOnly(2027, 1, 1)
                }
            ]
        };

        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.IncomeChange,
            Name = "Zam",
            Amount = 70_000m,
            StartDate = new DateOnly(2027, 1, 1),
            RecurringIncomeId = streamId,
            ScenarioId = Guid.NewGuid()
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.ApplyAsync(plan, [request], "trigger"));
    }

    [Fact]
    public async Task ApplyAsync_CreditCardInstallmentPurchase_UpdatesCardCharges()
    {
        var sut = CreateSut();
        var cardId = Guid.NewGuid();
        var card = new CreditCard
        {
            Id = cardId,
            Name = "Bonus",
            Bank = "Garanti",
            Limit = 100_000m,
            StatementClosingDay = 15,
            PaymentDueDay = 25
        };
        var plan = CreateBasePlan() with { CreditCards = [card] };
        var scenarioId = Guid.NewGuid();

        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.CreditCardInstallmentPurchase,
            Name = "Bilgisayar",
            Amount = 30_000m,
            StartDate = new DateOnly(2026, 10, 5),
            PaymentCount = 3,
            CreditCardId = cardId,
            ScenarioId = scenarioId
        };

        var result = await sut.ApplyAsync(plan, [request], "Kart harcaması eklendi");

        Assert.False(result.AlreadyApplied);
        Assert.Equal(SimulationApplyDestination.CreditCard, result.Destination);
        Assert.Equal(cardId, result.EntityId);

        var cards = await _creditCardRepository.GetCreditCardsAsync();
        Assert.Single(cards);
        Assert.Equal(3, cards[0].Charges.Count);
        Assert.Contains(cards[0].Charges, c => c.Id == scenarioId);
        Assert.Equal(30_000m, cards[0].Charges.Sum(c => c.Amount));
    }

    [Fact]
    public async Task ApplyAsync_FinancingLoan_SavesPaymentPlanAndAdHocIncome()
    {
        var sut = CreateSut();
        var plan = CreateBasePlan();
        var scenarioId = Guid.NewGuid();

        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.FinancingLoan,
            Name = "İhtiyaç Kredisi",
            Amount = 100_000m,
            StartDate = new DateOnly(2026, 10, 1),
            PaymentCount = 10,
            FirstPaymentDate = new DateOnly(2026, 11, 1),
            TotalRepaymentAmount = 130_000m,
            ScenarioId = scenarioId
        };

        var result = await sut.ApplyAsync(plan, [request], "Finansman eklendi");

        Assert.False(result.AlreadyApplied);
        Assert.Equal(SimulationApplyDestination.Payments, result.Destination);

        var paymentPlans = await _paymentPlanRepository.GetPaymentPlansAsync();
        Assert.Single(paymentPlans);
        Assert.Equal(scenarioId, paymentPlans[0].Id);
        Assert.Equal(10, paymentPlans[0].Installments.Count);
        Assert.Equal(130_000m, paymentPlans[0].Installments.Sum(x => x.Amount));

        var incomes = await _adHocIncomeRepository.GetAdHocIncomesAsync();
        Assert.Single(incomes);
        Assert.Equal(scenarioId, incomes[0].Id);
        Assert.Equal(100_000m, incomes[0].Amount);
    }

    [Fact]
    public async Task ApplyAsync_MultipleRequests_PersistsAllAndReturnsCountMessage()
    {
        var sut = CreateSut();
        var plan = CreateBasePlan();
        var cashId = Guid.NewGuid();
        var incomeId = Guid.NewGuid();

        var req1 = new SimulationRequest
        {
            Type = SimulationScenarioType.CashPurchase,
            Name = "Mobilya",
            Amount = 20_000m,
            StartDate = new DateOnly(2026, 10, 10),
            ScenarioId = cashId
        };
        var req2 = new SimulationRequest
        {
            Type = SimulationScenarioType.FutureIncome,
            Name = "Prim",
            Amount = 15_000m,
            StartDate = new DateOnly(2026, 10, 20),
            ScenarioId = incomeId
        };

        var result = await sut.ApplyAsync(plan, [req1, req2], "Toplu simülasyon");

        Assert.False(result.AlreadyApplied);
        Assert.Equal("2 koşul finans planına eklendi.", result.Message);

        var expenses = await _largeExpenseRepository.GetPlannedLargeExpensesAsync();
        Assert.Single(expenses);
        Assert.Equal(cashId, expenses[0].Id);

        var incomes = await _adHocIncomeRepository.GetAdHocIncomesAsync();
        Assert.Single(incomes);
        Assert.Equal(incomeId, incomes[0].Id);
    }

    [Fact]
    public async Task ApplyAsync_LoanEarlyClosure_SavesLoanPrepayment()
    {
        var sut = CreateSut();
        var loanId = Guid.NewGuid();
        var loan = new Loan
        {
            Id = loanId,
            Name = "Taşıt",
            Bank = "İş",
            MonthlyPayment = 10_000m,
            PaymentDay = 1,
            NextPaymentDate = new DateOnly(2026, 11, 1),
            RemainingInstallmentCount = 20,
            RemainingDebt = 150_000m
        };
        var plan = CreateBasePlan() with { Loans = [loan] };
        var scenarioId = Guid.NewGuid();

        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.LoanEarlyClosure,
            Name = "Kredi Kapat",
            Amount = 0m,
            StartDate = new DateOnly(2026, 11, 1),
            LoanId = loanId,
            ScenarioId = scenarioId
        };

        var result = await sut.ApplyAsync(plan, [request], "Erken kapama");

        Assert.False(result.AlreadyApplied);
        Assert.Equal(SimulationApplyDestination.Payments, result.Destination);

        var prepayments = await _loanRepository.GetLoanPrepaymentsAsync();
        Assert.Single(prepayments);
        Assert.Equal(scenarioId, prepayments[0].Id);
        Assert.Equal(loanId, prepayments[0].LoanId);
        Assert.Equal(LoanPrepaymentMode.FullClosure, prepayments[0].Mode);
    }

    [Fact]
    public async Task ApplyAsync_CashDebt_SavesTemporaryPaymentPlan()
    {
        var sut = CreateSut();
        var plan = CreateBasePlan();
        var scenarioId = Guid.NewGuid();

        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.CashDebt,
            Name = "Borç",
            Amount = 40_000m,
            StartDate = new DateOnly(2026, 10, 1),
            PaymentCount = 4,
            FirstPaymentDate = new DateOnly(2026, 11, 1),
            ScenarioId = scenarioId
        };

        var result = await sut.ApplyAsync(plan, [request], "Borç eklendi");

        Assert.False(result.AlreadyApplied);
        Assert.Equal(SimulationApplyDestination.Payments, result.Destination);

        var plans = await _paymentPlanRepository.GetPaymentPlansAsync();
        Assert.Single(plans);
        Assert.Equal(scenarioId, plans[0].Id);
        Assert.Equal(4, plans[0].Installments.Count);
        Assert.Equal(40_000m, plans[0].Installments.Sum(x => x.Amount));
    }

    [Fact]
    public async Task ApplyAsync_IncomeChange_SavesIncomeAmountHistory()
    {
        var sut = CreateSut();
        var streamId = Guid.NewGuid();
        var plan = CreateBasePlan();
        var scenarioId = Guid.NewGuid();

        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.IncomeChange,
            Name = "Gelir Artışı",
            Amount = 65_000m,
            StartDate = new DateOnly(2027, 1, 1),
            RecurringIncomeId = streamId,
            ScenarioId = scenarioId
        };

        var result = await sut.ApplyAsync(plan, [request], "Zam uygulandı");

        Assert.False(result.AlreadyApplied);
        Assert.Equal(SimulationApplyDestination.IncomeHistory, result.Destination);

        var histories = await _recurringIncomeRepository.GetIncomeAmountHistoriesAsync();
        Assert.Single(histories);
        Assert.Equal(streamId, histories[0].RecurringIncomeId);
        Assert.Equal(65_000m, histories[0].Amount);
        Assert.Equal(new DateOnly(2027, 1, 1), histories[0].EffectiveDate);
    }

    private static FinancialPlan CreateBasePlan() => new()
    {
        Settings = new UserSettings
        {
            PeriodAnchor = new PeriodAnchor(15),
            ProjectionAnchorDate = new DateOnly(2026, 9, 15),
            ProjectionOpeningBalance = 10_000m
        }
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
}
