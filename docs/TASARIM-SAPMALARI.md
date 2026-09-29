# Tasarım Sapmaları — Konseptten Bilerek Ayrıldığımız Yerler

Ekran adımlarının varsayılanı "konsepti sadakatle uygula"dır. Bu dosya, o varsayılanın
**geçerli olmadığı** yerleri kaydeder.

`SAPMALAR.md`'nin görsel kardeşidir. O davranışın sapmalarını (`S`) tutar, bu görselin
(`GS`).

Bir `GS` kaydı üç soruya cevap verir: konsept ne vaat ediyor, **neden uygulanamıyor ya da
uygulanmamalı**, yerine ne yapılacak. Gerekçesi yazılmamış bir sapma, altı ay sonra
"konseptte böyle değildi" diye geri alınır.

## İki referans, iki otorite

Planör'e geçişten sonra elde iki görsel kaynak var ve **farklı soruların** otoritesidirler:

| Kaynak | Neyin otoritesi | Neyin otoritesi **değil** |
|---|---|---|
| Yerleşim konsepti (5 panel: Ana Sayfa, Simülatör, Dönem Ayrıntısı, Kurulum, Geçmiş) | Bileşenler, blok sırası, ekranın hangi soruyu cevapladığı | Renk: koyu lacivert + altın paleti **emekli** (`GS7`) |
| Planör marka paleti ve işareti | Renk, tema, ad, ikon, açılış ekranı, ses tonu | Yerleşim, veri, ekran içeriği |

Planör'ün tanıtım görseli (logo + telefon maketleri) **ikisi de değildir**: bir pazarlama
kompozisyonudur, ekran konsepti değil. Bkz. aşağıda § Konsept otorite değildir, madde 4.

## Kullanım

- **Aşama 4 (Görsel Bütçe)** bu dosyayı okur. Adımı etkileyen bir `GS` kaydı varsa karar
  zaten verilmiştir; yeniden tartışılmaz, uygulanır.
- Yeni bir görsel sapma kararı verildiğinde **Aşama 4'te** buraya yazılır, Aşama 10'da
  değil. Sebep: Aşama 5'teki düzen sözleşmesi doğrudan bu karara dayanacak.
- `Tür` sütunu: `konsept-sapması` (konsept bir şey diyor, biz başka yapıyoruz) ·
  `veri-kısıtı` (konseptin gösterdiği veri yok ya da güvenilir değil) ·
  `kasıtlı sadeleştirme` (yapılabilirdi, bilerek yapmıyoruz) ·
  `marka` (Planör marka tarifinden gelen karar).
- `Durum` sütunu: `açık` · `uygulandı` · `iptal` (gerekçesiyle).

## Numaralandırma

`GS1`–`GS19` sistem geneli kararlar (tema, palet, marka, türetme kuralı).
`GS20`+ ekran bazlı kararlar.

İptal edilen kayıt silinmez; durumu `iptal` olur ve yerine geçen kaydı gösterir. Numara bir
daha kullanılmaz.

---

## Sistem geneli

### GS1 — Tek tema: koyu. Açık tema hiç doğmuyor

| | |
|---|---|
| **Tür** | kasıtlı sadeleştirme |
| **Konsept** | Yalnız koyu tema gösteriyor; açık tema paneli yok. |
| **Eski uygulama** | Açık pastel palet (`SoftPink`, `SoftPeach`, `SoftYellow`). Yani iki palet arasında tam bir kopuş var. |
| **Neden böyleydi** | İkisini `AppThemeBinding` ile birlikte yaşatmak, birinci Mizan'ın **yarım kalmış altı yeniden adlandırmasının** renk katmanındaki tekrarı olurdu: iki tema doğar, biri hep eksik kalır, hangisinin doğru olduğu dosyaya göre değişir. |
| **Kaydın koyduğu şart** | "Açık tema ileride istenirse **iki paletin tamamı** aynı anda tanımlanır ve bu kayıt `iptal` olur." |
| **Durum** | **iptal** → `GS7`. Planör marka paleti iki temayı birlikte tanımlıyor; şart sağlandı. Yarım tema yasağı kaldırılmadı, test edilir hâle geldi. |

### GS2 — Konsepti olmayan 9 ekran türetilir, tasarlanmaz

