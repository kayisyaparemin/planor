namespace Mizan.Application.Models;

/// <summary>
/// Kayıt türü seçicideki tek bir seçenek: hangi grupta durduğu, hangi formu açtığı ve formun yeni mi var
/// olan bir kayıt için mi açıldığı. Metin taşımaz;
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

    /// <summary>
    /// Seçenek yeni bir kayıt açmaz, var olan bir kayda bir şey ekler (kartla harcama karta, erken ödeme
    /// krediye, yeni tutar gelire); seçici formu açmadan önce hangi kayda olduğunu çözer (S77-5).
    /// </summary>
    public bool EditsExisting { get; init; }
}
