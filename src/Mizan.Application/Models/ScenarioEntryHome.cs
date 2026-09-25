namespace Mizan.Application.Models;

/// <summary>
/// Bir senaryo koşulunun simülatör haricinde uygulamanın hangi ekranından
/// ve formundan doğrudan girilebileceğini belirleyen konum türü.
/// </summary>
public enum ScenarioEntryHome
{
    /// <summary>Finansal Yapı ekranında simülatör ile aynı paylaşılan ortak formdan doğrudan kaydedilir.</summary>
    SharedForm,

    /// <summary>Finansal Yapı ekranının gelir formu; düzenli gelir akışını tanımlar veya günceller.</summary>
    IncomeForm,

    /// <summary>Kart Kontrol ekranındaki ekstre ödeme tercihleri.</summary>
    CardControl
}