| | |
|---|---|
| **Tür** | veri-kısıtı |
| **Konsept** | 5 ekranı kapsıyor: Ana Sayfa, Simülatör, Dönem Ayrıntısı, Kurulum özeti, Geçmiş. Bir de hatırlatıcı bildirimi. |
| **Neden sorun** | Uygulamada 14 ekran var. Kalan 9 ekranın görselini adım adım "uydurmak", eski projenin 14 farklı görsel dilini yeniden üretir; tasarım sistemi ekran ekran çatallanır. |
| **Yeni** | Türetme kuralı: **konsepti olmayan ekran, konsepti olan en yakın ekranın bileşenlerinden kurulur.** Aşama 4'te "hangi konsept ekranına benziyor" sorusu cevaplanır. Yeni bileşen ancak **iki** farklı ekranda kullanılacaksa doğar; tek ekranda kalan yerleşim o sayfanın XAML'inde durur. Planör'e geçiş bu kuralı değiştirmez: tanıtım görselindeki maketler türetme kaynağı değildir. |
| **Etkiler** | `EK-V1`, `EK-V5`, `EK-V6`, `EK-V7`, `EK-V11`, `EK-V13` |
| **Durum** | açık |

### GS3 — İki token ölçülemedi, türetildi

| | |
|---|---|
| **Tür** | veri-kısıtı |
| **Konsept** | Negatif rakam metni ve "GERÇEK" kutusunun haki tonu. |
| **Neden sorundu** | İkisi de JPEG konseptte ince metin üzerindeydi; ölçülen değerler tutarsız çıktı. `NegativeText`, `ActualSurface`, `ActualText` türetilmişti. |
| **Durum** | **iptal** → `GS9`. Konseptin paleti emekli (`GS7`); bu üç değer artık yok. Planör paletinde türetilen değerlerin tamamı `GS9`'da. `ActualText` token'ı kalktı: gerçekleşen kutusu nötr, üstündeki metin `TextPrimary`. |

### GS4 — Aynı anda tek uyarı gösterilir

| | |
|---|---|
| **Tür** | kasıtlı sadeleştirme |
| **Eski uygulama** | `MainPage` uyarıları `BindableLayout` ile **liste** hâlinde gösteriyordu; üç uyarı birden görünebiliyordu (her biri başlık + gövde + buton = 3 etiket). |
| **Neden yanlış** | İki uyarı birden gösterildiğinde ikisi de okunmuyor; üçüncüsü ekranın yarısını yiyor. Uyarı, tanım olarak "şimdi bak" demek; çoğul uyarı kendi kendini iptal eder. |
| **Yeni** | Ekranda en fazla **bir** `InfoBanner`. Sıralama ViewModel'in işi: en yüksek önemli uyarı gösterilir, diğerleri gösterilmez. Kaç uyarı olduğu bilgisi de gösterilmez, çünkü sayı bir aksiyon değil. |
| **Etkiler** | `EK-V3`, `EK-V9` |
| **Durum** | açık |

### GS5 — "Hesaplama detayı (geliştirme)" ekrandan çıkar

| | |
|---|---|
| **Tür** | kasıtlı sadeleştirme |
| **Eski uygulama** | `MainPage` içinde `ToggleCalculationDetailsCommand` ile açılan, `CalculationDetails` dizesini `FontSize="12"` ile basan bir döküm alanı. |
| **Neden yanlış** | Hata ayıklama çıktısı, üretim ekranında. Hiçbir kullanıcı sorusuna bağlanmıyor. Ayrıca ViewModel'de metin üretiyor; `.claude/rules/03-mvvm.md` bunu zaten yasaklıyor. |
| **Yeni** | Taşınmaz. Hesap doğrulaması gerektiğinde yolu testtir, ekran değil. |
| **Etkiler** | `EK-V3` |
| **Durum** | açık |

### GS6 — Ürün adı Planör; kod adı Mizan kalır

| | |
|---|---|
| **Tür** | marka |
| **Karar** | Ürün Mizan'dan **Planör**'e yeniden adlandırıldı (2026-09-23). |
| **Neden kod adı kalıyor** | Taşıma sürüyor. `Mizan.sln`'i, `Mizan.*` projelerini, ad alanlarını ve depoyu şimdi yeniden adlandırmak, taşıma protokolünün ortasında yüzlerce dosyaya dokunmak demek. Eski projenin **yarım kalmış altı yeniden adlandırması** tam olarak böyle doğdu. Kullanıcı kod adını hiçbir yerde görmüyor. |
| **Yeni** | Kullanıcıya görünen her yüzey Planör: uygulama etiketi (`ApplicationTitle`), ikon, açılış ekranı, `Resources/Strings`. **GK11** bunu testle zorluyor: eski ad görünen hiçbir metinde geçemez. Kod tanımlayıcılarında `Mizan` geçmesi hata değildir. |
| **Bu protokolün dokunmadığı** | `ApplicationId` (`com.mizan.app`) ve `G1` geçiş adımı taşıma planına aittir. Kod adının da değişmesi istenirse bu, taşıma bittikten sonra **tek adımda, tek commit'te** yapılan ayrı bir karardır; yarım yapılmaz. |
| **Etkiler** | `T2` (uygulandı), tüm V adımları |
| **Durum** | açık |

