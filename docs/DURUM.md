# Durum

Bu repo şu anda **boş iskelet** hâlinde. Hiçbir iş kodu taşınmadı.

## Nerede kalındı

| | |
|---|---|
| Son tamamlanan adım | **A14** — tarihçe sorgusu: `HistoryQueryService` |
| Sıradaki adım | **A15** — mevcut dönem motoru: `PeriodProgressService` |
| Test sayısı | 797 |
| Şema sürümü | — |

## Adım günlüğü

Her taşıma adımından sonra buraya en üste 3–6 satırlık bir giriş eklenir:
ne geldi, hangi kararı verdik, nereye dikkat etmeli.

### A14 — tarihçe sorgusu: `HistoryQueryService`

Geçmiş ekranının veri kaynağı olan salt okuma servisi `HistoryQueryService` ile modelleri `HistoryPeriod` (6 alan olduğu için init-only) ve `HistorySummary` taşındı. Servis, her gerçekleşmeyi orijinal planıyla, dönemin nihai revizyonuyla, kapanıştan çıkan finansal durumla ve A8'deki karneyle eşleştiriyor; `S26` gereği `IMizanStore` yerine yalnız `IPeriodHistoryRepository` kullanıyor. Nihai plan kuralı olduğu gibi korundu: kapanışa açılış günü (UTC takvim günü) dahil o güne kadar oluşan en son revizyon; aynı anda oluşanlarda büyük numara kazanır (`I27`). `S29` ile son dönemler özeti dönem sonu bakiyelerini (stok) toplamak yerine dönem içi net değişimleri topluyor; fark eskisiyle kuruşu kuruşuna aynı. Ayrıca tek dönem sorgusu bulunamayan id için `null` dönüyor, özet adedi sıfır/negatifse `ArgumentOutOfRangeException` fırlatıyor. **A16 dikkat:** nihai revizyon kuralı eskide `PeriodReviewService` içinde iki kez daha kopyalanmıştı; A16'da kopyalanmayacak, `HistoryQueryService.SelectFinalPlanRevisions` bağımlılıksız ortak bir yardımcıya çıkarılıp iki servis de onu kullanacak (T6, M8). 19 yeni test eklendi (toplam 797 test yeşil, Application kapsamı %97,94, Domain %97,59, mimari kalkanlar temiz).

### A13b — erken kapama önerisi: `LoanPayoffAdvisor`

"Bu krediyi hangi gün kapatabilirim?" sorusunu, ufuktaki her taksit gününü deneyip projeksiyonu kapamalı/kapamasız koşturarak cevaplayan `LoanPayoffAdvisor` ile `LoanPayoffAdvice` (9 parametreli positional record yerine init-only) ve `LoanPayoffAdviceStatus` taşındı. Öneri kuralı olduğu gibi korundu: hiçbir dönemde açık baz çizgiden büyümez **ve** net kazanç (12. dönem sonu farkı + ufuk sonrası ödenmeyecek taksitler) pozitifse gün önerilir, öneri en erken böyle gündür; en kârlı gün farklıysa o da verilir (`I26`). Eskide yalnız senaryo planı kurmak için bütün `SimulationCalculator` sürükleniyordu; artık D24'te ayrılan `ScenarioPlanBuilder` enjekte ediliyor. `S11` gereği `firstSalaryDate` → `firstPeriodStartDate`; 138 satırlık `AdviseLoan` K3 için aday bulma, aday değerlendirme ve özetleme adımlarına bölündü. Dikkat: ufuk sonrası açık faizi sayılmadığından derin açıkta sonuç `NotWorthIt` değil `NoSafeMonth` çıkar (kasıtlı sadeleştirme olarak kayda geçti). 12 yeni test eklendi (toplam 778 test yeşil, Application kapsamı %97,89, Domain %97,59, mimari kalkanlar temiz).

### A13a — kapatma bedeli ve kaydetme kapısı: `LoanPayoffService`

