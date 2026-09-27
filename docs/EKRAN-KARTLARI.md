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

## EK-V3 — Ana sayfa

> Sayfa dosyası: `DashboardPage.xaml`
> Adım V3 ile tamamlandı (Alternatif B / GS20, Kapı C kararlarıyla).

**Eski hâl:** `MainPage.xaml` 425 satır, **54 `<Label>`**, 5 kart, 11 buton, 1 geliştirici dökümü.

### 1. Sorular

| Kod | Soru | Eskide nasıl cevaplanıyordu |
|---|---|---|
| S1 | "Bu dönem param yetecek mi / dönem sonunda elimde ne kalacak?" | PLAN kartı + GİDİŞAT'ın "Dönem sonu" matrisi (8 etiket) |
| S2 | "Bugün elimde ne var ve bunu sisteme nasıl işlerim?" | Mevcut tutar kartı (4 etiket, 2 açıklama cümlesi, entry, buton) |
| S3 | "Bu dönem daha ne ödeyeceğim?" | KALAN kartı (3 + şablon 4 etiket + toplam) |
| S4 | "Plandan saptım mı, durumum ne?" | GİDİŞAT kartı — **dört metin matrisi, ~28 etiket** |
| S5 | "Diğer bölümlere nereden giderim?" | 6 ayrı buton |

### 2. Kesme kararları (GS20 — Alternatif B)