### GS7 — İki tema, tek mekanizma. Konseptin paleti emekli, yerleşimi geçerli

| | |
|---|---|
| **Tür** | marka |
| **Konsept** | Koyu lacivert + altın, yalnız koyu tema. |
| **Planör** | Marka paleti iki temayı birlikte tanımlıyor: titanyum koyu temada metin, açık temada kart zemini; grafit koyu temada zemin, açık temada başlık metni. |
| **Neden** | `GS1`'in şartı buydu ve marka tarifi onu sağladı. Yarım tema riski ortadan kalkmadı; artık test ediliyor: iki palet dosyasından biri tablodaki bir token'ı taşımazsa kırmızı. |
| **Yeni** | `DarkPalette.xaml` + `LightPalette.xaml`, sistem temasını izleyen tek geçiş noktası (`App`), XAML'de renk yalnız `{DynamicResource}`. `AppThemeBinding` hâlâ yasak (GK8). Kontrast çiftleri iki temada ayrı ayrı test edilir (GK10). Konseptin 5 paneli **yerleşim** referansı olarak kalır; **renk** referansı artık marka paletidir. |
| **Bedel** | Her ekran iki temada doğrulanır (Aşama 9'da ekran görüntüsü iki temada). Grafikler renklerini çizim anında okumak zorunda. |
| **Etkiler** | `T1`, `T3`, `T4`, tüm V adımları |
| **Durum** | açık |

### GS8 — Arayüzde tek kromatik renk: çelik mavisi gösterge. Turuncu yalnız işarette

| | |
|---|---|
| **Tür** | marka |
| **Marka** | Yükselen grafik çizgisi turuncu (`#E76F3C` / `#DE5B26`). Eski şirket çağrışımı yüzünden turuncu arayüzde istenmiyorsa çelik mavisi (`#2B4C6F` – `#3B6998`) birebir ikame. |
| **Karar** | Varsayılan: **çelik mavisi** `Indicator`. Turuncu uygulama ikonunda ve açılış ekranında, işaretin içinde kalır. Birincil aksiyon renksizdir: koyu temada titanyum, açık temada grafit. |
| **Neden** | 1) Marka notundaki çekince: turuncu eski şirketi çağrıştırıyor. 2) Gösterge dilinde turuncu/kehribar "dikkat" demek; turuncu bir ilerleme çubuğu `WarningText` ile karışır. 3) Renkli buton hero kartın içinde kayboluyordu (`#3B6998` / `#2B4C6F` = 1,55); tek renkli aksiyon iki temada da ≥ 7,2 veriyor. |
| **Bedel** | Çelik mavisinin marka tonları koyu temada kartın üstünde okunmuyor (`#3B6998` / `SurfaceCard` = 2,42). Koyu temada türetilmiş açık ton `#6E97C6` kullanılıyor (4,6). Açık temada marka tonu `#3B6998` olduğu gibi. |
| **Turuncu seçilirse** | Yalnız `Indicator` değişir. Koyu: `#E76F3C` (marka; kartta 4,45, grafik yüzeyinde 4,30). Açık: **`#C24E1E`** (türetildi; marka tonu `#DE5B26` açık grafik yüzeyinde 2,93 verip eşiği geçemiyor, `#C24E1E` 3,75). Ayrıca `WarningText` sarıya kaydırılır ki gösterge ile uyarı karışmasın. Başka token değişmez; kontrast tablosunun `Indicator` satırları yeniden hesaplanır. |
| **Kilit** | `T1`'de kullanıcı onayıyla kilitlenir; kilitlenince `uygulandı`. |
| **Etkiler** | `T1`, `T2`, `T4` |
| **Durum** | uygulandı |

### GS9 — Marka tarifinin kapsamadığı değerler türetildi

| | |
|---|---|
| **Tür** | veri-kısıtı |
| **Marka** | Dört renk ailesi tanımlıyor: titanyum, grafit, turuncu/çelik mavisi, metalik gri. Semantik renkler, ikincil yüzeyler, basılı hâller ve biçim (yarıçap) tanımlı değil. |
| **Çatışma** | Marka `#64748B`'yi "tarih aralıkları, açıklama metinleri" için veriyor ama bu değer **eşiği geçemiyor**: açık kartta 4,32 (gövde metni 4,5 ister), koyu kartta 2,91 (3,0 bile değil). |
| **Yeni** | `#64748B` açık temada `TextMuted` ve `BorderStrong` oluyor (4,3). Tarih ve açıklama metni `TextSecondary`'ye geçiyor (açık `#4E5B6D` 6,3; koyu `#A7B0BC` 6,3). Koyu temada metalik gri yerine türetilmiş `#7A889B` (3,8). Semantik, karşılaştırma ve ikincil yüzeyler grafit ile çelik mavisinin tonlarından türetildi; hepsi kontrast tablosundan geçiyor. |
| **Biçim** | Yarıçaplar "endüstriyel ciddiyet" tarifine göre keskinleşti: çip 8 → 4, kart 14 → 8, hero 18 → 12. Eski tablodaki 4'ün katı olmayan iki değer (14, 18) böylece düzeldi. Pill buton kalktı; `RadiusPill` yalnız ilerleme çubuğunda. |
| **Kaynak ayrımı** | Hangi değerin markadan geldiği, hangisinin türetildiği `C:\Users\kayis\Documents\mizan\docs\v2\04-NEDEN-BU-TASARIM.md` § Planör paleti'nde (eski depo, arşiv) tek tek yazılı. |
| **Kilit** | `T1`'de kullanıcı onayıyla kilitlenir; kilitlenince `uygulandı` ve türetilen değerler nihai sayılır. |
| **Etkiler** | `T1` |
| **Durum** | uygulandı |

