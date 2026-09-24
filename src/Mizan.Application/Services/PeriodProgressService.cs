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
    IClock clock)
{
    // Kartın son bilinen ekstresinden açık dönemin vadesine uzanmaya yeten ekstre sayısı.
    private const int ProjectedStatementCount = 6;

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
        if (ledger is null)
        {
            return null;
        }

        var settings = await _userSettingsRepository.GetSettingsAsync(cancellationToken);
        var cards = await _creditCardRepository.GetCreditCardsAsync(cancellationToken);
        var period = new CashFlowPeriod(ledger.Plan.PeriodStart, ledger.Plan.PeriodEnd);
        var currentCardPayments = CurrentCardPayments(cards, period, settings.CreditCardCarryInterestRate);

        return PeriodProgressCalculator.Calculate(
            ledger,
            currentCardPayments,
            settings.DeficitFinancingInterestRate,
            _clock.Today);
    }

    // Dondurulan plan kartın o günkü tahminini saklar; bu, aradan geçen ekstre girişleri, harcamalar ve
    // ödeme kararlarından sonraki hâlidir. Plan satırıyla aynı yarı açık pencereden seçilir (S30).
    private Dictionary<Guid, decimal> CurrentCardPayments(
        IReadOnlyList<CreditCard> cards,
        CashFlowPeriod period,
        decimal carryInterestRate) =>
        cards.ToDictionary(
            card => card.Id,
            card => _cardStatementCalculator
                .Project(card, ProjectedStatementCount, useProjectionFallback: true, carryInterestRate: carryInterestRate)
                .Where(x => period.Contains(x.PaymentDueDate))
                .Sum(x => x.Payment ?? 0m));
}
