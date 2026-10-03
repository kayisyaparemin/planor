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
| **Durum** | **iptal** → `GS24`. "Rota + Tempo" yenilemesi (`V3a`): halka kaydırılan kartın ikinci sayfasına geçti, görsel merkezin grafiği bakiye rotası oldu, `HeroInputCard` ve gezinme satırları kalktı. |

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
| **Durum** | uygulandı (`T9`); (6)'nın kaydırılan kart yarısı `GS24` ile, "Bakiye gir" önizlemesi yarısı `GS25` ile değişti: ikisi de tonlu `SurfaceChart` |

### GS23 — `AreaTrend` tarih eksenli olur ve rota çizer; `RingGauge` tempo işareti taşır

| | |
|---|---|
| **Tür** | primitif genişlemesi (GK7: beşinci primitif yok; kaynak eski proje değil, bugünkü kod) |
| **Konsept** | `ana-sayfa-rota-tempo.png`: 1. sayfada gerçek noktalar düz çizgi ve alan, son gözlemden dönem sonu tahminine kesikli devam, "bugün" işareti, planın dönem sonu; 2. sayfada halka, doluluğun yanında geçen süreyi de gösteriyor. |
| **Neden değiştirildi** | Bugünkü `AreaTrend` x eksenini sıra numarasından kuruyor, tarih bilmiyor; "bugün" işareti ve dönem sonuna uzanan devam çizilemiyor. `RingGauge` yalnız doluluğu söylüyor; harcamanın süreye göre hızlı mı yavaş mı olduğu (tempo) halkada görünmüyor. |
| **Yeni** | (1) `AreaTrend` x eksenini **hep tarihe göre** kurar; eksen ilk ve son noktanın (kesikli devam dahil) tarih aralığıdır. 12 dönem kullanımı (`V8`) aynı alanlarla çalışır, yalnız aylar 28–31 gün olduğu için aralıklar çok az eşitsizleşir. (2) İsteğe bağlı üç yeni alan: `ProjectionSeries` (son gözlemden sonraki kesikli devam), `Today` (dikey hairline), `PlanLevel` (planın dönem sonu için yatay hairline, `ChartThreshold` tipiyle). Mevcut `Series`, `PlannedSeries`, `Threshold` aynen kalır. (3) `RingGauge` isteğe bağlı `TimeRatio` (0–1) alır: halkayı kesen kısa bir radyal işaret çizer. (4) Yeni token yok. Roller: tahmin devamı = `Indicator` kesikli, bugün = `TextSecondary` düz hairline, plan seviyesi = `TextSecondary` hairline, tempo işareti = `TextSecondary`. Hepsi `SurfaceCard` üstünde `I93` ile doğrulanmış çiftler. (5) Çizgi noktalar arasında ve sonrasında nasıl gideceği (`S68-6`) primitifin değil verinin işidir: primitif verilen noktaları bağlar. (6) `<summary>` soruları: `AreaTrend` "Bakiye nereye gidiyor, plana ve eşiğe göre neredeyim?", `RingGauge` "Ne kadarı tamamlandı, geçen süreye göre önde miyiz geride mi?". (7) `AreaTrend` 200 satırı aşmasın diye x/y eşleme `ChartScale` (`internal`) yardımcısına çıkar; GK7 primitif sayısını sınırlar, yardımcıyı değil. (8) `StackedBar` değişmez: iki satır kararı `V11`'in Aşama 4'ünde verilir. |
| **Etkiler** | `TASARIM-SISTEMI.md` § Grafikler (soru tablosu, rol → token), `DesignChartTests`, `V3` (kullanır), `V8` (etkilenmez), `V11` (StackedBar kararı) |
| **Durum** | uygulandı (`T10`); `V3a` Kapı C'de genişledi (`GS24`): bakiye noktaları, dönem sonu halkası, kesikli bugün çizgisi, dolgu ve halka izi göstergenin saydam tonu; `RingGauge`'un açı hatası düzeltildi |

### GS24 — Ana Sayfa "Rota + Tempo": kaydırılan hero, kapanış bakiye kartının yerinde, grafikte yazı ve işaret yok

