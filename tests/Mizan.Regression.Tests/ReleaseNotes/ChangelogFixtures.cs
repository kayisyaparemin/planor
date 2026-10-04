namespace Mizan.Regression.Tests.ReleaseNotes;

/// <summary>Sürüm notu okuyucusunun testlerinde kullanılan örnek CHANGELOG metinleri.</summary>
internal static class ChangelogFixtures
{
    public const string Valid = """
        # Değişiklik Günlüğü

        ## [Yayınlanmamış]

        - Henüz yayınlanmamış bir madde.

        ## [0.2.0] - 2026-11-01

        ### Eklendi

        - İkinci sürümün maddesi.

        ## [0.1.0] - 2026-10-05

        ### Eklendi

        - İlk sürümün birinci maddesi.
        - İlk sürümün ikinci maddesi.

        ### Düzeltildi

        - İlk sürümün düzeltmesi.

        ## [0.0.1] - 2026-09-01

        - Eski sürümün maddesi.
        """;

    /// <summary>Verilen başlık ve gövdeyi, arkasında eski bir sürüm bırakarak tek bir değişiklik günlüğüne çevirir.</summary>
    public static string With(string heading, string body) =>
        $"# Değişiklik Günlüğü\n\n{heading}\n\n{body}\n\n## [0.0.1] - 2026-09-01\n\n- Eski madde.\n";
}
