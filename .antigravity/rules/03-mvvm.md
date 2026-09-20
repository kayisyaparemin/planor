# 03 — Sunum Katmanı ve MVVM

## Neden `Mizan.Presentation` ayrı bir proje

ViewModel'ler `net8.0` bir kütüphanede yaşar ve **MAUI'ye referans vermez.**
Bunun üç doğrudan sonucu var:

1. `Shell.Current`, `DisplayAlert`, `Application.Current`, `Page` — hiçbiri **derlenmez.**
   Eski projede bunlar yazılı kurallardı ve düzenli olarak ihlal ediliyordu.
2. ViewModel'ler `Mizan.Presentation.Tests` içinde **normal unit testlerle** test edilir.
   Emülatör gerekmez, MAUI host gerekmez.
3. Eskiden bu imkânsız olduğu için yazılmış 12 adet "XAML metnini `File.ReadAllText` ile
   tarayan" sahte test bu repoda hiç doğmaz.

## ViewModel kuralları

**Ham veri sunar, metin üretmez.** `decimal`, `DateOnly`, `bool` sunulur; `"12.500,00 ₺"`
üretilmez. Para ve tarih biçimlendirmesi XAML tarafında `IValueConverter` ile yapılır.
Kültür (`tr-TR`) sunum katmanında değil, converter'da tanımlıdır.

```csharp
// ❌  ViewModel'de biçimlendirme
[ObservableProperty] private string remainingText = "—";

// ✅  Ham değer + XAML converter
[ObservableProperty] private decimal? remaining;
```

**`async void` yasak.** Komutlar `[RelayCommand]` ile `Task` döner.

**Navigasyon ve diyalog port üzerinden.** `INavigationService`, `IDialogService`.
Rota sabitleri tek yerde: `Mizan.Presentation/Navigation/Routes.cs`.

**`IServiceProvider` enjekte edilmez.** Yapıcı enjeksiyonu, en fazla 5 bağımlılık.
Daha fazlası gerekiyorsa ekran iki ViewModel'e bölünür (bir çocuk ViewModel özellik olarak
açılır — bu meşru ve tercih edilen yoldur).

**Meşgul durumu tek desen:**
```csharp
[RelayCommand]
private async Task SaveAsync()
{
    if (IsBusy) { return; }
    try
    {
        IsBusy = true;
        await _obligations.SaveAsync(...);
    }
    finally
    {
        IsBusy = false;
    }
}
```

## Sayfa ve XAML kuralları

- **Code-behind'de yalnız `InitializeComponent()` bulunur.** Buton tıklaması, liste seçimi,
  form aksiyonu — hepsi `Command` / `CommandParameter` ile bağlanır.
- Bir code-behind'de `async void` yalnız MAUI'nin dayattığı event handler'larda olabilir ve
  gövdesi **baştan sona** `try/catch` içindedir (aksi hâlde Android'de süreç çöker).
- Her `ContentPage` ve her `DataTemplate` `x:DataType` taşır (derlenmiş binding).
- `DataTemplate` içinde `x:Reference PageRoot` **yasaktır** — Release/AOT'ta sessizce kopar.
  Komut gerekiyorsa öğenin kendi ViewModel'ine koy.
- Ekranda görünen her metin `Resources/Strings` üzerinden gelir; XAML'e gömülü Türkçe yazı yok.

## AutomationId'ler

**Tek kaynak:** `src/Mizan.Presentation/Automation/AutomationIds.cs`

```csharp
/// <summary>
/// Uçtan uca testlerin ekrandaki öğeleri bulmak için kullandığı kimlikler.
/// Tek yerde durur: eski projede aynı metinler XAML'de, iki PowerShell betiğinde
/// ve bir YAML akışında ayrı ayrı yazılıydı; biri değişince diğerleri sessizce bozuluyordu.
/// </summary>
public static class AutomationIds
{
    public const string BugunkuBakiyeGirisi = "input-today-balance";
    public const string GozlemiKaydetDugmesi = "btn-save-observation";
}
```

XAML bunu `{x:Static}` ile kullanır, string kopyalamaz:
```xml
<Entry AutomationId="{x:Static automation:AutomationIds.BugunkuBakiyeGirisi}" />
```

E2E akışları (Maestro) bu sınıftan **üretilir**, elle yazılmaz. Böylece kimlik silindiğinde
derleme kırılır; E2E sessizce yanlış yere tıklamaya devam etmez.

İsimlendirme deseni: `<tür>-<alan>-<eylem>`, kebab-case, İngilizce.
`btn-`, `input-`, `lbl-`, `card-`, `flyout-item-`.
