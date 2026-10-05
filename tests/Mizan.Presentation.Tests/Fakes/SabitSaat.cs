using Mizan.Application.Abstractions;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Birim testlerinde deterministik zaman akışını sağlayan sabit saat. Varsayılan olarak UTC'de yaşar;
/// yerel saatin UTC'den farklı olduğu durumu <see cref="YerelSaatiAyarla"/> kurar.
/// </summary>
public sealed class SabitSaat : IClock
{
    public DateOnly Today { get; set; }
    public DateTimeOffset UtcNow { get; set; }
    public DateTimeOffset Now { get; set; }

    public SabitSaat(DateOnly? today = null)
    {
        Today = today ?? new DateOnly(2026, 9, 26);
        UtcNow = new DateTimeOffset(Today.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);
        Now = UtcNow;
    }

    /// <summary>Saati, verilen saat dilimindeki yerel ana kurar; bugün, UTC ve yerel saat birlikte değişir.</summary>
    public void YerelSaatiAyarla(DateTimeOffset yerel)
    {
        Now = yerel;
        UtcNow = yerel.ToUniversalTime();
        Today = DateOnly.FromDateTime(yerel.DateTime);
    }
}
