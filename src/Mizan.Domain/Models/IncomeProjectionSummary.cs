namespace Mizan.Domain.Models;

/// <summary>
/// Bir nakit akış dönemine ait tüm gelir kalemlerini ve tür bazlı toplamları özetleyen sonuç kaydı.
/// 12 dönemlik projeksiyon motorunun o dönemdeki toplam nakit girişini belirlemesinde kullanılır.
/// </summary>
/// <param name="Items">Dönem içine düşen ve tarihe göre sıralanmış tüm gelir kalemleri dökümü.</param>
/// <param name="RecurringTotal">Dönemdeki düzenli gelir akışlarının toplam tutarı.</param>
/// <param name="AdHocTotal">Dönemdeki tek seferlik arızi gelirlerin toplam tutarı.</param>
/// <param name="TotalIncome">Dönemdeki tüm gelirlerin genel toplamı (RecurringTotal + AdHocTotal).</param>
public sealed record IncomeProjectionSummary(
    IReadOnlyList<IncomeProjectionItem> Items,
    decimal RecurringTotal,
    decimal AdHocTotal,
    decimal TotalIncome);
