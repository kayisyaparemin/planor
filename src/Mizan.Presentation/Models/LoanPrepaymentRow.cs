using Mizan.Domain.Models;

namespace Mizan.Presentation.Models;

/// <summary>
/// Kredi formundaki planlı erken ödemelerden birinin ham satırı: şekli, tarihi, ara ödemede
/// anaparadan düşecek tutar ve o gün ödenecek tutar. Şekil, tarih ve tutar metni App'teki
/// çeviricilerle kurulur (EK-V6c, kural: ham veri).
/// </summary>
/// <param name="Id">Erken ödemenin kimliği; kayıtlı erken ödemede korunur.</param>
/// <param name="Mode">Tamamen kapatma ya da ara ödemenin vadeye / taksite yansıma biçimi.</param>
/// <param name="Date">Erken ödemenin yapılacağı gün.</param>
/// <param name="PrincipalAmount">Ara ödemede anaparadan düşecek tutar; tamamen kapatmada null.</param>
/// <param name="Amount">O gün ödenecek tutar; hesaplanamıyorsa null.</param>
public sealed record LoanPrepaymentRow(
    Guid Id, LoanPrepaymentMode Mode, DateOnly Date, decimal? PrincipalAmount, decimal? Amount);
