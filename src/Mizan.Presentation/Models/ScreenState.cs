namespace Mizan.Presentation.Models;

/// <summary>
/// Sayfaların ve bileşenlerin sunum durumunu (yükleniyor, içerik, boş veri veya hata)
/// MAUI UI katmanından bağımsız olarak tip güvenli temsil etmek için vardır.
/// </summary>
public enum ScreenState
{
    /// <summary>İçerik yükleniyor, iskelet blokları gösterilmelidir.</summary>
    Loading,

    /// <summary>İçerik hazır ve geçerli veri mevcut.</summary>
    Content,

    /// <summary>Kayıt veya veri bulunmuyor, boş durum mesajı ve yönlendirici aksiyon gösterilmelidir.</summary>
    Empty,

    /// <summary>Beklenmeyen bir hata oluştu, hata mesajı ve tekrar deneme aksiyonu gösterilmelidir.</summary>
    Error
}
