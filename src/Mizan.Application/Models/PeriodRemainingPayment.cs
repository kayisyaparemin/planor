namespace Mizan.Application.Models;

/// <summary>
/// Açık dönemde henüz yapılmamış bir ödeme, ana sayfanın "Kalan ödemeler" kartında görüneceği tutarla. Plan satırı
/// kartın dönem başındaki tahminini taşır ve dönem içi harcamaları bilmez (I23); kalanı plan satırından okuyan ekran
/// "Şu an"dan farklı rakam söylüyordu. Planlanan tutar burada bilerek yoktur: kalanı gösteren ekran ona uzanamasın (I165).
/// </summary>
/// <param name="LineId">Kalan plan satırının kimliği.</param>
/// <param name="DueKey">
/// Hatırlatıcı cevabının bağlandığı ödeme anahtarı: listeden "Ödedim" denen ödeme hatırlatıcının cevabıyla aynı kaydı
/// yazar (S88). Satır kimliği değil, kaynak + ad + vade: plan revizyonu satıra yeni kimlik verse de cevap kaybolmaz (I22, S33).
/// </param>
/// <param name="Name">Plandaki ödeme satırının adı.</param>
/// <param name="DueDate">Plandaki vade.</param>
/// <param name="Amount">Ödenecek tutar: kart satırında kartın bugünkü hâli, diğerlerinde planlanan (<c>ProjectedPaymentAmount</c>).</param>
public sealed record PeriodRemainingPayment(
    Guid LineId,
    string DueKey,
    string Name,
    DateOnly DueDate,
    decimal Amount);
