# İş Akışı — Bir Ekran Adımı

Faz V adımları (`V0`–`V13`) bu protokolle yürür. **On aşama vardır ve hiçbiri atlanamaz.**

Bu protokol `tasima-adimi.md`'nin yerine geçmez, **üzerine biner**: Aşama 1, 2, 3, 6, 7, 9,
10 oradaki aşamalarla birebir aynı mantıktadır. Eklenen üç aşama şunlar:

| Yeni aşama | Neden var |
|---|---|
| **Aşama 4 — Görsel Bütçe** | Eski ekranlarda çıkarma kararını zorlayan hiçbir mekanizma yoktu; 86 etiketli ekran böyle oluştu |
| **Aşama 5 — Düzen Sözleşmesi** | Yerleşim, XAML yazıldıktan sonra değişirse pahalı; blok şeması ucuz |
| **Aşama 8 — XAML** | Görsel katman ayrı bir aşamadır; ViewModel yeşilken başlar, yeşili bozamaz |

Amaç kodu taşımak değil; ekranın **kullanıcının hangi sorusunu cevapladığının bilinerek**
yazılmasını sağlamak. Eski Mizan'ın ekran problemi eksik bilgi değildi, **kararı verilmemiş
bilgiydi.** Yeni ekranlar Planör'ün ekranlarıdır: yerleşimi konseptten, rengi ve sesi
markadan alırlar (`docs/TASARIM-SAPMALARI.md` § İki referans, iki otorite).

Üç onay kapısı var: **A** (Aşama 2+3+4), **B** (Aşama 5) ve **C** (Aşama 9 — görsel onay).
A ve B'den onay almadan kod yazılmaz; C'den onay almadan kayıt ve commit yapılmaz.

---

## Aşama 1 — Keşif  *(kod yazılmaz)*

Eski repodaki (`C:\Users\kayis\Documents\mizan`) sayfayı ve ViewModel'ini oku. Raporla:

- Hangi dosyalar, kaç satır, kaç `<Label>`, kaç kart, kaç buton
- ViewModel kaç bağımlılık alıyor, kaç `partial` dosyaya bölünmüş
- Bu ekran hangi Application portlarına bağlı — **hepsi taşınmış mı?**
- `docs/TASIMA-PLANI.md`'de bu adımın gerektirdiği önceki adımlar tamam mı
- **Konseptte bu ekranın bir paneli var mı?** Yoksa hangi ekrana benzeyeceği Aşama 4'te
  kararlaştırılacak. Planör tanıtım görselindeki telefon maketleri konsept paneli
  **sayılmaz**

Taşınmamış bir bağımlılığa rastladıysan **dur ve söyle.** Sırayı kendi başına değiştirme.

## Aşama 2 — Soru Listesi  *(ONAY KAPISI A'nın parçası)*

Taşıma protokolündeki "Anlatım" aşamasının ekran hâli. Kullanıcıya Türkçe anlat:

1. **Bu ekran kullanıcının hangi sorularını cevaplıyor?** Soruları kullanıcının diliyle yaz
   ve **önem sırasına koy.** En fazla beş soru. Örnek:
   - S1: "Bu dönem param yetecek mi?"
   - S2: "Bugün elimde ne var?"
   - S3: "Bu dönem daha ne ödeyeceğim?"
2. **Eski ekran bu soruları nasıl cevaplıyordu?** Soru başına kaç etiket harcanıyordu.
3. **Hangi bilgi hiçbir soruya bağlanamıyor?** Bunlar Aşama 4'ün adayları.
4. **Yeni hâlinde hangi dosyalar oluşacak?** Yol yol liste.

Bir bilgi hiçbir sorunun cevabı değilse ekranda olmasının gerekçesi yoktur. Eski
`MainPage`'deki "Hesaplama detayı (geliştirme)" bölümü bunun saf örneği.

Anlatımı bitirince **durma, Aşama 3 ve 4'ü de aynı mesajda sun.**

