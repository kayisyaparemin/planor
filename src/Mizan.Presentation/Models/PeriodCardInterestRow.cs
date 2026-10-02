namespace Mizan.Presentation.Models;

/// <summary>
/// Dönem ayrıntısındaki kart faizi listesinin bir satırı (EK-V9 S4). Kart faizi ekstre borcuna eklenir, o dönem
/// cepten çıkmaz; bu yüzden dönemin akışında değil kart başına ayrı listede durur (S75-5). 12 Dönem'deki
/// "12 dönemde faiz" toplamının dönem dönem kırılımı buradan okunur.
/// </summary>
/// <param name="CardName">Faizin biriktiği kart.</param>
/// <param name="Amount">Bu dönemin ekstresinde devreden borca işleyen faiz.</param>
public sealed record PeriodCardInterestRow(string CardName, decimal Amount);
