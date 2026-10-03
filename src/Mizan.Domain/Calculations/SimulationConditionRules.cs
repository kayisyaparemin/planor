using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Simülatördeki bir denemenin tarihi ne zaman geçmiş sayılır sorusunun tek yeri (S76-3, 5). Deneme formu yeni
/// denemeyi, simülasyon servisi listede günlerce bekleyen denemeyi aynı kuralla değerlendirir; iki ayrı kopya
/// olsaydı form kabul ettiği bir denemeyi ertesi gün listede "tarihi geçti" diye işaretleyen kural başka bir gün
/// sayabilirdi. Geçmişte olmuş bir şey deneme değil kayıttır; tarihi geçen deneme hesaba girmez, uygulanmaz.
/// </summary>
public static class SimulationConditionRules
{
    /// <summary>Formda bugünden önceye tarihlenen deneme kaydedilirken gösterilen mesaj.</summary>
    public const string PastDateMessage = "Denemenin tarihi bugünden önce olamaz.";

    /// <summary>Denemenin tarihi bugünden önce mi: bugün geçerli, dün geçmiştir.</summary>
    public static bool IsDatePassed(SimulationRequest request, DateOnly today) =>
        request.StartDate < today;
}
