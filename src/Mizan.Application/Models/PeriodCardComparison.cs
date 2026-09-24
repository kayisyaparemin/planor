namespace Mizan.Application.Models;

/// <summary>
/// Bir kartın bu dönemdeki ödemesi: dondurulan planın dediği ile kartın bugünkü hâlinin dediği yan yana.
/// Dönem içi plansız kart harcaması planı değiştirmez (I23); kullanıcı farkı burada görür.
/// </summary>
/// <param name="CardId">Kartın kimliği.</param>
/// <param name="Name">Plandaki ödeme satırının adı.</param>
/// <param name="DueDate">Plandaki vade.</param>
/// <param name="Planned">Planlanan ödeme; plan kilitli olduğu için dönem içinde değişmez.</param>
/// <param name="Current">Kartın bugünkü hâline göre bu dönemde vadesi gelen ödeme; kart artık kayıtlı değilse <c>null</c>.</param>
public sealed record PeriodCardComparison(
    Guid CardId,
    string Name,
    DateOnly DueDate,
    decimal Planned,
    decimal? Current);
