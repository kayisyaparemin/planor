using Mizan.App.Pages;
using Mizan.Presentation.Navigation;

namespace Mizan.App.Services;

/// <summary>
/// MAUI Shell altyapısını kullanarak UI iş parçacığında güvenli sayfa gezinmesi
/// ve kök sayfa yönetimini gerçekleştiren somut navigasyon adaptörü.
/// </summary>
public sealed class MauiNavigationService(IServiceProvider services) : INavigationService
{
    /// <inheritdoc />
    public Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        return MainThread.InvokeOnMainThreadAsync(() =>
        {
            if (string.Equals(route, Routes.Dashboard, StringComparison.Ordinal))
            {
                if (Microsoft.Maui.Controls.Application.Current is { } app)
                {
                    app.MainPage = services.GetRequiredService<AppShell>();
                    return;
                }
            }
            else if (string.Equals(route, Routes.ProfileSelection, StringComparison.Ordinal))
            {
                if (Microsoft.Maui.Controls.Application.Current is { } app)
                {
                    app.MainPage = services.GetRequiredService<ProfileSelectionPage>();
                    return;
                }
            }

            if (Shell.Current is not null)
            {
                _ = parameters is not null
                    ? Shell.Current.GoToAsync(route, parameters)
                    : Shell.Current.GoToAsync(route);
            }
        });
    }

    /// <inheritdoc />
    public Task NavigateBackAsync()
    {
        return MainThread.InvokeOnMainThreadAsync(() =>
        {
            if (Shell.Current is not null)
            {
                _ = Shell.Current.GoToAsync("..");
            }
        });
    }

    /// <inheritdoc />
    public Task PopModalAsync()
    {
        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (Shell.Current?.Navigation?.ModalStack?.Count > 0)
            {
                await Shell.Current.Navigation.PopModalAsync();
            }
        });
    }
}
