using Mizan.Application.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Onaylanan simülasyon senaryosundan türetilen tüm varlıkları tek bir atomik işlemde
/// (veritabanı transaction'ı) kalıcı depoya yazan altyapı sözleşmesi.
/// </summary>
public interface ISimulationBatchWriter
{
    /// <summary>
    /// Simülasyon varlık paketini tek bir atomik işlemde kalıcı depoya yazar.
    /// </summary>
    Task WriteBatchAsync(
        SimulationPersistenceBatch batch,
        CancellationToken cancellationToken = default);
}
