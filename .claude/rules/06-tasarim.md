---
paths:
  - "src/Mizan.App/**"
  - "docs/TASARIM-*.md"
  - "docs/EKRAN-KARTLARI.md"
---

# 06 — Görsel Tasarım

Bu dosya `.claude/rules/03-mvvm.md`'nin üzerine biner: o ViewModel ile sayfa arasındaki sınırı
çizer, bu sayfanın **neye benzediğini** kısıtlar.

Ürünün adı **Planör**; kod adı `Mizan` kalır (`GS6`). Tanımların tamamı
`docs/TASARIM-SISTEMI.md`'de. Buradaki on iki kural o tanımların **zorlanma biçimidir.**

## On iki görsel kural

| # | Kural | Nasıl zorlanıyor |
|---|---|---|
| **GK1** | XAML ham renk içermez; her renk `docs/TASARIM-SISTEMI.md`'deki bir token'dır ve token adı rolü söyler, boyayı söylemez | `DesignTokenTests.Xaml_HamRenk_Iceremez`, `DesignTokenTests.TokenAdi_BoyaAdiOlamaz` |
| **GK2** | `FontSize` yalnız yedi kademeli tip skalasından gelir; sayısal literal yasak | `DesignTokenTests.Xaml_FontSize_SkalaDisiOlamaz` |
| **GK3** | `Margin`, `Padding`, `Spacing`, `CornerRadius` token'dan gelir; sayısal literal yasak | `DesignTokenTests.Xaml_OlcuLiterali_Yasak` |
| **GK4** | Görsel bütçe: sayfa başına ≤ 1 hero, ≤ 4 kart, aynı anda ≤ 1 grafik, ≤ 28 `<Label>` | `DesignBudgetTests.Sayfa_GorselButceyiAsamaz`, `DesignHeroPagerTests` |
| **GK5** | Cümle bütçesi: sayfa başına ≤ 3 `Cumle_*` dizesi, her biri ≤ 90 karakter | `DesignBudgetTests.Sayfa_CumleButcesiniAsamaz` |
| **GK6** | İkon tek kaynaktan (`Icons.cs`); XAML'de ham glif veya emoji yok; boyut 16/20/24 | `DesignTokenTests.Xaml_HamGlif_Iceremez` |
| **GK7** | Grafik yalnız beş primitiften biri olabilir; verisi `Mizan.Presentation`'dan ham seri gelir | `DesignChartTests.Grafik_YalnizPrimitiflerden` |
| **GK8** | İki tema, tek mekanizma: iki palet aynı anahtar kümesiyle tanımlı, XAML renge yalnız `DynamicResource` ile bakar, `AppThemeBinding` yasak | `DesignTokenTests.Renkler_SistemdekiTokenlarlaBirebir`, `DesignTokenTests.Xaml_RenkTokeni_DynamicResourceIle`, `DesignTokenTests.Xaml_AppThemeBinding_Yasak` |
| **GK9** | Her sayfanın `docs/EKRAN-KARTLARI.md`'de bir kartı vardır ve kart sayfadaki kartları/grafiği listeler | `DesignBudgetTests.HerSayfanin_EkranKarti_Var` |
| **GK10** | `docs/TASARIM-SISTEMI.md`'deki kontrast çiftleri **iki temada da** eşiği geçer | `DesignContrastTests.KontrastCiftleri_EsigiGecer` |
| **GK11** | Görünen ad tek kaynaktan: uygulama etiketi `Planör`, eski ad görünen hiçbir metinde yok; XAML dışındaki platform renkleri token değerleriyle aynı | `DesignBrandTests.UygulamaAdi_SistemdekiAdla`, `DesignBrandTests.GorunenMetin_EskiAdiIceremez`, `DesignBrandTests.PlatformRenkleri_TokenlarlaAyni` |
| **GK12** | XAML'deki her `{StaticResource X}` tanımlı bir anahtara bakar. XAML çalışma anında yüklendiği için eksik anahtar derlemeyi kırmaz, sayfa açılınca uygulamayı çökertir | `DesignResourceTests.Xaml_StaticResource_TanimliAnahtaraBakar` |