### GS10 — ChartCard ve StateBlock'un T3/T4/T5 Sorumluluk Sınırları

| | |
|---|---|
| **Tür** | kasıtlı sadeleştirme |
| **Sorun** | `docs/TASARIM-SISTEMI.md` § Bileşenler tablosunda 12 bileşen sayılmakta ve `ChartCard` (10) ile `StateBlock` (11) burada yer almaktadır. Ancak `docs/TASIMA-PLANI.md`'de T4 adımı 4 adet `IDrawable` grafik primitifini, T5 adımı ise `StateBlock` (boş / yükleniyor / hata) ve iskelet yükleme desenini müstakil adımlar olarak tanımlamaktadır. |
| **Karar** | 1) `ChartCard`, T3'te grafiğin kendisini (`IDrawable`) üretmez. Başlık, lejant çipi ve grafik yüzeyini (`GraphicsView` veya genel içerik `View`) sarmalayan bir `ContentView` kabuğu olarak tanımlanır. T4 adımında üretilecek 4 `IDrawable` primitifi bu yüzeye bağlanacaktır. 2) `StateBlock`, T3'te üç durumu (`Empty`, `Loading`, `Error`) görsel olarak karşılayan tekil `ContentView` arayüzü olarak tanımlanır. T5 adımında ise sayfa genelinde dinamik iskelet yükleme deseni ve durum makinesi orkestrasyonu derinleştirilecektir. |
| **Etkiler** | `T3`, `T4`, `T5` |
| **Durum** | uygulandı |

### GS11 — Bileşenlerde Semantik Renk Yönetimi: Sıfır Kod Renkleri ve Dinamik Kaynak

| | |
|---|---|
| **Tür** | marka / kasıtlı sadeleştirme |
| **Sorun** | `ComparisonStrip`, `MetricRow` ve `InfoBanner` semantik renklere (pozitif, negatif, uyarı, nötr) ihtiyaç duyar. GK8 kuralı uyarınca `AppThemeBinding` yasaktır ve renkler yalnız `{DynamicResource}` ile bağlanabilir. Code-behind içinde renk nesneleri (`Color.FromArgb`) oluşturmak GK1 ve K2 kurallarını bozar. |
| **Karar** | Bileşenlerin code-behind sınıflarında hiçbir renk kodu ya da `Color` nesnesi yer almaz. Semantik durum bileşene bir enum veya metin (`SemanticType`: `Default`, `Positive`, `Negative`, `Warning`) olarak aktarılır. Bileşenin XAML katmanı `VisualStateManager` veya dinamik `DynamicResource` atamasıyla ilgili tema token'larını (`PositiveText`, `PositiveSurface`, `NegativeText`, `NegativeSurface`, `WarningText`, `WarningSurface`) temaya tam uyumlu şekilde uygular. |
| **Etkiler** | `ComparisonStrip`, `MetricRow`, `InfoBanner`, `SummaryCard` |
| **Durum** | uygulandı |

### GS12 — ListCard Satır Kısıtı ve Taşma (Overflow) Bildirimi

| | |
|---|---|
| **Tür** | kasıtlı sadeleştirme |
| **Sorun** | `docs/TASARIM-SISTEMI.md`: "ListCard sınırı 4 satır. Daha fazlası varsa kart '+7 daha' satırı gösterir ve detay sayfasına gider." kuralını koymuştur, ancak bileşenin bu taşmayı nasıl yansıtacağı açık değildir. |
| **Karar** | `ListCard`, `ItemsSource` ve `ItemTemplate` alırken, 4 satırdan fazla öğe bulunduğunda kartın altında otomatik veya bindable olarak beliren bir taşma satırı (`OverflowText` örn: "+4 daha" ve `OverflowCommand`) sunar. XAML liste sunumu 4 satırla sınırlandırılır; 5. satır yerini detay ekranına yönlendiren `ChevronRight` satırına bırakır. |
| **Etkiler** | `ListCard`, `EK-V3`, `EK-V6`, `EK-V7`, `EK-V9` |
| **Durum** | uygulandı |

