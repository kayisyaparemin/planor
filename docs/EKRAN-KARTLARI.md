# Ekran Kartları

Her ekranın bir kartı vardır. Kart, o ekranın Aşama 4'te verilen **çıkarma kararlarının** ve
Aşama 5'te onaylanan **düzeninin** tek kaydıdır.

Kural **GK9** iki yönlü çalışır: kartsız sayfa da, sayfası olmayan kart da kırmızıdır.

Kart anahtarı adım kodundan gelir: `EK-V3` = `V3` adımının ekranı.

## Kart biçimi

Her kart şu altı bölümü taşır — boş bırakılan bölüm, verilmemiş bir karardır:

1. **Sorular** — ekranın cevapladığı sorular, kullanıcının diliyle, önem sırasıyla (≤ 5)
2. **Kesme kararları** — her bilgi parçası: hero / kart / şema / grafik / satır / derine / çıkar
3. **Bütçe** — sayım tablosu (GK4, GK5). `Grafik  N / 1` gözün **aynı anda** gördüğü sayıdır;
   kaydırılan hero varsa ek bir `Hero sayfa  N / 2` satırı yazılır (`GS22`)
4. **Blok şeması** — bileşen, token, cevapladığı soru kodu
5. **Üç durum** — boş, yükleniyor, hata
6. **Konsept ilişkisi** — hangi konsept panelinden geliyor, ya da neyden türetildi

---

## EK-V3 — Ana sayfa

