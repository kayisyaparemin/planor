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
    private const string InvalidTotalRepaymentMessage = "Toplam geri ödeme ana tutardan düşük olamaz.";
    private const string InvalidFirstPaymentDateMessage = "İlk ödeme tarihi başlangıç tarihinden önce olamaz.";

    /// <summary>Form girdilerini doğrular ve geçerliyse istek modelini üretir.</summary>
    public static bool TryBuild(
        SimulationConditionViewModel vm, SimulationRequest? loaded, DateOnly today,
        out SimulationRequest request, out string error)
    {
        request = new SimulationRequest();
        if (string.IsNullOrWhiteSpace(vm.Name)) { error = MissingNameMessage; return false; }
        if (!ValidateAmount(vm, out var amount, out error)) { return false; }
        if (!ValidatePaymentCount(vm.Option, vm.PaymentCountInput, out var paymentCount, out error)) { return false; }

        decimal? totalRepayment = null;
        if (vm.Option.Key == "financing" && !ValidateFinancing(vm, amount, out totalRepayment, out error))
        {
            return false;
        }

        request = BuildRequest(vm, loaded, amount, paymentCount, totalRepayment);
        if (SimulationConditionRules.IsDatePassed(request, today))
        {
            error = SimulationConditionRules.PastDateMessage;
            return false;
        }

        error = string.Empty;
        return true;
    }

    /// <summary>Formdaki değişiklikleri kontrol eder.</summary>
    public static bool HasChanges(SimulationConditionViewModel vm, SimulationRequest? loaded, DateOnly today) =>
        loaded is { } l ? HasEditChanges(vm, l) : HasNewChanges(vm, today);

    /// <summary>Yüklenen deneme verisini form alanlarına aktarır.</summary>
    public static void Populate(SimulationConditionViewModel vm, SimulationRequest req)
    {
        vm.SetLoadedOption(SimulationScenarioCatalog.For(req.Type));
        vm.ScenarioType = req.Type;
        vm.Name = req.Name;
        vm.SelectedPrepaymentMode = req.PrepaymentMode ?? (req.Type == SimulationScenarioType.LoanEarlyClosure ? LoanPrepaymentMode.FullClosure : LoanPrepaymentMode.ReduceTerm);
        vm.AmountInput = vm.NeedsAmount ? StatementEntryViewModel.FormatAmount(req.Amount) : string.Empty;
        vm.Date = req.StartDate;
        vm.CardId = req.CreditCardId;
        vm.LoanId = req.LoanId;
        vm.PaymentCountInput = req.PaymentCount > 1 ? req.PaymentCount.ToString(CultureInfo.InvariantCulture) : string.Empty;
        vm.SelectedCardPaymentMode = req.CardPaymentType ?? CreditCardPaymentType.FullStatement;
        vm.SelectedCardPaymentScope = req.AppliesToAllStatements;
        vm.TotalRepaymentAmountInput = req.TotalRepaymentAmount is { } t ? StatementEntryViewModel.FormatAmount(t) : string.Empty;
        vm.FirstPaymentDate = req.FirstPaymentDate ?? req.StartDate;
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

    private static bool ValidateFinancing(
        SimulationConditionViewModel vm, decimal amount, out decimal? totalRepayment, out string error)
    {
        totalRepayment = null;
        if (!StatementEntryViewModel.TryParseAmount(vm.TotalRepaymentAmountInput, out var parsed) || parsed < amount)
        {
            error = InvalidTotalRepaymentMessage;
            return false;
        }
        if (vm.FirstPaymentDate < vm.Date)
        {
            error = InvalidFirstPaymentDateMessage;
            return false;
        }
        totalRepayment = parsed;
        error = string.Empty;
        return true;
    }

    private static SimulationRequest BuildRequest(
        SimulationConditionViewModel vm, SimulationRequest? loaded, decimal amount, int paymentCount, decimal? totalRepayment)
    {
        var isCard = vm.Option.Key == "card-payment-mode";
        var isLoan = vm.Option.Key == "loan-prepayment";
        var isFinancing = vm.Option.Key == "financing";
        var type = SimulationScenarioCatalog.Resolve(vm.Option, paymentCount, vm.SelectedPrepaymentMode, loaded?.Type);

        return (loaded ?? new SimulationRequest()) with
        {
            Type = type, Name = vm.Name.Trim(), Amount = amount, StartDate = vm.Date,
            CreditCardId = vm.CardId, PaymentCount = paymentCount,
            CardPaymentType = isCard ? vm.SelectedCardPaymentMode : null,
            AppliesToAllStatements = isCard && vm.SelectedCardPaymentScope,
            LoanId = vm.LoanId, PrepaymentMode = isLoan ? vm.SelectedPrepaymentMode : null,
            TotalRepaymentAmount = totalRepayment,
            FirstPaymentDate = isFinancing ? vm.FirstPaymentDate : null
        };
    }

    private static bool HasEditChanges(SimulationConditionViewModel vm, SimulationRequest l) =>
        vm.Name.Trim() != l.Name
        || vm.AmountInput != (vm.NeedsAmount ? StatementEntryViewModel.FormatAmount(l.Amount) : string.Empty)
        || vm.Date != l.StartDate
        || vm.PaymentCountInput != (l.PaymentCount > 1 ? l.PaymentCount.ToString(CultureInfo.InvariantCulture) : string.Empty)
        || vm.SelectedCardPaymentMode != (l.CardPaymentType ?? CreditCardPaymentType.FullStatement)
        || vm.SelectedCardPaymentScope != l.AppliesToAllStatements
        || vm.LoanId != l.LoanId
        || vm.SelectedPrepaymentMode != (l.PrepaymentMode ?? (l.Type == SimulationScenarioType.LoanEarlyClosure ? LoanPrepaymentMode.FullClosure : LoanPrepaymentMode.ReduceTerm))
        || vm.TotalRepaymentAmountInput != (l.TotalRepaymentAmount is { } t ? StatementEntryViewModel.FormatAmount(t) : string.Empty)
        || vm.FirstPaymentDate != (l.FirstPaymentDate ?? l.StartDate);

    private static bool HasNewChanges(SimulationConditionViewModel vm, DateOnly today) =>
        !string.IsNullOrWhiteSpace(vm.Name)
        || (vm.NeedsAmount && !string.IsNullOrWhiteSpace(vm.AmountInput))
        || vm.Date != today
        || !string.IsNullOrWhiteSpace(vm.PaymentCountInput)
        || (vm.IsCardPaymentMode && (vm.SelectedCardPaymentMode != CreditCardPaymentType.FullStatement || vm.SelectedCardPaymentScope))
        || !string.IsNullOrWhiteSpace(vm.TotalRepaymentAmountInput)
        || vm.FirstPaymentDate != vm.Date
        || (vm.IsLoanPrepayment && vm.SelectedPrepaymentMode != LoanPrepaymentMode.FullClosure);

    private static bool ValidateAmount(SimulationConditionViewModel vm, out decimal amount, out string error)
    {
        amount = 0m;
        error = string.Empty;
        if (vm.Option.Key == "card-payment-mode" ||
            (vm.Option.Key == "loan-prepayment" && vm.SelectedPrepaymentMode == LoanPrepaymentMode.FullClosure))
        {
            return true;
        }

        return StatementEntryViewModel.TryParseAmount(vm.AmountInput, out amount) && amount > 0m
            ? true
            : Fail(InvalidAmountMessage, out error);
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

        if (option.Key is "recurring" or "financing" or "cash-debt")
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
