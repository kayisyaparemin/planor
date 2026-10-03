namespace Mizan.Application.Models;

/// <summary>
/// Kayıt türü seçicinin grupları; Finansal Yapı listesinin dört grubuyla aynı adı ve sırayı taşır
/// (Gelirler, Kartlar, Krediler, Ödemeler): eklenen kayıt listede aynı adlı grupta görünür. Eski
/// "Borç / Kredi" ile "Hesap" ayrımı kullanıcıya bir şey söylemiyordu (V6f Kapı C, S77).
/// </summary>
public enum RecordEntryGroup
{
    /// <summary>Düzenli ve tek seferlik gelirler.</summary>
    Income,

    /// <summary>Kredi kartları.</summary>
    Card,

    /// <summary>Bankadaki krediler.</summary>
    Loan,

    /// <summary>Tek seferlik, düzenli ve taksitli ödemeler.</summary>
    Payment
}
