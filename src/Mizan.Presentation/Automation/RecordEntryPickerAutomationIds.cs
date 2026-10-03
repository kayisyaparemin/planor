namespace Mizan.Presentation.Automation;

/// <summary>
/// Finansal Yapı "Ekle"sinin açtığı kayıt türü seçicisinin (V6f) otomasyon kimlikleri.
/// <see cref="AutomationIds"/> 200 satır sınırına dayandığı için ayrı dosyadadır, aynı ad alanındadır.
/// </summary>
public static class RecordEntryPickerAutomationIds
{
    /// <summary>Kayıt türü seçici sayfası kimliği.</summary>
    public const string PageRecordEntryPicker = "page-record-entry-picker";

    /// <summary>Dört grubun karolarını taşıyan alanın kimliği.</summary>
    public const string PickerRecordEntryType = "picker-record-entry-type";

    /// <summary>"Hangi kart?" ikinci seviyesinde adayları taşıyan listenin kimliği.</summary>
    public const string ListRecordEntryChoices = "list-record-entry-choices";
}
