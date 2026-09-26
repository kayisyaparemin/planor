# Ekran Kartları

Her ekranın bir kartı vardır. Kart, o ekranın Aşama 4'te verilen **çıkarma kararlarının** ve
Aşama 5'te onaylanan **düzeninin** tek kaydıdır.

Kural **GK9** iki yönlü çalışır: kartsız sayfa da, sayfası olmayan kart da kırmızıdır.

Kart anahtarı adım kodundan gelir: `EK-V3` = `V3` adımının ekranı.

## Kart biçimi

Her kart şu altı bölümü taşır — boş bırakılan bölüm, verilmemiş bir karardır:

1. **Sorular** — ekranın cevapladığı sorular, kullanıcının diliyle, önem sırasıyla (≤ 5)
2. **Kesme kararları** — her bilgi parçası: hero / kart / şema / grafik / satır / derine / çıkar
3. **Bütçe** — sayım tablosu (GK4, GK5)
4. **Blok şeması** — bileşen, token, cevapladığı soru kodu
5. **Üç durum** — boş, yükleniyor, hata
6. **Konsept ilişkisi** — hangi konsept panelinden geliyor, ya da neyden türetildi

---

## EK-V3 — Ana sayfa  *(işlenmiş örnek)*

> Bu kart, protokolün nasıl doldurulduğunu gösteren referanstır. `V3` adımı çalıştığında
> Aşama 4–5'te teyit edilir; şu hâli **teklif**, onaylanmış karar değil.

**Eski hâl:** `MainPage.xaml` 424 satır, **54 `<Label>`**, 5 kart, 6 buton,
1 geliştirici dökümü.

### 1. Sorular

| Kod | Soru | Eskide nasıl cevaplanıyordu |
|---|---|---|
| S1 | "Bu dönem param yetecek mi?" | PLAN kartı + GİDİŞAT'ın "Dönem sonu" matrisi (8 etiket) |
| S2 | "Bugün elimde ne var?" | Mevcut tutar kartı (4 etiket, 2 açıklama cümlesi) |
| S3 | "Bu dönem daha ne ödeyeceğim?" | KALAN kartı (3 + şablon 4 etiket) |
| S4 | "Plandan saptım mı?" | GİDİŞAT kartı — **dört metin matrisi, ~28 etiket** |
| S5 | "Nereye gitmem gerekiyor?" | 6 ayrı buton |

### 2. Kesme kararları

| Bilgi | Karar | Gerekçe |
|---|---|---|
| Dönem sonu projeksiyonu | **Hero rakam** | S1 ekranın asıl sorusu; tek büyük sayı, `TextPrimary` |
| Mevcut tutar girişi | **Kart** (`HeroInputCard`) | S2 hem soru hem aksiyon; konseptin derin mavi kartı |
| Plan / gerçek / fark | **Şema** (`ComparisonStrip`) | Dört matrisin yerine üç sayı; **ilişki** asıl cevap |
| Kalan ödemeler | **Kart** (`ListCard`, ≤ 4 satır + "+N daha") | S3 tarama sorusu, okuma sorusu değil |
| Dönem ilerlemesi | **Satır** (`PeriodRail`) | Kart değil; başlığın altında tek şerit |
| Gezinme (4 hedef) | **Satır** (`NavRow`) | 6 butondan 4'e; "dönemi kapat" hero kartın altında |
| Yaşam gideri 3 kolon | **Derine** → `EK-V9` | Kategori kırılımı dönem ayrıntısının işi |
| Kredi kartı plan/mevcut tablosu | **Derine** → `EK-V7` | Kart kontrol ekranı zaten bunu gösteriyor |
| KMH faizi plan/mevcut | **Derine** → `EK-V9` | İkinci seviye ayrıntı |
| Uyarı listesi (çok kayıtlı) | **Satır** — en fazla **bir** `InfoBanner` | İki uyarı birden gösterilirse ikisi de okunmuyor → `GS4` |
| "Hesaplama detayı (geliştirme)" | **Çıkar** | Hiçbir soruya bağlanmıyor; hata ayıklama çıktısı |
| `StatusMessage` alt satırı | **Çıkar** | Hata durumu `StateBlock`'a taşındı |
| İkinci gözlem açıklaması | **Çıkar** | İki cümle tek cümleye indi (GK5) |