A13 ~460 satır ve iki iş yeteneği taşıdığı için ikiye bölündü; bu alt adımda kredinin bugünkü kapatma bedelini (`Describe`), planlı erken ödemelerin o günkü tutarlarını (`DescribePrepayments`) ve kaydetme kapısını (`PrepareForSave`) taşıyan `LoanPayoffService` ile modelleri `LoanPayoffOverview`, `PlannedLoanPrepayment` (K3 gereği müstakil dosyalar) taşındı. Kaydetmede bankanın kapatma tutarı otoritedir: tarihsizse bugünle damgalanır, anapara ondan geri çözülüp elle girilenin yerine yazılır; gelecek tarihli, son taksitten eski veya taksitlerle uyuşmayan tutar reddedilir (`I25`). `S28` ile modelin servisin statik metodunu çağırarak ürettiği engel metni (`IssueMessage`/`DescribeIssue`, M6/M8 ihlali) taşınmadı, metin V6'da `LoanAnalysisIssue`'dan üretilecek; hata mesajları Türkçe kaldı ama `tr-TR` kültürü taşımıyor (tarih sabit `dd.MM.yyyy`, tutar mesajda yok). Eski yorumdaki "(K2)" eski numaralandırmaydı, v2'de düşürüldü. 22 yeni test eklendi (toplam 766 test yeşil, Application kapsamı %98, Domain %96,92, mimari kalkanlar temiz).

### A12 — araç mutabakatı: `FinancialInstrumentReconciliationService`

