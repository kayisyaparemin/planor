using Microsoft.Maui.Graphics;

namespace Mizan.App.Charts;

/// <summary>
/// "Ne kadarı tamamlandı?" sorusunu cevaplar.
/// Dönem içi ödemelerin veya bütçe hedefinin tamamlanma oranını tek bir dairesel halka göstergesiyle sunar.
/// </summary>
public sealed class RingGauge : IDrawable
{
    private const float StrokeThickness = 6f;

    /// <summary>
    /// Tamamlanma oranı (0.0 ile 1.0 arasında).
    /// </summary>
    public decimal Ratio { get; set; }

    /// <summary>
    /// Çizim yüzeyine dairesel halka göstergesini çizer.
    /// </summary>
    /// <param name="canvas">Çizim tuvali.</param>
    /// <param name="dirtyRect">Çizim alanı sınırları.</param>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (canvas == null || dirtyRect.Width <= 0 || dirtyRect.Height <= 0) { return; }

        var size = Math.Min(dirtyRect.Width, dirtyRect.Height) - StrokeThickness;
        if (size <= 0) { return; }

        var x = dirtyRect.Center.X - (size / 2f);
        var y = dirtyRect.Center.Y - (size / 2f);

        canvas.StrokeSize = StrokeThickness;
        canvas.StrokeLineCap = LineCap.Round;

        DrawBackgroundTrack(canvas, x, y, size);
        DrawActiveProgress(canvas, x, y, size);
    }

    private static void DrawBackgroundTrack(ICanvas canvas, float x, float y, float size)
    {
        canvas.StrokeColor = ChartColorResolver.ResolveColor("BorderSubtle");
        canvas.DrawArc(x, y, size, size, 0f, 360f, false, false);
    }

    private void DrawActiveProgress(ICanvas canvas, float x, float y, float size)
    {
        var clampedRatio = Math.Clamp((float)Ratio, 0f, 1f);
        if (clampedRatio <= 0f) { return; }

        var sweepAngle = clampedRatio * 360f;
        canvas.StrokeColor = ChartColorResolver.ResolveColor("Indicator");

        // Tepeden (saat 12 pozisyonu: -90 derece) başlayarak saat yönünde çizer
        canvas.DrawArc(x, y, size, size, -90f, -90f + sweepAngle, false, false);
    }
}
