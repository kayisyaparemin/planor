using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Dönem kapanışı özet ekranı: dönem sonu gerçekleşen bakiyesini, sapmayı,
/// yaşam harcaması ve ödemelerin karne farklarını tek ekranda sunar (EK-V11).
/// </summary>
public partial class PeriodSettlementPage : ContentPage
{
    private readonly PeriodSettlementViewModel? viewModel;

    /// <summary>
    /// <see cref="PeriodSettlementPage"/> sınıfının yeni bir örneğini başlatır.
    /// </summary>
    public PeriodSettlementPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Belirtilen ViewModel ile <see cref="PeriodSettlementPage"/> sınıfının yeni bir örneğini başlatır.
    /// </summary>
    /// <param name="viewModel">Dönem kapanışı görünüm modeli.</param>
    public PeriodSettlementPage(PeriodSettlementViewModel viewModel) : this()
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
                System.Diagnostics.Debug.WriteLine($"PeriodSettlementPage yükleme hatası: {ex}");
            }
        }
    }
}
