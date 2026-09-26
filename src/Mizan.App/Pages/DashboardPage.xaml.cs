using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Kullanıcının aktif nakit akış döneminin gidişatını, kalan bütçe halkasını,
/// acil hatırlatıcılarını ve dönem sonu projeksiyonunu sunan ana sayfa.
/// </summary>
public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel? viewModel;

    /// <summary>
    /// <see cref="DashboardPage"/> sınıfının yeni bir örneğini başlatır.
    /// </summary>
    public DashboardPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Belirtilen ViewModel ile <see cref="DashboardPage"/> sınıfının yeni bir örneğini başlatır.
    /// </summary>
    /// <param name="viewModel">Ana sayfa görünüm modeli.</param>
    public DashboardPage(DashboardViewModel viewModel) : this()
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
                System.Diagnostics.Debug.WriteLine($"DashboardPage yükleme hatası: {ex}");
            }
        }
    }
}
