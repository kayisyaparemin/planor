using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Bir kredinin bugünkü hâlini — çözülen faizini, kalan anaparasını ve bugün kapatılırsa
/// ödenecek bedeli — tek pakette taşır. Kullanıcı sözleşmedeki faizi çoğu zaman bilmez;
/// finansal yapı ekranı bu yüzden faizi ve kapatma bedelini buradan gösterir.
/// Engel varsa metin üretmez; sunum katmanı <see cref="LoanAnalysis.Issue"/> üzerinden
/// kendi metnini kurar (S28).
/// </summary>
/// <param name="Loan">Görünümü çıkarılan kredi.</param>
/// <param name="Analysis">Kredinin itfa analizi; faiz çözülemediyse engel durumunu taşır.</param>
/// <param name="Today">Bugün kapatılırsa ödenecek bedelin dökümü; faiz çözülemediyse null.</param>
public sealed record LoanPayoffOverview(
    Loan Loan,
    LoanAnalysis Analysis,
    LoanPayoffQuote? Today);
