namespace Mizan.Presentation.Automation;

/// <summary>
/// Finansal Yapı'dan açılan kayıt formlarının (kart, kredi, gelir, ödeme — V6b–V6e) otomasyon
/// kimlikleri. <see cref="AutomationIds"/> ile aynı ad alanında ve aynı kurallarla durur; dosya
/// sınırı (200 satır) yüzünden ayrıdır, kimlikler iki sınıf arasında da tekildir.
/// </summary>
public static class RecordFormAutomationIds
{
    /// <summary>Kart formu sayfası kimliği.</summary>
    public const string PageCardForm = "page-card-form";

    /// <summary>Kart formu: kart adı girişi kimliği.</summary>
    public const string InputCardName = "input-card-name";

    /// <summary>Kart formu: banka girişi kimliği.</summary>
    public const string InputCardBank = "input-card-bank";

    /// <summary>Kart formu: limit girişi kimliği.</summary>
    public const string InputCardLimit = "input-card-limit";

    /// <summary>Kart formu: güncel borç girişi kimliği.</summary>
    public const string InputCardDebt = "input-card-debt";

    /// <summary>Kart formu: kesim günü girişi kimliği.</summary>
    public const string InputCardClosingDay = "input-card-closing-day";

    /// <summary>Kart formu: son ödeme günü girişi kimliği.</summary>
    public const string InputCardDueDay = "input-card-due-day";

    /// <summary>Kart formu: kaydet butonu kimliği.</summary>
    public const string BtnSaveCard = "btn-save-card";

    /// <summary>Kart formu: vazgeç butonu kimliği.</summary>
    public const string BtnCancelCard = "btn-cancel-card";

    /// <summary>Kart formu: gelecek harcamalar listesi kimliği.</summary>
    public const string ListCardCharges = "list-card-charges";

    /// <summary>Kart formu: harcama giriş bloğunu açan buton kimliği.</summary>
    public const string BtnOpenCardCharge = "btn-open-card-charge";

    /// <summary>Kart formu: harcama açıklaması girişi kimliği.</summary>
    public const string InputChargeDescription = "input-charge-description";

    /// <summary>Kart formu: harcama aylık tutarı girişi kimliği.</summary>
    public const string InputChargeAmount = "input-charge-amount";

    /// <summary>Kart formu: taksit sayısı girişi kimliği.</summary>
    public const string InputChargeCount = "input-charge-count";

    /// <summary>Kart formu: ilk taksit tarihi seçicisi kimliği.</summary>
    public const string PickerChargeFirstDate = "picker-charge-first-date";

    /// <summary>Kart formu: harcamayı listeye ekleyen buton kimliği.</summary>
    public const string BtnAddCardCharge = "btn-add-card-charge";

    /// <summary>Kart formu: harcama girişinden vazgeçen buton kimliği.</summary>
    public const string BtnCancelCardCharge = "btn-cancel-card-charge";

    /// <summary>Kredi formu sayfası kimliği.</summary>
    public const string PageLoanForm = "page-loan-form";

    /// <summary>Kredi formu: kredi adı girişi kimliği.</summary>
    public const string InputLoanName = "input-loan-name";

    /// <summary>Kredi formu: banka girişi kimliği.</summary>
    public const string InputLoanBank = "input-loan-bank";

    /// <summary>Kredi formu: aylık taksit girişi kimliği.</summary>
    public const string InputLoanPayment = "input-loan-payment";

    /// <summary>Kredi formu: kalan taksit sayısı girişi kimliği.</summary>
    public const string InputLoanCount = "input-loan-count";

    /// <summary>Kredi formu: sonraki taksit tarihi seçicisi kimliği.</summary>
    public const string PickerLoanNextDate = "picker-loan-next-date";

    /// <summary>Kredi formu: kredi türü seçicisi kimliği.</summary>
    public const string PickerLoanKind = "picker-loan-kind";

    /// <summary>Kredi formu: faiz kartındaki kalan anapara girişi kimliği.</summary>
    public const string InputLoanPrincipal = "input-loan-principal";

    /// <summary>Kredi formu: faiz kartındaki bankanın kapatma tutarı girişi kimliği.</summary>
    public const string InputLoanClosureAmount = "input-loan-closure-amount";

    /// <summary>Kredi formu: kaydet butonu kimliği.</summary>
    public const string BtnSaveLoan = "btn-save-loan";

