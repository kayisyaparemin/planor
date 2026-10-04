namespace Mizan.Presentation.Charts;

/// <summary>
/// Sütunlu rota grafiğinin tek sütunu: dönemin bir diliminin sonundaki bakiye. Rengi ve konumu App katmanı verir;
/// burada yalnız tarih, tutar ve sütunun bugünün neresinde durduğu taşınır.
/// </summary>
/// <param name="Date">Sütunun bakiyesini aldığı gün: dilimin son günü, bugünün diliminde bugün.</param>
/// <param name="Value">O günkü bakiye.</param>
/// <param name="IsAhead">Gün bugünden sonraysa <c>true</c>: tutar tahmindir.</param>
/// <param name="IsToday">Bugünün düştüğü dilimse <c>true</c>.</param>
public sealed record ChartColumn(DateOnly Date, decimal Value, bool IsAhead, bool IsToday);