## Aşama 3 — Sapma Kararı *(davranış)*  *(ONAY KAPISI A'nın parçası)*

`tasima-adimi.md` Aşama 3 ile **birebir aynı.** Ekranın davranışı hakkında:

1. `docs/SAPMALAR.md`'de bu adımı etkileyen `S` kaydı var mı? Varsa oku ve uygula.
2. Eski kod bu ekranın davranışını yanlış mı modellemiş? Arayüzün vaat ettiği şeyi motor
   gerçekten yapıyor mu?
3. `docs/SOZLUK.md`'deki yasaklı terime dokunuyor mu?
4. Kullanıcının bu ekran için ayrıca istediği bir davranış değişikliği var mı?

Üç şıktan birini öner: **olduğu gibi taşı** / **şu sapmayla taşı** / **hiç taşıma.**

Karar verildiyse `docs/SAPMALAR.md`'ye **şimdi** yaz.

## Aşama 4 — Görsel Bütçe ve Kesme Kararı  *(ONAY KAPISI A)*

Bu aşama protokolün en önemli parçası. Aşama 2'deki her bilgi parçasını **bir yere koy** ya
da **çıkar.** Kararsız bırakılan bilgi ekranda birikir.

Her bilgi parçası için altı şıktan **birini** öner:

| Şık | Ne zaman |
|---|---|
| **Hero** | Ekranın tek en önemli sayısı. Sayfada bir tane. |
| **Kart** | Bir soruyu tek başına cevaplıyor. Sayfada en fazla dört. |
| **Şema** | İki-üç sayının **ilişkisi** asıl cevap (`ComparisonStrip`, `RingGauge`). |
| **Grafik** | Üçten fazla noktanın **yönü** asıl cevap. Sayfada bir tane. |
| **Satır** | Cevabın parçası ama başlı başına cevap değil (`MetricRow`, liste satırı). |
| **Derine** | İkinci seviye ayrıntı — `NavRow` + detay sayfası. |
| **Çıkar** | Hiçbir soruya bağlanmıyor, ya da geliştirici içindi. |

Sonra bütçe sayımını **tablo hâlinde** göster:

```
Hero      1 / 1
Kart      3 / 4
Grafik    1 / 1
NavRow    4 / 5
Label    24 / 28   (DataTemplate içindekiler bir kez sayıldı)
Cumle_    2 / 3
```

Bütçe aşılıyorsa **çıkar, küçültme.** "Puntoyu düşürelim sığar" cevabı yasaktır — bu, 86
etiketli ekranın nasıl oluştuğunun tam açıklamasıdır.

Konseptte paneli olmayan ekranlar için ayrıca şunu söyle: **hangi konsept ekranına
benziyor** ve hangi bileşenlerini ödünç alıyor. Yeni bileşen öneriyorsan gerekçesi
"**iki** farklı ekranda kullanılacak" olmalı; tek ekranda kalacaksa o sayfanın kendi
yerleşimidir.

Konseptin vaat ettiği bir şeyi veri veremiyorsa (ya da tersi), bu bir **görsel sapmadır**:
`docs/TASARIM-SAPMALARI.md`'ye `GS` kaydı yaz — **şimdi**, Aşama 10'da değil.

> **Kullanıcı onaylamadan Aşama 5'e geçilmez.** Aşama 2, 3 ve 4 birlikte sunulur, tek onay
> alınır. Soru sorarsa cevapla ve beklemeye devam et.

## Aşama 5 — Düzen Sözleşmesi  *(ONAY KAPISI B)*

Onaylanan bütçeyi **blok şemasına** çevir. XAML yazılmaz; metin şeması yazılır. Her blok
için: bileşen adı, token'lar, hangi soruyu cevapladığı.

