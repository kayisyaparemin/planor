using System.Globalization;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Kredi erken kapama ve ara ödemenin uygulama katmanı. Kalan anapara ya da bankanın tarihli
/// kapatma tutarı, faizin tek kaynağıdır (<see cref="LoanAmortizationCalculator"/>); bu servis
/// krediyi kaydetmeden önce bu veriyi doğrular ki üstüne kurulan her rakam anlamlı olsun.
/// </summary>
public sealed class LoanPayoffService(
    IClock clock,
    LoanAmortizationCalculator amortizationCalculator,
    LoanPaymentScheduleBuilder scheduleBuilder)
{
    // Kültürden bağımsız sabit desen; ekran biçimi sunum kenarına aittir (S28).
    private const string MessageDateFormat = "dd.MM.yyyy";

    private readonly IClock _clock =
        clock ?? throw new ArgumentNullException(nameof(clock));
    private readonly LoanAmortizationCalculator _amortizationCalculator =
        amortizationCalculator ?? throw new ArgumentNullException(nameof(amortizationCalculator));
    private readonly LoanPaymentScheduleBuilder _scheduleBuilder =
        scheduleBuilder ?? throw new ArgumentNullException(nameof(scheduleBuilder));

    /// <summary>Her kredinin bugünkü faizini, anaparasını ve kapatma bedelini çıkarır.</summary>
    public IReadOnlyList<LoanPayoffOverview> Describe(IEnumerable<Loan> loans)
    {
        ArgumentNullException.ThrowIfNull(loans);
        return loans.Select(Describe).ToArray();
    }

    /// <summary>Kredinin bugünkü faizini, anaparasını ve bugün kapatılırsa ödenecek bedeli çıkarır.</summary>
    public LoanPayoffOverview Describe(Loan loan)
    {
        var analysis = _amortizationCalculator.Analyze(loan);
        var today = analysis.Amortization is { } amortization
            ? _amortizationCalculator.PayoffOn(loan, amortization, _clock.Today)
            : null;
        return new LoanPayoffOverview(loan, analysis, today);
    }

    /// <summary>
    /// Plandaki erken ödemeleri, her biri o günkü kredi durumundan hesaplanan tutarıyla,
    /// tarih sırasına göre listeler.
    /// </summary>
    public IReadOnlyList<PlannedLoanPrepayment> DescribePrepayments(FinancialPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return plan.Loans
            .SelectMany(loan => DescribePrepayments(loan, plan.LoanPrepayments))
            .OrderBy(x => x.Prepayment.Date)
            .ToArray();
    }

    /// <summary>
    /// Krediyi <see cref="PrepareForSave"/>'in kurallarıyla, hata fırlatmadan değerlendirir: kayıt
    /// kabul edecekse kaydedilecek hâlinin bugünkü görünümünü, reddedecekse null döner. Kayıt ile
    /// canlı faiz kartı aynı kuralı paylaşır; bankanın tutarı uyuşmazsa anaparaya düşülmez (S64-9).
    /// </summary>
    public LoanPayoffOverview? Preview(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);
        return Prepare(loan, out var prepared) is null ? Describe(prepared) : null;
    }

    /// <summary>
    /// Krediyi kaydetmeden önce doğrular. Bankanın kapatma tutarı verilmişse otoritedir:
    /// tarihsizse bugünle damgalanır, kalan anapara ondan geri çözülür ve
    /// <see cref="Loan.RemainingDebt"/>'e yazılır.
    /// </summary>
    /// <exception cref="InvalidOperationException">Kredi verisi tutarsızsa, kullanıcıya gösterilecek Türkçe mesajla.</exception>
    public Loan PrepareForSave(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);
        return Prepare(loan, out var prepared) is { } error ? throw new InvalidOperationException(error) : prepared;
    }

    private IEnumerable<PlannedLoanPrepayment> DescribePrepayments(Loan loan, IReadOnlyList<LoanPrepayment> prepayments)
    {
        var replay = _scheduleBuilder.Replay(loan, prepayments);
        return prepayments
            .Where(x => x.LoanId == loan.Id)
            .Select(prepayment => new PlannedLoanPrepayment(
                loan,
                prepayment,
                replay.Payments.Where(x => x.SourceId == prepayment.Id).Select(x => (decimal?)x.Amount).FirstOrDefault(),
                replay.IgnoredBecauseUnquotable));
    }

    // Kaydın tek kuralı: kabul ederse kaydedilecek hâli verir ve null döner, etmezse kredi
    // değişmeden kalır ve kullanıcıya gösterilecek mesaj döner.
    private string? Prepare(Loan loan, out Loan prepared)
    {
        prepared = loan;
        if (loan.EarlyClosureAmount is null or <= 0m)
        {
            prepared = loan with { EarlyClosureAmount = null, EarlyClosureAmountAsOf = null };
            return PrincipalError(prepared);
        }

        // Tarih sorulmaz: banka tutarı yalnız görüldüğü gün geçerlidir ve kullanıcı onu gördüğü
        // gün girer. Daha önce kaydedilmiş, değişmemiş tutar kendi tarihini taşır.
        var quoted = loan with { EarlyClosureAmountAsOf = loan.EarlyClosureAmountAsOf ?? _clock.Today };
        if (QuoteDateError(quoted) is { } dateError)
        {
            return dateError;
        }

        if (_amortizationCalculator.Analyze(quoted).Amortization is not { Source: LoanRateSource.BankQuote } amortization)
        {
            return "Bu kapatma tutarı taksit tutarı ve kalan taksit sayısıyla uyuşmuyor. " +
                   "Tutarı ve taksit bilgilerini kontrol et.";
        }

        prepared = quoted with { RemainingDebt = amortization.Principal };
        return null;
    }

    private string? QuoteDateError(Loan loan)
    {
        var asOf = loan.EarlyClosureAmountAsOf!.Value;
        var today = _clock.Today;
        if (asOf > today)
        {
            return "Kapatma tutarının tarihi bugünden sonra olamaz.";
        }

        var previousDue = LoanAmortizationCalculator.PreviousDueDate(loan);
        if (asOf >= previousDue)
        {
            return null;
        }

        // Bugünün tutarı bile son ödenen taksitten eskiyse sorun tutarda değil, sonraki ödeme
        // tarihindedir: bir aydan fazla ileride girilmiş, son taksit gelecekte görünüyor.
        return asOf == today
            ? $"Sonraki ödeme tarihi ({loan.NextPaymentDate.ToString(MessageDateFormat, CultureInfo.InvariantCulture)}) bugünden bir aydan " +
              "fazla ileride; bu durumda son ödenen taksit bugünden sonra görünüyor ve kapatma tutarı " +
              "hesaplanamıyor. Sonraki ödeme tarihini kontrol et."
            : $"Kayıtlı kapatma tutarı son ödenen taksitten ({previousDue.ToString(MessageDateFormat, CultureInfo.InvariantCulture)}) önce " +
              "alınmış. Bankadan bugünkü tutarı gir.";
    }

    private string? PrincipalError(Loan loan)
    {
        if (loan.RemainingDebt is null)
        {
            return null;
        }

        return _amortizationCalculator.Analyze(loan).Issue switch
        {
            LoanAnalysisIssue.PrincipalNotBelowInstallments =>
                "Kalan anapara, kalan taksitlerin toplamından küçük olmalı. Bankanın gösterdiği kalan borç " +
                "taksitlerin toplamıysa bu alanı boş bırakıp kapatma tutarını gir.",
            LoanAnalysisIssue.ImplausibleRate =>
                "Bu anaparayla kredinin aylık faizi gerçekçi olmayan bir değere çıkıyor. " +
                "Kalan anaparayı ya da bankanın kapatma tutarını kontrol et.",
            _ => null
        };
    }
}
