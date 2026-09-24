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

- [x] **F1** — Mimari test kalkanı. `Mizan.Architecture.Tests` içinde K1–K8'i denetleyen testler.
      *Bu adım kasıtlı olarak birincidir: kuralları zorlayan mekanizma, kuralların koruyacağı
      koddan önce ayakta olmalı.*
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
- [x] **A11** — plan revizyonu: `HistoricalPlanRevisionService`
- [x] **A12** — araç mutabakatı: `FinancialInstrumentReconciliationService`
- [x] **A13** — kredi kapatma *(~460 satır ve iki iş yeteneği olduğu için iki alt adıma bölündü)*
  - [x] **A13a** — kapatma bedeli ve kaydetme kapısı: `LoanPayoffService` *(S28 ile ekran metni ve kültür elendi)*
  - [x] **A13b** — erken kapama önerisi: `LoanPayoffAdvisor` *(`SimulationCalculator` yerine yalnız `ScenarioPlanBuilder`)*
- [x] **A14** — tarihçe sorgusu: `HistoryQueryService` *(S29 ile özet bakiye yerine net değişim toplar)*
- [ ] **A15** — mevcut dönem motoru: `PeriodProgressService`
      *(düğüm T6: dondurma kuralı elle kopyalanmayacak, ortak yardımcı kullanılacak)*
- [ ] **A16** — dönem mutabakatı: `PeriodReviewService`
- [ ] **A17** — **plan okuma ve plan yazma ayrılır**: `IPlanReader` + `IPlanChangeRecorder`
      — *düğüm T5. Eskide tek `FinancialPlanQueryService` vardı: 11 bağımlılık, 21 çağrı
      noktası, adı "query" olmasına rağmen plan revizyonu yazıyordu. Bu adım planın en
      kritik kararıdır; Aşama 2'de ayrıntılı konuşulacak.*
- [ ] **A18** — kart yükümlülüğü: `CreditCardObligationService`
- [ ] **A19** — simülasyon iş akışı: `ISimulationWorkflowService`
- [ ] **A20** — yükümlülük yönetimi: `IObligationManagementService`
- [ ] **A21** — dönem iş akışı: `IPeriodWorkflowService`
- [ ] **A22** — cephe `MizanService`: **TAŞINMAZ.** Düğüm T7: 515 satırlık tek kapı,
      16 ViewModel'in 11'i yalnız buna bağlıydı. ViewModel'ler dar portlara bağlanacak.
- [ ] **A23** — sunum yardımcıları: `CashFlowPeriodDetailPresenter`, `SimulatorInsightService`
- [ ] **A24** — kataloglar: senaryo ve kayıt girişi katalogları
- [x] **A25** — ekstre içe aktarma portları: **TAŞINMADI (ELENDİ)** — S21: Otomatik ekstre içe aktarma özelliği bütünüyle elendi
- [ ] **A26** — yedekleme: `BackupService`, `IProfileBackupArchive`
- [ ] **A27** — telemetri portu: `ITelemetryService` *(`Abstractions/` altında — düğüm T9)*

## Faz I — Infrastructure

- [ ] **I1** — **temiz şema v1**: 29 tablo, `PRAGMA user_version`, gerçek foreign key'ler.
      *Eski v17'nin 16 yalancı kolonu, 12 ölü kolonu ve hiçbir migration'ı taşınmaz.*
- [ ] **I2** — depo implementasyonları *(dar port başına ayrı sınıf — tek 2.400 satırlık
      `SqliteMizanStore` değil)*
- [ ] **I3** — profil deposu ve profil başına veritabanı
- [ ] **I4** — yedekleme arşivi *(düğüm T8: `IProfileFileLayout` portu üzerinden)*
- [x] **I5** — PDF ekstre içe aktarma: **TAŞINMADI (ELENDİ)** — S21: PdfPig ve banka ayrıştırıcıları elendi
- [ ] **I6** — telemetri adaptörü + PII maskesi
      *(eskinin açığı: maske yalnız `event.Message`'ı kapsıyordu; exception metni, breadcrumb,
      extra ve ekran görüntüsü açıkta kalıyordu. `AttachScreenshot` varsayılan olarak kapalı.)*

## Faz V — Ekranlar

- [ ] **V0** — kabuk ve altyapı: `ViewModelBase`, `INavigationService`, `IDialogService`,
      `Routes`, `AutomationIds`, `AppShell`, `MauiProgram`
- [ ] **V1** — profil seçimi
- [ ] **V2** — hatırlatıcı kartı *(sayfasız çocuk ViewModel)*
- [ ] **V3** — ana sayfa (dashboard)
- [ ] **V4** — kurulum sihirbazı *(eskide 958 satır / 3 partial — adım ViewModel'lerine bölünecek)*
- [ ] **V7** — kart kontrol — **V6 ve V10'dan ÖNCE.** Eskide `CommitmentsPage` ve
      `SimulationPage` code-behind'de `CardControlViewModel` örnekliyordu; gizli bağımlılık.
- [ ] **V6** — finansal yapı *(eskide 1.344 satır / 6 partial — en büyük ViewModel)*
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
      sonucu koşulsuz "başarılı" sayıyordu — bu tekrarlanmayacak)*
- [ ] **K4** — Sürüm hattı: sürüm notları `CHANGELOG.md`'den okunur, elle `echo` edilmez

## Faz G — Geçiş

- [ ] **G1** — Eski uygulamanın (`com.coinflow.mobile`, şema v17) yedek arşivini okuyan
      tek seferlik içe aktarıcı. Yeni app id `com.mizan.app` olduğu için cihazda güncelleme
      değil yan yana kurulum olur; veri bu yolla taşınır.

---

## İlerleme

| Faz | Tamamlanan | Toplam |
|---|---|---|
| F | 4 | 4 |
| D | 24 | 24 |
| H | 4 | 4 |
| A | 12 | 25 *(A22 ve A25 taşınmıyor)* |
| I | 0 | 5 *(I5 taşınmıyor)* |
| V | 0 | 14 |
| K | 0 | 4 |
| G | 0 | 1 |
