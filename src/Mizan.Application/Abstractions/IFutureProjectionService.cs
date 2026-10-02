using Mizan.Domain.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// "12 Dönem" ekranının okuma portu: açık dönemden sonraki 12 dönemin projeksiyonunu verir.
/// Zincir ana sayfanın dönem sonundan başlar (S74-1); böylece iki ekran aynı dönem için iki ayrı rakam
/// söylemez ve kullanıcının girdiği bakiye gelecek dönemlere de yansır. Zincirin nereden başlayacağı bir
/// iş kuralı olduğu için ViewModel'de değil burada kurulur; hiçbir şey yazmaz.
/// </summary>
public interface IFutureProjectionService
{
    /// <summary>
    /// Açık dönemden sonraki 12 dönemi hesaplar. Açık dönem yoksa ya da plan projeksiyon kuramıyorsa
    /// (gelir ve bakiye yok, I12) <c>null</c> döner; ana sayfa boşken bu ekran bir rakam uydurmaz (S74-2).
    /// </summary>
    Task<FinancialProjectionResult?> GetAsync(CancellationToken cancellationToken = default);
}
