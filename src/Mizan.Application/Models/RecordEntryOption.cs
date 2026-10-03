namespace Mizan.Application.Models;

/// <summary>
/// Kayıt türü seçicideki tek bir seçenek: hangi grupta durduğu ve hangi formu açtığı. Metin taşımaz;
/// başlık ve alt satır App'in metin kataloğunda anahtarla bulunur (kural 03, GK5). Anahtarlar simülatör
/// kataloğundaki ortak türlerle aynıdır ki iki ekran aynı metni kullanabilsin (S77-6).
/// </summary>
public sealed record RecordEntryOption
{
    /// <summary>Seçeneğin benzersiz anahtarı; görünen metin bu anahtarla bulunur.</summary>
    public required string Key { get; init; }

    /// <summary>Seçeneğin durduğu grup.</summary>
    public required RecordEntryGroup Group { get; init; }

    /// <summary>Seçeneğin açtığı form.</summary>
    public required RecordEntryForm Form { get; init; }
}