### 3. Bütçe

```
Hero rakam    1 / 1     dönem sonu projeksiyonu
Hero yüzey    1 / 1     HeroInputCard
Kart          3 / 4     HeroInputCard, SummaryCard(plan), ListCard(kalan)
Grafik        0 / 1     ana sayfada grafik yok — 12 dönem seyri EK-V8'de
NavRow        4 / 5
Label        26 / 28    (DataTemplate içindekiler bir kez sayıldı)
Cumle_        1 / 3     Cumle_GozlemNotu
```

### 4. Blok şeması

```
┌─ PageHeader ─────────────────────────────────────┐
│ Etiket_Slogan          TypeEyebrow/TextSecondary │
│ Baslik_AnaSayfa        TypeTitle      [Settings] │
└──────────────────────────────────────────────────┘
┌─ PeriodRail ─────────────────────────────────────┐  ← S1
│ Dönem adı              TypeSection               │
│ 4/30 gün · 15 Ekim     TypeCaption               │
│ ▓▓▓▓░░░░░░░░░          Indicator / RadiusPill    │
└──────────────────────────────────────────────────┘
┌─ HeroInputCard         SurfaceHero / RadiusHero ─┐  ← S2
│ Etiket_MevcutTutar     TypeEyebrow / TextOnHero  │
│ [ Entry  SurfaceSunken ]  [ Kaydet  ActionFill ] │
│ Cumle_GozlemNotu       TypeCaption               │
└──────────────────────────────────────────────────┘
┌─ SummaryCard           SurfaceCard / RadiusCard ─┐  ← S1
│ Etiket_Plan            TypeEyebrow               │
│ Dönem sonu                    41.723  TypeHero   │
│                               TextPrimary        │
└──────────────────────────────────────────────────┘
┌─ ComparisonStrip ────────────────────────────────┐  ← S4
│ ┌ PLAN ──────┐ ┌ GERÇEK ────┐ ┌ FARK ──────┐     │
│ │ PlanSurface│ │ActualSurf. │ │Positive/Neg│     │
│ │ 40.554,84  │ │ 40.554,84  │ │    0,00    │     │
│ └────────────┘ └────────────┘ └────────────┘     │
└──────────────────────────────────────────────────┘
┌─ ListCard ───────────────────────────────────────┐  ← S3
│ Etiket_Kalan           TypeEyebrow               │
│ 21 Eyl  giyim                    4.500  ⎫        │
│ 21 Eyl  giyim                    4.500  ⎬ şablon │
│ 27 Eyl  Denizbank Deniz            749  ⎭        │
│ +4 daha                ChevronRight / NavRow     │
│ Toplam                          11.249  TypeFig. │
└──────────────────────────────────────────────────┘
[ Aksiyon_DonemiKapat    ActionFill / RadiusCard ]     ← S1
┌─ NavRow × 4 ─────────────────────────────────────┐  ← S5
│ Insights   Önümüzdeki 12 dönem              ›    │
│ Schedule   Geçmiş dönemler                  ›    │
│ Payments   Gelir ve ödemelerini yönet       ›    │
│ Insights   Simülatörü aç                    ›    │
└──────────────────────────────────────────────────┘
```

### 5. Üç durum

| Durum | Görünen |
|---|---|
| Boş | `StateBlock`: `Settings` ikonu + `Bos_KurulumYapilmadi` + "Kurulumu tamamla" |
| Yükleniyor | İskelet: `PeriodRail` + üç kart şekli, `SurfaceSunken` |
| Hata | `StateBlock`: `Close` ikonu + `Hata_PlanOkunamadi` + "Tekrar dene" |

### 6. Konsept ilişkisi

Konsept panel 1 (Ana Sayfa) birebir kaynak. `ComparisonStrip`, konseptin "Dönem Ayrıntısı"
panelinden ödünç alındı — orada da kullanılıyor, yani GK'nın "iki ekran" kuralına uyuyor.

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

---

## Doldurulacak kartlar

Aşağıdaki kartlar ilgili V adımının Aşama 4–5'inde doldurulur. **Boş bir kart, o adımın
henüz başlamadığını gösterir** — kartı önceden doldurmak, onay kapısını atlamaktır.