*(Bu giriş A12 commit'inde (`7d55e74`) yazılmamıştı; A13 öncesinde koddan türetilerek eklendi.)* Dönem kapanışında fiilî ödemeleri kredilere, kartlara, vadeli planlara, büyük harcamalara ve erken ödemelere uygulayıp yeni döneme devreden sözleşme durumlarını üreten `FinancialInstrumentReconciliationService` ve sonuç modeli `ReconciledFinancialInstruments` taşındı. K3 sınırı için kredi tarafı (taksit ödendiğinde yalnız anapara payının düşmesi, erken ödemenin tüketilmesi, bayatlayan banka kapatma tutarının düşürülmesi) müstakil `LoanInstrumentReconciler`'a ayrıldı. `S27` kararıyla ödenmeyen taksit, borç ve büyük harcamaların devir tarihi `newAnchor + 1 gün` yerine doğrudan yeni dönemin ilk günü (`carryDate = newAnchor`) yapıldı. 15 yeni test eklendi (toplam 744 test yeşil).

### A11 — plan revizyonu: `HistoricalPlanRevisionService`

Açık nakit akış dönemi devam ederken kullanıcının kasıtlı planlama değişikliklerini (kart ödeme tercihi, yeni borç, büyük harcama iptali vb.) orijinal dondurulmuş planı (`PeriodPlanSnapshot`) mutasyona uğratmadan append-only plan revizyonları (`PeriodPlanRevision`) olarak kaydeden `HistoricalPlanRevisionService` ve iki plan taahhüdü arasında fark olup olmadığını inceleyen imza modelleri (`PlanRevisionSignature`, `PlanRevisionLineSignature`) taşındı. Kural M5 ve `S26` gereğince tanrı arayüz `IMizanStore` yerine dar port `IPeriodHistoryRepository` kullanıldı. K3 kuralı için 268 satırlık monolitik dosya servis ve imza modellerine bölündü; M3 yapıcı kuralı için `PlanRevisionLineSignature` init-only özelliklerle refactor edildi. `S1` uyarınca `PeriodAnchor`, `S18` uyarınca doğal dönemsellik (`period.Contains`) entegre edilerek dönem içi gerçekleşen günlük kart harcamalarının yapay plan revizyonu tetiklemesi engellendi. Fark yaratmayan plan güncellemelerinde mükerrer revizyon üretilmemesi ve orijinal plan taahhüdünün değişmezliği `I24` invariant'ı olarak sabitlendi. 14 yeni birim testi eklendi (toplam 729 test yeşil, Application kapsamı %96.43, Domain %94.29, mimari kalkanlar temiz).

### A10 — dönem planı dondurma: `PeriodPlanSnapshotService`, `FinancialSnapshotService`

Dönem başında nakit akışı taahhüdünü donduran `PeriodPlanSnapshotService`, finansal durum yaşam döngüsünü ve takvim döngüsü onarımını yöneten `FinancialSnapshotService` ve transfer paketi `FinancialSnapshotBundle` taşındı. Kullanıcının düzensiz gelir (tek seferlik arızi gelir veya başlangıç bakiyesiyle borç kapatma) durumunda da projeksiyon kurabilmesi için `FinancialPlan.CanBuildProjection` kuralı ve `I12` invariant'ı genişletildi. Kural M5 ve `S26` gereğince tanrı arayüz `IMizanStore` yerine dar port `IPeriodHistoryRepository` kullanıldı; K3 gereği `FinancialSnapshotBundle` müstakil bir modele ayrıldı ve `Freeze` gövdesi küçük yardımcılarla refactor edildi. `S1` uyarınca `IncomeDay` yerine `PeriodAnchor`, `S13` uyarınca `startingBalance`, `S7`/`S16` uyarınca dönemsel harcama havuzu ve `S18` uyarınca doğal dönemsellik (`period.Contains`) entegre edildi. Dondurulan plan taahhüdünün ve ödeme satırlarının dönem başladıktan sonra hiçbir plan mutasyonundan etkilenmeyeceği `I23` invariant'ı olarak sabitlendi. 14 yeni birim testi eklendi (toplam 715 test yeşil, Application kapsamı %96.43, Domain %94.29, mimari kalkanlar temiz).

### A9 — projeksiyon ince kabuğu: `FinancialProjectionService`, `ProjectionBoundaryResolver`

Saf Domain projeksiyon motorunu (`FinancialProjectionCalculator`) sunum katmanının Dashboard ve 12 dönem takvimi ihtiyaçlarına bağlayan ince uygulama kabuğu `FinancialProjectionService` ve projeksiyonun başlangıç çapasını, devreden açılış bakiyesini ve gerçekleşmemiş ilk dönemini çözümleyen `ProjectionBoundaryResolver` taşındı. Kural K3 ve M6 gereğince `ProjectionBoundary` servis dosyasından çıkarılıp müstakil bir modele (`Models/ProjectionBoundary.cs`) dönüştürüldü; `DashboardSnapshot` ile birlikte M3 yapıcı parametre sınırına tam uyum için init-only özelliklerle refactor edildi. `S18` uyarınca eski yapay tahsis ve strateji alanları (`CurrentStrategy`, `PendingStrategy`) `DashboardSnapshot`'tan elendi; `S11`, `S4`, `S13` ve `S1` gereğince yasaklı terimler (`FirstUnrealizedSalaryDate` -> `FirstUnrealizedPeriodStartDate`, `StartingSavings` -> `StartingBalance`, `TwelvePeriodEndingProjectedBalance` -> `TwelvePeriodEndingBalance`, `IncomeDay` -> `PeriodAnchor`) düzeltildi. K3 kuralı için metot gövdeleri (≤ 40 satır) küçük odaklı yardımcılarla sadeleştirildi. 13 yeni birim testi eklendi (toplam 701 test yeşil, Application kapsamı %96.43, Domain %94.29, mimari kalkanlar temiz).

### A8 — saf hesap yardımcıları: `PlanActualComparisonCalculator`, `ObligationValidation`

Kapanan nakit akış döneminin dondurulan/revize plan taahhüdü ile fiilî gerçekleşmesi arasındaki bütçe sapmalarını 12 ayrı kategoride kuruşu kuruşuna karşılaştırıp Türkçe açıklama özetini üreten saf hesaplayıcı `PlanActualComparisonCalculator` ve borç enstrümanlarının normalizasyonunu sağlayan `ObligationValidation` taşındı. Kural K3 gereğince `PlanActualComparison` ve `PlanActualComparisonLine` müstakil modellere çıkarıldı; S13 uyarınca yasaklı `ActualEndingSavings` özelliği `ActualEndingBalance` olarak düzeltildi. `PlanActualComparisonCalculator` içinde gereksiz ara nesne yerine doğrudan plan/revizyon kıyası yapılarak M3 ve K3 metot satır sınırlarına tam uyuldu; işletim sistemi dilinden bağımsız olarak Türkçe para formatı (`tr-TR`) sabitlendi. S23/S1/S2/S18 uyarınca eski ölü `ValidateOnboardingDraft` elendi; kart ve vadeli plan normalizasyonları ile Domain `CreditCardValidator` delegasyonu `ObligationValidation` altında toplandı. 18 yeni test eklendi (toplam 688 test yeşil, Application kapsamı %96.43, Domain %94.29, mimari kalkanlar temiz).

### A7 — depo kompozisyonu. Eski `IMizanStore` taşınmaz, dar portlar kullanılır (düğüm T10, S26)

Eski mimaride 10 ayrı veri deposunu miras alarak 40 metot barındıran tanrı arayüz `IMizanStore` ve servislerin dar portlar yerine bu arayüze bağlanması sorunu (Düğüm T10 — Ayrıştırma Dekoratif, Kural M5) çözüldü. `S26` kararı uyarınca eski `IMizanStore` bütünüyle elendi (Taşımama hakkı). A2–A6 adımlarında oluşturulan 12 dar portun bağımsızlığı korundu; veritabanı ilklendirmesi profil açılışına (`IProfileStoreSwitch`), tohumlama ise altyapı katmanına bırakıldı. `RepositoryContractsTests` ve `TypeSafetyRules` altında hiçbir depo portunun başka bir depoyu miras alamayacağı, kompozit depo arayüzü kurulamayacağı ve `IMizanStore`'un üretimde var olamayacağı mimari test kalkanıyla sabitlendi. 5 yeni test eklendi (toplam 670 test yeşil, Application kapsamı %96.43, Domain %94.29, mimari kalkanlar temiz).

### A6 — simülasyon taslağı portu: `ISimulationDraftRepository`, `SimulationDraft`, `SimulationDraftCondition`

Kullanıcının simülatör ekranında oluşturduğu varsayımsal plan taslaklarının, koşul sıralamasının ve aktiflik tercihlerinin kalıcı olarak saklanmasını sağlayan veri erişim portu ve modelleri taşındı. Kural K3 gereğince eski `SimulationDraftModels.cs` monolitindeki modeller müstakil dosyalara (`SimulationDraft.cs`, `SimulationDraftCondition.cs`) çıkarıldı ve K8 uyumlu Türkçe XML dokümantasyonları eklendi. `S25` kararı uyarınca eski şemadaki yasaklı terimleri (`SalaryScheduleEntry`, `PaymentAssignmentStrategies`, `OneTimeIncome`) taşıyan ve Kural M5'i ihlal eden plan uygulama batch metodu (`ApplySimulationBatchAsync`) arayüzden elendi (Taşımama hakkı); port yalnızca taslak yönetimine odaklanarak `ISimulationDraftRepository` olarak adlandırıldı. `RepositoryContractsTests` ve `InMemorySimulationDraftRepository` test çifti oluşturuldu; koşul sırasının, açık/kapalı durumunun korunması ve son güncellenme tarihine göre azalan sıralama testlerle kalkan altına alındı. 7 yeni test eklendi (toplam 665 test yeşil, Application kapsamı %96.43, Domain %94.29, mimari kalkanlar temiz).

### A5 — hatırlatıcı sözlüğü: `PaymentReminderPlanner`, `PaymentReminderPayload`

Ödeme günü hatırlatma sistemi sözlüğü, modelleri, saf zamanlama motoru ve Android bildirim veri taşıyıcısı taşındı. Kural K3 ve M3 gereğince eski `PaymentReminderModels.cs` monolitindeki 8 public tip müstakil dosyalara çıkarıldı ve 5'ten fazla özellik taşıyan modeller (`PaymentReminder`, `PaymentReminderBoard`, `PaymentReminderResponse`) init-only özelliklerle refactor edildi. 294 satırlık monolitik `PaymentReminderPlanner` saf zamanlama motoru (`PaymentReminderPlanner`) ve Türkçe bildirim/kart metin formatlayıcısı (`PaymentReminderFormatter`) olarak iki odaklı sınıfa bölündü. `S24` kararı doğrultusunda eski dağınık repo yapıları birleştirilerek bağımsız dar port `IPaymentReminderRepository` eklendi; `RepositoryContractsTests` ve `InMemoryPaymentReminderRepository` test çifti oluşturuldu. Aynı güne düşen ödemelerin konsolidasyonu, 35 günlük ufuk ve geçmiş saat filtresi, gece sessizliği (22:00–08:00 arası sabah 09:00'a öteleme) ve dayanıklı anahtarlama (`DueKey`) `I22` invariant'ı olarak sabitlendi. 43 yeni test eklendi (toplam 658 test yeşil, Application kapsamı %96.43, Domain %94.29, mimari kalkanlar temiz).

