using CommunityToolkit.Mvvm.ComponentModel;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Kredi formunun faiz kartı: kalan anapara ve bankanın kapatma tutarını taşır, formun o anki
/// taksit bilgileriyle bugün kapatmanın bedelini ve aylık faizi canlı çözer. Hesap kaydın
/// kurallarıyla yapılır; kayıt bir tutarı reddedecekse kart da faiz göstermez (EK-V6c, S64-4, S64-9).
/// </summary>
public sealed partial class LoanPayoffViewModel : ObservableObject
{
    private const string InvalidPrincipalMessage = "Kalan anapara sıfırdan büyük bir tutar olmalı.";
    private const string InvalidClosureMessage = "Bankanın kapatma tutarı sıfırdan büyük bir tutar olmalı.";

    private readonly IObligationManagementService _service;
    private (string Principal, string Closure) _loaded = (string.Empty, string.Empty);
    private (decimal Amount, DateOnly AsOf)? _loadedQuote;
    private Loan? _terms;

    [ObservableProperty] private string principalInput = string.Empty;
    [ObservableProperty] private string closureInput = string.Empty;
    [ObservableProperty] private decimal? monthlyRate;
    [ObservableProperty] private decimal? payoffAmount;
    [ObservableProperty] private decimal? fee;
    [ObservableProperty] private decimal? interestSaving;
    [ObservableProperty] private bool isResolved;
    [ObservableProperty] private bool hasPayoff;
    [ObservableProperty] private bool hasFee;
    [ObservableProperty] private bool hasInterestSaving;
    [ObservableProperty] private bool needsAmount = true;
    [ObservableProperty] private bool isMismatch;

    /// <summary>Faiz kartını kredi yazma portunun önizlemesiyle başlatır.</summary>
    public LoanPayoffViewModel(IObligationManagementService service) =>
        _service = service ?? throw new ArgumentNullException(nameof(service));

    /// <summary>İki tutardan biri dolduruldukları andan beri değişti mi.</summary>
    public bool HasChanges => (PrincipalInput, ClosureInput) != _loaded;

    /// <summary>
    /// Tutarları krediden doldurur. Yalnız biri dolu gelir: kapatma tutarı son ödenen taksitten
    /// önce alınmamışsa o, değilse kalan anapara (S64-4). Bayat tutar gösterilmez, kaydedince düşer.
    /// </summary>
    public void Fill(Loan? loan)
    {
        _terms = null;
        _loadedQuote = loan is { EarlyClosureAmount: > 0m, EarlyClosureAmountAsOf: { } asOf } &&
                       asOf >= LoanAmortizationCalculator.PreviousDueDate(loan)
            ? (loan.EarlyClosureAmount.Value, asOf)
            : null;
        ClosureInput = _loadedQuote is { } quote ? StatementEntryViewModel.FormatAmount(quote.Amount) : string.Empty;
        PrincipalInput = _loadedQuote is null && loan?.RemainingDebt is { } debt
            ? StatementEntryViewModel.FormatAmount(debt)
            : string.Empty;
        _loaded = (PrincipalInput, ClosureInput);
    }

    /// <summary>
    /// Formun taksit bilgileri değişince kartı yeniden çözer. Taslak null ise (aylık taksit ya da
    /// kalan taksit geçersiz) faiz hesaplanmaz.
    /// </summary>
    public void Refresh(Loan? terms)
    {
        _terms = terms;
        Recompute();
    }

    /// <summary>
    /// Tutarları kaydedilecek krediye yazar. Geçersiz tutar varsa kullanıcıya gösterilecek mesajı
    /// döner ve kredi değişmez.
    /// </summary>
    public string? TryApply(Loan loan, out Loan result)
    {
        ArgumentNullException.ThrowIfNull(loan);
        result = loan;
        if (ReadAmounts(out var principal, out var closure) is { } error)
        {
            return error;
        }

        result = WithAmounts(loan, principal, closure);
        return null;
    }

    partial void OnPrincipalInputChanged(string value) => Recompute();

    partial void OnClosureInputChanged(string value) => Recompute();

    private void Recompute()
    {
        var hasAmount = ReadAmounts(out var principal, out var closure) is null && (principal ?? closure) is not null;
        var previewed = hasAmount && _terms is not null;
        var overview = previewed ? _service.PreviewLoan(WithAmounts(_terms!, principal, closure)) : null;
        var quote = overview?.Analysis.Amortization is not null ? overview.Today : null;

        NeedsAmount = !hasAmount;
        IsResolved = overview?.Analysis.Amortization is not null;
        IsMismatch = previewed && !IsResolved;
        MonthlyRate = overview?.Analysis.Amortization?.MonthlyRate;
        HasPayoff = quote is { HasAnythingToClose: true };
        PayoffAmount = HasPayoff ? quote!.Amount : null;
        Fee = HasPayoff ? quote!.Fee : null;
        HasFee = Fee > 0m;
        InterestSaving = HasPayoff ? quote!.InterestSaving : null;
        HasInterestSaving = InterestSaving > 0m;
    }

    private string? ReadAmounts(out decimal? principal, out decimal? closure)
    {
        closure = null;
        if (!TryReadAmount(PrincipalInput, out principal))
        {
            return InvalidPrincipalMessage;
        }

        return TryReadAmount(ClosureInput, out closure) ? null : InvalidClosureMessage;
    }

    // S64-4: kapatma tutarı varsa otoritedir; değişmediyse kendi tarihini taşır, değiştiyse tarihsiz
    // gider ve kayıt bugünle damgalar. Kalan anaparayı o zaman kayıt tutardan çözer.
    private Loan WithAmounts(Loan loan, decimal? principal, decimal? closure) => closure is { } amount
        ? loan with
        {
            RemainingDebt = principal ?? loan.RemainingDebt,
            EarlyClosureAmount = amount,
            EarlyClosureAmountAsOf = _loadedQuote is { } quote && quote.Amount == amount ? quote.AsOf : null
        }
        : loan with { RemainingDebt = principal, EarlyClosureAmount = null, EarlyClosureAmountAsOf = null };

    // Boş alan tutarsızdır; dolu ama okunamayan ya da sıfırdan büyük olmayan tutar geçersizdir.
    private static bool TryReadAmount(string text, out decimal? amount)
    {
        amount = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        if (!StatementEntryViewModel.TryParseAmount(text, out var value) || value <= 0m)
        {
            return false;
        }

        amount = value;
        return true;
    }
}
