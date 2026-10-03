namespace Mizan.Application.Models;

/// <summary>
/// Finansal Yapı'nın kayıt türü seçicisinde sunulan seçenekler: listedeki dört grup (Gelir, Kart, Kredi,
/// Ödeme) ve her grubun ≤ 4 seçeneği, her biri Planör'deki bir forma bağlı (S77-3). Eski katalog altı
/// seçeneği simülatörün ortak formundan türetiyordu; o form Finansal Yapı'da yok (S62-7), bu yüzden
/// seçenekler burada doğrudan yazılır. "Kredi / finansman çek" ortak form gelince eklenir (V10d).
/// </summary>
public static class FinancialRecordEntryCatalog
{
    /// <summary>Bir grupta sunulabilecek en fazla seçenek: iki sütunlu ızgarada iki satır (GS29).</summary>
    public const int MaxOptionsPerGroup = 4;

    /// <summary>Grupların seçicideki sırası; Finansal Yapı listesinin sırası.</summary>
    public static IReadOnlyList<RecordEntryGroup> Groups { get; } =
        [RecordEntryGroup.Income, RecordEntryGroup.Card, RecordEntryGroup.Loan, RecordEntryGroup.Payment];

    /// <summary>Bütün seçenekler, grup sırasıyla. Ortak anahtarlar simülatör kataloğundakilerle aynıdır.</summary>
    public static IReadOnlyList<RecordEntryOption> Options { get; } =
    [
        Option("recurring-income", RecordEntryGroup.Income, RecordEntryForm.RecurringIncome),
        Option("income", RecordEntryGroup.Income, RecordEntryForm.AdHocIncome),
        Option("credit-card", RecordEntryGroup.Card, RecordEntryForm.CreditCard),
        Option("bank-loan", RecordEntryGroup.Loan, RecordEntryForm.Loan),
        Option("cash", RecordEntryGroup.Payment, RecordEntryForm.PlannedExpense),
        Option("recurring", RecordEntryGroup.Payment, RecordEntryForm.PaymentPlan),
        Option("cash-debt", RecordEntryGroup.Payment, RecordEntryForm.PaymentPlan),
        Option("payment-plan", RecordEntryGroup.Payment, RecordEntryForm.PaymentPlan)
    ];

    /// <summary>Bir grubun seçenekleri, sırasıyla.</summary>
    public static IReadOnlyList<RecordEntryOption> OptionsIn(RecordEntryGroup group) =>
        Options.Where(x => x.Group == group).ToArray();

    /// <summary>Anahtarı verilen seçenek; seçicinin öğesinden kataloğa dönmek için.</summary>
    public static RecordEntryOption For(string key) => Options.Single(x => x.Key == key);

    private static RecordEntryOption Option(string key, RecordEntryGroup group, RecordEntryForm form) =>
        new() { Key = key, Group = group, Form = form };
}
