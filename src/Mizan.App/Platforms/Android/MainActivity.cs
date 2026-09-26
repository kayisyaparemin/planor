using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Mizan.App.Platforms.Android;

namespace Mizan.App;

/// <summary>
/// Android platformunun ana etkinlik (MainActivity) giriş noktası.
/// </summary>
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    /// <inheritdoc />
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        ActivityResults.OnActivityResult(requestCode, resultCode, data);
    }
}
