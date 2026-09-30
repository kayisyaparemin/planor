using AndroidContext = Android.Content.Context;
using AndroidMotionEvent = Android.Views.MotionEvent;
using AndroidMotionEventActions = Android.Views.MotionEventActions;
using AndroidView = Android.Views.View;
using AndroidViewConfiguration = Android.Views.ViewConfiguration;

namespace Mizan.App.Platforms.Android;

/// <summary>
/// Kaydırılan hero kartında yatay kaydırmayı sayfanın dikey kaydırmasından ayırır. Kart dikey kayan sayfanın
/// içinde durur; parmak yatay giderken biraz aşağı yukarı oynayınca Android dokunuşu sayfaya verir ve MAUI'nin
/// kaydırma tanıyıcısı iptal olurdu (V3a Kapı C: "kaydırmak çok zor"). Parmak yatay gitmeye başlayınca sayfanın
/// o dokunuşu almasını engeller; bırakınca yeterince yol alındıysa bir sayfa ilerler ya da geri gelir. Sayfa
/// parmağı takip etmez, bırakınca değişir (GS24: animasyonsuz).
/// </summary>
internal sealed class HeroPagerSwipeListener : Java.Lang.Object, AndroidView.IOnTouchListener
{
    // Bir sayfa geçişi için parmağın yatayda alması gereken yol (dp); MAUI tanıyıcısının varsayılanı 100'dü.
    private const float SwipeDistanceDp = 48f;

    private readonly Action<int> _onSwipe;
    private readonly float _touchSlop;
    private readonly float _swipeDistance;
    private float _startX;
    private float _startY;
    private bool _isHorizontal;

    /// <summary>Dinleyiciyi kurar; <paramref name="onSwipe"/> sonraki sayfa için +1, önceki için -1 alır.</summary>
    public HeroPagerSwipeListener(AndroidContext context, Action<int> onSwipe)
    {
        _onSwipe = onSwipe;
        _touchSlop = AndroidViewConfiguration.Get(context)?.ScaledTouchSlop ?? 0;
        _swipeDistance = SwipeDistanceDp * (context.Resources?.DisplayMetrics?.Density ?? 1f);
    }

    /// <inheritdoc />
    public bool OnTouch(AndroidView? view, AndroidMotionEvent? motion)
    {
        if (view is null || motion is null) { return false; }

        switch (motion.ActionMasked)
        {
            case AndroidMotionEventActions.Down:
                _startX = motion.RawX;
                _startY = motion.RawY;
                _isHorizontal = false;
                break;
            case AndroidMotionEventActions.Move:
                ClaimHorizontalDrag(view, motion);
                break;
            case AndroidMotionEventActions.Up:
                FinishSwipe(motion);
                break;
        }
        return true;
    }

    private void ClaimHorizontalDrag(AndroidView view, AndroidMotionEvent motion)
    {
        var horizontal = Math.Abs(motion.RawX - _startX);
        var vertical = Math.Abs(motion.RawY - _startY);
        if (_isHorizontal || horizontal <= _touchSlop || horizontal <= vertical) { return; }

        // Dikey hareket önce eşiği geçerse sayfa kayar; yatay önce geçerse dokunuş bu kartındır.
        _isHorizontal = true;
        view.Parent?.RequestDisallowInterceptTouchEvent(true);
    }

    private void FinishSwipe(AndroidMotionEvent motion)
    {
        var distance = motion.RawX - _startX;
        if (!_isHorizontal || Math.Abs(distance) < _swipeDistance) { return; }

        // Sola kaydırmak sonraki sayfayı getirir.
        _onSwipe(distance < 0 ? 1 : -1);
    }
}
