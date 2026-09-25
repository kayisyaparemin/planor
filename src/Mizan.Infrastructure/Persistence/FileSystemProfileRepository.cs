using System.Text.Json;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;

namespace Mizan.Infrastructure.Persistence;

/// <summary>
/// Her profilin kendi klasöründe (profiles/{id}/mizan.db3 ve profile.json) tutulduğu,
/// disk tabanlı profil deposu. Meta dosyası kaybolsa bile veritabanı duruyorsa profili kurtarır.
/// </summary>
public sealed class FileSystemProfileRepository(string rootDirectory) : IProfileRepository, IProfileFileLayout
{
    private readonly string _rootDirectory = string.IsNullOrWhiteSpace(rootDirectory)
        ? throw new ArgumentException("Uygulama veri klasörü gereklidir.", nameof(rootDirectory))
        : rootDirectory;

    /// <inheritdoc />
    public string RootDirectory => _rootDirectory;

    /// <inheritdoc />
    public string GetProfileDirectory(Guid profileId) =>
        Path.Combine(
            _rootDirectory,
            DatabaseConstants.ProfilesDirectoryName,
            profileId.ToString("N"));

    /// <inheritdoc />
    public string GetDatabasePath(Guid profileId) =>
        Path.Combine(GetProfileDirectory(profileId), DatabaseConstants.DatabaseFileName);

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserProfile>> GetProfilesAsync(
        CancellationToken cancellationToken = default)
    {
        var profilesDirectory = Path.Combine(
            _rootDirectory, DatabaseConstants.ProfilesDirectoryName);

        if (!Directory.Exists(profilesDirectory))
        {
            return [];
        }

        var profiles = new List<UserProfile>();
        foreach (var directory in Directory.EnumerateDirectories(profilesDirectory))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!Guid.TryParseExact(Path.GetFileName(directory), "N", out var id))
            {
                continue;
            }

            var profile = await ReadMetadataAsync(id, directory, cancellationToken);
            if (profile is not null)
            {
                profiles.Add(profile);
            }
        }

        return profiles;
    }

    /// <inheritdoc />
    public async Task SaveProfileAsync(
        UserProfile profile,
        CancellationToken cancellationToken = default)
    {
        var directory = GetProfileDirectory(profile.Id);
        Directory.CreateDirectory(directory);

        // Önce geçici dosyaya yaz, sonra yerine taşı: yazma yarıda kesilirse
        // eski meta dosyası bozulmadan kalır.
        var path = Path.Combine(directory, DatabaseConstants.MetadataFileName);
        var temporaryPath = path + ".tmp";
        var metadata = new ProfileMetadata(
            profile.Name,
            profile.CreatedAt,
            profile.LastOpenedAt);

        await using (var stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(
                stream,
                metadata,
                ProfileMetadataJsonContext.Default.ProfileMetadata,
                cancellationToken);
        }

        File.Move(temporaryPath, path, overwrite: true);
    }

    /// <inheritdoc />
    public Task DeleteProfileAsync(
        Guid profileId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var directory = GetProfileDirectory(profileId);
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }

        return Task.CompletedTask;
    }

    private async Task<UserProfile?> ReadMetadataAsync(
        Guid id,
        string directory,
        CancellationToken cancellationToken)
    {
        var metadataPath = Path.Combine(directory, DatabaseConstants.MetadataFileName);
        if (File.Exists(metadataPath))
        {
            try
            {
                await using var stream = File.OpenRead(metadataPath);
                var metadata = await JsonSerializer.DeserializeAsync(
                    stream,
                    ProfileMetadataJsonContext.Default.ProfileMetadata,
                    cancellationToken);

                if (metadata is not null &&
                    !string.IsNullOrWhiteSpace(metadata.Name))
                {
                    return new UserProfile
                    {
                        Id = id,
                        Name = metadata.Name,
                        CreatedAt = metadata.CreatedAt,
                        LastOpenedAt = metadata.LastOpenedAt
                    };
                }
            }
            catch (JsonException)
            {
                // Bozuk meta dosyası: veritabanı varsa kurtarılır.
            }
        }

        return RecoverFromDatabase(id, directory);
    }

    private static UserProfile? RecoverFromDatabase(Guid id, string directory)
    {
        if (!File.Exists(Path.Combine(directory, DatabaseConstants.DatabaseFileName)))
        {
            return null;
        }

        return new UserProfile
        {
            Id = id,
            Name = UserProfile.DefaultName,
            CreatedAt = new DateTimeOffset(
                Directory.GetCreationTimeUtc(directory),
                TimeSpan.Zero)
        };
    }
}
