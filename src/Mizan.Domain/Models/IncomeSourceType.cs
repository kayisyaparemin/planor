namespace Mizan.Domain.Models;

/// <summary>
/// Projeksiyon gelir kaleminin kaynağını (düzenli akış veya tek seferlik arızi gelir) belirtir.
/// Nakit akış projeksiyonunda gelir kalemlerinin periyodiklik türünü sınıflandırmak için kullanılır.
/// </summary>
public enum IncomeSourceType
{
    /// <summary>Düzenli ve periyodik olarak tekrarlayan gelir akışı (aylık gelir, kira geliri, düzenli hakediş vb.).</summary>
    Recurring = 1,

    /// <summary>Belirli bir tarihte tahsil edilen tek seferlik arızi gelir (ikramiye, prim, iade vb.).</summary>
    AdHoc = 2
}
