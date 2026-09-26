namespace Mizan.Presentation.Dialogs;

/// <summary>
/// ViewModel katmanının MAUI kullanıcı arayüzü bileşenlerine doğrudan erişmeden
/// kullanıcıya uyarı, onay veya seçim pencereleri sunmasını sağlayan servis sözleşmesi.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Kullanıcıya tek butonlu bir bilgilendirme veya uyarı mesajı gösterir.
    /// </summary>
    Task ShowAlertAsync(string title, string message, string button = "Tamam");

    /// <summary>
    /// Kullanıcıya iki seçenekli (onay/red) bir soru yöneltir ve cevabı döner.
    /// </summary>
    Task<bool> ConfirmAsync(string title, string message, string accept = "Evet", string cancel = "Hayır");

    /// <summary>
    /// Kullanıcıdan tek satırlık metin girdisi talep eder; iptal edilirse null döner.
    /// </summary>
    Task<string?> PromptAsync(string title, string message, string accept = "Tamam", string cancel = "Vazgeç", string placeholder = "", int maxLength = -1);

    /// <summary>
    /// Kullanıcıya bir seçenekler listesi sunar; seçilen seçeneğin metnini döner.
    /// </summary>
    Task<string?> ChooseAsync(string title, string cancel, string? destruction, params string[] options);
}