GK1–GK12'yi bozan bir şey yazmak zorunda kaldıysan, **kodu değil kuralı tartışmaya aç.**
Sessizce istisna yapma; kullanıcıya "şu kural şu sebeple engel oluyor, ne yapalım?" diye sor.

Testler `Mizan.Architecture.Tests` içinde yaşar; kaynak dosya metni tarama izni yalnız o
projede var (kural K7).

---

## GK1 — Ham renk yok

```xml
<!-- ❌ -->
<Label TextColor="#F3F4F6" />
<Border BackgroundColor="White" />

<!-- ✅ -->
<Label TextColor="{DynamicResource TextPrimary}" />
<Border BackgroundColor="{DynamicResource SurfaceCard}" />
```

Yasak olan iki şey ayrı ayrı test edilir:

1. **Değer kopyalamak**: XAML'de `#RRGGBB` veya adlandırılmış MAUI rengi (`White`, `Red`).
2. **Boya adı kullanmak**: palet dosyasına `x:Key="Titanium"` diye bir token eklemek.
   Yasaklı ad tablosu `docs/TASARIM-SISTEMI.md` § Yasaklı token adları'ndadır.

Sebep: eski projede `White` anahtarının değeri `#FFFACF`, yani sarı idi. Planör'de sebep
daha da güçlü: `#F3F4F6` koyu temada **birincil metin**, açık temada **kart zemini**. Aynı
boya iki temada iki ayrı rol; boya adı hangisi olduğunu söyleyemez.

Marka paletinin kendi adları da boya adıdır: "Titanyum", "Grafit", "Kehribar", "Çelik
Mavisi". Hiçbiri token adı olmaz.

## GK2 — Punto skaladan gelir

```xml
<!-- ❌ -->  <Label FontSize="18" />
<!-- ✅ -->  <Label FontSize="{StaticResource TypeSection}" />
```

Yedi kademe var: `TypeHero`, `TypeTitle`, `TypeSection`, `TypeFigure`, `TypeBody`,
`TypeCaption`, `TypeEyebrow`. Sekizinci bir kademeye ihtiyaç duyduğunu düşünüyorsan
yerleşim yanlıştır; hiyerarşiyi punto ile değil boşlukla kur.

`TypeHero` bir sayfada **en fazla bir kez** kullanılır (GK4 bunu sayar). Hero rakam
`TextPrimary` rengindedir; öne çıkaran renk değil boyuttur.

Punto ve ölçü token'ları temadan bağımsızdır, `StaticResource` ile bağlanır. `DynamicResource`
yalnız renk içindir.

## GK3 — Ölçü token'dan gelir

```xml
<!-- ❌ -->  <VerticalStackLayout Spacing="10" Padding="14,18" />
<!-- ✅ -->  <VerticalStackLayout Spacing="{StaticResource Space3}"
                                 Padding="{StaticResource CardPadding}" />
```

Hepsi 4'ün katı. `10` ve `18` gibi değerler eski projede vardı ve hiçbir ritim üretmiyordu.

İstisna: `Grid` içindeki `ColumnDefinitions`/`RowDefinitions` oranları (`*`, `Auto`, `2*`)
ölçü literali sayılmaz.

## GK4 — Görsel bütçe

Sayfa başına:

| Sınır | Değer | Nasıl sayılıyor |
|---|---:|---|
| Hero rakam | **1** | `TypeHero` kullanımı |
| Hero yüzey | **1** | `SurfaceHero` kullanımı (`HeroInputCard` veya `InfoBanner`) |
| Kart | **4** | `SummaryCard`, `ListCard`, `ChartCard`, `HeroInputCard`, `EntryTypeTiles` örnekleri |
| Grafik | **1 (aynı anda)** | `ChartCard`, `Sparkline`, `AreaTrend`, `StackedBar`, `RingGauge`, `ColumnTrend`, `GraphicsView`; `HeroPager` dışındakilerin hepsi + pager başına en kalabalık sayfa |
| Kaydırılan hero | **1** | `HeroPager` örneği; 1 kart sayılır, sayfaları ayrıca kart sayılmaz |
| Hero sayfa | **2** | `HeroPage` örnekleri; sayfa başına en fazla 1 grafik |
| `<Label>` | **28** | XAML'deki `<Label` sayısı |
| Gezinme satırı | **5** | `NavRow` örnekleri |

