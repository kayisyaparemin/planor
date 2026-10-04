using Microsoft.Maui.Graphics;
using MauiApp = Microsoft.Maui.Controls.Application;

namespace Mizan.App.Charts;

/// <summary>
/// Grafik primitiflerinin MAUI çizim anında güncel temaya ait dinamik renk token'larını doğrudan okuyabilmesi
/// ve headless test ortamlarında güvenli fallback renkleri sağlayabilmesi için vardır.
/// </summary>
public static class ChartColorResolver
{
    private static readonly Color DefaultIndicator = Color.FromArgb("#6E97C6");
    private static readonly Color DefaultTextSecondary = Color.FromArgb("#A7B0BC");
    private static readonly Color DefaultNegativeText = Color.FromArgb("#EE9D96");
    private static readonly Color DefaultSurfaceChart = Color.FromArgb("#233040");
    private static readonly Color DefaultBorderSubtle = Color.FromArgb("#343B46");
    private static readonly Color DefaultTextOnAction = Color.FromArgb("#1E232A");

    // Dolgu ve halka izi çizgi renginin bu kadar saydam tonudur (GS24): zemin tonlu da olsa düz de olsa seçilir.
    private const float TintAlpha = 0.2f;

    /// <summary>
    /// Bir token'ın saydam tonunu çözer. Grafik dolgusu ve halka izi ayrı bir zemin token'ı olsaydı, grafiği taşıyan
    /// kartın zemini o token'a eşit olduğunda kaybolurdu; çizgi renginin tonu her zeminde çizginin ailesinde kalır.
    /// </summary>
    /// <param name="tokenName">Tasarım sistemindeki renk token adı.</param>
    /// <returns>Token renginin saydam hâli.</returns>
    public static Color ResolveTint(string tokenName) => ResolveColor(tokenName).WithAlpha(TintAlpha);

    /// <summary>
    /// Belirtilen tasarım sistemi token adını çalışma zamanı kaynaklarından çözer; bulunamazsa fallback renk döner.
    /// </summary>
    /// <param name="tokenName">Tasarım sistemindeki renk token adı.</param>
    /// <returns>Çizim için çözümlenen MAUI Color nesnesi.</returns>
    public static Color ResolveColor(string tokenName)
    {
        if (MauiApp.Current?.Resources is { } resources &&
            resources.TryGetValue(tokenName, out var resourceValue) &&
            resourceValue is Color resolvedColor)
        {
            return resolvedColor;
        }

        return tokenName switch
        {
            "Indicator" => DefaultIndicator,
            "TextSecondary" => DefaultTextSecondary,
            "NegativeText" => DefaultNegativeText,
            "SurfaceChart" => DefaultSurfaceChart,
            "BorderSubtle" => DefaultBorderSubtle,
            "TextOnAction" => DefaultTextOnAction,
            _ => Colors.Transparent
        };
    }
}
