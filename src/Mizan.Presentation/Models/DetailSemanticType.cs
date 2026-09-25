namespace Mizan.Presentation.Models;

/// <summary>
/// Dönem ayrıntısı ekranındaki metriklerin ve tutarların finansal anlamını ve renk/vurgu temasını belirler.
/// </summary>
public enum DetailSemanticType
{
    /// <summary>Gelir akışları (pozitif katkı).</summary>
    Income,

    /// <summary>Zorunlu borç ve taksit ödemeleri.</summary>
    Mandatory,

    /// <summary>Yaşam gideri ve planlı harcamalar.</summary>
    Expense,

    /// <summary>Faiz maliyetleri (kart devir faizi, finansman açığı faizi).</summary>
    Interest,

    /// <summary>Pozitif dönem neti ve devreden fazla bakiye.</summary>
    Surplus,

    /// <summary>Finansman açığı (negatif bakiye / KMH kullanımı).</summary>
    Deficit,

    /// <summary>Dönem sonu ve dönem başı nötr projeksiyon bakiyeleri.</summary>
    Projection
}