Grafik sınırı gözün **aynı anda** gördüğüdür (`GS22`): kaydırılan hero'nun gizli sayfasındaki
grafik sayılmaz, ama pager'ın yanında ikinci bir grafik duruyorsa ekran iki grafiklidir.
Kaydırılan kartın ve "Bakiye gir" önizlemesinin zemini tonlu `SurfaceChart`'tır (`GS24`, `GS25`);
`SurfaceHero` koyu temada grafiği taşımaz (kontrast tablosu).

Sınıra dayandığında **çıkar, küçültme.** Üç yol var:

1. Bilgiyi bir seviye derine al: `NavRow` + detay sayfası.
2. Tekrarlayan satırı `DataTemplate`'e taşı (bir kez sayılır).
3. Ekranı ikiye böl; o ekran zaten iki ekrandı.

`<Label>` sayısı `DataTemplate` içindekileri **bir kez** sayar. Yani 12 satırlık bir liste
üç etiket eder. Bütçe böylece aynı zamanda bir yapı kuralıdır.

> Eski projede ekran başına ortalama 44, en kötüsü 86 `<Label>` vardı. 28 sınırı bu
> ölçümden geliyor, keyfî bir yuvarlak sayı değil.

## GK5 — Cümle bütçesi

Ekrandaki metin türüne göre anahtar öneki alır. `Resources/Strings` içinde:

| Önek | Ne | Sınır |
|---|---|---|
| `Baslik_` | Sayfa ve kart başlığı | — |
| `Etiket_` | Alan etiketi, eyebrow | ≤ 24 karakter |
| `Cumle_` | Açıklama cümlesi | **≤ 90 karakter, sayfa başına ≤ 3** |
| `Aksiyon_` | Buton metni | ≤ 28 karakter |
| `Bos_` / `Hata_` | Durum metni | ≤ 90 karakter |
| `Bicim_` | Ham değeri saran şablon (`{0}` yer tutuculu); XAML'de `StringFormat` ile kullanılır | — |

`Cumle_` bütçesi bu kural kitabının kullanıcıya verdiği tek sözdür: **bir ekran üç cümleden
fazlasını okutmaz.** Eski `MainPage`'de tek bir kartta iki açıklama cümlesi vardı ve ekranda
toplam yedi cümle geçiyordu.

Dördüncü cümleye ihtiyacın varsa cevap şu: o cümle bir sayı olmalı, bir ikon olmalı ya da
hiç olmamalı.

## GK6 — İkon tek kaynaktan

```xml
<!-- ❌ -->  <Label Text="→" />
<!-- ❌ -->  <Label Text="💰" />
<!-- ✅ -->  <Label Text="{x:Static icons:Icons.ChevronRight}"
                    FontFamily="MaterialSymbols"
                    FontSize="{StaticResource IconSmall}"
                    TextColor="{DynamicResource Indicator}" />
```

`Icons.cs` `AutomationIds.cs` ile aynı mantıkta çalışır: kimlik silinirse derleme kırılır.

