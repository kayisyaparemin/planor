using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Kullanıcının simülatörde veya Finansal Yapı'da seçtiği plan türü kartı sözleşmesi.
/// Bir veya birden fazla alt motor senaryo türünü tek bir kullanıcı dostu kart altında birleştirir.
/// </summary>
public sealed record ScenarioOption
{
    /// <summary>Seçeneğin benzersiz anahtarı.</summary>
    public required string Key { get; init; }

    /// <summary>Seçeneğin ait olduğu üst işlevsel grup.</summary>
    public required ScenarioGroup Group { get; init; }

    /// <summary>Kartın kullanıcıya gösterilen başlığı.</summary>
    public required string Title { get; init; }

    /// <summary>Kartın kısa özeti.</summary>
    public required string Summary { get; init; }

    /// <summary>Kartın detaylı açıklaması.</summary>
    public required string Description { get; init; }

    /// <summary>Bu seçeneğin kapsadığı alt motor senaryo türleri.</summary>
    public required IReadOnlyList<SimulationScenarioType> Types { get; init; }

    /// <summary>Seçeneğin simülatör dışından hangi ekranda doğrudan girilebileceği.</summary>
    public required ScenarioEntryHome EntryHome { get; init; }

    /// <summary>Bu seçeneğin varsayılan motor senaryo türü.</summary>
    public SimulationScenarioType DefaultType => Types[0];
}
