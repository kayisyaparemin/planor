namespace Mizan.Application.Models;

/// <summary>
/// Finansal Yapı ekranında yeni bir kayıt eklenirken kullanıcının göreceği üst düzey kategori grupları.
/// </summary>
public enum RecordEntryGroup
{
    /// <summary>Harcama kalemleri (peşin, kart, periyodik).</summary>
    Spending,

    /// <summary>Borçlanma ve kredi işlemleri.</summary>
    Debt,

    /// <summary>Düzenli ve tek seferlik nakit gelirleri.</summary>
    Income,

    /// <summary>Kredi kartı, banka kredisi ve değişken ödeme planı gibi hesap kayıtları.</summary>
    Account
}
