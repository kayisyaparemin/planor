namespace Mizan.Presentation.Automation;

/// <summary>
/// Profil seçimi ekranına G1b'de eklenen girişin otomasyon kimlikleri.
/// <see cref="AutomationIds"/> 200 satır sınırına dayandığı için ayrı dosyadadır, aynı ad alanındadır.
/// </summary>
public static class ProfileSelectionAutomationIds
{
    /// <summary>
    /// Eski uygulamadan profil alma butonu kimliği. Dolu liste ve ilk kurulum durumları birbirini dışladığı için
    /// ikisinde aynı kimlik kullanılır.
    /// </summary>
    public const string BtnImportLegacy = "btn-import-legacy";
}
