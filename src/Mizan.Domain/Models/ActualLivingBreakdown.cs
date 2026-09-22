namespace Mizan.Domain.Models;

/// <summary>
/// Dönem içi fiilî serbest yaşam harcamalarının kategori bazlı kırılım kaydı.
/// Kullanıcının yaşam gideri havuzunu hangi ana başlıklarda tükettiğini dondurmak için vardır.
/// </summary>
public sealed record ActualLivingBreakdown
{
    /// <summary>Kırılım kaydının tekil kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Bağlı olduğu dönem gerçekleşmesi kaydının kimliği.</summary>
    public Guid PeriodActualId { get; init; }

    /// <summary>Harcama kategorisi adı (market, sosyal, fatura vb.).</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Bu kategori altında fiilen gerçekleşen harcama tutarı.</summary>
    public decimal Amount { get; init; }
}