| | |
|---|---|
| **Tür** | konsept-sapması (yerleşim konseptten; `GS20`'nin yerine geçer) |
| **Konsept** | `ana-sayfa-rota-tempo.png`, `ana-sayfa-rota-tempo-durumlar.png`, `ana-sayfa-rota-tempo-kapanis.png` (halka sayfasının bakiyesiz hâli). Başlıkta dönem aralığı ve gün sayacı; kaydırılan kartın 1. sayfasında dönem sonu tahmini, plana göre fark, plan ve rota grafiği ("Plan" ve "Bugün" yazılarıyla, gözlem noktaları yuvarlak işaretli); 2. sayfada halka, ortasında kalan yaşam gideri ve "10 gün kaldı", altında harcanan / geçen süre ve tempo cümlesi; bankadaki bakiye kartı; hatırlatıcı; kalan ödemeler (2 satır, "Tümünü gör"). Kapanış ertelenmişken ayrı bir kapanış kartı ve bakiye kartı birlikte. Bakiye girilmemişken grafik yerine düz plan çizgisi ve "İlk bakiyeyle gidişat çizilir." Boş hâlde kesikli grafik yer tutucusu. |
| **Neden değiştirildi** | Konsept tek bakış için üretildi, veriye ve bütçeye bakılmadı: (a) kapanış kartı + bakiye kartı + hatırlatıcı + kalan ödemeler + kaydırılan kart 5 kart eder (sınır 4); üstelik biten döneme bakiye yazılamaz (`S68-4`), yani konseptteki "Bakiye gir" o hâlde çıkmaz yol. (b) `AreaTrend` yazı çizmez ve gözlem işareti çizmez (`GS23`); konumu değişen bir "Bugün" etiketi XAML'de hesaplanamaz. (c) "10 gün kaldı" başlıktaki gün sayacıyla aynı bilgi (konsept aynı bilgiyi iki yerde gösteriyor). (d) Bakiye girilmemişken planın gerçek rotası artık var (`S71-6`); yer tutucu çizgi ve cümle ondan az şey söylüyor ve bir cümle harcıyor. (e) Satır başına ikon GK6'ya aykırı. |
| **Yeni** | (1) Kaydırılan kart `HeroPager`: iki `HeroPage`, zemin `SurfaceCard` (`GS22`), yatay kaydırma **ve** dokunulabilir iki nokta ile geçiş, **animasyonsuz** (`TASARIM-SISTEMI.md` § Hareket yalnız üç yerde animasyona izin verir). Her yüklemede 1. sayfa. `CarouselView` kullanılmaz: dikey `ScrollView` içinde kaydırma çakışması, yükseklik ölçümü ve kaydırma animasyonu. (2) Kapanış ertelenmişken bakiye kartının yerinde "Dönem bitti" + dönemin son günü + "Dönemi kapat"; kapanış kartı ayrı değil, cümlesi yok (`S72-6`). (3) Grafikte "Bugün" ve "Plan" yazıları yok, çizgileri var (`Today`, `PlanLevel`); gözlem noktası işareti yok, çizgi gözlem gününde kırılır. Grafiğin altında yalnız dönemin ilk ve son günü (`S72-7`). (4) Halkada "10 gün kaldı" yok. (5) Bakiye girilmemişken grafik planın rotasını kesikli çizer, hero "Planlanan dönem sonu" der; "İlk bakiyeyle gidişat çizilir" cümlesi yok. Halka sayfası: halka boş, ortada "Bakiye girilmedi", tek cümle "Harcama temposu ilk bakiye girişiyle hesaplanır." (6) Kalan ödemeler `ListCard`: en fazla 3 satır (konsept 2; önceki `EK-V3` kararı 3), satırda ikon yok, "Tümünü gör" yalnız gizli satır varsa. (7) Boş hâl ortak `StateBlock` (`T5`); konseptin kesikli yer tutucusu yok. Hata metni genel kalır: hata yalnız veritabanından değil hesaptan da gelebilir. (8) Gezinme satırları ve ayarlar ikonu yok; aynı hedefler yan menüde. |
| **Kapı C kararları** | Kullanıcı ilk emülatör bakışında ekranı konsepte göre "çok çirkin" buldu ve kaydırmanın zor olduğunu söyledi (2026-09-30). (a) **Halka hatası:** MAUI açıyı saat 3'ten saatin tersine sayar; `RingGauge` tepeyi -90 (saat 6) sanıp ters çiziyordu, işaret %47'de saat 12'deydi. Tepe 90°, saat yönü açıyı azaltır; halka 6 → 10 kalınlığında. (b) **Tonlu zemin:** kaydırılan kart `SurfaceChart` zemininde, köşe `RadiusHero` (konseptteki gibi diğer kartlardan ayrılır). `SurfaceHero` koyu temada metni taşımıyordu (`GS22`); `SurfaceChart`'ta beş metin/çizgi çifti eşiği geçer ve tabloya girdi (`I93`). Dolgu ve halka izi artık ayrı bir zemin token'ı değil, `Indicator`'ın %20 saydam tonu: `SurfaceChart` dolgusu aynı renkli kartta kaybolurdu. (c) **Grafik:** bakiye girilen günlerde dolu nokta, kesikli devamın sonunda içi boş halka, bugün çizgisi kesikli (düz hâli rotayı bölen bir kenar gibi okunuyordu). "Bugün" / "Plan" yazıları yine yok. (d) **Kaydırma:** Android dikey kaydırması parmağı çalıyordu; MAUI kaydırma tanıyıcısının yerine `HeroPagerSwipeListener` yatay hareketi sahiplenir, eşik 100 → 48 dp. Sayfa parmağı takip etmez (animasyonsuz kalır). (e) **Noktalar:** etkin sayfa uzun çubuk (`Indicator`), diğeri nokta (`TextMuted`, pasif öğe). (f) **Başlık:** sağdaki gün sayacı araç çubuğunun kenarında kesiliyordu → `TitleBarPadding`. (g) Halkanın ortasındaki etiket küçük harf `Caption`, tutar `SemiBold`. Değişmeyen: grafiğin solundaki dik yükseliş gerçek veridir (dönemin ilk günü yatan gelir ertesi günün noktasında, `S71`); açık temada kart ile sayfa zemininin yakınlığı ve BÜYÜK HARF etiketler bütün ekranların paletinden ve stilinden gelir, bu adımda değişmedi. |
| **Etkiler** | `EK-V3`, `DashboardPage.xaml`, `HeroPager` (yeni bileşen, `DesignComponentTests`), `DashboardViewModel`; `GS20` iptal; `S72` (davranış tarafı); Kapı C: `RingGauge`, `AreaTrend`, `ChartColorResolver`, `GS22` (6), `GS23`, `I93`, `TASARIM-SISTEMI.md` (kontrast çiftleri, rol → token, `ChartHeight`, `TitleBarPadding`) |
| **Durum** | uygulandı (`V3a`, Kapı C düzeltmeleriyle; `I93`, `I110`) |

### GS25 — "Bakiye gir": önizleme ana sayfa kartının tonunda, ok ve takvim ikonu yok, "güncel" yok

| | |
|---|---|
| **Tür** | konsept-sapması (yerleşim konseptten; `GS22` (6)'nın "Bakiye gir" yarısını değiştirir) |
| **Konsept** | `ana-sayfa-rota-tempo-durumlar.png`, "Bakiye gir · Açık" paneli: başlıkta ✕ + "Bakiye gir"; "Bankadaki güncel bakiye", büyük tutar alanı ve sağında ₺; "Son giriş 58.940 ₺ · 27 Eylül"; takvim ikonlu "29 Eylül · bugün" satırı ve "Değiştir"; tonlu önizleme kartı: "Bu girişle dönem sonu tahmini", "41.723 ₺ → 44.380 ₺", "Plana göre +480 ₺", kısa rota grafiği; "Kaydet"; altta açık sayı klavyesi. |
| **Neden değiştirildi** | (a) Ok GK6'ya göre bir ikondur ve `Icons.cs`'te ileri ok yok; metne gömülemez. (b) Takvim ikonu da yok; "Değiştir"in seçiciyi açması code-behind (`Focus()`) ya da yeni bir bileşen ister (kural `03`). (c) "güncel" geriye tarihli girişte yanlış. (d) Klavyenin kendiliğinden açılması code-behind ister. (e) Kısa grafik yeni bir ölçü token'ı ister. (f) `GS22` (6) önizlemeyi `SurfaceCard`'a koymuştu; konsept ve ana sayfanın kaydırılan kartı (`GS24` b) tonlu `SurfaceChart`'ta. Aynı rakam ve aynı grafik iki ekranda iki zeminde durursa akraba oldukları okunmaz; açık temada `SurfaceCard` sayfa zeminine de yakın. |
| **Yeni** | (1) Önizleme kartı ana sayfanın kaydırılan kartıyla aynı: zemin `SurfaceChart`, kenar `BorderSubtle`, köşe `RadiusHero` (kullanıcı kararı, 2026-09-30). Grafik ve metin çiftleri bu zeminde zaten doğrulandı (`I93`); token değişmez. (2) Ok yerine kartta "Önceki tahmin" satırı, yalnız daha önce bakiye girildiyse (`S73-6`). (3) Tarih satırı diğer formların tarih seçicisi (`SurfaceSunken`), seçili gün bugünse yanında "Bugün"; takvim ikonu ve "Değiştir" yok (kullanıcı kararı). (4) Eyebrow "BANKADAKİ BAKİYE" (ana sayfanın bakiye kartıyla aynı söz). (5) Klavye kendiliğinden açılmaz; kullanıcı tutar alanına dokunur. (6) Grafik `ChartHeight`; tarih etiketleri yok. (7) Önizleme kartı yalnız geçerli tutar varken görünür. (8) Plan tutarı kartta yok; yalnız "Plana göre" farkı. |
| **Etkiler** | `EK-V3` (ikinci sayfa), `BalanceEntryPage.xaml`, `06-tasarim.md` GK4 zemin cümlesi, `GS22` (6) |
| **Durum** | uygulandı (`V3b`; bütçe hero 1/1, kart 1/4, grafik 1/1, label 11/28, cümle 1/3) |

### GS26 — 12 dönem: konseptin karo ızgarası, faiz gidişat kartında, dolgu sıfıra; yazı tipi, ikon ve boş hâl sistemden

| | |
|---|---|
| **Tür** | konsept-sapması (yerleşim konseptten) |
| **Konsept** | Claude Design, "Planör · 12 Dönem" (`docs/assets/konsept/12-donem-*.png`: koyu ve açık dolu, eksiye düşmeyen veri, yükleniyor, boş, hata, erken kapamanın üç hâli). Başlıkta geri oku + "12 Dönem". Tonlu kart: "EN DÜŞÜK DÖNEM SONU", hero rakam, "10 Mart 2027 dönemi sonunda", alan grafiği (dolgu sıfır çizgisine iner, sıfır gri kesikli), altında iki uç tarih, üst çizgili iki satır: "12 dönem sonra", "12 dönemde faiz". "DÖNEM SONLARI": 1 px ayırıcılı 3 × 4 karo; ay adı (yıl ilk karoda ve Ocak'ta), tutar 16 pt, en düşük kalın, eksiler kırmızı, karo dokunulabilir. "ERKEN KAPAMA": satır başına ad (16) + durum (14; "18 Eyl 2027 · 21.400 ₺ ile kapat", "Kapatmak açık oluşturur", "Kalan anapara gerekli", "Kapama planlı · tarih", "Kapatmak kazandırmıyor") + sağda "net +2.950 ₺" (yeşil) + "›"; satır arası çizgi. Yazı tipi IBM Plex Sans. Yükleniyor: ekranın şeklini taklit eden iskelet. Boş: takvim ikonu, "Dönemler gelir gününden hesaplanır; önce gelir gününü belirle.", "Gelir gününü belirle". Hata: üçgen ikon, "12 dönem şu an hesaplanamadı.", "Tekrar dene". |
| **Neden değiştirildi** | (a) Tasarım sistemi yeni yazı tipi eklemez (OpenSans). (b) Sayfa yan menüden açılır; başlıkta kabuğun menü düğmesi durur, geri oku değil. (c) Rol tablosunda eşik çizgisi `NegativeText`; gri sıfır çizgisi tabloda yok. (d) "›" bir karakter değil ikondur (GK6). (e) Takvim ve uyarı ikonları envanterde yok. (f) 16 / 14 satır puntosu skalada satır karşılığı değil (16 = `TypeFigure` SemiBold); diğer listeler 14 / 12. (g) Konseptin boş hâli veriye bakmadan kurulmuş: v2'de gelir günü kurulumda sorulur, eksik kalamaz; ekranın boş kalma sebebi açık dönemin olmaması ya da planın projeksiyon kuramaması (gelir ve bakiye yok, `S74`-2). (h) Diğer ekranların iskeleti `SkeletonBlock`. (i) `GS12`: `ListCard` en fazla 4 satır; ızgara 12 dönemin hepsini gösterir ve gidilecek bir "tüm dönemler" sayfası yok. (j) `AreaTrend` dolguyu tabana indiriyor; eksi dönemler dolgudan ayrışmıyor. |
| **Yeni** | (1) **Dönem sonları 3 × 4 karo ızgara:** ham `Border` (`SurfaceCard`, `RadiusCard`) içinde `Grid` + `BindableLayout`, satır ve sütun dönemin sırasından; 1 px ayırıcı = ızgara aralığı `StrokeHairline` + zemin `BorderSubtle`, karo zemini `SurfaceCard`. Ay adı `Caption` / `TextSecondary` (yıl ilk karoda ve Ocak'ta), tutar `TypeFigure`: en düşük SemiBold, diğerleri Regular; eksi `NegativeText`. 12 karonun hepsi; `GS12`'nin 4 satır sınırı bu ızgaraya uygulanmaz. Karoya dokunma `V9`'da. Yeni bileşen yok. (2) **Gidişat kartı** V3b önizleme kartının deseninde: `SurfaceChart`, `BorderSubtle`, `RadiusHero`; token değişmez, çiftler `I93`'te. Hero `TextPrimary` (eksi de olsa), açıklama `TextSecondary`. Altında iki `MetricRow`: "12 dönem sonra" (eksiyse `Negative`) ve "12 dönemde faiz" (toplam; 0 ₺ dahil hep görünür; kırılım `V9`). Satır üst çizgileri alınmadı (`MetricRow` olduğu gibi). (3) **Sıfır eşiği her zaman** çizilir ve ölçeğe girer: sıfıra uzaklık güvenlik payıdır. `ChartTrend`'e isteğe bağlı `Threshold` (varsayılan `null`, `V3` değişmez), `TrendGrafigiConverter`'da tek atama. Çizgi rol tablosundaki gibi `NegativeText` kesikli. (4) **Dolgu eşiğe iner:** `AreaTrend` eşik verildiyse alan dolgusunu tabana değil eşiğe indirir; eksi dönemler sıfırın altında ayrı bir cep olur. Eşik yoksa eskisi gibi tabana (`V3` değişmez). (5) **Boş hâl** `StateBlock`: `Schedule` + "12 dönem için…" metni + "Finansal Yapı'ya git"; **hata** `StateBlock`: `Close` + konseptin metni + "Tekrar dene". (6) **Erken kapama** `ListCard` (satır arası çizgi yok): ad `TypeBody`, durum `Caption` (konseptin metinleri; tarih kısa ay adıyla), sağda "net" `Caption` + tutar `Figure` / `PositiveText` (iki `Label`; ikisi de bütçede sayılır), `Icons.ChevronRight`. Ufukta denenecek taksiti olan her kredi bir satırdır, sıra Finansal Yapı'daki kredi sırası; `GS12`'nin 4 satır sınırı bu karta da uygulanmaz (gidilecek bir "tüm krediler" sayfası yok, gerçek veride kredi sayısı 3'ü geçmiyor; kullanıcı kararı, 2026-10-03). (7) Yükleniyor: iki `SkeletonBlock` (gidişat kartı, ızgara). (8) Yazı tipi OpenSans, başlık kabuk başlığı. |
| **Etkiler** | `EK-V8`, `FuturePeriodsPage.xaml`, `ChartTrend`, `TrendGrafigiConverter`, `AreaTrend` / `ChartScale` (dolgunun tabanı), `TASARIM-SISTEMI.md` § Grafikler (`AreaTrend` girdisi), `GS12` (bu ızgara ve erken kapama kartı için istisna) |
| **Durum** | uygulandı (`V8a`: 1–5, 7, 8; `V8b`: 6; bütçe hero 1/1, kart 3/4, grafik 1/1, label 15/28, cümle 0/3) |

### GS27 — Dönem ayrıntısı: konseptin kıyas şeridi ve uyarı bandı yok, yerleşim 12 dönemin gidişat kartından

| | |
|---|---|
| **Tür** | konsept-sapması / veri-kısıtı |
| **Konsept** | Yerleşim konseptinin "Dönem Ayrıntısı" paneli (`EK-V9` taslağı): `InfoBanner` + `ComparisonStrip` + kategori satırları. Panelin görüntüsü iki repoda da yok; elde yalnız bu tarif var. |
| **Neden değiştirildi** | (a) `ComparisonStrip` plan / gerçek / fark üçlüsüdür; gelecekteki bir dönemde gerçek yoktur. Baz ↔ senaryo kıyası simülatörün sorusudur (`V10`, `S75`-7). (b) `InfoBanner` eski ekranın gelir karşılama cümlelerinin yeridir ("Bu ay dönem gelirlerin ihtiyacın … altında kalıyor"); GK5: cümle bir sayı olmalı — hero'nun altındaki "dönem başına göre" satırı aynı cevabı işaretli tutar olarak verir. (c) Kategori satırları (Krediler, Kartlar, …) ödeme listesinin tekrarı (`S75`-3). (d) Sayfa 12 dönem karosundan açılır; kullanıcı karonun büyümüş hâlini görmeli. |
| **Yeni** | (1) **Akış kartı** 12 dönem gidişat kartının deseninde: ham `Border`, `SurfaceChart` / `BorderSubtle` / `RadiusHero`; `SurfaceHero` değil (eksi rengi `SurfaceHero`'da koyu temada eşiği geçmiyor, `T9`). Eyebrow, hero dönem sonu (`TextPrimary`, eksi de olsa), "dönem başına göre" satırı (işaretli, `NegativeText` / `PositiveText`), dönemin ilk–son günü, altında akışın altı `MetricRow`'u. Grafik yok: `StackedBar` altı satırın söylediğini ikinci kez söylerdi. (2) **Ödemeler** `ListCard`: ad `TypeBody`, gün `Caption` (tahminiyse "· tahmini"), tutar `Figure`; 4 satır + "+N daha" yerinde açılır (`GS21`); kart satırında `ChevronRight`, diğer satırlar dokunulmaz. (3) **Kart faizi** `ListCard`, yalnız faiz varsa; satır kart adı + tutar, toplam notta. (4) Başlık karonun dönem adı ("Ekim 2026"), kabuğun geri okuyla. (5) Ana sayfanın kalan ödemeler kartında "Tümünü Gör" yerine "+N daha" ve yerinde açılma (`S75`-9); kart sayısı ve etiket bütçesi değişmez. Yeni bileşen yok. |
| **Etkiler** | `EK-V9`, `PeriodDetailPage.xaml`, `EK-V3` (5), `DashboardPage.xaml` (5), `GS21` (iki yeni kullanım) |
| **Durum** | uygulandı (`V9`; bütçe hero 1/1, kart 3/4, grafik 0/1, label 12/28, cümle 0/3) |

### GS28 — Simülatör: konseptin dönem kartları yok, yerleşim 12 dönemden; grafikte iki çizgi

| | |
|---|---|
| **Tür** | konsept-sapması / kasıtlı sadeleştirme |
| **Konsept** | Yerleşim konseptinin "Simülatör" paneli (5 panelli konsept, `EK-V10` taslağı): dönem başına `MetricRow` + "Detay Gör". Panelin görüntüsü repoda yok. Eski ekran: 11 kart + 12 dönem kart şablonu, 64 `<Label>`. |
| **Neden değiştirildi** | (a) Dönem başına satır ve "Detay Gör", 12 kart × birkaç satır demek: GK4'ü aşar ve "en çok nerede sıkışırım?" sorusuna tek bakışta cevap vermez; 12 Dönem aynı soruyu gidişat kartı + karo ızgarasıyla çözdü (`GS26`). (b) Simülatörün asıl cevabı iki zincirin farkıdır; tek seri farkı göstermez. (c) Kullanıcı kararı (2026-10-03): "12 dönemin aynısı olsun: grafik + 12'li ızgara". (d) Dönem ayrıntısı denemeyi bilmez (`S76`-9); "Detay Gör" taşınmaz. |
| **Yeni** | (1) **Sonuç kartı** 12 Dönem'in gidişat kartının deseninde (`SurfaceChart`, `BorderSubtle`, `RadiusHero`): eyebrow, hero denemeyle en düşük dönem sonu (`TextPrimary`, eksi de olsa), şu anki gidişata göre fark (işaretli; `NegativeText` / `PositiveText`), dönem, `AreaTrend`, iki uç tarih. "12 dönem sonra" ve "12 dönemde faiz" satırları 12 Dönem'in satır diliyle (`MetricRow` + açık deneme varken altında `Caption` stilinde sol baş "Şu an ₺...", sağ baş renkli fark tutarı; `MetricRow`'un `Auto, *, Auto` ve `Space2` kolonlarıyla milimetrik hizalı; deneme yoksa alt satırlar gizlenir, `ComparisonStrip` kutusu konsept uyumsuzluğu nedeniyle elendi). (2) **`AreaTrend`'de karşılaştırma serisi:** `ChartTrend`'e isteğe bağlı ikinci seri (varsayılan `null`; `V3` ve `V8` değişmez). Şu anki gidişat `planned` rolüyle çizilir (`TextSecondary`, kesikli, dolgusuz); denemeyle seri düz `Indicator`, dolgu sıfır eşiğine iner (`GS26`-4). Açık deneme yoksa ikinci seri yoktur ve grafik 12 Dönem'inkiyle aynıdır. Yeni primitif yok (GK7). (3) **Dönem sonları** 12 Dönem'in 3 × 4 ızgarası (`GS26`-1), denemeyle rakamlar; karo dokunulmaz. (4) **Denemeler** `ListCard`'ı: ad, tür · tarih, tutar, aç/kapa; kapalı ya da geçersiz satır soluk. (5) **Koşul formu** V6 formlarının deseninde. Bloklar, token'lar ve aç/kapa denetimi `EK-V10` Aşama 5'te. |
| **Etkiler** | `EK-V10`, `SimulatorPage.xaml`, `SimulationConditionPage.xaml`, `ChartTrend`, `TrendGrafigiConverter`, `AreaTrend`, `TASARIM-SISTEMI.md` § Rol → token eşlemesi (karşılaştırma serisi `planned`), `GS26` (ızgaranın ikinci kullanımı), `Styles.xaml` (`Switch` örtük stili) |
| **Durum** | uygulandı (`V10a`: 4 ve 5'in nakit ödeme alanları; aç/kapa MAUI `Switch`, açık hâl `Indicator`, Kapı B; `V10b1`: 1'in hero ve farkı, 2 — sonuç kartı, ikinci seri `ChartTrend.Comparison`, grafik kurucusu `ProjectionTrend`; `V10b2`: 1'in 12 dönem sonra ve faiz satırları 12 Dönem'in satır diliyle, 3 — 3 × 4 karo ızgarası; Kapı C onaylı 2026-10-04). |

### GS29 — Tür seçici: konseptin karo ızgarası; gruplar Finansal Yapı listesinden, grup rengi semantik token'lardan, karo başına bir ikon

| | |
|---|---|
| **Tür** | konsept uygulaması + yeni bileşen (kaynak eski projenin `EntryTypePickerView`'ü; konsept `docs/assets/konsept/ekleme-ekranı-acik.png`, `ekleme-ekranı-koyu.png`, kullanıcı getirdi 2026-10-03) |
| **Konsept** | "Ne eklemek istiyorsun?" başlığı ve altında tek cümle; dört grup (GELİR, KART, KREDİ, ÖDEME), her başlığın solunda grubun renginde nokta; her grubun altında iki sütunlu karolar: grubun yüzeyinde renkli ikon karesi, kalın başlık, gri alt satır. KART ve KREDİ tek karoyla yan yana, ÖDEME 2 × 2. Gelir yeşil, Kart mavi, Kredi sarı, Ödeme kırmızı. |
| **Neden değiştirildi** | Önce iki yerleşim denendi ve Kapı C'de geri döndü: (a) eski düzen — yan yana grup şeridi + seçili grubun ≤ 3 seçeneği: "Hesap" ve "Borç / Kredi" adları kullanıcıya bir şey söylemiyordu, grupta 1–3 seçenek olduğu için ekranın altı boş kalıyordu; (b) bütün gruplar ikonsuz karolarla: kullanıcı Design'dan konsept getirdi. Konseptten sapanlar: (c) başlık altı cümle ("Planına ekleyeceğin kaydın türünü seç.") başlığın sorusunu tekrar ediyor ve kodun iç kavramını ("plan") taşıyor (V6a'da "Planına girenler" de aynı sebeple çıktı) — alınmadı (kullanıcı kararı). (d) Planör'de kategori rengi yok; konseptin dört rengi var olan semantik token'lara düşer. Bu ekranda renk "olumlu / olumsuz" değil grubu söyler (kullanıcı kararı). (e) Karo köşesi `RadiusCard` (8); konseptinki biraz daha yuvarlak, sistemde karo yarıçapı yok. |
| **Yeni** | (1) **Gruplar Finansal Yapı listesinin grupları:** GELİR · KART · KREDİ · ÖDEME, listenin sırasıyla; eklenen kayıt listede aynı adlı grupta görünür (`S77`-3). (2) **Grup rengi:** Gelir `PositiveText` / `PositiveSurface`, Kart `Indicator` / `SurfaceChart`, Kredi `WarningText` / `WarningSurface`, Ödeme `NegativeText` / `NegativeSurface`. Renk sayfadan `DynamicResource` ile bileşene verilir (GK8); bileşen temayı bilmez. Noktanın `Backdrop` üzerindeki üç yeni kontrast satırı tabloda (eşik 3,0). (3) **Karo:** örtük `Border` (`SurfaceCard`, `BorderSubtle`, `RadiusCard`, `CardPadding`); ikon karesi grubun yüzeyinde `Space2` iç boşlukla, ikon `IconMedium` 24 × 24 kutuda; başlık `TypeFigure` `OpenSansSemibold` `TextPrimary`, alt satır `Caption`. Dokununca formu açar; seçili durumu yoktur, `›` yok. (4) **Karo başına bir ikon:** GK6 metnine "karo da kart gibi sayılır" eklendi (kullanıcı kararı). Beş yeni ikon `Icons.cs`'te (Repeat, AutoAwesome, EventAvailable, PieChart, BarChart; kod noktaları fontun kendisinden doğrulandı); kart, banka ve nakit için var olanlar. İkon rengi kuralına tür seçici istisnası yazıldı. (5) **Izgara:** en fazla iki sütun; tek karolu bölüm kendi genişliğini doldurur. Satır ve sütun tanımı karo sayısından, karonun yeri sırasından (`KaroIzgarasiConverter`, 12 Dönem karosu deseni). `V6f1`'de KART ve KREDİ sayfada yan yana iki bölümdü; `V6f2`'de ikinci karolarıyla (Kartla harcama, Krediye erken ödeme) her biri kendi satırına geçti, GELİR üçüncü karoyla (Gelir değişikliği) iki satır oldu. Üç yeni ikon: `ShoppingBag`, `FastForward`, `TrendingUp` (kod noktaları fonttan doğrulandı). "Hangi kart?" ikinci seviyesinde adaylar `ListCard` değil, aynı karo dilinde tam genişlik karolardır (ad + Finansal Yapı'daki bağlam + `›`): liste beşinci kart olurdu (GK4). (6) Bileşen (`EntryTypeTiles`) bir grubu çizer ve bir kart sayılır; sayfada dört örnek: kart 4/4. İki ekran kullanır (Finansal Yapı `V6f`, simülatör `V10c`). |
| **Etkiler** | `EK-V6f`, `EK-V10` (`V10c`), `TASARIM-SISTEMI.md` (§ Bileşenler, § İkonlar, § Kontrast çiftleri), `.claude/rules/06-tasarim.md` (GK4 kart listesi, GK6 karo cümlesi), `Icons.cs`, `DesignBudgetAnalyzer` (bileşen kart sayılır), `DesignComponentTests` |
| **Durum** | uygulandı (`V6f1`, Kapı C onaylı 2026-10-03, koyu + açık; `V6f2`, Kapı C onaylı 2026-10-04) |

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
