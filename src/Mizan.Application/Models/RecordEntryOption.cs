namespace Mizan.Application.Models;

/// <summary>
/// Finansal Yapı ekranında "+ Ekle" butonu ile açılan kayıt türü kartı sözleşmesi.
/// Ortak formdan girilen kayıtlar bağlı oldukları simülatör seçeneğini de taşır.
/// </summary>
public sealed record RecordEntryOption
{
    /// <summary>Kayıt giriş seçeneğinin benzersiz anahtarı.</summary>
    public required string Key { get; init; }

    /// <summary>Seçeneğin ait olduğu kayıt kategorisi grubu.</summary>
    public required RecordEntryGroup Group { get; init; }

    /// <summary>Kartın kullanıcıya gösterilen başlığı.</summary>
    public required string Title { get; init; }

    /// <summary>Kartın kısa özeti.</summary>
    public required string Summary { get; init; }

    /// <summary>Kartın detaylı açıklaması.</summary>
    public required string Description { get; init; }

    /// <summary>Kaydın hangi form bileşeni üzerinden girileceği.</summary>
    public required RecordEntryForm Form { get; init; }

    /// <summary>Ortak formdan girilen kayıtlar için bağlı simülatör seçeneği.</summary>
    public ScenarioOption? Scenario { get; init; }
}
