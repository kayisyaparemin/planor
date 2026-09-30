using Microsoft.Maui.Graphics;

namespace Mizan.App.Charts;

/// <summary>
/// "Ne kadarı tamamlandı, geçen süreye göre önde miyiz geride mi?" sorusunu cevaplar.
/// Dönem içi ödemelerin veya bütçe hedefinin tamamlanma oranını tek bir dairesel halka göstergesiyle sunar;
/// verilmişse geçen sürenin oranı halkayı kesen bir işaretle gösterilir. Dolgu işaretin önündeyse tempo
/// hızlı, gerisindeyse yavaştır.
/// </summary>
public sealed class RingGauge : IDrawable
{
    private const float StrokeThickness = 10f;
    private const float TimeMarkerOverhang = 6f;
    private const float TimeMarkerHalfAngle = 1.5f;

    // MAUI açıyı saat 3'ten saatin tersine sayar: tepe 90 derecedir ve saat yönünde ilerlemek açıyı azaltır.
    private const float TopAngle = 90f;

    /// <summary>
    /// Tamamlanma oranı (0.0 ile 1.0 arasında).
    /// </summary>
    public decimal Ratio { get; set; }

    /// <summary>
    /// Geçen sürenin oranı (0.0 ile 1.0 arasında); <c>null</c> ise zaman işareti çizilmez.
    /// </summary>
    public decimal? TimeRatio { get; set; }

    /// <summary>
    /// Çizim yüzeyine dairesel halka göstergesini çizer.
    /// </summary>
    /// <param name="canvas">Çizim tuvali.</param>
    /// <param name="dirtyRect">Çizim alanı sınırları.</param>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (canvas == null || dirtyRect.Width <= 0 || dirtyRect.Height <= 0) { return; }

        // İşaret halkanın iki yanına taşar; taşan kısım da çizim alanında kalsın.
        var size = Math.Min(dirtyRect.Width, dirtyRect.Height) - StrokeThickness - TimeMarkerOverhang;
        if (size <= 0) { return; }

        var x = dirtyRect.Center.X - (size / 2f);
        var y = dirtyRect.Center.Y - (size / 2f);

        canvas.StrokeSize = StrokeThickness;
        canvas.StrokeLineCap = LineCap.Round;

        DrawBackgroundTrack(canvas, x, y, size);
        DrawActiveProgress(canvas, x, y, size);
        DrawTimeMarker(canvas, x, y, size);
    }

    private static void DrawBackgroundTrack(ICanvas canvas, float x, float y, float size)
    {
        // İz göstergenin saydam tonudur: tonlu kartta da düz kartta da seçilir (GS24).
        canvas.StrokeColor = ChartColorResolver.ResolveTint("Indicator");
        canvas.DrawEllipse(x, y, size, size);
    }

    private void DrawActiveProgress(ICanvas canvas, float x, float y, float size)
    {
        var clampedRatio = Math.Clamp((float)Ratio, 0f, 1f);
        if (clampedRatio <= 0f) { return; }

        canvas.StrokeColor = ChartColorResolver.ResolveColor("Indicator");

        if (clampedRatio >= 0.999f)
        {
            canvas.DrawEllipse(x, y, size, size);
            return;
        }

        var sweepAngle = clampedRatio * 360f;
        canvas.DrawArc(x, y, size, size, TopAngle, TopAngle - sweepAngle, true, false);
    }

    private void DrawTimeMarker(ICanvas canvas, float x, float y, float size)
    {
        if (TimeRatio is not { } timeRatio) { return; }

        // İşaret doluluk yayıyla aynı açı kuralını kullanır ki iki yönün varsayımı ayrışmasın; yalnız daha
        // kalın ve kısadır, böylece halkanın iki yanına taşıp onu keser.
        var markerCenter = TopAngle - (Math.Clamp((float)timeRatio, 0f, 1f) * 360f);
        canvas.StrokeColor = ChartColorResolver.ResolveColor("TextSecondary");
        canvas.StrokeSize = StrokeThickness + TimeMarkerOverhang;
        canvas.StrokeLineCap = LineCap.Butt;
        canvas.DrawArc(x, y, size, size, markerCenter - TimeMarkerHalfAngle, markerCenter + TimeMarkerHalfAngle, false, false);
    }
}
