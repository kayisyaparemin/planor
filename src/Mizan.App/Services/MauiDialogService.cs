using Mizan.Presentation.Dialogs;

namespace Mizan.App.Services;

/// <summary>
/// MAUI sayfa hiyerarşisi üzerinden kullanıcıya uyarı, onay, metin girişi
/// ve işlem seçim sayfalarını sunan somut diyalog servisi adaptörü.
/// </summary>
public sealed class MauiDialogService : IDialogService
{
    /// <inheritdoc />
    public async Task ShowAlertAsync(string title, string message, string button = "Tamam")
    {
        try
        {
            if (MainThread.IsMainThread)
            {
                await CurrentPage().DisplayAlert(title, message, button);
            }
            else
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    CurrentPage().DisplayAlert(title, message, button));
            }
        }
        catch
        {
            // Pencere veya sayfa hazır değilse hata bastırılır; uygulama çökmez.
        }
    }

    /// <inheritdoc />
    public async Task<bool> ConfirmAsync(string title, string message, string accept = "Evet", string cancel = "Hayır")
    {
        try
        {
            return MainThread.IsMainThread
                ? await CurrentPage().DisplayAlert(title, message, accept, cancel)
                : await MainThread.InvokeOnMainThreadAsync(() =>
                    CurrentPage().DisplayAlert(title, message, accept, cancel));
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<string?> PromptAsync(string title, string message, string accept = "Tamam", string cancel = "Vazgeç", string placeholder = "", int maxLength = -1)
    {
        try
        {
            return MainThread.IsMainThread
                ? await CurrentPage().DisplayPromptAsync(title, message, accept, cancel, placeholder, maxLength)
                : await MainThread.InvokeOnMainThreadAsync(() =>
                    CurrentPage().DisplayPromptAsync(title, message, accept, cancel, placeholder, maxLength));
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<string?> ChooseAsync(string title, string cancel, string? destruction, params string[] options)
    {
        try
        {
            if (MainThread.IsMainThread)
            {
                var choice = await CurrentPage().DisplayActionSheet(title, cancel, destruction, options);
                return choice is null || choice == cancel ? null : choice;
            }

            return await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var choice = await CurrentPage().DisplayActionSheet(title, cancel, destruction, options);
                return choice is null || choice == cancel ? null : choice;
            });
        }
        catch
        {
            return null;
        }
    }

    private static Page CurrentPage()
    {
        Page? page = null;
        if (Shell.Current?.CurrentPage is { } shellPage)
        {
            page = shellPage;
        }
        else if (Microsoft.Maui.Controls.Application.Current?.MainPage is { } mainPage)
        {
            page = mainPage;
        }
        else if (Microsoft.Maui.Controls.Application.Current?.Windows.Count > 0 &&
                 Microsoft.Maui.Controls.Application.Current.Windows[0]?.Page is { } windowPage)
        {
            page = windowPage;
        }

        return page is null
            ? throw new InvalidOperationException("Geçerli ekran bulunamadı.")
            : ResolveTopPage(page);
    }

    private static Page ResolveTopPage(Page page)
    {
        var modalStack = page.Navigation.ModalStack;
        var top = modalStack.Count > 0 ? modalStack[^1] : page;
        return DescendToVisiblePage(top);
    }

    private static Page DescendToVisiblePage(Page page) => page switch
    {
        NavigationPage { CurrentPage: { } current } => DescendToVisiblePage(current),
        FlyoutPage { Detail: { } detail } => DescendToVisiblePage(detail),
        TabbedPage { CurrentPage: { } current } => DescendToVisiblePage(current),
        _ => page
    };
}
