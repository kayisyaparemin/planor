using Mizan.Application.Services;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Kart kontrol testlerinin ortak düzeneği: sahte depo üstünde <b>gerçek</b> kart servisi. Böylece
/// kaydedilen karar, yeniden yüklemede alan kurallarından (<c>CreditCardValidator</c>, karar
/// çözümleyici) geçmiş hâliyle görünür. Kart: kesim 15, son ödeme 25, bakiye tarihi 1 Eylül 2026,
/// asgari oran %20, faiz ayarı varsayılan %5.
/// </summary>
internal sealed class CardControlFixture
{
    public static readonly DateOnly StatementDate = new(2026, 9, 15);
    public static readonly DateOnly DueDate = new(2026, 9, 25);

    public FakeCreditCardRepository Repository { get; } = new();
    public FakeDialogService Dialog { get; } = new();
    public CardControlViewModel ViewModel { get; }

    public CardControlFixture()
    {
        var service = new CreditCardObligationService(
            Repository, new SabitSaat(new DateOnly(2026, 9, 27)), new CreditCardPaymentPreferenceResolver(), new FakePlanChangeRecorder());
        ViewModel = new CardControlViewModel(
            Repository, service, new FakeUserSettingsRepository(), new CreditCardStatementCalculator(), Dialog);
    }

    public CreditCard Add(CreditCard card)
    {
        Repository.Cards[card.Id] = card;
        return card;
    }

    public CreditCard Stored(CreditCard card) => Repository.Cards[card.Id];

    public static CreditCardStatement Statement(decimal amount, decimal minimum) => new()
    {
        StatementDate = StatementDate, DueDate = DueDate, StatementAmount = amount, MinimumPaymentAmount = minimum
    };

    public static CreditCard Card(
        string name, decimal carried = 0m, CreditCardStatement? statement = null,
        CurrentStatementPaymentMode? plan = null, decimal? custom = null,
        CreditCardPaymentStrategy strategy = CreditCardPaymentStrategy.FullStatement,
        ProjectionFallbackStrategy fallback = ProjectionFallbackStrategy.Minimum,
        decimal limit = 50000m)
    {
        var id = Guid.NewGuid();
        return new CreditCard
        {
            Id = id, Name = name, Bank = "Garanti BBVA", Limit = limit, CarriedBalance = carried,
            BalanceAsOfDate = new DateOnly(2026, 9, 1), StatementClosingDay = 15, PaymentDueDay = 25,
            MinimumPaymentRate = 0.20m, PaymentStrategy = strategy, ProjectionFallbackStrategy = fallback,
            CurrentStatement = statement is null ? null : statement with { CreditCardId = id },
            CurrentStatementPaymentPlan = plan is null ? null : new CurrentStatementPaymentPlan { Mode = plan.Value, CustomAmount = custom }
        };
    }
}
