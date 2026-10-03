using Mizan.Application.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Simülatörün sonuç kartının okuma portu: ekranın elindeki deneme listesinden şu anki gidişat zincirini ve denemeli
/// zinciri hesaplar. Liste ekrandan gelir, diskten okunmaz: kullanıcı anahtarı çevirdiği anda yazma bitmeden de
/// sonuç yenilenebilsin ve üst üste binen isteklerde ekran son isteği seçebilsin (S73-2). Hiçbir şey yazmaz;
/// <see cref="ISimulationWorkflowService"/> listeyi saklar, bu port yalnız hesaplar.
/// </summary>
public interface ISimulationResultService
{
    /// <summary>
    /// Açık ve tarihi geçmemiş denemeleri hesaba alarak sonucu üretir; geçerlilik burada, bugüne göre yeniden
    /// değerlendirilir (S76-5). Açık dönem yoksa ya da plan zincir kuramıyorsa <c>null</c> döner (S76-1); hesap
    /// kuralı bozan bir denemede (kart bulunamadı, tutar geçersiz) hata fırlatır, ekran bunu hata durumuna çevirir.
    /// </summary>
    /// <param name="conditions">Çalışma listesinin o anki hâli: deneme ve açık/kapalı tercihi.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    Task<SimulationOutcome?> CalculateAsync(
        IReadOnlyList<SimulationDraftCondition> conditions,
        CancellationToken cancellationToken = default);
}