**Kart başına en fazla bir ikon.** Her satıra ikon koymak 619 etiketli ekranın ikonlu hâlini
üretir; problem çözülmez, kılık değiştirir. Karo da kart gibi sayılır: bağımsız bir dokunma yüzeyidir ve
en fazla bir ikon taşır (tür seçici, `GS29`); bir listenin satırı karo değildir. *(Bu cümlenin testi yok;
`docs/MIMARI.md` → "Tavsiyeler" düzeyindedir, GK6'nın testi ham glif ve ikon listesini korur.)*

Eski XAML'de `→` karakteri buton metinlerine gömülüydü ("Bu dönemi kapat →"). Ok bir ikon,
metin değil.

## GK7 — Beş grafik primitifi

`src/Mizan.App/Charts/` altında yalnız şunlar olabilir: `Sparkline`, `AreaTrend`,
`StackedBar`, `RingGauge`, `ColumnTrend`.

Her birinin `<summary>`'si **cevapladığı soruyu cümle olarak** taşır (K8 zaten `<summary>`
zorunlu kılıyor; bu kural içeriğini kısıtlıyor):

```csharp
/// <summary>
/// "Bakiye nereye gidiyor, plana ve eşiğe göre neredeyim?" sorusunu cevaplar.
/// Dönem sonu bakiyesi 12 dönem boyunca sıfırın altına düşüyorsa kullanıcının
/// bunu tek bakışta görmesi gerekiyor; sayı listesi bu yönü göstermiyordu.
/// </summary>
```

Veri `Mizan.Presentation/Charts/` altındaki `ChartSeries` tipleriyle gelir. ViewModel renk
bilmez, biçim bilmez, `Key` olarak **rol** verir (`"planned"`, `"actual"`). Rolün hangi
token'a düştüğü `docs/TASARIM-SISTEMI.md` § Rol → token eşlemesi'nde yazılı.

Drawable rengi **çizim anında** kaynaklardan okur, kurucuda saklamaz; tema değişince
grafik yeniden çizilir.

Altıncı bir primitif eklemek: önce cevapladığı soruyu yaz, sonra kullanıcıya sor.

## GK8 — İki tema, tek mekanizma

```xml
<!-- ❌ -->  <Label TextColor="{AppThemeBinding Dark=#F3F4F6, Light=#1E232A}" />
<!-- ❌ -->  <Label TextColor="{StaticResource TextPrimary}" />
<!-- ✅ -->  <Label TextColor="{DynamicResource TextPrimary}" />
```

- İki palet: `DarkPalette.xaml` ve `LightPalette.xaml`. **Anahtar kümeleri birebir aynı**,
  değerleri `docs/TASARIM-SISTEMI.md` § Renk token'ları'ndaki Koyu/Açık kolonlarından.
- Tema seçimi **tek yerde**: `App`, sistem temasına göre paleti birleştirir ve tema
  değişince değiştirir. Sayfa, bileşen ve ViewModel temayı bilmez.
- XAML renge yalnız `{DynamicResource}` ile bakar. `{StaticResource}` ilk temada donar; tema
  değişince ekranın yarısı eski renkte kalır. Bu, yarım temanın sessiz hâlidir.
- `AppThemeBinding` yasak: tema kararını sayfalara dağıtır, her sayfa iki değeri ayrı ayrı
  taşır ve biri eksik kalır.

Test üç yönden bakar: iki palet dosyası tabloyla birebir mi (eksik token bir temada eksik
kalamaz), renk token'ı `StaticResource` ile bağlanmış mı, `AppThemeBinding` geçiyor mu.

GK8 eskiden "tek tema: koyu" idi (`GS1`). O kayıt kendi şartıyla iptal oldu: marka paleti
iki temayı birlikte tanımladı (`GS7`). Kuralın özü değişmedi: **yarım tema yok.**

## GK9 — Kartsız ekran olmaz

Her `*Page.xaml` için `docs/EKRAN-KARTLARI.md`'de `EK-<adım kodu>` başlıklı bir kart bulunur
ve kart şunları listeler: cevaplanan sorular, görsel bütçe sayımı, blok şeması, üç durum.

Test iki yönlü çalışır: kartsız sayfa da, sayfası olmayan kart da kırmızıdır.

Sebep: ekran kartı, Aşama 4'te verilen **çıkarma kararlarının** tek kaydı. Kayıt olmadan o
kararlar üç ay sonra "neden bu sayı burada yok?" sorusuna dönüşür.

