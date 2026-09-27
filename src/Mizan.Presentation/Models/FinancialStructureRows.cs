namespace Mizan.Presentation.Models;

/// <summary>
/// Finansal Yapı ekranının dört grubu, satırları sıralanmış hâlde. Plandan tek geçişte üretilir ki
/// ekran hangi kaydın hangi gruba düştüğünü kendisi bilmek zorunda kalmasın (EK-V6, S62-1).
/// </summary>
/// <param name="Incomes">Düzenli ve tek seferlik gelirler.</param>
/// <param name="Cards">Kredi kartları.</param>
/// <param name="Loans">Krediler.</param>
/// <param name="Payments">Ödeme planları ve planlı büyük harcamalar.</param>
public sealed record FinancialStructureRows(
    IReadOnlyList<FinancialRecordRow> Incomes,
    IReadOnlyList<FinancialRecordRow> Cards,
    IReadOnlyList<FinancialRecordRow> Loans,
    IReadOnlyList<FinancialRecordRow> Payments)
{
    /// <summary>Hiç satırı olmayan yapı.</summary>
    public static FinancialStructureRows Empty { get; } = new([], [], [], []);

    /// <summary>Dört grubun hiçbirinde satır yoksa true; ekran boş durumu gösterir.</summary>
    public bool IsEmpty => Incomes.Count + Cards.Count + Loans.Count + Payments.Count == 0;
}
