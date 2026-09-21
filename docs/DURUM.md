# Durum

Bu repo şu anda **boş iskelet** hâlinde. Hiçbir iş kodu taşınmadı.

## Nerede kalındı

| | |
|---|---|
| Son tamamlanan adım | **D6** — taksit bölüştürme: `ScheduledAmount`, `InstallmentScheduleCalculator` |
| Sıradaki adım | **D7** — kart sözleşmesi: `CreditCard`, `CreditCardStatement`, `CardCharge`, ödeme planları |
| Test sayısı | 132 |
| Şema sürümü | — |

## Adım günlüğü

Her taşıma adımından sonra buraya en üste 3–6 satırlık bir giriş eklenir:
ne geldi, hangi kararı verdik, nereye dikkat etmeli.

### D6 — taksit bölüştürme: `ScheduledAmount`, `InstallmentScheduleCalculator`

Taksitli harcama ve borç bölüştürme altyapısı taşındı. Eski projede tek dosyaya sıkıştırılmış olan `ScheduledAmount` değer nesnesi K3 ve M2 kuralları gereğince `Mizan.Domain.Models` altına tekil dosya olarak çıkarıldı; K8 uyumlu Türkçe XML dokümantasyonu eklendi. Eski kodda sınıf içine manuel yazılmış olan kuruş artık hesabı F3 adımında taşınan `MoneyRules.Distribute` saf metoduna delege edilerek `I2` invariant'ı (`BR-MONEY-01`) korundu; takvim vadeleri için `CalendarRules.AddMonthsKeepingDay` kullanılarak ay sonu kenetlenmesi ve artık yıl kuralları (`BR-CALENDAR-01`) sağlandı. 14 yeni test eklendi (toplam 132 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

### D4 — gelir defteri: `RecurringIncome`, `IncomeAmountHistory`, `ActiveRecurringIncome`, `AdHocIncome`, `IncomeResolver`

Gelir modelleri ve çoklu gelir çözümleyici taşındı. Eski mimaride tek bir maaşın geçmişi olarak tasarlanıp ikinci bir düzenli geliri sessizce ezen kritik model hatası (`S2` sapması) düzeltildi; `RecurringIncome` (akış) ve `IncomeAmountHistory` (akışa bağlı etkin tarihli tutar geçmişi) olarak ikiye ayrıldı (Seçenek 1). Her düzenli gelirin kendi ödeme gününü taşıması sağlandı (`S3`). Ayrıcalıklı maaş ve diğer gelir kavramları elenerek `RecurringIncome` ve `AdHocIncome` eşit vatandaş yapıldı (`S5`), yasaklı `Salary` terimi temizlendi (`S11`). `BR-INCOME-01` gereğince dönem başlangıcı itibarıyla geçerli en son tutarın seçilmesi ve dönem içi zamların korunması sağlandı (`I4` invariant'ı). K3 kuralı gereğince her tip tekil dosyaya ayrıldı, K8 uyumlu Türkçe XML özetleri eklendi. 10 yeni test eklendi (toplam 118 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

### D5 — kredi sözleşmesi: `Loan`, `LoanScheduleCalculator`

Kredi sözleşmesi (`Loan`) ve saf taksit takvimi üreticisi (`LoanScheduleCalculator`) taşındı. Eski projede `FinanceModels.cs` dosyasına yığılmış olan model K3 kuralı gereğince kendi tekil dosyasına ayrıldı, K8 uyumlu Türkçe XML özetleri ve savunmacı null/değer denetimleri eklendi. `BR-CALENDAR-01` gereğince 31 çeken aylardan Şubat (artık yıl 29 ve normal 28) ve 30 çeken aylara geçişlerde vade gününün korunması ve geri kazanılması garanti altına alındı. Kalan toplam nominal borç ve son taksit hesaplamaları donduruldu. 22 yeni test eklendi (toplam 108 test yeşil, Domain kapsamı %100).

### D10 — dönem kullanım düzeni: TAŞINMADI (ELENDİ)

Eski projede maaş öncesi/sonrası harcamaları yapay olarak farklı dönemlere kaydıran ve 600+ satırlık geçiş karmaşası (`TransitionCatchUp`, `ForwardFundedAmount`) üreten `CashFlowAllocationMode`, `CashFlowAllocationStrategy`, `PaymentAllocationStrategyResolver` ve `CashFlowAllocationPlanner` mimariden elendi (taşımama hakkı). Mizan v2'nin bağımsız dönem çapası (`PeriodAnchor`, `S1`) ve çoklu gelir akışı (`RecurringIncome`, `S2`) vizyonuyla uyumlu olarak "doğal dönemsellik" ilkesi benimsendi (`S18` sapması): vadesi `[PeriodStart, PeriodEnd)` aralığına düşen her kalem doğrudan o döneme aittir. D1'de taşınmış olan ölü `CashFlowAllocationMode` enum'ı temizlendi. Toplam 86 test yeşil, mimari kalkanlar temiz.

### D3 — dönem takvimi: `CashFlowPeriod`, `CashFlowPeriodCalculator`

Dönem takvimi altyapısı taşındı. `PeriodAnchor` değer nesnesi eklenerek tek tamsayı gün kısıtı aşıldı (S1 sapması uygulandı), `IncomeDay` yasaklı terimi temizlendi. `CashFlowPeriod` ile yarı açık aralık kuralı (`[Start, End)`) `I3` invariant'ı olarak sabitlendi (`Contains`, `DayCount`, `end > start` kontrolü). `CashFlowPeriodCalculator` ile ay sonu kenetlenmeli 12-60 dönemlik seri üretimi, dönem başlangıcı ve mutabakat (settlement) tarihi hesaplamaları sağlandı. K3 gereğince tipler tekil dosyalara ayrıldı. 33 yeni test eklendi. Toplam 87 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz.

### D2 — takvim kuralı: `CalendarRules`


F2 adımında zaman ve takvim altyapısı kapsamında (`CalendarRules.cs` ve `CalendarRulesTests.cs`) taşınmış
olan takvim kuralları doğrulandı. `BR-CALENDAR-01` kuralına göre ay sonu kenetlenmesi (Şubat 28/29,
30 çeken aylar) ve uzun aya geçişte tercih edilen günün geri kazanılması korunuyor. Ayrı bir kod
yazılmasına gerek kalmadan adım tamamlandı. Toplam 54 test yeşil, mimari kalkanlar temiz.

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
