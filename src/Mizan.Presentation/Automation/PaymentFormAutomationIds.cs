namespace Mizan.Presentation.Automation;

/// <summary>
/// Finansal Yapı'dan açılan ödeme formlarının (ödeme planı ve planlı büyük harcama — V6e)
/// otomasyon kimlikleri. <see cref="AutomationIds"/> ile aynı ad alanında durur.
/// </summary>
public static class PaymentFormAutomationIds
{
    /// <summary>Ödeme planı formu sayfası kimliği.</summary>
    public const string PagePaymentPlanForm = "page-payment-plan-form";

    /// <summary>Ödeme planı adı girişi kimliği.</summary>
    public const string InputPaymentPlanName = "input-payment-plan-name";

    /// <summary>Ödeme planı taksitler listesi kimliği.</summary>
    public const string ListPaymentPlanInstallments = "list-payment-plan-installments";

    /// <summary>Taksit giriş bloğunu açan buton kimliği.</summary>
    public const string BtnOpenPaymentPlanInstallment = "btn-open-payment-plan-installment";

    /// <summary>Taksit tutarı girişi kimliği.</summary>
    public const string InputPaymentPlanInstallmentAmount = "input-payment-plan-installment-amount";

    /// <summary>Taksit sayısı girişi kimliği.</summary>
    public const string InputPaymentPlanInstallmentCount = "input-payment-plan-installment-count";

    /// <summary>İlk vade tarihi seçicisi kimliği.</summary>
    public const string PickerPaymentPlanInstallmentDate = "picker-payment-plan-installment-date";

    /// <summary>Taksiti listeye ekleyen buton kimliği.</summary>
    public const string BtnAddPaymentPlanInstallment = "btn-add-payment-plan-installment";

    /// <summary>Taksit girişini kapatan buton kimliği.</summary>
    public const string BtnCancelPaymentPlanInstallment = "btn-cancel-payment-plan-installment";

    /// <summary>Ödeme planını kaydeden buton kimliği.</summary>
    public const string BtnSavePaymentPlan = "btn-save-payment-plan";

    /// <summary>Ödeme planı formundan çıkan buton kimliği.</summary>
    public const string BtnCancelPaymentPlan = "btn-cancel-payment-plan";

    /// <summary>Planlanan büyük harcama formu sayfası kimliği.</summary>
    public const string PagePlannedExpenseForm = "page-planned-expense-form";

    /// <summary>Planlanan harcama adı girişi kimliği.</summary>
    public const string InputPlannedExpenseName = "input-planned-expense-name";

    /// <summary>Planlanan harcama tutarı girişi kimliği.</summary>
    public const string InputPlannedExpenseAmount = "input-planned-expense-amount";

    /// <summary>Planlanan harcama tarihi seçicisi kimliği.</summary>
    public const string PickerPlannedExpenseDate = "picker-planned-expense-date";

    /// <summary>Planlanan harcamayı kaydeden buton kimliği.</summary>
    public const string BtnSavePlannedExpense = "btn-save-planned-expense";

    /// <summary>Planlanan harcama formundan çıkan buton kimliği.</summary>
    public const string BtnCancelPlannedExpense = "btn-cancel-planned-expense";
}
