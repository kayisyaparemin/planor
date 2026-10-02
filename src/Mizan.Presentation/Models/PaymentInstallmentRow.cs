namespace Mizan.Presentation.Models;

/// <summary>
/// Ödeme planı formundaki tekil taksit satırı (EK-V6e, S65-3).
/// </summary>
/// <param name="Id">Taksitin benzersiz kimliği.</param>
/// <param name="DueDate">Taksitin vade tarihi.</param>
/// <param name="Amount">Taksit tutarı.</param>
/// <param name="IsPaid">Taksitin ödenip ödenmediği.</param>
public sealed record PaymentInstallmentRow(Guid Id, DateOnly DueDate, decimal Amount, bool IsPaid);