## GK10 — Kontrast

`docs/TASARIM-SISTEMI.md` § Kontrast çiftleri tablosundaki her satır **iki temada ayrı ayrı**
hesaplanır ve eşiği geçmek zorundadır. Bir token değeri değiştiğinde bu test onu yakalar.

`TextMuted` bilerek eşiğe yakın: **gövde metni olarak kullanılamaz.** Tabloda eşiği 3,0
yazılıdır ve yalnız ≥ 19 pt metinde, pasif öğede ya da dekoratif öğede geçerlidir.

Markanın metalik grisi (`#64748B`) bu yüzden tarih ve açıklama metni **olamıyor**: açık
kartta 4,32, koyu kartta 2,91 veriyor (`GS9`). Marka bir değer önerir, kontrast testi karar
verir.

## GK11 — Görünen ad ve platform yüzeyleri

Üç şey test edilir:

1. **Uygulama etiketi**: `Mizan.App.csproj` içindeki `<ApplicationTitle>`,
   `docs/TASARIM-SISTEMI.md` § Marka'daki "Ürün adı" satırıyla birebir aynı: `Planör`.
2. **Eski ad görünmez**: "Eski ad" satırındaki kelime (`Mizan`) `Resources/Strings`
   değerlerinde ve XAML'in görünen metin özniteliklerinde (`Text`, `Title`, `Placeholder`,
   `SemanticProperties.Description`, `SemanticProperties.Hint`) **tam kelime** olarak
   geçemez. `x:Class`, `xmlns` ve `clr-namespace` değerleri taranmaz; kod adı orada meşru.
3. **Platform renkleri**: XAML dışındaki renkler GK1'in görüş alanında değil. Bu test onları
   görür: `MauiIcon` ve `MauiSplashScreen` `Color` değeri ve `appicon.svg` dolgusu =
   `Backdrop` koyu;
   `values/colors.xml` = açık palet, `values-night/colors.xml` = koyu palet
   (`colorPrimary`, `colorPrimaryDark` = `Backdrop`; `colorAccent` = `Indicator`).

Sebep: yeniden adlandırma, eski projenin en bilinen hastalığıydı. Bir ad **görünen yüzeyde**
yarım kalırsa kullanıcı iki ürün görür. Kod adının kalması bir karar (`GS6`); görünen adın
yarım kalması bir hata.

---

## Sayfa yazma sırası

XAML yazmaya başlamadan önce bu üçü hazır olmalı:

1. `docs/EKRAN-KARTLARI.md`'de ekran kartı (Aşama 4–5'te yazıldı)
2. ViewModel'in ham veri sözleşmesi (Aşama 6'da yazıldı, testleri yeşil)
3. Kullanılacak bileşenlerin listesi; hepsi `Components/` altında **zaten var**

Üçüncü madde önemli: bir ekran için yeni bileşen yazmak, o ekranın Aşama 5'inde
kararlaştırılmış olmalı. XAML yazarken bileşen icat etmek, tasarım sistemini ekran ekran
çatallamanın yoludur.

## Code-behind

`.claude/rules/03-mvvm.md` değişmedi: code-behind'de yalnız `InitializeComponent()` bulunur.
Grafik çizimi de buna dahildir: `IDrawable` ayrı bir sınıftır, code-behind'e yazılmaz.
Palet dosyalarının (`DarkPalette`, `LightPalette`) code-behind'i de yalnız
`InitializeComponent()` taşır. Tema geçişi `App`'in işidir.

## AutomationId

`.claude/rules/03-mvvm.md`'deki desen aynen geçerli. Bileşenler `AutomationId`'yi **dışarıdan**
alır:

```xml
<components:SummaryCard AutomationId="{x:Static automation:AutomationIds.PlanKarti}" />
```

Bileşenin içine sabit `AutomationId` gömmek yasaktır: aynı bileşen iki ekranda iki farklı
kimlik taşır.
