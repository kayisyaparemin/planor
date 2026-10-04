namespace Mizan.ReleaseNotes;

/// <summary>
/// Bir sürümün <c>CHANGELOG.md</c>'deki bölümü. GitHub Release sayfasına olduğu gibi yazılan metin budur;
/// sürümle bağı kaybolmasın diye sürüm ve tarih notun yanında taşınır.
/// </summary>
/// <param name="Version">Sürüm numarası (<c>X.Y.Z</c>, <c>v</c> öneki olmadan).</param>
/// <param name="ReleaseDate">Bölüm başlığındaki yayın tarihi.</param>
/// <param name="Body">Başlığın altındaki metin; alt başlıklar ve maddeler dahil, baştaki/sondaki boşluk kırpılmış.</param>
public sealed record ReleaseNote(string Version, DateOnly ReleaseDate, string Body);