```
┌─ PageHeader ───────────────────────────────┐
│ Etiket_Slogan (TypeEyebrow, TextSecondary) │
│ Baslik_AnaSayfa (TypeTitle)      [Settings]│
└────────────────────────────────────────────┘
┌─ PeriodRail ───────────────────────────────┐
│ 15 Eylül 2026 Dönemi      (TypeSection)    │  ← S1
│ 4/30 gün · 15 Ekim         (TypeCaption)   │
│ ▓▓▓▓░░░░░░░░░░░  Indicator, RadiusPill     │
└────────────────────────────────────────────┘
┌─ HeroInputCard  SurfaceHero ───────────────┐
│ Etiket_MevcutTutar        (TypeEyebrow)    │  ← S2
│ [ 12.400,00        ]  [ Gözlemi Kaydet ]   │
│ Cumle_GozlemNotu          (TypeCaption)    │
└────────────────────────────────────────────┘
...
```

Şemada **her bloğun sağında cevapladığı soru kodu** (`← S1`) yazılı olur. Sorusuz blok
Aşama 4'ten kaçmış demektir; geri dön.

Şemayı `docs/EKRAN-KARTLARI.md`'ye `EK-<adım kodu>` başlığıyla yaz. Sonra üç durumu tanımla:
boş, yükleniyor, hata — her biri tek cümle.

> **Kullanıcı onaylamadan Aşama 6'ya geçilmez.** Yerleşimi gördükten sonra fikir değişir;
> bu kapı tam olarak o yüzden var.

## Aşama 6 — ViewModel Sözleşmesi + Kırmızı Test

Önce genel API, gövdeler boş:

```csharp
/// <summary>… neden var olduğu …</summary>
public sealed partial class DashboardViewModel : ViewModelBase
{
    [ObservableProperty] private decimal? currentBalance;
    [ObservableProperty] private ChartSeries? projection;

    [RelayCommand]
    private Task SaveObservationAsync() => throw new NotImplementedException();
}
```

Kurallar (`rules/03-mvvm.md`): **ham veri, metin üretilmez.** `decimal`, `DateOnly`, `bool`,
`ChartSeries` sunulur; `"12.500,00 ₺"` üretilmez, renk adı sunulmaz.

Sonra davranışı tarif eden testleri yaz ve **kırmızı olduklarını göster.** Testler
`NotImplementedException` ile değil beklenen değerle başarısız olmalı. Sınır durumları:
boş veri, tek kayıt, negatif bakiye, dönemin ilk ve son günü.

Eski projedeki ViewModel testlerinden **kopyalama** — eski repoda ViewModel'ler hiç test
edilemiyordu, kopyalayacak bir şey yok.

## Aşama 7 — Yeşil

En yalın implementasyon. Testler geçsin. "İleride lazım olur" diye özellik ekleme.

Bağımlılık sınırı: **en fazla 5.** Aşılıyorsa ekran bir çocuk ViewModel'e bölünür — bu meşru
ve tercih edilen yoldur (`rules/03-mvvm.md`).

## Aşama 8 — XAML

Aşama 5'te onaylanan blok şemasını, **yalnız var olan bileşenler ve token'larla** kur.