### GS13 — Grafik Primitiflerinin Veri Sözleşmesi ve Çizim Zamanı Renk Güvenliği

| | |
|---|---|
| **Tür** | kasıtlı sadeleştirme / veri-kısıtı |
| **Sorun 1** | `StackedBar` ("≤ 4 kategori") ve `RingGauge` ("Tek oran 0–1") için `TASARIM-SISTEMI.md`'deki `ChartPoint(DateOnly Date, decimal Value)` modeli yetersizdir. Kategorik dağılım tarih taşımaz, kategori adı (`Label`/`Category`) gerektirir. |
| **Karar 1** | `Mizan.Presentation/Charts/` altına `ChartCategory(string Key, string Label, decimal Value)` record tipi eklenir. `RingGauge` ise girdisini doğrudan `decimal Ratio` (veya `RingGaugeValue`) olarak alır. |
| **Sorun 2** | `AreaTrend` tanımında "Tek seri + bir eşik çizgisi" yazmakta, ancak rol-token tablosunda hem `actual` (düz çizgi) hem `planned` (kesikli çizgi) yer almaktadır. |
| **Karar 2** | `AreaTrend`, ana seriyi (`actual`) zorunlu alırken, opsiyonel bir karşılaştırma serisi (`planned`) ve opsiyonel `ChartThreshold` (eşik çizgisi) destekler; böylece hem tekil trendi hem plan-gerçek seyrini çizebilir. |
| **Sorun 3** | `IDrawable` sınıfları MAUI yaşam döngüsünde renkleri `Application.Current.Resources`'tan okur. Headless birim testlerinde veya `Application.Current`'ın null olduğu durumlarda çizimin `NullReferenceException` fırlatmaması gerekir. |
| **Karar 3** | `ChartColorResolver` yardımcısı dinamik kaynakları sorgular; kaynak bulunamazsa sistem token'larının tema kontrast eşiğini geçen standart fallback değerlerine düşer. |
| **Etkiler** | `T4`, `EK-V8`, `EK-V9`, `EK-V10`, `EK-V12` |
| **Durum** | uygulandı |

### GS14 — Durum Blokları ve İskelet Deseni: Spinner Yasağı, StateBlock Sorumluluğu ve SkeletonBlock

| | |
|---|---|
| **Tür** | konsept-sapması / kasıtlı sadeleştirme |
| **Sorun 1 (Çelişki)** | `docs/TASARIM-SISTEMI.md` § Bileşenler tablosunda `StateBlock` "Boş / yükleniyor / hata, üç durum tek bileşen" olarak adlandırılmış, T3'te bileşen içine bir `ActivityIndicator` (spinner) konulmuştur. Ancak § Durumlar tablosunda "Yükleniyor: İskelet (kart şekli, SurfaceSunken). Spinner yok; yerleşim zıplamaz." kuralı yer almaktadır. Spinner içeren bir durum bloğu, ekran yerleşiminin zıplamasına neden olur ve tasarım sisteminin temel kuralını çiğner. |
| **Karar 1** | `StateBlock` içindeki `ActivityIndicator` tamamen kaldırılır. `StateBlock`, ekran ve kart düzeyinde **Boş (`Empty`)** ve **Hata (`Error`)** durumlarını karşılayan odaklı semantik mesaj + tek aksiyon bileşeni olarak sınırlandırılır. |
| **Sorun 2 (Eksiklik)** | İskelet yükleme deseni için sistemde tanımlı bir XAML yapı taşı bileşeni bulunmamaktadır. |
| **Karar 2** | `src/Mizan.App/Components/` altına `SkeletonBlock` bileşeni eklenir. `SurfaceSunken` zeminini, sistemin yarıçap token'larını (`RadiusCard`, `RadiusHero`, `RadiusChip`) ve `HeightRequest` değerlerini kullanarak sayfa şemalarındaki kartların ve satırların yerini tutar. § Hareket kuralları uyarınca harici shimmer kütüphaneleri (NuGet) eklenmez; saf token zemin kutusu ile yerleşim zıplaması önlenir. |
| **Sorun 3 (Presentation Sözleşmesi)** | Faz V ekranlarında ViewModel'lerin MAUI'den bağımsız olarak sayfa durumunu yönetebilmesi için saf bir duruma ihtiyaç vardır. |
| **Karar 3** | `src/Mizan.Presentation/Models/` altına `ScreenState` enum'ı (`Loading`, `Content`, `Empty`, `Error`) eklenir (K2). |
| **Etkiler** | `StateBlock`, `SkeletonBlock`, `ScreenState`, tüm V ekranları (`EK-V1` .. `EK-V13`) |
| **Durum** | uygulandı |

