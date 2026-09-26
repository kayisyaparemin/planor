using System.Collections.Concurrent;
using Android.App;
using Android.Content;

namespace Mizan.App.Platforms.Android;

/// <summary>
/// Başka bir platform ekranını (dosya seçici, depolama izni ekranı) açıp dönüş sonucunu bekleten yardımcı sınıf.
/// </summary>
public static class ActivityResults
{
    private static readonly ConcurrentDictionary<int, TaskCompletionSource<(Result ResultCode, Intent? Data)>> Pending = new();
    private static int nextRequestCode = 7300;

    /// <summary>
    /// Verilen Intent'i ActivityResult bekleyecek şekilde başlatır.
    /// </summary>
    public static Task<(Result ResultCode, Intent? Data)> StartAsync(Intent intent)
    {
        var activity = Platform.CurrentActivity ??
                       throw new InvalidOperationException("Ekran açılamadı.");
        var requestCode = Interlocked.Increment(ref nextRequestCode);
        var pending = new TaskCompletionSource<(Result, Intent?)>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        Pending[requestCode] = pending;
        try
        {
            activity.StartActivityForResult(intent, requestCode);
        }
        catch
        {
            Pending.TryRemove(requestCode, out _);
            throw;
        }

        return pending.Task;
    }

    /// <summary>
    /// MainActivity'den gelen Activity sonucunu ilgili bekleyen göreve iletir.
    /// </summary>
    public static void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        if (Pending.TryRemove(requestCode, out var pending))
        {
            pending.TrySetResult((resultCode, data));
        }
    }
}
