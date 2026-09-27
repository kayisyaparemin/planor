# Taşıma Planı

Eski proje: `C:\Users\kayis\Documents\mizan`

Her satır bir taşıma adımıdır ve bir prompt'a karşılık gelir.
Sıra **bağımlılık analizinden** çıkarılmıştır: bir adım, yalnızca üstündeki adımlarda
taşınmış şeylere dayanır. Sırayı değiştirmek geri dönmek demektir.

Protokol: `.antigravity/workflows/tasima-adimi.md` — yedi aşama, atlanamaz.

**Kritik yol:** `D1 → D2 → D3 → D5 → D13 → D16 → D17 → D19 → D20 → D21 → D24`
Geri kalan her şey bu omurgadan sarkar.

---

## Faz F — Temel

- [ ] **F1** — Mimari test kalkanı. `Mizan.Architecture.Tests` içinde K1–K8'i denetleyen testler.
      *Bu adım kasıtlı olarak birincidir: kuralları zorlayan mekanizma, kuralların koruyacağı
      koddan önce ayakta olmalı.*
      ⚠️ **Geri açıldı:** kural kitabı K9'u (yasaklı terim) listeliyor ama testi yok.
      `ArchitectureTests.YasakliTerimler_KaynaktaGecemez` ve
      `ArchitectureTests.YasakliTerimRegex_YanlisPozitifUretmez` yazılmalı.
- [x] **F2** — `IClock` + `SystemClock` + takvim kuralları (`CalendarRules`)
- [x] **F3** — Para ve yuvarlama yardımcıları + `SOZLUK.md`'nin ilk doldurulması
- [x] **F4** — `.runsettings` + kapsam eşiği, CI'da zorlanır hâle getirilir

## Faz D — Domain  *(saf hesap, sıfır paket)*