### A4 — dönem tarihçesi portu: `FinancialHistoryData`, `IPeriodHistoryRepository`, `IPeriodObservationRepository`

Dondurulmuş dönem planları, finansal durum anlık görüntüleri, revizyonlar, dönem gerçekleşmeleri ve açık dönemin gözlem defterini yöneten veri erişim portları taşındı. `FinancialHistoryData` salt okuma modeli (Query DTO) olarak Application katmanına alındı. Kural M5 uyarınca eski monolitik `IFinancialSnapshotRepository` ve `IObservationRepository` yapıları gözden geçirildi; bildirim ve hatırlatıcı yanıtları `S24` kararıyla A5 adımına ayrılarak `IPeriodObservationRepository` yalnızca canlı dönemin serbest nakit ve borç gerçekleşme işaretlerine odaklandı. Eski şemaya ait yasaklı modelleri barındıran `ApplyOnboardingSetupAsync` metodu `S23` uyarınca elendi (Taşımama hakkı). S12 ve `SOZLUK.md` doğrultusunda dönem kapanışı taahhüdü `CommitPeriodSettlementAsync` olarak sabitlendi. Port sözleşme kuralları (`RepositoryContractsTests`), DTO testleri ve bellek içi test çiftleri (`InMemoryPeriodHistoryRepository`, `InMemoryPeriodObservationRepository`) oluşturuldu. 7 yeni test eklendi (toplam 615 test yeşil, Application kapsamı %98.18, Domain %94.29, mimari kalkanlar temiz).

