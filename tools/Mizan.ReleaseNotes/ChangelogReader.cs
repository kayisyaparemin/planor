using System.Globalization;
using System.Text.RegularExpressions;

namespace Mizan.ReleaseNotes;

/// <summary>
/// <c>CHANGELOG.md</c>'den bir sürümün notunu okur (S82). Not iş akışında üretilmediği için "yanlış
/// sürümün notu" ve "boş not" yayına gidemez: okunamayan her şey ihlaldir, sessizce boş döndürülmez.
/// </summary>
public static partial class ChangelogReader
{
    private const string SectionMarker = "## ";
    private const string DateFormat = "yyyy-MM-dd";

    /// <summary>İstenen sürümün bölümünü bulur ve doğrular.</summary>
    /// <param name="changelog">CHANGELOG.md'nin metni.</param>
    /// <param name="version">İstenen sürüm; <c>v</c> öneki kabul edilir (etiketten gelir).</param>
    /// <returns>Sürümün notu.</returns>
    /// <exception cref="ArgumentException">Sürüm <c>X.Y.Z</c> biçiminde değilse.</exception>
    /// <exception cref="InvalidDataException">Bölüm yoksa, birden fazlaysa, başlığı/tarihi okunamıyorsa ya da madde içermiyorsa.</exception>
    public static ReleaseNote Read(string changelog, string version)
    {
        ArgumentNullException.ThrowIfNull(changelog);
        var requested = NormalizeVersion(version);
        var lines = changelog.ReplaceLineEndings("\n").Split('\n');

        var headingIndex = FindSectionHeading(lines, requested);
        var releaseDate = ParseDate(SectionHeadingPattern().Match(lines[headingIndex]), requested);
        var body = ExtractBody(lines, headingIndex);
        if (!ListItemPattern().IsMatch(body))
        {
            throw new InvalidDataException($"CHANGELOG.md'de '{requested}' sürümünün bölümü boş: en az bir '- ' maddesi olmalı.");
        }

        return new ReleaseNote(requested, releaseDate, body);
    }

    private static string NormalizeVersion(string version)
    {
        var match = VersionPattern().Match(version ?? string.Empty);
        return match.Success
            ? match.Groups["version"].Value
            : throw new ArgumentException($"Sürüm 'X.Y.Z' biçiminde olmalı (örn. 0.1.0); verilen: '{version}'.", nameof(version));
    }

    private static int FindSectionHeading(string[] lines, string version)
    {
        var found = new List<int>();
        for (var index = 0; index < lines.Length; index++)
        {
            var match = SectionHeadingPattern().Match(lines[index]);
            if (match.Success && match.Groups["version"].Value == version)
            {
                found.Add(index);
            }
        }

        return found.Count switch
        {
            1 => found[0],
            0 => throw new InvalidDataException($"CHANGELOG.md'de '{version}' sürümünün bölümü yok: '## [{version}] - YYYY-AA-GG' başlığı bulunamadı."),
            _ => throw new InvalidDataException($"CHANGELOG.md'de '{version}' sürümü {found.Count} kez geçiyor; hangi notun yayınlanacağı belirsiz."),
        };
    }

    private static DateOnly ParseDate(Match heading, string version)
    {
        var dateMatch = DatePartPattern().Match(heading.Groups["rest"].Value);
        if (dateMatch.Success
            && DateOnly.TryParseExact(dateMatch.Groups["date"].Value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }

        throw new InvalidDataException($"CHANGELOG.md'de '{version}' başlığının tarihi okunamadı: '## [{version}] - YYYY-AA-GG' biçiminde olmalı.");
    }

    private static string ExtractBody(string[] lines, int headingIndex)
    {
        var body = new List<string>();
        for (var index = headingIndex + 1; index < lines.Length; index++)
        {
            if (lines[index].StartsWith(SectionMarker, StringComparison.Ordinal))
            {
                break;
            }

            body.Add(lines[index]);
        }

        return string.Join('\n', body).Trim();
    }

    [GeneratedRegex(@"^v?(?<version>\d+\.\d+\.\d+)$", RegexOptions.CultureInvariant)]
    private static partial Regex VersionPattern();

    [GeneratedRegex(@"^## \[(?<version>[^\]]*)\](?<rest>.*)$", RegexOptions.CultureInvariant)]
    private static partial Regex SectionHeadingPattern();

    [GeneratedRegex(@"^ - (?<date>\d{4}-\d{2}-\d{2})$", RegexOptions.CultureInvariant)]
    private static partial Regex DatePartPattern();

    [GeneratedRegex(@"^\s*- \S", RegexOptions.CultureInvariant | RegexOptions.Multiline)]
    private static partial Regex ListItemPattern();
}