### Tier 0 — bağımlılıksız
- [x] **D1** — para sözlüğü: 8 enum (`LoanKind`, `CreditCardPaymentType` …) *(S21 ile `CreditCardStatementSource` elendi)*
- [x] **D2** — takvim kuralı: `CalendarRules` *(F2'de geldiyse atla)*

### Tier 1
- [x] **D3** — dönem takvimi: `CashFlowPeriod`, `CashFlowPeriodCalculator` — *yarı açık aralık burada doğar*
- [x] **D4** — gelir defteri: `RecurringIncome`, `IncomeAmountHistory`, `ActiveRecurringIncome`, `AdHocIncome`, `IncomeResolver`
- [x] **D5** — kredi sözleşmesi: `Loan`, `LoanScheduleCalculator`
- [x] **D6** — taksit bölüştürme: `ScheduledAmount`, `InstallmentScheduleCalculator`
- [x] **D7** — kart sözleşmesi: `CreditCard`, `CreditCardStatement`, `CardCharge`, ödeme planları *(S21 ile içe aktarma alanları elendi)*
- [x] **D8** — geçici ödeme planı: `TemporaryPaymentPlan`, `TemporaryPaymentInstallment`
- [x] **D9** — planlı büyük harcama + `UserSettings`
- [x] **D10** — dönem kullanım düzeni: **TAŞINMADI (ELENDİ)** — S18: Yapay tahsis (Upcoming/Previous) yerine doğal dönemsellik `[Start, End)` benimsendi
- [x] **D11** — kart ödeme tercihi: `CreditCardPaymentPreferenceResolver` *(izole, sırası esnek)*

### Tier 2
- [x] **D12** — gelir projeksiyonu: `IncomeProjectionCalculator`
- [x] **D13** — kredi itfası: `LoanAmortizationCalculator` *(bisection ile örtük faiz çözümü)*
- [x] **D14** — kart ekstresi: `CreditCardStatementCalculator` *(eskide 3 partial, 540 satır — bölündü: Calculator, DateResolver, PaymentDecisionResolver; S21 ile importedExactDate ve StatementSource elendi)*
- [x] **D15** — finansal plan bütünü: `FinancialPlan` *(ve `LoanPrepayment` sözleşmesi)*

### Tier 3
- [x] **D16** — kredi erken ödeme: `LoanPaymentScheduleBuilder`, `LoanReplay` *(model `LoanPrepayment` D15'te taşındı)*
- [x] **D17** — yükümlülük listesi: `ObligationModels` *(kendi leaf dosyası — kural M2)*,
      `ScheduledPaymentCalculator`, `MandatoryPaymentCalculator`
- [x] **D18** — kart ödemesi mutabakatı: `CreditCardActualPaymentReconciler`
      *(düğüm T2: projeksiyonu kendisi hesaplamayacak, hazır projeksiyon alacak)*

### Tier 4–6
- [x] **D19** — dönem ödemeleri gruplama: `PeriodObligationGrouper` *(eski 226 satırlık karmaşık `CashFlowAllocationPlanner` yerine doğal dönemsellikle yalınlaştırıldı)*
- [x] **D20** — projeksiyon modeli: `CashFlowPeriodProjection`, `FinancialProjectionResult`
- [x] **D21** — **12 dönemlik projeksiyon motoru**: `FinancialProjectionCalculator`
      — *Domain'in yakınsama noktası; Application'ın kapısı*
- [x] **D22** — hedef tutar: `TargetAmountCalculator`
- [x] **D23** — senaryo sözlüğü: `SimulationRequest`, `SimulationResult`
- [x] **D24** — simülasyon motoru: `SimulationCalculator`
      *(düğüm T1: bağımlılığını kendisi `new`lemeyecek, zorunlu parametre — K3/K4 gereği `ScenarioPlanBuilder` ve `SimulationCalculator` olarak ayrıştırıldı)*

## Faz H — Tarihçe  *(D'den bağımsız, paralel ilerleyebilir)*

- [x] **H1** — dönem planı defteri: `FinancialSnapshot`, `PeriodPlanSnapshot`, `PeriodPlanRevision`
      *(S31 düzeltmesiyle geri açılıp kapandı: plan ve revizyon `IncomeLines` / `PeriodPlanIncomeLine` taşır)*
- [x] **H2** — dönem gerçekleşmesi: `PeriodActual`, `ActualPayment`, `ActualFlow`
- [x] **H3** — dönem gözlem defteri: `PeriodObservation` ve çocukları (`PeriodObservationPayment`) — *S20 kararıyla spekülatif `PeriodObservationFlow` elendi*
- [x] **H4** — checkpoint taahhüdü: `PeriodSettlementCommit` *(eski `FinancialReviewCommit` — S12 gereği adlandırıldı)*

## Faz A — Application

- [x] **A1** — saat ve profil kimliği: `IClock`, `UserProfile` *(`IClock` zaten F2'de taşınmıştı; bu adımda yalnız `UserProfile` eklendi)*
- [x] **A2** — depo portları: `ILoanRepository`, `ICreditCardRepository`, `ITemporaryPaymentPlanRepository`, `IPlannedLargeExpenseRepository`, `IRecurringIncomeRepository`, `IAdHocIncomeRepository`, `IUserSettingsRepository` *(dar portlar — kural M5, düğüm T10; S2, S5, S11, S18)*
- [x] **A3** — profil servisi: `IProfileRepository`, `ProfileService`, `IProfileStoreSwitch`, `ProfileNameValidator` *(S22 ile `AdoptLegacyDatabase` elendi, `ProfileNameValidator` K3 kuralı için ayrıldı)*
- [x] **A4** — dönem tarihçesi portu: `FinancialHistoryData`, `IPeriodHistoryRepository`, `IPeriodObservationRepository` *(S23 ile `ApplyOnboardingSetup` elendi, S24 ile reminder ayrıldı)*
- [x] **A5** — hatırlatıcı sözlüğü: `PaymentReminderPlanner`, `PaymentReminderPayload` *(ve `PaymentReminderFormatter`, `IPaymentReminderRepository`)*
- [x] **A6** — simülasyon taslağı portu: `ISimulationDraftRepository`, `SimulationDraft`, `SimulationDraftCondition` *(S25 ile apply batch elendi, dar taslak portu yapıldı)*
- [x] **A7** — depo kompozisyonu. Eski `IMizanStore` **taşınmaz**; dar portlar kullanılır *(düğüm T10; S26 ile IMizanStore ve kompozit arayüzler elendi, mimari testle yasaklandı)*
- [x] **A8** — saf hesap yardımcıları: `PlanActualComparisonCalculator`, `ObligationValidation`
- [x] **A9** — projeksiyon ince kabuğu: `FinancialProjectionService`, `ProjectionBoundaryResolver`
- [x] **A10** — dönem planı dondurma: `PeriodPlanSnapshotService`, `FinancialSnapshotService`
      — *"dondurulmuş plan değişmez" invariant'ı burada doğar*
      *(S31 düzeltmesiyle geri açılıp kapandı: dondurma gelir satırlarını tarihleriyle üretir; satır üretimi 200 satır sınırı için `PeriodPlanLineBuilder`'a çıktı)*
- [x] **A11** — plan revizyonu: `HistoricalPlanRevisionService`
      *(S31 düzeltmesiyle geri açılıp kapandı: revizyon gelir satırlarını taşır, imza yalnız yatış günü değişikliğini de yakalar)*
- [x] **A12** — araç mutabakatı: `FinancialInstrumentReconciliationService`
- [x] **A13** — kredi kapatma *(~460 satır ve iki iş yeteneği olduğu için iki alt adıma bölündü)*
  - [x] **A13a** — kapatma bedeli ve kaydetme kapısı: `LoanPayoffService` *(S28 ile ekran metni ve kültür elendi)*
  - [x] **A13b** — erken kapama önerisi: `LoanPayoffAdvisor` *(`SimulationCalculator` yerine yalnız `ScenarioPlanBuilder`)*
- [x] **A14** — tarihçe sorgusu: `HistoryQueryService` *(S29 ile özet bakiye yerine net değişim toplar)*
- [x] **A15** — mevcut dönem motoru: `PeriodProgressService`
      *(düğüm T6: dondurma kuralı elle kopyalanmayacak, ortak yardımcı kullanılacak. İki katmana
      dokunduğu, ~500 satır olduğu ve dar portlarla 7 bağımlılığa çıktığı için bölündü; `S31`
      gereği `A15c`'den önce bir düzeltme girer)*
  - [x] **A15a** — KMH kuralı tek yerde: `DeficitFinancingRules` *(Domain; `FinancialProjectionCalculator` onu kullanır)*
  - [x] **A15b** — açık dönemi bulmak tek yerde: `FinancialHistoryData.FindOpenPlan`, `OpenPeriodLedgerReader` *(kural M8; `FinancialSnapshotService` ve `HistoricalPlanRevisionService`'teki kopyalar ona geçer)*
  - [x] **S31 düzeltmesi** — dondurulan plana gelir satırları *(H1, A10, A11'e dokunur; ayrı onayla)*
  - [x] **A15c** — mevcut dönemin gidişatı *(~440 satır ve iki iş yeteneği olduğu için iki alt adıma bölündü)*
    - [x] **A15c-1** — ödeme satırının durumu: `PeriodPaymentLineClassifier`, `PeriodPaymentLineClassification` *(S33)*
    - [x] **A15c-2** — gidişat: `PeriodProgressService`, `PeriodProgress` *(S30, S31, S32, S34)*
- [x] **A16** — dönem mutabakatı: `PeriodSettlementService` *(S12 gereği adlandırıldı; K3/K4 için `PeriodActualBuilder` ayrıldı, T6/M8 için `FindFinalRevisions` ortaklaştırıldı, S35–S39)*
- [x] **A17** — **plan okuma ve plan yazma ayrılır**: `IPlanReader` + `IPlanChangeRecorder`
      — *düğüm T5, kural M4; S40, S41, S42 ile 7 dar repo kompozisyonu ve sıfır yan etkili salt okuyucu kuruldu*
- [x] **A18** — kart yükümlülüğü: `CreditCardObligationService`
      — *S43: dar repo ICreditCardRepository ve yazma portu IPlanChangeRecorder'a bağlandı; ICreditCardObligationService arayüzü kuruldu; PDF içe aktarma kalıntıları temizlendi*
- [x] **A19** — simülasyon iş akışı: `ISimulationWorkflowService`
      — *S44: dar depolar IncomePlanWriter ve FinancialInstrumentWriter kompozisyonuna bağlandı; S45/S46 temizlendi; ISimulationPlanApplier ile tam M3 sınırında kalındı*
- [x] **A20** — yükümlülük yönetimi: `IObligationManagementService`
      *(S47: tanrı arayüz ve partial elendi; kredi, vadeli plan ve büyük harcamalar 5 dar bağımlılıkla IObligationManagementService'te toplandı; gelir yönetimi IIncomePlanService portuna ayrıldı)*
- [x] **A21** — dönem iş akışı: `IPeriodWorkflowService`
      *(S48: Kural M5 ve M3 gereği dönem mutabakat/gözlem servisi IPeriodWorkflowService ve ödeme hatırlatıcı servisi IPaymentReminderService olarak ayrıştırıldı; PaymentDueCollector odaklı yardımcı servisi eklendi)*
- [x] **A22** — cephe `MizanService`: **TAŞINMADI (ELENDİ)** — S49: Düğüm T7, Kural M3; 515 satırlık tanrı cephe elendi, ViewModel'ler dar portlara bağlanır, mimari testle yasaklandı
- [x] **A23** — sunum yardımcıları *(~1.300 satır ve iki bağımsız sunum yeteneği olduğu için iki alt adıma bölündü; S50)*
  - [x] **A23a** — dönem ayrıntısı sunumu: `CashFlowPeriodDetailPresenter`, modelleri (`CashFlowPeriodDetailData`, `DetailMetric` …) *(S50: Mizan.Presentation projesine taşındı, S18 yapay tahsis elendi, S11/S13 yasaklı terimler düzeltildi)*
  - [x] **A23b** — simülatör içgörüleri ve faiz kıyaslaması: `SimulatorInsightService`, `SimulatorProjectionMath`, `SimulatorInterestPresenter`, `SimulatorTimelineNarrative` *(S50: Mizan.Presentation projesine taşındı, K3/K4 için faiz kıyaslaması ve anlatı derleyicisi ayrıldı, M3 yapıcı sınırlarına tam uyuldu)*
- [x] **A24** — kataloglar: senaryo ve kayıt girişi katalogları: `SimulationScenarioCatalog`, `FinancialRecordEntryCatalog` *(S51: tekil dosyalara bölündü, Salary ve yapay tahsis elendi, M3 yapıcı sınırlarına tam uyuldu)*
- [x] **A25** — ekstre içe aktarma portları: **TAŞINMADI (ELENDİ)** — S21: Otomatik ekstre içe aktarma özelliği bütünüyle elendi
- [x] **A26** — yedekleme: `BackupService`, `IProfileBackupArchive`
      *(S52: BackupRetentionRules saf sınıfına ayrıldı, IBackupService dar portu eklendi, S22 uyarınca HasLegacyDatabase elendi)*
- [x] **A27** — telemetri portu: `ITelemetryService` *(`Abstractions/` altında — düğüm T9)*

## Faz I — Infrastructure

- [x] **I1** — **temiz şema v1**: 30 tablo, `PRAGMA user_version = 1`, gerçek foreign key'ler.
      *Eski v17'nin 16 yalancı kolonu, 12 ölü kolonu ve hiçbir migration'ı taşınmaz.*
      *(S53: 30 tablo — 29 eski tablo - S18 tahsis - S20 gözlem akışları + S2/S5 gelir geçmişi + S31 plan/revizyon gelir satırları)*
- [x] **I2** — depo implementasyonları *(~2.400 satırlık tanrı sınıf yerine dar port başına ayrı sınıf — S54; adım büyüklüğü kuralı uyarınca 4 alt adıma bölündü)*
  - [x] **I2a** — gelir ve kredi depoları: `SqliteUserSettingsRepository`, `SqliteRecurringIncomeRepository`, `SqliteAdHocIncomeRepository`, `SqliteLoanRepository` *(S54: doğrudan bağlantı, otomatik cascade, takma adsız temiz entity eşlemesi)*
  - [x] **I2b** — borç planları ve kredi kartı depoları: `SqliteTemporaryPaymentPlanRepository`, `SqlitePlannedLargeExpenseRepository`, `SqliteCreditCardRepository` *(S54: cascade silme, CreditCardEntityMapper ile temiz haritalama, K3 kuralı korundu)*
  - [x] **I2c** — taslak, bildirim ve canlı gözlem depoları: `SqliteSimulationDraftRepository`, `SqlitePaymentReminderRepository`, `SqlitePeriodObservationRepository` *(S54: dar portlar, S20 spekülatif akışlar elendi, foreign key cascade ile güvenli temizlik)*
  - [x] **I2d** — dönem tarihçesi ve mutabakat deposu: `SqlitePeriodHistoryRepository` *(S54: atomik transaction mutabakatı, S31 gelir satırları desteği, odaklı mapper/writer ayrımı)*
- [x] **I3** — profil deposu ve profil başına veritabanı
- [x] **I4** — yedekleme arşivi *(düğüm T8: `IProfileFileLayout` portu üzerinden. Eski 480 satır / 3 partial;
      K3/K4 ve belgelerle ~500 satır ve iki iş yeteneği olduğu için bölündü; eskide hiçbir adımda olmayan
      klasör deposu I4c olarak eklendi — S57)*
  - [x] **I4a** — yedek alma: `ProfileBackupArchive` (yazma, parmak izi, son yedek kaydı), `BackupArchiveFormat`,
        `BackupManifest`, `SqliteDatabaseSnapshot`, `DatabaseContentFingerprint`, `BackupStateFile`
        *(S57: biçim 2, `VACUUM INTO ?`, girdi adı arşiv sabitinden; port I4b'de üstlenilir)*
  - [x] **I4b** — yedeği tanıma ve geri yükleme: `ReadSummaryAsync`, `ImportAsync` *(biçim 1 = eski uygulamanın
        yedeği, açık mesajla reddedilir; `user_version` denetimi; hep-ya-hiç taşıma; `IProfileBackupArchive` bildirimi)*
        *(S58: sürüm iki uçtan, hazırlık yalnız veritabanlarını taşır, kaydı port yazar; `BackupManifestReader`,
        `BackupDatabaseValidator`, `ProfileImportTransaction`, `BackupWorkDirectory`)*
  - [x] **I4c** — yedek klasörü: `FolderBackupStorage`, `IStorageAccess`
        *(Aşama 3 kararı: v2 eskisiyle aynı `Mizan` klasörüne ve aynı `Mizan-yedek-` önekiyle yazarsa iki uygulama
        aynı günün dosyasını birbirinin üzerine yazar, "en yeni 7" temizliği eski uygulamanın yedeklerini — G1'in
        girdisini — siler)*
        *(S59: ayrım önekte — v2 `Mizan-yedegi-`, onaylı tek sabitlik Application istisnası; klasör deposu bütün
        dosyaları listeler; `AndroidStorageAccess`, izinler ve klasör adı V0/V13'te)*
- [x] **I5** — PDF ekstre içe aktarma: **TAŞINMADI (ELENDİ)** — S21: PdfPig ve banka ayrıştırıcıları elendi
- [x] **I6** — telemetri adaptörü + PII maskesi: **Sentry TAŞINMADI** — S60: DSN hiç ayarlanmamıştı (örnek
      adres), port hiçbir yerden çağrılmıyordu, internet izni yalnız Sentry'nin AAR'ından geliyordu, maske
      `Message`'da da bozuktu; yerine `NullTelemetryService` ve çevrimdışılık kalkanı (`I40`: ağ izni yok,
      `allowBackup` kapalı, Sentry paketi yasak)
      *(eskinin açığı: maske yalnız `event.Message`'ı kapsıyordu; exception metni, breadcrumb,
      extra ve ekran görüntüsü açıkta kalıyordu. `AttachScreenshot` varsayılan olarak kapalı.)*

## Faz T — Tasarım Sistemi (Planör)

`T1`–`T2` tasarım bootstrap'ı ile kuruldu (`docs/v2/05-TASARIM-BOOTSTRAP-PROMPT.md`).
`T3`–`T6` normal adımlardır ve `.antigravity/workflows/tasima-adimi.md` ile yürür, ama
kaynak "eski proje" değil, `docs/TASARIM-SISTEMI.md`'dir: Aşama 1 (keşif) sistemdeki tanımı
okur, Aşama 3 (sapma kararı) tanım eksik ya da yanlışsa `docs/TASARIM-SAPMALARI.md`'ye
`GS` kaydı yazar.

⚠️ **Faz T, `V0`'dan önce biter.** Bileşen yoksa ekran yazılamaz, token yoksa bileşen
yazılamaz. Faz V adımları `tasima-adimi.md` değil `tasarim-adimi.md` ile yürür (10 aşama).

- [x] **T1** — token katmanı: `DarkPalette.xaml` + `LightPalette.xaml` (23 token × 2 tema),
      `Tipografi.xaml` (7 kademe), `Olcu.xaml` (boşluk, yarıçap, vuruş). `Styles.xaml`
      token'lara geçer, tema sistemi izler (`GS7`). Kurallar **GK1, GK2, GK3, GK8, GK10**
      ve testleri.
- [x] **T2** — ikon ve marka katmanı: Material Symbols Rounded statik font, `Icons.cs`
      (12 ikon), üç boyut kademesi; uygulama etiketi `Planör`, Planör ikonu ve açılış
      ekranı. Kod adı `Mizan` kalır (`GS6`). Kurallar **GK6, GK11** ve testleri.
- [x] **T3** — bileşen kitaplığı: `Components/` altında 12 `ContentView`, her biri ≤ 200
      satır. Yeni bileşen ancak iki ekranda kullanılacaksa doğar (`PageHeader`, `PeriodRail`,
      `HeroInputCard`, `SummaryCard`, `ListCard`, `ComparisonStrip`, `MetricRow`, `NavRow`,
      `InfoBanner`, `ChartCard`, `StateBlock`, `ReminderCard`). Renkler yalnız `{DynamicResource}`
      ile bağlanır, iki temada denetlenir (`GS10`, `GS11`, `GS12`).
- [x] **T4** — grafik primitifleri: `Charts/` altında 4 `IDrawable`;
      `Mizan.Presentation/Charts/` altında ham seri tipleri (`ChartPoint`, `ChartSeries`,
      `ChartThreshold`, `ChartCategory`). Renk çizim anında okunur, tema değişince yeniden çizilir. Kural
      **GK7** ve testi (`GS13`).
- [x] **T5** — durum blokları: `StateBlock` (boş / hata) + `SkeletonBlock` iskelet yükleme
      deseni, `ScreenState` modeli ve spinner yasağı kalkanı (`GS14`).
- [x] **T6** — görsel bütçe testleri: **GK4, GK5, GK9.** *(T3–T5 bitmeden yazılamaz;
      sayım bileşen adlarına dayanıyor.)*

Faz V sırasında kullanıcı geri bildiriminden doğan sistem işleri. Ekran içinde değil,
bileşen / servis düzeyinde çözülür (`duzeltme.md` tür G, "sistem" satırı):

- [ ] **T7** — başlık aksiyonunun anlaşılırlığı: `PageHeader`'daki tek başına ikon (ilk
      kullanım: `EK-V7` kart değiştirme) dokunulabilir olduğunu belli etmiyor; kullanıcı
      kartını nereden değiştireceğini ilk bakışta bulamadı. İkon + kısa metin ya da seçici
      görünümü gibi, her ekranda aynı çalışacak bir çözüm. *(V7 Kapı C)*
- [ ] **T8** — diyalogların tasarımı: `IDialogService` seçim ve uyarıları Android'in düz
      sistem diyaloğuyla açıyor (kart seçme, vade kararı, varsayılan ödeme şekli). Planör
      token'larıyla çizilmiş, hareketli (örn. alttan açılan sayfa, geçiş animasyonu) bir
      diyalog; `MauiDialogService` arkasında, ViewModel'ler değişmeden. *(V7 Kapı C)*

## Faz V — Ekranlar

- [x] **V0** — kabuk ve altyapı: `ViewModelBase`, `INavigationService`, `IDialogService`,
      `Routes`, `AutomationIds`, `AppShell`, `MauiProgram`
- [x] **V1** — profil seçimi
- [x] **V2** — hatırlatıcı kartı *(sayfasız çocuk ViewModel)*
- [x] **V3** — ana sayfa (dashboard)
- [x] **V4** — kurulum sihirbazı *(eskide 958 satır / 3 partial — 8 ContentView adımı + OnboardingPlanWriter ile daraltıldı)*.
      Kapı C'de V3 ana sayfasının **dolu** hâli de iki temada kontrol edildi ve onaylandı.
- [x] **V7** — kart kontrol — **V6 ve V10'dan ÖNCE.** Eskide `CommitmentsPage` ve
      `SimulationPage` code-behind'de `CardControlViewModel` örnekliyordu; gizli bağımlılık.
      Tek adım (`S61`, `EK-V7`): sıradaki ödeme ve kararı, kararın bedeli, sonraki ödemeler,
      varsayılan ödeme şekli, elle ekstre girişi. Sol menüde **geçici** giriş. *(Önce V7a/V7b
      diye bölünmüştü; V7a tek başına ekranın cevabını vermediği için Kapı C'de birleştirildi.)*
- [ ] **V6** — finansal yapı *(eskide 1.344 satır / 6 partial — en büyük ViewModel)*.
      `S61`'den devralınanlar: gelecek kart harcaması girişi ve sol menüdeki geçici
      "Kart Kontrol" öğesinin kaldırılması (kart satırı `Routes.CardControl` + `cardId` açar).
      Aşama 1'de beş alt adıma bölündü (`S62`); her form adımı kendi türünü "Ekle" seçicisine
      ve satır diyaloğuna "Düzenle" olarak ekler.
  - [x] **V6a** — liste: `FinancialStructurePage`, dört grup (Gelirler, Kartlar, Krediler,
        Ödemeler), satır diyaloğu (kart → kart kontrol, sil); geçici "Kart Kontrol" öğesi
        kalkar (`S61`-8); `Routes.Commitments` → `Routes.FinancialStructure`; taşma yerinde
        açılır (`GS21`)
  - [ ] **V6b** — kart formu: ekle / düzenle + gelecek kart harcamaları (`S61`-7); başlıkta "Ekle"
  - [ ] **V6c** — kredi formu: ekle / düzenle + faiz ve bugün kapatma bedeli + planlı erken ödemeler
  - [ ] **V6d** — gelir formu: düzenli gelir, tutar değişikliği, tek seferlik gelir
  - [ ] **V6e** — ödeme formu: taksitli ödeme planı + planlı büyük harcama
- [ ] **V5** — ilk düzen seçimi *(V6'ya dayanır)*
- [ ] **V8** — 12 dönem
- [ ] **V9** — dönem ayrıntısı
- [ ] **V10** — simülatör *(eskide 1.034 satır / 4 partial)*
- [ ] **V11** — dönem kapanışı sihirbazı
- [ ] **V12** — geçmiş + geçmiş ayrıntısı
- [ ] **V13** — ayarlar + düzen değişikliği

## Faz K — Kalkanlar

- [ ] **K1** — E2E: `AutomationIds` sabitinden **üretilen** Maestro akışı
- [ ] **K2** — CI: PR kapısı, kapsam eşiği, APK doğrulama *(debug imza reddi, package id,
      versionCode, versionName, label kontrolleri)*
- [ ] **K3** — Emülatör regresyon betiği *(eskinin açığı: yedek koordinat ile tıklayıp
      sonucu koşulsuz "başarılı" sayıyordu — bu tekrarlanmayacak)*. Ekran adımlarında ajan
      uygulamayı yalnız `scripts/emulatorde-ac.ps1` ile açar, bakmak kullanıcıdadır;
      otomatik emülatör regresyonunun yeri burasıdır.
- [ ] **K4** — Sürüm hattı: sürüm notları `CHANGELOG.md`'den okunur, elle `echo` edilmez

## Faz G — Geçiş

- [ ] **G1** — Eski uygulamanın (`com.coinflow.mobile`, şema v17) yedek arşivini okuyan
      tek seferlik içe aktarıcı. Yeni app id `com.mizan.app` olduğu için cihazda güncelleme
      değil yan yana kurulum olur; veri bu yolla taşınır.
      *(S31: eski açık dönem planında gelir satırı yok — içe aktarıcı açık planı yeniden dondurmalı
      ya da boş `IncomeLines` gidişatta tanımlı davranışa bağlanmalı; Aşama 3'te karar verilir)*

---

## İlerleme

| Faz | Tamamlanan | Toplam |
|---|---|---|
| F | 3 | 4 *(F1 K9 testi için geri açıldı)* |
| D | 24 | 24 |
| H | 4 | 4 |
| A | 25 | 25 *(A22 ve A25 taşınmıyor)* |
| I | 5 | 5 *(I5 taşınmıyor; I4 üç alt adımda tamamlandı; I6'da Sentry taşınmadı — S60)* |
| T | 6 | 8 *(T7, T8 V7 Kapı C'de açıldı)* |
| V | 5 | 14 *(V6 beş alt adımda: V6a tamam)* |
| K | 0 | 4 |
| G | 0 | 1 |
