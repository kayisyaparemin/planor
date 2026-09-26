using Microsoft.Maui.Graphics;
using Mizan.Presentation.Charts;

namespace Mizan.App.Charts;

/// <summary>
/// "Yön ne, yukarı mı aşağı mı?" sorusunu cevaplar.
/// Eksen ve etiket taşımadan, ham serideki bakiye veya akış seyrini en yalın çizgi geometrisiyle gösterir.
/// </summary>
public sealed class Sparkline : IDrawable
{
    /// <summary>
    /// Çizilecek zaman serisi verisi.
    /// </summary>
    public ChartSeries? Series { get; set; }

    /// <summary>
    /// Çizim yüzeyine grafik geometrisini çizer.
    /// </summary>
    /// <param name="canvas">Çizim tuvali.</param>
    /// <param name="dirtyRect">Çizim alanı sınırları.</param>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (canvas == null || dirtyRect.Width <= 0 || dirtyRect.Height <= 0) { return; }
        if (Series == null || Series.Points.Count == 0) { return; }

        var points = Series.Points;
        var min = (float)points.Min(p => p.Value);
        var max = (float)points.Max(p => p.Value);
        var range = Math.Max(max - min, 0.0001f);

        canvas.StrokeColor = ChartColorResolver.ResolveColor("Indicator");
        canvas.StrokeSize = 2f;
        canvas.StrokeLineCap = LineCap.Round;

        var path = BuildPath(points, dirtyRect, min, range);
        canvas.DrawPath(path);
    }

    private static PathF BuildPath(IReadOnlyList<ChartPoint> points, RectF rect, float min, float range)
    {
        var path = new PathF();
        if (points.Count == 1)
        {
            var y = rect.Center.Y;
            path.MoveTo(rect.Left, y);
            path.LineTo(rect.Right, y);
            return path;
        }

        var stepX = rect.Width / (points.Count - 1);
        for (var i = 0; i < points.Count; i++)
        {
            var x = rect.Left + (i * stepX);
            var normalizedY = ((float)points[i].Value - min) / range;
            var y = rect.Bottom - (normalizedY * rect.Height);

            if (i == 0)
            {
                path.MoveTo(x, y);
            }
            else
            {
                path.LineTo(x, y);
            }
        }

        return path;
    }
}
