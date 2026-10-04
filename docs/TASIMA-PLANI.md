# Taşıma Planı

Eski proje: `C:\Users\kayis\Documents\mizan`

Her satır bir taşıma adımıdır ve bir prompt'a karşılık gelir.
Sıra **bağımlılık analizinden** çıkarılmıştır: bir adım, yalnızca üstündeki adımlarda
taşınmış şeylere dayanır. Sırayı değiştirmek geri dönmek demektir.

Protokol: `/tasima-adimi` (`.claude/skills/tasima-adimi/SKILL.md`) — sekiz aşama, atlanamaz.

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
- [x] **D22** — hedef tutar: `TargetAmountCalculator` *(V10b1'de silindi: `S76`-10; "şu tarihte X harcarsam?" bir nakit ödeme denemesidir)*
- [x] **D23** — senaryo sözlüğü: `SimulationRequest`, `SimulationResult`
- [x] **D24** — simülasyon motoru: `SimulationCalculator`
      *(düğüm T1: bağımlılığını kendisi `new`lemeyecek, zorunlu parametre — K3/K4 gereği `ScenarioPlanBuilder` ve `SimulationCalculator` olarak ayrıştırıldı)*

## Faz H — Tarihçe  *(D'den bağımsız, paralel ilerleyebilir)*

- [x] **H1** — dönem planı defteri: `FinancialSnapshot`, `PeriodPlanSnapshot`, `PeriodPlanRevision`
      *(S31 düzeltmesiyle geri açılıp kapandı: plan ve revizyon `IncomeLines` / `PeriodPlanIncomeLine` taşır)*
- [x] **H2** — dönem gerçekleşmesi: `PeriodActual`, `ActualPayment`, `ActualFlow`
- [x] **H3** — dönem gözlem defteri: `PeriodObservation` ve çocukları (`PeriodObservationPayment`) — *S20 kararıyla spekülatif `PeriodObservationFlow` elendi*
- [x] **H4** — checkpoint taahhüdü: `PeriodSettlementCommit` *(eski `FinancialReviewCommit` — S12 gereği adlandırıldı)*
- [x] **H5** — dönem içinde birden fazla gözlem *(V3 yenilemesi, kaynak eski proje değil — bkz. `V3`)*. Sözlük gözlemi "dönem
      içinde kullanıcının girdiği anlık bakiye" diye tanımlıyor, ama kod dönem başına **tek** kayıt tutuyor
      ve her bakiye girişinde üzerine yazıyor (`period_observations.PeriodPlanSnapshotId UNIQUE`,
      `PeriodWorkflowService.ObserveCurrentBalanceAsync`). Artık her bakiye girişi yeni bir gözlem olur,
      "son gözlem" en yenisidir; ana sayfa grafiği dönemin gözlemlerini çizer. Aşama 3 kararları:
      - Ödeme işaretleri (`PeriodObservationPayment`) bugün bu tek kaydın çocuğu ve bakiyeden bağımsız
        yazılıyor (`ObservePaymentAsync`). Dönemin kendisine mi bağlanır, her gözlem mi taşır?
      - Aynı gün ikinci giriş öncekinin yerine mi geçer (gün başına tek gözlem, UNIQUE indeksle)?
      - Dönem kapanınca gözlemler silinir mi (bugün `FinalizeSettlementAsync` siliyor), `V12` için kalır mı?
      - Dönem bitmiş ama kapanmamışken (kapanış ertelendi) girilen bakiye hangi döneme yazılır? Bugünün
        tarihiyle girilirse biten dönemin aralığı dışında kalır; tarih biten dönemin son gününe mi
        kenetlenir, yoksa bu durumda giriş kapanışın bakiyesi mi sayılır?

      Yeni `S` kaydı. *(S68: işaret döneme bağlanır ve `PeriodPaymentMark` olur, aynı gün ikinci giriş
      öncekinin yerine geçer, kapanış gözlemleri silmez, kapanmamış biten döneme bakiye yazılmaz — önce kapanış.
      H5 yalnız kuralları getirdi: `PeriodObservationRules`, `I80`–`I82`. Model şekli şemayla birlikte `I7b`'de;
      sıra `H5 → I7a → I7b → A28`.)*

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
- [x] **A28** — dönemin gözlemleri *(V3 yenilemesi, kaynak eski proje değil — bkz. `V3`; `H5`'e dayanır)*: bakiye girişi yeni gözlem
      ekler, üzerine yazmaz; `OpenPeriodLedger` dönemin gözlemlerini taşır, gidişat son gözlemi kullanır.
      `IPeriodObservationRepository`'nin tek kayıt dönen imzası listeye döner. "Bakiye gir" sayfası için
      iki ek (konsept, bkz. `V3`):
      - **Gözlem tarihi seçilir** (varsayılan bugün). Bugün `ObserveCurrentBalanceAsync` tarihi
        `_clock.Today`'den alıyor. Geriye tarihli gözlem serinin ortasına girer; "son gözlem" giriş
        sırasına değil tarihe göredir. Tarih dönem içinde olmalı, gelecek olamaz.
      - **Önizleme:** kaydetmeden, girilen bakiyeyle gidişatı hesaplayan salt okuma
        (`PeriodProgressCalculator` bağımlılıksız; taslak gözlemi deftere ekleyip hesaplar).

      Aşama 1'de ~300 satırı aşarsa bölünür. **`I7b`'den sonra** (S68). S68'den gelenler: kapanış gözlemleri
      silmez (7); ödeme işaretinin bakiyeye yansıması ödeme gününe göre (8); kapanmamış biten döneme gözlem
      yazılmaz, "Bakiye gir" önce kapanışı ister (4, `PeriodObservationRules.CanObserveOn`). Açık notlar: kurulum
      gözlemi dönemden önceki güne düşebiliyor; hatırlatıcı cevabı gözlemle zaman damgasıyla kıyaslanıyor,
      geriye tarihli gözlemde bu yanıltır.
- [x] **A29** — harcama temposu *(V3 yenilemesi, kaynak eski proje değil — bkz. `V3`)*: `PeriodProgress`'e yaşam havuzundan harcanan
      oran ile geçen süre oranı ve aradaki fark (puan). **İkisi aynı güne göre** hesaplanır: son gözlemin
      günü. Bugünle kıyaslanırsa bakiye girilmedikçe harcama donar, süre ilerler ve ekran "harcama geride"
      diyerek yanlış güven verir. Gözlem yoksa tempo yok. Kavram önce `SOZLUK.md`'ye girer.
- [x] **A30** — dönemin bakiye rotası *(V3 yenilemesi, kaynak eski proje değil — bkz. `V3`; `A28`'e dayanır)*
      *(S71: her gün bir nokta, günün başındaki bakiye; aralar plandan, fark günlere yayılır; son nokta ekrandaki
      rakam; `PeriodBalancePathCalculator`, `ProjectedPaymentAmount` (M8); `I105`–`I107`)*.
      `V3` Aşama 1'de bulundu (2026-09-30): `S68-6`'yı (çizgi noktaların arasında ve son noktadan sonra
      plandan çizilir; gözlem yoksa baştan sona plan) iki adım birbirine bırakmış — `T10` günlüğü "`A28`'in
      işi", `A28` notu f "nokta serisi `V3`'ün" diyor. Hesap her gelir ve ödemeyi kendi gününe koyuyor, yani
      iş kuralı; Presentation'da yapılamaz (kural 01). `PeriodProgress` dönemin gözlemlerini ve rotayı
      taşır; `PreviewAsync` taslak gözlemle aynı rotayı verir ("Bakiye gir" önizleme grafiği).

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
- [ ] **I7** — ilk şema yükseltmesi, v1 → v2 *(V3 yenilemesi, kaynak eski proje değil — bkz. `V3`)*. Bugün `DatabaseSchema` yalnız
      boş veritabanını v1'e kurar; v1'i yükseltecek bir yol yok. İki alt adım:
  - [x] **I7a** — göç altyapısı: sürüm sürüm ilerleyen, işlem içinde çalışan yükseltme; eski sürümlü
        yedek geri yüklenince yükseltilir (`BackupDatabaseValidator` bugün yalnız "daha yeni"yi reddediyor).
        Emülatördeki telefon verisiyle (v1) denenir.
        *(S69: şema bir göç listesidir — `SchemaMigration`, `SchemaMigrations`, `SchemaMigrationRunner`; boş
        veritabanı da v1'den başlar, `CurrentSchemaVersion` sabiti çıktı. Yükseltme tek işlem; göç sırasında yabancı
        anahtar denetimi kapalı, sonunda `foreign_key_check`. Yedekten gelen eski sürüm hazırlıkta yükseltilir.
        Yayımlanmış adım değişmez: kural `05`. `I83`–`I86`)*
  - [x] **I7b** — v2 ve gözlem modelinin yeni şekli (S68-2, 8, 9). Aşama 1'de iş yeteneğine göre ikiye bölündü
        (dört katman, iki bağımsız yetenek); sıra `I7b1 → I7b2`:
    - [x] **I7b1** — ödeme işareti gözlemden ayrılır (S68-8): `PeriodPaymentMark`, `period_payment_marks`
          (`(PeriodPlanSnapshotId, PeriodPlanPaymentLineId)` UNIQUE), v2 göçü eski gözlem ödemelerini kopyalayıp
          eski tabloyu düşürür. Port ayrı açılmadı, iki metot eklendi. İşaret koymak gözleme dokunmaz; kapanış
          işaretleri silmez. `I87`–`I89`. Gözlem eski şekliyle kaldı.
    - [x] **I7b2** — gözlemin yeni şekli (S68-2, 9): `period_observations` yeniden kurulur (v3): `(PeriodPlanSnapshotId,
          ObservedOn)` UNIQUE, bakiye zorunlu (bakiyesiz eski satırlar elenir), `ObservedLivingSpend` ve `Note`
          çıkar, iki damga yerine tek kayıt zamanı. Port listeyle çalışır; Application yalnız derlenecek kadar
          uyarlanır, davranış `A28`'de. Yeniden kurma sırası aşağıdaki nottadır.
        *(I7a'dan: v2, `SchemaMigrations`'a yeni adım olarak yazılır ve özeti `SchemaMigrationsTests`'e eklenir.
        UNIQUE ancak tablo yeniden kurularak kalkar: yenisini kur → kopyala → eskisini sil → yenisinin adını
        değiştir → indeksleri yeniden kur; sıra önemli, önce eskisinin adı değişirse çocukların bağı ona döner.
        Bu yol emülatördeki 4 profilin ve telefon dönüşümündeki 2 profilin kopyasında prova edildi: satır
        sayıları aynı, `integrity_check` ok, kopuk bağ 0.)*

## Faz T — Tasarım Sistemi (Planör)

`T1`–`T2` tasarım bootstrap'ı ile kuruldu (`C:\Users\kayis\Documents\mizan\docs\v2\05-TASARIM-BOOTSTRAP-PROMPT.md`; eski depo, arşiv, otorite değil).
`T3`–`T6` normal adımlardır ve `/tasima-adimi` ile yürür, ama
kaynak "eski proje" değil, `docs/TASARIM-SISTEMI.md`'dir: Aşama 1 (keşif) sistemdeki tanımı
okur, Aşama 3 (sapma kararı) tanım eksik ya da yanlışsa `docs/TASARIM-SAPMALARI.md`'ye
`GS` kaydı yazar.

⚠️ **Faz T, `V0`'dan önce biter.** Bileşen yoksa ekran yazılamaz, token yoksa bileşen
yazılamaz. Faz V adımları `/tasima-adimi` değil `/tasarim-adimi` ile yürür (10 aşama).

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
bileşen / servis düzeyinde çözülür (`/duzeltme` tür G, "sistem" satırı):

- [ ] **T7** — başlık aksiyonunun anlaşılırlığı: `PageHeader`'daki tek başına ikon (ilk
      kullanım: `EK-V7` kart değiştirme) dokunulabilir olduğunu belli etmiyor; kullanıcı
      kartını nereden değiştireceğini ilk bakışta bulamadı. İkon + kısa metin ya da seçici
      görünümü gibi, her ekranda aynı çalışacak bir çözüm. *(V7 Kapı C)*
- [ ] **T8** — diyalogların tasarımı: `IDialogService` seçim ve uyarıları Android'in düz
      sistem diyaloğuyla açıyor (kart seçme, vade kararı, varsayılan ödeme şekli). Planör
      token'larıyla çizilmiş, hareketli (örn. alttan açılan sayfa, geçiş animasyonu) bir
      diyalog; `MauiDialogService` arkasında, ViewModel'ler değişmeden. *(V7 Kapı C)*
      Tarih seçicinin açılan penceresi de Android'in sistem penceresi; `V6c3` Kapı C'de yalnız düğme
      rengi düzeltildi (`Platforms/Android/Resources/values/styles.xml`), tasarımı bu işin kapsamında.
- [x] **T9** — kaydırılan hero: **GK4 kural değişikliği** *(GS22: aynı anda ≤ 1 grafik; `HeroPager` ≤ 1, ≤ 2 sayfa, sayfa başına ≤ 1 grafik; kart zemini `SurfaceCard`, token değişmedi; koruyan: I92, I93)* *(V3 yenilemesi, kaynak eski proje değil — bkz. `V3`. Kullanıcı kararı,
      2026-09-29: ana sayfada iki görünüm; açılışta grafik, sağa kaydırınca halka)*. Bugün
      `DesignBudgetAnalyzer` her `GraphicsView`'ü sayıyor, sınır 1. Önerilen kural: ekranda **aynı anda**
      en fazla bir grafik görünür; sayfada en fazla bir kaydırılan hero, en fazla iki sayfa, sayfa başına
      bir grafik. Kural metni (`06-tasarim.md`, `TASARIM-SISTEMI.md`), analizci ve ekran kartındaki bütçe
      satırının biçimi birlikte değişir. Hero rakam 1 kalır: halkanın ortasındaki tutar `TypeTitle`.
      Kaydırılan kartın zemini de burada seçilir: `SurfaceHero` koyu temada grafiği taşımıyor
      (`Indicator` 2,92 < 3,0; `NegativeText` 4,18 ve `TextSecondary` 4,05 < 4,5), `SurfaceCard`'da
      hepsi geçiyor. Ya `SurfaceCard` ya token değişikliği (`GS`); kontrast tablosuna yeni çiftler.
      "Bakiye gir" sayfasındaki önizleme kartı da aynı zemin sorusu: koyu temada `PositiveText` /
      `SurfaceHero` 4,25 < 4,5.
- [x] **T10** — grafik primitiflerinin genişlemesi *(GS23; koruyan: I94, I95)* *(V3 yenilemesi, kaynak eski proje değil — bkz. `V3`; GK7: beşinci primitif yok)*.
      `AreaTrend`: gerçek noktalar düz çizgi ve alan, son gözlemden dönem sonu tahminine kesikli devam,
      "bugün" işareti, plan çizgisi (planın dönem sonu). `RingGauge`: dolulukla birlikte zaman işareti
      (tempo). İkisinin de `<summary>`'deki sorusu değişir; eklemeler isteğe bağlı olur, `AreaTrend`'in
      12 dönem kullanımı (`V8`) bozulmaz. Rol → token eşlemesine yeni roller. `V11`'in Aşama 4'ünde plan /
      gerçekleşen çubukları kalırsa `StackedBar` iki satır çizer.
      S68-6: çizgi noktaların arasında ve son noktadan sonra plandan çizilir — her gelir ve ödeme kendi gününde
      bakiyeyi değiştirir, kalan yaşam gideri dönem sonuna iner; gözlem yoksa çizgi baştan sona plandır.

## Faz V — Ekranlar

- [x] **V0** — kabuk ve altyapı: `ViewModelBase`, `INavigationService`, `IDialogService`,
      `Routes`, `AutomationIds`, `AppShell`, `MauiProgram`
- [x] **V1** — profil seçimi
- [x] **V2** — hatırlatıcı kartı *(sayfasız çocuk ViewModel)*
- [x] **V3** — ana sayfa (dashboard). **"Rota + Tempo" yenilemesi (2026-09-29 geri açıldı, 2026-09-30 kapandı: `V3a` + `V3b`).**
      *Kaynak eski proje değil:* bugünkü `DashboardPage` ve aşağıdaki konsept görüntüleri.
      İlk hâl (`GS20`: halka + hero rakam + gözlem kartı + gezinme satırları) tamamlanmıştı. Yeni yerleşim
      Claude Design'da üretilen D yönü: `docs/assets/konsept/ana-sayfa-rota-tempo.png` (dolu hâl, iki tema,
      yükleniyor) ve `ana-sayfa-rota-tempo-durumlar.png` (boş, bakiye hiç girilmemiş, kapanış ertelenmiş,
      hata, "Bakiye gir" sayfası) ve `ana-sayfa-rota-tempo-kapanis.png` (halka sayfasının gözlemsiz hâli;
      aynı görüntüdeki kapanış sayfası `V11`'in). Görüntüler **yalnız bu ekranın yerleşim konseptidir**:
      renkleri token değildir, grafiğindeki iniş çıkışlar bugün hiçbir veriye dayanmıyor (bkz. `H5`).
      - Kaydırılan kart, iki sayfa, iki nokta; açılışta her zaman 1. sayfa.
        1. Dönem sonu tahmini (hero rakam) + plana göre fark (`PlannedEndingBalance` hazır) + bakiye trendi (`ColumnTrend`, konsept `anasayfa-grafik-yerine-sutun.png`, `GS30`).
        2. Kalan yaşam gideri + tempo halkası (`RingGauge`) + harcanan / geçen süre + tek tempo cümlesi.
      - Bankadaki bakiye (son gözlem ve tarihi) + "Bakiye gir"; hatırlatıcı kartı; kalan ödemeler
        (adet · toplam, ilk satırlar, "Tümünü gör").
      - Başlıkta dönem aralığı ve gün sayacı. Gezinme satırları ve ayarlar ikonu kalkar; yan menü karşılıyor.
      - "Dönemi kapat" butonu yalnız kapanış ertelendiyse görünür ve `V11`'in özet sayfasını açar.
      - **"Bakiye gir" ayrı bir sayfa** (✕ ile kapanır): tutar, son giriş ve tarihi, gözlem tarihi
        (varsayılan bugün, "Değiştir"), kaydetmeden önce "bu girişle dönem sonu tahmini" önizlemesi ve
        küçük grafik. Sayfa `EK-V3`'e ikinci sayfa olarak yazılır (GK9 kartı dosya adından eşliyor).
        Kapanış ertelenmişken sayfa önce kapanışı ister (S68-4). Tarih ve önizleme `A28`'de.
      - Bakiye hiç girilmemişken hero'da planın dönem sonu, "plan değeri · henüz gözlem yok" etiketiyle.
        Bugün gözlem yoksa tahmin `null` dönüyor ve ekranda tire görünüyor (`PeriodProgressCalculator`).

      **Önce:** grafik verisi `H5 → I7a → I7b → A28 → A30` (S68), tempo `A29`, kural ve primitifler `T9 → T10`.
      **Bölünme** (Aşama 1, 2026-09-30; ~300 satırı ikiye aşıyor): **V3a** ana sayfa (`HeroPager` dahil, iki
      hero sayfası, bakiye kartı, kalan ödemeler) → **V3b** "Bakiye gir" sayfası (tarih, önizleme, küçük
      grafik, ertelenmiş kapanışta önce kapanış). İkisi `EK-V3`'ü paylaşır.
      - [x] **V3a** — ana sayfa *(2026-09-30; `S72`, `GS24`, `GS20` iptal; `HeroPager` bileşeni; koruyan: I93, I108–I110)*.
            Kapı C'de halkanın açı hatası (T10), tonlu hero zemini, grafik noktaları ve Android kaydırması düzeltildi.
      - [x] **V3b** — "Bakiye gir" sayfası *(2026-09-30; `S73`, `GS25`; `BalanceEntryPage`, önizleme kartı `BalancePreviewViewModel`, ortak grafik `BalancePathTrend`; koruyan: I111–I113)*.
            Tarih dönem başı ile bugün arası, canlı önizleme (son istek kazanır), eksi bakiye, kapanışı bekleyen dönemde önce kapanış.
      Bu adımlar `/tasima-adimi` ile yürür ama kaynak eski proje değil, bu satırdır (Faz T'deki gibi):
      Aşama 1 mevcut kodu okur, Aşama 3 yeni `S` / `GS` kaydı yazar. **Sonra** ekran, `/tasarim-adimi V3`:
      kart `EK-V3` yerinde yeniden yazılır (GK9 kart anahtarı harf eki alamıyor), `GS20` iptal olur, yerine yeni `GS`.

      Ekran adımının Aşama 4 kararları:
      - Kart bütçesi: kapanış ertelenmişken konseptte kaydırılan kart, kapanış kartı, bakiye kartı ve
        kalan ödemeler var; hatırlatıcı da görünürse 5 kart olur (sınır 4). Kapanış kartı bakiye kartıyla
        birleşir ya da hatırlatıcıyla aynı yeri paylaşır.
      - Kapanış kartının cümlesi ("Son bakiyeyi gir ve kapat") `V11` kararıyla uyuşmalı: kart özet
        sayfasını açar, bakiye girmek zorunlu değil.
      - Boş hâlde konsept ikon yerine kesikli bir grafik yer tutucusu çiziyor; bütün ekranlar aynı
        `StateBlock`'u kullanıyor (`T5`).
      - Hata metni ("Yerel kayıt açılamadı") her yükleme hatasına uymuyor; hata veritabanından değil
        hesaptan da gelebilir.
      - Kaydırma mekanizması: `CarouselView` dikey `ScrollView` içinde kaydırma çakışması riski taşıyor.
      - Başlıktaki bitiş tarihi `PeriodEnd`'in bir gün öncesi; dönem sonu döneme dahil değil.
      - Cümle bütçesi sınırda: "İlk bakiyeyle gidişat çizilir", "Harcama temposu ilk bakiye girişiyle
        hesaplanır" ve boş hâlin cümlesi 3/3. Tempo cümlesi sayı taşıdığı için `Bicim_`, sayılmaz.
- [x] **V4** — kurulum sihirbazı *(eskide 958 satır / 3 partial — 8 ContentView adımı + OnboardingPlanWriter ile daraltıldı)*.
      Kapı C'de V3 ana sayfasının **dolu** hâli de iki temada kontrol edildi ve onaylandı.
- [x] **V7** — kart kontrol — **V6 ve V10'dan ÖNCE.** Eskide `CommitmentsPage` ve
      `SimulationPage` code-behind'de `CardControlViewModel` örnekliyordu; gizli bağımlılık.
      Tek adım (`S61`, `EK-V7`): sıradaki ödeme ve kararı, kararın bedeli, sonraki ödemeler,
      varsayılan ödeme şekli, elle ekstre girişi. Sol menüde **geçici** giriş. *(Önce V7a/V7b
      diye bölünmüştü; V7a tek başına ekranın cevabını vermediği için Kapı C'de birleştirildi.)*
- [x] **V6** — finansal yapı *(eskide 1.344 satır / 6 partial — en büyük ViewModel)*.
      *(2026-10-03: `V6f` için geri açıldı — kullanıcı "Ekle"deki düz diyalog yerine eski dört gruplu tür
      seçicisini istedi, `S77`. 2026-10-04: `V6f2` ile yeniden kapandı.)*
      `S61`'den devralınanlar: gelecek kart harcaması girişi ve sol menüdeki geçici
      "Kart Kontrol" öğesinin kaldırılması (kart satırı `Routes.CardControl` + `cardId` açar).
      Aşama 1'de beş alt adıma bölündü (`S62`); her form adımı kendi türünü "Ekle" seçicisine
      ve satır diyaloğuna "Düzenle" olarak ekler.
  - [x] **V6a** — liste: `FinancialStructurePage`, dört grup (Gelirler, Kartlar, Krediler,
        Ödemeler), satır diyaloğu (kart → kart kontrol, sil); geçici "Kart Kontrol" öğesi
        kalkar (`S61`-8); `Routes.Commitments` → `Routes.FinancialStructure`; taşma yerinde
        açılır (`GS21`)
  - [x] **V6b** — kart formu (`S63`, `EK-V6b`). Aşama 1'de ~430 satır çıktığı için ikiye bölündü;
        Kapı A ve B ortak, Kapı C her alt adımda ayrı.
    - [x] **V6b1** — `CardFormPage`: kartın tanımı (ad, banka, limit, kesim / son ödeme günü,
          güncel borç), ekle / düzenle; Finansal Yapı başlığında "Ekle" (tek seçenekli seçici),
          kart satırında "Düzenle"
    - [x] **V6b2** — aynı sayfaya gelecek kart harcamaları (`S61`-7): açıklama, aylık tutar ×
          taksit sayısı, ilk taksit tarihi; liste ve silme
    - [ ] **Takip** — taksitli harcamanın bütün taksitlerini tek seferde silme; bugün her ay tek
          tek siliniyor *(V6b2 sonrası kullanıcı isteği; `/duzeltme`, büyük ihtimalle tür K: taksitler
          birbirine bağlı saklanmıyor, `CardCharge`'ta grup kimliği yok; "Açıklama (i/N)" adına
          dayanmak eski veride tutmaz)*
  - [x] **V6c** — kredi formu (`S64`, `EK-V6c`). Aşama 1'de ~800 satır çıktığı için üçe bölündü;
        Kapı A ve B ortak, Kapı C her alt adımda ayrı.
    - [x] **V6c1** — `LoanFormPage`: kredinin tanımı (ad, banka, aylık taksit, kalan taksit, sonraki
          taksit tarihi, kredi türü), ekle / düzenle; Finansal Yapı "Ekle"de "Kredi", kredi satırında "Düzenle"
    - [x] **V6c2** — faiz ve bugün kapatma bedeli: kalan anapara + bankanın kapatma tutarı, canlı
          "Bugün kapatırsan" kartı
    - [x] **V6c3** — planlı erken ödemeler: liste, giriş, silme; kredi ile tek kayıt (Application eki).
          Simülatörden uygulananlar da burada görünür ve silinir (`S64`-13). *(Aşama 1 bulgusu, adım dışı:
          `INSERT OR REPLACE` + `ON DELETE CASCADE` dönem kapanışında kredilerin erken ödemelerini ve ödeme
          planlarının taksitlerini, gelir kaydında tutar geçmişini siliyor — `/duzeltme` işi. Kredi formunun
          kaydı tek işleme geçtiği için bu yoldan artık silinmiyor. **Düzeltildi** (tür B, `I73`–`I77`):
          `INSERT OR REPLACE` yasaklandı, kapanış kartları ve taksitleri de yazıyor.)*
  - [x] **V6d** — gelir formu: düzenli gelir, tutar değişikliği, tek seferlik gelir. Aşama 1'de ~800 satır
        çıktığı için üçe bölündü; Kapı A ve B ortak, Kapı C her alt adımda ayrı.
    - [x] **V6d1** — `IncomeFormPage`: düzenli gelirin tanımı (ad, ödeme günü; yeni gelirde aylık net tutar),
          ekle / düzenle; Finansal Yapı "Ekle"de "Düzenli gelir", gelir satırında "Düzenle"; gelir ve
          tutarı tek işlemde kaydedilir
    - [x] **V6d2** — aynı sayfaya tutar değişiklikleri: yürürlükteki ve ileri tarihli tutarlar
          (simülatörden gelenler dahil), yeni tutar girişi, planlı değişikliği silme
    - [x] **V6d3** — `AdHocIncomeFormPage`: tek seferlik gelir (açıklama, tutar, tarih), ekle / düzenle;
          "Ekle"de "Tek seferlik gelir"
  - [x] **V6e** — ödeme formu: taksitli ödeme planı + planlı büyük harcama (`PaymentPlanFormPage`, `PlannedExpenseFormPage`, `S65`, `EK-V6e`)
  - [x] **V6f** — kayıt türü seçici (`S77`, `GS29`, `EK-V6f`; eski projede `EntryTypePickerView` + `RecordEntryPicker`).
        "Ekle" düz diyalog yerine ayrı sayfa açar: Finansal Yapı listesinin dört grubu (Gelir · Kart · Kredi ·
        Ödeme) aynı anda, her biri renkli başlık ve ikonlu karolarla (konsept `ekleme-ekranı-acik` / `-koyu`).
        Karo, seçiciyle yer değiştiren formu açar. Grup iki ekranın bileşenidir (`EntryTypeTiles`); simülatör
        `V10c`'de geçer. Aşama 1'de ~450 satır çıktığı için ikiye bölündü; Kapı A ve B ortak, Kapı C her alt adımda ayrı.
    - [x] **V6f1** — seçici sayfası + bileşen + doğrudan formu olan 8 seçenek (Düzenli gelir, Tek seferlik
          gelir, Kredi kartı, Bankadaki kredi, Nakit ödeme, Düzenli ödeme, Taksitli borç, Ödeme planı);
          Finansal Yapı "Ekle" seçiciyi açar *(2026-10-03; `S77`, `GS29`; `RecordEntryPickerPage`,
          `EntryTypeTiles`; koruyan: I131–I133. Kapı C'de iki kez döndü: grup adları ve şerit düzeni → konseptin ızgarası)*
    - [x] **V6f2** — üst kaydı gereken 3 seçenek (Kartla harcama → kart, Krediye erken ödeme → kredi,
          Gelir değişikliği → gelir): kayıt yoksa önce ekleme önerisi, tekse doğrudan form, çoksa aynı
          sayfada ikinci seviye liste. KART ve KREDİ ikinci karolarıyla yan yana düzenden kendi satırlarına geçer
          *(2026-10-04; `S77` V6f2 notları d–h; onay diyaloğu, yerinde "Hangi kart?", form olduğu gibi, adaylar
          dokununca okunur; koruyan: I134–I137. Kapı C ilk turda onaylandı)*
- [x] **V5** — ilk düzen seçimi: taşınmadan elendi (`S18`, `S62`, `S47`; yapay tahsis ve harcama kaydırma yerine doğal dönemsellik; sayfası ve kodu yoktur, bütçe 0)
- [x] **V8** — 12 dönem (`S74`, `GS26`, `EK-V8`). Aşama 1'de ~400 satır (+ ~100 satır Application eki) çıktığı için
      ikiye bölündü; Kapı A ve B ortak, Kapı C her alt adımda ayrı. Zincir ana sayfanın dönem sonundan başlar,
      liste açık dönemden sonraki 12 dönemdir; hedef tutar taşınmaz (`V10`'a not).
  - [x] **V8a** — `FuturePeriodsPage` *(2026-10-03; `S74`, `GS26`; konsept Claude Design "Planör · 12 Dönem"; koruyan: I114–I116)*:
        yeni okuma portu `IFutureProjectionService`, hero (en düşük dönem sonu) + `AreaTrend` (sıfır çizgisi, dolgu sıfıra),
        3 × 4 karo ızgarası, "12 dönem sonra" ve "12 dönemde faiz" satırları. Karoya dokunma `V9`'da.
  - [x] **V8b** — aynı sayfaya erken kapama önerisi kartı *(2026-10-03; `S74`-7, `GS26`-6; koruyan: I117, I118)*:
        `IFutureProjectionService.GetPayoffAdviceAsync` (`LoanPayoffAdvisor` aynı zincirde, arka planda), `ListCard`
        kredi başına bir satır (4 sınırı yok), satır krediyi açar. En kârlı gün ve "Simülatörde dene" `V10`'da.
- [x] **V9** — dönem ayrıntısı *(2026-10-03; `S75`, `GS27`, `EK-V9`; koruyan: I119–I121)*: `PeriodDetailPage`, 12 Dönem
      karosundan dönemin ilk günüyle açılır, dönem aynı zincirden seçilir (yeni port yok). Dönem sonu, dönem başına göre,
      akış (dönem başı, gelir, ödemeler, yaşam gideri, KMH faizi), ödemeler (4 + yerinde açılır, kart satırı Kart Kontrol'e),
      kart faizi. Ana sayfanın "Tümünü Gör"ü yerinde açılmaya döndü. Ekran kayıtları `Composition/ScreenRegistrations.cs`'e
      ayrıldı (kural 01 testli). Simülatörün dönem kıyası `V10`'da.
- [x] **V10** — simülatör *(eskide 1.034 satır / 4 partial; S74-5, S75-7, 8, S76, GS28, EK-V10. V10a–V10f tamamlandı)*.
      `S76`, `GS28`, `EK-V10`. Aşama 1'de ~1.500 satır çıktığı için altıya bölündü (2026-10-03); Kapı A ve B ortak,
      Kapı C her alt adımda ayrı. Yerleşim 12 Dönem'den türetilir (konsept görüntüsü yok).
  - [x] **V10a** — koşul formu iskeleti + nakit ödeme; simülatör sayfasında deneme listesi (aç/kapa, düzenle, sil);
        tek çalışma listesi (taslak tablosu, port yeniden şekillenir); `A23b` sunum kodu silinir
        *(2026-10-03; `S76`, `GS28`; `SimulatorPage`, `SimulationConditionPage`; koruyan: I122–I124)*. Tek istekli
        `SimulateAsync` / `ApplySimulationAsync` aşırı yüklemeleri (`S76`-4) dokunulacakları `V10b` / `V10f`'ye kaldı.
  - [x] **V10b** — sonuç; Aşama 1'de ~500 satır çıktığı için ikiye bölündü (2026-10-03); Kapı A ve B `V10` ile ortak
    - [x] **V10b1** — "en düşükte ne olur?": zincir 12 Dönem'le ortak + açık döneme düşen etki (Application / Domain eki),
          sonuç kartı (hero, aynı dönemde şu anki gidişata göre fark, iki serili `AreaTrend`, iki uç tarih), canlı hesap,
          boş / hata durumu; `TargetAmountCalculator`, `FriendlySummary` ve tek istekli `SimulateAsync` silinir
          *(2026-10-03; `S76` V10b notları, `GS28`; `SimulationResultViewModel`, `ISimulationResultService`,
          `ProjectionChainBuilder`; koruyan: I125–I130)*. `SimulatorViewModel` 199 / 200 satırda: `V10c` liste
          yönetimini çocuk ViewModel'e ayırmak zorunda
    - [x] **V10b2** — "bir yıl sonra ne olur, faiz ne?": 12 dönem sonra ve faiz satırları (12 Dönem'in satır diliyle — `MetricRow` + `Caption` alt ızgarası), 12'li karo ızgarası *(2026-10-04; `S76` V10b2 notları, `GS28`; `SimulatorPage`, `SimulationResultViewModel`, `FuturePeriodTile`; koruyan: I138, I139)*
  - [x] **V10c** — harcama türleri: kartla harcama, düzenli ödeme; kart ödeme şekli. *(`S77`: simülatörün
        "Ekle"si düz diyalogdan `V6f`'in `EntryTypeTiles` bileşenine geçti (`SimulationConditionPickerPage`);
        `EK-V10`'daki "Plan türü grup seçici → Çıkar" satırı bu adımda geri alındı.)* *(2026-10-04, `S77` V10 notları i:
        **kartı seçici çözer** — kartla harcama ve kart ödeme şekli için aday yoksa yalnız uyarı (`I140`), tek
        kartta doğrudan form (`I141`), birden fazlasında yerinde "Hangi kart?". Deneme formu kartın kimliğiyle açılır,
        adını salt okunur gösterir, düzenlemede kart değişmez. Kartla harcamada taksit boşsa tek çekim, sayı girilirse
        taksitli (`I142`); düzenli ödemede ödeme sayısı 1–120; kart ödeme şeklinde tutar yok, ödeme şekli ve kapsam seçilir.
        Ortak aday çözümleyici `RecordCandidateResolver` çıkarıldı; koruyan: I140–I142)*
  - [x] **V10d** — borç türleri: kredi çekme, taksitli nakit borç, krediye erken ödeme; kredi satırları
        *(`S77` V10 notları i: krediye erken ödemenin **kredisini seçici çözer** — `V10c`'nin ortak yardımcısıyla,
        "Hangi kredi?"; aday taksiti kalmış aktif kredilerdir. Kayıtlı kredi yoksa uyarı verir `I143`, tek kredide
        doğrudan formu açar, birden fazlasında "Hangi kredi?" aday seçim karoları sunar `I144`. Formda kredi çekmede
        toplam geri ödeme >= anapara ve ilk ödeme >= işlem tarihi doğrulanır; erken ödemede tamamen kapat tutarsız,
        ara ödemede anaparadan düşecek tutar zorunludur `I145`. Simülatör sonuç kartında açık erken ödeme denemesinde
        faiz kazancı, kredi çekme denemesinde kredi maliyeti tekil metrik satırı olarak sunulur `I146`; koruyan: I143–I146)*
  - [x] **V10e** — gelir türleri: tek seferlik gelir, gelir değişikliği *(2026-10-04; `S77` V10 notları i: gelir
        değişikliğinin **düzenli gelirini seçici çözer**, "Hangi gelir?"; eski simülatörde bu soru yoktu, tek maaş
        geçmişine yazıyordu. Kayıtlı düzenli gelir yoksa uyarı `I147`, tek gelirde doğrudan form `I148`, formda hedef
        gelir salt okunur ve düzenlemede değişmez `I149`; koruyan: I147–I149)*
  - [x] **V10f** — "Planıma ekle": tek işlem (Application + Infrastructure), onay, uygulananlar listeden düşer
        *(2026-10-04; `S76`-12, `EK-V10` S5: atomik yazma portu `ISimulationBatchWriter` ve `SqliteSimulationBatchWriter` transaction adaptörü;
        tekil `ApplySimulationAsync` aşırı yüklemeleri kaldırıldı; kullanıcı onay diyaloğu, uygulananların listeden düşmesi,
        `CanApply` görünürlüğü; koruyan: I150–I154)*
- [x] **V11** — dönem kapanışı: özet sayfası *(kullanıcı kararları, 2026-09-29; konsept
      `docs/assets/konsept/ana-sayfa-rota-tempo-kapanis.png`; kaynak eski proje değil, eski
      `PeriodReviewPage` okunmaz; S78, GS31, EK-V11)*. Çapa günü uygulama açılınca kendiliğinden
      açılır: dönem sonu, plana göre fark, farkın kaynağı ve tek "Dönemi kapat". ✕ ile kapatmak ertelemek
      demek; ana sayfada "Dönemi kapat" kalır ve bu sayfayı açar. Kapanış bakiyesinin yanındaki "Değiştir"
      bakiyeyi kapanışın içinde değiştirir, gözlem yazmaz ve "Bakiye gir"i açmaz (S68-5): "planlandığı gibi"
      denirse planın dönem sonu, değilse kullanıcının söylediği farklardan çıkan bakiye. Kapanmamış biten
      döneme bakiye yazılmaz (S68-4). Birden fazla dönem
      geçtiyse sırayla. Eski çok adımlı sihirbaz taşınmaz; ödemeler ve yaşam harcaması dönemin
      kayıtlarından gelir.
      - Farkın kaynağı: yaşam gideri (planlanan / harcanan), ödemeler (kaçı ödendi), bakiye eksiye
        düştüyse KMH faizi. **Gelir satırı yok** (kullanıcı kararı): uygulama gelirin gerçekte ne kadar
        yattığını bilmiyor, planlandığı gün planlanan tutarla yattığını varsayıyor
        (`PeriodProgressCalculator.IncomeReceivedBy`, `PeriodActualBuilder`). **Bilinen sınır:** eksik yatan
        gelir yaşam giderinde fazla harcama gibi görünür. Aşama 3'te `S` kaydı olarak yazılır.
      - Aşama 4: konseptteki Plan / Gerçekleşen çubukları bilgi eklemiyor; fark bu ölçekte görünmüyor ve
        aynı bilgiyi "Farkın kaynağı" kartı veriyor. Kalırsa `StackedBar` iki satır çizmeli (`T10`). Hero
        kartın zemini `T9`'daki kontrast kararına bağlı. Ertelenen kapanış her açılışta yeniden mi açılır,
        yoksa yalnız ana sayfadaki buton mu kalır?
- [x] **V12** — geçmiş + geçmiş ayrıntısı *(2026-10-04; S68-6, 7, S29, GS32, EK-V12; `HistoryPage.xaml`, `HistoryDetailPage.xaml`, `HistoryViewModel`, `HistoryDetailViewModel`, `HistoryQueryService`; son dönemler net değişim özeti, bakiye çizgisi trend grafiği, farkın kaynağı, +N açılır ödemeler listesi; koruyan: HistoryViewModelTests, HistoryDetailViewModelTests)*
- [x] **V13** — ayarlar + düzen değişikliği *(2026-10-04; S4, S18, S19, S79, GS33, EK-V13; `SettingsPage.xaml`, `SettingsViewModel`, `SettingsFormParser`, `SettingsStrings`; dönem başlangıç günü, serbest yaşam gideri bütçesi, faiz oranları, bildirim modu çipleri, yerel yedekleme tetikleme ve Planör sürüm güvencesi; koruyan: SettingsViewModelTests)*

## Faz K — Kalkanlar

- [x] **K1** — E2E: `AutomationIds` sabitinden **üretilen** Maestro akışı *(2026-10-04; MaestroFlowBuilder, RegressionFlowDefinition, MaestroFlowGenerator, full_regression_flow.yaml; C# sembollerinden tam tip-güvenli E2E akışı; koruyan: MaestroFlowGeneratorTests)*
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
| H | 5 | 5 *(H5 V3 yenilemesi için açıldı ve kapandı)* |
| A | 28 | 28 *(A22 ve A25 taşınmıyor; A28, A29, A30 V3 yenilemesi için açıldı)* |
| I | 5 | 6 *(I5 taşınmıyor; I4 üç alt adımda tamamlandı; I6'da Sentry taşınmadı — S60; I7 V3 yenilemesi için açıldı)* |
| T | 6 | 10 *(T7, T8 V7 Kapı C'de açıldı; T9, T10 V3 yenilemesi için açıldı)* |
| V | 12 | 14 *(V3 "Rota + Tempo" için geri açıldı, V3a ve V3b olarak bölündü ve kapandı; V6 on alt adımda tamamdı, `V6f` tür seçici için geri açıldı ve iki alt adımda (V6f1, V6f2) yeniden kapandı; V8 iki alt adımda: V8a, V8b tamam; sayı V8b'de kutulardan yeniden sayıldı: V0–V8; V9 tamam; V10 yedi alt adıma bölündü (V10b ikiye: V10b1, V10b2), V10a–V10f tamam; V11 dönem kapanışı özeti tamam; V12 geçmiş ve geçmiş ayrıntısı tamam)* |
| K | 1 | 4 |
| G | 0 | 1 |
