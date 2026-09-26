using Mizan.App.Resources.Styles;

namespace Mizan.App;

/// <summary>
/// Uygulamanın ana giriş noktası ve tema yönetim kökü.
/// </summary>
public partial class App : Microsoft.Maui.Controls.Application
{
    /// <summary>
    /// <see cref="App"/> sınıfının yeni bir örneğini başlatır.
    /// </summary>
    public App()
    {
        InitializeComponent();

        ApplyPalette(RequestedTheme);
        RequestedThemeChanged += (_, e) => ApplyPalette(e.RequestedTheme);

        MainPage = new AppShell();
    }

    /// <summary>
    /// <see cref="App"/> sınıfının belirtilen kabuk ile yeni bir örneğini başlatır.
    /// </summary>
    /// <param name="appShell">Uygulama kabuğu.</param>
    public App(AppShell appShell) : this()
    {
        MainPage = appShell;
    }

    private void ApplyPalette(AppTheme theme)
    {
        var dictionaries = Resources.MergedDictionaries;
        var current = dictionaries.FirstOrDefault(d => d is DarkPalette or LightPalette);
        if (current is not null)
        {
            dictionaries.Remove(current);
        }

        dictionaries.Add(theme == AppTheme.Light ? new LightPalette() : new DarkPalette());
    }
}
