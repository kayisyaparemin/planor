# Durum

Bu repo şu anda **boş iskelet** hâlinde. Hiçbir iş kodu taşınmadı.

## Nerede kalındı

| | |
|---|---|
| Son tamamlanan adım | **D19** — dönem ödemeleri gruplama: `PeriodObligationGrouper` |
| Sıradaki adım | **D20** — projeksiyon modeli: `CashFlowPeriodProjection`, `FinancialProjectionResult` |
| Test sayısı | 403 |
| Şema sürümü | — |

## Adım günlüğü

Her taşıma adımından sonra buraya en üste 3–6 satırlık bir giriş eklenir:
ne geldi, hangi kararı verdik, nereye dikkat etmeli.

### D19 — dönem ödemeleri gruplama: `PeriodObligationGrouper`

Kullanıcının kredi taksitleri, kredi kartı ekstre ödemeleri, vadeli borç senetleri ve planlı büyük harcamalarını ilgili nakit akış dönemlerine [Start, End) dağıtan saf `PeriodObligationGrouper` motoru ve `PeriodObligationGroup`, `PeriodObligationPlan` modelleri taşındı. Eski projede maaş öncesi/sonrası psikolojik kaydırmalar nedeniyle ortaya çıkan 600+ satırlık yapay tahsis makinesi (Upcoming/Previous modları, geçiş catch-up/forward funded tutarları) S18 doğal dönemsellik ilkesi uyarınca tamamen elendi (Seçenek 1). Yükümlülüklerin vadesi hangi dönemin yarı açık aralığına (`period.Contains(dueDate)`) düşüyorsa doğrudan ve tekil olarak o döneme atanması kuralı yeni `I16` invariant'ı olarak sabitlendi; ilk dönemden önce ve 12 dönemlik projeksiyon ufkundan sonra kalan kalemlerin sessizce kaybolmayıp açıkça `PreFirstPeriodItems` ve `PostHorizonItems` listelerinde raporlanması garanti altına alındı. 10 yeni test eklendi (toplam 403 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

### D18 — kart ödemesi mutabakatı: `CreditCardActualPaymentReconciler`

