namespace Mizan.Infrastructure.LegacyImport;

/// <summary>
/// Eski uygulamanın (<c>com.coinflow.mobile</c>) yedek ve veritabanı düzeninin sabitleri. Eski adlar yalnız
/// bu klasörde yaşar (S83); üretim kodunun geri kalanı eski uygulamanın nasıl adlandırdığını bilmez.
/// </summary>
internal static class LegacySchemaV17
{
    /// <summary>
    /// Dönüştürülebilen tek eski şema sürümü. Eski uygulama bir veritabanını açılışta v17'ye yükseltiyordu;
    /// daha eskisi bu dönüşümün kolonlarını taşımaz, daha yenisi hiç olmadı.
    /// </summary>
    public const int SupportedVersion = 17;

    /// <summary>
    /// Eski veritabanının dönüşüm bağlantısında takılacağı ad; SQL'deki <c>old.</c> öneki.
    /// </summary>
    public const string AttachedAlias = "old";

    /// <summary>
    /// Eski tek gelir geçmişinden (<c>salary_schedule</c>) kurulan düzenli gelir akışının adı (S83-5).
    /// </summary>
    public const string IncomeStreamName = "Gelir";

    /// <summary>
    /// Eski yedekte bir profilin veritabanı girdisinin adı.
    /// </summary>
    public static string DatabaseEntryName(Guid profileId) =>
        $"profiles/{profileId:N}/coinflow.db3";

    /// <summary>
    /// SQLite'ta rastgele sürüm 4 GUID'i metin olarak üreten ifade; satır başına yeniden hesaplanır, bu yüzden
    /// bir görünüm ya da alt sorgu yerine her ekleme sorgusunda satır içi kullanılır.
    /// </summary>
    public const string NewGuidExpression =
        "lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || " +
        "substr(lower(hex(randomblob(2))), 2) || '-' || substr('89ab', 1 + (abs(random()) % 4), 1) || " +
        "substr(lower(hex(randomblob(2))), 2) || '-' || lower(hex(randomblob(6)))";
}
