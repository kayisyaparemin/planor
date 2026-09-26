using Mizan.Presentation.Navigation;

namespace Mizan.App.Services;

/// <summary>
/// MAUI Shell altyapısını kullanarak UI iş parçacığında güvenli sayfa gezinmesi
/// ve modal pencere yönetimini gerçekleştiren somut navigasyon adaptörü.
/// </summary>
public sealed class MauiNavigationService : INavigationService
{
    /// <inheritdoc />
    public Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        if (MainThread.IsMainThread)
        {
            return Shell.Current is null
                ? Task.CompletedTask
                : parameters is not null
                    ? Shell.Current.GoToAsync(route, parameters)
                    : Shell.Current.GoToAsync(route);
        }

        return MainThread.InvokeOnMainThreadAsync(() =>
        {
            if (Shell.Current is null)
            {
                return Task.CompletedTask;
            }

            return parameters is not null
                ? Shell.Current.GoToAsync(route, parameters)
                : Shell.Current.GoToAsync(route);
        });
    }

    /// <inheritdoc />
    public Task NavigateBackAsync()
    {
        if (MainThread.IsMainThread)
        {
            return Shell.Current is null
                ? Task.CompletedTask
                : Shell.Current.GoToAsync("..");
        }

        return MainThread.InvokeOnMainThreadAsync(() =>
        {
            return Shell.Current is null
                ? Task.CompletedTask
                : Shell.Current.GoToAsync("..");
        });
    }

    /// <inheritdoc />
    public Task PopModalAsync()
    {
        if (MainThread.IsMainThread)
        {
            return Shell.Current?.Navigation?.ModalStack?.Count > 0
                ? Shell.Current.Navigation.PopModalAsync()
                : Task.CompletedTask;
        }

        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (Shell.Current?.Navigation?.ModalStack?.Count > 0)
            {
                await Shell.Current.Navigation.PopModalAsync();
            }
        });
    }
}
