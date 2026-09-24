using Mizan.Application.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Kullanıcının simülatörde oluşturduğu varsayımsal plan taslaklarının
/// kalıcı olarak saklanmasını, listelenmesini ve silinmesini sağlayan veri erişim portu.
/// </summary>
public interface ISimulationDraftRepository
{
    /// <summary>
    /// Kayıtlı tüm simülasyon taslaklarını son güncellenme tarihine göre azalan sırada getirir.
    /// </summary>
    Task<IReadOnlyList<SimulationDraft>> GetDraftsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen kimliğe sahip simülasyon taslağını getirir; bulunamazsa null döner.
    /// </summary>
    Task<SimulationDraft?> GetDraftByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Yeni bir simülasyon taslağı ekler veya mevcut olanı günceller.
    /// Koşul listesi baştan yazılarak sıra ve aktiflik durumu korunur.
    /// </summary>
    Task UpsertDraftAsync(
        SimulationDraft draft,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen kimliğe sahip simülasyon taslağını ve bağlı tüm koşullarını siler.
    /// </summary>
    Task DeleteDraftAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
