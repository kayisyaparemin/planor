# Durum

Bu repo şu anda **boş iskelet** hâlinde. Hiçbir iş kodu taşınmadı.

## Nerede kalındı

| | |
|---|---|
| Son tamamlanan adım | **D1** — para sözlüğü: 10 enum (`LoanKind`, `CreditCardPaymentType`, `CashFlowAllocationMode` …) |
| Sıradaki adım | **D2** — takvim kuralı: `CalendarRules` *(F2'de geldi, atlanabilir → D3)* |
| Test sayısı | 54 |
| Şema sürümü | — |

## Adım günlüğü

Her taşıma adımından sonra buraya en üste 3–6 satırlık bir giriş eklenir:
ne geldi, hangi kararı verdik, nereye dikkat etmeli.

### D1 — para sözlüğü: 10 enum (`LoanKind`, `CreditCardPaymentType`, `CashFlowAllocationMode` …)

Domain katmanının temel sınıflandırmaları ve kullanıcı tercihlerini temsil eden 10 enum taşındı.
Eski projede tek bir `FinanceModels.cs` dosyasına yığılmış olan enum'lar K3 kuralı gereğince `Mizan.Domain/Models/`
altında her biri tekil dosyaya ayrıldı ve K8 kuralına uygun Türkçe `<summary>` dokümantasyonu eklendi.
6502 sayılı Kanun kredi ayrımları, kart stratejileri ve nakit akış tahsis modlarının sayısal değerleri
ve sözleşmeleri 10 yeni testle kalkan altına alındı. Toplam 54 test yeşil, kapsam %100.

### F4 — `.runsettings` + kapsam eşiği, CI'da zorlanır hâle getirilir

Kapsam kalkanı kuruldu ve CI'da zorunlu kılındı. Kök dizine `.runsettings` eklenerek Coverlet ile
standart Cobertura formatında kapsam toplanması sağlandı. `scripts/verify-coverage.ps1` betiği ile
`Mizan.Domain` (%90), `Mizan.Application` (%80) ve `Mizan.Presentation` (%70) katman bazlı eşikleri
hesaplayıp denetleyen mekanizma oluşturuldu. `.github/workflows/ci.yml` iş akışı eklenerek PR ve push
süreçlerinde derleme, test ve kapsam eşiği denetimi zorunlu bir kapı hâline getirildi. 44 test yeşil.

### F3 — Para ve yuvarlama yardımcıları + `SOZLUK.md`'nin ilk doldurulması

Para ve yuvarlama standardı merkezileştirildi. `MoneyRules` saf hesap olarak `Mizan.Domain` altına
alındı; `MidpointRounding.AwayFromZero` ile 2 basamaklı kuruş yuvarlaması ve taksit/eşit bölüştürmede
kuruş artığını son parçaya aktaran `Distribute` metodu eklendi (`I2` invariant'ı, 10 yeni test).
Eski `ViewModelBase`'e gömülü kültürlü formatlayıcı elendi. `SOZLUK.md` S1–S16 kararları doğrultusunda
çoklu gelir akışı, dönem çapası ve yasaklı terimler sınırlarıyla eksiksiz dolduruldu. Toplam 44 test yeşil.

### F2 — `IClock` + `SystemClock` + takvim kuralları (`CalendarRules`)

Zaman ve takvim altyapısı taşındı. `CalendarRules` saf hesap olarak `Mizan.Domain` altına,
`IClock` portu `Mizan.Application` altına, `SystemClock` adaptörü ise `Mizan.Infrastructure` altına
alındı (T9 düğümü çözüldü, K3 gereği dosyalar ayrıldı). `BR-CALENDAR-01` artık yıl ve ay sonu kenetleme
davranışını koruyan 16 domain testi ve 2 altyapı testi eklendi. Toplam 25 test yeşil.

### F1 — Mimari test kalkanı

`Mizan.Architecture.Tests` altında K1–K8 mimari kurallarını ve tip/dosya boyutu
sınırlarını denetleyen 7 test yazıldı. Test projelerinde xUnit konvansiyonu için
CA1707 uyarısı bastırıldı. 7 test yeşil.

### Bootstrap — iskelet kuruldu

11 proje (5 kaynak + 6 test), kural kitabı, CI ve doküman iskeleti oluşturuldu.
Kod yok. `dotnet build` ve `dotnet test` yeşil, 0 test çalışıyor.

Alınan yapısal kararlar:
- `Mizan.Presentation` MAUI'ye referans vermiyor, bu yüzden ViewModel'ler test edilebilir
  ve UI tipi kullanmak derleme hatası.
- Her katmanın kendi test projesi var, katman ihlali proje referansıyla engelleniyor.
- App id `com.mizan.app`; eski `com.coinflow.mobile` mirası yok, veri taşıma G1 adımında.
