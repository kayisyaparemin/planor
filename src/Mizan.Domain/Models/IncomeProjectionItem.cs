namespace Mizan.Domain.Models;

/// <summary>
/// Belirli bir nakit akış dönemine yansıtılan tekil bir gelir kalemini (düzenli veya tek seferlik) temsil eden değer nesnesi.
/// Dönemsel nakit akış projeksiyonunda gelir girişlerinin ayrıntılı dökümünü ve tarihsel sıralamasını sağlar.
/// </summary>
/// <param name="Name">Gelir kaleminin adı veya açıklaması.</param>
/// <param name="Type">Gelirin türü (Düzenli veya Tek Seferlik).</param>
/// <param name="SourceDate">Gelirin hesaba geçeceği öngörülen tahsilat tarihi.</param>
/// <param name="Amount">Gelirin kuruş hassasiyetindeki net tutarı.</param>
public sealed record IncomeProjectionItem(
    string Name,
    IncomeSourceType Type,
    DateOnly SourceDate,
    decimal Amount)
{
    /// <summary>Kaynak düzenli gelir akışının kimliği (varsa).</summary>
    public Guid? RecurringIncomeId { get; init; }

    /// <summary>Kaynak tek seferlik gelirin kimliği (varsa).</summary>
    public Guid? AdHocIncomeId { get; init; }
}