### GS15 — Görsel Bütçe ve Ekran Kartı Doğrulama Kuralları: GK4, GK5 ve GK9 Zorlama Mekanizması

| | |
|---|---|
| **Tür** | kasıtlı sadeleştirme / konsept-sapması |
| **Sorun 1 (GK9 Yaşam Döngüsü)** | GK9 "kartsız sayfa da, sayfası olmayan kart da kırmızıdır" der. Ancak T6 adımında henüz Faz V ekranları (`*Page.xaml`) oluşturulmamıştır; ayrıca `EK-V0` (kabuk) ve `EK-V2` (çocuk kart) `*Page.xaml` gerektirmeyen sayfasız istisnalardır. Tüm kartlar için koşulsuz sayfa dosyası aramak Faz T ve Faz V sırasında yanlış kırmızı üretir. |
| **Karar 1** | `EK-V0` ve `EK-V2` açık sayfasız istisna olarak tescil edilir. Sayfa → Kart yönü derhal ve katı zorlanır: diskteki her `*Page.xaml` mutlaka `EKRAN-KARTLARI.md`'de tanımlı bir `EK-*` kartına sahip olmalıdır (kartsız sayfa kırmızı). Kart → Sayfa yönü ise Faz V süresince `Bütçe özeti` tablosunda tamamlandı/aktif (`✅`) işaretlenen kartlar üzerinden, Faz V sonunda ise tüm 12 sayfa için zorunlu kılınır. |
| **Sorun 2 (GK4 `<Label>` Sayımı)** | GK4 tablosunda `<Label>` sınırı "28 - XAML'deki `<Label` sayısı" olarak geçer. `EK-V3` örneğinde bütçe "26 / 28" olarak hesaplanırken, sayfadaki bileşenlerin iç etiketlerinin toplam algısal yükü sayılmıştır; saf XAML düzeyinde ise bileşenler (`<components:SummaryCard/>`) tekil etiket içermez. |
| **Karar 2** | `DesignBudgetAnalyzer` iki katmanlı denetim uygular: (a) XAML dosyasındaki doğrudan `<Label>` ve `DataTemplate` içi `<Label>` sayısı ≤ 28 olmalıdır. (b) `docs/EKRAN-KARTLARI.md`'deki ilgili kartın Bütçe tablosunda beyan edilen toplam Label sayısı ≤ 28 olmalıdır. Böylece hem XAML şişmesi engellenir hem de ekran kartındaki toplam algısal yük 28 tavanına sadık kalır. |
| **Sorun 3 (GK5 Cümle Bütçesi)** | GK5 önek ve karakter sınırlarını tanımlamıştır, ancak henüz `src/Mizan.App/Resources/Strings` klasörü mevcut değildir (Faz V başında V0/V1 ile eklenecektir). |
| **Karar 3** | `DesignBudgetTests.Sayfa_CumleButcesiniAsamaz`, sentetik doğrulama kalkanıyla tüm önek kurallarını (`Etiket_` ≤ 24, `Cumle_` ≤ 90, `Aksiyon_` ≤ 28, `Bos_`/`Hata_` ≤ 90, sayfa başına `Cumle_` ≤ 3) test eder; `Resources/Strings` dizini ve XAML sayfaları diskte oluştuğu andan itibaren gerçek kaynakları otomatik olarak tarar; dizin henüz yoksa güvenli şekilde bekler. |
| **Etkiler** | `T6`, tüm V ekranları (`EK-V0` .. `EK-V13`), `DesignBudgetTests` |
| **Durum** | uygulandı |

---

## Ekran bazlı

### GS20 — Ana Sayfa: Kart Duvarı Yerine RingGauge Grafik Merkezi ve İkonlu Metrik Satırları

| | |
|---|---|
| **Tür** | kasıtlı sadeleştirme |
| **Konsept** | Konsept Panel 1 (Ana Sayfa) dikeyde sıralı ağır kart blokları (`SummaryCard`, `ComparisonStrip`, `ListCard`, `HeroInputCard`) barındırır; ekran bir "kart duvarı" gibi akar. |
| **Neden değiştirildi** | Sayfadaki kart sınırlarının ve iç içe kutuların yoğunluğu algısal yükü artırmakta, veriyi taramayı zorlaştırmaktadır. Kullanıcı dönemin durumunu tek bir bakışta hissetmek istemektedir. |
| **Yeni** | Kart sayısı azaltıldı (≤ 3 kart). Ortada tek bir görsel merkez olarak `RingGauge` (kalan bütçe oranı halkası) + yanındaki `TypeHero` dönem sonu rakamı konumlandırıldı. Kalan ödemeler ağır bir `ListCard` kutusu yerine temiz ikonlu satırlara (`MetricRow` deseni) dönüştürüldü. GK4 gereğince sayfa başına ≤ 1 grafik kuralı tam sağlandı (`RingGauge` 1/1). |
| **Kapı C kararları** | (1) `InfoBanner` konmadı: GK4 onu hero yüzey sayar ve `HeroInputCard` ile birlikte sınırı (1) aşar; dönem kapanışını "Dönemi Kapat" butonu gösterir. (2) Görsel merkezde "Planlanan: X" notu yerine "Kalan bütçe / Harcanan" ikilisi durur: halka kalan bütçe oranını gösterdiği için yanında kalan tutar okunur. |
| **Etkiler** | `EK-V3`, `DashboardPage.xaml`, `DashboardViewModel` |
| **Durum** | uygulandı |

