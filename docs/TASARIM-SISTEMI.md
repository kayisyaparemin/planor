# Planör — Tasarım Sistemi

Görsel kararların **tek kaynağı** burasıdır. Marka, renk, punto, boşluk, ikon, bileşen ve
grafik tanımları yalnız burada yazılır; XAML bunları `{DynamicResource}` / `{StaticResource}`
ile kullanır, değer kopyalamaz.

## Temel ilke

> **Bir token, ancak onu ihlal eden XAML'i kırmızıya düşüren bir test varsa bu dosyaya
> girer.**

Testi olmayan görsel fikirler aşağıdaki "Tavsiyeler" bölümünde durur. Oradan buraya terfi
etmenin tek yolu, onu koruyan testi yazmaktır.

## Bu dosya makine tarafından okunur

Beş tablo testler tarafından ayrıştırılır. **Biçimlerini bozma**: kolon sırası ve kod
işaretleri (`` ` ``) testin ayrıştırma kuralıdır.

| Tablo | Okuyan test |
|---|---|
| § Marka | `DesignBrandTests.UygulamaAdi_SistemdekiAdla`, `DesignBrandTests.GorunenMetin_EskiAdiIceremez` |
| § Renk token'ları (Koyu **ve** Açık kolonu) | `DesignTokenTests.Renkler_SistemdekiTokenlarlaBirebir`, `DesignTokenTests.Xaml_RenkTokeni_DynamicResourceIle`, `DesignBrandTests.PlatformRenkleri_TokenlarlaAyni` |
| § Yasaklı token adları | `DesignTokenTests.TokenAdi_BoyaAdiOlamaz` |
| § Tip skalası | `DesignTokenTests.Xaml_FontSize_SkalaDisiOlamaz` |
| § Kontrast çiftleri (Koyu **ve** Açık kolonu) | `DesignContrastTests.KontrastCiftleri_EsigiGecer` |

---

## Marka

Ürünün adı **Planör**. Kod deposunun adı **Mizan** olarak kalır (`GS6`).

Bu tabloyu `DesignBrandTests` okur: "Ürün adı" satırı uygulama etiketiyle
(`<ApplicationTitle>`) birebir aynı olmalı, "Eski ad" satırı kullanıcıya görünen hiçbir
metinde geçemez.

| Alan | Değer |
|---|---|
| Ürün adı | `Planör` |
| Eski ad | `Mizan` |

Kod adıyla ürün adı ayrı şeylerdir:

| Yüzey | Ad | Sebep |
|---|---|---|
| Uygulama etiketi, mağaza adı, ekran metinleri, bildirimler | **Planör** | Kullanıcı yalnız bunu görür |
| `Mizan.sln`, `Mizan.*` projeleri, ad alanları, `mizan-v2` deposu | **Mizan** | Taşıma sürüyor; yarıda kalan yeniden adlandırma eski projenin hastalığıydı (`GS6`) |
| `ApplicationId` (`com.mizan.app`) | **Mizan** | Taşıma planının kararı (`G1`); bu protokol dokunmaz |

### İşaret

Planör silueti (titanyum) ve yükselen gösterge çizgisi (turuncu), grafit kare zemin üstünde.

| Varlık | Dosya | Zemin |
|---|---|---|
| Uygulama ikonu | `Resources/AppIcon/appicon.svg` (düz zemin) + `appiconfg.svg` (işaret) | `Backdrop` koyu değeri |
| Açılış ekranı | `Resources/Splash/splash.svg` (işaret) | `Backdrop` koyu değeri, **iki temada da** |

- İşaret **vektördür** ve kullanıcıdan gelir. Ajan logoyu çizmez, tanıtım görselinden
  izlemez ya da "benzerini" üretmez. Dosya yoksa adım durur.
- **Turuncu yalnız işaretin içinde yaşar.** Arayüz token'ı değildir (`GS8`).
- İşaret uygulamanın içinde kullanılmaz: sayfa başlığında, kartta, boş durumda logo yok.
  Marka, ikon ve açılış ekranında görünür, arayüzde ise renkleri ve tonuyla.

---

## Tema

**İki tema: koyu ve açık. Uygulama sistem temasını izler.** Planör marka paleti iki temayı
birlikte tanımlıyor (`GS7`). Kural **GK8**: iki tema, **tek mekanizma**.

| Parça | Nasıl |
|---|---|
| Palet dosyaları | `Resources/Styles/DarkPalette.xaml` ve `LightPalette.xaml`. İkisi de `x:Class`'lı `ResourceDictionary` (kod İngilizce, `rules/02-okunabilirlik.md`). **Anahtar kümeleri birebir aynı**, değerler § Renk token'ları'ndaki Koyu/Açık kolonlarından |
| Tema seçimi | Tek yerde: `App`, açılışta `RequestedTheme`'e göre paleti birleştirir ve `RequestedThemeChanged`'de değiştirir. `UserAppTheme` ayarlanmaz |
| XAML'de renk | Yalnız `{DynamicResource <token>}`. `{StaticResource}` ilk temada donar, tema değişince ekran yarım boyanır |
| `AppThemeBinding` | **Yasak.** Tema kararını sayfalara dağıtır; her sayfa iki değeri ayrı ayrı taşır ve biri eksik kalır |
| Android | `Platforms/Android/Resources/values/colors.xml` (açık) ve `values-night/colors.xml` (koyu): `colorPrimary` ve `colorPrimaryDark` = `Backdrop`, `colorAccent` = `Indicator` |
| Uygulama içi tema seçici | **Yok.** İstenirse `EK-V13` Aşama 4'te karar verilir |

Yarım tema, yarım kalmış bir yeniden adlandırmayla aynı şeydir. Bu yüzden bir token yalnız
bir temada tanımlıysa derleme değil **test** kırmızıya düşer
(`Renkler_SistemdekiTokenlarlaBirebir` iki palet dosyasını da tabloyla karşılaştırır).

---

## Renk token'ları

Dosyalar: `src/Mizan.App/Resources/Styles/DarkPalette.xaml` (Koyu kolonu),
`LightPalette.xaml` (Açık kolonu)

Token adı **rolü** söyler, boyayı söylemez. Planör'de bu kural bir adım daha keskin: aynı
değer iki temada farklı rollere düşüyor. `#F3F4F6` koyu temada **birincil metin**, açık temada
**kart zemini**. Adı `Titanium` olan bir token hangi rolü taşıdığını söyleyemez.

### Yüzeyler

| Token | Koyu | Açık | Nerede |
|---|---|---|---|
| `Backdrop` | `#1E232A` | `#E5E7EB` | Sayfa zemini, kabuk başlığı, durum çubuğu. Kart dışındaki her yer. |
| `SurfaceCard` | `#272D36` | `#F3F4F6` | Kart, liste kabı, grafik kartı. |
| `SurfaceSunken` | `#13161B` | `#D9DDE3` | Giriş alanı (`Entry`, `Picker`), iskelet yükleme bloğu. |
| `SurfaceHero` | `#2B4C6F` | `#CFDBE8` | Ekranın tek hero kartı ve bilgi şeridi. Ekranda **en fazla bir kez**. |
| `SurfaceChart` | `#233040` | `#DDE5EE` | Grafik dolgusu, ikincil şerit. |

### Çizgiler

| Token | Koyu | Açık | Nerede |
|---|---|---|---|
| `BorderSubtle` | `#343B46` | `#D4D8DE` | Kart içi ayırıcı, liste satırı arası. |
| `BorderStrong` | `#7A889B` | `#64748B` | Odaklanmış giriş alanı, ikincil buton kenarı. |

### Aksiyon

Birincil aksiyon **tek renklidir**: koyu temada titanyum, açık temada grafit. Renk aksiyona
değil **bilgiye** harcanır.

| Token | Koyu | Açık | Nerede |
|---|---|---|---|
| `ActionFill` | `#E5E7EB` | `#1E232A` | Birincil buton dolgusu. |
| `ActionFillPressed` | `#C3C8D0` | `#13161B` | Basılı ve devre dışı hâl. |

### Gösterge

Logodaki yükselen çizginin arayüzdeki karşılığı. Ekrandaki **tek kromatik vurgu**.

| Token | Koyu | Açık | Nerede |
|---|---|---|---|
| `Indicator` | `#6E97C6` | `#3B6998` | Grafik çizgisi, ilerleme çubuğu, seçili durum, gezinme oku. **Metin rengi değildir.** |

### Metin

| Token | Koyu | Açık | Nerede |
|---|---|---|---|
| `TextPrimary` | `#F3F4F6` | `#1E232A` | Başlık, rakam (hero rakam dahil), gövde. |
| `TextSecondary` | `#A7B0BC` | `#4E5B6D` | İkincil satır, etiket, `Eyebrow`, tarih aralığı. |
| `TextMuted` | `#7A889B` | `#64748B` | **Yalnız ≥ 19 pt, pasif öğe veya dekoratif.** Gövde metni olamaz. |
| `TextOnAction` | `#1E232A` | `#F3F4F6` | `ActionFill` üzerindeki metin. |
| `TextOnHero` | `#F3F4F6` | `#1E232A` | `SurfaceHero` üzerindeki metin. |

### Semantik

Her semantik rolün bir yüzeyi ve bir metni var; ikisi birlikte kullanılır ve bu birliktelik
kontrast tablosunda test edilir.

| Token | Koyu | Açık | Anlamı |
|---|---|---|---|
| `PositiveSurface` | `#1C3A2D` | `#D7EDE0` | Lehte fark, tamamlanmış, ödenmiş. |
| `PositiveText` | `#7CC29C` | `#1E6A45` | Aynı anlamın metin hâli. |
| `NegativeSurface` | `#46242A` | `#F5DCD9` | Açık, eksi bakiye, gecikmiş. |
| `NegativeText` | `#EE9D96` | `#A2362F` | Aynı anlamın metin hâli. |
| `WarningSurface` | `#3B3320` | `#F3E8C9` | Dikkat gerektiren ama hatalı olmayan durum. |
| `WarningText` | `#DDB968` | `#765710` | Aynı anlamın metin hâli. |

### Karşılaştırma

`ComparisonStrip`'in iki nötr kutusu. Üzerlerindeki metin `TextPrimary`'dir; renk yalnız
**FARK** kutusunda (semantik) konuşur.

| Token | Koyu | Açık | Anlamı |
|---|---|---|---|
| `PlanSurface` | `#233040` | `#DDE5EE` | **Planlanan** değer. `SurfaceChart` ile aynı değer, ayrı rol. |
| `ActualSurface` | `#313843` | `#DCDFE4` | **Gerçekleşen** değer. |

> `PlanSurface` ve `SurfaceChart` aynı değeri taşıyor ama **ayrı token'lar.** Sebep: ikisi
> ayrı rol; biri ileride değişirse diğeri değişmemeli. Bu, eski `Canvas = SoftYellow` takma
> adının tersidir: orada iki ad tek rol içindi, burada iki rol tek değeri paylaşıyor.

**Toplam: 23 token, her biri iki temada.** Yeni bir renk token'ı eklemek bu dosyayı, iki
palet dosyasını ve testi birlikte değiştirmek demektir. `T1` bittikten sonra bu bir tasarım
kararıdır ve `GS` kaydı gerektirir.

---

## Yasaklı token adları

Bu tabloyu `DesignTokenTests.TokenAdi_BoyaAdiOlamaz` okur ve `DarkPalette.xaml`,
`LightPalette.xaml` ile `Styles.xaml`'daki `x:Key` değerlerini tarar. Eşleştirme tam kelime
(PascalCase token) bazlıdır: `AccentGold` → `Accent` + `Gold` → kırmızı.

| Yasaklı ad | Yerine | Sebep |
|---|---|---|
| `White` | `TextPrimary` / `Backdrop` | Eskide `White` = `#FFFACF` (sarı) idi. |
| `Black` | `Backdrop` | Saf siyah kullanılmıyor. |
| `Lime` | `PositiveText` | Eskide `Lime` = `#FFCABF` (pembe) idi. |
| `Mint` | `Indicator` | Eskide `Mint` = `#BFEAFF` (mavi) idi. |
| `Sun` | `WarningText` | Boya adı. |
| `Pink` | — | Boya adı. |
| `Canvas` | `Backdrop` | `SoftYellow`'un takma adıydı. |
| `Line` | `BorderSubtle` | `SoftLavender`'ın takma adıydı. |
| `PrimaryDeep` | `SurfaceHero` | `Ink`'in takma adıydı. |
| `Ink` | `TextPrimary` | Boya adı. |
| `Soft*` | Rol adı | `SoftPink`, `SoftPeach`, `SoftYellow`, `SoftSky`, `SoftLavender`, beşi de boya adı. |
| `Slate*` / `Gray*` / `Grey*` | Rol adı | Tailwind tonu, rol değil. |
| `Gold` | `ActionFill` / `Indicator` | Eski konseptin altın vurgusu (`AccentGold`). Planör'de altın yok. |
| `Blue` | `Indicator` / `SurfaceHero` | Eski konseptin `AccentBlue`'su. Boya adı. |
| `Orange` / `Amber` | — / `WarningText` | Marka paletinin boya adları ("Teknik Gösterge Turuncusu", "Kehribar"). Turuncu token değil (`GS8`). |
| `Titanium` | `TextPrimary` / `SurfaceCard` / `ActionFill` | Marka paletinin boya adı. Aynı değer iki temada üç ayrı rol taşıyor; boya adı bunu saklar. |
| `Graphite` / `Anthracite` | `Backdrop` / `TextPrimary` / `ActionFill` | Aynı sebep. |
| `Steel` / `Aviation` | `Indicator` / `SurfaceHero` | Boya adı ("Çelik Mavisi", "Havacılık Mavisi"). |
| `Metallic` | `BorderStrong` / `TextMuted` | Boya adı ("Nötr Metalik Gri"). |

---

## Tip skalası

Yedi kademe. Sekizincisi yok; bir metin bu kademelerden birine sığmıyorsa yerleşim
yanlıştır.

Dosya: `src/Mizan.App/Resources/Styles/Tipografi.xaml`

Bu tabloyu `DesignTokenTests.Xaml_FontSize_SkalaDisiOlamaz` okur: XAML'de `FontSize="24"`
gibi sayısal literal bulursa kırmızıya düşer; yalnız `FontSize="{StaticResource TypeTitle}"`
biçimine izin verir.

| Token | Punto | Ağırlık | Nerede |
|---|---:|---|---|
| `TypeHero` | 34 | SemiBold | Ekranın tek hero rakamı veya büyük tarih başlığı. Ekranda **en fazla bir kez**. |
| `TypeTitle` | 26 | SemiBold | Sayfa başlığı. |
| `TypeSection` | 19 | SemiBold | Kart başlığı, bölüm başlığı. |
| `TypeFigure` | 16 | SemiBold | Satır içi rakam, tutar. |
| `TypeBody` | 14 | Regular | Gövde metni, liste satırı. |
| `TypeCaption` | 12 | Regular | İkincil satır, tarih, not. |
| `TypeEyebrow` | 11 | SemiBold | CAPS etiket. `CharacterSpacing="1.2"`. |

Yazı tipi: `OpenSansRegular` / `OpenSansSemibold` (mevcut kaynaklar). Planör marka tarifi
yazı tipi tanımlamıyor; yeni font eklenmez.

Hero rakam **`TextPrimary`** rengindedir. Öne çıkaran rengi değil, boyutu ve etrafındaki
boşluktur.

---

## Boşluk ve yarıçap

Hepsi 4'ün katı. Kural GK3: XAML'de `Margin`, `Padding`, `Spacing`, `CornerRadius`,
`HeightRequest` için sayısal literal yasak; token kullanılır.

| Token | Değer | Nerede |
|---|---:|---|
| `Space1` | 4 | İkon ile metin arası. |
| `Space2` | 8 | Satır içi öğeler arası. |
| `Space3` | 12 | Kart içi dikey boşluk. |
| `Space4` | 16 | Kart iç dolgusu, sayfa kenar boşluğu. |
| `Space5` | 24 | Kartlar arası. |
| `Space6` | 32 | Bölümler arası. |

Yarıçaplar Planör'ün "endüstriyel ciddiyet" tarifine göre keskinleşti (`GS9`). Pill buton yok.

| Token | Değer | Nerede |
|---|---:|---|
| `RadiusChip` | 4 | Çip, badge, küçük kutu, `ComparisonStrip` kutusu. |
| `RadiusCard` | 8 | Kart, buton, giriş alanı. |
| `RadiusHero` | 12 | Hero kartı, bilgi şeridi. |
| `RadiusPill` | 999 | Yalnız ilerleme çubuğunun uçları. |

| Token | Değer | Nerede |
|---|---:|---|
| `StrokeHairline` | 1 | Tek çizgi kalınlığı. Başka kalınlık yok. |

Dokunma hedefi: **en az 48×48**. Bu bir token değil, sınır; `MinimumHeightRequest="48"`
bileşenlerin içinde tanımlıdır ve ekranda tekrarlanmaz.

---

## Kontrast çiftleri

Bu tabloyu `DesignContrastTests.KontrastCiftleri_EsigiGecer` okur: her satırı **iki temada
ayrı ayrı** hesaplar (değerleri `DarkPalette.xaml` ve `LightPalette.xaml`'dan alır) ve
herhangi biri **eşik** kolonundan küçükse kırmızıya düşer.

Eşik kuralı: gövde metni **4,5**; ≥ 19 pt metin ve arayüz öğesi (buton kenarı, grafik
çizgisi, ilerleme çubuğu, odak kenarı) **3,0**.

| Metin token'ı | Yüzey token'ı | Eşik | Koyu | Açık |
|---|---|---:|---:|---:|
| `TextPrimary` | `Backdrop` | 4,5 | 14,4 | 12,8 |
| `TextPrimary` | `SurfaceCard` | 4,5 | 12,6 | 14,4 |
| `TextPrimary` | `SurfaceSunken` | 4,5 | 16,5 | 11,6 |
| `TextSecondary` | `Backdrop` | 4,5 | 7,2 | 5,6 |
| `TextSecondary` | `SurfaceCard` | 4,5 | 6,3 | 6,3 |
| `TextMuted` | `SurfaceCard` | 3,0 | 3,8 | 4,3 |
| `TextOnAction` | `ActionFill` | 4,5 | 12,8 | 14,4 |
| `TextOnAction` | `ActionFillPressed` | 4,5 | 9,4 | 16,5 |
| `ActionFill` | `Backdrop` | 3,0 | 12,8 | 12,8 |
| `ActionFill` | `SurfaceCard` | 3,0 | 11,2 | 14,4 |
| `ActionFill` | `SurfaceHero` | 3,0 | 7,2 | 11,2 |
| `TextOnHero` | `SurfaceHero` | 4,5 | 8,1 | 11,2 |
| `BorderStrong` | `SurfaceCard` | 3,0 | 3,8 | 4,3 |
| `Indicator` | `Backdrop` | 3,0 | 5,2 | 4,6 |
| `Indicator` | `SurfaceCard` | 3,0 | 4,6 | 5,2 |
| `Indicator` | `SurfaceChart` | 3,0 | 4,4 | 4,5 |
| `PositiveText` | `SurfaceCard` | 4,5 | 6,6 | 6,0 |
| `NegativeText` | `SurfaceCard` | 4,5 | 6,5 | 6,1 |
| `WarningText` | `SurfaceCard` | 4,5 | 7,4 | 6,1 |
| `PositiveText` | `PositiveSurface` | 4,5 | 5,9 | 5,3 |
| `NegativeText` | `NegativeSurface` | 4,5 | 6,4 | 5,2 |
| `WarningText` | `WarningSurface` | 4,5 | 6,7 | 5,5 |
| `TextPrimary` | `PositiveSurface` | 4,5 | 11,3 | 12,9 |
| `TextPrimary` | `NegativeSurface` | 4,5 | 12,3 | 12,1 |
| `TextPrimary` | `WarningSurface` | 4,5 | 11,4 | 12,9 |
| `TextPrimary` | `PlanSurface` | 4,5 | 12,2 | 12,4 |
| `TextPrimary` | `ActualSurface` | 4,5 | 10,7 | 11,8 |

"Koyu" ve "Açık" kolonları bilgi içindir; doğruluğundan test sorumludur. Bir token değeri
değişirse bu kolonlar bayatlar; testin verdiği değeri yaz.

`ActionFill / SurfaceHero` satırı bilerek var: hero kartın içindeki birincil aksiyon
(`HeroInputCard`) iki temada da seçilebilir olmalı. Butonu çelik mavisiyle (`#3B6998`)
boyayan ilk taslakta bu satır **1,55** veriyordu; mavi buton mavi kartın içinde kayboluyordu.
Aksiyonun tek renkli olmasının sebebi bu (`GS8`).

---

## İkonlar

**Tek kaynak:** `src/Mizan.App/Icons/Icons.cs`

Yazı tipi: **Material Symbols Rounded** (statik `.ttf`, `Resources/Fonts/`). Değişken font
sürümü Android'de güvenilir yüklenmediği için statik ihracı kullanılır.

XAML ham glif veya emoji **içeremez** (kural GK6). Kullanım:

```xml
<Label Text="{x:Static icons:Icons.ChevronRight}"
       FontFamily="MaterialSymbols"
       FontSize="{StaticResource IconMedium}"
       TextColor="{DynamicResource Indicator}" />
```

İkon boyutu üç kademe: `IconSmall` 16, `IconMedium` 20, `IconLarge` 24. Başka boyut yok.

İkon rengi üç token'dan biri: `TextPrimary` (ana), `TextSecondary` (ikincil), `Indicator`
(seçili durum, gezinme oku).

Envanter, **konseptte görünen ikonlar.** Yeni ikon eklemek `Icons.cs`'e satır eklemek
demektir ve sebebini `<summary>` taşır:

| Ad | Nerede |
|---|---|
| `Menu` | Kabuk başlığı (flyout). |
| `ChevronRight` | Gezinme satırı, "Detay Gör". |
| `ArrowBack` | Geri. |
| `Close` | Kapat. |
| `Insights` | Grafik kartı başlığı. |
| `Notifications` | Hatırlatıcı. |
| `Check` | Ödendi, tamamlandı. |
| `Schedule` | Ertelendi, gelecek tarih. |
| `Settings` | Ayarlar. |
| `CreditCard` | Kredi kartı satırı. |
| `AccountBalance` | Kredi satırı. |
| `Payments` | Gelir satırı. |

**Kart başına en fazla bir ikon.** Bir listedeki her satıra ikon koymak, 619 etiketli ekranın
ikonlu hâlini üretir: problem çözülmez, kılık değiştirir.

---

## Bileşenler

Dizin: `src/Mizan.App/Components/`. Her biri bir `ContentView`, ≤ 200 satır (K3).

**Yeni bileşen ancak iki farklı ekranda kullanılacaksa doğar.** Tek ekranda kalan yerleşim
o ekranın XAML'inde durur.

"Konsept karşılığı" kolonu yerleşim konseptinin panellerine işaret eder. Konseptin
**yerleşimi** geçerli, **paleti** emekli (`GS7`).

| Bileşen | Ne yapar | Konsept karşılığı |
|---|---|---|
| `PageHeader` | Eyebrow + sayfa başlığı + tek aksiyon | "PLANLA · SİMÜLE ET · KARAR VER / Ana Sayfa / Ayarlar" |
| `PeriodRail` | Dönem adı + gün sayacı + `Indicator` ilerleme çubuğu | "15 Eylül 2026 Dönemi · 4/30 gün" |
| `HeroInputCard` | `SurfaceHero`: etiket + giriş + birincil aksiyon + tek ipucu | "Mevcut Tutar / Gözlemi Kaydet" |
| `SummaryCard` | Eyebrow + tek not + sağa yaslı rakam | "PLAN / Dönem sonu 41.723 TL" |
| `ListCard` | Eyebrow + tek not + ≤ 4 satır (`DataTemplate`) + toplam | "KALAN" kartı |
| `ComparisonStrip` | Üç kutu: plan / gerçek / fark; yalnız fark semantik renkli | "PLAN · GERÇEK · FARK" şeridi |
| `MetricRow` | Sol etiket + sağ rakam, semantik renk, opsiyonel ikon | "Dönem gelirleri 150.000,00 TL" |
| `NavRow` | İkon + metin + chevron | "Önümüzdeki 12 dönem →" |
| `InfoBanner` | `SurfaceHero` şerit, **tek cümle** | "Dönem sonu planla aynı seviyede gerçekleşti" |
| `ChartCard` | Başlık + lejant çipi + grafik yüzeyi | "Eylül 2026 / Deterministik Projeksiyon" |
| `StateBlock` | Boş / yükleniyor / hata, üç durum tek bileşen | Konseptte yok, türetildi |
| `ReminderCard` | Hatırlatıcı metni + iki aksiyon | "Ödedim / Ertele" bildirimi |

`ListCard` sınırı **4 satır**. Daha fazlası varsa kart "+7 daha" satırı gösterir ve detay
sayfasına gider. Sebep: dördüncü satırdan sonra kullanıcı okumuyor, tarıyor.

`ComparisonStrip` bu sistemin en önemli bileşeni: eski `MainPage`'in **dört metin
matrisinin** yerine geçiyor.

Bileşen içinde renk her zaman `{DynamicResource}` ile bağlanır; bileşen hangi temada
olduğunu bilmez.

---

## Grafikler

Dizin: `src/Mizan.App/Charts/`. Her biri bir `IDrawable`, ≤ 200 satır.

Dört primitif var, beşincisi yok (kural GK7). Her birinin `<summary>`'si cevapladığı soruyu
cümle olarak taşır.

| Primitif | Cevapladığı soru | Girdi |
|---|---|---|
| `Sparkline` | "Yön ne, yukarı mı aşağı mı?" | Tek seri, eksen yok, etiket yok. |
| `AreaTrend` | "Seviye zamanla eşiğin altına iniyor mu?" | Tek seri + bir eşik çizgisi. |
| `StackedBar` | "Bu dönem neyden oluşuyor?" | ≤ 4 kategori. |
| `RingGauge` | "Ne kadarı tamamlandı?" | Tek oran (0–1). |

**Ekranda en fazla bir grafik.** İkinci bir grafik gerekiyorsa o ekran iki ekrandır.

### Veri sözleşmesi

Grafik verisi `Mizan.Presentation`'dan **ham seri** olarak gelir. Renk, biçim ve etiket App
katmanında eklenir, çünkü `Mizan.Presentation` MAUI'ye bakamaz (K2).

Dizin: `src/Mizan.Presentation/Charts/`

```csharp
public sealed record ChartPoint(DateOnly Date, decimal Value);
public sealed record ChartSeries(string Key, IReadOnlyList<ChartPoint> Points);
public sealed record ChartThreshold(decimal Value);
```

`Key` bir **rol** adıdır (`"planned"`, `"actual"`), renk adı değil. App katmanı `Key`'i
token'a eşler. ViewModel hiçbir renk bilmez.

### Rol → token eşlemesi

Logonun kendisi bir grafik: titanyum gövde, tek renkli yükselen çizgi. Grafikler aynı dili
konuşur.

| Rol | Token | Çizim |
|---|---|---|
| `actual` / tek seri | `Indicator` | Düz çizgi, `StrokeHairline` × 2 |
| `planned` | `TextSecondary` | Kesikli çizgi |
| Eşik | `NegativeText` | Kesikli yatay çizgi |
| Alan dolgusu | `SurfaceChart` | Dolgu |
| `RingGauge` dolu / boş | `Indicator` / `BorderSubtle` | Halka |

`IDrawable` renkleri **çizim anında** `Application.Current.Resources`'tan okur, kurucuda
saklamaz. Tema değişince `GraphicsView.Invalidate()` çağrılır; yoksa grafik eski temada kalır.

### Grafik ne zaman metinden iyidir

Grafik, **üçten fazla noktanın yönünü** gösteriyorsa metinden iyidir. Tek bir karşılaştırma
(plan vs gerçek) için grafik değil `ComparisonStrip` kullanılır: iki sayı bir grafiğe
sığdırıldığında grafik bilgi eklemiyor, yer kaplıyor.

---

## Durumlar

Her ekran üç durumu **tanımlar** ve ekran kartında yazar:

| Durum | Görünen | Kural |
|---|---|---|
| Boş | `StateBlock`: ikon + tek cümle + tek aksiyon | Cümle ne yapılacağını söyler, durumu anlatmaz. |
| Yükleniyor | İskelet (kart şekli, `SurfaceSunken`) | Spinner yok; yerleşim zıplamaz. |
| Hata | `StateBlock`: ikon + tek cümle + "Tekrar dene" | Hata metni kullanıcı diliyle; istisna metni gösterilmez. |

---

## Hareket

Minimum. Üç yerde animasyon var, başka yerde yok:

| Nerede | Ne | Süre |
|---|---|---|
| Sayfa geçişi | Kabuğun varsayılanı | — |
| İlerleme çubuğu | Değer değişiminde yumuşama | 200 ms |
| Grafik ilk çizim | Soldan sağa açılma | 300 ms |

Tema geçişi animasyonsuzdur. Sebep: her animasyon bir kare bütçesidir ve bu uygulama
Android'de düşük donanımda çalışacak. Süsleme için kare harcanmaz. Planör motorsuz uçar;
gereksiz hareket onun dili değil.

---

## Tavsiyeler *(kural değil, testi yok)*

Buradaki maddeler ihlal edilirse hiçbir test kırmızıya düşmez. Kural kitabına terfi etmenin
yolu testini yazmaktır.

- **Renk bilgi taşır, süs taşımaz.** Bir ekranda `Indicator` en fazla iki yerde görünür:
  biri grafik ya da ilerleme, biri seçili durum.
- Bir ekranda **en fazla iki** semantik renk kullan. Üç renk, hiçbirinin dikkat çekmemesi
  demek.
- Rakamı büyütmek yerine yanındaki her şeyi küçült. `TypeHero`'yu ikinci bir yerde
  kullanmak isteğin, aslında kartın kalabalık olduğunun işareti.
- Bir kart başlığı soru olarak okunabiliyorsa doğru yazılmış: "Bu dönemde ne olacak?"
- **Planör'ün sesi mühendis sesi:** kısa, ölçülü, rakamla konuşan. Ünlem yok, "harika!"
  yok, kutlama yok. "Dönem sonu planla aynı seviyede" yeter.
- Boş durum cümlesi kullanıcıyı suçlamaz. "Henüz gelir eklemedin" değil, "İlk gelirini
  ekleyerek başla".
- Konseptte olmayan bir ekran tasarlıyorsan, önce konseptteki hangi ekrana benzediğini söyle.
  Benzemiyorsa o ekran fazla karmaşıktır.
- Yeni bir ekranı iki temada da düşün. Koyu temada güzel duran ince bir çizgi, açık temada
  kaybolabilir; kontrast testi yalnız tablodaki çiftleri görür.
