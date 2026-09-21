namespace Mizan.Domain.Models;

/// <summary>
/// Kredinin itfa analizi sonucunu, çözümlenen faiz/anapara modelini ve varsa engel durumunu sarmalar.
/// Simülasyon motorunun ve UI analiz bileşenlerinin kredinin sağlıklı hesaplanabilir olup olmadığını
/// tek noktadan güvenle denetleyebilmesi için var edilmiştir.
/// </summary>
public sealed record LoanAnalysis(
    Loan Loan,
    LoanAmortization? Amortization,
    LoanAnalysisIssue Issue)
{
    /// <summary>Analizin başarıyla sonuçlanıp itfanın çözümlenip çözümlenmediği.</summary>
    public bool IsSuccess => Issue == LoanAnalysisIssue.None && Amortization is not null;
}
