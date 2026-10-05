namespace Mizan.Presentation.Models;

/// <summary>
/// Ana sayfanın plan / şu an tablosunda bir kartın satırı (S87-1). Dönem içinde karta yapılan plansız harcama
/// dondurulan planı değiştirmez (I23), yalnız kartın bu dönemki ödemesini büyütür; kullanıcı dönem sonunun
/// plandan neden saptığını bu iki rakamın yan yana durmasından görür.
/// </summary>
/// <param name="Name">Plandaki ödeme satırının adı (kartın adı).</param>
/// <param name="Planned">Dondurulan planın bu dönem için öngördüğü ödeme.</param>
/// <param name="Current">Kartın bugünkü hâline göre bu dönemki ödeme; kart artık kayıtlı değilse <c>null</c>.</param>
/// <param name="IsAbovePlan">Şu anki ödeme planı aşıyorsa <c>true</c>: satır olumsuz renkte görünür (S87-3).</param>
public sealed record PeriodCardComparisonRow(string Name, decimal Planned, decimal? Current, bool IsAbovePlan);
