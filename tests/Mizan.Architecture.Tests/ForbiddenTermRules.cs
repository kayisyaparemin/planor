using System.Text.RegularExpressions;

namespace Mizan.Architecture.Tests;

/// <summary>
/// Sözlükteki yasaklı terimlerin (docs/SOZLUK.md) kaynak kodlara sızmasını engelleyen kural sınıfı (K9).
/// PascalCase token ayrıştırması ve düz metin denetimi yaparak yanlış pozitif üretmeden terimleri denetler.
/// </summary>
internal static partial class ForbiddenTermRules
{
    private static readonly Regex IdentifierRegex = new(@"\b[a-zA-Z_][a-zA-Z0-9_]*\b", RegexOptions.Compiled);
    private static readonly Regex TokenSplitRegex = new(@"([A-Z]+(?=[A-Z][a-z0-9]|\b)|[A-Z][a-z0-9]*|[a-z0-9]+)", RegexOptions.Compiled);
    private static readonly Regex MaasRegex = new(@"\bmaaş\b|\bmaas\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static IReadOnlyList<string> CheckForbiddenTerms()
    {
        var violations = new List<string>();
        var dictionaryPath = Path.Combine(SolutionPaths.Root, "docs", "SOZLUK.md");
        if (!File.Exists(dictionaryPath))
        {
            violations.Add("docs/SOZLUK.md dosyası bulunamadı.");
            return violations;
        }

        var tableRules = ParseTable(File.ReadAllText(dictionaryPath), violations);
        if (violations.Count > 0)
        {
            return violations;
        }

        var files = GetFilesToScan();
        foreach (var file in files)
        {
            ScanFile(file, tableRules, violations);
        }

        return violations;
    }

    public static bool IsIdentifierAllowed(string identifier, string forbiddenTerm)
    {
        var tokens = Tokenize(identifier);
        return !MatchesForbiddenTerm(tokens, forbiddenTerm);
    }

    private static void ScanFile(string file, IReadOnlyList<TermRule> rules, List<string> violations)
    {
        var relativePath = Path.GetRelativePath(SolutionPaths.Root, file).Replace('\\', '/');
        var isSrc = relativePath.StartsWith("src/", StringComparison.OrdinalIgnoreCase);
        var lines = File.ReadAllLines(file);

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            CheckLine(relativePath, isSrc, i + 1, line, rules, violations);
        }
    }

    private static void CheckLine(string path, bool isSrc, int lineNo, string line, IReadOnlyList<TermRule> rules, List<string> violations)
    {
        if (MaasRegex.IsMatch(line) && !IsExemptPath(path, "maaş"))
        {
            violations.Add($"{path} ({lineNo}): Yasaklı 'maaş' terimi kaynakta geçemez.");
            return;
        }

        var matches = IdentifierRegex.Matches(line);
        foreach (Match match in matches)
        {
            var tokens = Tokenize(match.Value);
            foreach (var rule in rules)
            {
                if (!rule.AppliesTo(isSrc) || IsExemptPath(path, rule.Term))
                {
                    continue;
                }

                if (MatchesForbiddenTerm(tokens, rule.Term))
                {
                    violations.Add($"{path} ({lineNo}): Yasaklı '{rule.Term}' tanımlayıcısı bulundu: '{match.Value}'.");
                }
            }
        }
    }

