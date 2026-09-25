namespace Mizan.Infrastructure.Persistence;

/// <summary>
/// Profilin disk üzerinde profile.json dosyasında saklanan ad ve zaman damgası meta verisi.
/// </summary>
internal sealed record ProfileMetadata(
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastOpenedAt);
