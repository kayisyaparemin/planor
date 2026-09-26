using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Kullanıcının profiller arasında geçiş yapmasını veya yeni profil oluşturmasını sağlayan sayfa.
/// </summary>
public partial class ProfileSelectionPage : ContentPage
{
    private readonly ProfileSelectionViewModel? viewModel;

    /// <summary>
    /// <see cref="ProfileSelectionPage"/> sınıfının yeni bir örneğini başlatır.
    /// </summary>
    public ProfileSelectionPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Belirtilen ViewModel ile <see cref="ProfileSelectionPage"/> sınıfının yeni bir örneğini başlatır.
    /// </summary>
    /// <param name="viewModel">Profil seçimi görünüm modeli.</param>
    public ProfileSelectionPage(ProfileSelectionViewModel viewModel) : this()
    {
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    /// <inheritdoc />
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (viewModel is not null)
        {
            try
            {
                await viewModel.LoadCommand.ExecuteAsync(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Profil sayfası yükleme hatası: {ex}");
            }
        }
    }
}
