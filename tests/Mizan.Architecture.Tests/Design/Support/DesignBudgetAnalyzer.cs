using System.Text.RegularExpressions;

namespace Mizan.Architecture.Tests.Design.Support;

/// <summary>
/// GK4 ve GK5 kuralları doğrultusunda XAML sayfalarının görsel ve cümle
/// bütçesini analiz eden saf mimari denetçi.
/// </summary>
internal static class DesignBudgetAnalyzer
{
    public const int MaxHeroFigures = 1;
    public const int MaxHeroSurfaces = 1;
    public const int MaxCards = 4;
    public const int MaxCharts = 1;
    public const int MaxHeroPagers = 1;
    public const int MaxHeroPages = 2;
    public const int MaxChartsPerHeroPage = 1;
    public const int MaxLabels = 28;
    public const int MaxNavRows = 5;
    public const int MaxSentences = 3;

    private const string ChartPattern = @"<(?:[A-Za-z0-9_]+:)?(?:ChartCard|Sparkline|AreaTrend|StackedBar|RingGauge)\b|<GraphicsView\b";
    private const string HeroPagerPattern = @"<(?:[A-Za-z0-9_]+:)?HeroPager\b.*?</(?:[A-Za-z0-9_]+:)?HeroPager>";
    private const string HeroPagePattern = @"<(?:[A-Za-z0-9_]+:)?HeroPage\b[^>]*?(?:/>|>.*?</(?:[A-Za-z0-9_]+:)?HeroPage>)";

    public const int MaxEtiketLength = 24;
    public const int MaxCumleLength = 90;
    public const int MaxAksiyonLength = 28;
    public const int MaxDurumLength = 90;

    /// <summary>
    /// Verilen XAML içeriğindeki görsel bütçe öğelerini sayar ve sınır aşımlarını raporlar.
    /// </summary>
    public static VisualBudgetAnalysis AnalyzeXaml(string xamlContent)
    {
        var heroFigureCount = CountMatches(xamlContent, @"TypeHero|Style=""\{StaticResource HeroFigure\}""");
        var heroSurfaceCount = CountMatches(xamlContent, @"\{DynamicResource SurfaceHero\}|<(?:[A-Za-z0-9_]+:)?(?:HeroInputCard|InfoBanner)\b");
        var cardCount = CountMatches(xamlContent, @"<(?:[A-Za-z0-9_]+:)?(?:SummaryCard|ListCard|ChartCard|HeroInputCard|HeroPager|EntryTypeTiles)\b");
        var hero = MeasureHeroPagers(xamlContent);
        var chartCount = hero.VisibleCharts;
        var navRowCount = CountMatches(xamlContent, @"<(?:[A-Za-z0-9_]+:)?NavRow\b");
        var labelCount = CountMatches(xamlContent, @"<Label\b");
        var sentences = ExtractSentenceKeys(xamlContent);

        var violations = new List<string>();
        if (heroFigureCount > MaxHeroFigures) { violations.Add($"Hero rakam sınırı aşıldı: {heroFigureCount} > {MaxHeroFigures}"); }
        if (heroSurfaceCount > MaxHeroSurfaces) { violations.Add($"Hero yüzey sınırı aşıldı: {heroSurfaceCount} > {MaxHeroSurfaces}"); }
        if (cardCount > MaxCards) { violations.Add($"Kart sayısı sınırı aşıldı: {cardCount} > {MaxCards}"); }
        if (chartCount > MaxCharts) { violations.Add($"Grafik sayısı sınırı aşıldı: {chartCount} > {MaxCharts}"); }
        if (hero.Pagers > MaxHeroPagers) { violations.Add($"Kaydırılan hero sınırı aşıldı: {hero.Pagers} > {MaxHeroPagers}"); }
        if (hero.Pages > MaxHeroPages) { violations.Add($"Hero sayfa sınırı aşıldı: {hero.Pages} > {MaxHeroPages}"); }
        if (hero.MaxChartsOnOnePage > MaxChartsPerHeroPage) { violations.Add($"Hero sayfa başına grafik sınırı aşıldı: {hero.MaxChartsOnOnePage} > {MaxChartsPerHeroPage}"); }
        if (navRowCount > MaxNavRows) { violations.Add($"NavRow sayısı sınırı aşıldı: {navRowCount} > {MaxNavRows}"); }
        if (labelCount > MaxLabels) { violations.Add($"<Label> sayısı sınırı aşıldı: {labelCount} > {MaxLabels}"); }
        if (sentences.Count > MaxSentences) { violations.Add($"Cümle sayısı sınırı aşıldı: {sentences.Count} > {MaxSentences}"); }

        return new VisualBudgetAnalysis(
            heroFigureCount, heroSurfaceCount, cardCount, chartCount, navRowCount, labelCount, sentences.Count, hero.Pagers, hero.Pages, violations);
    }

