using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Kullanıcının planladığı bir ara ödeme veya erken kapamayı, o gün ödenecek tutarıyla birlikte taşır.
/// Tam kapamanın tutarı saklanmaz; kalan anapara değiştikçe değiştiği için her seferinde
/// kredinin o günkü durumundan yeniden hesaplanır.
/// </summary>
/// <param name="Loan">Erken ödemenin ait olduğu kredi.</param>
/// <param name="Prepayment">Planlanmış erken ödeme taahhüdü.</param>
/// <param name="Amount">
/// O gün ödenecek tutar; kredi o tarihten önce bitiyorsa ya da faiz çözülemiyorsa null.
/// </param>
/// <param name="IsUnquotable">Kredinin faizi çözülemediği için tutarın hesaplanamadığını belirtir.</param>
public sealed record PlannedLoanPrepayment(
    Loan Loan,
    LoanPrepayment Prepayment,
    decimal? Amount,
    bool IsUnquotable);
