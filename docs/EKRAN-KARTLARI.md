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
| EK-V0 | Kabuk | — | — | ✅ |
| EK-V1 | Profil seçimi | 10 | 4 | ✅ |
| EK-V2 | Hatırlatıcı | 15 | 2 | ✅ |
| EK-V3 | Ana sayfa | 54 | 11 | ✅ |
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