Dönem mutabakatı ve kapanışında kredi kartına yapılan fiili ödemeyi borçtan düşerek devreden bakiyeyi ve sonraki dönem başlangıç durumunu belirleyen saf `CreditCardActualPaymentReconciler` taşındı. Eski projede yaprak olması gerekirken yapıcıda ekstre hesaplayıcısı enjekte eden ve tek bir ödeme için 24 aylık projeksiyon çalıştıran T2 düğümü çözüldü: hesaplayıcı bağımlılığı ve kullanılmayan `carryInterestRate` parametresi elendi; hazır ekstre projeksiyonu (`CreditCardStatementProjection`) doğrudan parametre olarak alındı (Seçenek 1). Fiili ödemeden sonra yalnızca kalan anaparanın devretmesi ve faizin ödeme anında kapitalize edilmemesi kuralı (`I11` ve `I15`), ekstre kesim tarihine kadar olan harcamaların elenerek mükerrer sayımın önlenmesi ve kapanan ekstreye ait planların emekliye ayrılması sağlandı. 11 yeni test eklendi (toplam 393 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

### D17 — yükümlülük listesi: `ObligationModels`, `ScheduledPaymentCalculator`, `MandatoryPaymentCalculator`

Kullanıcının kredi, kredi kartı, vadeli borç ve taksitli ödemelerini tek bir takvim üzerinde birleştiren ve kategori bazlı zorunlu çıkış özeti (`MandatoryPaymentSummary`) üreten `MandatoryPaymentCalculator` ile vadeli borç taksitlerini yükümlülük kalemlerine (`ObligationItem`) dönüştüren `ScheduledPaymentCalculator` taşındı. Eski projede tek dosyaya yığılmış olan 5 tip K3 kuralı uyarınca müstakil dosyalara ayrıldı; modeller M2 kuralı gereğince `Models/` altına leaf olarak taşındı. Eski `ObligationItem` üzerinde yer alan yapay tahsis (`UpcomingPeriod` / `PreviousPeriod`) ve geçiş kalıntıları S18 doğal dönemsellik ilkesi doğrultusunda temizlendi; `PaymentAllocationReason` enum'ı elendi. M3 ctor parametre kısıtı (≤ 5) korunarak modeller init-only özelliklerle refactor edildi. Planlanan büyük harcamanın zorunlu özet toplamına girmemesi `I14` invariant'ı olarak sabitlendi. 17 yeni test eklendi (toplam 382 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

### D16 — kredi erken ödeme: `LoanPaymentScheduleBuilder`, `LoanReplay`

Kredi sözleşmesi üzerindeki kısmi ara ödeme ve erken kapama olaylarını kronolojik sırayla takvim üzerinde simüle eden `LoanPaymentScheduleBuilder` motoru, `LoanReplay`, `LoanScheduledPayment`, `LoanPaymentKind` modelleri ve `LoanPrepaymentValidator` taşındı. Eski mimaride 347 satırlık dev monolitik dosya ve 3 public tipi tek dosyada tutan yapı K3 kuralı uyarınca müstakil dosyalara bölündü; takvim oynatma motoru ile iş kuralı doğrulaması ayrıştırıldı (Seçenek 1). Domain katmanına sızmış olan `CultureInfo.GetCultureInfo("tr-TR")` bağımlılıkları temizlenerek kültürden bağımsızlaştırıldı. Erken kapamada o güne kadar vadesi gelen taksitlerin öncelikli tahsili, ara ödemede kıst faiz ve 6502 sayılı Kanun komisyonlarının işletilmesi, vade kısaltmada son taksit daralması ve taksit azaltmada annüite yeniden hesaplaması `I13` invariant'ı (`BR-LOAN-01`) olarak sabitlendi. 22 yeni test eklendi (toplam 365 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

### D15 — finansal plan bütünü: `FinancialPlan`

Kullanıcının tüm finansal varlık, yükümlülük, sözleşme ve bütçe parametrelerini tek bir çatı altında toplayan bütüncül `FinancialPlan` modeli ve saf `FinancialPlanValidator` taşındı. Eski mimarideki yapay tahsis modelleri (`PaymentAssignmentStrategies`) S18 doğal dönemsellik ilkesi doğrultusunda tamamen elendi; tek maaş ve diğer gelir kısıtları (`Salaries`, `OtherIncomes`) S2, S3 ve S5 kararları gereğince `RecurringIncomes`, `IncomeHistories` ve `AdHocIncomes` modellerine dönüştürüldü. Eski projede 4 farklı serviste kod tekrarına ve M8 ihlaline yol açan `CanBuildProjection` mantığı `FinancialPlan` üzerine saf bir türetilmiş özellik olarak alındı ve `I12` invariant'ı olarak sabitlendi. Model bütünlüğünü sağlamak adına D16'da planlanan 12 satırlık saf `LoanPrepayment` sözleşmesi bu adımda taşındı (Karar 1 / Seçenek 1). 19 yeni test eklendi (toplam 343 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

### D14 — kart ekstresi: `CreditCardStatementCalculator`

Kredi kartı ekstre döngülerini, dönem içi harcamaları, asgari ödeme tutarlarını ve devreden bakiye üzerindeki akdi faizi (carry faizini) simüle eden nakit akış projeksiyon motoru taşındı. Eski projede 3 partial dosyaya ve 540 satıra yayılmış olan monolitik yapı K4 (partial yasağı) ve K3 (200 satır sınırı) kuralları gereğince 3 saf bileşene (`CreditCardDateResolver`, `CreditCardPaymentDecisionResolver`, `CreditCardStatementCalculator`) bölündü; modeller (`CreditCardPaymentResolution`, `CreditCardStatementProjection`, `CreditCardPaymentDecision`) K3 ve M2 kuralları uyarınca müstakil dosyalara çıkarıldı ve M3 ctor kısıtı gereğince init-only özelliklerle refactor edildi. Kesilmiş ekstrelerde banka faizinin nihai kabul edilip mükerrer faiz işletilmemesi, kalan anaparanın ödeme anında kapitalize edilmeyip sonraki ekstrede satır faiz olarak yansıması ve banka kesin tarihlerinin korunması `I11` invariant'ı (`BR-CARD-01`) olarak sabitlendi. 28 yeni test eklendi (toplam 324 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

### D13 — kredi itfası: `LoanAmortizationCalculator`

Kredinin annüite taksit tutarından örtük aylık efektif faiz oranını bisection (ikiye bölme) yöntemiyle geri çözen, tarihli erken kapama bedelini ve 6502 sayılı Kanun'a dayalı yasal tazminatları hesaplayan saf motor taşındı. Eski projede tek bir 377 satırlık dosyaya yığılmış olan 6 public tip (`LoanRateSource`, `LoanAnalysisIssue`, `LoanAmortization`, `LoanAnalysis`, `LoanPayoffQuote`, `LoanAmortizationCalculator`) K3 kuralı uyarınca müstakil dosyalara ayrıldı; record modelleri ctor parametre kısıtını (M3) aşmayacak şekilde init-only özelliklerle refactor edildi. `LoanScheduleCalculator` zorunlu primary constructor parametresi yapılarak M1 kuralı sağlandı; özel `RoundMoney` yerine F3 adımındaki `MoneyRules.Round` entegre edildi. Taksit ödendiğinde yalnızca anapara payının düşmesi ve yasal erken ödeme komisyonu tavanları `I10` invariant'ı (`BR-LOAN-01`) olarak sabitlendi. 30 yeni test eklendi (toplam 296 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

### D12 — gelir projeksiyonu: `IncomeProjectionCalculator`

Dönemsel nakit akışında düzenli ve arızi gelirleri birleştiren saf projeksiyon hesaplayıcısı taşındı. Eski mimarideki tek maaş kısıtı (S2) kaldırılarak çoklu aktif gelir akışlarının (`ActiveRecurringIncome`) toplanması sağlandı; gelirin küresel ayara bağımlılığı elenerek her gelirin kendi `PaymentDay`'ine göre dönem içi gerçekleşme tarihine kavuşması (S3) ve kısa aylarda ay sonuna kenetlenmesi garanti altına alındı. Eski koddaki `Salary`, `PrimaryIncome` ve `OtherIncome` yasaklı hiyerarşisi temizlenerek eşit vatandaş `Recurring` ve `AdHoc` modelleri (`IncomeSourceType`, `IncomeProjectionItem`, `IncomeProjectionSummary`) K3 kuralı uyarınca müstakil dosyalara ayrıldı. İlk döneme mahsus çapa öncesi pencereye (`prePeriodIncomeStart`) düşen arızi gelirlerin kaybolmasını engelleyen ve çapa öncesini mükerrer saymayan kural `I9` invariant'ı (`BR-INCOME-01`) olarak kayda geçirildi. 14 yeni test eklendi (toplam 266 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

### D11 — kart ödeme tercihi: `CreditCardPaymentPreferenceResolver`

Kredi kartı ekstre ödeme tercihlerini (asgari, tamamı, özel tutar) etkin tarihli (effective-dated) geçmiş üzerinden çözümleyen saf hesaplayıcı taşındı. Eski projede tek dosyada instance ve statik metot karmaşası yaratan yapı M8 uyarınca tutarlı bir API'ye kavuşturuldu; bayat ve elenmiş harcama kaydırma referansları temizlendi, K8 uyumlu Türkçe XML özetleri ve savunmacı null denetimleri eklendi. Yürürlükteki kararın tespiti (`Resolve`), arayüz için kronolojik sıralama (`Ordered`), tablonun gereksiz şişmesini engelleyen karar özdeşliği denetimi (`RepresentsSameDecision`) ve iş kuralları doğrulaması (`Validate`) sağlandı. Etkin tarihli append-only tarihçe kuralı `I8` invariant'ı olarak tescillendi. 21 yeni test eklendi (toplam 252 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

### D9 — planlı büyük harcama + `UserSettings`

Planlanan tek seferlik büyük harcama (`PlannedLargeExpense`) ve kullanıcı ayarları (`UserSettings`) modelleri taşındı. Eski projede tek dosyaya yığılmış olan modeller K3 gereğince tekil dosyalara ayrıldı ve K8 uyumlu Türkçe XML özetleri eklendi. `PlannedLargeExpense` üzerine `IsActive` yeteneği kazandırıldı. `UserSettings` modelinde `S1` sapması doğrultusunda yasaklı `IncomeDay` yerine `PeriodAnchor` değer nesnesi; `S7` ve `S16` uyarınca aylık oranlamalı `MonthlyVariableExpenseAllowance` yerine dönem başına serbest havuzu temsil eden `PeriodVariableExpenseAllowance` yerleştirildi. `ProjectionOpeningBalance` ve `ProjectionAnchorDate` alanlarının Ayarlar UI ekranı için değil, başlangıç projeksiyon zemini olduğu netleştirildi. Eski projede kurulum tarihi ile çapa arasındaki yapay hayalet dönemin kapatılması saçmalığını önlemek amacıyla `S19` kararı (`SAPMALAR.md`) kayda geçirildi. Dağınık doğrulamalar `PlannedLargeExpenseValidator` ve `UserSettingsValidator` saf sınıflarında toplandı. 30 yeni test eklendi (toplam 231 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

### D8 — geçici ödeme planı: `TemporaryPaymentPlan`, `TemporaryPaymentInstallment`

Kredi ve kart haricindeki vadeli borç, senet, taksit ve periyodik yükümlülük modelleri taşındı. Eski projede `FinanceModels.cs` içine sıkıştırılmış ve anemik DTO olarak bırakılmış olan modeller K3 uyarınca tekil dosyalara ayrıldı; K8 uyumlu Türkçe XML dokümantasyonları eklendi. Modele ödenmemiş taksitleri toplayan kalan borç (`RemainingAmount`, `I7`), kalan taksit adedi (`RemainingInstallmentCount`), tamamlanma kontrolü (`IsCompleted`), vadesi en yakın sıradaki ödeme (`NextInstallment`) ve kronolojik sıralayıp PlanId kenetleyen `Normalize()` zengin yetenekleri kazandırıldı. Dağınık doğrulama kontrolleri `TemporaryPaymentPlanValidator` saf sınıfında toplandı. 26 yeni test eklendi (toplam 201 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

### D7 — kart sözleşmesi: `CreditCard`, `CreditCardStatement`, `CardCharge`, ödeme planları

Kredi kartı sözleşmesi ve ödeme planı modelleri taşındı. Eski projede tek bir dosyaya yığılmış olan 6 model (`CreditCard`, `CreditCardStatement`, `CardCharge`, `CreditCardPaymentPlan`, `CurrentStatementPaymentPlan`, `CreditCardPaymentPreference`) K3 ve M2 kuralları uyarınca tekil dosyalara ayrıldı; K8 uyumlu Türkçe XML dokümantasyonları eklendi. Ekstresiz ve ekstreli güncel borç ayrımı (`KnownTotalDebt`, `I5`, `BR-CARD-01`) ile türetilmiş `AvailableLimit` ve `IsActive` özellikleri eklendi. UI form yükünü hafifletmek amacıyla BDDK yasal mevzuatına dayalı asgari ödeme oranı (`CreditCardRules.ResolveMinimumPaymentRate`, `I6`, `BR-CARD-04`) ve standart vade kuralı tanımlandı. Dağınık doğrulama mantığı saf `CreditCardValidator` sınıfında toplandı. 34 yeni test eklendi (toplam 175 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

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
