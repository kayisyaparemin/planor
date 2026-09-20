namespace Mizan.Domain.Models;

/// <summary>
/// Bir kredi kartı ekstresinin sisteme nasıl aktarıldığını belirten kaynak türü.
/// </summary>
public enum CreditCardStatementSource
{
    /// <summary>Kullanıcı tarafından elle girilen ekstre.</summary>
    Manual = 0,
    /// <summary>Banka PDF ekstresinden otomatik ayrıştırılarak aktarılan ekstre.</summary>
    PdfImport = 1
}