| Bilgi / Öğe | Karar | Gerekçe |
|---|---|---|
| Bütçe doluluk halkası | **Grafik** (`RingGauge`) | S1, S4: Kalan yaşam havuzu / bütçe oranını tek bakışta hissettiren görsel merkez (GK4: 1/1 grafik) |
| Dönem sonu projeksiyonu | **Hero rakam** | S1 ekranın asıl sorusu; görsel merkez kartında halkanın yanında `HeroFigure`, `TextPrimary` |
| Kalan bütçe / harcanan | **Satır** (hero'nun altında iki değer) | S1, S4: halkanın gösterdiği oranın tutar karşılığı (Kapı C: "Planlanan" notunun yerine) |
| Mevcut tutar girişi | **Hero yüzey** (`HeroInputCard`) | S2 hem soru hem aksiyon; çelik mavisi hero kartı |
| Dönem ilerlemesi | **Satır** (`PeriodRail`) | S1 takvim bağlamı; başlığın altında tek şerit |
| Kalan ödemeler | **Satır** (ikonlu satır, ≤ 3 satır + "+N ödeme daha" `NavRow`) | S3 tarama sorusu; ağır kart kutusu yerine hafif ikonlu satırlar; taşma dönem ayrıntısına gider |
| Hatırlatıcı bildirimi | **Kart** (`ReminderCard`) | S3/S1 acil ödeme; çocuk kart, aktif kayıt yoksa görünmez |
| Öncelikli uyarı / bildirim | **Çıkar** | Kapı C: GK4 `InfoBanner`'ı hero yüzey sayar, `HeroInputCard` ile sınırı aşar; kapanış sinyali "Dönemi Kapat" butonudur |
| Gezinme (4 hedef) | **Satır** (`NavRow × 4`) | S5 rotalar; 12 Dönem, Geçmiş, Finansal Yapı, Simülatör |
| Dönemi kapat | **Aksiyon** (`ActionFill`) | S1 dönemi tamamlama aksiyonu |
| Yaşam gideri 3 kolon | **Derine** → `EK-V9` | Kategori kırılımı dönem ayrıntısının işi |
| Kredi kartı plan/mevcut tablosu | **Derine** → `EK-V7` | Kart kontrol ekranı zaten bunu gösteriyor |
| KMH faizi plan/mevcut | **Derine** → `EK-V9` | İkinci seviye ayrıntı |
| "Hesaplama detayı (geliştirme)" | **Çıkar** | GS5 kuralı; hiçbir kullanıcı sorusuna bağlanmıyor |
| `StatusMessage` hata etiketi | **Çıkar** | Hata durumu `StateBlock` veya diyalogla yönetilir |
| İkinci gözlem açıklaması | **Çıkar** | GK5 cümle bütçesi; tek net cümle |

### 3. Bütçe (Alternatif B — uygulanan)

```
Hero rakam    1 / 1     dönem sonu projeksiyonu (ProjectedEndingBalance)
Hero yüzey    1 / 1     HeroInputCard
Kart          3 / 4     görsel merkez (RingGauge + hero), HeroInputCard, ReminderCard (çocuk)
Grafik        1 / 1     RingGauge (kalan bütçe oranı halkası)
NavRow        5 / 5     12 Dönem, Geçmiş, Finansal Yapı, Simülatör + "+N ödeme daha" taşması
Label        11 / 28    sayfa XAML'inde sayılan (bileşen içleri hariç)
Cumle_        2 / 3     Cumle_GozlemIpucu, Cumle_AcikDonemYokRehber (boş durum)
```

### 4. Blok şeması (Alternatif B)

```
┌─ PageHeader ─────────────────────────────────────────┐
│ Etiket_Slogan          TypeEyebrow / TextSecondary   │
│ Baslik_AnaSayfa        TypeTitle   / TextPrimary     │  [Settings]
└──────────────────────────────────────────────────────┘
┌─ PeriodRail ─────────────────────────────────────────┐  ← S1
│ Dönem adı              TypeSection / TextPrimary     │
│ 4/30 gün · 15 Ekim     TypeCaption / TextSecondary   │
│ ▓▓▓▓░░░░░░░░░          Indicator   / RadiusPill      │
└──────────────────────────────────────────────────────┘
┌─ Görsel Merkez: Bütçe & Dönem Sonu (SurfaceCard) ────┐  ← S1, S4
│ ┌─ RingGauge ─┐  Etiket_DonemSonuTahmini  (Eyebrow)  │
│ │   ╭─────╮   │  41.723 ₺                 (HeroFigure)│
│ │   │ %68 │   │  KALAN BÜTÇE     HARCANAN  (Caption)  │
│ │   ╰─────╯   │  6.800 ₺         3.200 ₺   (TypeBody) │
│ └ 1* sütun, kare ┘  3* sütun                          │
└──────────────────────────────────────────────────────┘
┌─ HeroInputCard         SurfaceHero / RadiusHero ─────┐  ← S2
│ Etiket_MevcutTutar     TypeEyebrow / TextOnHero      │
│ [ Entry  SurfaceSunken ]  [ Kaydet   ActionFill ]    │
│ Cumle_GozlemNotu       TypeCaption / TextOnHeroSec.  │
└──────────────────────────────────────────────────────┘
┌─ ReminderCard (HasActiveReminder == true) ───────────┐  ← S3
│ [Notifications]  {Title} (TypeSection)               │
│                  {Message} (TypeBody)                │
│ [ Ertele  SecondaryButton ]  [ Ödedim  ActionFill ]  │
└──────────────────────────────────────────────────────┘
┌─ Kalan Yükümlülükler (İkonlu Satırlar Deseni) ───────┐  ← S3
│ Etiket_Kalan           TypeEyebrow / TextSecondary   │
│ [Payments]   21 Eyl  Giyim                    4.500 ₺│
│ [Payments]   24 Eyl  Market                   1.200 ₺│
│ [CreditCard] 27 Eyl  Denizbank                  750 ₺│
│ +4 ödeme daha (Toplam: 11.249 ₺)  ChevronRight/NavRow│
└──────────────────────────────────────────────────────┘
[ Aksiyon_DonemiKapat    ActionFill  / RadiusCard ]       ← S1
┌─ NavRow × 4 (SurfaceCard / RadiusCard) ──────────────┐  ← S5
│ [Insights]   Önümüzdeki 12 dönem                ›    │
│ [Schedule]   Geçmiş dönemler                    ›    │
│ [Payments]   Finansal yapıyı yönet              ›    │
│ [Insights]   Simülatörü aç                      ›    │
└──────────────────────────────────────────────────────┘
```

Dönem planı henüz yokken veya kurulum tamamlanmamışken (Boş Durum):

```
┌─ PageHeader ─────────────────────────────────────────┐
│ Baslik_AnaSayfa        TypeTitle   / TextPrimary     │  [Settings]
└──────────────────────────────────────────────────────┘
┌─ StateBlock (Empty) ─────────────────────────────────┐  ← S1
│ [AccountBalance]                                     │
│ Cumle_AcikDonemYokRehber                             │
│ [ Aksiyon_TemizBasla ]  → Onboarding (V4)            │
└──────────────────────────────────────────────────────┘
```

### 5. Üç durum

| Durum | Görünen |
|---|---|
| Boş | `StateBlock`: `AccountBalance` ikonu + `Cumle_AcikDonemYokRehber` + `Aksiyon_TemizBasla` (kurulum sihirbazı V4'te bağlanır). |
| Yükleniyor | Üç `SkeletonBlock` (spinner yok). `LoadAsync` her yüklemede önce `ScreenState.Loading`'e geçer. |
| Hata | `StateBlock`: `Close` ikonu + `Hata_DashboardYuklenemedi` + `Aksiyon_TekrarDene` (yüklemeyi yeniden çalıştırır). |

Görsel kontrol (Kapı C): boş hâl iki temada onaylandı. Dolu hâl V3'te açık dönem
oluşturulamadığı için görülemedi; kontrolü V4'ün Kapı C'sine taşındı.

### 6. Konsept ilişkisi

Konsept panel 1 (Ana Sayfa) temel ilham kaynağıdır. `GS20` kararı ile dikey kart blokları sadeleştirilmiş; T4'te üretilen `RingGauge` primitifi bütçe durumunu özetleyen tek görsel merkez olarak konumlandırılmıştır.

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
| "Ödediklerin" geçmiş listesi | **Derine** → `EK-V9` | Dönem gerçekleşmesi ve mutabakat ekranında yer alır |
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
| "+ Ekle" seçici ve satır içi formlar | **Derine** → `V6b`–`V6e` | S4; her tür kendi sayfasında |
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
Ekle        → ChooseAsync("Ne eklemek istiyorsun?", "Vazgeç", null, "Kredi kartı", "Kredi")
                → Routes.CardForm | Routes.LoanForm
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
> "Ekle → Kredi" ve kredi satırında "Düzenle". `V6c2` ve `V6c3` açık.
> Not: kart anahtarı ayrıştırıcısı (`EK-V\d+`) harf ekini tanımaz; bu kart GK9'da `EK-V6`'nın
> gövdesi olarak okunur (`EK-V6b` ile aynı).

**Eski hâl:** `CommitmentsPage.xaml` kredi formu bölümü 35 satır, **11 `<Label>`** (ortak başlık,
açıklama ve ad alanıyla 14), 9 giriş; Krediler bölümünde satır başına faiz / kapatma cümlesi ve
erken ödeme satırları (4 etiket + not cümlesi + Sil); erken ödeme girişi `ScenarioConditionFormView`
içinde ~7 etiket, 5 giriş. ViewModel: `CommitmentsViewModel.Loans` (120 satır), `.Operations`
(kredi ve erken ödeme satırları, silme), `ScenarioConditionForm` (403 satır / 2 dosya, ortak).

### 1. Sorular

| Kod | Soru | Eskide nasıl cevaplanıyordu |
|---|---|---|
| S1 | "Kredimi nasıl eklerim; taksit, kalan taksit ve sonraki ödemeyi nasıl düzeltirim?" | 14 etiket, 9 giriş; ödeme günü ve tarih ayrı iki alan |
| S2 | "Bu kredinin faizi ne; bugün kapatırsam ne öderim, ne kadar faizden kurtulurum?" | Listede satır başına tek etiketlik iki satırlık cümle; formda 3 yardım cümlesi |
| S3 | "Planlı erken ödemelerim neler, o gün ne kadar ödeyeceğim?" | Krediler listesinde ayrı satırlar: 4 etiket + not cümlesi + Sil |
| S4 | "Yeni bir erken ödeme ya da kapamayı nasıl planlarım?" | "+ Ekle" → ortak senaryo formu (~7 etiket, 5 giriş) ya da simülatör |

### 2. Kesme kararları

| Bilgi / Öğe | Karar | Gerekçe |
|---|---|---|
| Ad, banka, aylık taksit, kalan taksit, sonraki taksit tarihi, kredi türü | **Satır** (form kartı: `Eyebrow` + giriş, `Grid *,*`) | S1 · `V6c1` |
| Kaydet / Vazgeç | **Aksiyon** (`ActionFill` / `SecondaryButton`) | S1 · kredi ve erken ödemeleri birlikte yazar; değişiklik varsa çıkış onayı |
| Finansal Yapı "Ekle"de "Kredi", kredi satırında "Düzenle" | **Aksiyon / Satır** (mevcut diyaloglara seçenek) | `EK-V6` S3, S4 · `V6c1` |
| Kalan anapara, bankanın kapatma tutarı | **Satır** (faiz kartında) | S2 · `V6c2` · ikisi de isteğe bağlı |
| Aylık faiz, bugün ödenecek, erken ödeme ücreti, kurtulunacak faiz | **Kart** (faiz kartı, `MetricRow`'lar, canlı) | S2 · `V6c2` |
| Faiz hesaplanamadığında ne yapmalı | **Satır** (faiz kartında tek `Cumle_`, iki varyant) | S2 · `V6c2` |
| Planlı erken ödemeler (şekil · tarih · o günkü tutar) | **Kart** (`ListCard`, ≤ 4 satır + "+N daha" yerinde, `GS21`) | S3 · `V6c3` |
| Erken ödeme girişi (tarih, şekil, ara ödemede tutar) | **Kart** ("Erken ödeme planla" ile açılan giriş bloğu) | S4 · `V6c3` |
| Erken ödemeyi silme | **Satır** (dokun → diyalog → Sil) | S3 · `V6c3` |
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
Label        16 / 28    form 6 (V6c1) + faiz kartı 4 (V6c2) + satır şablonu 3 + giriş 3 (V6c3); DataTemplate içi bir kez
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
┌─ Faiz kartı (Border SurfaceCard / RadiusCard / CardPadding) ───────┐  V6c2
│ Etiket_KalanAnapara          Eyebrow                               │  ← S2
│ [ Entry Numeric  "İsteğe bağlı" ]                  tam genişlik    │
│ Etiket_KapatmaTutari         Eyebrow  ("Bankanın kapatma tutarı")  │  ← S2
│ [ Entry Numeric  "İsteğe bağlı" ]                  tam genişlik    │
│ ── Hâl A: faiz çözüldü ──                                          │
│ MetricRow  Etiket_AylikFaiz     "Aylık faiz (vergi dahil)"  %2,79  │  ← S2  Yuzde
│ MetricRow  Etiket_BugunKapatirsan                        150.230 ₺ │  ← S2  Para
│ MetricRow  Etiket_ErkenOdemeUcreti  yalnız ücret > 0       1.489 ₺ │  ← S2  (konut, sabit faiz)
│ MetricRow  Etiket_KurtulacaginFaiz  Semantic=Positive     33.770 ₺ │  ← S2
│ ── Hâl B: anapara da kapatma tutarı da yok, ya da tanım eksik ──   │
│ Cumle_FaizIcinTutarGir       Caption                               │
│ ── Hâl C: girilen tutar taksitlerle uyuşmuyor ──                   │
│ Cumle_TutarUyusmuyor         Caption                               │
└────────────────────────────────────────────────────────────────────┘
┌─ ListCard  Etiket_ErkenOdemeler ───────────────────────────────────┐  V6c3 · ← S3 · yalnız kayıt varsa
│ Tamamen kapatma          TypeBody / TextPrimary     152.410 ₺      │
│ 15 Mart                  Caption (Tarih)            Figure         │
│ Ara ödeme · vade kısalır                             50.000 ₺      │  tutar hesaplanamıyorsa "—"
│ 15 Ocak                                                            │
│   … en fazla 4 satır, tarihe göre; fazlası: +N daha (GS21)          │
│   dokun → ChooseAsync(şekil, "Vazgeç", "Sil")                       │
└────────────────────────────────────────────────────────────────────┘
[ Aksiyon_ErkenOdemePlanla             SecondaryButton ]                 V6c3 · ← S4 · giriş kapalıyken
┌─ Giriş bloğu (Border SurfaceCard / RadiusCard / CardPadding) ──────┐  V6c3 · ← S4 · açıkken
│ Etiket_OdemeTarihi           Etiket_OdemeSekli                     │
│ [ DatePicker ≥ bugün ]       [ Picker  Tamamen kapat ▾ ]           │
│ Etiket_AnaparadanDusecek     Eyebrow   yalnız ara ödemede          │
│ [ Entry Numeric ]                                                  │
│ [ Aksiyon_Ekle  ActionFill ]  [ Aksiyon_Vazgec  SecondaryButton ]  │
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
  "Değişiklik" formun yüklendiği andaki değerlerle karşılaştırılır (`V6c3`'te erken ödeme listesi dahil).
- Düzenleme yüklenen kredinin üstüne `with` ile kurulur (`S64`-3). Ödeme günü sonraki taksit
  tarihinin günüdür; kayıtlı gün o ayda aynı tarihe kenetleniyorsa korunur (`S64`-2).
- Tutar girişi V7'nin Türkçe okuyucusuyla (`I60`). Ad boş, taksit ≤ 0, kalan taksit < 1 → diyalog;
  kalan kurallar `SaveLoanAsync` / `PrepareForSave`'in Türkçe mesajlarıyla diyalogda.
- Kredi türü `Picker`'ı ham `LoanKind` değerleri taşır; görünen ad App'teki `KrediTuruConverter`'da
  (`ItemDisplayBinding`). Alan yarım genişlik olduğu için adlar kısa: "İhtiyaç / taşıt", "Konut, sabit",
  "Konut, değişken" (başlık zaten "Kredi türü"; sabit / değişken faizin türüdür). `DatePicker` ve `Picker` renkleri `EK-V6b`'deki `DatePicker` gibi satır içi
  `DynamicResource` (`SurfaceSunken`, `TextPrimary`) ile bağlanır.
- Faiz kartı (`V6c2`): formun her değişikliğinde sessizce kurulan krediden `LoanPayoffService`
  ile hesaplanır; kapatma tutarı değiştiyse bugünün tarihiyle değerlendirilir. Aylık faiz yüzde
  olarak `YuzdeConverter`'la yazılır. Hâl B / C ayrımı: tutar girilmiş ama faiz çözülemiyorsa C.
- Erken ödeme satırı (`V6c3`) ham veri taşır (`LoanPrepaymentRow`: kimlik, şekil, tarih, tutar);
  şekil metni `ErkenOdemeTuruConverter`'da. Satır şablonu `ContentPage.Resources`'ta (`EK-V6b` notu:
  iç içe `ListCard.ItemTemplate` bütçe analizcisinde ikinci kart sayılır). "Ekle" girişi formun o
  anki kredisine göre `LoanPrepaymentValidator` ile doğrular; Kaydet hepsini Application'da yeniden
  doğrular ve tek kayıtta yazar.
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
| EK-V0 | Kabuk | — | — | ✅ |
| EK-V1 | Profil seçimi | 10 | 4 | ✅ |
| EK-V2 | Hatırlatıcı | 15 | 2 | ✅ |
| EK-V3 | Ana sayfa | 54 | 11 | ✅ |
| EK-V4 | Kurulum | 77 | | ⬜ |
| EK-V5 | İlk düzen | 6 | | ⬜ |
| EK-V6 | Finansal yapı | 86 | 4 | ✅ V6a (formlar V6b–V6e) |
| EK-V6b | Kart formu | 28 | 13 | ✅ V6b1 + V6b2 |
| EK-V6c | Kredi formu | 14 | 6 | ✅ V6c1 (V6c2, V6c3 açık; sayfanın tamamı 16) |
| EK-V7 | Kart kontrol | 73 | 19 | ✅ |
| EK-V8 | 12 dönem | 37 | | ⬜ |
| EK-V9 | Dönem ayrıntısı | 81 | | ⬜ |
| EK-V10 | Simülatör | 64 | | ⬜ |
| EK-V11 | Dönem kapanışı | 50 | | ⬜ |
| EK-V12 | Geçmiş + ayrıntı | 29 | | ⬜ |
| EK-V13 | Ayarlar + düzen | 52 | | ⬜ |
| | **Toplam** | **619** | | |
