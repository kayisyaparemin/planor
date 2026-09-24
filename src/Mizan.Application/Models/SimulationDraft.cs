namespace Mizan.Application.Models;

/// <summary>
/// Kullanıcının simülatörde oluşturduğu, adlandırdığı ve sakladığı varsayımsal koşullar bütünü.
/// Canlı finansal plana veya nakit akış projeksiyonuna doğrudan dahil olmaz;
/// simülatöre geri yüklenmek üzere kalıcı olarak saklanır.
/// </summary>
public sealed record SimulationDraft(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<SimulationDraftCondition> Conditions)
{
    /// <summary>
    /// Taslak içindeki koşulların listesi (boş olamaz, null ise boş liste atanır).
    /// </summary>
    public IReadOnlyList<SimulationDraftCondition> Conditions { get; init; } = Conditions ?? Array.Empty<SimulationDraftCondition>();

    /// <summary>
    /// Taslak içindeki aktif (açık) durumdaki koşulların adedi.
    /// </summary>
    public int EnabledConditionCount => Conditions.Count(c => c.IsEnabled);

    /// <summary>
    /// Taslak içinde en az bir aktif koşul olup olmadığını belirtir.
    /// </summary>
    public bool HasEnabledConditions => EnabledConditionCount > 0;
}
