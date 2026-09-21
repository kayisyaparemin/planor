namespace Mizan.Domain.Models;

/// <summary>
/// Krediye ait erken ödeme olaylarının takvim üzerinde kronolojik oynatılması sonucunda
/// ortaya çıkan güncel ödeme takvimini ve ara kredi durumlarını temsil eden değişmez sonuç modeli.
/// </summary>
/// <param name="Loan">Erken ödeme olaylarının uygulandığı orijinal kredi sözleşmesi.</param>
/// <param name="Payments">Kronolojik olarak sıralanmış güncel taksit ve erken/ara ödeme kalemleri.</param>
/// <param name="StateAfterEvent">Her erken ödeme olayından hemen sonraki güncellenmiş kanonik kredi durumları.</param>
/// <param name="PrincipalBeforeEvent">Erken ödeme olayının uygulandığı andan hemen önceki kalan anapara tutarları.</param>
/// <param name="IgnoredBecauseUnquotable">Kredinin faizi türetilemediği için olayların oynatılamadığını belirtir.</param>
public sealed record LoanReplay(
    Loan Loan,
    IReadOnlyList<LoanScheduledPayment> Payments,
    IReadOnlyDictionary<Guid, Loan> StateAfterEvent,
    IReadOnlyDictionary<Guid, decimal> PrincipalBeforeEvent,
    bool IgnoredBecauseUnquotable)
{
    /// <summary>Takvimdeki tüm ödemelerin toplam tutarı.</summary>
    public decimal Total => Payments.Sum(x => x.Amount);

    /// <summary>Takvimdeki son ödemenin tarihi; takvim boşsa null döner.</summary>
    public DateOnly? LastPaymentDate => Payments.Count == 0 ? null : Payments[^1].Date;
}
