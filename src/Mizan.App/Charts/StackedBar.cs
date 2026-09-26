using Microsoft.Maui.Graphics;
using Mizan.Presentation.Charts;

namespace Mizan.App.Charts;

/// <summary>
/// "Bu dönem neyden oluşuyor?" sorusunu cevaplar.
/// Dönem içi zorunlu borç, kart, harcama veya taksit dağılımını en fazla dört kategoride oranlayarak gösterir.
/// </summary>
public sealed class StackedBar : IDrawable
{
    private static readonly string[] SegmentColorTokens =
    [
        "Indicator",
        "TextSecondary",
        "SurfaceChart",
        "BorderSubtle"
    ];

    /// <summary>
    /// Dağılımı gösterilecek en fazla 4 harcama/yükümlülük kategorisi.
    /// </summary>
    public IReadOnlyList<ChartCategory>? Categories { get; set; }

    /// <summary>
    /// Çizim yüzeyine yığılmış çubuk grafiğini çizer.
    /// </summary>
    /// <param name="canvas">Çizim tuvali.</param>
    /// <param name="dirtyRect">Çizim alanı sınırları.</param>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (canvas == null || dirtyRect.Width <= 0 || dirtyRect.Height <= 0) { return; }
        if (Categories == null || Categories.Count == 0) { return; }

        var validCategories = Categories.Take(4).Where(c => c.Value > 0).ToList();
        var total = validCategories.Sum(c => (float)c.Value);
        if (total <= 0) { return; }

        DrawSegments(canvas, dirtyRect, validCategories, total);
    }

    private static void DrawSegments(ICanvas canvas, RectF rect, IReadOnlyList<ChartCategory> list, float total)
    {
        var currentX = rect.Left;
        var barHeight = Math.Min(rect.Height, 24f);
        var barY = rect.Top + ((rect.Height - barHeight) / 2f);

        for (var i = 0; i < list.Count; i++)
        {
            var segmentRatio = (float)list[i].Value / total;
            var segmentWidth = segmentRatio * rect.Width;
            if (segmentWidth <= 0) { continue; }

            var token = SegmentColorTokens[i % SegmentColorTokens.Length];
            canvas.FillColor = ChartColorResolver.ResolveColor(token);

            // İlk ve son segment için köşe yuvarlatma, tek segmentte tam yuvarlatma
            canvas.FillRoundedRectangle(currentX, barY, segmentWidth, barHeight, 4f);
            currentX += segmentWidth;
        }
    }
}
