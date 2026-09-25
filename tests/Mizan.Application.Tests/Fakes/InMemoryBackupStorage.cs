using Mizan.Application.Abstractions;
using Mizan.Application.Models;

namespace Mizan.Application.Tests.Fakes;

/// <summary>
/// Testler için bellek içi yedek depolama test çifti.
/// </summary>
public sealed class InMemoryBackupStorage : IBackupStorage
{
    private readonly Dictionary<string, byte[]> _files = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DateTimeOffset> _timestamps = new(StringComparer.Ordinal);

    public string LocationDescription { get; set; } = "Dahili depolama › Mizan";

    public bool HasAccess { get; set; } = true;

    public Task<bool> RequestAccessAsync() => Task.FromResult(HasAccess);

    public async Task SaveAsync(string fileName, string sourcePath, CancellationToken cancellationToken = default)
    {
        var bytes = await File.ReadAllBytesAsync(sourcePath, cancellationToken);
        _files[fileName] = bytes;
        _timestamps[fileName] = DateTimeOffset.UtcNow;
    }

    public void AddFile(string fileName, byte[] content, DateTimeOffset modifiedAt)
    {
        _files[fileName] = content;
        _timestamps[fileName] = modifiedAt;
    }

    public Task<IReadOnlyList<StoredBackup>> ListAsync(CancellationToken cancellationToken = default)
    {
        var list = _files.Keys
            .Select(name => new StoredBackup(name, _timestamps[name]))
            .ToArray();
        return Task.FromResult<IReadOnlyList<StoredBackup>>(list);
    }

    public Task<Stream> OpenReadAsync(string fileName, CancellationToken cancellationToken = default)
    {
        if (!_files.TryGetValue(fileName, out var content))
        {
            throw new FileNotFoundException($"Dosya bulunamadı: {fileName}");
        }

        return Task.FromResult<Stream>(new MemoryStream(content, writable: false));
    }

    public Task DeleteAsync(string fileName, CancellationToken cancellationToken = default)
    {
        _files.Remove(fileName);
        _timestamps.Remove(fileName);
        return Task.CompletedTask;
    }
}
