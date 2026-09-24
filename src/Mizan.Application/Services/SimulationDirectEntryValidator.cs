using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Finansal Yapı ekranındaki ortak formdan simülasyon senaryo türlerinin
/// doğrudan eklenip eklenemeyeceğini doğrulayan yardımcı sınıftır.
/// Kendi özel ekranı olan türlerin (düzenli gelir, kart ödeme tercihi vb.) ortak formdan girilmesini engeller.
/// </summary>
public static class SimulationDirectEntryValidator
{
    /// <summary>
    /// Belirtilen simülasyon senaryo türünün Finansal Yapı ekranındaki ortak formdan doğrudan girilip girilemeyeceğini belirtir.
    /// </summary>
    public static bool IsDirectEntry(SimulationScenarioType type) => type switch
    {
        SimulationScenarioType.CashPurchase or
        SimulationScenarioType.CreditCardSinglePayment or
        SimulationScenarioType.CreditCardInstallmentPurchase or
        SimulationScenarioType.RecurringPayment or
        SimulationScenarioType.FinancingLoan or
        SimulationScenarioType.CashDebt or
        SimulationScenarioType.LoanEarlyClosure or
        SimulationScenarioType.LoanPartialPrepayment or
        SimulationScenarioType.FutureIncome => true,
        _ => false
    };

    /// <summary>
    /// Senaryo türünün doğrudan giriş için uygun olup olmadığını doğrular; uygun değilse kural ihlali fırlatır.
    /// </summary>
    public static void Validate(SimulationScenarioType type)
    {
        if (!IsDirectEntry(type))
        {
            throw new InvalidOperationException($"{TypeText(type)} Finansal Yapı'dan bu formla girilemez.");
        }
    }

    private static string TypeText(SimulationScenarioType type) => type switch
    {
        SimulationScenarioType.IncomeChange => "Gelir değişikliği",
        SimulationScenarioType.CreditCardPaymentMode => "Kart ödeme şekli",
        _ => "Koşul"
    };
}