### A3 — profil servisi: `IProfileRepository`, `ProfileService`, `IProfileStoreSwitch`, `ProfileNameValidator`

Çok kiracılı profil mimarisinin portları ve yaşam döngüsü yönetim servisi taşındı. Kural K3 uyarınca eski kodda tek bir dosyaya yığılmış olan arayüzler müstakil dosyalara (`IProfileRepository.cs` ve `IProfileStoreSwitch.cs`) ayrıldı; 252 satırlık monolitik servis K3 sınırına (≤ 200 satır) uyum için ad doğrulama, Türkçe kültür kurallarıyla duyarsız karşılaştırma ("İpek" == "ipek") ve benzersiz isim türetme yetenekleri saf `ProfileNameValidator` sınıfına delege edilerek sadeleştirildi. S22 kararı (Taşımama hakkı) gereğince eski uygulamanın tek veritabanı döneminden kalan ve v2'de ölü kod olan `HasLegacyDatabase` ve `AdoptLegacyDatabaseAsync` metotları sözleşmeden temizlendi; veri aktarımı `G1` adımına bırakıldı. Açık olan ve son kalan profilin silinmesi engellenerek veri güvenliği korundu; profil açılışında `IProfileStoreSwitch` üzerinden veritabanı anahtarlanması ve oturum kimliği (`SessionId`) yenilenmesi garanti altına alındı. `RepositoryContractsTests` güncellendi, `InMemoryProfileRepository` ve `InMemoryProfileStoreSwitch` test çiftleri eklendi. 23 yeni test eklendi (toplam 608 test yeşil, Application kapsamı %97.96, Domain %94.29, mimari kalkanlar temiz).

### A2 — depo portları: `ILoanRepository`, `ICreditCardRepository`, …

Temel finansal enstrümanların ve kullanıcı ayarlarının veri deposu erişim kapıları olan dar portlar taşındı. Kural M5 ve Düğüm T10 gereğince 40 metotlu tanrı arayüz `IMizanStore` bütünüyle elendi; her varlık en fazla 6 metottan oluşan kendi odaklı arayüzüne kavuşturuldu (`ILoanRepository`, `ICreditCardRepository`, `ITemporaryPaymentPlanRepository`, `IPlannedLargeExpenseRepository`, `IRecurringIncomeRepository`, `IAdHocIncomeRepository`, `IUserSettingsRepository`). S5 ve S11 uyarınca ayrıcalıklı maaş yapısı elenerek çoğul `IRecurringIncomeRepository` ve tek seferlik `IAdHocIncomeRepository` olarak genelleştirildi; S18 doğal dönemsellik uyarınca yapay tahsis CRUD metotları ayarlardan temizlendi. `ILoanRepository` erken ödeme kaydı için `UpsertLoanPrepaymentAsync` metoduyla tamamlandı; `IUserSettingsRepository` M5 uyarınca yalnız `UserSettings` modeline odaklandı. Port sözleşme kuralları (`RepositoryContractsTests`) ve Application katmanı testlerinde kullanılacak bellek içi test çiftleri (`InMemory*Repository`) oluşturuldu. 10 yeni test eklendi (toplam 585 test yeşil, Application kapsamı %80, Domain %94.29, mimari kalkanlar temiz).

### A1 — saat ve profil kimliği: `IClock`, `UserProfile`

Application katmanının ilk adımı taşındı. Keşif sırasında `IClock`'un F2'de zaten taşınmış olduğu (arayüz `Mizan.Application/Abstractions`, `SystemClock` adaptörü `Mizan.Infrastructure/Time` altında, T9 düğümü) tespit edildi; bu adımda yalnız `UserProfile` record'u eklendi ve `TASIMA-PLANI.md`'deki A1 kutusu bu notla işaretlendi. Model, her profilin kendi izole `.db3` dosyasına karşılık geldiği kimliği taşır (`Id`, `Name`, `CreatedAt`, `LastOpenedAt`, `DefaultName = "Profilim"`, `MaxNameLength = 30`); eski `ProfileModels.cs` içeriğiyle davranışsal olarak birebir, yalnız K3 gereği dosya adı tip adıyla eşleşecek şekilde `UserProfile.cs` olarak taşındı. `SAPMALAR.md` tarandı, bu adımı etkileyen açık bir sapma kaydı bulunmadığı için sadakatle taşındı. 5 yeni test eklendi (toplam 575 test yeşil, Application kapsamı %100, mimari kalkanlar temiz).

