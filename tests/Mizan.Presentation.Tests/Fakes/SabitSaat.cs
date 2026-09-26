using Mizan.Application.Abstractions;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Birim testlerinde deterministik zaman akışını sağlayan sabit saat.
/// </summary>
public sealed class SabitSaat : IClock
{
    public DateOnly Today { get; set; }
    public DateTimeOffset UtcNow { get; set; }

    public SabitSaat(DateOnly? today = null)
    {
        Today = today ?? new DateOnly(2026, 9, 26);
        UtcNow = new DateTimeOffset(Today.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);
    }
}
