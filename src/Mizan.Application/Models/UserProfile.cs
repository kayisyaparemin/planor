namespace Mizan.Application.Models;

/// <summary>
/// Uygulamayı kullanan kişi. Her profilin finans verisi kendi veritabanında
/// durur; profiller arasında hiçbir kayıt paylaşılmaz.
/// </summary>
public sealed record UserProfile
{
    /// <summary>
    /// Profil öncesi sürümden gelen tek veritabanının taşındığı profilin adı.
    /// Adı kaybolmuş (meta dosyası okunamayan) profil de bu adla listelenir.
    /// </summary>
    public const string DefaultName = "Profilim";

    /// <summary>
    /// Profil adının kabul edilen en uzun karakter sayısı.
    /// </summary>
    public const int MaxNameLength = 30;

    /// <summary>
    /// Profilin kimliği; profilin veritabanı dosya adı bu değerden türetilir.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Profil seçim ekranında gösterilen ad.
    /// </summary>
    public string Name { get; init; } = DefaultName;

    /// <summary>
    /// Profilin oluşturulduğu an.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Profilin en son açıldığı an; hiç açılmadıysa <c>null</c>.
    /// </summary>
    public DateTimeOffset? LastOpenedAt { get; init; }
}