    /// <summary>Kredi formu: vazgeç butonu kimliği.</summary>
    public const string BtnCancelLoan = "btn-cancel-loan";

    /// <summary>Kredi formu: planlı erken ödemeler listesi kimliği.</summary>
    public const string ListLoanPrepayments = "list-loan-prepayments";

    /// <summary>Kredi formu: erken ödeme girişini açan buton kimliği.</summary>
    public const string BtnOpenLoanPrepayment = "btn-open-loan-prepayment";

    /// <summary>Kredi formu: erken ödeme şekli seçicisi kimliği.</summary>
    public const string PickerPrepaymentMode = "picker-prepayment-mode";

    /// <summary>Kredi formu: erken ödeme tarihi seçicisi kimliği.</summary>
    public const string PickerPrepaymentDate = "picker-prepayment-date";

    /// <summary>Kredi formu: ara ödemede anaparadan düşecek tutar girişi kimliği.</summary>
    public const string InputPrepaymentAmount = "input-prepayment-amount";

    /// <summary>Kredi formu: erken ödemeyi listeye ekleyen buton kimliği.</summary>
    public const string BtnAddLoanPrepayment = "btn-add-loan-prepayment";

    /// <summary>Kredi formu: erken ödeme girişini kapatan buton kimliği.</summary>
    public const string BtnCancelLoanPrepayment = "btn-cancel-loan-prepayment";

    /// <summary>Gelir formu sayfası kimliği.</summary>
    public const string PageIncomeForm = "page-income-form";

    /// <summary>Gelir formu: gelir adı girişi kimliği.</summary>
    public const string InputIncomeName = "input-income-name";

    /// <summary>Gelir formu: yeni gelirin aylık net tutarı girişi kimliği.</summary>
    public const string InputIncomeAmount = "input-income-amount";

    /// <summary>Gelir formu: ödeme günü girişi kimliği.</summary>
    public const string InputIncomePaymentDay = "input-income-payment-day";

    /// <summary>Gelir formu: kaydet butonu kimliği.</summary>
    public const string BtnSaveIncome = "btn-save-income";

    /// <summary>Gelir formu: vazgeç butonu kimliği.</summary>
    public const string BtnCancelIncome = "btn-cancel-income";

    /// <summary>Gelir formu: tutar değişiklikleri listesi kimliği.</summary>
    public const string ListIncomeAmounts = "list-income-amounts";

    /// <summary>Gelir formu: tutar değişikliği girişini açan buton kimliği.</summary>
    public const string BtnOpenIncomeAmount = "btn-open-income-amount";

    /// <summary>Gelir formu: yeni tutar girişi kimliği.</summary>
    public const string InputIncomeNewAmount = "input-income-new-amount";

    /// <summary>Gelir formu: geçerlilik tarihi seçicisi kimliği.</summary>
    public const string PickerIncomeEffectiveDate = "picker-income-effective-date";

    /// <summary>Gelir formu: tutar değişikliğini listeye ekleyen buton kimliği.</summary>
    public const string BtnAddIncomeAmount = "btn-add-income-amount";

    /// <summary>Gelir formu: tutar değişikliği girişini kapatan buton kimliği.</summary>
    public const string BtnCancelIncomeAmount = "btn-cancel-income-amount";

    /// <summary>Tek seferlik gelir formu sayfası kimliği.</summary>
    public const string PageAdHocIncomeForm = "page-ad-hoc-income-form";

    /// <summary>Tek seferlik gelir formu: açıklama girişi kimliği.</summary>
    public const string InputAdHocIncomeDescription = "input-ad-hoc-income-description";

    /// <summary>Tek seferlik gelir formu: tutar girişi kimliği.</summary>
    public const string InputAdHocIncomeAmount = "input-ad-hoc-income-amount";

    /// <summary>Tek seferlik gelir formu: tarih seçicisi kimliği.</summary>
    public const string PickerAdHocIncomeDate = "picker-ad-hoc-income-date";

    /// <summary>Tek seferlik gelir formu: kaydet butonu kimliği.</summary>
    public const string BtnSaveAdHocIncome = "btn-save-ad-hoc-income";

    /// <summary>Tek seferlik gelir formu: vazgeç butonu kimliği.</summary>
    public const string BtnCancelAdHocIncome = "btn-cancel-ad-hoc-income";
}