- Yeni bileşen yazma. Gerekiyorsa Aşama 5'te kararlaştırılmış olması gerekirdi; dur ve sor.
- Ham renk, ham punto, ham ölçü, ham glif yok (GK1, GK2, GK3, GK6).
- Renk yalnız `{DynamicResource}` ile bağlanır; punto ve ölçü `{StaticResource}` ile (GK8).
- Görünen hiçbir metinde eski ad yok; ürünün adı gerekiyorsa `Resources/Strings`'den (GK11).
- Her `ContentPage` ve `DataTemplate` `x:DataType` taşır.
- `DataTemplate` içinde `x:Reference PageRoot` yasak (Release/AOT'ta sessizce kopar).
- Görünen her metin `Resources/Strings`'den, GK5 önekleriyle.
- Her etkileşimli öğe `AutomationIds`'ten `{x:Static}` ile kimlik alır.
- Code-behind'de yalnız `InitializeComponent()`.

Bitince bütçe sayımını **tekrar** yap ve Aşama 4'teki tabloyla karşılaştır. Sapma varsa
sebebini söyle.

## Aşama 9 — Kalkan

```bash
dotnet build Mizan.sln -warnaserror -nologo -v q
```
```bash
dotnet test Mizan.sln -nologo -v q
```

Hepsi geçmeli: **0 hata, 0 uyarı, tüm testler yeşil.** Mimari testler (K1–K9) **ve** görsel
testler (GK1–GK12) dahil.

Bir görsel test kırmızıysa:

| Kırmızı olan | Ne demek |
|---|---|
| `Xaml_HamRenk_Iceremez` | Token yerine değer kopyalanmış |
| `Xaml_RenkTokeni_DynamicResourceIle` | Renk `StaticResource` ile bağlanmış; tema değişince o öğe eski renkte kalır |
| `GorunenMetin_EskiAdiIceremez` | Görünen bir metinde eski ad geçiyor |
| `Sayfa_GorselButceyiAsamaz` | Aşama 4'teki karar XAML'de uygulanmamış |
| `Sayfa_CumleButcesiniAsamaz` | Dördüncü cümle eklenmiş — o cümle sayı veya ikon olmalı |
| `HerSayfanin_EkranKarti_Var` | Aşama 5'te ekran kartı yazılmamış |
| `KontrastCiftleri_EsigiGecer` | Token değeri elle değiştirilmiş |
| `Xaml_StaticResource_TanimliAnahtaraBakar` | Olmayan bir stile/kaynağa bakılıyor; sayfa açılınca uygulama çöker |

**Kuralı esnetme, ekranı düzelt.** Kural gerçekten yanlışsa kullanıcıya söyle; kural
değişikliği ayrı bir iştir.

Kalkan yeşilse uygulamayı emülatörde **aç ve bırak**:

```powershell
./scripts/emulatorde-ac.ps1
```

Betik çalışan emülatör yoksa `mizan_emulator`'u başlatır, uygulamayı derleyip kurar ve ön
planda açar. **Bir kez** çalıştırılır; başarısızsa hatayı düzelt ve tekrar çalıştır.
Başarılıysa emülatör açık kalır ve ajan **emülatöre bir daha dokunmaz**: `adb` ile tıklama,
`screencap`, UI dökümü, tema değiştirme, görüntü okuma yok. Ajanın emülatörü sürmesi bir ekran
adımının maliyetinin çoğunu yiyordu. Eski projede regresyon betiği yedek koordinata tıklayıp
sonucu koşulsuz "başarılı" sayıyordu; ekrana ajanın değil kullanıcının bakması bunu önler.

Sonra kullanıcıya bir **görsel kontrol listesi** bırak:

- **Yol:** ekrana nasıl ulaşılır, tek satır (örn. "Profil seç → Ana sayfa")
- **Bak:** bu ekrana özgü en fazla beş risk, soru olarak (örn. "PeriodRail çubuğu açık
  temada zemine karışıyor mu?", "Uzun tutar hero'da taşıyor mu?")
- **Tema:** dolu hâl koyu ve açık temada. Geçiş: `adb shell cmd uimode night yes` / `no`

Kontrast testi yalnız tablodaki çiftleri görür. İnce bir ayırıcının açık temada kaybolduğunu
ya da bir öğenin tema değişince eski renkte kaldığını yalnız göz görür.

Boş, yükleniyor ve hata hâlleri her ekranda ayrıca gösterilmez: davranışlarını Aşama 6
testleri korur, görünüşleri ortak `StateBlock` bileşenindedir. Adım `StateBlock`'a dokunduysa
ya da ekran kendi durum görünümünü kurduysa, üç hâl de listeye eklenir.

> **ONAY KAPISI C:** Kullanıcı "tamam" demeden Aşama 10'a geçilmez.
>
> Sorun bildirilirse **emülatör kapatılmaz.** Ajan hatayı emülatörde kendisi yeniden üretmeye
> çalışmaz; kullanıcının tarifiyle çalışır, tarif eksikse **bir** soru sorar. Davranış
> hatasıysa (tür B) önce kırmızı test yazılır; görünüş hatasıysa (tür G) XAML düzeltilir.
> Sonra Aşama 9'un iki komutu koşulur ve betik tekrar çalıştırılır; betik açık emülatöre
> birkaç saniyede kurar ve uygulamayı yeniden başlatır. Liste yalnız düzeltilen maddeyle
> tekrar sunulur.
>
> Hata bildirimi (kullanıcı): **ekran · tema · ne gördüm · ne bekliyordum.** İsteğe bağlı
> olarak kullanıcının kendi aldığı bir ekran görüntüsü eklenebilir; ajan görüntü istemez.
>
> Otomatik emülatör regresyonu bu protokolün işi değildir; `K1` ve `K3` adımlarına aittir.

## Aşama 10 — Kayıt

Sırayla:

1. `docs/TASIMA-PLANI.md` — adımın kutusunu işaretle
2. `docs/DURUM.md` — "Bu adım ne getirdi" altına 3–6 satır; **bütçe sayımını da yaz** ve
   ekran görüntüsü yolu yerine "Görsel kontrol: kullanıcı onayladı (koyu + açık)."
3. `docs/EKRAN-KARTLARI.md` — kart tamamlanmış hâliyle (Aşama 5'te yazılmış olmalı)
4. `docs/TASARIM-SAPMALARI.md` — `GS` kaydı verildiyse (Aşama 4'te yazılmış olmalı)
5. Yeni invariant doğduysa `docs/INVARYANTLAR.md` — koruyan testin tam adıyla
6. Yeni kavram adlandırdıysan `docs/SOZLUK.md`
7. **Tek commit.** İngilizce özet + Türkçe gövde.

```
feat(app): add dashboard screen with period rail and projection chart

Ana sayfa taşındı ve yeniden tasarlandı (eski: MainPage.xaml 424 satır /
54 Label → 218 satır / 23 Label).

Eski GİDİŞAT kartındaki dört metin matrisi tek ComparisonStrip'e indi;
12 dönem projeksiyonu AreaTrend grafiğine taşındı. "Hesaplama detayı
(geliştirme)" bölümü ekrandan çıkarıldı (Aşama 4 kararı).

Bütçe: hero 1/1, kart 3/4, grafik 1/1, label 23/28, cümle 2/3.

Adım: V3 — ana sayfa
```

---

## Adım büyüklüğü

- Bir adım **bir ekran** getirir (çocuk ViewModel'leri dahil).
- Bir adım en fazla **~300 satır üretim kodu** (ViewModel + XAML birlikte).
- Bir adım **yeni bileşen doğurmaz.** Bileşenler Faz T'de doğar; istisna Aşama 5'te
  onaylanmış olmalı.

Üçünden biri aşılıyorsa Aşama 1'de dur, bölünme öner, kullanıcıya sor. Eski projedeki
`CommitmentsViewModel` 1.344 satır / 6 `partial` dosyaydı — `V6` adımı kesinlikle bölünecek.

## Faz T bitmeden Faz V başlamaz

Bu protokol bileşenlerin ve token'ların **var olduğunu** varsayar. `T1`–`T6` tamamlanmadan
bir ekran adımına başlamak, tasarım sistemini ekran ekran icat etmek demektir — yani eski
projenin 14 farklı görsel dilinin yeniden üretilmesi.

## Adım dışında fark edilenler

Bu protokol ekran adımının *içinde* fark edilenleri çözer. Uygulamayı kullanırken ya da
konsepte bakarken bir görsel tutarsızlık yakaladıysan — yani adım dışında — o iş
`duzeltme.md` akışına gider, **tür G**.
