using CommunityToolkit.Mvvm.ComponentModel;
using Mizan.Presentation.Models;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Tüm ViewModel sınıflarının temelini oluşturan, meşguliyet durumu ve sayfa durumunu
/// MAUI katmanından bağımsız olarak yöneten temel sınıf.
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string busyMessage = string.Empty;

    [ObservableProperty]
    private ScreenState state = ScreenState.Content;

    /// <summary>
    /// Ekranın kullanıcı etkileşimine açık olup olmadığını belirtir.
    /// Meşguliyet durumunun (<see cref="IsBusy"/>) tersidir.
    /// </summary>
    public bool CanInteract => !IsBusy;

    /// <summary>
    /// Meşguliyet durumunu ve isteğe bağlı açıklamasını günceller.
    /// </summary>
    /// <param name="busy">İşlemin devam edip etmediği.</param>
    /// <param name="message">Kullanıcıya gösterilecek meşguliyet bildirimi.</param>
    public void SetBusy(bool busy, string message = "")
    {
        IsBusy = busy;
        BusyMessage = message;
    }

    partial void OnIsBusyChanged(bool value) =>
        OnPropertyChanged(nameof(CanInteract));
}
