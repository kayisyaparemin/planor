using System.Globalization;
using System.Text.RegularExpressions;

namespace Mizan.Architecture.Tests.Design.Support;

/// <summary>
/// docs/EKRAN-KARTLARI.md belgesini ayrıştırarak GK9 kuralının gerektirdiği ekran
/// kartı envanterini, zorunlu bölümleri ve beyan edilen görsel bütçeleri sunar.
/// </summary>
internal static class ScreenCardDocument
{
    private static readonly Lazy<string> ContentLazy = new(() =>
        File.ReadAllText(Path.Combine(SolutionPaths.Root, "docs", "EKRAN-KARTLARI.md")));

    public static string Content => ContentLazy.Value;

    private static readonly string[] RequiredSections =
    [
        "Sorular",
        "Kesme kararları",
        "Bütçe",
        "Blok şeması",
        "Üç durum",
        "Konsept ilişkisi"
    ];

    private static readonly HashSet<string> HeadlessExceptions = new(StringComparer.OrdinalIgnoreCase)
    {
        "EK-V0", // Kabuk ve altyapı: AppShell bir sayfa değildir (GK9 istisnası).
        "EK-V2"  // Hatırlatıcı kartı: sayfasız çocuk ViewModel, ReminderCard bileşenidir.
    };

    /// <summary>
    /// Belgede tanımlı tüm kart anahtarlarını (örn. EK-V0, EK-V1, EK-V3 ...) döndürür.
    /// </summary>
    public static IReadOnlyList<ScreenCardInfo> GetAllCards()
    {
        var cards = new List<ScreenCardInfo>();
        var pattern = new Regex(@"^#{2,3}\s+(?<key>EK-V\d+)\s*—\s*(?<title>[^\r\n]+)", RegexOptions.Multiline);
        var matches = pattern.Matches(Content);

        for (int i = 0; i < matches.Count; i++)
        {
            var match = matches[i];
            var key = match.Groups["key"].Value;
            var title = match.Groups["title"].Value.Trim();
            var startIndex = match.Index;
            var endIndex = (i + 1 < matches.Count) ? matches[i + 1].Index : Content.Length;

            var sectionDivider = Content.IndexOf("\n---\n", startIndex, StringComparison.Ordinal);
            if (sectionDivider > startIndex && sectionDivider < endIndex)
            {
                endIndex = sectionDivider;
            }

            var cardBody = Content.Substring(startIndex, endIndex - startIndex);
            var isHeadless = HeadlessExceptions.Contains(key);
            var isFilled = RequiredSections.All(s => cardBody.Contains(s, StringComparison.OrdinalIgnoreCase));

            cards.Add(new ScreenCardInfo(key, title, isHeadless, isFilled, cardBody));
        }

        return cards;
    }

    /// <summary>
    /// Bir kartın Bütçe bölümünde beyan edilen sayısal limitleri ayrıştırır.
    /// </summary>
    public static DeclaredBudget? ParseDeclaredBudget(string cardBody)
    {
        var labelMatch = Regex.Match(cardBody, @"Label\s+(?<val>\d+)\s*/\s*28");
        var heroMatch = Regex.Match(cardBody, @"Hero rakam\s+(?<val>\d+)\s*/\s*1");
        var heroSurfaceMatch = Regex.Match(cardBody, @"Hero yüzey\s+(?<val>\d+)\s*/\s*1");
        var cardMatch = Regex.Match(cardBody, @"Kart\s+(?<val>\d+)\s*/\s*4");
        var chartMatch = Regex.Match(cardBody, @"Grafik\s+(?<val>\d+)\s*/\s*1");
        var heroPageMatch = Regex.Match(cardBody, @"Hero sayfa\s+(?<val>\d+)\s*/\s*2");
        var navMatch = Regex.Match(cardBody, @"NavRow\s+(?<val>\d+)\s*/\s*5");
        var sentenceMatch = Regex.Match(cardBody, @"Cumle_\s+(?<val>\d+)\s*/\s*3");

        if (!labelMatch.Success)
        {
            return null;
        }

        return new DeclaredBudget(
            int.Parse(labelMatch.Groups["val"].Value, CultureInfo.InvariantCulture),
            heroMatch.Success ? int.Parse(heroMatch.Groups["val"].Value, CultureInfo.InvariantCulture) : 0,
            heroSurfaceMatch.Success ? int.Parse(heroSurfaceMatch.Groups["val"].Value, CultureInfo.InvariantCulture) : 0,
            cardMatch.Success ? int.Parse(cardMatch.Groups["val"].Value, CultureInfo.InvariantCulture) : 0,
            chartMatch.Success ? int.Parse(chartMatch.Groups["val"].Value, CultureInfo.InvariantCulture) : 0,
            navMatch.Success ? int.Parse(navMatch.Groups["val"].Value, CultureInfo.InvariantCulture) : 0,
            sentenceMatch.Success ? int.Parse(sentenceMatch.Groups["val"].Value, CultureInfo.InvariantCulture) : 0,
            heroPageMatch.Success ? int.Parse(heroPageMatch.Groups["val"].Value, CultureInfo.InvariantCulture) : 0);
    }
}

/// <summary>
/// Ekran kartı belgesindeki bir kartın temel meta verileri.
/// </summary>
internal sealed record ScreenCardInfo(
    string Key,
    string Title,
    bool IsHeadlessException,
    bool IsFilled,
    string Body);

/// <summary>
/// Ekran kartında beyan edilen görsel bütçe değerleri.
/// </summary>
internal sealed record DeclaredBudget(
    int LabelCount,
    int HeroCount,
    int HeroSurfaceCount,
    int CardCount,
    int ChartCount,
    int NavRowCount,
    int SentenceCount,
    int HeroPageCount);