### Düzeltme — S21: Banka ekstresinden otomatik içe aktarma (PDF Statement Import) özelliği elendi

Kredi kartı ekstrelerinin banka PDF belgelerinden otomatik ayrıştırılması özelliği, bankaların standart bir format sunmaması, analizin tamamlanmamış olması ve MVP'de yarım/kırılgan çalışması gerekçesiyle v2 kapsamından bütünüyle elendi (S21, Taşımama hakkı). F, D fazlarında taşınmış olan sınıflardaki izler temizlendi: `CreditCardStatementSource.cs` silindi; `CreditCardStatement` modelinden `Source`, `SourceDocumentFingerprint`, `ImportedAt` özellikleri, `CreditCardStatementProjection`'dan `StatementSource`, `CreditCardStatementCalculator`'dan `source` eşlemesi ve `CreditCardDateResolver`'dan `importedExactDate` parametreleri kaldırıldı. `Directory.Packages.props`'taki `PdfPig` bağımlılığı elendi; `A25` ve `I5` adımları iptal edildi. 3 bayat test temizlendi (toplam 570 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

### H4 — checkpoint taahhüdü: `PeriodSettlementCommit`, `PeriodSettlementCommitValidator`

Bir nakit akış döneminin kapanışı (checkpoint / settlement) anında, kapanan dönemin fiilî gerçekleşmesini (`PeriodActual`), güncellenen finansal araçları (krediler, kartlar, vadeli planlar, büyük harcamalar, tüketilen erken ödemeler) ve yeni dönemin dondurulmuş başlangıç planını (`PeriodPlanSnapshot`, `FinancialSnapshot`) atomik bir bütün olarak veritabanına aktaran taahhüt sözleşmesi taşındı. `S12` ve `SOZLUK.md` uyarınca yasaklı `Review` terimi elenerek model `PeriodSettlementCommit` olarak adlandırıldı; K3 kuralı gereğince `FinancialHistoryModels.cs` monolitinden `Mizan.Domain/Models/` altına müstakil bir dosyaya ayrıldı. 10 parametreli okunaksız positional record yapısı init-only özellikler ve boş liste varsayılanlarıyla refactor edildi. `FinancialHistoryData` salt bir okuma modeli (Query DTO) olduğu için Domain'e taşınmayıp Application A4 adımına bırakıldı (Seçenek A). Kapanan dönemin bitiş tarihi ile yeni dönemin başlangıç tarihinin eşleşmesi (`Actual.PeriodEnd == NewPlan.PeriodStart`), teyit edilen fiilî kapanış bakiyesinin yeni dönemin açılış bakiyesine devretmesi (`Actual.ConfirmedEndingBalance == NewSnapshot.ProjectionOpeningBalance == NewPlan.OpeningBalance`) ve snapshot referanslarının kenetlenmesi kuralları saf `PeriodSettlementCommitValidator` ve `IsConsistent` ile kalkan altına alınarak `I21` invariant'ı olarak sabitlendi. 15 yeni test eklendi (toplam 573 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

### H3 — dönem gözlem defteri: `PeriodObservation`, `PeriodObservationPayment`

Açık nakit akış döneminde kullanıcının anlık serbest nakit bakiyesini (`ObservedBalance`), dönemin canlı harcanan yaşam giderini (`ObservedLivingSpend`) ve plandaki borç ödemelerinin ara gerçekleşme durumlarını saklayan gözlem defteri modelleri (`PeriodObservation`, `PeriodObservationPayment`) taşındı. Eski mimaride tek bir dosyaya yığılmış olan modeller K3 ve M2 kuralları uyarınca `Mizan.Domain/Models/` altında müstakil dosyalara ayrıldı; K8 uyumlu Türkçe `<summary>` açıklamaları eklendi. `S20` kararı uyarınca Mizan'ın "bakiye üzerinden gidişat türetme" nakit akışı felsefesine aykırı olan, arayüzde hiç var olmamış ve eski kodda spekülatif ölü tablo olarak kalmış `PeriodObservationFlow` bütünüyle elendi (Taşımama hakkı). Gözlem defterinin snapshot zincirini ilerletmediği ve dondurulan planı mutasyona uğratmadığı ilkesi korundu; `HasObservedBalance`, `TotalObservedPayments`, `FindPayment`, `IsPaymentSettled`, `IsSettled` ve `CalculateVariance` gibi zengin iş yetenekleri eklendi. 9 yeni test eklendi (toplam 558 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

### H2 — dönem gerçekleşmesi: `PeriodActual`, `ActualPayment`, `ActualFlow`

Kapanan nakit akış döneminin fiilî karnesini, dondurulan plan taahhüdüyle (`PeriodPlanSnapshot`) karşılaştırarak saklayan gerçekleşme modelleri (`PeriodActual`, `ActualPayment`, `ActualFlow`, `ActualLivingBreakdown`, `ActualPaymentStatus`, `ActualFlowType`) taşındı. Eski projede tek bir dosyaya yığılmış olan 6 tip K3 ve M2 kuralları gereğince `Mizan.Domain/Models/` altında müstakil dosyalara ayrıldı; K8 uyumlu Türkçe `<summary>` açıklamaları eklendi. `S19` kararı uyarınca kurulum ile ilk çapa arasındaki hayalet dönemin kapatılması engellendi; `PeriodActual` her zaman tam bir nakit akış döneminin yarı açık aralığını (`[PeriodStart, PeriodEnd)`) denetleyecek şekilde `ContainsDate` ile kenetlendi. Kasa mutabakat farkı (`ReconciliationAdjustment`), ödeme kapanış durumu (`IsSettled`), plan/fiili farkı (`Variance`), plansız akış türleri ve toplam fiilî çıkışlar (`TotalActualOutflows`) zengin iş yetenekleri olarak modellendi. 25 yeni test eklendi (toplam 549 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

### H1 — dönem planı defteri: `FinancialSnapshot`, `PeriodPlanSnapshot`, `PeriodPlanRevision`

Dönem başında dondurulan nakit akışı taahhüdünü ve kasıtlı planlama değişikliklerini temsil eden tarihçe modelleri (`FinancialSnapshot`, `PeriodPlanSnapshot`, `PeriodPlanRevision`, `PeriodPlanPaymentLine`, `FinancialSnapshotSource`, `PlanPaymentSourceType`) taşındı. Eski mimaride tek bir 247 satırlık dosyaya yığılmış olan modeller K3 ve M2 kuralları gereğince `Mizan.Domain/Models/` altında müstakil dosyalara ayrıldı; K8 uyumlu Türkçe `<summary>` açıklamaları eklendi. `S1` sapması uyarınca yasaklı `IncomeDay` yerine `PeriodAnchor` entegre edildi; `S18` doğal dönemsellik ilkesi doğrultusunda yapay tahsis modu (`StrategyUsed`) ile yapay pencere alanları (`PaymentWindowStart`, `PaymentWindowEnd`) elendi. `PlannedInterest` türetilmiş özelliği eklenerek iki snapshot modeli arasında faiz hesap simetrisi sağlandı; `PlannedNetChange`, `TotalPlannedOutflows`, `HasDeficit` ve `ContainsDate` zengin iş yetenekleri kazandırıldı. 20 yeni test eklendi (toplam 524 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

### D24 — simülasyon motoru: `SimulationCalculator`

Kullanıcının canlı finansal planına dokunmadan "What-If" senaryolarını izole bir dünyada koşturup baz durumla karşılaştıran simülasyon motoru `SimulationCalculator` ve senaryo koşullarını plana işleyen saf `ScenarioPlanBuilder` taşındı. Eski projede 4 partial dosyaya ve 800+ satıra yığılmış olan monolitik yapı K3 ve K4 kuralları gereğince iki odaklı sınıfa ayrıldı; T1 düğümü ve M1 kuralı gereğince gizli `new` deseni elenerek tüm hesaplayıcı ve doğrulayıcı bağımlılıkları zorunlu parametre yapıldı. Kural K1 uyarınca Domain'e sızmış olan `CultureInfo.GetCultureInfo("tr-TR")` ve kültürlü para formatlamaları temizlendi; S8 ve S18 kararları doğrultusunda yapay tahsis modelleri bütünüyle dışarıda bırakılarak çoklu gelir zamları `IncomeAmountHistory` üzerinden işlendi. Kredi kartı ekstre, erken kapama, finansman ve nakit harcama etkileri `I20` invariant'ı olarak sabitlendi. 18 yeni test eklendi (toplam 495 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

### D23 — senaryo sözlüğü: `SimulationRequest`, `SimulationResult`

Kullanıcının varsayımsal harcama, borçlanma, taksit, finansman ve gelir artışı kararlarını mevcut plana dokunmadan izole olarak simüle etmesini sağlayan senaryo sözleşmeleri (`SimulationRequest`, `SimulationResult`, `SimulationImpactRow`, `SimulationRiskSummary`, `LoanPrepaymentImpact`, `SimulationScenarioType`) ve saf doğrulayıcı `SimulationRequestValidator` taşındı. Eski mimaride tek bir dosyaya yığılmış olan 6 tip K3 ve M2 kuralları uyarınca `Mizan.Domain/Models/` altında müstakil dosyalara ayrıldı; 15 parametreli okunaksız positional record yapısı M3 kuralı gereğince init-only özelliklerle ve yardımcı yapıcıyla refactor edildi. Yasaklı `SalaryChange` terimi S11 ve S8 uyarınca `IncomeChange` olarak düzeltildi ve çoklu gelir akışı yapısında hangi akışın (`RecurringIncomeId`) değiştiğini belirten modele kavuşturuldu; aynı akış için aynı tarihte mükerrer gelir değişikliği engellenirken farklı akışlara bağımsız zam yeteneği `I19` invariant'ı olarak sabitlendi. S18 kararı doğrultusunda eski yapay tahsis (`PaymentStrategyChange`, `NewCashFlowAllocationMode`) bütünüyle elendi. 28 yeni test eklendi (toplam 477 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

### D22 — hedef tutar: `TargetAmountCalculator`

Kullanıcının belirlediği finansal hedef tutara nakit akış projeksiyonu boyunca ne zaman ulaştığını veya başlangıç açılış bakiyesiyle zaten ulaşıp ulaşmadığını tespit eden saf hesaplayıcı `TargetAmountCalculator` ve sonuç sözleşmesi `TargetReachabilityResult` taşındı. Eski projede tek bir dosyaya yığılmış olan iki tip K3 ve M2 kuralları uyarınca müstakil dosyalara ayrıldı; D20 adımında standartlaştırılan `OpeningBalance` ve `EndingBalance` alanlarına bağlandı. Hedef tutarın pozitif olması, ilk dönem açılışında hedefe zaten ulaşılmışsa `IsAlreadyReached = true` dönmesi ve kronolojik kümülatif bakiye üzerinde ilk ulaşım döneminin bulunması `I18` invariant'ı olarak tescillendi. Savunmacı null denetimleri ve K8 uyumlu Türkçe dokümantasyon eklendi. 19 yeni test eklendi (toplam 449 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

### D21 — 12 dönemlik projeksiyon motoru: `FinancialProjectionCalculator`

Kullanıcının tüm gelir, borç, kart ve harcama dinamiklerini 12 nakit akış dönemi boyunca ileriye doğru simüle eden ana Domain yakınsama motoru `FinancialProjectionCalculator` taşındı. Eski mimarideki 6 bağımlılık ve yapay tahsis modelleri (Upcoming/Previous, geçiş catch-up bütçeleri) elenerek M3 kuralı uyarınca 5 saf bileşenle ve S18 doğal dönemsellikle bütünleştirildi. Dönem içi negatif bakiye oluştuğunda işletilen finansman açığı faizi (KMH) ve bu açığın sonraki döneme devredilerek telafisine kadar bileşik maliyet üretmesi `I17` invariant'ı olarak sabitlendi; kart ekstre döngüleri son ödeme tarihine göre ilgili döneme atanıp kümülatif faiz maliyetleri döküldü. K3 satır limitlerine (dosya ≤ 200, metot ≤ 40) tam uyuldu. 11 yeni test eklendi (toplam 430 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

### D20 — projeksiyon modeli: `CashFlowPeriodProjection`, `FinancialProjectionResult`

12 dönemlik nakit akış projeksiyonunun çekirdek veri modelleri (`CashFlowPeriodProjection`, `FinancialProjectionResult`, `CreditCardPaymentProjectionStatus`, `ProjectionInterestSummary`) taşındı. Eski mimaride tek bir dosyaya yığılmış olan 4 tip K3 kuralı uyarınca `Mizan.Domain/Models/` altında müstakil dosyalara ayrıldı; 37 parametreli okunaksız positional record yapısı M3 kuralı gereğince init-only özelliklerle refactor edildi. Eski modeldeki yapay tahsis çöplüğü (Upcoming/Previous modları, geçiş pencereleri, forward funded ve catch-up alanları) S18 doğal dönemsellik ilkesi doğrultusunda tamamen temizlendi; yasaklı `PrimaryIncome`/`OtherIncome` terimleri S5 ve S11 kararlarıyla `RecurringIncomeTotal` ve `AdHocIncomeTotal` olarak düzeltildi. Tek bir dönemin açılış/kapanış bakiyesi, finansman açığı faizi, devreden açık ve telafi göstergeleri saf türetilmiş özelliklerle modellendi; 12 dönemin kümülatif faiz toplamları `PeriodObligationPlan` ile bütünleştirildi. 7 yeni test eklendi (toplam 419 test yeşil, Domain kapsamı %100, mimari kalkanlar temiz).

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
