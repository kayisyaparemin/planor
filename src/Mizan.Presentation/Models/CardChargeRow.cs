namespace Mizan.Presentation.Models;

/// <summary>
/// Kart formundaki gelecek kart harcamalarından birinin ham satırı: açıklama, karta yansıyacağı
/// tarih ve tutar. Tarih ve tutar metni App'teki çeviricilerle kurulur (EK-V6b, kural: ham veri).
/// </summary>
/// <param name="Id">Harcamanın kimliği; kayıtlı harcamada korunur.</param>
/// <param name="Description">Harcamanın açıklaması; taksitliyse "Telefon (3/12)" biçiminde.</param>
/// <param name="PostingDate">Harcamanın karta yansıyacağı gün.</param>
/// <param name="Amount">Harcamanın (taksidin) tutarı.</param>
public sealed record CardChargeRow(Guid Id, string Description, DateOnly PostingDate, decimal Amount);
