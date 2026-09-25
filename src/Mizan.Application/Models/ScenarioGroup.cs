namespace Mizan.Application.Models;

/// <summary>
/// Simülasyon senaryo seçeneklerinin arayüzde filtrelenmesini ve çip menüleriyle
/// düzenlenmesini sağlayan üst düzey işlevsel gruplama türü.
/// </summary>
public enum ScenarioGroup
{
    /// <summary>Nakit, kart veya periyodik harcama senaryoları.</summary>
    Spending,

    /// <summary>Finansman kredisi, nakit borçlanma veya kredi erken ödeme senaryoları.</summary>
    Debt,

    /// <summary>Tek seferlik arızi gelir veya düzenli gelir değişikliği senaryoları.</summary>
    Income,

    /// <summary>Kredi kartı ekstre ödeme tercihi gibi operasyonel ayar senaryoları.</summary>
    Setting
}