### EK-V1 — Profil seçimi
Konsept karşılığı **yok.** Türetme adayı: `EK-V3`'ün `ListCard` + `NavRow` deseni.

### EK-V2 — Hatırlatıcı kartı
Konsept karşılığı **var** (bildirim paneli: "Ödedim / Ertele"). Sayfası olmayan çocuk
ViewModel; `ReminderCard` bileşeni olarak `EK-V3` içinde görünür.

### EK-V4 — Kurulum sihirbazı
Konsept karşılığı **var** (Kurulum 8/8 özeti). Eskide 958 satır / 3 `partial` — adım
ViewModel'lerine bölünecek. Her adım kendi ekran kartını almaz; **tek kart, sekiz blok
şeması.** Sapma `S6`, `S9` geçerli.

### EK-V7 — Kart kontrol
Konsept karşılığı **yok.** `V6` ve `V10`'dan önce gelir. `EK-V3`'ten devralınan yükler:
kredi kartı plan/mevcut tablosu.

### EK-V6 — Finansal yapı
Konsept karşılığı **yok.** Eskide 1.344 satır / 6 `partial`, **86 `<Label>`** — en kalabalık
ekran. Aşama 1'de **bölünme önerisi zorunlu**; tek adımda 28 etikete inmesi beklenmiyor.

### EK-V5 — İlk düzen seçimi
Konsept karşılığı **yok.** `V6`'ya dayanır.

### EK-V8 — 12 dönem
Konsept karşılığı **var** (Simülatör panelindeki dönem kartları deseni). Grafik adayı:
`AreaTrend` — "seviye 12 dönemde eşiğin altına iniyor mu?"

### EK-V9 — Dönem ayrıntısı
Konsept karşılığı **var** (Dönem Ayrıntısı paneli: `InfoBanner` + `ComparisonStrip` +
kategori satırları). `EK-V3`'ten devralınan yükler: yaşam gideri kırılımı, KMH faizi.
Eskide 605 satır / 81 `<Label>`.

### EK-V10 — Simülatör
Konsept karşılığı **var** (Simülatör paneli). Eskide 1.034 satır / 4 `partial`, 64 `<Label>`.
Dönem başına `MetricRow` + "Detay Gör" deseni konseptte net.

### EK-V11 — Dönem kapanışı sihirbazı
Konsept karşılığı **yok.** Türetme adayı: `EK-V4`'ün sihirbaz deseni.

### EK-V12 — Geçmiş + geçmiş ayrıntısı
Konsept karşılığı **var** (Geçmiş paneli, grafikli). Tek grafikli ekranların referansı;
`ChartCard` + lejant çipi deseni buradan gelir.

### EK-V13 — Ayarlar + düzen değişikliği
Konsept karşılığı **yok.** Türetme adayı: `NavRow` listesi; kart kullanılmaz. Uygulama içi tema
seçici istenecekse kararı burada verilir (`GS7`: varsayılan, sistemi izlemek).

---

## Bütçe özeti

Adımlar tamamlandıkça doldurulur. "Eski" kolonu eski projeden ölçüldü.

| Kart | Ekran | Eski `<Label>` | Yeni `<Label>` | Durum |
|---|---|---:|---:|---|
| EK-V0 | Kabuk | — | — | ⬜ |
| EK-V1 | Profil seçimi | 10 | | ⬜ |
| EK-V2 | Hatırlatıcı | — | | ⬜ |
| EK-V3 | Ana sayfa | 54 | | ⬜ |
| EK-V4 | Kurulum | 77 | | ⬜ |
| EK-V5 | İlk düzen | 6 | | ⬜ |
| EK-V6 | Finansal yapı | 86 | | ⬜ |
| EK-V7 | Kart kontrol | 73 | | ⬜ |
| EK-V8 | 12 dönem | 37 | | ⬜ |
| EK-V9 | Dönem ayrıntısı | 81 | | ⬜ |
| EK-V10 | Simülatör | 64 | | ⬜ |
| EK-V11 | Dönem kapanışı | 50 | | ⬜ |
| EK-V12 | Geçmiş + ayrıntı | 29 | | ⬜ |
| EK-V13 | Ayarlar + düzen | 52 | | ⬜ |
| | **Toplam** | **619** | | |
