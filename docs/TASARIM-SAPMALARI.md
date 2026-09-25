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
| **Neden yanlış** | Hata ayıklama çıktısı, üretim ekranında. Hiçbir kullanıcı sorusuna bağlanmıyor. Ayrıca ViewModel'de metin üretiyor; `rules/03-mvvm.md` bunu zaten yasaklıyor. |
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
| **Etkiler** | `T2`, tüm V adımları |
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
| **Durum** | açık, **kullanıcı kararı bekliyor** |

### GS9 — Marka tarifinin kapsamadığı değerler türetildi

| | |
|---|---|
| **Tür** | veri-kısıtı |
| **Marka** | Dört renk ailesi tanımlıyor: titanyum, grafit, turuncu/çelik mavisi, metalik gri. Semantik renkler, ikincil yüzeyler, basılı hâller ve biçim (yarıçap) tanımlı değil. |
| **Çatışma** | Marka `#64748B`'yi "tarih aralıkları, açıklama metinleri" için veriyor ama bu değer **eşiği geçemiyor**: açık kartta 4,32 (gövde metni 4,5 ister), koyu kartta 2,91 (3,0 bile değil). |
| **Yeni** | `#64748B` açık temada `TextMuted` ve `BorderStrong` oluyor (4,3). Tarih ve açıklama metni `TextSecondary`'ye geçiyor (açık `#4E5B6D` 6,3; koyu `#A7B0BC` 6,3). Koyu temada metalik gri yerine türetilmiş `#7A889B` (3,8). Semantik, karşılaştırma ve ikincil yüzeyler grafit ile çelik mavisinin tonlarından türetildi; hepsi kontrast tablosundan geçiyor. |
| **Biçim** | Yarıçaplar "endüstriyel ciddiyet" tarifine göre keskinleşti: çip 8 → 4, kart 14 → 8, hero 18 → 12. Eski tablodaki 4'ün katı olmayan iki değer (14, 18) böylece düzeldi. Pill buton kalktı; `RadiusPill` yalnız ilerleme çubuğunda. |
| **Kaynak ayrımı** | Hangi değerin markadan geldiği, hangisinin türetildiği `docs/v2/04-NEDEN-BU-TASARIM.md` § Planör paleti'nde (eski depoda) tek tek yazılı. |
| **Kilit** | `T1`'de kullanıcı onayıyla kilitlenir; kilitlenince `uygulandı` ve türetilen değerler nihai sayılır. |
| **Etkiler** | `T1` |
| **Durum** | açık |

---

## Ekran bazlı

> `GS20`+ kayıtları ilgili V adımının Aşama 4'ünde yazılır. Şu an boş olması doğrudur.

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