### GS21 — Finansal Yapı: ListCard taşması detay sayfasına değil, grubun kendisine açılır

| | |
|---|---|
| **Tür** | kasıtlı sadeleştirme |
| **Konsept** | Konsept paneli yok (`GS2`). `GS12` ve `docs/TASARIM-SISTEMI.md`: `ListCard` en fazla 4 satır gösterir; fazlası "+N daha" satırıyla **detay sayfasına** gider. |
| **Neden değiştirildi** | Finansal Yapı'nın dört grubu (Gelirler, Kartlar, Krediler, Ödemeler) için gidilecek bir detay ekranı yok; "tüm krediler" diye ayrı bir sayfa, aynı satırları ikinci kez gösterirdi. Gerçek veride (iki telefon profili) grup başına en fazla 3 kayıt var; taşma nadirdir ve yalnız bu ekranın kendi listesidir. |
| **Yeni** | Grupta 4'ten fazla kayıt varsa ilk 4 satır ve "+N daha" satırı görünür; dokununca o grup yerinde bütün satırlarıyla açılır. `ListCard` bileşeni değişmez: taşma satırındaki `ChevronRight` kalır, komut ViewModel'deki genişletme komutudur. |
| **Etkiler** | `EK-V6`, `FinancialStructurePage.xaml`, `FinancialRecordGroup`; `GS12` (yalnız bu ekran için istisna) |
| **Durum** | uygulandı (`V6a`; taşma metni `FazlaKayitConverter` ile, gizli satır yoksa boş) |

### GS22 — GK4 grafik sınırı "aynı anda görünen" olur; kaydırılan hero kartı `SurfaceCard` üstünde durur

| | |
|---|---|
| **Tür** | kural değişikliği (sistem geneli; numara `GS20`+ aralığında çünkü `V3` yenilemesinden doğdu) |
| **Konsept** | `ana-sayfa-rota-tempo.png`: tek kartta iki sayfa. 1. sayfa bakiye trendi (`AreaTrend`), 2. sayfa tempo halkası (`RingGauge`). Kullanıcı kararı, 2026-09-29: açılışta grafik, sağa kaydırınca halka. |
| **Neden değiştirildi** | `GK4` "sayfada 1 grafik" diyordu ve `DesignBudgetAnalyzer` XAML'deki her grafik örneğini sayıyordu. Kural gözün aynı anda taşıdığı yükü korumak için var; kaydırılan kartın gizli sayfası o yükü artırmıyor. Kural metni de analizciden dardı (`ChartCard` diyordu, analizci beş etiketi sayıyordu). |
| **Yeni** | (1) Ekranda **aynı anda** en fazla 1 grafik görünür: `HeroPager` dışındaki grafikler + pager başına 1. (2) Sayfada en fazla 1 `HeroPager`, pager başına en fazla 2 `HeroPage`, `HeroPage` başına en fazla 1 grafik. (3) `HeroPager` 1 kart sayılır; sayfaları ayrıca kart sayılmaz. (4) Kart bütçesinde `Grafik  N / 1` aynı anda görünen sayıdır, `Hero sayfa  N / 2` yeni satırdır. (5) Hero rakam 1 kalır: halkanın ortasındaki tutar `TypeTitle`. (6) Kaydırılan kartın ve "Bakiye gir" önizleme kartının zemini `SurfaceCard`: `SurfaceHero` koyu temada `Indicator` (2,92), `NegativeText` (4,18), `TextSecondary` (4,05) ve `PositiveText` (4,25) çiftlerini eşiğin altına düşürüyor, `SurfaceCard`'da hepsi geçiyor. Token değişmez; `GS8` etkilenmez. |
| **Etkiler** | `06-tasarim.md` GK4, `TASARIM-SISTEMI.md` § Grafikler, `EKRAN-KARTLARI.md` bütçe biçimi, `DesignBudgetAnalyzer`, `ScreenCardDocument`, `I57`; `GS20`'yi `V3` iptal edecek; `HeroPager` bileşeni `V3`'te yazılır |
| **Durum** | uygulandı (`T9`) |

