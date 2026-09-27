using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;
using Mizan.Presentation.Onboarding.Support;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Kredi formunun çocuğu: kredinin tanımını (ad, banka, aylık taksit, kalan taksit, sonraki taksit
/// tarihi, kredi türü) taşır, doğrular ve krediye çevirir. Ödeme günü sorulmaz, sonraki taksit
/// tarihinden çözülür; formun dokunmadığı her alan korunur (EK-V6c, S64-2, S64-3).
/// </summary>
public sealed partial class LoanDefinitionViewModel : ObservableObject
{
    private (string Name, string Bank, string Payment, string Count, DateOnly Date, LoanKind Kind) _loaded;

    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string bank = string.Empty;
    [ObservableProperty] private string paymentInput = string.Empty;
    [ObservableProperty] private string countInput = string.Empty;
    [ObservableProperty] private DateOnly nextPaymentDate;
    [ObservableProperty] private LoanKind kind;

    /// <summary>Kredi türü seçicisinin seçenekleri; görünen adlar App'teki çeviricidedir.</summary>
    public IReadOnlyList<LoanKind> Kinds { get; } = [LoanKind.Consumer, LoanKind.HousingFixed, LoanKind.HousingVariable];

    /// <summary>Alanlar dolduruldukları andan beri değişti mi.</summary>
    public bool HasChanges => Current() != _loaded;

    /// <summary>Alanları krediden doldurur; kredi yoksa bir ay sonrası tarihli boş yeni kredi formu.</summary>
    public void Fill(Loan? loan, DateOnly today)
    {
        Name = loan?.Name ?? string.Empty;
        Bank = loan?.Bank ?? string.Empty;
        PaymentInput = loan is null ? string.Empty : StatementEntryViewModel.FormatAmount(loan.MonthlyPayment);
        CountInput = loan?.RemainingInstallmentCount.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        NextPaymentDate = loan?.NextPaymentDate ?? today.AddMonths(1);
        Kind = loan?.Kind ?? LoanKind.Consumer;
        _loaded = Current();
    }

    /// <summary>
    /// Alanları doğrulayıp krediyi kurar. Geçersizse kullanıcıya gösterilecek mesajı döner ve kredi
    /// kurulmaz. Ad, taksit ve kalan taksit kurulumla aynı kurallardan geçer.
    /// </summary>
    public string? TryBuild(Loan? existing, out Loan? loan)
    {
        loan = null;
        var hasPayment = StatementEntryViewModel.TryParseAmount(PaymentInput, out var payment);
        var hasCount = int.TryParse(CountInput, NumberStyles.Integer, CultureInfo.InvariantCulture, out var count);
        var day = ResolvePaymentDay(existing);
        var error = OnboardingValidator.ValidateLoan(Name, hasPayment ? payment : null, hasCount ? count : null, day);
        if (error is null)
        {
            loan = Build(existing, payment, count, day);
        }

        return error;
    }

    private Loan Build(Loan? existing, decimal payment, int count, int day)
    {
        var sameSchedule = existing is not null && existing.MonthlyPayment == payment && existing.RemainingInstallmentCount == count;
        var loan = (existing ?? new Loan()) with
        {
            Name = Name.Trim(),
            Bank = Bank.Trim(),
            MonthlyPayment = payment,
            RemainingInstallmentCount = count,
            NextPaymentDate = NextPaymentDate,
            PaymentDay = day,
            Kind = Kind,
            FinalPaymentAmount = sameSchedule ? existing!.FinalPaymentAmount : null
        };

        // S64-4: son ödenen taksitten önce alınmış kapatma tutarı artık geçerli değil; eskisi gibi düşer.
        return loan.EarlyClosureAmountAsOf is { } asOf && asOf < LoanAmortizationCalculator.PreviousDueDate(loan)
            ? loan with { EarlyClosureAmount = null, EarlyClosureAmountAsOf = null }
            : loan;
    }

    // S64-2: gün tarihin günüdür; kayıtlı gün o ayda aynı tarihe kenetleniyorsa (31 → 30 Eylül) korunur.
    private int ResolvePaymentDay(Loan? existing) =>
        existing is not null && existing.PaymentDay is >= 1 and <= 31 &&
        CalendarRules.ResolveDay(NextPaymentDate.Year, NextPaymentDate.Month, existing.PaymentDay) == NextPaymentDate
            ? existing.PaymentDay
            : NextPaymentDate.Day;

    private (string, string, string, string, DateOnly, LoanKind) Current() =>
        (Name, Bank, PaymentInput, CountInput, NextPaymentDate, Kind);
}
