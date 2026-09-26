namespace Mizan.Presentation.Navigation;

/// <summary>
/// ViewModel'lerin platform veya MAUI UI katmanına doğrudan bağımlı olmadan
/// rota bazlı gezinme ve modal yönetimi gerçekleştirmesini sağlayan navigasyon sözleşmesi.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Belirtilen uygulama içi rotaya isteğe bağlı parametrelerle gezinir.
    /// </summary>
    Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null);

    /// <summary>
    /// Gezinme yığınında bir önceki sayfaya geri döner.
    /// </summary>
    Task NavigateBackAsync();

    /// <summary>
    /// En üstteki aktif modal pencereyi kapatır.
    /// </summary>
    Task PopModalAsync();
}
