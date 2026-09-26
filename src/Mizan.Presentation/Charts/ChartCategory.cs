namespace Mizan.Presentation.Charts;

/// <summary>
/// Kategorik grafiklerde (örn: StackedBar) tek bir harcama veya ödeme kategorisinin tutar ve etiket bilgisini temsil etmek için vardır.
/// </summary>
/// <param name="Key">Kategorinin rol veya kimlik anahtarı.</param>
/// <param name="Label">Kullanıcıya gösterilen kategori adı veya etiketi.</param>
/// <param name="Value">Kategorinin sayısal tutar değeri.</param>
public sealed record ChartCategory(string Key, string Label, decimal Value);
