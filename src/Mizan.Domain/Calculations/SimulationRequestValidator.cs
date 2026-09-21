using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Simülasyon senaryo isteklerinin tutarlılığını, yasal ve matematiksel kısıtlarını
/// ve çoklu gelir akışı çakışmalarını denetleyen saf doğrulayıcı.
/// </summary>
public static class SimulationRequestValidator
{
    /// <summary>
    /// Tekil bir simülasyon isteğini doğrular. Hatalı durumda <see cref="InvalidOperationException"/>
    /// veya <see cref="ArgumentOutOfRangeException"/> fırlatır.
    /// </summary>
    public static void Validate(
        SimulationRequest request,
        DateOnly? projectionAnchorDate = null)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidOperationException("Plan adı gereklidir.");
        }

        ValidateFutureIncome(request, projectionAnchorDate);
        ValidateLoanPrepayment(request);
        ValidateAmountAndCount(request);
        ValidateCreditCardPayment(request);
        ValidateFinancingLoan(request);
        ValidateFirstPaymentDate(request);
        ValidateIncomeChange(request);
    }

    /// <summary>
    /// Simülasyon istekleri listesini doğrular. Boş liste veya çakışan gelir değişikliklerinde
    /// <see cref="InvalidOperationException"/> fırlatır.
    /// </summary>
    public static void Validate(IReadOnlyList<SimulationRequest> requests)
    {
        ArgumentNullException.ThrowIfNull(requests);

        if (requests.Count == 0)
        {
            throw new InvalidOperationException(
                "Simülasyon için en az bir koşul eklemelisin.");
        }

        foreach (var request in requests)
        {
            Validate(request);
        }

        ValidateNoConflictingIncomeChanges(requests);
    }

    private static void ValidateFutureIncome(
        SimulationRequest request,
        DateOnly? projectionAnchorDate)
    {
        if (request.Type == SimulationScenarioType.FutureIncome &&
            projectionAnchorDate is { } anchor &&
            request.StartDate < anchor)
        {
            throw new InvalidOperationException(
                $"Gelir tarihi son güncelleme tarihinden ({anchor:dd.MM.yyyy}) " +
                "önce olamaz. Bu tarihte gelen para zaten mevcut tutarına " +
                "dahil olmalı; onu güncelle.");
        }
    }

    private static void ValidateLoanPrepayment(SimulationRequest request)
    {
        if (request.Type is SimulationScenarioType.LoanEarlyClosure or
                SimulationScenarioType.LoanPartialPrepayment &&
            request.LoanId is null)
        {
            throw new InvalidOperationException(
                "Erken ödeme için bir kredi seçmelisin.");
        }

        if (request.Type == SimulationScenarioType.LoanPartialPrepayment &&
            request.PrepaymentMode is not (LoanPrepaymentMode.ReduceTerm or
                LoanPrepaymentMode.ReduceInstallment))
        {
            throw new InvalidOperationException(
                "Ara ödemede vadenin mi taksitin mi azalacağını seçmelisin.");
        }
    }

    private static void ValidateAmountAndCount(SimulationRequest request)
    {
        if (request.Type is not SimulationScenarioType.CreditCardPaymentMode and
            not SimulationScenarioType.LoanEarlyClosure &&
            request.Amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "Plan tutarı 0'dan büyük olmalı.");
        }

        var needsCount = request.Type is
            SimulationScenarioType.CreditCardInstallmentPurchase or
            SimulationScenarioType.FinancingLoan or
            SimulationScenarioType.CashDebt or
            SimulationScenarioType.RecurringPayment;

        if (needsCount && request.PaymentCount is < 1 or > 120)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "Ödeme sayısı 1 ile 120 arasında olmalıdır.");
        }
    }

    private static void ValidateCreditCardPayment(SimulationRequest request)
    {
        if (request.Type != SimulationScenarioType.CreditCardPaymentMode)
        {
            return;
        }

        if (request.CreditCardId is null)
        {
            throw new InvalidOperationException(
                "Ödeme şekli planı için bir kredi kartı seçmelisin.");
        }

        if (request.CardPaymentType is CreditCardPaymentType.FixedAmount)
        {
            throw new InvalidOperationException(
                "Kart ödeme şekli yalnızca asgari veya tamamı olabilir.");
        }
    }

    private static void ValidateFinancingLoan(SimulationRequest request)
    {
        if (request.Type != SimulationScenarioType.FinancingLoan)
        {
            return;
        }

        if (request.TotalRepaymentAmount is null or <= 0m)
        {
            throw new InvalidOperationException(
                "Finansman için toplam geri ödeme gereklidir.");
        }

        if (request.TotalRepaymentAmount < request.Amount)
        {
            throw new InvalidOperationException(
                "Toplam geri ödeme ana tutardan düşük olamaz.");
        }

        if (request.FirstPaymentDate is null)
        {
            throw new InvalidOperationException(
                "Finansman için ilk ödeme tarihi gereklidir.");
        }
    }

    private static void ValidateFirstPaymentDate(SimulationRequest request)
    {
        if (request.FirstPaymentDate is DateOnly firstPayment &&
            firstPayment < request.StartDate &&
            request.Type is SimulationScenarioType.FinancingLoan or
                SimulationScenarioType.CashDebt or
                SimulationScenarioType.RecurringPayment)
        {
            throw new InvalidOperationException(
                "İlk ödeme tarihi başlangıç tarihinden önce olamaz.");
        }
    }

    private static void ValidateIncomeChange(SimulationRequest request)
    {
        if (request.Type == SimulationScenarioType.IncomeChange &&
            request.RecurringIncomeId is null)
        {
            throw new InvalidOperationException(
                "Gelir değişikliği için bir düzenli gelir akışı seçmelisin.");
        }
    }

    private static void ValidateNoConflictingIncomeChanges(
        IReadOnlyList<SimulationRequest> requests)
    {
        var conflictingIncome = requests
            .Where(x => x.Type == SimulationScenarioType.IncomeChange)
            .GroupBy(x => (x.RecurringIncomeId, x.StartDate))
            .FirstOrDefault(x => x.Count() > 1);

        if (conflictingIncome is not null)
        {
            throw new InvalidOperationException(
                $"{conflictingIncome.Key.StartDate:dd.MM.yyyy} için iki farklı gelir değişikliği var. " +
                "Simülasyonu çalıştırmadan önce birini düzenle veya kaldır.");
        }
    }
}
