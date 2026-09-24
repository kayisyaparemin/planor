namespace Mizan.Application.Abstractions;

/// <summary>
/// Kullanıcının finansal planında gerçekleşen kasıtlı planlama değişikliklerini (yeni borç, kredi kapatma,
/// gelir güncellemesi vb.) açık nakit akış dönemine plan revizyonu olarak kaydeden yazma portudur (Kural M4).
/// </summary>
public interface IPlanChangeRecorder
{
    /// <summary>
    /// Güncel finansal planı okur, ilk durum snapshot'ının varlığını garanti eder ve açık dönem varsa
    /// belirtilen tetikleyici açıklamasıyla yeni bir plan revizyonu kaydeder (I24).
    /// </summary>
    Task RecordChangeAsync(
        string trigger,
        CancellationToken cancellationToken = default);
}
