using Mizan.Application.Abstractions;

namespace Mizan.Infrastructure.Time;

/// <summary>
/// Gerçek işletim sistemi saatini sağlayan IClock adaptörü.
/// Yalnızca çalışma zamanında (DI kökünde) kullanılır; testlerde sahte (fake) saat kullanılır.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);

    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
