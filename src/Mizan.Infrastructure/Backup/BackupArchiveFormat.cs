namespace Mizan.Infrastructure.Backup;

/// <summary>
/// Yedek zip'inin biçim sözleşmesi: biçim numarası ve girdi adları.
/// Diskteki profil yerleşiminden bilerek ayrı tutulur; disk yerleşimi değişse bile
/// telefonda duran eski yedekler aynı adlarla okunabilmelidir (S57).
/// </summary>
internal static class BackupArchiveFormat
{
    /// <summary>
    /// Bu uygulamanın yazdığı yedek biçimi. Biçim 1 eski uygulamanın (şema v17) yedeğidir;
    /// aynı manifest alanlarını taşıdığı için yalnız bu numarayla ayırt edilir.
    /// </summary>
    public const int Version = 2;

    /// <summary>
    /// Eski uygulamanın (<c>com.coinflow.mobile</c>) yazdığı yedek biçimi. Bu uygulama onu geri yüklemez,
    /// yalnız tanıyıp açık mesajla reddeder; içe aktarma ayrı bir iştir (G1).
    /// </summary>
    public const int LegacyVersion = 1;

    /// <summary>
    /// Biçimi, tarihi, şema sürümünü ve profil listesini taşıyan manifest girdisinin adı.
    /// </summary>
    public const string ManifestEntryName = "mizan-backup.json";

    /// <summary>
    /// Profilin veritabanı anlık görüntüsünün zip içindeki girdi adı.
    /// </summary>
    public static string DatabaseEntryName(Guid profileId) =>
        $"profiles/{profileId:N}/mizan.db3";
}
