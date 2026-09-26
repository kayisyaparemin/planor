using Mizan.Presentation.Services;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Birim testlerinde dosya seçme ve kayıtlı yedeği akış olarak açma işlemlerini taklit eden test dublörü.
/// </summary>
public sealed class FakeBackupFilePicker : IBackupFilePicker
{
    public Stream? NextPickedStream { get; set; }
    public Stream? NextStoredStream { get; set; }
    public string? LastOpenedStoredFileName { get; private set; }

    public Task<Stream?> PickAndOpenAsync()
    {
        return Task.FromResult(NextPickedStream);
    }

    public Task<Stream> OpenStoredAsync(string fileName)
    {
        LastOpenedStoredFileName = fileName;
        return Task.FromResult(NextStoredStream ?? new MemoryStream());
    }
}