    private static bool MatchesForbiddenTerm(IReadOnlyList<string> tokens, string forbiddenTerm)
    {
        if (forbiddenTerm.Equals("Savings", StringComparison.OrdinalIgnoreCase))
        {
            for (var i = 0; i < tokens.Count; i++)
            {
                if (!tokens[i].Equals("Savings", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var hasGoalOrTarget = (i + 1 < tokens.Count && (tokens[i + 1].Equals("Goal", StringComparison.OrdinalIgnoreCase) || tokens[i + 1].Equals("Target", StringComparison.OrdinalIgnoreCase)))
                                   || (i > 0 && (tokens[i - 1].Equals("Goal", StringComparison.OrdinalIgnoreCase) || tokens[i - 1].Equals("Target", StringComparison.OrdinalIgnoreCase)));
                if (!hasGoalOrTarget)
                {
                    return true;
                }
            }
            return false;
        }

        var forbiddenParts = Tokenize(forbiddenTerm);
        if (forbiddenParts.Count == 1)
        {
            return tokens.Any(t => t.Equals(forbiddenTerm, StringComparison.OrdinalIgnoreCase));
        }

        for (var i = 0; i <= tokens.Count - forbiddenParts.Count; i++)
        {
            var allMatch = true;
            for (var j = 0; j < forbiddenParts.Count; j++)
            {
                if (!tokens[i + j].Equals(forbiddenParts[j], StringComparison.OrdinalIgnoreCase))
                {
                    allMatch = false;
                    break;
                }
            }
            if (allMatch)
            {
                return true;
            }
        }

        return false;
    }

    public static IReadOnlyList<string> Tokenize(string identifier)
    {
        var matches = TokenSplitRegex.Matches(identifier);
        var tokens = new List<string>(matches.Count);
        foreach (Match match in matches)
        {
            tokens.Add(match.Value);
        }
        return tokens;
    }

    private static bool IsExemptPath(string path, string term)
    {
        if (path.Contains("LegacyImport", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        if (path.Contains("Mizan.Architecture.Tests", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        if (path.Contains("RoutesTests.cs", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        if (term.Equals("IncomeDay", StringComparison.OrdinalIgnoreCase) && path.Contains("Persistence", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        if (term.Equals("Salary", StringComparison.OrdinalIgnoreCase) && path.Contains("DatabaseConstraintTests.cs", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        return false;
    }

    private static IEnumerable<string> GetFilesToScan()
    {
        var dirs = new[] { SolutionPaths.SourceDirectory, SolutionPaths.TestsDirectory };
        return dirs.Where(Directory.Exists)
                   .SelectMany(d => Directory.GetFiles(d, "*.*", SearchOption.AllDirectories))
                   .Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
                   .Where(f => !f.Contains("\\obj\\", StringComparison.OrdinalIgnoreCase) && !f.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
                            && !f.Contains("\\bin\\", StringComparison.OrdinalIgnoreCase) && !f.Contains("/bin/", StringComparison.OrdinalIgnoreCase));
    }

    private static List<TermRule> ParseTable(string docContent, List<string> violations)
    {
        const string startMarker = "<!-- YASAKLI-TERIMLER:BASLANGIC -->";
        const string endMarker = "<!-- YASAKLI-TERIMLER:BITIS -->";
        var startIndex = docContent.IndexOf(startMarker, StringComparison.Ordinal);
        var endIndex = docContent.IndexOf(endMarker, StringComparison.Ordinal);

        if (startIndex < 0 || endIndex < 0 || endIndex <= startIndex)
        {
            violations.Add("SOZLUK.md tablosundaki YASAKLI-TERIMLER sınır etiketleri bulunamadı.");
            return [];
        }

        var tableChunk = docContent.Substring(startIndex, endIndex - startIndex);
        var lines = tableChunk.Split('\n');
        var rules = new List<TermRule>();

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (!line.StartsWith('|') || line.StartsWith("| Yasaklı", StringComparison.Ordinal) || line.StartsWith("|---", StringComparison.Ordinal))
            {
                continue;
            }

            var cols = line.Split('|').Select(c => c.Trim()).Where(c => !string.IsNullOrEmpty(c)).ToList();
            if (cols.Count < 3)
            {
                continue;
            }

            var term = cols[0].Trim('`');
            var scope = cols[2];
            if (term.Contains('/'))
            {
                foreach (var sub in term.Split('/'))
                {
                    rules.Add(new TermRule(sub.Trim(' ', '`'), scope));
                }
            }
            else
            {
                rules.Add(new TermRule(term, scope));
            }
        }

        return rules;
    }

    private sealed record TermRule(string Term, string Scope)
    {
        public bool AppliesTo(bool isSrc) => Scope.Contains("src+tests", StringComparison.OrdinalIgnoreCase)
            || (isSrc && Scope.Contains("src", StringComparison.OrdinalIgnoreCase));
    }
}
