namespace Mizan.Infrastructure.Backup;

/// <summary>
/// Yedek tanınırken ya da geri yüklenirken kullanıcıya giden hata mesajları. Manifest ve veritabanı
/// denetimleri aynı durumu aynı cümleyle söylesin diye tek yerde durur.
/// </summary>
internal static class BackupRestoreErrors
{
    /// <summary>
    /// Dosya zip değil, manifesti yok ya da manifest bir Mizan yedeğine ait değil.
    /// </summary>
    public static InvalidOperationException NotABackup(Exception? inner = null) =>
        new("Bu dosya bir Mizan yedeği değil.", inner);

    /// <summary>
    /// Dosya eski uygulamanın yedeği (biçim 1); bu uygulama onu geri yüklemez.
    /// </summary>
    public static InvalidOperationException LegacyBackup() =>
        new("Bu yedek eski Mizan uygulamasından alınmış; bu sürümde geri yüklenemez.");

    /// <summary>
    /// Eski uygulamadan içe aktarmaya başka bir dosya verildi (örn. Planör'ün kendi yedeği).
    /// </summary>
    public static InvalidOperationException NotLegacyBackup() =>
        new("Bu yedek eski Mizan uygulamasından alınmamış.");

    /// <summary>
    /// Yedeğin biçimi ya da şema sürümü bu uygulamanın tanıdığından yeni.
    /// </summary>
    public static InvalidOperationException NewerVersion() =>
        new("Bu yedek Mizan'ın daha yeni bir sürümünden alınmış. Önce uygulamayı güncelle.");

    /// <summary>
    /// Dosya bir Mizan yedeği ama içeriği geri yüklenemiyor; <paramref name="reason"/> hangi parçanın
    /// bozuk olduğunu söyler.
    /// </summary>
    public static InvalidOperationException Corrupt(string reason, Exception? inner = null) =>
        new($"Yedek geri yüklenemedi: {reason}", inner);
}
