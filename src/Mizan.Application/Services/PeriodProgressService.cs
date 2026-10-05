using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Ana sayfanın mevcut dönem verisini (<see cref="PeriodProgress"/>) okuyan ince kabuk: açık dönemin
/// defterini, kullanıcı ayarlarını ve kartları dar portlardan okur, kartların bugünkü hâliyle bu dönemde
/// vadesi gelen ödemesini bulur ve hesabı <see cref="PeriodProgressCalculator"/>'a bırakır.
/// Eskide tanrı arayüz <c>IMizanStore</c>'a bağlıydı (T10); hiçbir şey yazmaz.
/// </summary>
public sealed class PeriodProgressService(
    OpenPeriodLedgerReader ledgerReader,
    IUserSettingsRepository userSettingsRepository,
    ICreditCardRepository creditCardRepository,
    CreditCardStatementCalculator cardStatementCalculator,
    IClock clock) : IPeriodProgressService
{
    private readonly OpenPeriodLedgerReader _ledgerReader =
        ledgerReader ?? throw new ArgumentNullException(nameof(ledgerReader));
    private readonly IUserSettingsRepository _userSettingsRepository =
        userSettingsRepository ?? throw new ArgumentNullException(nameof(userSettingsRepository));
    private readonly ICreditCardRepository _creditCardRepository =
        creditCardRepository ?? throw new ArgumentNullException(nameof(creditCardRepository));
    private readonly CreditCardStatementCalculator _cardStatementCalculator =
        cardStatementCalculator ?? throw new ArgumentNullException(nameof(cardStatementCalculator));
    private readonly IClock _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    /// <summary>
    /// Açık dönemin gidişatını getirir; açık dönem yoksa (henüz plan dondurulmamış ya da son dönem
    /// kapatılmış ve yenisi başlamamışsa) <c>null</c> döner.
    /// </summary>
    public async Task<PeriodProgress?> GetAsync(CancellationToken cancellationToken = default)
    {
        var ledger = await _ledgerReader.ReadAsync(cancellationToken);
        return ledger is null ? null : await CalculateAsync(ledger, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PeriodProgress> PreviewAsync(
        decimal balance,
        DateOnly observedOn,
        CancellationToken cancellationToken = default)
    {
        var ledger = await _ledgerReader.ReadAsync(cancellationToken) ??
            throw new InvalidOperationException("Önizleme için önce güncel bir dönem planı gerekir.");
        ObservationDayGuard.EnsureCanObserve(ledger.Plan, observedOn, _clock.Today);

        var draft = new PeriodObservation
        {
            PeriodPlanSnapshotId = ledger.Plan.Id,
            ObservedOn = observedOn,
            ObservedBalance = balance,
            RecordedAtUtc = _clock.UtcNow
        };
        return await CalculateAsync(
            ledger with { Observations = PeriodObservationRules.Record(ledger.Observations, draft) },
            cancellationToken);
    }

    private async Task<PeriodProgress> CalculateAsync(OpenPeriodLedger ledger, CancellationToken cancellationToken)
    {
        var settings = await _userSettingsRepository.GetSettingsAsync(cancellationToken);
        var cards = await _creditCardRepository.GetCreditCardsAsync(cancellationToken);
        var period = new CashFlowPeriod(ledger.Plan.PeriodStart, ledger.Plan.PeriodEnd);
        var currentCardPayments = CurrentCardPayments.Of(
            cards, period, settings.CreditCardCarryInterestRate, _cardStatementCalculator);

        return PeriodProgressCalculator.Calculate(
            ledger,
            currentCardPayments,
            settings.DeficitFinancingInterestRate,
            _clock.Today);
    }
}
