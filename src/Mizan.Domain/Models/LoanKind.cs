namespace Mizan.Domain.Models;

/// <summary>
/// Erken ödeme ücretini belirleyen kredi türü (6502 sayılı Tüketicinin Korunması Hakkında Kanun).
/// </summary>
public enum LoanKind
{
    /// <summary>İhtiyaç ve taşıt kredisi — md. 27 gereği erken ödeme ücreti alınamaz.</summary>
    Consumer = 0,
    /// <summary>Sabit faizli konut finansmanı — md. 37 gereği kalan vade ≤36 ay için en fazla %1, >36 ay için %2 ücret alınabilir.</summary>
    HousingFixed = 1,
    /// <summary>Değişken faizli konut finansmanı — md. 37 gereği erken ödeme ücreti alınamaz.</summary>
    HousingVariable = 2
}
