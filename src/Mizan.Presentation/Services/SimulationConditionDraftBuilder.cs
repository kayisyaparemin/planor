using System.Globalization;
using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;
using Mizan.Presentation.ViewModels;

namespace Mizan.Presentation.Services;

/// <summary>
/// Simülatör deneme formunun girdilerini doğrular ve çalışma listesi isteğine çevirir (EK-V10, S76-8).
/// </summary>
public static class SimulationConditionDraftBuilder
{
    private const string MissingNameMessage = "Denemeye bir ad ver.";
    private const string InvalidAmountMessage = "Tutarı sıfırdan büyük bir sayı olarak gir.";
    private const string InvalidInstallmentMessage = "Taksit sayısını 1 veya daha büyük bir sayı olarak gir.";
    private const string InvalidRecurringCountMessage = "Ödeme sayısını 1 ile 120 arasında bir sayı olarak gir.";

    /// <summary>Form girdilerini doğrular ve geçerliyse istek modelini üretir.</summary>
    public static bool TryBuild(
        ScenarioOption option, SimulationRequest? loaded, string name, string amountInput, DateOnly date,
        Guid? cardId, string paymentCountInput, CreditCardPaymentType cardPaymentMode, bool cardPaymentScope,
        DateOnly today, out SimulationRequest request, out string error)
    {
        request = new SimulationRequest();
        if (string.IsNullOrWhiteSpace(name)) { error = MissingNameMessage; return false; }
        if (!ValidateAmount(option, amountInput, out var amount, out error)) { return false; }
        if (!ValidatePaymentCount(option, paymentCountInput, out var paymentCount, out error)) { return false; }

        var isCardPaymentMode = option.Key == "card-payment-mode";
        request = (loaded ?? new SimulationRequest()) with
        {
            Type = SimulationScenarioCatalog.Resolve(option, paymentCount, null, loaded?.Type),
            Name = name.Trim(),
            Amount = amount,
            StartDate = date,
            CreditCardId = cardId,
            PaymentCount = paymentCount,
            CardPaymentType = isCardPaymentMode ? cardPaymentMode : null,
            AppliesToAllStatements = isCardPaymentMode && cardPaymentScope
        };

        if (SimulationConditionRules.IsDatePassed(request, today))
        {
            error = SimulationConditionRules.PastDateMessage;
            return false;
        }

        error = string.Empty;
        return true;
    }

    /// <summary>İsteği var olan çalışma listesine ekler ya da yerinde günceller.</summary>
    public static IReadOnlyList<SimulationDraftCondition> ApplyCondition(
        IEnumerable<SimulationWorkingCondition> currentList, SimulationRequest request)
    {
        var list = currentList.Select(x => new SimulationDraftCondition(x.Request, x.IsEnabled)).ToList();
        var index = list.FindIndex(x => x.Request.ScenarioId == request.ScenarioId);
        if (index < 0) { list.Add(new SimulationDraftCondition(request)); }
        else { list[index] = list[index] with { Request = request }; }
        return list;
    }

    private static bool ValidateAmount(ScenarioOption option, string amountInput, out decimal amount, out string error)
    {
        amount = 0m;
        error = string.Empty;
        if (option.Key != "card-payment-mode" && (!StatementEntryViewModel.TryParseAmount(amountInput, out amount) || amount <= 0m))
        {
            error = InvalidAmountMessage;
            return false;
        }

        return true;
    }

    private static bool ValidatePaymentCount(ScenarioOption option, string input, out int count, out string error)
    {
        count = 1;
        error = string.Empty;
        if (option.Key == "card" && !string.IsNullOrWhiteSpace(input))
        {
            return int.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out count) && count >= 1
                ? true
                : Fail(InvalidInstallmentMessage, out error);
        }

        if (option.Key == "recurring")
        {
            return int.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out count) && count is >= 1 and <= 120
                ? true
                : Fail(InvalidRecurringCountMessage, out error);
        }

        return true;
    }

    private static bool Fail(string message, out string error)
    {
        error = message;
        return false;
    }
}
