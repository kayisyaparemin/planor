namespace Mizan.Presentation.Services;

/// <summary>
/// Harici depodan dosya seçme veya depodaki bir yedeği akış olarak açma sözleşmesi.
/// </summary>
public interface IBackupFilePicker
{
    /// <summary>
    /// Kullanıcıya sistem dosya seçicisini açtırır ve seçilen dosyanın akışını döner; iptalde null döner.
    /// </summary>
    Task<Stream?> PickAndOpenAsync();

    /// <summary>
    /// Harici depoda bulunan belirtilen dosya adlı yedeği salt-okunur akış olarak açar.
    /// </summary>
    Task<Stream> OpenStoredAsync(string fileName);
}