    /// <summary>
    /// Gözün aynı anda gördüğü grafiği bulur (GS22): <c>HeroPager</c> dışındaki her grafik
    /// görünür, pager içinden ise yalnız en kalabalık sayfa görünür.
    /// </summary>
    private static HeroPagerMeasure MeasureHeroPagers(string xaml)
    {
        var pagers = Regex.Matches(xaml, HeroPagerPattern, RegexOptions.Singleline);
        var visible = CountMatches(Regex.Replace(xaml, HeroPagerPattern, string.Empty, RegexOptions.Singleline), ChartPattern);
        var pageCount = 0;
        var mostChartsOnOnePage = 0;

        foreach (Match pager in pagers)
        {
            var pages = Regex.Matches(pager.Value, HeroPagePattern, RegexOptions.Singleline);
            var pageCharts = pages.Select(page => CountMatches(page.Value, ChartPattern)).ToList();
            var pagerOnly = Regex.Replace(pager.Value, HeroPagePattern, string.Empty, RegexOptions.Singleline);
            var tallest = pageCharts.Count == 0 ? 0 : pageCharts.Max();

            pageCount += pages.Count;
            mostChartsOnOnePage = Math.Max(mostChartsOnOnePage, tallest);
            visible += CountMatches(pagerOnly, ChartPattern) + tallest;
        }

        return new HeroPagerMeasure(pagers.Count, pageCount, visible, mostChartsOnOnePage);
    }

    private sealed record HeroPagerMeasure(int Pagers, int Pages, int VisibleCharts, int MaxChartsOnOnePage);

    /// <summary>
    /// Metin anahtarlarının ve değerlerinin GK5 karakter sınırlarına uygunluğunu denetler.
    /// </summary>
    public static IReadOnlyList<string> ValidateStringEntry(string key, string value)
    {
        var violations = new List<string>();
        if (key.StartsWith("Etiket_", StringComparison.Ordinal) && value.Length > MaxEtiketLength)
        {
            violations.Add($"'{key}' etiket sınırı aşıldı: {value.Length} > {MaxEtiketLength}");
        }
        else if (key.StartsWith("Cumle_", StringComparison.Ordinal) && value.Length > MaxCumleLength)
        {
            violations.Add($"'{key}' cümle sınırı aşıldı: {value.Length} > {MaxCumleLength}");
        }
        else if (key.StartsWith("Aksiyon_", StringComparison.Ordinal) && value.Length > MaxAksiyonLength)
        {
            violations.Add($"'{key}' aksiyon sınırı aşıldı: {value.Length} > {MaxAksiyonLength}");
        }
        else if ((key.StartsWith("Bos_", StringComparison.Ordinal) || key.StartsWith("Hata_", StringComparison.Ordinal)) && value.Length > MaxDurumLength)
        {
            violations.Add($"'{key}' durum metni sınırı aşıldı: {value.Length} > {MaxDurumLength}");
        }
        return violations;
    }

    private static int CountMatches(string text, string pattern) =>
        Regex.Matches(text, pattern, RegexOptions.Multiline).Count;

    private static HashSet<string> ExtractSentenceKeys(string text)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        var matches = Regex.Matches(text, @"\bCumle_[A-Za-z0-9_]+\b");
        foreach (Match m in matches)
        {
            keys.Add(m.Value);
        }
        return keys;
    }
}

/// <summary>
/// Bir XAML sayfasının görsel ve cümle bütçesi analiz sonucu. <c>Charts</c> gözün aynı anda
/// gördüğü grafik sayısıdır (GS22): kaydırılan hero'nun gizli sayfası sayılmaz.
/// </summary>
internal sealed record VisualBudgetAnalysis(
    int HeroFigures,
    int HeroSurfaces,
    int Cards,
    int Charts,
    int NavRows,
    int Labels,
    int Sentences,
    int HeroPagers,
    int HeroPages,
    IReadOnlyList<string> Violations)
{
    public bool IsValid => Violations.Count == 0;
}
