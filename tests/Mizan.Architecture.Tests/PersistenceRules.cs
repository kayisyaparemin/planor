using System.Text.RegularExpressions;

namespace Mizan.Architecture.Tests;

/// <summary>
/// Veritabanına yazma biçimini denetler. SQLite'ın <c>INSERT OR REPLACE</c>'i çakışan satırı silip
/// yeniden ekler; üst/alt bağları <c>ON DELETE CASCADE</c> ile kurulu olduğu için bu silme alt
/// kayıtları da götürür. Bir krediyi, geliri ya da ödeme planını güncellemek erken ödemelerini,
/// tutar geçmişini ya da taksitlerini sessizce siliyordu. Hangi tablonun alt tablosu olduğu şema
/// büyüdükçe değiştiği için kural tabloya bakmaz: bu yazma biçimi hiçbir yerde kullanılmaz.
/// </summary>
internal static class PersistenceRules
{
    private static readonly Regex ReplacingWriteRegex = new(
        @"\bInsertOrReplace(Async)?\s*[(<]|\bOR\s+REPLACE\b|\bREPLACE\s+INTO\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static IReadOnlyList<string> CheckNoInsertOrReplace()
    {
        var files = Directory.GetFiles(SolutionPaths.SourceDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("\\obj\\") && !f.Contains("/obj/") &&
                        !f.Contains("\\bin\\") && !f.Contains("/bin/"));

        return files
            .SelectMany(file => CheckSourceContent(Path.GetFileName(file), File.ReadAllText(file)))
            .ToList();
    }

    public static IReadOnlyList<string> CheckSourceContent(string fileName, string content)
    {
        var lines = content.Split('\n');
        var violations = new List<string>();
        for (var i = 0; i < lines.Length; i++)
        {
            // Yorum satırı yasağın gerekçesini anlatabilir; yalnız çalışan kod denetlenir.
            if (lines[i].TrimStart().StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            if (ReplacingWriteRegex.IsMatch(lines[i]))
            {
                violations.Add($"{fileName} (Satır {i + 1}): INSERT OR REPLACE satırı silip yeniden ekler ve alt kayıtları götürür; SqliteUpsert.Upsert kullanılır.");
            }
        }

        return violations;
    }
}
