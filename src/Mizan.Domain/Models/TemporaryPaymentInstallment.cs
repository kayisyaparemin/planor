namespace Mizan.Domain.Models;

/// <summary>
/// Kredi ve kart dışındaki bir ödeme planına ait tekil takvimli taksit veya ödeme kalemi.
/// Kullanıcının senetli, elden veya periyodik taahhütlerinin vadeli taksit takvimini oluşturmak için vardır.
/// </summary>
public sealed record TemporaryPaymentInstallment
{
    /// <summary>Taksitin benzersiz kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Taksitin ait olduğu ödeme planının kimliği.</summary>
    public Guid PlanId { get; init; }

    /// <summary>Taksitin ödeneceği vade tarihi.</summary>
    public DateOnly DueDate { get; init; }

    /// <summary>Taksitin ödeme tutarı.</summary>
    public decimal Amount { get; init; }

    /// <summary>Taksitin fiilen ödenip ödenmediği.</summary>
    public bool IsPaid { get; init; }
}
