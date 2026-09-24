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
    /// Krediyi kaydetmeden önce doğrular. Bankanın kapatma tutarı verilmişse otoritedir:
    /// tarihsizse bugünle damgalanır, kalan anapara ondan geri çözülür ve
    /// <see cref="Loan.RemainingDebt"/>'e yazılır.
    /// </summary>
    /// <exception cref="InvalidOperationException">Kredi verisi tutarsızsa, kullanıcıya gösterilecek Türkçe mesajla.</exception>
    public Loan PrepareForSave(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);
        if (loan.EarlyClosureAmount is null or <= 0m)
        {
            return ValidatePrincipal(loan with { EarlyClosureAmount = null, EarlyClosureAmountAsOf = null });
        }

        // Tarih sorulmaz: banka tutarı yalnız görüldüğü gün geçerlidir ve kullanıcı onu gördüğü
        // gün girer. Daha önce kaydedilmiş, değişmemiş tutar kendi tarihini taşır.
        var quoted = loan with { EarlyClosureAmountAsOf = loan.EarlyClosureAmountAsOf ?? _clock.Today };
        EnsureQuoteDateIsUsable(quoted);

        if (_amortizationCalculator.Analyze(quoted).Amortization is not { Source: LoanRateSource.BankQuote } amortization)
        {
            throw new InvalidOperationException(
                "Bu kapatma tutarı taksit tutarı ve kalan taksit sayısıyla uyuşmuyor. " +
                "Tutarı ve taksit bilgilerini kontrol et.");
        }

        return quoted with { RemainingDebt = amortization.Principal };
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

    private void EnsureQuoteDateIsUsable(Loan loan)
    {
        var asOf = loan.EarlyClosureAmountAsOf!.Value;
        var today = _clock.Today;
        if (asOf > today)
        {
            throw new InvalidOperationException("Kapatma tutarının tarihi bugünden sonra olamaz.");
        }

        var previousDue = LoanAmortizationCalculator.PreviousDueDate(loan);
        if (asOf >= previousDue)
        {
            return;
        }

        // Bugünün tutarı bile son ödenen taksitten eskiyse sorun tutarda değil, sonraki ödeme
        // tarihindedir: bir aydan fazla ileride girilmiş, son taksit gelecekte görünüyor.
        throw new InvalidOperationException(asOf == today
            ? $"Sonraki ödeme tarihi ({loan.NextPaymentDate.ToString(MessageDateFormat, CultureInfo.InvariantCulture)}) bugünden bir aydan " +
              "fazla ileride; bu durumda son ödenen taksit bugünden sonra görünüyor ve kapatma tutarı " +
              "hesaplanamıyor. Sonraki ödeme tarihini kontrol et."
            : $"Kayıtlı kapatma tutarı son ödenen taksitten ({previousDue.ToString(MessageDateFormat, CultureInfo.InvariantCulture)}) önce " +
              "alınmış. Bankadan bugünkü tutarı gir.");
    }

    private Loan ValidatePrincipal(Loan loan)
    {
        if (loan.RemainingDebt is null)
        {
            return loan;
        }

        return _amortizationCalculator.Analyze(loan).Issue switch
        {
            LoanAnalysisIssue.PrincipalNotBelowInstallments => throw new InvalidOperationException(
                "Kalan anapara, kalan taksitlerin toplamından küçük olmalı. Bankanın gösterdiği kalan borç " +
                "taksitlerin toplamıysa bu alanı boş bırakıp kapatma tutarını gir."),
            LoanAnalysisIssue.ImplausibleRate => throw new InvalidOperationException(
                "Bu anaparayla kredinin aylık faizi gerçekçi olmayan bir değere çıkıyor. " +
                "Kalan anaparayı ya da bankanın kapatma tutarını kontrol et."),
            _ => loan
        };
    }
}
