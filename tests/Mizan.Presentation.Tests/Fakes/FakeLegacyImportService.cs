using Mizan.Application.Abstractions;
using Mizan.Application.Models;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Birim testlerinde eski yedekten içe aktarma servisini taklit eden test dublörü.
/// </summary>
public sealed class FakeLegacyImportService : ILegacyImportService
{
    public BackupImportResult NextResult { get; set; } = new(new BackupSummary(DateTimeOffset.UtcNow, []), []);
    public Exception? NextException { get; set; }
    public bool ImportCalled { get; private set; }

    public Task<BackupImportResult> ImportAsync(Stream source, CancellationToken cancellationToken = default)
    {
        ImportCalled = true;
        return NextException is null
            ? Task.FromResult(NextResult)
            : Task.FromException<BackupImportResult>(NextException);
    }
}