> Sayfa dosyaları: `DashboardPage.xaml` (`V3a`), `BalanceEntryPage.xaml` (`V3b`, aşağıda "Sayfa 2")
> "Rota + Tempo" yenilemesi (`GS24`, `S72`): `V3a` bu sayfa, tamamlandı (Kapı C kararları `GS24`'te).
> "Bakiye gir" sayfası bu kartın ikinci sayfasıdır (`S73`, `GS25`), `V3b` ile tamamlandı. İlk hâl (`GS20`) iptal.

**Önceki hâl (`V3`, `GS20`):** `DashboardPage.xaml` 233 satır, 11 `<Label>`, 3 kart, 1 buton, 5 `NavRow`, 1 grafik (halka).
**Eski proje:** `MainPage.xaml` 425 satır, **54 `<Label>`**, 5 kart, 11 buton, 1 geliştirici dökümü.

### 1. Sorular

| Kod | Soru | Önceki hâl nasıl cevaplıyordu |
|---|---|---|
| S1 | "Dönem sonunda elimde ne kalacak, plana göre neredeyim?" | Eyebrow + hero rakam (2 etiket); plan ve fark yok, grafik yok, bakiye girilmemişse tire |
| S2 | "Yaşam giderinden ne kaldı, harcamam süreye göre hızlı mı?" | Kalan oran halkası + kalan / harcanan (4 etiket); süre ayrı çubukta ve bugüne göre (`S70`) |
| S3 | "Bankada şu an ne var, en son ne zaman girdim?" | Yarım: giriş kutucuğu vardı, son bakiye ve tarihi ekrana bağlı değildi |
| S4 | "Bu dönem daha ne ödeyeceğim, bugün ödemem gereken var mı?" | Hatırlatıcı + ≤ 3 ikonlu satır + "+N ödeme daha" (5 etiket) |
| S5 | "Dönem bitti mi, kapatmam gerekiyor mu?" | Dönem çubuğundaki gün sayacı + "Dönemi Kapat" butonu |

### 2. Kesme kararları (`GS24`)

| Bilgi / Öğe | Karar | Gerekçe |
|---|---|---|
| Dönem sonu (tahmin; bakiye yoksa plan) | **Hero** (`HeroFigure`), kaydırılan kartın 1. sayfası | S1 ekranın asıl sorusu; bakiye yokken planın dediği gösterilir, tire değil (`S72-1`) |
| Plana göre fark + plan tutarı | **Satır** (hero'nun altında) | S1; yalnız bakiye girildiyse (`S72-2`), fark işaretli ve semantik renkli |
| Bakiye rotası | **Grafik** (`ColumnTrend`), 1. sayfa | S1 "oraya nasıl gidiyorum": eşit dilimli sütunlar; bugüne kadarki dolu `Indicator`, tahmin soluk, plan altı olumsuz renk, bugün rozeti (`GS30`); bakiye yokken planın rotası |
| Dönemin ilk ve son günü | **Satır** (grafiğin altında iki etiket) | Grafiğin tarih ekseni; son gün `PeriodEnd − 1` (`S72-7`) |
| Kalan yaşam gideri | **Şema** (`RingGauge` ortasında, `TypeTitle`), 2. sayfa | S2; halka dolgusu harcanan oran, işareti geçen süre (`S72-3`) |
| Harcanan % / geçen süre % | **Satır** (2 × `MetricRow`) | S2 halkanın sayı karşılığı; ikisi de son bakiyenin gününe göre |
| Tempo cümlesi | **Satır** (`Bicim_`, bir tek `Cumle_` "aynı hızda") | S2'nin tek cümlelik cevabı (`S72-4`) |
| Son bakiye + tarihi + "Bakiye gir" | **Kart** | S3; giriş ayrı sayfa (`V3b`, `S72-8`) |
| Dönem bitti + son gün + "Dönemi kapat" | **Kart** (bakiye kartının yerinde) | S5; biten döneme bakiye yazılamaz (`S68-4`), cümle yok (`V11`) |
| Hatırlatıcı | **Kart** (`ReminderCard`, `EK-V2`) | S4 acil ödeme; aktif kayıt yoksa görünmez |
| Kalan ödemeler | **Kart** (`ListCard`, ≤ 3 satır: ad, vade, tutar) | S4 tarama sorusu; adet ve toplam kartın notunda |
| Tüm kalan ödemeler | **Kart** içinde yerinde açılır ("+N daha", yalnız gizli satır varsa; `GS21` deseni) | S4'ün ikinci seviyesi; açık dönemin ayrıntısı ana sayfanın kendisi, `EK-V9` yalnız gelecek dönemler (`S75`-9, `GS27`-5; ilk karar "→ `EK-V9`" idi) |
| Dönem aralığı + gün sayacı / "Bitti" | **Satır** (başlık, `Shell.TitleView`) | S5 takvim bağlamı |
| Gezinme satırları × 4, ayarlar ikonu | **Çıkar** | Aynı hedefler yan menüde |
| Dönem çubuğu (`PeriodRail`) | **Çıkar** | Aralık ve sayaç başlıkta, geçen süre halkanın işaretinde |
| Ödeme satırı ikonları | **Çıkar** | GK6: kart başına en fazla bir ikon |
| Harcanan tutarı | **Çıkar** | S2 oranı soruyor |
| Halkadaki "10 gün kaldı" | **Çıkar** | Başlıktaki gün sayacıyla aynı bilgi |
| Grafikteki "Bugün" / "Plan" yazıları, gözlem noktası işareti | **Çıkar** | `AreaTrend` yazı ve işaret çizmez; `ColumnTrend` rozet taşır (`GS30`) |
| Sayfa içi bakiye girişi (`HeroInputCard`) | **Çıkar** → `V3b` | Tarih ve önizlemeyle ayrı sayfa |

### 3. Bütçe

```
Hero rakam    1 / 1     dönem sonu (tahmin ya da plan)
Hero yüzey    0 / 1     kaydırılan kart tonlu SurfaceChart (GS24), SurfaceHero değil; HeroInputCard kalktı
Kart          4 / 4     HeroPager, bakiye/kapanış kartı, ReminderCard, ListCard
Grafik        1 / 1     aynı anda: 1. sayfada ColumnTrend, 2. sayfada RingGauge
Hero sayfa    2 / 2
NavRow        0 / 5
Label        25 / 28    başlık 4, 1. sayfa 7, 2. sayfa 5, bakiye kartı 6, ödeme satırı şablonu 3
Cumle_        3 / 3     Cumle_TempoAyniHiz, Cumle_TempoIlkBakiye, Cumle_AcikDonemYokRehber
```

Analizci kartı 2 sayar: ham `Border` (bakiye kartı) ve `ReminderCard` onun listesinde yok. Etiketi 28
sayar: `<Label.Text>` (2) ve `<Label.Triggers>` (1) özellik öğelerini de `<Label` sayıyor. Bütçe
dürüst sayımdır. `Cumle_TempoAyniHiz` dönüştürücüden gelir, sayfa XAML'inde görünmez; yine sayılır.

### 4. Blok şeması

```
┌─ Shell.TitleView ────────────────────────────────────────────┐  ← S5
│ [☰ kabuk]  10 Eylül – 9 Ekim                 20 / 30 gün      │
│            Bicim_DonemAraligi                Bicim_GunSayaci  │
│            TypeSection / TextPrimary         TypeCaption / TextSecondary
│            dönem bittiyse sağda Etiket_Bitti; dönem yoksa Baslik_AnaSayfa
└──────────────────────────────────────────────────────────────┘
┌─ HeroPager   SurfaceChart / BorderSubtle / RadiusHero / CardPadding ┐
│ ┌ HeroPage 1 ────────────────────────────────────────────────┐ │  ← S1
│ │ Etiket_DonemSonuTahmini  (bakiye yoksa Etiket_PlanlananDonemSonu)  Eyebrow
│ │ 41.723 ₺                                HeroFigure / TextPrimary
│ │ Plana göre −2.177 ₺  ·  Plan 43.900 ₺   TypeCaption; fark NegativeText /
│ │   (yalnız bakiye girildiyse)            PositiveText, plan TextSecondary
│ │ ColumnTrend  yükseklik ChartHeight                         │ │
│ │   Eşit dilimler: bugüne kadarki sütun Indicator, tahmin    │ │
│ │   soluk (%45); plan altı NegativeText; Bugün rozeti;       │ │
│ │   PlanLevel yatay TextSecondary hairline                   │ │
│ │ 10 Eylül                                        9 Ekim     │ │  Caption
│ └────────────────────────────────────────────────────────────┘ │
│ ┌ HeroPage 2 ────────────────────────────────────────────────┐ │  ← S2
│ │           ╭ RingGauge  ChartHeight × ChartHeight ╮          │ │
│ │           │   Etiket_KalanYasamGideri    Caption │          │ │
│ │           │   8.600 ₺                    TypeTitle / TextPrimary
│ │           ╰ dolgu Indicator = harcanan · işaret TextSecondary = geçen süre
│ │ MetricRow  Etiket_Harcanan ......................... %71    │ │
│ │ MetricRow  Etiket_GecenSure ........................ %67    │ │
│ │ Harcama, geçen sürenin 4 puan önünde.   TypeCaption / TextSecondary
│ │ bakiye yoksa: halka boş, ortada Etiket_BakiyeGirilmedi,     │ │
│ │   MetricRow'lar yok, altta Cumle_TempoIlkBakiye             │ │
│ └────────────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────────┘
                    ▬  •    etkin uzun çubuk Indicator, diğeri nokta TextMuted; dokunulabilir
┌─ Bakiye kartı (ham Border)  SurfaceCard / BorderSubtle / RadiusCard ─┐  ← S3
│ Etiket_BankadakiBakiye                     Eyebrow                   │
│ 58.940 ₺   27 Eylül        Figure / TextPrimary · Caption            │
│ (bakiye yoksa Etiket_HenuzGirilmedi  TypeBody / TextSecondary)       │
│                                  [ Aksiyon_BakiyeGir   ActionFill ]  │
├─ aynı yer, dönem bittiyse ──────────────────────────────────────────┤  ← S5
│ Etiket_DonemBitti                          Eyebrow                   │
│ 9 Ekim                     Figure / TextPrimary                      │
│                                  [ Aksiyon_DonemiKapat ActionFill ]  │
└──────────────────────────────────────────────────────────────────────┘
┌─ ReminderCard (HasActiveReminder) — EK-V2, değişmedi ────────────────┐  ← S4
└──────────────────────────────────────────────────────────────────────┘
┌─ ListCard ───────────────────────────────────────────────────────────┐  ← S4
│ Etiket_KalanOdemeler               4 ödeme · 24.009 ₺  (Bicim_OdemeAdediToplam)
│ Kredi taksiti                                   12.450 ₺             │  TypeBody / TextPrimary · Figure
│ 1 Ekim                                                               │  Caption
│ … en fazla 3 satır                                                   │
│ +3 daha                                                              │  yalnız gizli satır varsa; yerinde açılır (S75-9)
└──────────────────────────────────────────────────────────────────────┘
```

Yeni ölçü token'ı `ChartHeight` (144): `HeightRequest` literali yasak (GK3) ve iki hero sayfasının
görseli aynı yükseklikte durmalı. `HeroPager` iki sayfayı üst üste ölçer (görünmeyen sayfa saydam),
kart kaydırınca boy değiştirmez. Açılışta ve her yüklemede 1. sayfa.

### 5. Üç durum

| Durum | Görünen |
|---|---|
| Boş | `StateBlock`: `AccountBalance` ikonu + `Cumle_AcikDonemYokRehber` + `Aksiyon_TemizBasla` (kurulum sihirbazı). Başlıkta `Baslik_AnaSayfa`. |
| Yükleniyor | Kart şeklinde üç `SkeletonBlock` (kaydırılan kart, bakiye kartı, liste); spinner yok. Başlıkta dönem gelene kadar `Baslik_AnaSayfa`. |
| Hata | `StateBlock`: `Close` ikonu + `Hata_DashboardYuklenemedi` + `Aksiyon_TekrarDene`. Başlıkta `Baslik_AnaSayfa`. Metin genel kalır: hata hesaptan da gelebilir. |

Dolu hâlin iki çeşidi durum değil, içeriktir ve şemada yazılı: bakiye hiç girilmemiş (plan rakamı,
plan rotası, boş halka) ve dönem bitmiş ama kapanmamış (bakiye kartının yerinde kapanış).

### 6. Konsept ilişkisi

Claude Design'da üretilen D yönü: `docs/assets/konsept/ana-sayfa-rota-tempo.png` (dolu, iki tema,
yükleniyor), `ana-sayfa-rota-tempo-durumlar.png` (boş, bakiye girilmemiş, kapanış, hata) ve
`ana-sayfa-rota-tempo-kapanis.png` (halka sayfasının bakiyesiz hâli). Yerleşim oradan; renkler
token'lardan. Konseptten sapmalar `GS24`'te: kapanış bakiye kartının yerinde, grafikte yazı ve
gözlem işareti yok, "10 gün kaldı" yok, bakiyesiz grafik planın rotası, ödeme satırı 3 ve ikonsuz,
boş hâlde ortak `StateBlock`.

### Sayfa 2 — "Bakiye gir" (`V3b`)

> Sayfa dosyası: `BalanceEntryPage.xaml` (`Routes.BalanceEntry`). Ana sayfadaki "Bakiye Gir"den açılır,
> ✕ ya da geri tuşuyla onaysız kapanır. Davranış `S73`, ekran `GS25`.

**Önceki hâl:** sayfa yoktu; `V3a`'dan önce ana sayfada `HeroInputCard` (tutar + düğme, tarih hep bugün, önizleme yok).

#### 1. Sorular

| Kod | Soru | Önceki hâl nasıl cevaplıyordu |
|---|---|---|
| S1 | "Bankada ne var, bunu hangi güne yazıyorum?" | Tutar kutusu vardı; gün hep bugündü |
| S2 | "Bu girişle dönem sonunda ne kalır, plana göre nerede olurum?" | Hiç: etki ancak kaydettikten sonra görülüyordu |
| S3 | "En son ne girmiştim?" | Bu sayfada yok (ana sayfanın bakiye kartında, `V3a`) |

#### 2. Kesme kararları (`GS25`)

| Bilgi / Öğe | Karar | Gerekçe |
|---|---|---|
| Girilen bakiye | **Hero** (tutar alanı, `TypeHero`) | S1 sayfanın tek girdisi; eksi (KMH) ve sıfır geçerli (`S73-3`) |
| ₺ işareti | **Satır** (tutar alanının sağında) | Birim |
| Son giriş ve tarihi | **Satır** (`Bicim_SonGiris`), yalnız önceki giriş varsa | S3 |
| Bakiyenin günü | **Satır** (tarih seçici, `[dönem başı, bugün]`; bugünse "Bugün") | S1; varsayılan bugün (`S73-1`) |
| Bu girişle dönem sonu | **Kart** (önizleme), `TypeTitle` | S2; yalnız geçerli tutar varken (`S73-2`) |
| Plana göre fark | **Satır** (kartta, işaretli, renk artı/eksiye göre) | S2 |
| Önceki tahmin | **Satır** (kartta), yalnız önceki giriş varsa | S2 "girişim neyi değiştirdi" (`S73-6`); konseptteki okun yerine |
| Önizleme rotası | **Grafik** (`ColumnTrend`, `ChartHeight`) | S2 "oraya nasıl"; ana sayfanın 1. sayfasıyla aynı roller, taslak giriş noktadır |
| Kaydet | **Buton** (tam genişlik) | S1 |
| Dönem bitti + "Dönemi Kapat" | **Durum** (`StateBlock`) | `S68-4`: kapanışı bekleyen döneme bakiye yazılmaz (`S73-5`) |
| "güncel" kelimesi | **Çıkar** | Geriye tarihli girişte yanlış |
| Ok (`41.723 → 44.380`) | **Çıkar** → "Önceki tahmin" | GK6: ok bir ikon, `Icons.cs`'te ileri ok yok |
| Takvim ikonu, "Değiştir" | **Çıkar** | `Icons.cs`'te yok; seçicinin kendisine dokunulur (kullanıcı kararı) |
| Plan tutarı | **Çıkar** | Ana sayfada var; burada soru "girişim neyi değiştirir" |
| Grafiğin tarih etiketleri | **Çıkar** | Kısa grafik; ana sayfada var |
| Aynı gün değiştirme notu | **Çıkar** | `S68-2` sessiz çalışır, önizleme grafiği gösterir |

#### 3. Bütçe

```
Hero rakam    1 / 1     girilen bakiye (tutar alanı)
Hero yüzey    0 / 1     önizleme kartı tonlu SurfaceChart (GS25), SurfaceHero değil
Kart          1 / 4     önizleme kartı (ham Border; analizci 0 sayar)
Grafik        1 / 1     önizleme rotası (ColumnTrend)
Hero sayfa    0 / 2
NavRow        0 / 5
Label        11 / 28    başlık 1, tutar 2, son giriş 2, bugün 1, önizleme 5
Cumle_        1 / 3     Cumle_OnceKapanis
```

Etiket sayımı analizcinin sayımıdır: `<Label.Text>` (son giriş) ve `<Label.Triggers>` (fark) birer etiket sayılır.

#### 4. Blok şeması

```
┌─ Shell ──────────────────────────────────────────────────────┐
│ [✕] BackButtonBehavior IconOverride: Icons.Close, IconLarge, │
│     TextPrimary; komutu yok, geri gezinme onaysız (S73-7)    │
│ Shell.TitleView: Baslik_BakiyeGir   SectionTitle             │
└──────────────────────────────────────────────────────────────┘
┌─ Tutar (kartsız) ────────────────────────────────────────────┐  ← S1
│ Etiket_BankadakiBakiye                        Eyebrow        │
│ [ 62.300                                   ]   ₺             │
│   Entry TypeHero / OpenSansSemibold / TextPrimary,           │
│   Keyboard Numeric            Etiket_Tl  TypeTitle / TextSecondary
│ Son giriş 58.940 ₺ · 27 Eylül     Bicim_SonGiris  Caption    │  ← S3
│   (yalnız önceki giriş varsa)                                │
└──────────────────────────────────────────────────────────────┘
┌─ Tarih satırı ───────────────────────────────────────────────┐  ← S1
│ [ 30.09.2026 ]                                 Bugün         │
│   DatePicker SurfaceSunken / TextPrimary / TypeBody,         │
│   Min dönem başı, Max bugün     Etiket_Bugun Caption (bugünse)
└──────────────────────────────────────────────────────────────┘
┌─ Önizleme (ham Border) SurfaceChart / BorderSubtle / RadiusHero / CardPadding ┐  ← S2
│ Etiket_BuGirisleDonemSonu                       Eyebrow                  │
│ 44.380 ₺                        TypeTitle / OpenSansSemibold / TextPrimary
│ Plana göre +480 ₺     Bicim_PlanaGore  Caption; NegativeText / PositiveText
│ Önceki tahmin 41.723 ₺  Bicim_OncekiTahmin  Caption (yalnız önceki giriş varsa)
│ ColumnTrend  ChartHeight — ana sayfanın rolleri: eşit dilimler,          │
│   bugüne kadarki sütun Indicator, tahmin soluk (%45), plan altı Negative, │
│   Bugün rozeti, PlanLevel yatay TextSecondary                            │
│ yalnız geçerli tutar ve başarılı önizleme varken görünür                 │
└──────────────────────────────────────────────────────────────────────────┘
[ Aksiyon_Kaydet                                 ActionFill, tam genişlik ]  ← S1
```

Ana sayfa ile önizleme aynı grafiği aynı yoldan kurar (`BalancePathTrend`, bağımlılıksız yardımcı, M8).
Önizleme kartının durumu çocuk görünüm modelindedir (`BalancePreviewViewModel`: ya bütün hâliyle görünür ya
hiç); sayfa `BalanceEntryViewModel`'e bağlıdır (4 bağımlılık). Klavye kendiliğinden açılmaz (`GS25` 5).

#### 5. Üç durum

| Durum | Görünen |
|---|---|
| Boş (dönem bitti, kapanış bekliyor) | `StateBlock`: `Schedule` ikonu + `Cumle_OnceKapanis` + `Aksiyon_DonemiKapat` (`V11` rotası). Açık dönem hiç yoksa sayfa geri döner. |
| Yükleniyor | İki `SkeletonBlock` (tutar alanı, önizleme kartı yüksekliğinde); spinner yok. |
| Hata | `StateBlock`: `Close` ikonu + `Hata_BakiyeGirisYuklenemedi` + `Aksiyon_TekrarDene`. Önizleme hatası sayfa durumu değildir: kart gizlenir. Kayıt hatası uyarıyla söylenir, tutar yerinde kalır. |

#### 6. Konsept ilişkisi

`docs/assets/konsept/ana-sayfa-rota-tempo-durumlar.png`, "Bakiye gir · Açık" paneli. Yerleşim oradan,
renkler token'lardan. Sapmalar `GS25`'te: önizleme ana sayfa kartının tonunda, ok yerine "Önceki
tahmin", takvim ikonu ve "Değiştir" yok, "güncel" yok, klavye kendiliğinden açılmaz, grafik `ChartHeight`
ve tarihsiz, plan tutarı yok.

---

## EK-V0 — Kabuk ve altyapı

> Sayfası yok (kabuk, gezinme, diyalog portları). GK9 istisnası: `AppShell` bir `*Page` değil (`GS15`).
> Kabuk başlığı, flyout ve durum çubuğu tonları `T1`'de tanımlandı.

**Eski hâl:** `AppShell.cs` 129 satır C#, Service Locator (`IServiceProvider`), XAML yok, statik stil/renk aramaları.

### 1. Sorular

| Kod | Soru | Eskide nasıl cevaplanıyordu |
|---|---|---|
| S1 | "Uygulamanın ana bölümlerine nasıl geçerim?" | C# FlyoutItem'lar, ServiceLocator factory (6 menü öğesi) |
| S2 | "Şu an hangi profildeyim ve profiller arasında nasıl geçiş yaparım?" | C# CreateProfileHeader, MenuItem("Profil Değiştir") |
| S3 | "Geri dönmek veya modalı kapatmak istediğimde ne olur?" | Sayfalara sıkı bağlı INavigationService metotları |
| S4 | "Uygulama benden onay istediğinde ne görürüm?" | IUserFeedbackService / DisplayAlert |
| S5 | "İşlem yürütülürken durumun meşgul olduğunu sistem nasıl bilir?" | ViewModelBase içindeki IsBusy ve CanInteract |

### 2. Kesme kararları

| Bilgi / Öğesi | Karar | Gerekçe |
|---|---|---|
| Aktif profil adı | **Satır** (`TypeSection`) | FlyoutHeader içinde aktif profilin adı (`TextPrimary`) |
| "PROFİL" üst etiketi | **Satır** (`TypeEyebrow`) | FlyoutHeader içinde kategori etiketi (`TextSecondary`) |
| 6 ana gezinme rotası | **Menü Öğesi** (`FlyoutItem`) | Ana Sayfa, 12 Dönem, Simülatör, Finansal Yapı, Geçmiş, Ayarlar |
| Profil değiştirme aksiyonu | **Aksiyon** (`MenuItem`) | Profil seçim modalını tetikleyen menü aksiyonu |
| Sayfaya özel modal metotları | **Çıkar** | Rota bazlı genel navigasyona indirgendi |
| ViewModel'de para formatlama/kültür | **Çıkar** | Saf veri sunulur, formatlama XAML converter'larına aittir (K3, K5) |
| PDF ekstre içe aktarma bayrakları | **Çıkar** | S21 kararıyla özellik bütünüyle elendi |
| Sentry ve tanrı depo (IMizanStore) | **Çıkar** | S60 ve S26 kararlarıyla elendi |

### 3. Bütçe

```
Hero rakam    0 / 1     kabukta hero rakam yok
Hero yüzey    0 / 1     kabukta hero yüzey yok
Kart          0 / 4     kabuk bir sayfa değildir (GK9 istisnası)
Grafik        0 / 1     kabukta grafik yok
NavRow        0 / 5     kabukta navrow yok (FlyoutItem kullanılır)
Label         2 / 28    FlyoutHeader: Eyebrow(PROFİL), Section(Profil Adı)
Cumle_        0 / 3     açıklama cümlesi yok
```

### 4. Blok şeması

```
┌─ FlyoutHeader (SurfaceCard / Padding Space4) ────┐  ← S2
│ Etiket_Profil       TypeEyebrow / TextSecondary  │
│ {ActiveProfileName} TypeSection / TextPrimary    │
└──────────────────────────────────────────────────┘
┌─ FlyoutItem × 6 ─────────────────────────────────┐  ← S1
│ [Insights]       Ana Sayfa       (//dashboard)   │
│ [Schedule]       12 Dönem        (//projection)  │
│ [Insights]       Simülatör       (//simulation)  │
│ [Payments]       Finansal Yapı   (//commitments) │
│ [Schedule]       Geçmiş          (//history)     │
│ [Settings]       Ayarlar         (//settings)    │
└──────────────────────────────────────────────────┘
┌─ MenuItem ───────────────────────────────────────┐  ← S2
│ [ChevronRight]   Profil Değiştir                 │
└──────────────────────────────────────────────────┘
┌─ Shell.Chrome (Backdrop, TextPrimary, TextSecondary) ┐  ← S1
│ TopAppBar / StatusBarStyle                           │
└──────────────────────────────────────────────────────┘
```

### 5. Üç durum

| Durum | Görünen |
|---|---|
| Boş | Profil yoksa doğrudan profil seçim ve karşılama akışına yönlendirilir (`Routes.ProfileSelection`). |
| Yükleniyor | Kabuk ilklendirilip profil yüklenirken `ScreenState.Loading` etkindir; menü etkileşimi kilitlenir (`CanInteract == false`). |
| Hata | Profil veritabanı veya kabuk açılamazsa `ScreenState.Error` durumuyla diyalog uyarısı verilir ve profil seçimine dönülür. |

### 6. Konsept ilişkisi

Sayfasız kabuk altyapısı (GK9 istisnası, GS15). Kabuk başlığı, flyout menüsü ve durum çubuğu tonları `docs/TASARIM-SISTEMI.md` § Yüzeyler (`Backdrop`: Koyu `#1E232A`, Açık `#E5E7EB`) ile birebir uyumludur. İkonlar `Icons.cs` üzerinden Material Symbols Rounded fontundan gelir (GK6).

## EK-V1 — Profil seçimi

> Adım V1 ile tamamlandı.
> Sayfa dosyası: `ProfileSelectionPage.xaml`

---

## Doldurulacak kartlar

Aşağıdaki kartlar ilgili V adımının Aşama 4–5'inde doldurulur. **Boş bir kart, o adımın
henüz başlamadığını gösterir** — kartı önceden doldurmak, onay kapısını atlamaktır.

**Eski hâl:** `ProfileSelectionPage.xaml` 97 satır, 10 `<Label>`, 1 kart (Border), 5 buton, 1 spinner (`ActivityIndicator`).

### 1. Sorular

| Kod | Soru | Eskide nasıl cevaplanıyordu |
|---|---|---|
| S1 | "Hangi profillerim var ve hangisini açmak istiyorum?" | `Border` içinde 3 etiket (avatar, ad, son açılış) |
| S2 | "Yeni bir profil oluşturabilir miyim?" | "Yeni Profil" ve "Temiz Başla" butonları |
| S3 | "Yedekten profillerimi nasıl geri getirebilirim / ekleyebilirim?" | "Yedekten Geri Yükle" ve "Yedekten Ekle" butonları |
| S4 | "Mevcut bir profilin adını değiştirebilir veya silebilir miyim?" | Liste içi "Düzenle" link butonu + diyalog |
| S5 | "İlk kurulumda mıyım, yoksa profiller arası geçişte miyim?" | `IsFirstRun` ile açılan karşılama kutusu |

### 2. Kesme kararları

| Bilgi / Öğe | Karar | Gerekçe |
|---|---|---|
| Sayfa başlığı | **Satır** (`PageHeader`) | "PROFİLLER" eyebrow + "Profil Seçimi" başlığı (S1, S5) |
| Profil listesi | **Kart** (`ListCard`) | Profillerin listelendiği ana kapsayıcı kart (S1) |
| Profil satırı (Avatar + Ad + Son açılış) | **Satır** (`DataTemplate`) | Dokunulduğunda profili açar (S1) |
| Profil seçenekleri ("Düzenle") | **Satır** (Aksiyon butonu) | Ad değiştirme veya silme diyaloğunu tetikler (S4) |
| Yeni profil oluşturma | **Aksiyon** (`ActionFill`) | Listenin altındaki ana buton (S2) |
| Yedekten profil ekleme | **Aksiyon** (İkincil buton) | Harici veya yerel yedekten profil ekleme (S3) |
| İlk kurulum karşılama | **Kart** (`StateBlock`) | Profil yokken temiz başla veya yedekten dön kartı (S5) |
| `MİZAN` üst etiketi | **Çıkar** | GK11 (ürün adı Planör; eski ad yasağı) |
| Veri izolasyonu uzun açıklaması | **Çıkar** | GK5 cümle bütçesi; bilişsel yükü azaltma |
| Ham dosya yolu metni | **Çıkar** | Bilişsel yük; dosya seçim akışına devredildi |
| Spinner (`ActivityIndicator`) | **Çıkar** | GS14 (spinner kesinlikle yasak) |
| `StatusMessage` etiketi | **Çıkar** | Hata durumu `StateBlock` veya diyalogla yönetilir |

### 3. Bütçe

```
Hero rakam    0 / 1     profil seçiminde hero rakam yok
Hero yüzey    0 / 1     hero yüzey yok
Kart          1 / 4     profil listesi kartı (ListCard) veya boş durumda StateBlock
Grafik        0 / 1     grafik yok
NavRow        0 / 5     navrow yok (profil kartı satırları DataTemplate)
Label         7 / 28    PageHeader (2), DataTemplate [Avatar, İsim, Tarih] (3), StateBlock (2)
Cumle_        1 / 3     Cumle_ProfilSecimNotu veya Boş durum karşılama cümlesi
```

### 4. Blok şeması

```
┌─ PageHeader ─────────────────────────────────────────────┐
│ Etiket_Profiller       TypeEyebrow / TextSecondary       │  ← S5
│ Baslik_ProfilSecimi    TypeTitle / TextPrimary           │  ← S1
└──────────────────────────────────────────────────────────┘
┌─ ListCard (SurfaceCard / RadiusCard / Padding Space4) ───┐  ← S1
│ Etiket_MevcutProfiller TypeEyebrow / TextSecondary       │
│                                                          │
│ ┌─ Profil Satırı (DataTemplate) ───────────────────────┐ │
│ │ ┌ Avatar ──┐                                         │ │
│ │ │ "P"      │  Adı: Ev Bütçesi (TypeSection)          │ │  ← S1
│ │ │ SurfSunk │  Son açılış: 26 Eyl 2026 (TypeCaption)  │ │
│ │ └──────────┘  [ Düzenle ] (TypeCaption / Indicator)  │ │  ← S4
│ └──────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────┘
┌─ Aksiyon Butonları (Spacing Space3) ─────────────────────┐
│ [ Aksiyon_YeniProfil       ActionFill / RadiusCard ]     │  ← S2
│ [ Aksiyon_YedektenEkle     BorderStrong / RadiusCard ]   │  ← S3
└──────────────────────────────────────────────────────────┘
│ Cumle_ProfilSecimNotu      TypeCaption / TextSecondary   │  ← S5
```

Hiç profil yokken (İlk Kurulum / Boş Durum):

```
┌─ PageHeader ─────────────────────────────────────────────┐
│ Etiket_Planor          TypeEyebrow / TextSecondary       │  ← S5
│ Baslik_HosGeldiniz     TypeTitle / TextPrimary           │  ← S5
└──────────────────────────────────────────────────────────┘
┌─ StateBlock (SurfaceCard / RadiusCard / Padding Space5) ─┐  ← S5
│ [AccountBalance]       IconLarge / Indicator             │
│ Baslik_IlkKurulum      TypeSection / TextPrimary         │
│ Cumle_IlkKurulum       TypeBody / TextSecondary          │  ← S5
│                                                          │
│ [ Aksiyon_TemizBasla       ActionFill / RadiusCard ]     │  ← S2
│ [ Aksiyon_YedektenYukle    BorderStrong / RadiusCard ]   │  ← S3
└──────────────────────────────────────────────────────────┘
```

### 5. Üç durum

| Durum | Görünen |
|---|---|
| Boş | `StateBlock`: `AccountBalance` ikonu + `Cumle_IlkKurulum` ("Temiz bir başlangıç yapabilir veya mevcut bir yedeği geri yükleyebilirsin.") + `Aksiyon_TemizBasla` ve `Aksiyon_YedektenYukle` butonları. |
| Yükleniyor | İskelet (`SkeletonBlock`): `PageHeader` iskeleti + `ListCard` yüksekliğinde `SurfaceSunken` zeminli kart iskeleti; spinner yok. |
| Hata | `StateBlock`: `Close` ikonu (`NegativeText`) + `Hata_ProfillerOkunamadi` ("Profiller yüklenemedi.") + `Aksiyon_TekrarDene` butonu. |

### 6. Konsept ilişkisi

Konsept panellerinde profil seçimi ekranı yer almamaktadır (`GS2`). Yerleşim ve bileşenler, `EK-V3`'ün `ListCard` liste kapsayıcısı ile `StateBlock` durum deseni referans alınarak türetilmiştir.

---

## EK-V2 — Hatırlatıcı kartı

> Sayfası yok (çocuk kart, sayfasız çocuk ViewModel). GK9 istisnası: `ReminderCard` bir `*Page` değil (`GS15`).
> `EK-V3` (Ana Sayfa) içinde ve vadesi gelen/ertelenen aktif ödeme olduğunda görünür.

**Eski hâl:** `PaymentReminderCardView.xaml` (141 satır, 11 `<Label>`, 3 `<Border>`, 3 `<Button>` + FlexLayout içi buton şablonu) ve `PaymentReminderPaidView.xaml` (50 satır, 4 `<Label>`, 2 `<Border>`). Toplam 191 satır, 15 `<Label>`, 5 Border/kart.

### 1. Sorular

| Kod | Soru | Eskide nasıl cevaplanıyordu |
|---|---|---|
| S1 | "Bugün veya vadesi geçmiş/ertelenmiş, hemen ödemem gereken acil bir yükümlülük var mı?" | 6 etiket + SummaryText + 35 günlük tüm vadeleri döken Upcoming listesi |
| S2 | "Bu ödemeyi yaptım mı, yoksa daha sonraya mı ertelemek istiyorum?" | Bildirim çubuğu butonları veya kart satırına dokununca açılan onay diyaloğu |
| S3 | "Yanlışlıkla ödedim veya erteledim dediğim bir işlemi geri alabilir miyim?" | Snoozed (kırmızı kutular) ve Paid (yeşil kutular) listeleri üzerinden |
| S4 | "Ödeme bildirim tercihim (Kapalı/Rahat/Agresif) ve bildirim iznim açık mı?" | 3 butonluk mod seçici çipler, mod açıklaması ve uyarı kutusu |

### 2. Kesme kararları

| Bilgi / Öğe | Karar | Gerekçe |
|---|---|---|
| Aktif hatırlatıcı başlığı ve vadesi | **Kart** (`ReminderCard.Title`) | S1: Anlık müdahale bekleyen acil ödeme kimliği (`TypeSection`) |
| Ödeme tutarı ve detay açıklaması | **Kart** (`ReminderCard.Message`) | S1: Tek bakışta okunur tutar ve açıklama (`TypeBody`) |
| "Ödedim" aksiyonu | **Aksiyon** (`ActionFill`) | S2: Tek dokunuşla borcu kapatma (`PrimaryActionText`) |
| "Ertele" aksiyonu | **Aksiyon** (`SecondaryButton`) | S2: 3 saat öteleme (`SecondaryActionText`) |
| Bildirim modu çipleri (Kapalı/Rahat/Agresif) | **Derine** → `EK-V13` | S4: Sistemik ayar; ana sayfa hatırlatıcı kartını meşgul etmemeli |
| 35 günlük yaklaşan ödemeler listesi | **Derine** → `EK-V3` / `EK-V9` | `ListCard` (kalan ödemeler) zaten bu veriyi sunuyor; mükerrer bilgi |
| Ertelenenler geçmiş listesi | **Çıkar** | Vadesi gelen ertelenen zaten aktif hatırlatıcı olarak tekrar öne düşer |
| "Ödediklerin" geçmiş listesi | **Derine** → `V11` / `EK-V12` | Dönem gerçekleşmesi ve mutabakat ekranında yer alır. İlk karar `EK-V9` idi; dönem ayrıntısı yalnız gelecek dönemleri gösterir (`S75`-7, -9) |
| "Deneme bildirimi gönder" butonu | **Çıkar** | Geliştirici/test artığı |
| `StatusMessage` hata etiketi | **Çıkar** | Hata durumu diyalog servisi ile yönetilir |

### 3. Bütçe

```
Hero rakam    0 / 1     çocuk kartta hero yok
Hero yüzey    0 / 1     hero yüzey yok
Kart          1 / 4     ReminderCard (tekil çocuk kart)
Grafik        0 / 1     grafik yok
NavRow        0 / 5     navrow yok
Label         2 / 28    Title (TypeSection), Message (TypeBody)
Cumle_        1 / 3     Message (vade ve tutar bildirimi ≤ 90 kr)
```

### 4. Blok şeması

Aktif hatırlatıcı olduğunda (`HasActiveReminder == true`):

```
┌─ ReminderCard (SurfaceCard / BorderSubtle / RadiusCard / Padding Space4) ──┐
│ ┌─ Bildirim Başlığı (ColumnDefinitions: Auto, *) ────────────────────────┐ │
│ │ [Notifications]   IconMedium / Indicator                               │ │  ← S1
│ │ ┌─ Başlık & Mesaj (VerticalStackLayout, Spacing Space1) ─────────────┐ │ │
│ │ │ {Title}         TypeSection / TextPrimary                          │ │ │  ← S1
│ │ │ {Message}       TypeBody / TextSecondary                           │ │ │  ← S1
│ │ └────────────────────────────────────────────────────────────────────┘ │ │
│ └────────────────────────────────────────────────────────────────────────┘ │
│ ┌─ İki Aksiyon Butonu (Grid, ColumnDefinitions: *, *, Spacing Space2) ───┐ │
│ │ [ Ertele ]        SecondaryButton / RadiusCard                         │ │  ← S2
│ │ [ Ödedim ]        ActionFill / TextOnAction / RadiusCard               │ │  ← S2
│ └────────────────────────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────────────────────┘
```

Aktif hatırlatıcı olmadığında (`HasActiveReminder == false`):
Kart görünmezdir (`IsVisible = false`), ana sayfada yer tutmaz.

### 5. Üç durum

| Durum | Görünen |
|---|---|
| Boş | Vadesi gelen veya ertelenen aktif bir hatırlatıcı yoksa kart görünmezdir (`IsVisible = false`), yerleşim zıplaması yaratmaz. |
| Yükleniyor | Çocuk kart bağımsız bir yükleme iskeleti göstermez; ana sayfa (`EK-V3`) genel yükleme iskeletinin (`ScreenState.Loading`) bir parçası olarak değerlendirilir. |
| Hata | Hatırlatıcı defteri okunamaz veya yanıt kaydedilemezse diyalog uyarısı verilir (`Hata_HatirlaticiGuncellenemedi`), kart ana sayfayı kilitlemez. |

### 6. Konsept ilişkisi

Konsept bildirim panelindeki ("Ödedim / Ertele" aksiyonları) kart deseni doğrudan kaynaktır. Sayfasız çocuk ViewModel'dir (`GS15` istisnası); `ReminderCard` bileşeni olarak `EK-V3` (Ana Sayfa) içine gömülür.

---


## EK-V4 — Kurulum sihirbazı

> Sayfa dosyası: `OnboardingPage.xaml`
> Konsept karşılığı: Kurulum (8/8 özeti) paneli.
> Eski hâl: `OnboardingPage.xaml` 452 satır, **77 `<Label>`**, 20 `<Button>`, 10 `<Border>`, 25 `<Entry>`, 2 spinner.
> Eski ViewModel: 886 satır / 3 `partial` dosya (`OnboardingViewModel`, `Cards`, `Obligations`).

### 1. Sorular

| Kod | Soru | Eskide nasıl cevaplanıyordu |
|---|---|---|
| S1 | "Planör'ü kullanmaya nereden ve nasıl başlayacağım?" | Intro adımı (3 buton) + 8 form adımı; geliştiriciye özel örnek doldur butonu ekrandaydı. |
| S2 | "Dönemim ne zaman başlar, bugünkü param ve serbest yaşam havuzum nedir?" | Adım 1 (dönem günü + yapay tahsis picker'ı), Adım 6 (yaşam gideri), Adım 7 (mevcut bakiye). |
| S3 | "Düzenli gelirlerim neler ve ne zaman yatar?" | Adım 2: Tek gelir geçmişi (`SalaryScheduleEntry`), ödeme günü yoktu; ikinci gelir girilince ilki siliniyordu (`S2`). |
| S4 | "Hangi borç ve kredi yükümlülüklerim var?" | Adım 3 (Kartlar + PDF yükleme + 14 alan), Adım 4 (Krediler), Adım 5 (Yaklaşan ödemeler). |
| S5 | "Girdiğim kurulum bilgileri doğru mu, planımı başlatabilir miyim?" | Adım 8: 7 satırlık döküm + "Mizan'ı Başlat" butonu. |

### 2. Kesme kararları

| Bilgi / Öğe | Karar | Gerekçe |
|---|---|---|
| Adım başlığı ve sayacı | **Satır** (`PageHeader` deseni) | S1: "Adım 2/8" (`TypeEyebrow`) + "Gelirlerin" (`TypeTitle`) + [Atla] (`TextSecondary`) |
| İlerleme çubuğu | **Satır** (`ProgressBar`) | S1: % ilerleme görseli (`Indicator`, `RadiusPill`) |
| Adım rehber cümlesi | **Satır** (`TypeCaption`) | S1: Tek satır rehber açıklama (GK5: ≤ 90 karakter) |
| Aktif adım form kutusu | **Kart** (`SurfaceCard`) | S2–S4: İlgili adımın giriş alanları ve ekleme aksiyonu (GK4: ≤ 4 kart) |
| Eklenen kayıtlar listesi | **Satır** (`DataTemplate` satırları) | S3–S4: İsim + Tutar + [Sil] aksiyonu içeren hafif liste |
| Son adım kontrol özeti | **Kart** (`SurfaceCard`) | S5: Kompakt 7 metrik satırı (`MetricRow` deseni) |
| "Planör'ü Başlat" | **Aksiyon** (`ActionFill`) | S5: Kurulumu tamamlayan birincil buton |
| Alt navigasyon çubuğu | **Satır** (`Backdrop` zemin) | [Geri] (`SecondaryButton`) + [Devam] (`ActionFill`) butonları |
| "Örnek Veriyle Doldur" butonu | **Çıkar** | Geliştirici artığı |
| PDF ekstre yükleme alanı | **Çıkar** | `S21` kararı |
| Yapay tahsis seçim alanı | **Çıkar** | `S18` kararı |
| Kart gelişmiş ödeme stratejileri | **Çıkar** | `V7` / `V13` ekranlarına devredildi |
| Spinner (`ActivityIndicator`) | **Çıkar** | `GS14` kuralı |
| `StatusMessage` hata etiketleri | **Çıkar** | Hatalar diyalog servisiyle yönetilir |

### 3. Bütçe

```
Hero rakam    0 / 1     kurulum sihirbazında hero rakam yok
Hero yüzey    0 / 1     hero yüzey yok
Kart          2 / 4     form kartı (SurfaceCard), son adım özet kartı (SurfaceCard)
Grafik        0 / 1     grafik yok
NavRow        0 / 5     navrow yok (adım adımlama kullanılır)
Label        17 / 28    sayfada ve adım formlarında tanımlı etiketler (+1 etiket: Güncel Borç)
Cumle_        2 / 3     Cumle_AdimAciklama, Cumle_KurulumNotu (her biri ≤ 90 kr)
```

### 4. Blok şeması

Genel Sayfa Düzeni:

```
┌─ PageHeader (Header Alanı) ───────────────────────────────────┐
│ Etiket_AdimSayaci (TypeEyebrow, TextSecondary)                │  ← S1
│ Baslik_Adim (TypeTitle, TextPrimary)    [ Aksiyon_Atla ]      │  ← S1
└───────────────────────────────────────────────────────────────┘
┌─ İlerleme Çubuğu (ProgressBar, Indicator, RadiusPill) ────────┐  ← S1
└───────────────────────────────────────────────────────────────┘
│ Cumle_AdimRehberi (TypeCaption, TextSecondary)                │  ← S1

[ Aktif Adım Bloğu — 1..8'den biri ]

┌─ Alt Navigasyon Çubuğu (SurfaceCard / StrokeHairline) ────────┐  ← S1
│ [ Aksiyon_Geri  SecondaryButton ]  [ Aksiyon_Devam ActionFill]│
└───────────────────────────────────────────────────────────────┘
```

#### Adım 1 — Dönem Çapası:
```
┌─ FormCard (SurfaceCard / RadiusCard / Padding Space4) ────────┐
│ Etiket_DonemGunu         TypeEyebrow / TextSecondary          │  ← S2
│ [ Entry_DonemGunu        SurfaceSunken / RadiusCard ] (1–31)  │  ← S2
│ Cumle_DonemGunuRehber    TypeCaption / TextSecondary          │  ← S2
└───────────────────────────────────────────────────────────────┘
```

#### Adım 2 — Düzenli Gelirler:
```
┌─ FormCard (SurfaceCard / RadiusCard / Padding Space4) ────────┐
│ Etiket_GelirAdi          TypeEyebrow / TextSecondary          │  ← S3
│ [ Entry_GelirAdi         SurfaceSunken / RadiusCard ]         │  ← S3
│ Etiket_AylikNetTutar     TypeEyebrow / TextSecondary          │  ← S3
│ [ Entry_AylikNetTutar    SurfaceSunken / RadiusCard ]         │  ← S3
│ ┌─ İki Sütun (Spacing Space2) ──────────────────────────────┐ │
│ │ Etiket_OdemeGunu (1–31)       Etiket_GecerlilikTarihi     │ │  ← S3
│ │ [ Entry_OdemeGunu ]           [ DatePicker ]              │ │  ← S3
│ └───────────────────────────────────────────────────────────┘ │
│ [ Aksiyon_GelirEkle      SecondaryButton / RadiusCard ]       │  ← S3
│ ┌─ Eklenen Gelirler (DataTemplate) ─────────────────────────┐ │
│ │ [Payments]  Maaş · Her ayın 15'i         45.000 ₺  [Sil]  │ │  ← S3
│ └───────────────────────────────────────────────────────────┘ │
└───────────────────────────────────────────────────────────────┘
```

#### Adım 3 — Kredi Kartları:
```
┌─ FormCard (SurfaceCard / RadiusCard / Padding Space4) ────────┐
│ Etiket_KartAdi           TypeEyebrow / TextSecondary          │  ← S4
│ [ Entry_KartAdi          SurfaceSunken / RadiusCard ]         │  ← S4
│                                                               │
│ Etiket_Banka             TypeEyebrow / TextSecondary          │  ← S4
│ [ Entry_Banka            SurfaceSunken / RadiusCard ]         │  ← S4
│                                                               │
│ ┌─ İki Sütun (Spacing Space2) ──────────────────────────────┐ │
│ │ Etiket_Limit                  Etiket_GuncelBorc           │ │  ← S4
│ │ [ Entry_Limit ]               [ Entry_GuncelBorc (±/0) ]  │ │  ← S4
│ │                                                           │ │
│ │ Etiket_KesimGunu (1–31)       Etiket_SonOdemeGunu (1–31)  │ │  ← S4
│ │ [ Entry_KesimGunu ]           [ Entry_SonOdemeGunu ]      │ │  ← S4
│ └───────────────────────────────────────────────────────────┘ │
│                                                               │
│ [ Aksiyon_KartEkle       SecondaryButton / RadiusCard ]       │  ← S4
│ ┌─ Eklenen Kartlar (DataTemplate) ──────────────────────────┐ │
│ │ [CreditCard]  Bonus · Kesim: 12 · Vade: 22   Borç: 15.000 │ │  ← S4
│ └───────────────────────────────────────────────────────────┘ │
└───────────────────────────────────────────────────────────────┘
```

#### Adım 4 — Krediler:
```
┌─ FormCard (SurfaceCard / RadiusCard / Padding Space4) ────────┐
│ Etiket_KrediAdi          TypeEyebrow / TextSecondary          │  ← S4
│ [ Entry_KrediAdi         SurfaceSunken / RadiusCard ]         │  ← S4
│ ┌─ İki Sütun (Spacing Space2) ──────────────────────────────┐ │
│ │ Etiket_Banka                  Etiket_AylikTaksit          │ │  ← S4
│ │ [ Entry_Banka ]               [ Entry_AylikTaksit ]       │ │  ← S4
│ │ Etiket_TaksitGunu (1–31)      Etiket_KalanTaksitSayisi    │ │  ← S4
│ │ [ Entry_TaksitGunu ]          [ Entry_KalanTaksitSayisi ] │ │  ← S4
│ └───────────────────────────────────────────────────────────┘ │
│ Etiket_KalanAnapara (İsteğe bağlı)                            │  ← S4
│ [ Entry_KalanAnapara     SurfaceSunken / RadiusCard ]         │  ← S4
│ [ Aksiyon_KrediEkle      SecondaryButton / RadiusCard ]       │  ← S4
│ ┌─ Eklenen Krediler (DataTemplate) ─────────────────────────┐ │
│ │ [AccountBalance] Konut Kredisi · 18 taksit  12.500 ₺ [Sil]│ │  ← S4
│ └───────────────────────────────────────────────────────────┘ │
└───────────────────────────────────────────────────────────────┘
```

#### Adım 5 — Yaklaşan Ödemeler:
```
┌─ FormCard (SurfaceCard / RadiusCard / Padding Space4) ────────┐
│ Etiket_OdemeAdi          TypeEyebrow / TextSecondary          │  ← S4
│ [ Entry_OdemeAdi         SurfaceSunken / RadiusCard ]         │  ← S4
│ ┌─ İki Sütun (Spacing Space2) ──────────────────────────────┐ │
│ │ Etiket_OdemeTutari            Etiket_OdemeTarihi          │ │  ← S4
│ │ [ Entry_OdemeTutari ]         [ DatePicker ]              │ │  ← S4
│ └───────────────────────────────────────────────────────────┘ │
│ ┌─ Taksit / Tekrar Seçeneği (İsteğe bağlı) ─────────────────┐ │
│ │ Etiket_TaksitSayisi (Boşsa tek seferlik)                  │ │  ← S4
│ │ [ Entry_TaksitSayisi ]                                    │ │  ← S4
│ └───────────────────────────────────────────────────────────┘ │
│ [ Aksiyon_OdemeEkle      SecondaryButton / RadiusCard ]       │  ← S4
│ ┌─ Eklenen Ödemeler (DataTemplate) ─────────────────────────┐ │
│ │ [Payments]  Sigorta Poliçesi · 15 Eki       4.200 ₺  [Sil]│ │  ← S4
│ └───────────────────────────────────────────────────────────┘ │
└───────────────────────────────────────────────────────────────┘
```

#### Adım 6 — Serbest Yaşam Gideri:
```
┌─ FormCard (SurfaceCard / RadiusCard / Padding Space4) ────────┐
│ Etiket_YasamGideri       TypeEyebrow / TextSecondary          │  ← S2
│ [ Entry_YasamGideri      SurfaceSunken / RadiusCard ]         │  ← S2
│ Cumle_YasamGideriRehber  TypeCaption / TextSecondary          │  ← S2
└───────────────────────────────────────────────────────────────┘
```

#### Adım 7 — Mevcut Bakiye (Açılış Durumu):
```
┌─ FormCard (SurfaceCard / RadiusCard / Padding Space4) ────────┐
│ Etiket_MevcutBakiye      TypeEyebrow / TextSecondary          │  ← S2
│ [ Entry_MevcutBakiye     SurfaceSunken / RadiusCard ] (±)     │  ← S2
│ Cumle_MevcutBakiyeRehber TypeCaption / TextSecondary          │  ← S2
└───────────────────────────────────────────────────────────────┘
```

#### Adım 8 — Özet ve Başlatma (Konsept 8/8):
```
┌─ SummaryCard / ReviewCard (SurfaceCard / RadiusCard) ─────────┐  ← S5
│ Etiket_KurulumOzeti      TypeEyebrow / TextSecondary          │
│ Baslik_IlkPlanHazir      TypeSection / TextPrimary            │
│ ───────────────────────────────────────────────────────────── │
│ Dönem Çapası             Her ayın 15'i          (MetricRow)   │  ← S2
│ Düzenli Gelirler         1 akış · 45.000 ₺      (MetricRow)   │  ← S3
│ Kredi Kartları           1 kart · Borç: 15.000 ₺(MetricRow)   │  ← S4
│ Krediler                 1 kredi · 12.500 ₺     (MetricRow)   │  ← S4
│ Planlı Ödemeler          1 ödeme · 4.200 ₺      (MetricRow)   │  ← S4
│ Yaşam Gideri Havuzu      15.000 ₺               (MetricRow)   │  ← S2
│ Açılış Bakiyesi          18.400 ₺               (MetricRow)   │  ← S2
└───────────────────────────────────────────────────────────────┘
┌─ Başlatma Aksiyonu ───────────────────────────────────────────┐  ← S5
│ [ Aksiyon_PlanoruBaslat   ActionFill / TextOnAction ]         │
└───────────────────────────────────────────────────────────────┘
```

### 5. Üç durum

| Durum | Görünen |
|---|---|
| Boş | Sihirbaz adımları varsayılan olarak boş form alanlarıyla açılır; kullanıcı zorunlu olmayan adımları doğrudan "Atla" veya "Devam" ile geçebilir. |
| Yükleniyor | Kurulum başlatıldığında `ScreenState.Loading` etkindir; butonlar kilitlenir, spinner gösterilmez (`GS14`). |
| Hata | Kurulum kaydedilirken bir hata oluşursa `IDialogService` üzerinden kullanıcı diliyle hata uyarısı verilir (`Hata_KurulumKaydedilemedi`). |

### 6. Konsept ilişkisi

Konsept panellerindeki *Kurulum (8/8 özeti)* paneli doğrudan referanstır. 8 adımlı ilerleme çubuğu, temiz kart kutusu ve 8. adımdaki metrik özet dökümü konsept yerleşimiyle birebir uyumludur. Paletteki sarı/lacivert renkler emekli edilmiş olup Planör marka token'ları (`ActionFill`, `SurfaceCard`, `Indicator`) kullanılmıştır (`GS7`).

---

## EK-V5 — İlk düzen seçimi

> **Taşınmadan elendi (Taşımama hakkı: `S18`, `S47`, `S62`).** Sayfası yoktur (GK9 istisnası).
> Eski projedeki `InitialStrategyPage.xaml` (harcamaları yapay olarak geçmiş/gelecek döneme kaydıran düzen seçimi),
> Mizan v2'deki bağımsız dönem çapası (`S1`), çoklu gelir akışı (`S2`) ve **doğal dönemsellik** (`S18`, `I16`)
> ilkeleri doğrultusunda bütünüyle kaldırılmıştır. Yarı açık `[PeriodStart, PeriodEnd)` aralığına vadesi
> düşen her kalem istisnasız o döneme aittir. Likidite ve vade farkları bütçe kaydırmayla değil, dönem içi
> bakiye rotası ve KMH faiziyle izlenir (`S71`).
> Bu sebeple arayüzde bir düzen seçimi sayfası veya modalı üretilmemiştir.

---

## EK-V7 — Kart kontrol

> Sayfa dosyası: `CardControlPage.xaml`
> Adım **V7** ile tamamlandı (Kapı C onaylı). Davranış kararları: `S61`. İlk tasarım (V7a:
> borç/limit + ekstre kartı) Kapı C'de "cevap vermiyor" diye geri döndü; ekranın merkezi sıradaki
> ödeme oldu. Kapı C'de açılan sistem işleri: `T7` (başlık aksiyonunun anlaşılırlığı), `T8`
> (diyalog tasarımı).

**Eski hâl:** `CardControlPage.xaml` 491 satır, **73 `<Label>`**, 10 `<Border>`, 30 `<Button>`, 2 spinner;
`CardControlViewModel` 866 satır / 3 `partial`.

### 1. Sorular

| Kod | Soru | Eskide nasıl cevaplanıyordu |
|---|---|---|
| S1 | "Bu karttan sıradaki ödemede ne kadar çıkacak, nasıl ödeyeceğim?" | Yalnız kesilmiş ekstre varsa: "Bu Ekstreyi Nasıl Ödeyeceksin?" kutusu, 4 etiket + 3 segment; ekstre yoksa cevap yoktu |
| S2 | "Asgari ödersem ne olur?" | "Devreden • finansman • yeni harcama" döküm cümlesi (tek satır, 3 tutar) |
| S3 | "Sonraki aylarda bu kart ne kadar ödetecek?" | "Sonraki Ekstre" kutusu 5 etiket + 6 satır × (3 etiket + 3 düğme) |
| S4 | "Ayrı karar vermediğim ekstreler nasıl ödenir?" | "GENEL" açılır paneli: iki kutu, 8 etiket, 5 düğme |
| S5 | "Kartta ne kadar yer kaldı?" | Üst kutu: toplam borç + limit, 4 etiket |

### 2. Kesme kararları

| Bilgi / Öğe | Karar | Gerekçe |
|---|---|---|
| Sıradaki vadede ödenecek tutar | **Hero rakam** | S1; kesilmiş ekstre yoksa tahmini ekstreden |
| Vade tarihi + "tahmini" / "kesilmiş ekstre" işareti | **Satır** (hero kartında) | S1 bağlamı |
| Asgari / Tamamı / Özel seçici | **Satır** (hero kartında) | S1 kararın kendisi; seçili olan `ActionFill` |
| Ekstre tutarı · asgari | **Satır** (hero kartında, tek satır) | S1 bağlamı |
| Devreden tutar + sonraki ekstreye binen faiz | **Satır** (yalnız devir varsa) | S2 kararın bedeli |
| Elle ekstre girişi | **Kart** (hero kartının giriş hâli) | S1 tahmini gerçeğe çevirir; `S21` yalnız PDF'yi eledi |
| Sonraki ödemeler | **Kart** (`ListCard`, ≤ 4 satır) | S3; satıra dokununca o vade için karar (`GS12`) |
| Kartın varsayılan ödeme şekli + varsayım | **Satır** (`NavRow` → diyalog) | S4 |
| Limit · kullanılabilir | **Satır** (en altta, tek satır) | S5 bağlam |
| Banka · kesim günü · son ödeme günü | **Satır** (başlık altı) | kartın döngüsü |
| Kart değiştirme | **Aksiyon** (başlık ikonu, yalnız > 1 kart) | Hangi kart açık; kimliksiz açılışta en yakın ödemesi olan kart |
| Toplam borç, limit çubuğu | **Çıkar** | Kapı C: hiçbir kararı beslemiyor |
| Gelecek kart harcamaları formu | **Derine** → `EK-V6` | Kartın tanımına ait veri girişi (`S61`) |
| Ödeme tercihi geçmişi | **Çıkar** | Soruya bağlanmıyor; tarihçe kaydı değişmez |
| "Sonraki Ekstre" ayrı kutusu | **Çıkar** | Sonraki ödemelerin ilk satırıyla aynı bilgi |
| PDF'den içe aktarma | **Çıkar** | `S21` |
| "Kart Bilgilerini Düzenle" düğmesi | **Çıkar** | `EK-V6`'nın işi |
| Satır başına 3 düğme (18 düğme) | **Çıkar** | Satıra dokunma + diyalog |
| Spinner, `StatusMessage` | **Çıkar** | `GS14`; hatalar diyalogla |

### 3. Bütçe

```
Hero rakam    1 / 1     sıradaki vadede ödenecek tutar
Hero yüzey    0 / 1     hero yüzey yok
Kart          2 / 4     sıradaki ödeme kartı (ödeme / ödeme yok / giriş), sonraki ödemeler ListCard
Grafik        0 / 1     grafik yok
NavRow        1 / 5     varsayılan ödeme şekli
Label        23 / 28    19 etiket + 4 <Label.Text> öğesi (analizci onları da sayar); DataTemplate içi bir kez
Cumle_        2 / 3     Cumle_OdemeYok, Cumle_VadeKarari
```

### 4. Blok şeması

```
┌─ PageHeader ─────────────────────────────────────────────────────────┐
│ Etiket_KrediKarti          TypeEyebrow / TextSecondary                │
│ {CardName}                 TypeTitle / TextPrimary       [CreditCard] │  ikon yalnız > 1 kart
└───────────────────────────────────────────────────────────────────────┘
  Akbank · Kesim 25 · Son ödeme 5            Caption (Bicim_KartDongusu)
┌─ Sıradaki ödeme kartı (Border SurfaceCard / RadiusCard) ──────────────┐
│ Etiket_SiradakiOdeme                        (Eyebrow)                  │
│ ── Hâl A: ödeme var ──                                                │
│ 5 Ekim  (Figure)                  tahmini | kesilmiş ekstre (Caption)  │  ← S1
│ 26.747 ₺                                    (HeroFigure)               │  ← S1
│ Ekstre 26.747 ₺ · asgari 5.349 ₺            (Caption, Bicim_EkstreAsgari) │
│ [ Asgari ] [ Tamamı ] [ Özel ]      seçili: ActionFill / TextOnAction  │  ← S1
│ (Özel'e basılınca) [ tutar Entry ] [ Aksiyon_Kaydet ]                  │
│ 21.398 ₺ sonraki ekstreye devreder, ~1.070 ₺ faiz biner.  (Caption)    │  ← S2 yalnız devir varsa
│ [ Aksiyon_EkstreyiGir | Aksiyon_EkstreyiDuzenle   SecondaryButton ]    │  ← S1
│ ── Hâl B: önümüzdeki ödeme yok ──                                     │
│ Cumle_OdemeYok                              (Caption)                  │
│ [ Aksiyon_EkstreyiGir               SecondaryButton ]                 │
│ ── Hâl C: ekstre girişi / düzenleme ──                                │
│ Etiket_EkstreTutari [Entry]         Etiket_AsgariOdeme [Entry]         │
│ Etiket_KesimTarihi  [DatePicker]    Etiket_SonOdemeTarihi [DatePicker] │
│ [ Aksiyon_Kaydet  ActionFill ]      [ Aksiyon_Vazgec  Secondary ]      │
└───────────────────────────────────────────────────────────────────────┘
┌─ ListCard  Etiket_SonrakiOdemeler ────────────────────────────────────┐  ← S3
│ [Schedule]  5 Kasım  (TypeBody)                    17.866 ₺  (Figure)  │
│             Tamamı · kartın varsayılanı             (Caption)          │
│   … en fazla 4 satır, tutarı sıfır olan vadeler atlanır;               │
│   dokun → "5 Kasım ödemesi": Asgari / Tamamı / Kartın varsayılanına dön│
└───────────────────────────────────────────────────────────────────────┘
  Cumle_VadeKarari                          (Caption)                     ← S3
┌─ NavRow ──────────────────────────────────────────────────────────────┐  ← S4
│ [Settings]  Baslik_VarsayilanOdeme · Tamamı                        ›   │
│   dokun → Asgari / Tamamı / Her ekstrede sor;                         │
│   "Her ekstrede sor" → ikinci diyalog: karar yokken Asgari / Tamamı    │
└───────────────────────────────────────────────────────────────────────┘
  Limit 600.000 ₺ · kullanılabilir 575.767 ₺   (Caption, Bicim_LimitSatiri) ← S5
```

Uygulama notları:
- Sıradaki ödeme = kesilmiş ekstre, yoksa tutarı sıfırdan büyük ilk tahmini ekstre. Karar
  verilmemişse (kart "her ekstrede sor", varsayım devrede) hiçbir seçenek seçili görünmez.
- Asgari / Tamamı kararı kesilmiş ekstrede ekstrenin planına, tahminde o vadeye özel plana
  yazılır; Özel tutar tahminde sabit tutarlı vade planıdır (asgarinin altına inemez, alan kuralı).
- Yeni ekstrenin ödeme şekli kartın varsayılanından başlar (`S61`); giriş formunun tarihleri
  sıradaki vadeden dolar.
- Tutar ve tarih kuralları `CreditCardValidator`'dadır; ihlal diyalogla gösterilir.
- Yeni bileşen yok. App'e üç converter gelir: `TarihSecici` (`DateOnly` ↔ `DatePicker`), ödeme
  kuralı satırı ve varsayılan ödeme şekli metni.
- Sol menüdeki "Kart Kontrol" öğesi **geçicidir** (`S61`): kendi `AutomationId`'si ve
  `card-control` ile çakışmayan rotası (`cards`) vardır; `V6` kaldırır.

### 5. Üç durum

| Durum | Görünen |
|---|---|
| Boş | Profilde kart yoksa `StateBlock` (Boş, `CreditCard` ikonu, `Bos_KartYok`); aksiyon yok, kart `EK-V6`'dan eklenir. |
| Yükleniyor | Üç `SkeletonBlock`; spinner yok (`GS14`). |
| Hata | Okuma hatasında `StateBlock` (Hata, `Hata_KartYuklenemedi`, `Aksiyon_TekrarDene` → yükle); kaydetme hatasında diyalog, ekran olduğu gibi kalır. |

### 6. Konsept ilişkisi

Konsept karşılığı **yok** (`GS2`). Türetme kaynağı `EK-V3`: hero rakam + bağlam satırları,
`ListCard`, `NavRow`, `StateBlock`, `SkeletonBlock`. Giriş formu `EK-V4` Adım 3'ün form
satırlarından (`Eyebrow` + `Entry`, `Grid *,*`). Renk ve ses Planör marka token'larından (`GS7`).

## EK-V6 — Finansal yapı

> Sayfa dosyası: `FinancialStructurePage.xaml`
> Liste adım **V6a** ile tamamlandı (Kapı C onaylı, koyu + açık). Davranış kararları: `S62`; taşma: `GS21`. Formlar ayrı
> sayfalardır ve `V6b`–`V6e`'de gelir; her biri bu listeye "Ekle" seçeneği ve satır
> diyaloğuna "Düzenle" ekler. Form sayfalarının kartları kendi adımlarında yazılır.

**Eski hâl:** `CommitmentsPage.xaml` 450 satır, **86 `<Label>`**, 19 buton, 22 `Entry`, 11 seçici,
6 `Border`, 2 spinner; `CommitmentsViewModel` 1.344 satır / 6 `partial`.

### 1. Sorular

| Kod | Soru | Eskide nasıl cevaplanıyordu |
|---|---|---|
| S1 | "Planıma neler giriyor; gelirim, kartlarım, kredilerim, ödemelerim eksiksiz mi?" | 5 bölüm başlığı + 10 boş durum etiketi + şablonlarda ad ve tutar, ~25 etiket |
| S2 | "Her kayıt bana ne kadar tutuyor, sıradaki ne zaman?" | Şablon alt satırları (6 etiket); kartta ekstre yoksa "—" |
| S3 | "Bu kaydı nasıl silerim, kartın ödemesine nasıl giderim?" | Satır başına 1–2 düğme (Kartı Aç / Düzenle / Sil), şablonlarda 7 düğme |
| S4 | "Yeni bir şeyi nereden eklerim?" | "+ Ekle" + satır içi form (~49 etiket) — **`V6b`–`V6e`'de cevaplanır** |

### 2. Kesme kararları

| Bilgi / Öğe | Karar | Gerekçe |
|---|---|---|
| Gelirler, Kartlar, Krediler, Ödemeler grupları | **Kart** (4 × `ListCard`) | S1; boş grup görünmez |
| Satır: ad · bağlam · tutar · › | **Satır** (tek paylaşılan şablon) | S1, S2 |
| Satır aksiyonu (kart kontrol, sil; sonra düzenle) | **Satır** (dokun → diyalog) | S3; eskideki 7 düğme şablonu kalkar |
| Kredi faizi, bugün kapatma bedeli, planlı erken ödemeler | **Derine** → `V6c` | İkinci seviye ayrıntı |
| Kart ekstre / asgari dökümü, kesim günleri, ödeme kuralı | **Çıkar** | `EK-V7`'de var |
| Kartta "Limit • Güncel borç" | **Çıkar** | `EK-V7` Kapı C: hiçbir kararı beslemiyor |
| Grup toplamları | **Çıkar** | Tarihleri farklı kalemleri toplamak yanıltır; dönem toplamı `EK-V3` / `EK-V9`'un işi |
| "3 gelir • 2 kart…" sayım satırı, 10 boş durum etiketi | **Çıkar** | Liste kendisi cevap; boş grup gizli |
| Başlık üstü etiket ("FİNANSAL KAYITLAR") | **Çıkar** | Kapı B: başlık ve grup adları zaten söylüyor; ikinci ad kafa karıştırır |
| Spinner, `StatusMessage`, `BusyMessage` | **Çıkar** | `GS14`; hata diyalogla |
| PDF ekstre okuma, ilk ödeme düzeni penceresi | **Çıkar** | `S21`; `S18`, `D10` |
| "+ Ekle" seçici ve satır içi formlar | **Derine** → `V6f` seçici, `V6b`–`V6e` formlar | S4; seçici ayrı sayfa (`EK-V6f`), her tür kendi sayfasında |
| Ortak senaryo formu | **Derine** → `V10` | `S62`-7 |

### 3. Bütçe

```
Hero rakam    0 / 1     hero yok
Hero yüzey    0 / 1     hero yüzey yok
Kart          4 / 4     Gelirler, Kartlar, Krediler, Ödemeler (ListCard)
Grafik        0 / 1     grafik yok
NavRow        0 / 5     NavRow yok
Label         4 / 28    tek satır şablonu: ad, bağlam, tutar, › (dört ListCard aynı şablonu kullanır)
Cumle_        0 / 3     açıklama cümlesi yok
```

### 4. Blok şeması

```
┌─ PageHeader ──────────────────────────────────────────────────────┐
│ Baslik_FinansalYapi         TypeTitle   / TextPrimary    [ Ekle ]  │  üst etiket yok; Aksiyon_Ekle metin (V6b1, EK-V6b)
└────────────────────────────────────────────────────────────────────┘
┌─ ListCard  Etiket_Gelirler ────────────────────────────────────────┐  ← S1, S2, S3
│ Gelir                    TypeBody / TextPrimary      45.000 ₺   ›  │
│ Her ayın 15. günü        Caption (Bicim_HerAyGunu)   Figure        │
│ Yıl sonu primi                                       20.000 ₺   ›  │
│ Tek seferlik · 20 Aralık Caption (Bicim_TekSeferlik)               │
└────────────────────────────────────────────────────────────────────┘
┌─ ListCard  Etiket_Kartlar ─────────────────────────────────────────┐  ← S1, S2, S3
│ Bonus                                                26.747 ₺   ›  │
│ Sıradaki ödeme 5 Ekim    Caption (Bicim_SiradakiOdeme)             │
│ Axess                                                           ›  │  ödeme yoksa tutar gizli
│ Önümüzdeki ödeme yok     Caption (Etiket_OdemeYok)                 │
└────────────────────────────────────────────────────────────────────┘
┌─ ListCard  Etiket_Krediler ────────────────────────────────────────┐  ← S1, S2, S3
│ İhtiyaç kredisi                                       7.500 ₺   ›  │
│ 12 taksit kaldı · sonraki 15 Ekim   Caption (Bicim_KrediBaglami)   │
└────────────────────────────────────────────────────────────────────┘
┌─ ListCard  Etiket_Odemeler ────────────────────────────────────────┐  ← S1, S2, S3
│ Okul taksidi                                          4.000 ₺   ›  │
│ 3 ödeme kaldı · sonraki 20 Ekim     Caption (Bicim_PlanBaglami)    │
│ Tatil                                                30.000 ₺   ›  │
│ Tek seferlik · 12 Temmuz                                           │
│   … en fazla 4 satır; fazlası varsa:                               │
│ +2 daha                  OverflowText (Bicim_FazlaKayit)        ›  │  GS21: grubu yerinde açar
└────────────────────────────────────────────────────────────────────┘
```

Satır şablonu — sayfada **bir kez** tanımlanır (`ContentPage.Resources`, `x:DataType` =
`FinancialRecordRow`), dört `ListCard` aynı şablonu kullanır:

```
Grid  "*, Auto, Auto"  ColumnSpacing Space3        dokun → SelectRecordCommand (satır)
├─ VerticalStackLayout  Spacing Space1
│    Label  Name                      TypeBody / TextPrimary
│    Label  ., KayitBaglami           Caption
├─ Label  Amount, Para                Figure            IsVisible = HasAmount
└─ Label  Icons.ChevronRight          IconSmall / Indicator
```

Satır diyaloğu (`IDialogService`, `T8` gelene kadar sistem diyaloğu):

```
Kart        → ChooseAsync(ad, "Vazgeç", "Sil", "Ödemeyi yönet", "Düzenle")     V6b1
Kredi       → ChooseAsync(ad, "Vazgeç", "Sil", "Düzenle")                      V6c1
Diğerleri   → ChooseAsync(ad, "Vazgeç", "Sil")                 V6d–V6e: + "Düzenle"
Ödemeyi yönet → Routes.CardControl + cardId
Düzenle     → kart: Routes.CardForm + cardId (EK-V6b) · kredi: Routes.LoanForm + loanId (EK-V6c)
Ekle        → Routes.RecordEntryPicker (EK-V6f, S77): tür seçici; karo seçicinin yerine formunu açar
                (V6b1–V6e'de buradaki düz ChooseAsync diyaloğu vardı)
Sil         → ConfirmAsync("Kaydı sil", "{ad} ve ona bağlı kayıtlar kalıcı olarak silinecek.",
                           "Sil", "Vazgeç") → türüne göre sil → listeyi yeniden yükle
```

Uygulama notları:
- Listede yalnız **plana giren** kayıtlar durur (`S62`-5): aktif gelir; bugün ve sonrası tek
  seferlik gelir; aktif kart; taksiti kalmış aktif kredi; tamamlanmamış ödeme planı;
  `Planned` durumundaki büyük harcama. Taksiti hiç olmayan plan tutarsız satır olarak
  (bağlam "Ödemesi yok", tutar gizli) görünür ve silinebilir.
- Satır verileri ham: gelir → bugün geçerli tutar (yoksa ilk ileri tarihli tutar) ve ayın günü;
  tek seferlik gelir → tutar ve tarih; kart → sıradaki ödeme tutarı ve vadesi (`EK-V7` hero'suyla
  aynı hesap, ekstre yoksa tahmini); kredi → aylık taksit, kalan taksit, sonraki ödeme tarihi;
  plan → sıradaki taksit tutarı, kalan ödeme sayısı, sıradaki tarih; büyük harcama → tutar ve tarih.
- Grup içi sıra: gelirler ayın gününe, tek seferlikler tarihe göre (düzenliler önce); kartlar
  sıradaki ödeme tarihine (ödemesi olmayan sonda); krediler ve ödemeler sıradaki tarihe göre.
- Ad boşsa banka adı kullanılır (kart, kredi).
- Metin App'teki `KayitBaglamiConverter`'da kurulur (V7'nin `OdemeKuraliConverter` deseni);
  ViewModel metin üretmez.
- Yeni bileşen yok. `ListCard` değişmez (`GS21`). App'e iki converter gelir: `KayitBaglamiConverter`
  (bağlam satırı) ve `FazlaKayitConverter` (gizli satır yoksa boş taşma metni). Taşma metni
  `ListCard.Triggers` ile bağlanmaz: bütçe analizcisi özellik etiketini de kart sayar.
- Sol menüdeki geçici "Kart Kontrol" öğesi ve `cards` rotası kalkar; "Finansal Yapı" menü
  öğesi bu sayfayı açar. `Routes.Commitments` → `Routes.FinancialStructure` (`S62`-8).

### 5. Üç durum

| Durum | Görünen |
|---|---|
| Boş | Hiçbir grupta satır yoksa `StateBlock` (Boş, `AccountBalance` ikonu, `Bos_KayitYok`); aksiyon yok, başlıktaki "Ekle" kullanılır (`V6b1`). |
| Yükleniyor | Üç `SkeletonBlock`; spinner yok (`GS14`). |
| Hata | Okuma hatasında `StateBlock` (Hata, `Close` ikonu, `Hata_YapiYuklenemedi`, `Aksiyon_TekrarDene` → yükle); silme hatasında diyalog, liste olduğu gibi kalır. |

### 6. Konsept ilişkisi

Konsept karşılığı **yok** (`GS2`). Türetme kaynağı `EK-V7`'nin "Sonraki ödemeler" satırı
(ad/tarih + `Caption` bağlam + `Figure` tutar) ve `EK-V3`'ün kalan ödeme satırları; `StateBlock`,
`SkeletonBlock`. Taşma davranışı `GS21`. Renk ve ses Planör marka token'larından (`GS7`).

## EK-V6b — Kart formu

> Sayfa dosyası: `CardFormPage.xaml`
> Adım **V6b** iki alt adımda: `V6b1` kartın tanımı + "Ekle" / "Düzenle", `V6b2` gelecek kart
> harcamaları. Kapı A ve B ortak (bütün sayfa), Kapı C her alt adımda ayrı. Davranış kararları: `S63`.
> **V6b tamamlandı:** `V6b1` ve `V6b2` ayrı Kapı C'lerle onaylandı (koyu + açık).
> Not: kart anahtarı ayrıştırıcısı (`EK-V\d+`) harf ekini tanımaz; bu kart GK9'da `EK-V6`'nın
> gövdesi olarak okunur.

**Eski hâl:** `CommitmentsPage.xaml` kart formu bölümü 158 satır, **28 `<Label>`** (ortak ad alanı,
form başlığı, açıklama ve durum mesajıyla ~32), 9 buton, 21 giriş; `CardControlPage.xaml` gelecek
harcamalar bölümü 6 `<Label>`, 4 buton, 3 giriş. ViewModel: `CommitmentsViewModel.Cards` (338 satır),
`.Entry` (`EditCardAsync`), `.Plans` (harcama ekle / sil).

### 1. Sorular

| Kod | Soru | Eskide nasıl cevaplanıyordu |
|---|---|---|
| S1 | "Kartımı nasıl eklerim, bilgilerini nasıl düzeltirim?" | ~12 etiket: ad, banka, limit, kesim, son ödeme, asgari % |
| S2 | "Karta henüz yansımamış hangi taksitler var, hangi ay ne kadar?" | Bölüm başlığı + satır başına 3 etiket + Sil; açıklama hep "Gelecek taksit" |
| S3 | "Yeni bir taksitli alışverişi nasıl eklerim?" | Tarih + tutar + Ekle; taksitler tek tek |
| S4 | "Kartın şu anki borcu ne kadar?" *(kesilmiş ekstre yoksa)* | "Kesilmiş ekstren var mı?" Evet/Hayır + devreden + ekstreleşmemiş + bakiye tarihi: 4 etiket, 2 buton |

### 2. Kesme kararları

| Bilgi / Öğe | Karar | Gerekçe |
|---|---|---|
| Ad, banka, limit, kesim günü, son ödeme günü | **Satır** (form kartı: `Eyebrow` + `Entry`, `Grid *,*`) | S1 · `V6b1` |
| Güncel borç | **Satır** (form kartında, yalnız kesilmiş ekstre yokken) | S4 · `V6b1` · faizsiz ekstreleşmemiş harcama (`S63`-4) |
| Kaydet | **Aksiyon** (`ActionFill`) | S1 · kartı ve harcamaları birlikte yazar |
| Vazgeç | **Aksiyon** (`SecondaryButton`) | S1 · kaydedilmemiş değişiklik varsa onay sorar; geri oku ve cihazın geri tuşu da aynı yoldan geçer (Kapı B) |
| Finansal Yapı başlığında "Ekle" | **Aksiyon** (`PageHeader` metin aksiyonu; ikon değil, `T7`) | `EK-V6` S4 · `V6b1` · tek seçenekli seçici |
| Kart satırı diyaloğunda "Düzenle" | **Satır** (mevcut diyaloğa seçenek) | `EK-V6` S3 · `V6b1` |
| Gelecek harcamalar (açıklama · tarih · tutar) | **Kart** (`ListCard`, ≤ 4 satır + "+N daha" yerinde, `GS21`) | S2 · `V6b2` |
| Harcama girişi | **Kart** ("Gelecek harcama ekle" ile açılan giriş bloğu) | S3 · `V6b2` |
| Harcamayı silme | **Satır** (dokun → diyalog → Sil) | S2 · `V6b2` |
| PDF'den okuma | **Çıkar** | `S21` |
| Ekstre alanları, sonraki tahmini tarihler, bu ekstrenin ödeme planı | **Çıkar** | `EK-V7`'de (`S61`) |
| Varsayılan ödeme şekli, sabit tutar, varsayım | **Çıkar** | `EK-V7` NavRow'unda |
| Asgari %, bakiye tarihi, "Kesilmiş ekstren var mı?" | **Çıkar** | Oran limitten (`I6`); tarih bugün; ekstre varlığı veriden okunur |
| Form açıklama cümlesi, durum mesajı, spinner | **Çıkar** | Başlık yeter; hata diyalogla; `GS14` |
| Limit kullanım çubuğu | **Çıkar** (Kapı B) | `EK-V7` Kapı C aynı çubuğu "hiçbir kararı beslemiyor" diye çıkardı; formda yazarken oynar |
| Harcama toplamı | **Çıkar** | Tarihleri farklı kalemleri toplamak yanıltır (`EK-V6` ile aynı gerekçe) |

### 3. Bütçe

```
Hero rakam    0 / 1     hero yok
Hero yüzey    0 / 1     hero yüzey yok
Kart          3 / 4     form kartı (V6b1), harcama giriş bloğu, harcamalar ListCard (V6b2); analizci yalnız ListCard'ı sayar
Grafik        0 / 1     grafik yok
NavRow        0 / 5     NavRow yok
Label        13 / 28    form 6 (V6b1) + giriş 4 + satır şablonu 3 (V6b2); DataTemplate içi bir kez
Cumle_        0 / 3     açıklama cümlesi yok
```

### 4. Blok şeması

```
┌─ PageHeader ──────────────────────────────────────────────────────┐
│ Baslik_YeniKart | Baslik_KartiDuzenle   TypeTitle / TextPrimary    │  ← S1   üst etiket yok, aksiyon yok
└────────────────────────────────────────────────────────────────────┘
┌─ Form kartı (Border SurfaceCard / RadiusCard / CardPadding) ───────┐  V6b1
│ Etiket_KartAdi               Eyebrow                               │  ← S1
│ [ Entry  "Örn. Bonus, Maximum" ]                                   │
│ Etiket_Banka                 Eyebrow                               │  ← S1
│ [ Entry  "Örn. Garanti BBVA" ]                                     │
│ Etiket_Limit                 Eyebrow                               │  ← S1
│ [ Entry Numeric ]                                 tam genişlik     │
│ Etiket_GuncelBorc            Eyebrow                               │  ← S4 · yalnız ekstresiz kartta
│ [ Entry Numeric "0" ]                             tam genişlik     │
│ Etiket_KesimGunu             Etiket_SonOdemeGunu                   │  ← S1
│ [ Entry Numeric "1–31" ]     [ Entry Numeric "1–31" ]              │
└────────────────────────────────────────────────────────────────────┘
┌─ ListCard  Etiket_GelecekHarcamalar ───────────────────────────────┐  V6b2 · ← S2 · yalnız harcama varsa
│ Telefon (3/12)           TypeBody / TextPrimary        2.000 ₺     │
│ 5 Kasım                  Caption (Tarih)               Figure      │
│   … en fazla 4 satır, tarihe göre; fazlası:                         │
│ +7 daha                  OverflowText (Bicim_FazlaKayit)        ›  │  GS21: yerinde açar
│   dokun → ChooseAsync(açıklama, "Vazgeç", "Sil")                    │
└────────────────────────────────────────────────────────────────────┘
[ Aksiyon_GelecekHarcamaEkle          SecondaryButton ]                  V6b2 · ← S3 · giriş kapalıyken
┌─ Giriş bloğu (Border SurfaceCard / RadiusCard / CardPadding) ──────┐  V6b2 · ← S3 · açıkken
│ Etiket_Aciklama              Eyebrow                               │
│ [ Entry  "Örn. Telefon" ]                                          │
│ Etiket_AylikTutar            Etiket_TaksitSayisi                   │
│ [ Entry Numeric ]            [ Entry Numeric "1" ]                 │
│ Etiket_IlkTaksitTarihi       Eyebrow                               │
│ [ DatePicker  en erken bugün, varsayılan bir ay sonra ]            │
│ [ Aksiyon_Ekle  ActionFill ]  [ Aksiyon_Vazgec  SecondaryButton ]  │
└────────────────────────────────────────────────────────────────────┘
[ Aksiyon_Kaydet  ActionFill ]     [ Aksiyon_Vazgec  SecondaryButton ]    V6b1 · ← S1 · Grid *,*
  Vazgeç / geri oku / geri tuşu → değişiklik varsa
    ConfirmAsync("Kaydetmeden çık", "Yaptığın değişiklikler kaydedilmeyecek.", "Çık", "Kal")
```

Finansal Yapı'daki değişiklik (`EK-V6`, `V6b1`):

```
PageHeader  Baslik_FinansalYapi                          [ Aksiyon_Ekle ]   metin aksiyonu
  Ekle        → ChooseAsync("Ne eklemek istiyorsun?", "Vazgeç", null, "Kredi kartı")
                → Routes.CardForm (kimliksiz)                  V6c–V6e: + kendi seçenekleri
  Kart satırı → ChooseAsync(ad, "Vazgeç", "Sil", "Ödemeyi yönet", "Düzenle")
                Düzenle → Routes.CardForm + cardId
```

Uygulama notları:
- Sayfa `Routes.CardForm` (`card-form`) ile itilir. Kaydedince `NavigateBackAsync`; Finansal Yapı
  görünüşte listeyi yeniler. Vazgeç düğmesi, Shell geri oku (`BackButtonBehavior`) ve cihazın geri
  tuşu aynı `CancelCommand`'a gider; değişiklik yoksa onaysız döner. "Değişiklik", formun yüklendiği
  andaki değerlerle karşılaştırılır (`V6b2`'de harcama listesi de dahil).
- Düzenleme yüklenen kartın üstüne `with` ile kurulur; formun dokunmadığı her alan (ekstre, ekstre
  planı, vade planları, tercih geçmişi, bilinen sonraki tarihler, ödeme şekli, harcamalar `V6b1`'de)
  olduğu gibi kalır.
- Tutar girişi V7'nin Türkçe okuyucusuyla (`I60`); gün alanları 1–31. Ad boş, limit ≤ 0, gün aralık
  dışı → diyalog; kalan kurallar `CreditCardValidator`'da.
- Harcama satırı ham veri taşır (`CardChargeRow`: kimlik, açıklama, tarih, tutar); tarih metni
  mevcut `Tarih` converter'ıyla kurulur. Taşma `FazlaKayitConverter` ile.
- Yeni bileşen yok. Yeni converter yok.
- `V6b1` uygulaması: sayfa ViewModel'i (`CardFormViewModel`) yükleme, kaydetme, vazgeçme ve "Tekrar
  dene"yi (`RetryCommand`: aynı kartı yeniden yükler) yürütür; alanlar, doğrulama ve kartı kurma
  çocuk `CardDefinitionViewModel`'dedir (200 satır sınırı). `V6b2` harcamaları ikinci bir çocukla
  ekler. Form kimlikleri `RecordFormAutomationIds`'te (`AutomationIds` dosya sınırında); kimlik
  testleri ad alanındaki bütün sınıfları birlikte denetler.
- `V6b2` uygulaması: harcamalar ikinci çocuk `CardChargesViewModel`'de (liste, taşma, giriş, silme,
  değişiklik izleme); sayfa ViewModel'i yüklemede doldurur, kaydederken kartla birlikte yazar.
  Satır şablonu `ContentPage.Resources`'ta (`HarcamaSatiri`): `ListCard.ItemTemplate` iç içe
  yazılırsa bütçe analizcisi onu da kart sayar.

### 5. Üç durum

| Durum | Görünen |
|---|---|
| Boş | Yeni kart: form boş alanlarla açılır (StateBlock yok). Harcama yoksa `ListCard` gizlidir, yalnız "Gelecek harcama ekle" görünür. |
| Yükleniyor | Düzenlemede üç `SkeletonBlock`; spinner yok (`GS14`). Yeni kartta yükleme yok. |
| Hata | Kart okunamazsa `StateBlock` (Hata, `Close` ikonu, `Hata_KartYuklenemedi`, `Aksiyon_TekrarDene` → yükle). Kart bulunamazsa diyalog ve geri dönüş. Kaydetme hatasında diyalog; form olduğu gibi kalır. |

### 6. Konsept ilişkisi

Konsept karşılığı **yok** (`GS2`). Form satırları `EK-V4` Adım 3'ten (`Eyebrow` + `Entry`, `Grid *,*`),
giriş bloğu ve Kaydet / Vazgeç düzeni `EK-V7` Hâl C'den, harcama satırı ve taşma `EK-V6` satır
şablonundan ve `GS21`'den. Renk ve ses Planör marka token'larından (`GS7`).

## EK-V6c — Kredi formu

> Sayfa dosyası: `LoanFormPage.xaml`
> Adım **V6c** üç alt adımda: `V6c1` kredinin tanımı + "Ekle" / "Düzenle", `V6c2` faiz ve bugün
> kapatma bedeli, `V6c3` planlı erken ödemeler. Kapı A ve B ortak (bütün sayfa), Kapı C her alt
> adımda ayrı. Davranış kararları: `S64`.
> **V6c1 tamamlandı** (Kapı C onaylı, koyu + açık): form kartı, Kaydet / Vazgeç, Finansal Yapı'da
> "Ekle → Kredi" ve kredi satırında "Düzenle".
> **V6c2 tamamlandı** (Kapı C onaylı, koyu + açık): faiz kartı — kalan anapara ve bankanın kapatma
> tutarı, canlı bedel / ücret / kurtulunan faiz / aylık faiz (`S64`-9–11, `I68`, `I69`).
> **V6c3 tamamlandı** (Kapı C onaylı, koyu + açık; `S64`-12–16, `I70`–`I72`): erken ödeme listesi, girişi ve
> silmesi; kredi ile tek işlemde kayıt. Bir kez elendi, simülatörden doğan kaydı gösterecek yer kalmadığı
> için geri alındı. Kapı C: tarih penceresinin düğmeleri platform temasında düzeltildi. **Kart tamamlandı.**
> Not: kart anahtarı ayrıştırıcısı (`EK-V\d+`) harf ekini tanımaz; bu kart GK9'da `EK-V6`'nın
> gövdesi olarak okunur (`EK-V6b` ile aynı).

**Eski hâl:** `CommitmentsPage.xaml` kredi formu bölümü 35 satır, **11 `<Label>`** (ortak başlık,
açıklama ve ad alanıyla 14), 9 giriş; Krediler bölümünde satır başına faiz / kapatma cümlesi ve
erken ödeme satırları (4 etiket + not cümlesi + Sil); erken ödeme girişi `ScenarioConditionFormView`
içinde 8 etiket, 6 giriş. ViewModel: `CommitmentsViewModel.Loans` (120 satır), `.Operations`
(kredi ve erken ödeme satırları, silme), `ScenarioConditionForm` (403 satır / 2 dosya, ortak).

### 1. Sorular

| Kod | Soru | Eskide nasıl cevaplanıyordu |
|---|---|---|
| S1 | "Kredimi nasıl eklerim; taksit, kalan taksit ve sonraki ödemeyi nasıl düzeltirim?" | 14 etiket, 9 giriş; ödeme günü ve tarih ayrı iki alan |
| S2 | "Bu kredinin faizi ne; bugün kapatırsam ne öderim, ne kadar faizden kurtulurum?" | Listede satır başına tek etiketlik iki satırlık cümle; formda 3 yardım cümlesi |
| S3 | "Planlı erken ödemelerim neler, o gün ne kadar ödeyeceğim?" | Krediler listesinde ayrı satırlar: 4 etiket + not cümlesi + Sil |
| S4 | "Yeni bir erken ödeme ya da kapamayı nasıl planlarım?" | "+ Ekle" → ortak senaryo formu (8 etiket, 6 giriş) ya da simülatör |

### 2. Kesme kararları

| Bilgi / Öğe | Karar | Gerekçe |
|---|---|---|
| Ad, banka, aylık taksit, kalan taksit, sonraki taksit tarihi, kredi türü | **Satır** (form kartı: `Eyebrow` + giriş, `Grid *,*`) | S1 · `V6c1` |
| Kaydet / Vazgeç | **Aksiyon** (`ActionFill` / `SecondaryButton`) | S1 · kredi ve erken ödemeleri birlikte yazar; değişiklik varsa çıkış onayı |
| Finansal Yapı "Ekle"de "Kredi", kredi satırında "Düzenle" | **Aksiyon / Satır** (mevcut diyaloglara seçenek) | `EK-V6` S3, S4 · `V6c1` |
| Kalan anapara, bankanın kapatma tutarı | **Satır** (faiz kartında) | S2 · `V6c2` · ikisi de isteğe bağlı |
| Aylık faiz, bugün ödenecek, erken ödeme ücreti, kurtulunacak faiz | **Kart** (faiz kartı, `MetricRow`'lar, canlı) | S2 · `V6c2` |
| Faiz hesaplanamadığında ne yapmalı | **Satır** (faiz kartında tek `Cumle_`, iki varyant) | S2 · `V6c2` |
| Planlı erken ödemeler (şekil · tarih · o gün ödenecek tutar), simülatörden uygulananlar dahil | **Kart** (`ListCard`, ≤ 4 satır + "+N daha" yerinde, `GS21`) | S3 · `V6c3` · `S64`-13 |
| Erken ödeme girişi (şekil, tarih, ara ödemede tutar) | **Kart** ("Erken ödeme planla" ile açılan giriş bloğu) | S4 · `V6c3` |
| Erken ödemeyi silme | **Satır** (dokun → diyalog → Sil) | S3 · `V6c3` |
| Tutar "—" iken açıklama cümlesi | **Çıkar** | Sebebi faiz kartındaki iki cümle zaten söylüyor (`S64`-14) |
| Ara ödemede girilen anaparanın satırda ayrıca görünmesi | **Çıkar** | Satır o gün ödenecek toplamı gösterir: anapara + gün faizi + varsa ücret (S3) |
| Erken ödemenin kazancı (kurtulunan faiz, yeni bitiş / yeni taksit) | **Derine** → `V10` | Simülatörün sorusu (`S64`-16) |
| Ayrı "Ödeme günü" alanı | **Çıkar** | Tarihten çözülür (`S64`-2) |
| Form açıklaması, 3 yardım cümlesi, "kayıtlı tutar şu tarihli" notu | **Çıkar** | Faiz kartının tepkisi ve hata diyaloğu yeter |
| "banka tutarından" kaynak işareti, kalan anapara satırı | **Çıkar** | Kararı değiştirmiyor; anapara giriş alanıyla aynı bilgi |
| Başarı mesajı, spinner, durum satırı | **Çıkar** | Listeye dönüş yeter; `GS14` |
| Erken ödeme "plan adı", "Erken ödeme" rozeti, "Simülatörden uygulandı" notu | **Çıkar** | Kaydın adı yok; satır zaten kredinin sayfasında |
| Erken kapama önerisi (en kârlı gün) | **Derine** → `V8` | Eskide 12 dönem ekranındaydı |
| Hero rakam | **Yok** | Form sayfası; bugün ödenecek tutar faiz kartında satır |

### 3. Bütçe

```
Hero rakam    0 / 1     hero yok
Hero yüzey    0 / 1     hero yüzey yok
Kart          4 / 4     form kartı (V6c1), faiz kartı (V6c2), erken ödemeler ListCard + giriş bloğu (V6c3); analizci yalnız ListCard'ı sayar
Grafik        0 / 1     grafik yok
NavRow        0 / 5     NavRow yok
Label        16 / 28    form 6 (V6c1) + faiz kartı 4 (V6c2) + satır şablonu 3 + giriş 3 (V6c3); DataTemplate içi bir kez (şablon ContentPage.Resources'ta)
Cumle_        2 / 3     Cumle_FaizIcinTutarGir, Cumle_TutarUyusmuyor (aynı anda biri görünür)
```

### 4. Blok şeması

```
┌─ PageHeader ──────────────────────────────────────────────────────┐
│ Baslik_YeniKredi | Baslik_KrediyiDuzenle  TypeTitle / TextPrimary  │  ← S1   üst etiket yok, aksiyon yok
└────────────────────────────────────────────────────────────────────┘
┌─ Form kartı (Border SurfaceCard / RadiusCard / CardPadding) ───────┐  V6c1
│ Etiket_KrediAdi              Eyebrow                               │  ← S1
│ [ Entry  "Örn. İhtiyaç kredisi" ]                                  │
│ Etiket_Banka                 Eyebrow                               │  ← S1
│ [ Entry  "Örn. Ziraat Bankası" ]                                   │
│ Etiket_AylikTaksit           Etiket_KalanTaksit                    │  ← S1
│ [ Entry Numeric ]            [ Entry Numeric "Örn. 24" ]           │
│ Etiket_SonrakiTaksit         Etiket_KrediTuru                      │  ← S1
│ [ DatePicker dd.MM.yyyy ]    [ Picker  İhtiyaç / taşıt ▾ ]         │
└────────────────────────────────────────────────────────────────────┘
┌─ Faiz kartı (Border SurfaceCard / RadiusCard / CardPadding) ───────┐  V6c2 · Payoff çocuğu
│ Etiket_KalanAnapara          Eyebrow  "KALAN ANAPARA"              │  ← S2
│ [ Entry Numeric  YerTutucu_IstegeBagli "İsteğe bağlı" ] tam gen.   │  Payoff.PrincipalInput
│ Etiket_KapatmaTutari         Eyebrow  "BANKANIN KAPATMA TUTARI"    │  ← S2
│ [ Entry Numeric  YerTutucu_IstegeBagli "İsteğe bağlı" ] tam gen.   │  Payoff.ClosureInput
│ ── Hâl A: faiz çözüldü (IsResolved) ──                             │
│ MetricRow  Etiket_BugunKapatirsan  "Bugün kapatırsan"   150.230 ₺  │  ← S2  Para · yalnız HasPayoff
│ MetricRow  Etiket_ErkenOdemeUcreti "Erken ödeme ücreti"   1.489 ₺  │  ← S2  Para · yalnız HasFee (ücret > 0)
│ MetricRow  Etiket_KurtulacaginFaiz "Kurtulacağın faiz"   33.770 ₺  │  ← S2  Para · Semantic=Positive · yalnız HasInterestSaving (> 0)
│ MetricRow  Etiket_AylikFaiz  "Aylık faiz (vergi dahil)"     %2,79  │  ← S2  Yuzde · her zaman
│ ── Hâl B: geçerli tutar yok (NeedsAmount) ──                       │
│ Cumle_FaizIcinTutarGir       Caption / TextSecondary               │  ← S2
│   "Faizi görmek için kalan anaparayı ya da bankanın kapatma tutarını gir."
│ ── Hâl C: tutar var, taksit bilgisi tam, faiz çözülemedi (IsMismatch) ──
│ Cumle_TutarUyusmuyor         Caption / TextSecondary               │  ← S2
│   "Bu tutar taksitlerle uyuşmuyor; tutarı ve taksit bilgilerini kontrol et."
│ ── Tutar var ama aylık taksit / kalan taksit geçersiz: yalnız iki giriş ──
└────────────────────────────────────────────────────────────────────┘
┌─ ListCard  Etiket_ErkenOdemeler  "ERKEN ÖDEMELER" ─────────────────┐  V6c3 · ← S3 · yalnız kayıt varsa
│ Tamamen kapatma          TypeBody / TextPrimary      52.410 ₺      │  ← S3  şekil (ErkenOdemeTuruConverter)
│ 15 Mart 2027             Caption (Tarih, "d MMMM yyyy")  Figure    │  ← S3  o gün ödenecek; hesaplanamıyorsa "—"; yıl var: plan bir yıldan uzak olabilir
│ Ara ödeme · vade kısalır                             20.180 ₺      │
│ 15 Ocak 2027                                                       │
│   … tarihe göre, en fazla 4 satır; fazlası: +N daha, yerinde açılır (GS21)
│   dokun → ChooseAsync(şekil, "Vazgeç", "Sil")                       │  ← S3  silme Kaydet'e kadar bekler
└────────────────────────────────────────────────────────────────────┘
[ Aksiyon_ErkenOdemePlanla "Erken ödeme planla"  SecondaryButton ]       V6c3 · ← S4 · giriş kapalıyken
┌─ Giriş bloğu (Border SurfaceCard / RadiusCard / CardPadding) ──────┐  V6c3 · ← S4 · açıkken
│ Etiket_OdemeSekli  "ÖDEME ŞEKLİ"           Eyebrow, tam genişlik   │  ← S4  önce şekil: tutarın sorulup sorulmayacağını o belirler
│ [ Picker  Tamamen kapatma ▾ ]              tam genişlik            │        Tamamen kapatma · Ara ödeme · vade kısalır · Ara ödeme · taksit düşer
│ Etiket_OdemeTarihi "ÖDEME TARİHİ"   Etiket_AnaparadanDusecek        │  ← S4  Grid *,*; sağ sütun yalnız ara ödemede
│ [ DatePicker ≥ bugün ]              [ Entry Numeric "0" ]           │        tarih varsayılanı: bugünden sonraki ilk taksit günü
│ [ Aksiyon_Ekle  ActionFill ]  [ Aksiyon_Vazgec  SecondaryButton ]  │  ← S4  Ekle kaydın kurallarıyla doğrular; hata → diyalog, giriş açık kalır
└────────────────────────────────────────────────────────────────────┘
[ Aksiyon_Kaydet  ActionFill ]     [ Aksiyon_Vazgec  SecondaryButton ]    V6c1 · ← S1 · Grid *,*
  Vazgeç / geri oku / geri tuşu → değişiklik varsa
    ConfirmAsync("Kaydetmeden çık", "Yaptığın değişiklikler kaydedilmeyecek.", "Çık", "Kal")
```

Finansal Yapı'daki değişiklik (`EK-V6`, `V6c1`):

```
Ekle         → ChooseAsync("Ne eklemek istiyorsun?", "Vazgeç", null, "Kredi kartı", "Kredi")
                 Kredi → Routes.LoanForm (kimliksiz)
Kredi satırı → ChooseAsync(ad, "Vazgeç", "Sil", "Düzenle")
                 Düzenle → Routes.LoanForm + loanId
```

Uygulama notları:
- Sayfa `Routes.LoanForm` (`loan-form`) ile itilir; kaydedince `NavigateBackAsync`. Vazgeç, Shell
  geri oku (`BackButtonBehavior`) ve cihazın geri tuşu aynı `CancelCommand`'a gider (`EK-V6b` deseni).
  "Değişiklik" formun yüklendiği andaki değerlerle karşılaştırılır (erken ödeme listesi dahil).
- Düzenleme yüklenen kredinin üstüne `with` ile kurulur (`S64`-3). Ödeme günü sonraki taksit
  tarihinin günüdür; kayıtlı gün o ayda aynı tarihe kenetleniyorsa korunur (`S64`-2).
- Tutar girişi V7'nin Türkçe okuyucusuyla (`I60`). Ad boş, taksit ≤ 0, kalan taksit < 1 → diyalog;
  kalan kurallar `SaveLoanAsync` / `PrepareForSave`'in Türkçe mesajlarıyla diyalogda.
- Kredi türü `Picker`'ı ham `LoanKind` değerleri taşır; görünen ad App'teki `KrediTuruConverter`'da
  (`ItemDisplayBinding`). Alan yarım genişlik olduğu için adlar kısa: "İhtiyaç / taşıt", "Konut, sabit",
  "Konut, değişken" (başlık zaten "Kredi türü"; sabit / değişken faizin türüdür). `DatePicker` ve `Picker` renkleri `EK-V6b`'deki `DatePicker` gibi satır içi
  `DynamicResource` (`SurfaceSunken`, `TextPrimary`) ile bağlanır.
- Faiz kartı (`V6c2`): `LoanPayoffViewModel` çocuğu (`LoanFormViewModel.Payoff`); formun ya da iki
  tutarın her değişikliğinde taslak krediden `IObligationManagementService.PreviewLoan` ile
  hesaplanır (`S64`-9: kaydın kurallarıyla, hata fırlatmadan; kayıt reddedecekse null). Kapatma
  tutarı değiştiyse tarihsiz gider, servis bugünle damgalar; değişmediyse kendi tarihini taşır.
  Taslağa ad gerekmez; aylık taksit > 0 ve kalan taksit ≥ 1 yeter. Aylık faiz `YuzdeConverter`'la
  ("%2,79"). ViewModel ham veri sunar: `MonthlyRate`, `PayoffAmount`, `Fee`, `InterestSaving`
  (`decimal?`) ve `IsResolved`, `HasPayoff`, `HasFee`, `HasInterestSaving`, `NeedsAmount`,
  `IsMismatch` (`bool`). Satır sırası önem sırasıdır: bedel, içindeki ücret, kurtulunan faiz,
  aylık faiz. Kapatılacak taksit yoksa yalnız aylık faiz (`S64`-11).
- Açılışta yalnız bir tutar dolu gelir (`S64`-4): kapatma tutarı güncelse o, değilse kalan anapara.
  Kaydet: kapatma tutarı geçerliyse o yazılır (kalan anaparayı servis tutardan çözer); yalnız
  anapara varsa o; ikisi boşsa ikisi de temizlenir. Geçersiz tutar ("abc", 0, eksi) diyalogla
  reddedilir. Bayat tutarı düşürme `LoanDefinitionViewModel`'den bu çocuğa taşınır (`S64`-10).
  `HasChanges` iki tutarı da sayar.
- Erken ödemeler (`V6c3`): `LoanPrepaymentsViewModel` çocuğu (`LoanFormViewModel.Prepayments`,
  `CardChargesViewModel` deseni; portu, diyaloğu ve saati ebeveynden alır, form 5 bağımlılıkta kalır).
  Satır ham veri taşır (`LoanPrepaymentRow`: kimlik, şekil, tarih, anaparadan düşecek, o gün ödenecek);
  şekil metni `ErkenOdemeTuruConverter`'da, seçici de aynı çeviriciden geçer (`KrediTuruConverter`
  deseni). Satır şablonu `ContentPage.Resources`'ta (`EK-V6b` notu: iç içe `ListCard.ItemTemplate`
  bütçe analizcisinde ikinci kart sayılır).
- Yükleme: düzenlemede kredinin bütün kayıtlı erken ödemeleri, kaynağı fark etmeksizin (simülatör
  dahil, `S64`-13). Tutarlar formun ya da faiz kartındaki iki tutarın her değişikliğinde taslak
  krediyle `IObligationManagementService.PreviewLoanPrepayments`'tan yeniden gelir (`S64`-14);
  taslak yoksa ya da kayıt reddedecekse "—".
- "Ekle": ara ödemede tutar > 0, tarih ≥ bugün, taslak kredi var (yoksa "Önce aylık taksiti ve kalan
  taksiti gir."); sonra `ValidateLoanPrepayment` kaydın mesajını döner. Hata → diyalog
  ("Erken ödeme eklenemedi"), giriş açık kalır. Kaydet: `SaveLoanAsync(kredi, erken ödemeler)`;
  reddedilen erken ödemenin tarihi mesajın başında, diyalog "Kredi kaydedilemedi".
- `HasChanges` erken ödeme listesini de sayar (kimlik kümesi yüklendiği andakiyle aynı mı).
- Yeni bileşen yok. Yeni converter: `KrediTuruConverter` (`V6c1`), `YuzdeConverter` (`V6c2`),
  `ErkenOdemeTuruConverter` (`V6c3`).

### 5. Üç durum

| Durum | Görünen |
|---|---|
| Boş | Yeni kredi: form boş alanlarla açılır (sonraki taksit bir ay sonrası, tür İhtiyaç / taşıt); faiz kartında `Cumle_FaizIcinTutarGir`; erken ödeme yoksa `ListCard` gizli, yalnız "Erken ödeme planla" görünür. StateBlock yok. |
| Yükleniyor | Düzenlemede üç `SkeletonBlock`; spinner yok (`GS14`). Yeni kredide yükleme yok. |
| Hata | Kredi okunamazsa `StateBlock` (Hata, `Close` ikonu, `Hata_KrediYuklenemedi`, `Aksiyon_TekrarDene` → yükle). Kredi bulunamazsa diyalog ve geri dönüş. Kaydetme hatasında diyalog; form olduğu gibi kalır. |

### 6. Konsept ilişkisi

Konsept karşılığı **yok** (`GS2`). Form, erken ödeme `ListCard`'ı, giriş bloğu ve Kaydet / Vazgeç
düzeni `EK-V6b`'den; faiz kartındaki `MetricRow` satırları `EK-V3`'ten (`GS20`); kararın bedelini
gösterme fikri `EK-V7`'den. Renk ve ses Planör marka token'larından (`GS7`).

## EK-V6d — Gelir formu

> Sayfa dosyaları: `IncomeFormPage.xaml` (düzenli gelir), `AdHocIncomeFormPage.xaml` (tek seferlik gelir)
> Adım **V6d** üç alt adımda: `V6d1` düzenli gelirin tanımı + "Ekle" / "Düzenle", `V6d2` tutar
> değişiklikleri, `V6d3` tek seferlik gelir. Kapı A ve B ortak (iki sayfa), Kapı C her alt adımda ayrı.
> Davranış kararları: `S67`.
> **V6d1, V6d2 ve V6d3 tamamlandı** (Kapı C onaylı): form kartı (gelir adı, yeni gelirde aylık net tutar, ödeme günü),
> tutar değişiklikleri listesi (yürürlükteki ve ileri tarihliler), yeni tutar girişi ve planlı tutar silme,
> tek seferlik gelir formu (açıklama, tutar, tarih), Kaydet / Vazgeç, Finansal Yapı'da "Ekle → Düzenli gelir / Tek seferlik gelir"
> ve gelir satırında "Düzenle"; gelir ve tutarları tek işlemde yazılır, tutar yatış gününe göre çözülür (`S67`-2–6, `S67` V6d2 notları, `I4`, `I78`, `I79`).
> Not: kart anahtarı ayrıştırıcısı (`EK-V\d+`) harf ekini tanımaz; bu kart GK9'da `EK-V6`'nın
> gövdesi olarak okunur (`EK-V6b`, `EK-V6c` ile aynı).

**Eski hâl:** `CommitmentsPage.xaml` düzenli gelir formu 3 `<Label>` (ortak başlık, açıklama, "Ne eklemek
istiyorsun?" ve durum mesajıyla ~7), 2 giriş + tarih; tek seferlik gelir ortak senaryo formundan
(`ScenarioConditionFormView`, 4 `<Label>`, ortaklarla ~7; 3 giriş). Düzenleme yoktu. ViewModel:
`CommitmentsViewModel` (gelir payı ~35 satır), `ScenarioConditionForm` (403 satır / 2 dosya, ortak).

### 1. Sorular

| Kod | Soru | Eskide nasıl cevaplanıyordu |
|---|---|---|
| S1 | "Bu gelir ne, ayın kaçında yatıyor, ne kadar?" | ~7 etiket; gün sorulmuyordu (`S3`), düzenleme yoktu |
| S2 | "Şu an ne kadar alıyorum, ileride ne zaman değişecek?" | Cevapsız: her tutar ayrı "gelir" satırıydı, zam ile ikinci gelir ayırt edilemiyordu (`S2`) |
| S3 | "Bir zammı nasıl girerim, vazgeçersem nasıl silerim?" | Aynı formdan yeni kayıt; silme listedeki Sil düğmesi |
| S4 | "Tek seferlik bir geliri nasıl eklerim, nasıl düzeltirim?" | Ortak senaryo formu, "Plan adı" diliyle ~7 etiket; düzenleme yok |

### 2. Kesme kararları

| Bilgi / Öğe | Karar | Gerekçe |
|---|---|---|
| Gelir adı, aylık net tutar (yalnız yeni gelirde), ödeme günü | **Satır** (form kartı: `Eyebrow` + `Entry`, kurulumdaki sıra ve adlar) | S1 · `V6d1` |
| Kaydet / Vazgeç | **Aksiyon** (`ActionFill` / `SecondaryButton`) | S1 · gelir ve tutarları tek işlemde yazar; değişiklik varsa çıkış onayı |
| Finansal Yapı "Ekle"de "Düzenli gelir", "Tek seferlik gelir"; gelir satırında "Düzenle" | **Aksiyon / Satır** (mevcut diyaloglara seçenek) | `EK-V6` S3, S4 · `V6d1`, `V6d3` |
| Yürürlükteki ve ileri tarihli tutarlar (tarih · tutar), simülatörden gelenler dahil | **Kart** (`ListCard`, ≤ 4 satır + "+N daha" yerinde, `GS21`) | S2 · `V6d2` · yalnız düzenlemede |
| Tutar değişikliği girişi (yeni tutar, geçerlilik tarihi) | **Kart** ("Tutar değişikliği ekle" ile açılan giriş bloğu) | S3 · `V6d2` |
| Planlı değişikliği silme | **Satır** (dokun → diyalog → Sil; yürürlüğe girmiş satırda bilgi diyaloğu) | S3 · `V6d2` |
| Tek seferlik gelir: açıklama, tutar, tarih | **Satır** (ayrı sayfada form kartı) | S4 · `V6d3` |
| Yürürlükten kalkmış tutarlar | **Çıkar** | Veride kalır (kural 05); geçmiş `V12`'nin işi |
| Tutar kaydının açıklaması ("Başlangıç Tutarı", simülatör adı), "Planlanan gelir" rozeti | **Çıkar** | Kararı değiştirmiyor; tarih zaten söylüyor (`EK-V6c`'deki "Simülatörden uygulandı" gibi) |
| Form açıklaması, "Plan adı", başarı ve durum mesajı, spinner | **Çıkar** | Başlık yeter; hata diyalogla; `GS14` |
| Toplam / yıllık gelir | **Çıkar** | Dönem toplamı `EK-V3` / `EK-V9`'un işi |
| Geliri sonlandırma (pasife alma, bitiş tarihi) | **Çıkar** | Pasife alan bir yol yok; gelir bitince listeden silinir |
| Geçerlilik tarihinin anlamını anlatan cümle | **Çıkar** | Tutar yatış gününe göre çözülür (`S67`-4); tarih yatışla aynı anlamda |
| Hero rakam | **Yok** | Form sayfası |

### 3. Bütçe

`IncomeFormPage.xaml` (V6d1 + V6d2):

```
Hero rakam    0 / 1     hero yok
Hero yüzey    0 / 1     hero yüzey yok
Kart          3 / 4     form kartı (V6d1), tutarlar ListCard + giriş bloğu (V6d2); analizci yalnız ListCard'ı sayar
Grafik        0 / 1     grafik yok
NavRow        0 / 5     NavRow yok
Label         7 / 28    form 3 (V6d1) + satır şablonu 2 + giriş 2 (V6d2); DataTemplate içi bir kez (şablon ContentPage.Resources'ta)
Cumle_        0 / 3     açıklama cümlesi yok
```

`AdHocIncomeFormPage.xaml` (V6d3):

```
Hero rakam    0 / 1     hero yok
Hero yüzey    0 / 1     hero yüzey yok
Kart          1 / 4     form kartı
Grafik        0 / 1     grafik yok
NavRow        0 / 5     NavRow yok
Label         3 / 28    açıklama, tutar, tarih
Cumle_        0 / 3     açıklama cümlesi yok
```

### 4. Blok şeması

`IncomeFormPage.xaml`:

```
┌─ PageHeader ──────────────────────────────────────────────────────┐
│ Baslik_YeniGelir | Baslik_GeliriDuzenle  TypeTitle / TextPrimary   │  ← S1   üst etiket yok, aksiyon yok
└────────────────────────────────────────────────────────────────────┘
┌─ Form kartı (Border SurfaceCard / RadiusCard / CardPadding) ───────┐  V6d1
│ Etiket_GelirAdi  "GELİR ADI"          Eyebrow                      │  ← S1
│ [ Entry  "Örn. Kira geliri, emekli aylığı" ]       tam genişlik    │
│ Etiket_AylikNetTutar  "AYLIK NET TUTAR"   Eyebrow  · yalnız yeni   │  ← S1  ilk tutar bugünden yürürlükte
│ [ Entry Numeric "0" ]                              tam genişlik    │
│ Etiket_OdemeGunu  "ÖDEME GÜNÜ"         Eyebrow                     │  ← S1
│ [ Entry Numeric "1–31" ]                           tam genişlik    │
└────────────────────────────────────────────────────────────────────┘
┌─ ListCard  Etiket_AylikNetTutar  "AYLIK NET TUTAR" ────────────────┐  V6d2 · ← S2 · yalnız düzenlemede
│ 28 Eylül 2026 itibarıyla   TypeBody (Bicim_Itibariyla)  45.000 ₺   │  ← S2  yürürlükteki tutar
│ 15 Ocak 2027 itibarıyla                                 50.000 ₺   │  ← S2  ileri tarihli; Figure
│   … tarihe göre, en fazla 4 satır; fazlası: +N daha, yerinde açılır (GS21)
│   dokun (bugün ya da sonra) → ChooseAsync("Tutar değişikliği",     │  ← S3  silme Kaydet'e kadar bekler
│                               "Vazgeç", "Sil")                     │
│   dokun (bugünden önce)     → ShowAlertAsync: yürürlüğe girmiş     │
│                               tutar silinmez, yeni tutar ekle      │
└────────────────────────────────────────────────────────────────────┘
[ Aksiyon_TutarDegisikligiEkle "Tutar değişikliği ekle"  SecondaryButton ]  V6d2 · ← S3 · giriş kapalıyken
┌─ Giriş bloğu (Border SurfaceCard / RadiusCard / CardPadding) ──────┐  V6d2 · ← S3 · açıkken
│ Etiket_YeniTutar "YENİ TUTAR"   Etiket_GecerlilikTarihi "GEÇERLİLİK TARİHİ" │  Grid *,*
│ [ Entry Numeric "0" ]           [ DatePicker ≥ bugün ]             │  ← S3  varsayılan: bugünden sonraki ödeme günü
│ [ Aksiyon_Ekle  ActionFill ]  [ Aksiyon_Vazgec  SecondaryButton ]  │  ← S3  hata → diyalog, giriş açık kalır
└────────────────────────────────────────────────────────────────────┘
[ Aksiyon_Kaydet  ActionFill ]     [ Aksiyon_Vazgec  SecondaryButton ]    V6d1 · ← S1 · Grid *,*
  Vazgeç / geri oku / geri tuşu → değişiklik varsa
    ConfirmAsync("Kaydetmeden çık", "Yaptığın değişiklikler kaydedilmeyecek.", "Çık", "Kal")
```

`AdHocIncomeFormPage.xaml` (V6d3):

```
┌─ PageHeader ──────────────────────────────────────────────────────┐
│ Baslik_TekSeferlikGelir | Baslik_GeliriDuzenle  TypeTitle          │  ← S4   üst etiket yok, aksiyon yok
└────────────────────────────────────────────────────────────────────┘
┌─ Form kartı (Border SurfaceCard / RadiusCard / CardPadding) ───────┐
│ Etiket_Aciklama  "AÇIKLAMA"            Eyebrow                     │  ← S4  zorunlu
│ [ Entry  "Örn. Yıl sonu primi, vergi iadesi" ]     tam genişlik    │
│ Etiket_Tutar "TUTAR"            Etiket_Tarih "TARİH"               │  ← S4  Grid *,*
│ [ Entry Numeric "0" ]           [ DatePicker ≥ bugün, varsayılan bugün ] │
└────────────────────────────────────────────────────────────────────┘
[ Aksiyon_Kaydet  ActionFill ]     [ Aksiyon_Vazgec  SecondaryButton ]    ← S4 · Grid *,*
  Vazgeç / geri oku / geri tuşu → değişiklik varsa aynı onay
```

Finansal Yapı'daki değişiklik (`EK-V6`):

```
Ekle           → ChooseAsync("Ne eklemek istiyorsun?", "Vazgeç", null,
                             "Düzenli gelir", "Tek seferlik gelir", "Kredi kartı", "Kredi")
                   listenin grup sırası; "Düzenli gelir" V6d1'de, "Tek seferlik gelir" V6d3'te gelir
                   Düzenli gelir → Routes.IncomeForm · Tek seferlik gelir → Routes.AdHocIncomeForm
Gelir satırı   → ChooseAsync(ad, "Vazgeç", "Sil", "Düzenle")
                   düzenli: Routes.IncomeForm + incomeId (V6d1) · tek seferlik: Routes.AdHocIncomeForm + adHocIncomeId (V6d3)
```

Uygulama notları:
- Rotalar: `Routes.IncomeForm` (`income-form`) + `IncomeIdParameter` (`incomeId`); `Routes.AdHocIncomeForm`
  (`ad-hoc-income-form`) + `AdHocIncomeIdParameter` (`adHocIncomeId`). Kaydedince `NavigateBackAsync`.
  Vazgeç, Shell geri oku (`BackButtonBehavior`) ve cihazın geri tuşu aynı `CancelCommand`'a gider (`EK-V6b`).
- Tutar girişi V7'nin Türkçe okuyucusuyla (`I60`). Ad boş, tutar ≤ 0, gün 1–31 dışı → diyalog.
- Yeni gelirin ilk tutarı bugünden yürürlüğe girer, açıklaması boş. Düzenleme yüklenen gelirin üstüne `with`
  ile kurulur: aktiflik ve tutar geçmişi korunur (`I73`). Ödeme günü değişince tutarlar değişmez.
- Kaydet `IIncomePlanService.SaveRecurringIncomeAsync(gelir, eklenecek tutarlar[, silinecek kimlikler])`
  ile tek işlem, tek plan revizyonu (`S67`-3). Tutar yatış gününe göre çözülür (`S67`-4, `I4`).
- Tutarlar (`V6d2`): `IncomeAmountsViewModel` çocuğu (`CardChargesViewModel` deseni; diyaloğu ve saati
  ebeveynden alır, port gerekmez, form 5 bağımlılıkta kalır). Satır ham veri taşır (`IncomeAmountRow`: kimlik,
  geçerlilik tarihi, tutar, silinebilir mi). Liste: bugün yürürlükteki son tutar + ileri tarihliler; hiçbiri
  yürürlükte değilse (ileride başlayan gelir) yalnız ileri tarihliler. Silinebilir = geçerlilik tarihi ≥ bugün.
  Aynı tarihte ikinci tutar "Ekle"de reddedilir; "en az bir tutar" Kaydet'te (`S67` V6d2 notları a). Kurallar
  Domain'de `IncomeAmountRules`'ta, ekran ve servis paylaşır. Varsayılan tarih bugünden sonraki ilk ödeme günü,
  kayıtlı gelirin gününe göre. Silme diyaloğunun başlığı sabit "Tutar değişikliği" (ViewModel tarih biçimlendirmez).
  Satır şablonu `ContentPage.Resources`'ta. Tarih metni `Bicim_Itibariyla` ("{0} itibarıyla"; ek uyumu
  gerektirmediği için "…'den itibaren" değil). Tutar bölümü yalnız düzenlemede görünür.
- `HasChanges` tutar listesini de sayar (eklenen ya da silinen kimlik var mı).
- Tek seferlik gelir (`V6d3`): düzenleme `with`; tarih en erken bugün (liste yalnız bugün ve sonrasını gösterir,
  `S62`-5), varsayılan bugün. `SaveAdHocIncomeAsync` (mevcut).
- Metinler yeni `IncomeFormStrings.json`'da (iki sayfa ortak). "maaş" geçmez (`S11`).
- Yeni bileşen yok. Yeni converter yok (`Tarih`, `Para` mevcut).

### 5. Üç durum

| Durum | Görünen |
|---|---|
| Boş | Yeni gelir: form boş alanlarla açılır (StateBlock yok); tutar listesi ve "Tutar değişikliği ekle" yeni gelirde görünmez. Tek seferlik: boş form, tarih bugün. |
| Yükleniyor | Düzenlemede üç `SkeletonBlock`; spinner yok (`GS14`). Yeni kayıtta yükleme yok. |
| Hata | Gelir okunamazsa `StateBlock` (Hata, `Close` ikonu, `Hata_GelirYuklenemedi`, `Aksiyon_TekrarDene` → yükle). Gelir bulunamazsa diyalog ve geri dönüş. Kaydetme hatasında diyalog; form olduğu gibi kalır. |

### 6. Konsept ilişkisi

Konsept karşılığı **yok** (`GS2`). Form kartı, alan sırası ve adları `EK-V4` Adım 2'den (kurulumun gelir
adımı); tutar `ListCard`'ı, giriş bloğu ve Kaydet / Vazgeç düzeni `EK-V6b` ve `EK-V6c`'den. Renk ve ses
Planör marka token'larından (`GS7`).

### EK-V6e — Ödeme formu: taksitli ödeme planı + planlı büyük harcama (`V6e`)

> Sayfa dosyaları: `PaymentPlanFormPage.xaml` (`Routes.PaymentPlanForm` + `planId`) ve `PlannedExpenseFormPage.xaml` (`Routes.PlannedExpenseForm` + `expenseId`).
> Finansal Yapı başlığındaki "Ekle" seçicisinden (Ödeme planı / Planlı büyük harcama) ya da Ödemeler listesindeki satır diyaloğundan ("Düzenle") açılır.
> Davranış `S65`, düzen `EK-V6e`.

#### 1. Sorular

| Kod | Soru | Eskide nasıl cevaplanıyordu |
|---|---|---|
| S1 | "Ödeme planının adı nedir ve hangi taksitlerden oluşuyor?" | Satır içi formda plan adı ve tek tek girilen taksit satırları |
| S2 | "Taksitler ne zaman ve ne kadar ödenecek?" | Taksit satırları (tarih + tutar) |
| S3 | "Yeni taksit veya aylık taksit serisi nasıl eklenir?" | Tek tek tarih ve tutar seçerek (seri taksit yoktu) |
| S4 | "Planlı büyük harcamanın adı, tutarı ve tarihi nedir?" | Satır içi formda ad, tutar ve tarih |

#### 2. Kesme kararları

| Bilgi / Öğe | Karar | Gerekçe |
|---|---|---|
| Plan adı | **Giriş** (`Entry`) | S1: Plan tanımı |
| Taksitler listesi | **Kart** (`ListCard`) | S2: ≤ 4 satır, fazlası yerinde açılır (`GS21`); dokun → ödenmemişse silme diyaloğu |
| Taksit ekleme bloğu | **Giriş kartı** (`Border`) | S3: Tutar + taksit sayısı (1–120) + ilk vade tarihi (en erken bugün) |
| Büyük harcama alanları | **Giriş kartı** (`Border`) | S4: Harcama adı + tutar (> 0) + tarih (en erken bugün) |
| Kaydet / Vazgeç | **Buton** (`Grid *,*`) | S1, S4: Vazgeç / geri oku / geri tuşu değişiklik varsa onay sorar (`EK-V6b` deseni) |
| "Geçici / düzenli" ayrımı | **Çıkar** | Kullanıcıya bir şey söylemiyor (`S62`) |
| Durum rozetleri, notlar | **Çıkar** | Kalabalık ve gereksiz bilişsel yük |

#### 3. Bütçe

```
Ödeme Planı Formu:
Kart          3 / 4     plan adı kartı, taksitler ListCard'ı, taksit giriş kartı
Label         8 / 28    plan adı 1, taksit satırı 2, giriş 3, butonlar/hata 2
Cumle_        0 / 3

Büyük Harcama Formu:
Kart          1 / 4     harcama tanım kartı
Label         4 / 28    ad 1, tutar 1, tarih 1, hata 1
Cumle_        0 / 3
```

#### 4. Blok şeması

Ödeme Planı Formu (`PaymentPlanFormPage.xaml`):
```
┌─ PageHeader ──────────────────────────────────────────────────────┐
│ Baslik_YeniOdemePlani | Baslik_OdemePlaniniDuzenle                │  ← S1
└───────────────────────────────────────────────────────────────────┘
┌─ Form kartı (Border SurfaceCard / RadiusCard / CardPadding) ──────┐  ← S1
│ Etiket_PlanAdi               Eyebrow                              │
│ [ Entry  "Örn. Telefon taksiti, Sigorta" ]                        │
└───────────────────────────────────────────────────────────────────┘
┌─ ListCard  Etiket_Taksitler ──────────────────────────────────────┐  ← S2 (yalnız taksit varsa)
│ 15 Kasım 2026             TypeBody / TextPrimary      2.500 ₺     │
│   … en fazla 4 satır, vadeye göre; fazlası:                       │
│ +8 daha                   OverflowText (Bicim_FazlaKayit)       › │  GS21
│   dokun → ChooseAsync("Taksit", "Vazgeç", "Sil")                  │
└───────────────────────────────────────────────────────────────────┘
[ Aksiyon_TaksitEkle         SecondaryButton ]                         ← S3 (giriş kapalıyken)
┌─ Giriş bloğu (Border SurfaceCard / RadiusCard / CardPadding) ─────┐  ← S3 (açıkken)
│ Etiket_TaksitTutari          Etiket_TaksitSayisi                  │
│ [ Entry Numeric ]            [ Entry Numeric "1" ]                │
│ Etiket_IlkVadeTarihi         Eyebrow                              │
│ [ DatePicker  en erken bugün ]                                    │
│ [ Aksiyon_Ekle ActionFill ]  [ Aksiyon_Vazgec SecondaryButton ]   │
└───────────────────────────────────────────────────────────────────┘
[ Aksiyon_Kaydet ActionFill ]  [ Aksiyon_Vazgec SecondaryButton ]      ← S1 (Grid *,*)
```

Büyük Harcama Formu (`PlannedExpenseFormPage.xaml`):
```
┌─ PageHeader ──────────────────────────────────────────────────────┐
│ Baslik_YeniBuyukHarcama | Baslik_BuyukHarcamayiDuzenle            │  ← S4
└───────────────────────────────────────────────────────────────────┘
┌─ Form kartı (Border SurfaceCard / RadiusCard / CardPadding) ──────┐  ← S4
│ Etiket_HarcamaAdi            Eyebrow                              │
│ [ Entry  "Örn. Beyaz eşya, Tatil" ]                               │
│ Etiket_Tutar                 Etiket_Tarih                         │
│ [ Entry Numeric ]            [ DatePicker  en erken bugün ]       │
└───────────────────────────────────────────────────────────────────┘
[ Aksiyon_Kaydet ActionFill ]  [ Aksiyon_Vazgec SecondaryButton ]      ← S4 (Grid *,*)
```

#### 5. Üç durum

| Durum | Görünen |
|---|---|
| Boş | Yeni kayıtta form boş alanlarla açılır; ödeme planında taksit giriş bloğu varsayılan açık gelir. |
| Yükleniyor | Düzenlemede iki `SkeletonBlock`; spinner yok (`GS14`). Yeni kayıtta yükleme yok. |
| Hata | Kayıt okunamazsa `StateBlock` (Hata, `Close` ikonu, `Hata_OdemePlaniYuklenemedi` / `Hata_BuyukHarcamaYuklenemedi`, `Aksiyon_TekrarDene`). Bulunamazsa diyalog ve geri dönüş. Kaydetme hatasında diyalog. |

#### 6. Konsept ilişkisi

Konsept karşılığı **yok** (`GS2`). Form kartı, alan sırası ve adları `EK-V6b` ve `EK-V6d`'den; taksit `ListCard`'ı ve giriş bloğu `EK-V6b2` (gelecek harcamalar) deseniyle aynıdır.

### EK-V6f — Kayıt türü seçici (`V6f`)

> Sayfa dosyası: `RecordEntryPickerPage.xaml` (`Routes.RecordEntryPicker`). Finansal Yapı başlığındaki "Ekle"den açılır.
> Bileşen: `Components/EntryTypeTiles.xaml` (bir grup; simülatör `V10c`'de aynı bileşene geçer). Davranış `S77`, görünüm `GS29`.
> Konsept: `docs/assets/konsept/ekleme-ekranı-acik.png`, `docs/assets/konsept/ekleme-ekranı-koyu.png` (kullanıcı getirdi).
> `V6f1` tamamlandı (Kapı C onaylı 2026-10-03, koyu + açık); `V6f2` tamamlandı (Kapı C onaylı 2026-10-04):
> üst kayıtlı üç tür ve "Hangi kart?" hâli (`S77`-5, `S77` V6f2 notları d–h).

**Eski proje:** `EntryTypePickerView.xaml` 78 satır, 3 `<Label>` (işaret, başlık, özet), grup başına hap düğme;
`RecordEntryPicker` 76 satır, bağımlılıksız. Seçici `CommitmentsPage`'in satır içi formunun üstünde duruyordu.

#### 1. Sorular

| Kod | Soru | Eskide nasıl cevaplanıyordu |
|---|---|---|
| S1 | "Ne ekleyebilirim, aradığım şey hangi grupta?" | 4 grup çipi (düğme) + seçili grubun ≤ 3 başlığı |
| S2 | "Bu seçenek ne işe yarıyor, aradığım bu mu?" | Seçenek başına 1 özet satırı |
| S3 | "Seçtim, nasıl devam ederim?" | ●/○ işareti + seçicinin altında açılan satır içi form (~49 etiket) |
| S4 | "Bunu hangi kartıma / kredime / gelirime ekliyorum?" | Ortak formun içinde açılır liste ("Kart seç"); kayıt yoksa formda kırmızı uyarı. Gelir için soru yoktu (tek maaş varsayımı) |

#### 2. Kesme kararları

| Bilgi / Öğe | Karar | Gerekçe |
|---|---|---|
| Dört grup aynı anda, Finansal Yapı listesinin adları ve sırasıyla | **Kart** (grup başına bir `EntryTypeTiles`) | S1; eklenen kayıt listede aynı adlı grupta (`S77`-3) |
| Grup başlığı: renkli nokta + eyebrow | **Satır** | S1; renk grubu söyler (`GS29`-2) |
| Karo: ikon + başlık + alt satır | **Satır** (tek paylaşılan şablon) | S2, S3 |
| Seçeneğin formu | **Derine** (var olan form sayfaları) | S3; seçici formla yer değiştirir (`S77`-2) |
| Konseptin başlık altı cümlesi | **Çıkar** | Başlığın sorusunu tekrar ediyor, "plan" iç kavramı (`GS29`-c) |
| Grup şeridi, seçili grup durumu | **Çıkar** | Kapı C: alt boşluk; bütün gruplar aynı anda |
| ●/○ işareti, satır içi form, "Yeni Kayıt" başlığı, `›` | **Çıkar** | Form ayrı sayfa; karonun kendisi dokunulur |
| Katalogdaki uzun açıklamalar | **Çıkar** | GK5; yerine ≤ 24 harflik `Etiket_` |
| "Kredi / finansman çek" | **Sonra** → `V10d` | Ortak form gerektiriyor (`S77`-4) |
| Üst kaydın adayları (kart, kredi, düzenli gelir) | **Satır** (yerinde ikinci seviye, tek paylaşılan şablon) | S4; yalnız birden fazla aday varken (`S77`-5, V6f2 notları f) |
| Adayın bağlamı (sıradaki ödeme, kalan taksit, her ayın günü) | **Satır** | S4; aynı bankadan iki kartı ayırır; Finansal Yapı'daki satırla aynı |
| Adayın tutarı | **Çıkar** | "Hangisi?" sorusuna cevap değil; tutar Finansal Yapı'da |
| Açılır liste, formdaki "kayıt yok" uyarısı | **Çıkar** | Tek aday sorulmaz; aday yoksa onay diyaloğu (V6f2 notları e) |

#### 3. Bütçe

```
Hero rakam    0 / 1
Hero yüzey    0 / 1
Kart          4 / 4     EntryTypeTiles × 4 (Gelir, Kart, Kredi, Ödeme; analizci bileşeni kart sayar, I133);
                        aday karoları DataTemplate'te tekrar eden satır, gruplarla aynı anda görünmez
Grafik        0 / 1
NavRow        0 / 5
Label         7 / 28    bileşenin şablonları: eyebrow 1, ikon 1, karo başlığı 1, alt satır 1;
                        sayfada aday şablonu: ad 1, bağlam 1, › 1
Cumle_        0 / 3
```

Aday listesi `ListCard` değildir: beşinci kart olurdu (GK4). Adaylar birinci seviyenin karo diliyle,
ayrı dokunma yüzeyleri olarak durur; karo başına bir ikon (`›`).

#### 4. Blok şeması

`RecordEntryPickerPage.xaml` (`x:DataType` = `RecordEntryPickerViewModel`), `VerticalStackLayout` `Spacing Space5`.
Geri ok (`BackButtonBehavior`) ve cihazın geri tuşu `BackCommand`'dan geçer: ikinci seviyede karolara, birincide listeye döner.

```
┌─ PageHeader ──────────────────────────────────────────────────────┐
│ ChoiceGroup → SeciciMetni(Soru)   TypeTitle / TextPrimary          │  ← S1, S4
│   null: Baslik_NeEklemek · Kart: Baslik_HangiKart ·                │
│   Kredi: Baslik_HangiKredi · Gelir: Baslik_HangiGelir              │
└───────────────────────────────────────────────────────────────────┘

VerticalStackLayout  Spacing Space5  IsVisible = !IsChoosing          — birinci seviye: dört grup
EntryTypeTiles  IncomeSection   Accent PositiveText · AccentSurface PositiveSurface     ← S1–S3
● GELİR
╭───────────────────╮ ╭───────────────────╮
│ [⟳]               │ │ [✦]               │
│ Düzenli gelir     │ │ Tek seferlik gelir│
│ Her ay aynı gün…  │ │ Prim, satış, iade │
╰───────────────────╯ ╰───────────────────╯
╭───────────────────╮
│ [↗] Gelir değişikliği · Zam ya da yeni tutar
╰───────────────────╯
EntryTypeTiles  CardSection  Accent Indicator · AccentSurface SurfaceChart             ← S1–S3
● KART   [Kredi kartı] [Kartla harcama]
EntryTypeTiles  LoanSection  Accent WarningText · AccentSurface WarningSurface         ← S1–S3
● KREDİ  [Bankadaki kredi] [Krediye erken ödeme]
EntryTypeTiles  PaymentSection  Accent NegativeText · AccentSurface NegativeSurface    ← S1–S3
● ÖDEME
  [Nakit ödeme] [Düzenli ödeme]
  [Taksitli borç] [Ödeme planı]

VerticalStackLayout  Spacing Space3  IsVisible = IsChoosing  BindableLayout = Choices   — ikinci seviye
└─ aday (DataTemplate, x:DataType = FinancialRecordRow)                                 ← S4
   Border (örtük: SurfaceCard / BorderSubtle / RadiusCard / CardPadding)   dokun → ChooseRecordCommand
   └─ Grid "*, Auto"  ColumnSpacing Space3
      ├─ VerticalStackLayout  Spacing Space1
      │  ├─ Label  Name                    TypeFigure, OpenSansSemibold, TextPrimary (karo başlığıyla aynı)
      │  └─ Label  . → KayitBaglami        Caption ("Sıradaki ödeme 15 Ekim", "12 taksit · 3 Kasım")
      └─ Label  ChevronRight  MaterialSymbols, IconSmall, Indicator
```

`EntryTypeTiles` (bir grup), `VerticalStackLayout` `Spacing Space3`:

```
HorizontalStackLayout  Spacing Space2
├─ BoxView  Space2 × Space2, RadiusPill, Color = Accent
└─ Label  Section.Group → SeciciMetni     Eyebrow
Grid  sütun/satır = KaroIzgarasi(Options)  ColumnSpacing / RowSpacing Space3      ≤ 2 sütun; tek karo genişliği doldurur
└─ karo (DataTemplate, x:DataType = EntryTypeOptionItem)  Grid.Row/Column = KaroIzgarasi(Index)
   Border (örtük: SurfaceCard / BorderSubtle / RadiusCard / CardPadding)   dokun → OptionCommand
   └─ VerticalStackLayout  Spacing Space3
      ├─ Border  Padding Space2, zemin AccentSurface, kenarsız
      │  └─ Label  Key → SeciciMetni(Ikon)   MaterialSymbols, IconMedium, 24 × 24, renk Accent
      └─ VerticalStackLayout  Spacing Space1
         ├─ Label  Key → SeciciMetni          TypeFigure, OpenSansSemibold, TextPrimary
         └─ Label  Key → SeciciMetni(AltSatir) Caption
```

Uygulama notları:
- Metin ve ikon ViewModel'de değil: öğeler grup türü ve anahtar taşır; eyebrow, başlık, alt satır ve ikon
  `SeciciMetniConverter`'dan (`RecordEntryStrings`, `Icons`). Grup rengi sayfada `DynamicResource` ile verilir (GK8).
- Komutlar şablonda `RelativeSource AncestorType={x:Type comp:EntryTypeTiles}` ile; `x:Reference` şablonda yok (kural 03).
- Karo seçimi `NavigateToAsync(Routes.ReplacingCurrent(<rota>))` (`../`): seçici yığından düşer, formdan dönüş
  Finansal Yapı'ya iner ve liste `OnAppearing`'de yenilenir (`I132`).
- Formlar kendi başlığını korur ("Nakit ödeme" → "Planlı Büyük Harcama"; `S77` V6f notları a).
- Üst kayıtlı karo (`EditsExisting`): adaylar dokununca okunur (`IPlanReader` + `FinancialRecordRowBuilder`,
  Finansal Yapı'nın süzgeci). Aday yoksa onay diyaloğu → üst kaydın boş formu `../` ile; tek adayda form
  `../<rota>` + kimlik; birden fazlaysa `ChoiceGroup` dolar, sayfa ikinci seviyeye geçer (`S77` V6f2 notları e–h).
- Kartın, kredinin ve gelirin formu açılınca ilgili bölüm (gelecek harcama, erken ödeme, tutar değişikliği)
  formun altındadır; formlara dokunulmaz (V6f2 notları g).

| Grup | Karo | Alt satır | İkon | Hedef |
|---|---|---|---|---|
| Gelir | Düzenli gelir | Her ay aynı gün yatar | `Repeat` | `Routes.IncomeForm` |
| Gelir | Tek seferlik gelir | Prim, satış, iade | `AutoAwesome` | `Routes.AdHocIncomeForm` |
| Gelir | Gelir değişikliği | Zam ya da yeni tutar | `TrendingUp` | düzenli gelir → `Routes.IncomeForm` + `incomeId` |
| Kart | Kredi kartı | Limit ve ekstre günleri | `CreditCard` | `Routes.CardForm` |
| Kart | Kartla harcama | Tek çekim ya da taksit | `ShoppingBag` | kart → `Routes.CardForm` + `cardId` |
| Kredi | Bankadaki kredi | Devam eden taksitler | `AccountBalance` | `Routes.LoanForm` |
| Kredi | Krediye erken ödeme | Kapat ya da ara ödeme | `FastForward` | kredi → `Routes.LoanForm` + `loanId` |
| Ödeme | Nakit ödeme | Seçtiğin gün düşer | `Payments` | `Routes.PlannedExpenseForm` |
| Ödeme | Düzenli ödeme | Her ay aynı tutar | `EventAvailable` | `Routes.PaymentPlanForm` |
| Ödeme | Taksitli borç | Borcu taksitle öde | `PieChart` | `Routes.PaymentPlanForm` |
| Ödeme | Ödeme planı | Tutarı aydan aya değişir | `BarChart` | `Routes.PaymentPlanForm` |

#### 5. Üç durum

| Durum | Görünen |
|---|---|
| Boş | Yok: katalog sabittir ve her grupta en az bir seçenek vardır (katalog testi korur). Aday yoksa ikinci seviye açılmaz, onay diyaloğu sorulur. |
| Yükleniyor | Yok: katalog bellektedir, sayfa doğrudan dolu açılır; adaylar karoya dokununca okunur, okuma süresince karolar kilitlidir (iskelet yok, V6f2 notları h). |
| Hata | Adaylar okunamazsa diyalog; seçici birinci seviyede kalır. |

#### 6. Konsept ilişkisi

Konsept `ekleme-ekranı-acik` / `-koyu` sadakatle uygulandı; sapmalar `GS29`'da: başlık altı cümle yok, grup renkleri
semantik token'lardan, karo köşesi `RadiusCard`. Önceki iki deneme (eski şerit düzeni, ikonsuz ızgara) Kapı C'de geri döndü.

## EK-V8 — 12 dönem

> Sayfa dosyası: `FuturePeriodsPage.xaml` (`V8a`; erken kapama kartı `V8b`). Yan menüdeki "12 Dönem" (`//projection`).
> Durum: tamamlandı (`V8a` 2026-10-03, `V8b` 2026-10-03).
> Davranış `S74`, ekran `GS26`. Ekran hiçbir şey yazmaz.

**Eski proje:** `FutureMonthsPage.xaml` 224 satır, **37 `<Label>`**, 5 kart (sapma uyarısı, faiz, kredi kapatma,
hedef tutar, boş hâl) + 12 dönem kartı şablonu, 7 buton + görünmez dokunma katmanı, 1 giriş.

### 1. Sorular

| Kod | Soru | Eski ekran nasıl cevaplıyordu |
|---|---|---|
| S1 | "Önümüzdeki aylarda param eksiye düşecek mi, en çok hangi dönem sıkışacağım?" | Doğrudan cevap yok: 12 kartı kaydırıp "Dönem sonu" rakamlarını kıyaslamak (12 × 2 etiket) |
| S2 | "Bir yıl sonra elimde ne olacak, gidişat yukarı mı aşağı mı?" | Uzun kaydırmanın dibindeki son kart; grafik yok |
| S3 | "Her dönemin sonunda ne kalacak?" | Dönem kartı başına ~18 etiket: dönem, tahsis metni, dönem sonu, gelir / zorunlu / yaşam, devreden açık, 4 çip, ayrıntı butonu |
| S4 | "Bu gidişatla 12 dönemde ne kadar faiz ödeyeceğim?" | Faiz kartı, 7 etiket |
| S5 | "Kredimi erken kapatmalı mıyım, hangi gün, ne kazandırır?" (`V8b`) | Kredi başına 3 etiket (2'si üretilmiş cümle), "Simülatörde dene" ve "Krediyi düzenle" |

### 2. Kesme kararları

| Bilgi / Öğe | Karar | Gerekçe |
|---|---|---|
| En düşük dönem sonu + hangi dönem | **Hero** (`HeroFigure`) + açıklama satırı (`TextSecondary`) | S1 ekranın asıl sorusu; kırmızıyı ızgara ve grafik taşır |
| Zincir başı + 12 dönem sonunun yolu, sıfır çizgisi | **Grafik** (`AreaTrend`, seri + eşik; dolgu sıfıra iner) | S1 / S2 yön; eksi dönemler sıfırın altında cep (`GS26`-3, 4) |
| Zincir başı ve 12. dönemin son günü | **Satır** (grafiğin altında iki etiket) | Grafiğin tarih ekseni |
| 12 dönem sonra | **Satır** (`MetricRow`) | S2 |
| 12 dönemde faiz (toplam) | **Satır** (`MetricRow`), 0 ₺ dahil hep görünür | S4; ayrı kart değil (konsept) |
| Faiz kırılımı (kart / KMH) | **Derine** → `V9` | Dönem ayrıntısında dönem başına faiz |
| Bu dönemin sonu (ana sayfadaki rakam) | **Çıkar** | Zincirin başı grafiğin sol ucu (konsept) |
| Dönem başına ay + dönem sonu | **Kart** (3 × 4 karo ızgara, ham `Border` + `Grid`) | S3; 12'si tek ekran boyunda; en düşük kalın, eksiler `NegativeText` (`GS26`-1) |
| Dönem başına gelir / zorunlu / yaşam, dönem faizi, kart çipleri | **Derine** → `V9` | S3'ün ikinci seviyesi; karo dönem ayrıntısını açar (`V9`, `S75`-1) |
| Erken kapama: kredi, önerilen gün ve tutar, net kazanç | **Kart** (`ListCard`), `V8b` | S5; satır krediyi açar, kapama orada planlanır (`S74`-7) |
| Önerilenden farklı en kârlı gün | **Çıkar** → `V10` | İkinci bir karar noktası; simülatörün sorusu |
| Kredinin ömrü boyunca ödenmeyecek faiz (brüt tasarruf) | **Çıkar** | Net kazanç onu açık faizi düşülmüş hâliyle içeriyor; kredi formunda "Kurtulacağın faiz" var (`V8b` Kapı A) |
| "Simülatörde dene" | **Derine** → `V10` | Simülatör yok; `V10`'da bağlanır |
| Sapma uyarısı ("checkpoint'e dayanıyor") | **Çıkar** | Zincir ana sayfanın rakamından başlıyor (`S74`-4) |
| Hedef tutar ("Ne zaman karşılayabilirim?") | **Çıkar** → `V10` notu | Izgara dönem sonlarını zaten gösteriyor (`S74`-5) |
| Tahsis metni, "gelirden önce" çipi | **Çıkar** | `S18` |
| Devreden açık satırı | **Çıkar** | Önceki dönemin eksi sonuyla aynı bilgi |
| Sayfa eyebrow'u + açıklama cümlesi, "Mevcut dönemi gör" | **Çıkar** | Başlık "12 Dönem" yeter; ana sayfa yan menüde |
| Spinner, durum satırı | **Çıkar** | `SkeletonBlock` / `StateBlock` (`GS14`) |

### 3. Bütçe

```
Hero rakam    1 / 1     en düşük dönem sonu
Hero yüzey    0 / 1     gidişat kartı tonlu SurfaceChart (GS26-2), SurfaceHero değil
Kart          3 / 4     gidişat kartı, dönem sonları ızgarası, erken kapama ListCard* (V8b)   * yalnız öneri varsa
Grafik        1 / 1     AreaTrend
NavRow        0 / 5
Label        15 / 28    başlık 1, gidişat kartı 5 (eyebrow, hero, açıklama, iki uç tarih), ızgara 4 (eyebrow; karo: ay, tutar + tetikleyici), erken kapama şablonu 5 (ad, durum, "net", kazanç, ok; "ERKEN KAPAMA" `ListCard`'ın `EyebrowText`'i, etiket değil)
Cumle_        0 / 3
```

`V8a` sonunda: kart 2 / 4, label 10 / 28 (başlık `Shell.TitleView`, ana sayfadaki gibi). Gidişat kartındaki iki satır `MetricRow` bileşeni; sayfa XAML'inde
`<Label>` değiller. Analizci ham `Border`'ları kart saymaz; bütçe dürüst sayımdır. Eski ekranın 37 etiketinden 15'e.

`V8b` sonunda: kart 3 / 4, label 15 / 28 (analizci ve dürüst sayım aynı). Aşama 4'te "net" ile tutar tek etiketin
`FormattedText`'i olacaktı; iki `Label` oldu, çünkü aradaki boşluk metin kaynağında taşınamıyordu. Sayım değişmedi
(analizci `<Label.FormattedText>`'i de bir `<Label` sayar). Durum metnini `ErkenKapamaDurumuConverter` kurar;
tutar `ParaConverter`'ın biçimiyle yazılır.

### 4. Blok şeması

```
┌─ Kabuk başlığı ─────────────────────────────────────────────┐
│ [☰ kabuk]  12 Dönem               Baslik_GelecekDonemler    │
└─────────────────────────────────────────────────────────────┘
┌─ Gidişat kartı (ham Border)  SurfaceChart / BorderSubtle / RadiusHero / CardPadding ┐
│ Etiket_EnDusukDonemSonu                 Eyebrow             │  ← S1
│ −18.250 ₺                               HeroFigure / TextPrimary (eksi de olsa)
│ 10 Mart 2027 dönemi sonunda             Bicim_EnDusukDonem, Caption / TextSecondary
│ AreaTrend   yükseklik ChartHeight                           │  ← S1, S2
│   Series: zincir başı + 12 dönem sonu, düz Indicator        │
│   dolgu Indicator %20, sıfıra iner: eksi dönemler cep (GS26-4)
│   Threshold 0: kesikli NegativeText, her zaman ölçekte (GS26-3)
│ 10 Eki 2026                                 9 Eki 2027      │  Caption: zincir başı · 12. dönemin son günü
│ MetricRow  Etiket_OnIkiDonemSonra ............ 26.300 ₺     │  ← S2; eksiyse Negative
│ MetricRow  Etiket_OnIkiDonemdeFaiz ............ 3.750 ₺     │  ← S4; 0 ₺ dahil hep görünür
└─────────────────────────────────────────────────────────────┘
┌─ Dönem sonları (ham Border)  SurfaceCard / BorderSubtle / RadiusCard ┐  ← S3
│ Etiket_DonemSonlari                     Eyebrow             │
│ ┌───────────┬───────────┬───────────┐  Grid 3 sütun × 4 satır, BindableLayout (satır, sütun dizinden)
│ │ Ekim 2026 │ Kasım     │ Aralık    │  ay: Caption / TextSecondary; yıl ilk karoda ve Ocak'ta
│ │ 44.120 ₺  │ 47.900 ₺  │ 38.300 ₺  │  tutar: TypeFigure; en düşük SemiBold, diğerleri Regular;
│ ├───────────┼───────────┼───────────┤         eksi NegativeText, değilse TextPrimary
│ │ Ocak 2027 │ Şubat     │ Mart      │
│ │ 41.050 ₺  │ 45.600 ₺  │ −18.250 ₺ │
│ │ …                                 │  ayırıcı: aralık StrokeHairline, zemin BorderSubtle
│ └───────────┴───────────┴───────────┘  karo: zemin SurfaceCard, dolgu Space3
│ karoya dokun → Routes.PeriodDetail + periodStart (V9, S75-1)  │
└─────────────────────────────────────────────────────────────┘
┌─ ListCard (V8b; öneri yoksa ya da hata verdiyse görünmez) ──┐  ← S5
│ Etiket_ErkenKapama                      Eyebrow             │
│ Taşıt kredisi                        net +2.950 ₺   ›       │  TypeBody / TextPrimary · "net" Caption + tutar Figure
│ 18 Eyl 2027 · 21.400 ₺ ile kapat                            │    PositiveText · ChevronRight Indicator
│   güvenli gün yoksa   Etiket_KapatmakAcikOlusturur          │  Caption: Bicim_KapamaOnerisi
│   kazandırmıyorsa     Etiket_KapatmakKazandirmiyor          │  bu dördünde kazanç yok
│   anapara yoksa       Etiket_AnaparaGerekli                 │
│   kapama planlıysa    Bicim_KapamaPlanli                    │
│ satıra dokun → Routes.LoanForm + loanId                     │
│ her kredi bir satır, Finansal Yapı sırasıyla; 4 sınırı yok (GS26-6)
└─────────────────────────────────────────────────────────────┘
```

Sıra konseptin sırası. Erken kapama en altta: önerisi listeden sonra gelir (birkaç düzine projeksiyon);
hazır olunca görünür alanın altında açılır, yerleşim gözün önünde zıplamaz.

Grafik için iki küçük ek (`GS26`-3, 4): `ChartTrend`'e isteğe bağlı `Threshold` (varsayılan `null`) ve
`AreaTrend`'de eşik verildiyse dolgunun tabana değil eşiğe inmesi. `V3` eşik vermediği için değişmez. Yeni bileşen yok.

### 5. Üç durum

| Durum | Görünen |
|---|---|
| Boş | `StateBlock`: `Schedule` ikonu + `Bos_OnIkiDonemYok` + `Aksiyon_FinansalYapiyaGit`. Açık dönem yoksa ya da plan projeksiyon kuramıyorsa (`S74`-2). Konseptteki "Gelir gününü belirle" alınmadı (`GS26`-5). |
| Yükleniyor | İki `SkeletonBlock`: gidişat kartı (`ChartHeight`) ve ızgara; spinner yok (`GS14`). Erken kapama kartı yüklemeyi beklemez, hazır olana kadar görünmez. |
| Hata | `StateBlock`: `Close` ikonu + `Hata_OnIkiDonemHesaplanamadi` ("12 dönem şu an hesaplanamadı.") + `Aksiyon_TekrarDene`. Erken kapamanın hatası sayfayı düşürmez, yalnız kartı gizler (`S74`-7). |

### 6. Konsept ilişkisi

Claude Design, "Planör · 12 Dönem" (2026-10-02), `docs/assets/konsept/` altında: `12-donem-koyu-dolu.png`,
`12-donem-açık-dolu.png`, `12-donem-eksiye-düsmeyen-veri.png`, `12-donem-yukleniyor.png`, `12-donem-bos.png`,
`12-donem-hata.png` ve erken kapamanın hâlleri (`12-donem-erken-kapama-bilgi-eksik.png`, `-kapama-planli.png`,
`-kazandiran-yok.png`; önerili ve açık oluşturan satırlar dolu hâlde).
Yerleşim oradan, renkler ve tip skalası token'lardan. Konseptten sapmalar `GS26`'da: yazı tipi, kabuk menüsü,
eşik rengi, ikonlar, erken kapama satır puntoları, boş hâl metni, iskelet.

## EK-V9 — Dönem ayrıntısı

> Sayfa dosyası: `PeriodDetailPage.xaml` (`Routes.PeriodDetail` + `Routes.PeriodStartParameter`).
> 12 Dönem ızgarasında bir karoya dokununca açılır; kabuğun geri okuyla kapanır.
> Davranış `S75`, ekran `GS27`. Ekran hiçbir şey yazmaz.
> Durum: tamamlandı (`V9`, 2026-10-03).

**Eski proje:** `CashFlowPeriodDetailPage.xaml` 605 satır, **81 `<Label>`**, 23 `<Border>`, 3 buton, 11 liste;
ViewModel 123 satır / 4 bağımlılık, metni üreten sunucu 552 satır / 2 `partial`. Üç yerden açılıyordu
(ana sayfa, 12 dönem, simülatör).

### 1. Sorular

| Kod | Soru | Eski ekran nasıl cevaplıyordu |
|---|---|---|
| S1 | "Bu dönemin sonunda elimde ne kalacak, param eriyor mu?" | "DÖNEM SONU" ve "DÖNEM NETİ" kutuları (4 etiket) |
| S2 | "Bu rakam nereden çıkıyor?" | 4 kutu (8) + "Dönem Akışı" 8–9 satır + kategori kartı (3) + devreden açık kutusu (5): ~19 etiket |
| S3 | "Bu dönem neleri, hangi gün, ne kadar ödeyeceğim?" | Ödeme başına kart: tarih çipi, ad, kategori, tutar, tahsis metni, 3 çip, kart kararı + 2 dipnot: ~14 etiket |
| S4 | "Bu dönem faiz doğuyor mu, nereden?" | "Faiz Yükü" + "Kart bazında": 6 etiket |

### 2. Kesme kararları (`GS27`)

| Bilgi / Öğe | Karar | Gerekçe |
|---|---|---|
| Dönem sonu | **Hero** (`HeroFigure`) | S1; karodaki rakamla aynı (`S75`-1) |
| Dönem başına göre (dönem sonu − dönem başı) | **Satır** (hero altında, işaretli, renkli) | S1; eski "DÖNEM NETİ", gelir karşılama cümlelerinin yerine |
| Dönemin ilk ve son günü | **Satır** (`Caption`) | Bağlam; başlıkta ay adı (karonun adı) |
| Dönem başı · Gelir · Ödemeler · Yaşam gideri · KMH faizi | **Satır** (5 × `MetricRow`; KMH faizi yalnız > 0) | S2; toplamı dönem sonu (`S75`-3) |
| Ödeme kalemleri (büyük harcamalar dahil) | **Kart** (`ListCard`: ad, gün, tutar; 4 satır + "+N daha" yerinde, `GS21`) | S3; notta adet ve toplam = akıştaki "Ödemeler" |
| "tahmini" / "Belirlenmedi" | **Satır** içinde (gün satırında / tutarın yerinde) | S3 (`S75`-4) |
| Kart satırı → Kart Kontrol | **Derine** (yalnız kart satırlarında ›) | Eski kart kararı düğmelerinin yerine (`S75`-6) |
| Kart faizi, kart başına | **Kart** (`ListCard`, yalnız faiz varsa; toplam notta) | S4; ekstre borcuna eklenir, akışta değil (`S75`-5) |
| Gelir kalemleri | **Çıkar** | Akışta tek "Gelir" satırı (kullanıcı kararı) |
| `StackedBar`, `ComparisonStrip`, `InfoBanner` | **Çıkar** | `GS27` (a)(b) |
| 4 özet kutu, ara toplamlar, kategori toplamları, devreden açık kutusu | **Çıkar** | Hero ve akış satırlarının tekrarı (`S75`-3) |
| Kategori adı, tahsis metni, "Dönemden Önce" / "Plan Bekliyor" çipleri | **Çıkar** | `S18`; ödemenin adı yeterli |
| Kart ödeme şekli düğmeleri | **Çıkar** → `EK-V7` | Aynı karar Kart Kontrol'de (`S75`-6) |
| Hatırlatıcı + "ödediklerin" | **Çıkar** | Ana sayfada (`S75`-7) |
| Simülasyon blokları, baz ↔ senaryo kıyası | **Çıkar** → `V10` | `S75`-7 |
| Geliştirici dökümü | **Çıkar** | `GS5` |

### 3. Bütçe

```
Hero rakam    1 / 1     dönem sonu
Hero yüzey    0 / 1     akış kartı tonlu SurfaceChart (GS27-1), SurfaceHero değil
Kart          3 / 4     akış kartı (ham Border), ödemeler ListCard, kart faizi ListCard*   * yalnız faiz varsa
Grafik        0 / 1
NavRow        0 / 5
Label        12 / 28    başlık 1, akış kartı 5 (eyebrow, hero, "dönem başına göre" + tutar, aralık),
                        ödeme şablonu 4 (ad, gün, tutar, ›), faiz şablonu 2 (kart, tutar)
Cumle_        0 / 3
```

Eski ekranın 81 etiketinden 12'ye. Analizci ham `Border`'ı kart saymaz; bütçe dürüst sayımdır.

`V9` sonunda: sayım Aşama 4 ile aynı — hero 1, kart 3 (analizci 2), label 12 (analizci 16: `<Label.Triggers>` ve
`<Label.Text>` özellik öğelerini de sayıyor), cümle 0. Akıştaki gider satırları `ParaConverter`'ın `Eksi` parametresiyle
yazılır; ViewModel tutarları artı sunar.

### 4. Blok şeması

```
┌─ Shell.TitleView ───────────────────────────────────────────┐
│ [← kabuk]  Ekim 2026       Tarih 'MMMM yyyy', SectionTitle  │  karonun adı
└─────────────────────────────────────────────────────────────┘
┌─ Akış kartı (ham Border)  SurfaceChart / BorderSubtle / RadiusHero / CardPadding ┐
│ Etiket_DonemSonu                        Eyebrow             │  ← S1
│ −18.250 ₺                               HeroFigure / TextPrimary (eksi de olsa)
│ Dönem başına göre  −3.200 ₺             Caption + Caption işaretli:
│                                         eksi NegativeText, artı PositiveText, sıfır TextSecondary
│ 10 Ekim – 9 Kasım                       Bicim_DonemAraligi, Caption / TextSecondary
│ MetricRow Etiket_DonemBasi ............... −15.050 ₺        │  ← S2; eksiyse Negative
│ MetricRow Etiket_Gelir ................... +48.000 ₺        │     işaretli
│ MetricRow Etiket_Odemeler ................ −31.200 ₺        │     zorunlu + büyük harcama
│ MetricRow Etiket_YasamGideri ............. −20.000 ₺        │
│ MetricRow Etiket_KmhFaizi ................    −480 ₺        │     yalnız > 0
└─────────────────────────────────────────────────────────────┘
┌─ ListCard  (ödeme yoksa görünmez) ──────────────────────────┐  ← S3
│ Etiket_Odemeler                    7 ödeme · 31.200 ₺       │  NoteText: Bicim_OdemeAdediToplam
│ Konut kredisi                                 12.450 ₺      │  TypeBody / TextPrimary · Figure
│ 1 Kas                                                       │  Caption
│ Bonus kartı                                    8.200 ₺   ›  │  kart satırı: ChevronRight Indicator
│ 15 Kas · tahmini                                            │  Bicim_TahminiGun
│ Axess kartı                                Belirlenmedi  ›  │  Etiket_Belirlenmedi, Caption / TextSecondary
│ … ilk 4 satır                                               │
│ +3 daha                                                     │  OverflowText (FazlaKayitConverter), yerinde açılır
│ kart satırına dokun → Routes.CardControl + cardId; diğer satırlar dokunulmaz
└─────────────────────────────────────────────────────────────┘
┌─ ListCard  (yalnız kart faizi varsa) ───────────────────────┐  ← S4
│ Etiket_KartFaizi          640 ₺ · ekstre borcuna eklenir    │  NoteText: Bicim_KartFaiziToplam
│ Bonus kartı                                      420 ₺      │  TypeBody / TextPrimary · Figure
│ Axess kartı                                      220 ₺      │  her kart bir satır; 4 sınırı yok (kart sayısı)
└─────────────────────────────────────────────────────────────┘
```

Akışın sırası hesabın sırası; satırların toplamı hero'ya kuruşu kuruşuna iner (ekranda tam lira yuvarlanır,
1 ₺ fark görünebilir). "Büyük harcama" ayrı satır değil: listede adıyla durur ve "Ödemeler" ile listenin notu
aynı toplamı söyler.

### 5. Üç durum

| Durum | Görünen |
|---|---|
| Boş | `StateBlock`: `Schedule` ikonu + `Bos_DonemBulunamadi` ("Bu dönem artık 12 dönemin içinde değil.") + `Aksiyon_OnIkiDonemeDon` (geri). Zincir kurulamıyorsa ya da dönem zincirde yoksa (`S75`-2). |
| Yükleniyor | İki `SkeletonBlock`: akış kartı (`ChartHeight`) ve ödeme listesi; spinner yok (`GS14`). |
| Hata | `StateBlock`: `Close` ikonu + `Hata_DonemHesaplanamadi` ("Dönem şu an hesaplanamadı.") + `Aksiyon_TekrarDene`. |

### 6. Konsept ilişkisi

Yerleşim konseptinin "Dönem Ayrıntısı" paneli tarifiyle biliniyor (`InfoBanner` + `ComparisonStrip` + kategori
satırları); görüntüsü repoda yok. Ekran 12 Dönem'den türetildi (kullanıcı kararı, 2026-10-03): akış kartı
gidişat kartının deseni, listeler `ListCard`. Konseptten sapmalar `GS27`'de.

## EK-V10 — Simülatör

> Sayfa dosyaları: `SimulatorPage.xaml` (yan menü "Simülatör", `//simulation`) ve `SimulationConditionPage.xaml`
> (deneme formu: `Routes.SimulationCondition` + tür anahtarı ya da deneme kimliği).
> Davranış `S76`, ekran `GS28`. Altı alt adım: `V10a` form + liste, `V10b` sonuç, `V10c` harcama türleri,
> `V10d` borç türleri, `V10e` gelir türleri, `V10f` "Planıma ekle". Kapı A ve B ortak (2026-10-03).
> Ekran gerçek kayıtlara yalnız "Planıma ekle" ile yazar; denemeler kendi listesinde saklanır.
> Durum: `V10a`–`V10f` tamamlandı (2026-10-04); `EK-V10` simülatör ekranı eksiksiz kapandı.

**Eski proje:** `SimulationPage.xaml` 453 satır, **64 `<Label>`**, 14 buton, 11 kart + 12 dönem kart şablonu, en az 8
açıklama cümlesi; kod arkası 113 satır (onay ve gezinme). `ScenarioConditionFormView` 85 satır, 17 etiket, 13 giriş.
ViewModel 1.034 satır / 4 `partial` + form 403 satır / 2 `partial`.

### 1. Sorular

| Kod | Soru | Eski ekran nasıl cevaplıyordu |
|---|---|---|
| S1 | "Bunu yaparsam önümüzdeki aylarda eksiye düşer miyim, en çok nerede sıkışırım?" | 2–5 anlatı cümlesi + "Öne Çıkanlar" + 12 dönem kartının çipleri: ~20 etiket, cevap dağınık |
| S2 | "Bu deneme bir yıl sonra beni ne kadar geri ya da ileri götürür?" | Cevap yoktu: denemeyle "12 ay sonu" vardı, şu anki gidişatla farkı yoktu |
| S3 | "Hangi denemeleri kurdum, hangileri hesaba giriyor?" | Koşul listesi (satır başına 4 etiket + anahtar) + "Plan değişti" ve "Hiçbir koşul açık değil" uyarıları: ~11 etiket |
| S4 | "Bu deneme bana faiz ödetir mi, kazandırır mı?" | Faiz Karşılaştırması tablosu + Kredi Faizi Tasarrufu: ~10 etiket |
| S5 | "Karar verdim, bunu planıma nasıl eklerim?" | "Planı Uygula" + kod arkasında iki diyalog, üretilmiş uzun onay metni |

### 2. Kesme kararları (`S76`, `GS28`)

| Bilgi / Öğe | Karar | Gerekçe |
|---|---|---|
| Denemeyle en düşük dönem sonu + dönem | **Hero** (`HeroFigure`) + açıklama satırı | S1; 12 Dönem'in hero'su, denemeyle |
| Şu anki gidişata göre fark (aynı dönemde) | **Satır** (hero altında, işaretli, renkli) | S1; yalnız açık deneme varsa; denemeyle en düşük dönemin denemesiz sonuyla fark (`S76` V10b notları c) |
| Şu anki ve denemeyle zincir, sıfır çizgisi | **Grafik** (`AreaTrend`, iki seri) | S1 / S2 yön (`GS28`-2) |
| 12 dönem sonra: şu an / denemeyle / fark | **Şema** (`ComparisonStrip`) | S2; açık deneme yokken 12 Dönem'deki `MetricRow` |
| 12 dönemde faiz, faiz farkı | **Satır** (`MetricRow`; fark yalnız ≠ 0) | S4; kırılım tablosu yok (`S76`-9) |
| Kredi faizi kazancı, kredinin maliyeti | **Satır** (`MetricRow`, yalnız o deneme varsa; `V10d`) | S4 |
| Dönem sonları, denemeyle | **Kart** (12 Dönem'in 3 × 4 ızgarası, dokunulmaz) | S1; kullanıcı kararı "12 dönemin aynısı" |
| Denemeler: ad, tür · tarih, tutar, aç/kapa, geçersiz işareti | **Kart** (`ListCard`; satır diyaloğu Düzenle / Sil) | S3 |
| Deneme ekleme | **Aksiyon** (`PageHeader` "Ekle" → tür seçici) | S3; Finansal Yapı deseni |
| Denemenin alanları | **Derine** → `SimulationConditionPage` | S3 |
| Denemenin üst kaydı (kart, kredi, düzenli gelir) | **Derine** → tür seçicinin ikinci seviyesi ("Hangi kart?"); formda salt okunur ad | S3; `S77` V10 notları i (2026-10-04): açılır liste yok, Finansal Yapı'nın davranışı; `V10c`–`V10e` |
| "Planıma ekle" | **Aksiyon** (liste altında; `V10f`) | S5 |
| Anlatı cümleleri, içgörü çipleri, "Öne Çıkanlar" | **Çıkar** | Hero + grafik + şema aynı cevabı sayıyla veriyor (GK5) |
| Faiz kırılımı (kart / KMH / finansman), "gelirlerden kalan", "bu dönem gereken" | **Çıkar** | `S76`-9, `S18` |
| Hedef tutar | **Çıkar** | `S76`-10 |
| Adlı geçici planlar (kaydet / yükle / sil) | **Çıkar** | `S76`-4: tek çalışma listesi |
| "Simülasyonu Yap", "Plan değişti", "Hiçbir koşul açık değil" | **Çıkar** | `S76`-6: canlı hesap |
| Dönem kartı "Detayı Gör", baz ↔ senaryo dönem ayrıntısı | **Çıkar** | `S76`-9, `GS28` (d) |
| Katalogdaki tür açıklamaları, "Plan türü" grup seçici | **Çıkar** | GK5; tür "Ekle" seçicisinde seçilir. *`V10c`'de geri alınır: "Ekle" Finansal Yapı'nın tür karolarına (`EntryTypeTiles`, `GS29`) geçer, alt satırlar ≤ 24 harflik `Etiket_` (`S77`-7).* |
| Eyebrow + açıklama cümleleri, "1 · / 2 ·" başlıkları, "Gelir kullanımı" | **Çıkar** | Başlık yeter; `S18` |
| Spinner, durum satırı | **Çıkar** | `SkeletonBlock` / `StateBlock` (`GS14`) |

### 3. Bütçe

```
SimulatorPage                        SimulationConditionPage (en dolu tür)
Hero rakam    1 / 1                  Hero rakam   0 / 1
Hero yüzey    0 / 1  (SurfaceChart)  Kart         0 / 4   form kartı ham Border
Kart          3 / 4                  Grafik       0 / 1
Grafik        1 / 1                  Label       12 / 28  alan etiketleri (ad, kart|kredi|gelir ayrı
NavRow        0 / 5                                        + salt okunur adı, ödeme şekli, kapsam, tutar,
Label        15 / 28                                       tarih, sayı, ilk ödeme, toplam geri ödeme)
Cumle_        1 / 3                  Cumle_       0–1 / 3 (erken ödeme açıklaması, V10d kararı)
```

Simülatör sayfası: kart = sonuç kartı (ham `Border`), denemeler `ListCard`, dönem sonları (ham `Border`); analizci
yalnız `ListCard`'ı sayar. Label = sonuç kartı 8 (eyebrow, hero, dönem, "şu anki gidişata göre" + tutar, iki uç
tarih, "12 dönem sonra" eyebrow'u), deneme şablonu 3 (ad, bağlam, tutar), boş liste cümlesi 1, ızgara 3 (eyebrow,
ay, tutar). `MetricRow` ve `ComparisonStrip` bileşen; sayfada `<Label>` değiller. Eski 64 + 17 etiketten 15 + 11'e.

`V10a` sonunda: simülatör sayfası hero 0, kart 1 (denemeler `ListCard`), grafik 0, label 4 (analizci 7:
`<Label.Triggers>`'ı da sayıyor), cümle 1; deneme formu label 3, kart 0 (form kartı ham `Border`), cümle 0. Aşama 4'teki
`V10a` payıyla aynı; sonuç kartı (8) ve ızgara (3) `V10b`'de.

`V10b` iki alt adım (2026-10-03, Aşama 1: ~500 satır). **`V10b1`** sonuç kartının üst yarısını getirir: hero, fark
satırı, grafik, iki uç tarih (7 etiket) → simülatör sayfası hero 1/1, kart 2/4 (sonuç kartı ham `Border` +
denemeler `ListCard`), grafik 1/1, label 11/28 (analizci 15: `<Label.Triggers>`'ı da sayıyor), cümle 1/3, NavRow 0/5.
**`V10b2`** kartın alt yarısını (`ComparisonStrip` "12 dönem sonra", faiz `MetricRow`'ları, "12 dönem sonra" eyebrow'u:
+1 etiket) ve dönem sonları ızgarasını (kart 3/4, +3 etiket) ekler → toplam yukarıdaki tablo.

### 4. Blok şeması

**`SimulatorPage.xaml`**

```
┌─ PageHeader ────────────────────────────────────────────────┐
│ Simülatör                 Baslik_Simulator        [ Ekle ]  │  ← S3; Aksiyon_Ekle → tür seçici
└─────────────────────────────────────────────────────────────┘    (IDialogService; V10a'da tek seçenek)
┌─ Sonuç kartı (ham Border)  SurfaceChart / BorderSubtle / RadiusHero / CardPadding ┐  V10b1 (iki uç tarihe kadar)
│ Etiket_EnDusukDonemSonu                  Eyebrow            │  ← S1
│ 4.200 ₺                                  HeroFigure / TextPrimary (eksi de olsa); denemeyle
│ 10 Mart 2027 dönemi sonunda              Bicim_EnDusukDonem, Caption / TextSecondary
│ Şu anki gidişata göre   −30.000 ₺        Caption + Caption işaretli: eksi NegativeText,
│                                          artı PositiveText, sıfır TextSecondary · yalnız açık deneme varsa
│ AreaTrend  ChartHeight                                      │  ← S1, S2
│   Series: denemeyle, zincir başı + 12 dönem sonu, düz Indicator, dolgu sıfıra (GS26-4)
│   Karşılaştırma: şu anki gidişat, planned → TextSecondary kesikli, dolgusuz (GS28-2)
│   Threshold 0: NegativeText kesikli (GS26-3)
│ 10 Eki 2026                     9 Eki 2027    Caption ×2: zincir başı · 12. dönemin son günü
│ ── buradan aşağısı V10b2 ──                                 │
│ ── açık deneme yokken: 12 Dönem'in kartının aynısı ──       │
│ MetricRow Etiket_OnIkiDonemSonra ............... 52.300 ₺   │  ← S2; eksiyse Negative
│ ── açık deneme varken ──                                    │
│ Etiket_OnIkiDonemSonra                   Eyebrow            │  ← S2
│ ComparisonStrip  Etiket_SuAn · Etiket_Denemeyle · Etiket_Fark
│   52.300 ₺ (PlanSurface) | 22.300 ₺ (ActualSurface) | −30.000 ₺ (Negative / Positive / Default)
│ MetricRow Etiket_OnIkiDonemdeFaiz .................. 480 ₺  │  ← S4; denemeyle, 0 ₺ dahil hep
│ MetricRow Etiket_FaizFarki ........................ +480 ₺  │  ← S4; yalnız ≠ 0; artı Negative, eksi Positive
│ MetricRow Etiket_KrediFaizKazanci ............... 6.200 ₺  │  ← S4; V10d, erken ödeme varsa; Positive
│ MetricRow Etiket_KredininMaliyeti ............... 4.800 ₺  │  ← S4; V10d, kredi çekme varsa
└─────────────────────────────────────────────────────────────┘
┌─ ListCard  Etiket_Denemeler  (deneme yoksa görünmez) ───────┐  V10a  ← S3
│ Telefon                                  30.000 ₺    [●  ]  │  TypeBody / TextPrimary · Figure · aç/kapa
│ Nakit ödeme · 15 Kas 2026                                   │  Caption: DenemeBaglamiConverter (tür · tarih)
│ Tatil                                    45.000 ₺    [  ○]  │  kapalı: ad ve tutar TextSecondary
│ Kartla harcama · 6 taksit · 1 Ara                           │
│ Kurs                                     12.000 ₺    [  ○]  │  geçersiz: anahtar pasif, bağlamın yerine
│ Tarihi geçti                                                │  Etiket_TarihiGecti / Etiket_KaydiSilindi (NegativeText)
│ … 4 satır + "+N daha" yerinde (GS21)                        │
│ satıra dokun → diyalog: Düzenle / Sil (V6a deseni)          │
└─────────────────────────────────────────────────────────────┘
  Cumle_DenemeYok              Caption / TextSecondary; deneme yokken listenin yerinde  ← S3
  [ Planıma ekle ]             Button (ActionFill); yalnız açık ve geçerli deneme varsa  ← S5, V10f
┌─ Dönem sonları (ham Border)  SurfaceCard / BorderSubtle / RadiusCard ┐  V10b2  ← S1
│ Etiket_DonemSonlari                      Eyebrow            │
│ 3 × 4 karo: ay (Caption) + denemeyle dönem sonu (TypeFigure; en düşük SemiBold, eksi NegativeText)
│ 12 Dönem'in karo şablonunun aynısı (GS26-1); dokunma yok (S76-9)
└─────────────────────────────────────────────────────────────┘
```

Sıra: önce cevap (sonuç kartı), sonra girdiler (denemeler), en altta ikinci seviye (dönem sonları). Açık deneme
yokken sonuç kartı ve ızgara 12 Dönem'inkiyle kuruşu kuruşuna aynıdır (`S76`-1).

Aç/kapa satırdaki MAUI `Switch`'tir (Kapı B, 2026-10-03): tek dokunuşla kıyas. Uygulamadaki ilk `Switch`;
`Styles.xaml`'a örtük stil gelir, açık hâlin rengi `Indicator` (`DynamicResource`). Satırın geri kalanına dokunmak
Düzenle / Sil diyaloğunu açar.

**`SimulationConditionPage.xaml`**

```
┌─ PageHeader ────────────────────────────────────────────────┐
│ Nakit ödeme              türün başlığı (tür anahtarı → Baslik_*)
└─────────────────────────────────────────────────────────────┘
┌─ Form kartı (Border, örtük stil) ───────────────────────────┐  ← S3
│ Etiket_Ad                Eyebrow   [ Telefon          ]     │  hepsi; zorunlu (S76-8)
│ Etiket_Kart | Etiket_Kredi | Etiket_Gelir   üst kaydın adı  │  türe göre (V10c–e); salt okunur, seçimi
│                                             (Label)         │  tür seçici yapar (S77 V10 notları i)
│ Etiket_OdemeSekli · Etiket_Kapsam           Picker          │  kart ödeme şekli, erken ödeme (V10c, V10d)
│ ┌ tutar etiketi (türe göre) ─┐ ┌ tarih etiketi (türe göre) ┐│  Grid 2 sütun
│ │ [ 30.000        ] Numeric  │ │ [ 15.11.2026 ] DatePicker ││  tarih en erken bugün (S76-3)
│ └────────────────────────────┘ └───────────────────────────┘│
│ sayı etiketi (taksit / ödeme / ay)  ·  Etiket_IlkOdemeTarihi│  V10c, V10d
│ Etiket_ToplamGeriOdeme                                      │  V10d (kredi çekme)
└─────────────────────────────────────────────────────────────┘
[ Kaydet ]  [ Vazgeç ]   Kaydet listeye yazar ve geri döner; Vazgeç, geri ok ve cihazın geri tuşu
                         değişiklik varsa onay sorar (EK-V6b deseni). Doğrulama hatası diyalogla.
```

Alan blokları türler arasında ortaktır: etiketin metni türe göre değişir (converter), blok tekrarlanmaz; böylece
dokuz tür tek sayfada 12 etikette kalır. `V10a`'da yalnız ad, tutar ve tarih vardır.

**Üst kayıt** *(2026-10-04, `S77` V10 notları i; Kapı B'deki açılır liste kalktı)*: kartla harcama ve kart ödeme
şekli kartı (`V10c`), krediye erken ödeme krediyi (`V10d`), gelir değişikliği düzenli geliri (`V10e`) ister. Seçimi
formdaki açılır liste değil simülatörün tür seçicisi yapar, Finansal Yapı'daki gibi (`EK-V6f`): adaylar Finansal
Yapı'nın süzgecinden, tek adayda sorulmaz, birden fazlasında yerinde "Hangi kart?", geri karolara döner. Aday yoksa
yalnız uyarı ("önce kartını Finansal Yapı'dan ekle"); form açılmaz, üst kaydın formu önerilmez — simülatör planı
değiştirmez (`S76`). Form üst kaydın kimliğiyle açılır, adını salt okunur satırda gösterir; düzenlemede tür gibi üst
kayıt da değişmez.

### 5. Üç durum

| Durum | Görünen |
|---|---|
| Boş | `StateBlock`: `Schedule` + `Bos_SimulatorYok` + `Aksiyon_FinansalYapiyaGit` — açık dönem yok ya da zincir kurulamıyor (`S76`-1, `V10b`). Deneme yoksa sayfa boş değildir: sonuç kartı şu anki gidişatla, listenin yerinde `Cumle_DenemeYok`. `StateBlock` **sonuç kartının yerinde** durur (`S76` V10b notları f): deneme listesi ve "Ekle" kalır, kullanıcı denemelerini silebilir. |
| Yükleniyor | `SkeletonBlock`'lar: sonuç kartı (`ChartHeight`) ve liste; spinner yok (`GS14`). Canlı yeniden hesapta iskelet yok: eski sonuç yenisi gelene kadar yerinde kalır. Form: yalnız düzenlemede iki iskelet. |
| Hata | Liste okunamazsa `StateBlock`: `Close` + `Hata_SimulatorAcilamadi` + `Aksiyon_TekrarDene`. Hesap düşerse yalnız sonuç kartının yerinde `StateBlock` (`Hata_SimulasyonHesaplanamadi` + `Aksiyon_TekrarDene`); liste kullanılabilir kalır, bozuk deneme silinebilir. Form: deneme bulunamazsa diyalog ve geri. |

### 6. Konsept ilişkisi

Yerleşim konseptinin "Simülatör" paneli tarifiyle biliniyor (dönem başına `MetricRow` + "Detay Gör"); görüntüsü repoda
yok. Ekran 12 Dönem'den türetildi (kullanıcı kararı, 2026-10-03): sonuç kartı gidişat kartının deseni, ızgara aynı
şablon, kıyas `ComparisonStrip` (tasarım sisteminde baz ↔ senaryo için duruyor, ilk kullanımı), listeler `ListCard`,
form V6 formlarının deseni. Konseptten sapmalar `GS28`'de.

### EK-V11 — Dönem kapanışı: özet sayfası

> Sayfa dosyası: `PeriodSettlementPage.xaml` (`Routes.PeriodSettlement`)
> Durum: tamamlandı (Aşama 8)
> Konsept karşılığı: `docs/assets/konsept/ana-sayfa-rota-tempo-kapanis.png` (ve `anasayfa-donemi-kapat.png`, `anasayfa-donemi-kapat-koyu.png`).
> Eski hâl: `PeriodReviewPage.xaml` (515 satır, 50+ `<Label>`, 5 adımlı sihirbaz).
> Yeni hâl: Tek sayfada özet mutabakatı ve kapanış onayı (`GS31`, `S78`).

#### 1. Sorular

| Kod | Soru | Eski sihirbaz nasıl cevaplıyordu |
|---|---|---|
| S1 | "Dönem kaç lirayla kapandı, planladığımdan ne kadar saptım?" | Adım 5'te bakiye mutabakatı ve döküm kartında; fark açıkça öne çıkmıyordu |
| S2 | "Planımla gerçek arasındaki fark nereden kaynaklandı?" | Adım 1–4 boyunca gelir, ödeme, yaşam harcaması ve faizleri tek tek sorarak |
| S3 | "Kapanış bakiyesi doğru mu, değiştirmem gerekir mi?" | Adım 5'te bakiye giriş alanı ile |
| S4 | "Dönemi şimdi kapatırsam ne olur?" | Sihirbaz sonu onay butonu |
| S5 | "Şimdi kapatmak istemiyorsam ne yapabilirim?" | Sihirbazdan iptal ile çıkış |

#### 2. Kesme kararları (`GS31`)

| Bilgi / Öğe | Karar | Gerekçe |
|---|---|---|
| Dönem sonu gerçekleşen bakiye | **Hero** (`HeroFigure`, `TypeHero`) | S1 ekranın ana sayısı |
| Plana göre fark + plan tutarı | **Satır** (`TypeCaption`, `NegativeText`/`PositiveText`) | S1 sapma yönü ve büyüklüğü |
| Plan / Gerçekleşen çubukları (`StackedBar`) | **Çıkar** | Fark çubukta hissedilmez, alt kart aynı veriyi TL olarak verir |
| Konseptteki Gelir satırı | **Çıkar** | Gelir planlandığı gibi kabul edilir (`S31`, `S78`) |
| "Farkın kaynağı" kartı | **Kart** (`SurfaceCard`, `BorderSubtle`, `RadiusCard`, `CardPadding`) | S2 farkın kaynakları (yaşam gideri, ödemeler, KMH) |
| Yaşam gideri satırı | **Satır** (kart içinde) | S2 planlanan / harcanan ve fark |
| Ödemeler satırı | **Satır** (kart içinde) | S2 ödenen adet ve tutar farkı |
| KMH faizi satırı | **Satır** (kart içinde, eksi bakiye varsa) | S2 faiz maliyeti |
| Kapanış bakiyesi teyidi + [Değiştir] | **Satır** (kart içi alt satır) | S3 bakiye teyidi; düzenleme diyalogla, gözlem yazmaz |
| Yeni dönem başlangıç takvimi notu | **Satır** (`TypeCaption`, `TextSecondary`) | S4 yeni dönemin başlangıç günü |
| [Dönemi kapat] | **Aksiyon** (`ActionFill`, tam genişlik) | S1, S4 kapanış onayı |
| [✕] erteleme aksiyonu | **Aksiyon** (üst bar `BackButtonBehavior` / `Icons.Close`) | S5 erteleme |

#### 3. Bütçe

```
Hero rakam    1 / 1     Dönem sonu bakiyesi (HeroFigure)
Hero yüzey    0 / 1     SurfaceCard zemin kullanılır (T9 kontrast kararı)
Kart          2 / 4     Dönem sonu özet kartı (SurfaceCard), Farkın kaynağı kartı (SurfaceCard)
Grafik        0 / 1     StackedBar çıkarıldı (0/1)
NavRow        0 / 5     Yok
Label        14 / 28    Başlık (2), Hero kartı (3), Farkın kaynağı kartı (8), Alt not (1)
Cumle_        1 / 3     Cumle_DonemGecmiseTasinir (≤ 90 kr)
```

#### 4. Blok şeması

```
┌─ Shell ────────────────────────────────────────────────────────┐
│ [✕] BackButtonBehavior IconOverride: Icons.Close, IconLarge,   │  ← S5
│     TextPrimary; Command: CloseCommand                         │
│ Shell.TitleView:                                               │
│   Baslik_DonemiKapat (SectionTitle)  Bicim_DonemAraligi (Cap)  │  ← S1
└────────────────────────────────────────────────────────────────┘
┌─ Dönem Sonu Özet Kartı (SurfaceCard / BorderSubtle / RadCard) ─┐  ← S1
│ Etiket_DonemSonu                         TypeEyebrow           │
│ 42.180 ₺                                 HeroFigure / TextPrim │
│ Plana göre −1.720 ₺  ·  Plan 43.900 ₺    TypeCaption           │
│   (fark NegativeText/PositiveText, plan TextSecondary)         │
└────────────────────────────────────────────────────────────────┘
┌─ Farkın Kaynağı Kartı (SurfaceCard / BorderSubtle / RadCard) ──┐  ← S2
│ Etiket_FarkinKaynagi                     TypeSection           │
│                                                                │
│ Yaşam gideri                                            +900 ₺ │
│ Planlanan 29.650 ₺ · harcanan 28.750 ₺                         │
│                                                                │
│ Ödemeler                                                   0 ₺ │
│ 7 ödemenin 7'si ödendi                                         │
│                                                                │
│ KMH faizi                                               −120 ₺ │  (varsa)
│ ────────────────────────────────────────────────────────────── │
│ Kapanış bakiyesi 42.180 ₺ · 9 Ekim                 [ Değiştir ]│  ← S3
└────────────────────────────────────────────────────────────────┘
│ Dönem Geçmiş dönemler'e taşınır; yeni dönem 10 Ekim'de başlar. │  ← S4
│ Cumle_DonemGecmiseTasinir  TypeCaption / TextSecondary         │
│                                                                │
[ Aksiyon_DonemiKapat                      ActionFill, tam genişlik ] ← S1, S4
```

#### 5. Üç durum

| Durum | Görünen |
|---|---|
| Boş | Kapatılmaya hazır dönem yoksa `StateBlock`: `Check` ikonu + `Cumle_KapatilacakDonemYok` + `Aksiyon_AnaSayfayaDon`. |
| Yükleniyor | İki kart şeklinde `SkeletonBlock` (özet kartı ve fark kartı yüksekliğinde); spinner yok (`GS14`). |
| Hata | `StateBlock`: `Close` ikonu + `Hata_DonemKapanisiYuklenemedi` + `Aksiyon_TekrarDene`. |

#### 6. Konsept ilişkisi

Claude Design konsepti: `docs/assets/konsept/ana-sayfa-rota-tempo-kapanis.png` (ve `anasayfa-donemi-kapat.png`, `anasayfa-donemi-kapat-koyu.png`). Yerleşim oradan türetildi, renkler token'lardan (`T1`). Konseptten sapmalar `GS31`'de: StackedBar çubukları bilgi eklemediği için çıkarıldı, Gelir satırı kullanıcı kararıyla (`S31`, `S78`) kaldırıldı, zemin koyu tema kontrastı için `SurfaceCard` seçildi.


## EK-V12 — Geçmiş + geçmiş ayrıntısı

> Sayfa dosyaları: `HistoryPage.xaml` (Sayfa 1: liste ve net değişim özeti), `HistoryDetailPage.xaml` (Sayfa 2: karne, bakiye çizgisi ve döküm)
> Rota: `//history` (Flyout), `history-detail` (`Routes.HistoryDetail` + `actualId`)

**Eski proje:** `HistoryPage.xaml` 50 satır / 19 `<Label>` / 4 `Border`; `HistoryDetailPage.xaml` 40 satır / 35 `<Label>` / 8 `Border`. Toplam: 54 `<Label>`, 12 `Border`.

### 1. Sorular

| Kod | Soru | Önceki hâl nasıl cevaplıyordu |
|---|---|---|
| S1 | "Son dönemlerde genel olarak planıma göre neredeyim, toplamda ne kadar saptım?" | 3 sütunlu stok toplamı (7 etiket, bakiye stoklarını topladığı için yanıltıcıydı, `S29`) |
| S2 | "Geçmiş dönemlerim nasıl kapandı?" | CollectionView içinde kartlar (kart başına 7 etiket, renkli rozetler) |
| S3 | "Seçtiğim dönemde bakiye gün gün nasıl aktı, dalgalanma nereden kaynaklandı?" | Cevaplamıyordu; kapanışta gözlemler siliniyordu, grafik yoktu (`S68`) |
| S4 | "O dönemde plandan neden saptım, fark nereden kaynaklandı?" | 3 mini kart + yaşam giderleri kartı + kategori farkları + yeni durum kartı (20+ etiket) |
| S5 | "O dönemde hangi borçlarımı/ödemelerimi ödedim, hangileri aksadı?" | Ödemeler listesi (öğe başına 7 etiket) |

### 2. Kesme kararları (`GS32`)

| Bilgi / Öğe | Karar | Gerekçe |
|---|---|---|
| Son dönemler net değişim özeti | **Kart** (`SurfaceCard`, `BorderSubtle`, `RadiusCard`, `CardPadding`), Sayfa 1 | S1 makro karnesi; bakiye stoku değil net değişim (`S29`) |
| Planlanan / Gerçekleşen net değişim ve Fark | **Satır** (`MetricRow` × 3, kart içinde) | S1 sayısal karşılıkları; fark işaretli ve semantik renkli |
| Kapanmış dönemler listesi | **Kart** (`SurfaceCard` öğeler), Sayfa 1 | S2 dönem listesi |
| Dönem aralığı + durum rozeti | **Satır** (kart başlığı: `TypeSection` + `RadiusChip`) | S2 takvim bağlamı ve başarı durumu |
| Kapanış bakiyesi + plana göre fark | **Satır** (kart içinde: `TypeHeading` + `TypeCaption`) | S2 dönemin kapanış sayısı ve sapması |
| Ayrıntıya geçiş oku | **İkon** (`ChevronRight`, kart başına tek ikon) | S2 dokunarak ayrıntıya gitme |
| Dönem sonu gerçekleşen bakiye | **Hero** (`HeroFigure`, `SurfaceCard`), Sayfa 2 | S4 dönemin kapanış sayısı |
| Plana göre fark + plan tutarı | **Satır** (hero altında, semantik renkli) | S4 sapma |
| Dönemin bakiye çizgisi | **Grafik** (`ChartCard`, yükseklik `ChartHeight`), Sayfa 2 | S3 bakiye seyri (`S68-6, 7`, `T4`) |
| Açılış bakiyesi | **Satır** (`ChartCard` lejant çipi, `TypeEyebrow`) | S3 grafiğin başlangıç noktası |
| Farkın kaynağı kartı | **Kart** (`SurfaceCard`, `V11` ile aynı dil), Sayfa 2 | S4 sapmanın kaynakları |
| Yaşam gideri gerçekleşmesi | **Satır** (kart içi, planlanan / harcanan ve fark) | S4 |
| Ödemeler gerçekleşmesi | **Satır** (kart içi, planlanan / ödenen ve fark) | S4 |
| KMH faizi gerçekleşmesi | **Satır** (kart içi, eksi bakiye olduysa faiz) | S4 |
| Mutabakat düzeltmesi | **Satır** (kart içi, varsa kasa düzeltmesi) | S4 |
| Ödemeler gerçekleşme listesi | **Kart** (`ListCard`, ≤ 4 satır + yerinde açılır "+N daha"), Sayfa 2 | S5 tekil borç ödemelerinin durumu (`S75-7`) |
| Kapanış notu / özet cümlesi | **Satır** (`TypeCaption`, `TextSecondary`) | Karşılaştırma özeti (`Comparison.Summary`) |
| "PLAN vs GERÇEK" sloganı | **Çıkar** | GK5; bilgi taşımıyor |
| "Geçmiş planlar donmuş halde kalır..." notu | **Çıkar** | GK5; sistem açıklaması |
| "Yeni Güncel Durum" kartı | **Çıkar** | Gelecek dönemin açılışı geçmişin sorusu değildir |
| "Kategori Farkları" dökümü | **Çıkar** | v2'de mikro harcama fişi takibi yoktur (`S20`) |
| Revizyon sayacı notu | **Çıkar** | GK5; dondurulan nihai plana göre karne tek ve nettir (`I27`) |

### 3. Bütçe

**Sayfa 1 — Geçmiş Listesi (`HistoryPage`):**
```
Hero rakam    0 / 1     Listeleme ekranı, hero rakam yok
Hero yüzey    0 / 1     Yok
Kart          2 / 4     Özet kartı (SurfaceCard), Dönem listesi kartları (DataTemplate)
Grafik        0 / 1     Liste ekranında grafik yok
NavRow        0 / 5     Yok; kartların kendisi ChevronRight ile tıklanabilir
Label         9 / 28    Başlık (1), Özet kartı (4), Liste öğe şablonu (4)
Cumle_        1 / 3     Boş durum / özet cümlesi
```

**Sayfa 2 — Geçmiş Ayrıntısı (`HistoryDetailPage`):**
```
Hero rakam    1 / 1     Gerçekleşen dönem sonu bakiyesi (HeroFigure)
Hero yüzey    0 / 1     SurfaceCard zemin kullanılır (T9 kontrast kararı)
Kart          3 / 4     Dönem sonu kartı, Farkın kaynağı kartı, Ödemeler kartı (ListCard)
Grafik        1 / 1     Bakiye seyri (ChartCard içinde trend)
NavRow        0 / 5     Yok
Label        17 / 28    Başlık (2), Hero kartı (3), Farkın kaynağı (6), Ödeme satır şablonu (4), Kapanış notu (2)
Cumle_        1 / 3     Karşılaştırma özet cümlesi (Comparison.Summary)
```

### 4. Blok şeması

#### Sayfa 1 — Geçmiş Listesi (`HistoryPage`)
```
┌─ Shell.TitleView ────────────────────────────────────────────┐  ← S1
│ [☰ kabuk]  Geçmiş                                             │
│            Baslik_Gecmis  TypeSection / TextPrimary          │
└──────────────────────────────────────────────────────────────┘
┌─ Son Dönemler Özeti (ham Border)  SurfaceCard / BorderSubtle ┐  ← S1
│ Etiket_SonDonemlerOzeti        TypeEyebrow / TextSecondary   │
│ MetricRow  Etiket_PlanlananNetDegisim ........ +12.000 ₺     │  TextPrimary / PositiveText
│ MetricRow  Etiket_GerceklesenNetDegisim ...... +11.500 ₺     │  TextPrimary / PositiveText
│ ──────────────────────────────────────────────────────────── │  BorderSubtle Hairline
│ MetricRow  Etiket_NetFark ...................... −500 ₺      │  TextPrimary / NegativeText
└──────────────────────────────────────────────────────────────┘
┌─ Dönem Kartı (ham Border)  SurfaceCard / BorderSubtle / RadCard ┐  ← S2
│ 10 Ağustos – 9 Eylül 2026           [ Planın üzerinde ]      │  TypeSection · RadiusChip
│                                                              │
│ 41.500 ₺                                           › (Chevron│  TypeHeading · IconMedium
│                                                              │
│ Plana göre +500 ₺  ·  Plan 41.000 ₺                          │  TypeCaption (fark semantik)
└──────────────────────────────────────────────────────────────┘
```

#### Sayfa 2 — Geçmiş Ayrıntısı (`HistoryDetailPage`)
```
┌─ Shell.TitleView ────────────────────────────────────────────┐  ← S4
│ [← geri]  10 Ağustos – 9 Eylül 2026                          │
│           Bicim_DonemAraligi  TypeSection / TextPrimary      │
└──────────────────────────────────────────────────────────────┘
┌─ Dönem Sonu Kapanışı (ham Border)  SurfaceCard / RadCard ────┐  ← S4
│ Etiket_DonemSonu                         TypeEyebrow         │
│ 41.500 ₺                                 HeroFigure / TextPr │
│ Plana göre +500 ₺  ·  Plan 41.000 ₺      TypeCaption (fark sem)
└──────────────────────────────────────────────────────────────┘
┌─ ChartCard (SurfaceCard / BorderSubtle / RadiusCard) ────────┐  ← S3
│ Title: Etiket_BakiyeSeyri  LegendText: Açılış 50.000 ₺ (Chip)│
│ ╭──────────────────────────────────────────────────────────╮ │
│ │ AreaTrend (veya rota çizgisi)  yükseklik ChartHeight     │ │
│ │   Açılıştan kapanışa günlük bakiye çizgisi (actual),     │ │
│ │   gözlem noktaları, plan eşik çizgisi                    │ │
│ ╰──────────────────────────────────────────────────────────╯ │
│ 10 Ağustos                                          9 Eylül  │  TypeCaption / TextSecondary
└──────────────────────────────────────────────────────────────┘
┌─ Farkın Kaynağı Kartı (ham Border)  SurfaceCard / RadCard ───┐  ← S4
│ Etiket_FarkinKaynagi                     TypeSection         │
│                                                              │
│ Yaşam gideri                                          +300 ₺ │  TypeBody / TextPrimary · Figure
│ Planlanan 25.000 ₺ · harcanan 24.700 ₺                       │  TypeCaption / TextSecondary
│                                                              │
│ Ödemeler                                              +200 ₺ │  TypeBody / TextPrimary · Figure
│ 5 ödemenin 5'i ödendi                                        │  TypeCaption / TextSecondary
│                                                              │
│ KMH faizi (varsa)                                     −120 ₺ │  TypeBody / NegativeText · Figure
│ ──────────────────────────────────────────────────────────── │  BorderSubtle Hairline
│ Kasa düzeltmesi (varsa)                               +150 ₺ │  TypeCaption / TextSecondary
└──────────────────────────────────────────────────────────────┘
┌─ ListCard ───────────────────────────────────────────────────┐  ← S5
│ Etiket_Odemeler                        5 ödeme · 18.500 ₺    │
│ Kira                                      15.000 ₺  [Ödendi] │  TypeBody · Figure · RadiusChip
│ 15 Ağustos                                                   │  TypeCaption / TextSecondary
│ … en fazla 4 satır                                           │
│ +N daha                                (yerinde açılır)      │
└──────────────────────────────────────────────────────────────┘
│ Cumle_GecmisDonemOzeti  TypeCaption / TextSecondary          │  ← S1, S4
```

### 5. Üç durum

| Sayfa | Durum | Görünen |
|---|---|---|
| **Sayfa 1 (Geçmiş)** | Boş | Henüz kapatılmış dönem yoksa `StateBlock`: `Schedule` ikonu + `Cumle_KapanmisDonemYok` + `Aksiyon_AnaSayfayaDon`. |
| | Yükleniyor | İki kart şeklinde `SkeletonBlock` (özet kartı ve liste kartı yüksekliğinde); spinner yok (`GS14`). |
| | Hata | `StateBlock`: `Error` ikonu + `Hata_GecmisYuklenemedi` + `Aksiyon_TekrarDene`. |
| **Sayfa 2 (Ayrıntı)** | Boş | İlgili dönemin gerçekleşme kaydı bulunamazsa `StateBlock`: `Help` ikonu + `Cumle_DonemKaydiBulunamadi` + `Aksiyon_GeriDon`. |
| | Yükleniyor | Üç kart şeklinde `SkeletonBlock` (hero kartı, grafik kartı ve fark kartı yüksekliğinde). |
| | Hata | `StateBlock`: `Error` ikonu + `Hata_DonemAyrintisiYuklenemedi` + `Aksiyon_TekrarDene`. |

### 6. Konsept ilişkisi

Planör'ün 5 temel konsept panelinden "Geçmiş" panelidir. `ChartCard` (başlık + lejant çipi) ve tek grafikli trend görünümü bu konseptten doğmuştur (`T4`, `GS13`). Konseptten sapmalar `GS32`'dedir: bakiye stokları yerine net değişim toplamı (`S29`), mikro fiş kategorileri ve sonraki dönem verisinin çıkarılması (`S20`), kapanan dönemin gözlemleriyle bakiye seyrinin `ChartCard` üzerinde çizilmesi (`S68-6, 7`).

### EK-V13 — Ayarlar + düzen değişikliği

`SettingsPage.xaml` sayfasının ekran kartıdır.

### 1. Sorular

1. **S1: "Dönemimin döngüsü ve serbest harcama havuzum ne?"** (Dönem başlangıç günü 1–31 ve dönemsel yaşam gideri bütçesi)
2. **S2: "Gelecek tahminlerinde hangi faiz oranları varsayılıyor?"** (Kredi kartı devreden borç faizi % ve finansman açığı KMH faiz oranı %)
3. **S3: "Borç hatırlatıcı bildirimlerini hangi sıklıkla almak istiyorum?"** (Bildirim modu: Kapalı / Rahat / Agresif)
4. **S4: "Verilerim en son ne zaman yedeklendi ve şimdi yedek alabilir miyim?"** (Son yedek tarihi, izin durumu, Şimdi Yedekle aksiyonu)
5. **S5: "Hangi sürümü kullanıyorum ve verilerim nerede saklanıyor?"** (Uygulama adı: Planör, sürüm numarası, yerel saklama güvencesi)

### 2. Kesme kararları

| Bilgi / Öğeler | Karar | Gerekçe |
|---|---|---|
| Dönem başlangıç günü (`AnchorDay`) | **Satır / Giriş** | S1: Nakit akış döneminin döngü günü |
| Dönem yaşam gideri havuzu | **Satır / Giriş** | S1: Serbest harcama havuzu bütçesi |
| Kredi kartı devreden faiz oranı (%) | **Satır / Giriş** | S2: Projeksiyon akdi faiz varsayımı |
| KMH finansman açığı faiz oranı (%) | **Satır / Giriş** | S2: Eksi bakiye borçlanma faiz varsayımı |
| Bildirim modu çipleri (Kapalı / Rahat / Agresif) | **Satır / Seçici** | S3: Hatırlatıcı bildirim sıklığı tercihi (`S4`) |
| Son yedekleme durumu | **Satır** | S4: En son yedekleme zamanı ve dosya adı |
| Yedekleme açıklaması | **Satır** (`Cumle_`) | S4: Cihaz içi yedekleme politikası açıklaması |
| Yedek klasörü izin uyarısı ve "İzin Ver" | **Aksiyon** | S4: Harici depolama izni gerekliyse |
| "Şimdi Yedekle" butonu | **Aksiyon** | S4: Kullanıcının anlık tam yedek alması |
| Planör adı, sürüm ve yerel saklama güvencesi | **Satır** | S5: Sürüm ve "Veriler yalnızca bu cihazda saklanır" güvencesi |
| "Değişiklikleri Kaydet" | **Aksiyon** | Genel: Değişiklikleri kalıcı veri tabanına kaydeder |
| Gelir kullanım düzeni ve geçmişi | **Çıkar** | S18: Yapay tahsis elendi, doğal dönemsellik geçerli |
| Düzen değiştir modal sayfası (`StrategyChangePage`) | **Çıkar** | S18: Tahsis düzeni seçimi yoktur |
| Geliştirici test verisi yükle / sil | **Çıkar** | Geliştirici artığı |
| Açılış bakiyesi | **Çıkar** | S19: Kurulum çapa gününe kenetlenir, Ayarlar'dan düzenlenmez |
| `StatusMessage` hata etiketi | **Çıkar** | Hata ve bildirimler diyalog servisi ile verilir |

### 3. Bütçe

```
Hero rakam    0 / 1
Hero yüzey    0 / 1
Kart          0 / 4
Grafik        0 / 1
NavRow        0 / 5
Label        14 / 28
Cumle_        2 / 3
```

### 4. Blok şeması

`SettingsPage.xaml`:
```
┌─ PageHeader ────────────────────────────────────────────────────────┐
│ Baslik_Ayarlar (TypeTitle, TextPrimary)                             │
└─────────────────────────────────────────────────────────────────────┘
┌─ Bölüm: Planlama Ayarları (VerticalStackLayout) ────────────────────┐
│ Baslik_PlanlamaAyarlari (TypeSection, TextPrimary)                  │
│                                                                     │
│ Etiket_DonemGunu (TypeEyebrow, TextSecondary)                 ← S1 │
│ [ 10                             ] (Entry, Numeric)                 │
│                                                                     │
│ Etiket_YasamGideri (TypeEyebrow, TextSecondary)               ← S1 │
│ [ 20.000,00                      ] (Entry, Numeric)                 │
│                                                                     │
│ Etiket_KartDevredenFaiz (TypeEyebrow, TextSecondary)          ← S2 │
│ [ 5,00                           ] (Entry, Numeric)                 │
│                                                                     │
│ Etiket_FinansmanAcigiFaizi (TypeEyebrow, TextSecondary)       ← S2 │
│ [ 5,00                           ] (Entry, Numeric)                 │
└─────────────────────────────────────────────────────────────────────┘
┌─ Bölüm: Hatırlatıcı Bildirimleri ───────────────────────────────────┐
│ Baslik_HatirlaticiBildirimleri (TypeSection, TextPrimary)           │
│ [ Kapalı ]  [ Rahat ]  [ Agresif ] (Seçici Çipler)             ← S3 │
└─────────────────────────────────────────────────────────────────────┘
┌─ Bölüm: Yedekleme ──────────────────────────────────────────────────┐
│ Baslik_Yedekleme (TypeSection, TextPrimary)                         │
│ Son yedek: 4 Ekim 2026 22:45 · Mizan-yedegi-… (TypeBody)     ← S4 │
│ Cumle_YedekAciklamasi (TypeCaption, TextSecondary)             ← S4 │
│ [ İzin Ver ] (Gerekliyse, ActionOutline)                      ← S4 │
│ [ Şimdi Yedekle ] (SecondaryAction, TextPrimary)               ← S4 │
└─────────────────────────────────────────────────────────────────────┘
┌─ Bölüm: Hakkında ───────────────────────────────────────────────────┐
│ Baslik_Hakkinda (TypeSection, TextPrimary)                          │
│ Planör (TypeSection, TextPrimary)                              ← S5 │
│ Sürüm 2.0 (TypeCaption, TextSecondary)                        ← S5 │
│ Cumle_VeriGuvenligi (TypeCaption, TextSecondary)               ← S5 │
└─────────────────────────────────────────────────────────────────────┘
┌─ Kaydet Aksiyonu ───────────────────────────────────────────────────┐
│ [ Değişiklikleri Kaydet ] (PrimaryAction, ActionFill)               │
└─────────────────────────────────────────────────────────────────────┘
```

### 5. Üç durum

| Durum | Görünen |
|---|---|
| **Boş** | Ayarlar varsayılan değerlerle (çapa günü 10, %5 faizler, bildirim modu Rahat) dolu gelir; boş durum söz konusu değildir. |
| **Yükleniyor** | Form giriş alanları ve yedekleme bölümü yüksekliğinde `SkeletonBlock` iskelet blokları gösterilir; spinner kullanılmaz (`GS14`). |
| **Hata** | Ayarlar veritabanından okunamazsa `StateBlock`: `Error` ikonu + `Hata_AyarlarYuklenemedi` + `Aksiyon_TekrarDene` butonu gösterilir. |

### 6. Konsept ilişkisi

Konsept paneli doğrudan **yoktur** (`GS2`). Yerleşim dili `CardFormPage`/`LoanFormPage` form aralıklarından (`Space3`, `Space4`), `FinancialStructurePage`'in bölüm başlıkları (`TypeSection`) ve buton ritminden türetilmiştir. `EK-V13` direktifi uyarınca gereksiz kart kutuları kullanılmaz, temiz ve yalın form ritmi benimsenir (`GS33`).


---

## Bütçe özeti

Adımlar tamamlandıkça doldurulur. "Eski" kolonu eski projeden ölçüldü.

| Kart | Ekran | Eski `<Label>` | Yeni `<Label>` | Durum |
|---|---|---:|---:|---|
| EK-V0 | Kabuk | — | — | ✅ |
| EK-V1 | Profil seçimi | 10 | 4 | ✅ |
| EK-V2 | Hatırlatıcı | 15 | 2 | ✅ |
| EK-V3 | Ana sayfa | 54 | 25 | ✅ V3a |
| EK-V3 | "Bakiye gir" (sayfa 2) | — | 11 | ✅ V3b |
| EK-V4 | Kurulum | 77 | | ⬜ |
| EK-V5 | İlk düzen | 6 | 0 | ➖ Elendi (S18) |
| EK-V6 | Finansal yapı | 86 | 4 | ✅ V6a (formlar V6b–V6e) |
| EK-V6b | Kart formu | 28 | 13 | ✅ V6b1 + V6b2 |
| EK-V6c | Kredi formu | 14 | 16 | ✅ V6c1 + V6c2 + V6c3 |
| EK-V6d | Gelir formu | 14 | 7 + 3 | ✅ V6d1 + V6d2 + V6d3 |
| EK-V6e | Ödeme formu | 12 | 8 + 4 | ✅ V6e |
| EK-V6f | Kayıt türü seçici | 3 | 7 | ✅ V6f1 + V6f2 |
| EK-V7 | Kart kontrol | 73 | 19 | ✅ |
| EK-V8 | 12 dönem | 37 | 15 | ✅ V8a, V8b |
| EK-V9 | Dönem ayrıntısı | 81 | 12 | ✅ V9 |
| EK-V10 | Simülatör | 64 | 15 + 7 | ✅ V10a–V10f |
| EK-V11 | Dönem kapanışı | 50 | 19 | ✅ V11 |
| EK-V12 | Geçmiş + ayrıntı | 29 | 6 + 12 | ✅ V12 |
| EK-V13 | Ayarlar + düzen | 52 | 14 | ✅ V13 |
| | **Toplam** | **619** | | |