### GS23 — `AreaTrend` tarih eksenli olur ve rota çizer; `RingGauge` tempo işareti taşır

| | |
|---|---|
| **Tür** | primitif genişlemesi (GK7: beşinci primitif yok; kaynak eski proje değil, bugünkü kod) |
| **Konsept** | `ana-sayfa-rota-tempo.png`: 1. sayfada gerçek noktalar düz çizgi ve alan, son gözlemden dönem sonu tahminine kesikli devam, "bugün" işareti, planın dönem sonu; 2. sayfada halka, doluluğun yanında geçen süreyi de gösteriyor. |
| **Neden değiştirildi** | Bugünkü `AreaTrend` x eksenini sıra numarasından kuruyor, tarih bilmiyor; "bugün" işareti ve dönem sonuna uzanan devam çizilemiyor. `RingGauge` yalnız doluluğu söylüyor; harcamanın süreye göre hızlı mı yavaş mı olduğu (tempo) halkada görünmüyor. |
| **Yeni** | (1) `AreaTrend` x eksenini **hep tarihe göre** kurar; eksen ilk ve son noktanın (kesikli devam dahil) tarih aralığıdır. 12 dönem kullanımı (`V8`) aynı alanlarla çalışır, yalnız aylar 28–31 gün olduğu için aralıklar çok az eşitsizleşir. (2) İsteğe bağlı üç yeni alan: `ProjectionSeries` (son gözlemden sonraki kesikli devam), `Today` (dikey hairline), `PlanLevel` (planın dönem sonu için yatay hairline, `ChartThreshold` tipiyle). Mevcut `Series`, `PlannedSeries`, `Threshold` aynen kalır. (3) `RingGauge` isteğe bağlı `TimeRatio` (0–1) alır: halkayı kesen kısa bir radyal işaret çizer. (4) Yeni token yok. Roller: tahmin devamı = `Indicator` kesikli, bugün = `TextSecondary` düz hairline, plan seviyesi = `TextSecondary` hairline, tempo işareti = `TextSecondary`. Hepsi `SurfaceCard` üstünde `I93` ile doğrulanmış çiftler. (5) Çizgi noktalar arasında ve sonrasında nasıl gideceği (`S68-6`) primitifin değil verinin işidir: primitif verilen noktaları bağlar. (6) `<summary>` soruları: `AreaTrend` "Bakiye nereye gidiyor, plana ve eşiğe göre neredeyim?", `RingGauge` "Ne kadarı tamamlandı, geçen süreye göre önde miyiz geride mi?". (7) `AreaTrend` 200 satırı aşmasın diye x/y eşleme `ChartScale` (`internal`) yardımcısına çıkar; GK7 primitif sayısını sınırlar, yardımcıyı değil. (8) `StackedBar` değişmez: iki satır kararı `V11`'in Aşama 4'ünde verilir. |
| **Etkiler** | `TASARIM-SISTEMI.md` § Grafikler (soru tablosu, rol → token), `DesignChartTests`, `V3` (kullanır), `V8` (etkilenmez), `V11` (StackedBar kararı) |
| **Durum** | uygulandı (`T10`) |

---

## Konsept otorite değildir

Taşıma protokolündeki "eski kod otorite değildir" ilkesinin görsel karşılığı:

Konsept tasarım, aksi kanıtlanana kadar doğru varsayılır, **doğru olduğu için değil.**
Konseptin bir paneli, veri modelinin veremediği bir şey gösteriyorsa (ya da kullanıcıya
gereksiz bir şey gösteriyorsa) yol konsepte uymak değil, `GS` kaydı yazmaktır.

Özellikle şu dört durumda konsepte uyma:

1. **Konsept bir sayı gösteriyor ama o sayı yok.** Konsept panelleri üretilirken veri
   modeline bakılmadı; "Deterministik Projeksiyon" gibi bir etiketin arkasında gerçek bir
   hesap olduğunu varsayma. `docs/TASIMA-PLANI.md`'de karşılığı taşınmış mı diye bak.
2. **Konsept aynı bilgiyi iki yerde gösteriyor.** Mockup'ta güzel durur, ekranda bütçe yer.
3. **Konsept bir ekranı kalabalıklaştırıyor.** GK4 konseptten üstündür: bütçe ölçülmüş bir
   problemden geliyor, konsept panelleri ise tek bir bakış için üretilmiş.
4. **Görsel bir pazarlama kompozisyonu.** Planör tanıtım görselindeki telefon maketleri
   ekran konsepti değildir: "5 Yıllık Projeksiyon" diyor ama motor **12 dönem** hesaplıyor;
   ekran başlığı "Mizanpaj"; puan ve indirme sayıları yer tutucu. Oradan yalnız marka
   alınır (işaret, renk, ton). Yerleşim, veri ya da metin alınmaz.
