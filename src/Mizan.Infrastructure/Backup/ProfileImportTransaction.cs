using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Infrastructure.Persistence;

namespace Mizan.Infrastructure.Backup;

/// <summary>
/// Yedekten profil eklemeyi hep-ya-hiç yürütür: seçilen bütün veritabanları hazırlık klasöründe
/// doğrulanmadan hiçbiri yerine taşınmaz; taşıma yarıda kalırsa bu çağrının eklediği profiller geri alınır.
/// Profil kaydı yalnız <see cref="IProfileRepository"/> üzerinden yazılır, somut depo kurulmaz (S58).
/// </summary>
internal static class ProfileImportTransaction
{
    /// <summary>
    /// Seçilen profilleri hedef kimlik ve adlarıyla ekler; herhangi biri eklenemezse hiçbiri eklenmez.
    /// </summary>
    public static async Task RunAsync(
        Stream source,
        IReadOnlyList<ProfileImport> imports,
        IProfileRepository profiles,
        IProfileFileLayout layout,
        CancellationToken cancellationToken)
    {
        EnsureValidSelection(imports);
        var staging = BackupWorkDirectory.Create(layout.RootDirectory, ".restore-");
        try
        {
            var staged = await StageAsync(source, imports, staging, cancellationToken);
            await CommitAsync(staged, profiles, layout, cancellationToken);
        }
        finally
        {
            BackupWorkDirectory.TryDelete(staging);
        }
    }

    private static void EnsureValidSelection(IReadOnlyList<ProfileImport> imports)
    {
        if (imports.Count == 0)
        {
            throw new InvalidOperationException("Eklenecek profil seçilmedi.");
        }

        if (imports.Select(import => import.Target.Id).Distinct().Count() != imports.Count)
        {
            throw new InvalidOperationException("Aynı profil iki kez eklenemez.");
        }
    }

    /// <summary>
    /// Telefondaki profillere dokunmadan seçilen her veritabanını hazırlık klasörüne çıkarır ve doğrular.
    /// </summary>
    private static async Task<IReadOnlyList<StagedProfile>> StageAsync(
        Stream source,
        IReadOnlyList<ProfileImport> imports,
        string staging,
        CancellationToken cancellationToken)
    {
        using var zip = BackupManifestReader.Open(source);
        var manifest = await BackupManifestReader.ReadAsync(zip, cancellationToken);
        var staged = new List<StagedProfile>(imports.Count);
        foreach (var import in imports)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var profile = manifest.Profiles.FirstOrDefault(candidate => candidate.Id == import.SourceId) ??
                          throw BackupRestoreErrors.Corrupt("Seçilen profil yedekte yok.");
            string? database = null;
            if (profile.HasData)
            {
                database = Path.Combine(staging, $"{import.Target.Id:N}.db3");
                BackupDatabaseValidator.ExtractAndValidate(zip, profile, database);
            }

            staged.Add(new StagedProfile(import.Target, database));
        }

        return staged;
    }

    private static async Task CommitAsync(
        IReadOnlyList<StagedProfile> staged,
        IProfileRepository profiles,
        IProfileFileLayout layout,
        CancellationToken cancellationToken)
    {
        // Hiçbir şeyin üzerine yazılmaz: hedeflerden biri varsa taşıma hiç başlamaz.
        if (staged.Any(profile => Directory.Exists(layout.GetProfileDirectory(profile.Target.Id))))
        {
            throw new InvalidOperationException("Eklenecek profil telefonda zaten var.");
        }

        var created = new List<string>();
        try
        {
            foreach (var profile in staged)
            {
                created.Add(layout.GetProfileDirectory(profile.Target.Id));

                // Önce veri, sonra kayıt: süreç tam arada ölürse profil verisiyle kurtarılır (S58).
                if (profile.StagedDatabase is not null)
                {
                    var target = layout.GetDatabasePath(profile.Target.Id);
                    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                    File.Move(profile.StagedDatabase, target);
                }

                await profiles.SaveProfileAsync(profile.Target, cancellationToken);
            }
        }
        catch
        {
            created.ForEach(BackupWorkDirectory.TryDelete);
            throw;
        }
    }

    /// <summary>
    /// Doğrulanmış, yerine taşınmayı bekleyen profil: hedef kaydı ve varsa hazırlıktaki veritabanı.
    /// </summary>
    private sealed record StagedProfile(UserProfile Target, string? StagedDatabase);
}
