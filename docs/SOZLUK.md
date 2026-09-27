# Sözlük — Ortak Dil

Kod İngilizce, konuşma Türkçe. Bu tablo ikisini birbirine bağlar.

**Kural:** Kodda kullandığın her iş kavramı bu tabloda olmalı. Yeni bir kavram
adlandırıyorsan **önce buraya ekle**, sonra kodda kullan. Tabloda olmayan bir isim,
üzerinde anlaşılmamış bir isimdir.

Bir ismin yalan söylememesi de buradan denetlenir: eski şemada `loans.StartDate` alanı
aslında bir sonraki ödeme tarihini tutuyordu. Böyle bir şey bu tabloda görünür hâle gelir.

## Temel kavramlar

| Türkçe | Kod | Tanım |
|---|---|---|
| dönem | `CashFlowPeriod` | İki dönem çapası arasındaki yarı açık aralık `[başlangıç, sonraki)` |
| dönem çapası | `PeriodAnchor` | Dönemin ne zaman döndüğünü belirleyen kural. Gelirden **ayrı** bir kavramdır. |
| dönem başlangıcı | `PeriodStart` | Çapanın düştüğü gün, döneme **dahil** |
| dönem sonu | `PeriodEnd` | Sonraki çapa günü, döneme **dahil değil** |
| açılış bakiyesi | `OpeningBalance` | Dönemin ilk günündeki nakit |
| kapanış bakiyesi | `EndingBalance` | Dönemin son anındaki nakit; sonraki dönemin açılışına eşittir |
| projeksiyon | `Projection` | İleriye dönük dönem dizisi hesabı |
| yükümlülük | `Obligation` | Bu dönemde ödenmesi gereken her kalem |
| zorunlu çıkış | `MandatoryOutflow` | Yükümlülüklerin toplamı |
| yaşam gideri | `PeriodVariableExpenseAllowance` | **Dönem** için ayrılan serbest harcama havuzu (ay için değil) |
| dönem planı | `PeriodPlanSnapshot` | Dönem başında **dondurulan** taahhüt |
| dönem planı revizyonu | `PeriodPlanRevision` | Dönem içinde planlama kararlarında yapılan değişiklikleri append-only kaydeden sürüm |
| plan ödeme satırı | `PeriodPlanPaymentLine` | Dondurulan dönem planına ait tekil bir ödeme taahhüdü kalemi |
| plan ödeme kaynağı türü | `PlanPaymentSourceType` | Plan ödeme satırının kaynaklandığı borç veya harcama enstrümanı türü |
| plan gelir satırı | `PeriodPlanIncomeLine` | Dondurulan dönem planına ait, yatacağı günle birlikte tekil bir gelir kalemi; ödeme satırı değildir (S31) |
| finansal durum anlık görüntüsü | `FinancialSnapshot` | Kullanıcının belirli bir tarihteki açılış bakiyesi, çapa kuralı ve mutabakat tarihini donduran üst başlık kaydı |
| finansal durum kaynağı | `FinancialSnapshotSource` | Finansal durum kaydının oluşturulma yaşam döngüsü (kurulum, aylık kapanış, toparlama) |
| finansal tarihçe verisi | `FinancialHistoryData` | Dondurulmuş durumlar, planlar, revizyonlar ve gerçekleşmeleri içeren salt okuma tarihçe paketi (Query DTO) |
| güncel finansal durum | `FinancialHistoryData.FindLatestCurrentSnapshot` | Güncel işaretli durumlardan en yeni tarihli olanı; aynı tarihte birden fazlaysa en son oluşturulanı |
| açık dönem planı | `FinancialHistoryData.FindOpenPlan` | Güncel finansal duruma bağlı, kapanışı henüz kaydedilmemiş en son dondurulan plan. Mutabakat tarihi geçmiş ama kapatılmamış dönem hâlâ açıktır |
| açık dönem defteri | `OpenPeriodLedger` | Açık dönemin dondurulan planı, dönem içi revizyonları, gözlem defteri ve vadesi o döneme düşen hatırlatıcı cevapları bir arada |
| açık dönem defteri okuyucusu | `OpenPeriodLedgerReader` | Açık dönem defterini tarihçe, gözlem ve hatırlatıcı portlarından okuyup birleştiren, hiçbir şey yazmayan servis |
| dönem tarihçesi deposu | `IPeriodHistoryRepository` | Dondurulmuş planları, revizyonları, gerçekleşmeleri ve settlement taahhütlerini kalıcılaştıran port |
| dönem gözlem defteri deposu | `IPeriodObservationRepository` | Açık dönemin ara bakiye ve borç ödeme işaretlerini saklayan veri deposu portu |
| gerçekleşme | `PeriodActual` | Dönem kapanışında ölçülen fiilî durum |
| fiilî ödeme | `ActualPayment` | Dondurulan plan ödeme satırının dönem sonundaki gerçekleşme sonucu |
| plansız akış | `ActualFlow` | Dönem planında yer almayan arızi gelir veya gider satırı |
| fiilî yaşam gideri kırılımı | `ActualLivingBreakdown` | Serbest yaşam havuzundan fiilen yapılan harcamaların kategori dökümü |
| fiilî ödeme durumu | `ActualPaymentStatus` | Ödemenin yapılıp yapılmadığı veya farklı tutarla gerçekleştiği durumu |
| plansız akış türü | `ActualFlowType` | Plansız nakit hareketinin gelir mi ödeme mi olduğunu belirten yön |
| gözlem | `PeriodObservation` | Dönem içinde kullanıcının girdiği anlık bakiye |
| gözlem ödemesi | `PeriodObservationPayment` | Dondurulan plan ödeme satırının dönem içi gözlem defterindeki ara gerçekleşme kaydı |
| güncel ödeme satırları | `OpenPeriodLedger.CurrentPaymentLines` | Dönem içinde "planım şu an ne" sorusunun satırları: son revizyonunkiler, revizyon yoksa dondurulan planınkiler (I24) |
| ödeme satırı durumu | `PeriodPaymentLineClassification` | Açık dönemin güncel ödeme satırlarının bir güne göre durumu: gözlenen bakiyeye yansımış ödemeler, gözlemden sonra yapılanlar, kalan ve ertelenen satırlar |
| ödeme satırı sınıflandırıcısı | `PeriodPaymentLineClassifier` | Bir ödemenin yapılıp yapılmadığını açık işaret → hatırlatıcı cevabı → vade önceliğiyle bulan, işareti ve cevabı kaynak + vadeyle eşleyen bağımlılıksız yardımcı (I30, S33) |
| son revizyon | `OpenPeriodLedger.LatestRevision` | Dönem içinde "planım şu an ne" sorusunun cevabı olan revizyon; yoksa cevap dondurulan planın kendisi (I24) |
| güncel gelir satırları | `OpenPeriodLedger.CurrentIncomeLines` | Son revizyonun, yoksa dondurulan planın gelir satırları, yatacakları günlerle (S31) |
| gidişat | `PeriodProgress` | Açık dönemde planın dediği ile kullanıcının girdiği bakiyeden çıkan tahmin yan yana: harcanan ve kalan yaşam havuzu, KMH faizi, dönem sonu (I31) |
| gidişat hesaplayıcısı | `PeriodProgressCalculator` | Açık dönem defterinden gidişatı üreten bağımlılıksız yardımcı; KMH'yı `DeficitFinancingRules`'tan, satır durumunu `PeriodPaymentLineClassifier`'dan alır |
| gidişat servisi | `PeriodProgressService` | Defteri, ayarları ve kartları dar portlardan okuyup kartın güncel ödemesini bulan ve hesabı hesaplayıcıya bırakan ince kabuk |
| kart karşılaştırması | `PeriodCardComparison` | Bir kartın bu dönemdeki ödemesi: planlanan (kilitli) ile kartın bugünkü hâline göre güncel tutar |
| gözlenen yaşam harcaması | `ObservedLivingSpend` | Bakiye farkından, bakiyeye yansımış ödemeler ve yatmış gelir hesaba katılarak geri çözülen yaşam harcaması; fiş toplamı değildir (S20) |
| kalan yaşam havuzu | `RemainingVariableExpenseAllowance` | Dönemin yaşam havuzundan kalan; havuz aşıldıysa sıfır. Yasaklı eski adı `RemainingLivingBudget` (S16) |
| havuz aşımı | `LivingOverspend` | Gözlenen yaşam harcamasının havuzu aştığı tutar; dönem sonuna yansır |
| dönem sonu tahmini | `ProjectedEndingBalance` | Bu gidişatla dönemin kapanacağı bakiye, KMH faizi düşülmüş. Yasaklı eski adı `ProjectedEndingSavings` (S13) |
| dönem sonu sapması | `EndingDeviation` | Dönem sonu tahmini − planlanan kapanış |
| dönem kapanışı | `PeriodSettlement` | Planın gerçekleşmeyle mutabakatı |
| dönem kapanış taahhüdü | `PeriodSettlementCommit` | Kapanan dönemin gerçekleşmesini, güncellenen araçları ve yeni dönemin planını atomik olarak bağlayan taahhüt sözleşmesi |
| dönem kapanış doğrulayıcısı | `PeriodSettlementCommitValidator` | Dönem kapanış taahhüdünün referans, takvim ve devir bakiyesi sürekliliğini denetleyen saf doğrulayıcı |
| dönem mutabakatı uygunluğu | `PeriodSettlementAvailability` | Açık dönemin kapatılmaya hazır olup olmadığını sunan metinsiz durum sözleşmesi (S36) |
| ödeme gerçekleşme taslağı | `ActualPaymentDraft` | Dondurulan plan ödeme satırına ait kullanıcının girdiği fiilî durum ve tutar girdisi |
| plansız akış taslağı | `ActualFlowDraft` | Dönem içinde ortaya çıkan beklenmedik gelir veya gider akış girdisi |
| yaşam harcaması döküm taslağı | `LivingBreakdownDraft` | Serbest yaşam havuzundan harcanan tutarın kategori bazlı döküm kalemi |
| dönem mutabakatı taslağı | `PeriodSettlementDraft` | Dönem mutabakatında kullanıcının sunduğu tüm gerçekleşme bildirimlerini taşıyan girdi paketi |
| dönem mutabakatı bağlamı | `PeriodSettlementContext` | Dönem kapanış ekranı ve sihirbazı için plan, nihai revizyon, önerilen açılış bakiyesi ve karne bağlamı |
| dönem mutabakatı önizlemesi | `PeriodSettlementPreview` | Kapanış taslağının onay öncesi türetilen bakiyesini, teyitli bakiyesini, mutabakat düzeltmesini ve karnesini sunan önizleme |
| dönem mutabakatı sonucu | `PeriodSettlementResult` | Başarıyla tamamlanan dönem kapanışının yeni durum, yeni plan ve gerçekleşme karnesi paketi |
| dönem mutabakatı servisi | `PeriodSettlementService` | Dönem kapanış mutabakatını (uygunluk, bağlam, önizleme, kesinleştirme) koordine eden uygulama servisi |
| dönem fiilî durum kurucusu | `PeriodActualBuilder` | Kapanış taslağını doğrulayarak fiilî durumu (PeriodActual) ve türetilen bakiyeyi inşa eden bağımsız saf yardımcı |
| finansal plan | `FinancialPlan` | Kullanıcının tüm finansal varlık, yükümlülük, gelir ve ayarlarını tek çatı altında toplayan bütüncül sözleşme |
| plan doğrulayıcı | `FinancialPlanValidator` | Finansal planın ve alt bileşenlerinin iş kurallarına uygunluğunu denetleyen saf sınıf |
| plan gerçekleşme karşılaştırması | `PlanActualComparison` | Dondurulan/revize dönem planı ile fiilî gerçekleşme arasındaki bakiye farkını, kategorik satırları ve Türkçe özeti sunan sonuç sözleşmesi |
| plan gerçekleşme karşılaştırma satırı | `PlanActualComparisonLine` | Plan ile gerçekleşme arasındaki tek bir kategoriye ait bütçe ve fiilî tutar karşılaştırma kalemi |
| plan gerçekleşme kıyaslayıcısı | `PlanActualComparisonCalculator` | Dönem başında dondurulan plan ile dönem sonundaki fiilî gerçekleşmeyi 12 kategoride kuruşu kuruşuna karşılaştırıp özet üreten saf hesaplayıcı |
| dönemin nihai planı | `HistoryPeriod.Revision` | Kapanmış bir dönemde, kapanışa açılış günü dahil o güne kadar oluşturulan en son plan revizyonu; revizyon yoksa orijinal plan. Karne buna göre çıkar (I27) |
| tarihçe sorgu servisi | `HistoryQueryService` | Kapanmış dönemleri orijinal plan, nihai revizyon, gerçekleşme ve karneyle eşleştirip hiçbir şey yazmadan sunan salt okuma servisi |
| tarihçe dönemi | `HistoryPeriod` | Kapanmış tek bir dönemin orijinal planı, nihai revizyonu, gerçekleşmesi, kapanıştan çıkan finansal durumu ve karnesi |
| tarihçe özeti | `HistorySummary` | Son kapanan dönemlerin planlanan ve fiilî net değişim (kapanış − açılış) toplamları ile farkı; bakiye toplamı değildir (S29) |
| yükümlülük normalizasyonu | `ObligationValidation` | Kredi kartı ve vadeli borç enstrümanlarının saat sağlayıcısıyla eksik tarihlerini tamamlayan ve kart kurallarını doğrulayan saf yardımcı |

## Para ve yuvarlama

| Türkçe | Kod | Tanım |
|---|---|---|
| para yuvarlama | `MoneyRules.Round` | 2 basamak ve `MidpointRounding.AwayFromZero` ile kuruşa yuvarlama |
| kuruş korunumlu bölüştürme | `MoneyRules.Distribute` | Toplam tutarı eşit parçalara bölüp kuruş artığını son taksite ekleme |
| takvimli taksit tutarı | `ScheduledAmount` | Belirli bir vadeye bağlanmış takvimli taksit veya nakit çıkış tutarı |

## Gelir

Bu bölüm bilerek ayrıntılı: ürünün ekseni burada. Mizan yalnız maaşlı çalışanlara değil,
**her gelir düzenine** sahip kişilere hitap eder. Tek bir düzenli gelir varsayımı yoktur.

| Türkçe | Kod | Tanım |
|---|---|---|
| gelir akışı | `IncomeStream` | Bir gelir kaynağının bütünü |
| düzenli gelir | `RecurringIncome` | Tekrar eden gelir akışı. **Kendi ödeme gününü** taşır. |
| tek seferlik gelir | `AdHocIncome` | Tekrar etmeyen, tarihli gelir |
| gelir tutar geçmişi | `IncomeAmountHistory` | Bir akışın etkin tarihli tutar değişiklikleri. "En son kazanır" kuralı **akışın kendi içinde** işler, akışlar arasında değil. |
| gelir kalemi | `IncomeProjectionItem` | Bir dönemde beklenen tek bir gelir olayı |
| gelir kaynağı türü | `IncomeSourceType` | Gelir kaleminin periyodiklik türü (`Recurring` veya `AdHoc`) |
| dönem gelir özeti | `IncomeProjectionSummary` | Bir nakit akış dönemine ait tüm gelir kalemlerini ve tür bazlı toplamları özetleyen sonuç kaydı |
| gelir projeksiyon hesaplayıcısı | `IncomeProjectionCalculator` | Dönem için aktif düzenli gelir akışlarını ve tek seferlik gelirleri eşleştirip dönemsel gelir özetini deterministik olarak hesaplayan saf hesaplayıcı |
| gelir planı servisi | `IIncomePlanService` | Düzenli ve tek seferlik gelir akışlarının doğrulanmasını, kaydedilmesini, silinmesini ve açık dönem plan revizyonlarını yöneten kullanım senaryosu portu |

> Birden fazla düzenli gelir **toplanır**, birbirini ezmez. Bu, eski projede bir hataydı
> (`SAPMALAR.md` → S2) ve yeni projede bir invariant'tır.

## Dönemsellik ve Bakiye

| Türkçe | Kod | Tanım |
|---|---|---|
| doğal dönemsellik | `period.Contains(date)` | Vadesi dönemin `[Start, End)` aralığına düşen her nakit akışının doğrudan o döneme ait olması ilkesi (S18) |
| açık faizi | `DeficitFinancingInterest` | Negatif bakiyenin maliyeti (KMH) |
| açık faizi kuralı | `DeficitFinancingRules` | Faiz öncesi dönem sonu eksiyse açığa dönemlik oranı uygulayıp kuruşa yuvarlayan tek kaynak; hem projeksiyon hem açık dönemin gidişatı bunu çağırır (T6) |
| dönem projeksiyonu | `CashFlowPeriodProjection` | Tek bir dönemin gelir, zorunlu gider, yaşam havuzu, bakiye ve faiz projeksiyon sözleşmesi |
| finansal projeksiyon sonucu | `FinancialProjectionResult` | 12 dönemi, yükümlülük planını ve kümülatif faiz maliyetlerini içeren bütüncül projeksiyon sonucu |
| projeksiyon faiz özeti | `ProjectionInterestSummary` | Kredi kartı ve finansman açığı kümülatif faiz maliyetleri özeti |
| kart projeksiyon durumu | `CreditCardPaymentProjectionStatus` | Kredi kartının belirli bir ekstre döngüsündeki simüle edilen borç, ödeme ve carry faizini kart kimliğiyle sunan sözleşme |
| finansal projeksiyon hesaplayıcısı | `FinancialProjectionCalculator` | Kullanıcının tüm finansal planını (gelirler, krediler, kredi kartları, vadeli borçlar, büyük harcamalar) 12 nakit akış dönemi boyunca simüle eden ana motor |
| hedef tutar hesaplayıcısı | `TargetAmountCalculator` | Nakit akış projeksiyonu üzerinde kullanıcının hedef tutarına hangi dönemde ulaştığını deterministik olarak hesaplayan saf Domain motoru |
| projeksiyon başlangıç sınırı | `ProjectionBoundary` | Projeksiyonun başlangıç çapa tarihini, ilk projeksiyon dönemi başlangıcını ve devreden açılış bakiyesini taşıyan sınır sözleşmesi |
| projeksiyon başlangıç sınırı çözümleyicisi | `ProjectionBoundaryResolver` | Kapanan dönem gerçekleşmelerini ve mevcut durumu analiz ederek projeksiyonun başlangıç çapasını ve ilk açık dönemini belirleyen servis |
| ana ekran özeti | `DashboardSnapshot` | Aktif dönemi, çapa öncesi açık kalemleri, yaklaşan ilk 5 ödemeyi, 12 dönem sonu nakit dengesini ve en sıkışık dönemi sunan özet sözleşmesi |
| finansal projeksiyon servisi | `FinancialProjectionService` | Domain projeksiyon motorunu arayüzün ihtiyaç duyduğu Dashboard özeti ve dönem takvimine bağlayan ince uygulama servisi |

## Simülasyon

| Türkçe | Kod | Tanım |
|---|---|---|
| simülasyon senaryo türü | `SimulationScenarioType` | Varsayımsal nakit akış senaryolarının sınıflandırması (büyük harcama, taksitli borç, finansman kredisi, gelir artışı, ara ödeme vb.) |
| simülasyon isteği | `SimulationRequest` | Kullanıcının simülatör ekranında tanımladığı tekil bir varsayımsal senaryo koşulu sözleşmesi |
| simülasyon etki satırı | `SimulationImpactRow` | Belirli bir nakit akış döneminde baz durum (Baseline) ile senaryo durumu (Scenario) arasındaki parasal farkları sunan satır sözleşmesi |
| simülasyon risk özeti | `SimulationRiskSummary` | 12 dönemlik simülasyonda ortaya çıkan dip bakiye çukurlarını, ilk nakit açığı dönemini, açığın kapanma süresini ve toplam maliyeti özetleyen sözleşme |
| simülasyon sonucu | `SimulationResult` | Mevcut baz projeksiyon ile senaryo projeksiyonunun kümülatif faiz, tasarruf ve risk metriklerini içeren bütüncül karşılaştırma sonucu |
| kredi erken ödeme etkisi | `LoanPrepaymentImpact` | Erken kapama veya ara ödemelerin belirli bir krediye olan faiz kazancı, erken bitiş tarihi ve yeni taksit tutarı etkilerini sunan sözleşme |
| simülasyon istek doğrulayıcısı | `SimulationRequestValidator` | Simülasyon senaryo isteklerinin tutarlılığını, yasal ve matematiksel kısıtlarını ve çoklu gelir akışı çakışmalarını denetleyen saf sınıf |
| senaryo plan kurucusu | `ScenarioPlanBuilder` | Kullanıcının simülasyon senaryo isteklerini (kart harcaması, finansman, borç, gelir artışı vb.) mevcut finansal plana uygulayarak izole bir hipotetik plan inşa eden saf hesaplayıcı |
| simülasyon motoru | `SimulationCalculator` | Canlı baz plan ile varsayımsal senaryo koşullarını 12 dönem boyunca koşturup karşılaştıran, likidite farklarını, ek faiz maliyeti ve tasarruflarını hesaplayan ana motor |
| simülasyon taslağı | `SimulationDraft` | Kullanıcının simülatörde kurduğu ve adlandırarak sakladığı varsayımsal koşullar paketi; canlı plana girmez |
| simülasyon taslak koşulu | `SimulationDraftCondition` | Simülasyon taslağı içerisindeki tekil senaryo isteğini ve açık/kapalı (aktif/pasif) tercihini tutan kayıt |
| simülasyon taslağı deposu | `ISimulationDraftRepository` | Simülasyon taslaklarının ve bağlı koşullarının kalıcı olarak saklanmasını, listelenmesini ve silinmesini sağlayan veri erişim portu |
| senaryo grubu | `ScenarioGroup` | Simülasyon senaryo seçeneklerinin arayüzdeki işlevsel üst kümesi (Harcama, Borç, Gelir, Ayar) |
| senaryo giriş konumu | `ScenarioEntryHome` | Senaryo koşulunun simülatör dışında hangi ekrandan girilebileceğini belirleyen konum |
| senaryo seçeneği | `ScenarioOption` | Simülatörde veya Finansal Yapı'da sunulan tekil plan türü kartı sözleşmesi |
| senaryo kataloğu | `SimulationScenarioCatalog` | Simülatör ve Finansal Yapı ekranlarının paylaştığı tekil senaryo seçenekleri kataloğu ve motor türü çözümleyicisi |
| kayıt giriş grubu | `RecordEntryGroup` | Finansal Yapı ekranında yeni kayıt ekleme kategorileri |
| kayıt giriş formu | `RecordEntryForm` | Kayıt giriş seçeneğinin hangi form bileşeni üzerinden girileceğini belirten tür |
| kayıt giriş seçeneği | `RecordEntryOption` | Finansal Yapı ekranında kayıt türü kartı sözleşmesi |
| kayıt giriş kataloğu | `FinancialRecordEntryCatalog` | Finansal Yapı ekranında kayıt girişi seçenekleri kataloğu |

## Kredi

| Türkçe | Kod | Tanım |
|---|---|---|
| kredi | `Loan` | Banka kredisi |
| itfa | `Amortization` | Anapara/faiz ayrışması |
| kredi itfası | `LoanAmortization` | Çözümlenmiş annüite faiz, anapara ve taksit modeli |
| kredi analizi | `LoanAnalysis` | Kredi itfa analizi ve engel denetimi sonuç modeli |
| itfa faiz kaynağı | `LoanRateSource` | Faiz türetiminin kullanıcının girdiği anaparadan mı banka teklifinden mi çözüldüğü ayrımı |
| itfa engel durumu | `LoanAnalysisIssue` | İtfa çözümüne engel olan durumlar (pasif, eksik anapara, mantıksız faiz vb.) |
| erken kapama teklifi | `LoanPayoffQuote` | Belirli bir takvim gününde krediyi kapatmanın net nakit maliyeti ve faiz tasarrufu |
| kredi itfa hesaplayıcısı | `LoanAmortizationCalculator` | Annüite eşit taksitli kredilerde örtük faizi bisection yöntemiyle çözen ve tarihli kapama bedelini hesaplayan saf motor |
| taksit | `Installment` | Aylık ödeme kalemi |
| kalan borç | `RemainingDebt` | Bugün itibarıyla kalan anapara |
| erken ödeme | `Prepayment` | Plan dışı anapara ödemesi |
| kredi erken ödemesi | `LoanPrepayment` | Krediye planlanmış kısmi ara ödeme veya erken kapama taahhüdü |
| erken kapama | `Payoff` | Krediyi tümüyle kapatma |
| kredi ödeme türü | `LoanPaymentKind` | Kredi takvimindeki ödeme türü (taksit, erken kapama, ara ödeme) |
| takvimli kredi ödemesi | `LoanScheduledPayment` | Kredinin takvimdeki tekil bir nakit çıkış kalemi |
| kredi olay oynatımı | `LoanReplay` | Erken ödeme olaylarının kredi takvimi üstünde kronolojik oynatılmış sonucu |
| kredi ödeme takvimi oluşturucu | `LoanPaymentScheduleBuilder` | Erken ödeme olaylarını takvim üstünde oynatarak güncel takvim ve ara durumları üreten saf motor |
| kredi erken ödeme doğrulayıcısı | `LoanPrepaymentValidator` | Kredi erken ödeme girdilerinin iş kurallarına ve takvim durumuna uygunluğunu denetleyen saf doğrulayıcı |
| kredi kapatma servisi | `LoanPayoffService` | Kredinin bugünkü kapatma bedelini ve planlı erken ödeme tutarlarını çıkaran, krediyi kaydetmeden önce banka kapatma tutarının otoritesiyle doğrulayan uygulama servisi |
| kredi kapatma görünümü | `LoanPayoffOverview` | Bir kredinin analizini ve bugün kapatılırsa ödenecek bedelin dökümünü birlikte taşıyan model; engel metni taşımaz (S28) |
| planlı erken ödeme görünümü | `PlannedLoanPrepayment` | Planlanmış bir ara ödeme veya erken kapamanın o gün ödenecek, her seferinde yeniden hesaplanan tutarı |
| erken kapama danışmanı | `LoanPayoffAdvisor` | Ufuktaki her taksit gününü deneyip projeksiyonu kapamalı/kapamasız koşturarak krediyi hangi gün kapatmanın güvenli ve kazançlı olduğunu bulan uygulama servisi |
| erken kapama önerisi | `LoanPayoffAdvice` | Bir kredi için önerilen (en erken) ve en kârlı kapatma günü, kapatma bedeli, faiz tasarrufu ve net kazanç |
| erken kapama öneri durumu | `LoanPayoffAdviceStatus` | Önerinin sonucu: önerilir, güvenli ay yok, kazandırmaz, anapara gerekli, zaten kapanıyor |
| net kazanç | `NetGain` | Kapatmanın 12. dönem sonu bakiye farkı ile ufuk sonrasında ödenmeyecek taksitlerin toplamı; kapatma parasının açık faizi maliyeti içindedir |
| banka kapatma tutarı | `EarlyClosureAmount` | Bankanın belirli bir gün için bildirdiği erken kapama tutarı; kaydedildiğinde kalan anaparanın otoritesidir (I25) |

## Kredi kartı

| Türkçe | Kod | Tanım |
|---|---|---|
| kart | `CreditCard` | Kredi kartı |
| ekstre | `Statement` | Hesap kesim özeti |
| kesim tarihi | `StatementDate` | Ekstrenin kesildiği gün |
| son ödeme tarihi | `PaymentDueDate` | Ekstrenin vadesi |
| asgari ödeme | `MinimumPayment` | Yasal alt sınır |
| devreden bakiye | `CarriedBalance` | Ödenmeyip faize kalan tutar |
| dönem içi harcama | `UnbilledSpending` | Henüz ekstreye girmemiş harcama |
| kart harcaması / taksit | `CardCharge` | Karta yansıyacak bekleyen harcama veya taksitli işlem kalemi |
| kullanılabilir limit | `AvailableLimit` | Kartın kalan kullanılabilir kredi limiti (`Limit - KnownTotalDebt`) |
| yasal asgari oran | `CreditCardRules.ResolveMinimumPaymentRate` | BDDK mevzuatına göre kart limitine bağlı asgari ödeme oranı (%20 veya %40) |
| kart doğrulayıcı | `CreditCardValidator` | Kredi kartı sözleşmesi, ekstre ve plan tutarlılık denetleyicisi |
| özel ödeme planı | `CreditCardPaymentPlan` | Belirli bir vade için tanımlanmış istisnai ödeme tercihi |
| ekstre ödeme planı | `CurrentStatementPaymentPlan` | Kesilmiş mevcut ekstre için belirlenen anlık ödeme modu ve tutarı |
| ekstre ödeme tercihi | `CreditCardPaymentPreference` | Kart ödeme tercihlerinin etkin tarihli (effective-dated) tarihçe kaydı |
| ekstre ödeme tercihi çözümleyici | `CreditCardPaymentPreferenceResolver` | Kartın tarihsel ödeme tercihlerini etkin tarih mantığıyla çözümleyen, kronolojik sıralayan ve mükerrer kararları ayırt eden saf hesaplayıcı |
| akdi faiz / carry faizi | `CarryInterest` | Geçmiş ekstrelerden devreden ödenmemiş anapara bakiyesine bir sonraki ekstrede uygulanan faiz yükü (BR-CARD-01) |
| ekstre ödeme kararı | `CreditCardPaymentDecision` | Çözümlenen ekstre ödeme tutarı, karar kaynağı ve uygulanan ödeme tipini taşıyan sözleşme |
| ekstre ödeme kararı kaynağı | `CreditCardPaymentResolution` | Ödeme tutarının hangi kaynaktan (kesilmiş ekstre planı, vade istisnası, kart stratejisi, simülasyon yedeği) belirlendiğini belirten durum |
| ekstre projeksiyonu | `CreditCardStatementProjection` | Tek bir hesap kesim döngüsüne ait simüle edilen ekstre borcu, asgari tutarı, ödemesi, carry faizi ve devreden bakiye sözleşmesi |
| kart tarihi çözümleyici | `CreditCardDateResolver` | Kartın hesap kesim, son ödeme, bir sonraki döngü ve işlem eşleme tarihlerini takvim ve banka kurallarına göre çözümleyen saf hesaplayıcı |
| kart ödeme kararı çözümleyici | `CreditCardPaymentDecisionResolver` | Ekstre için plan, istisna ve strateji hiyerarşisine göre ödenecek tutar ve modu belirleyen saf hesaplayıcı |
| kart ekstre hesaplayıcısı | `CreditCardStatementCalculator` | Kredi kartı ekstre döngülerini, dönem içi harcamaları, asgari ödemeleri ve devreden bakiye üzerindeki carry faizini simüle eden saf projeksiyon motoru |
| sıradaki ödeme | `NextPaymentViewModel.FindPaymentIndex` | Kartın kesilmiş ekstresi; yoksa tutarı sıfırdan büyük ilk tahmini ekstresi. Kart kontrol ekranının merkezi; kararı tahminde vadeye özel plana yazılır (EK-V7, S61) |
| kararın bedeli | `CarriedAfterPayment` + sonraki `CarryInterest` | Ödeme ekstreden azsa sonraki ekstreye devreden tutar ve ona binen faiz (EK-V7 S2) |
| kart ödemesi mutabakatçısı | `CreditCardActualPaymentReconciler` | Dönem kapanışında kredi kartına yapılan fiili ödemeyi hazır ekstre projeksiyonundan düşüp kalan anaparayı bir sonraki döneme devreden saf hesaplayıcı |

## Vadeli ve Geçici Borç Planı

| Türkçe | Kod | Tanım |
|---|---|---|
| geçici ödeme planı | `TemporaryPaymentPlan` | Kredi ve kredi kartı haricindeki vadeli borç, senet, taksit ve periyodik yükümlülük sözleşmesi |
| plan taksiti | `TemporaryPaymentInstallment` | Geçici ödeme planına ait tekil takvimli ödeme kalemi |
| plan türü | `PaymentPlanKind` | Borç planının niteliği (Geçici, Taksitli, Periyodik veya Diğer) |
| plan doğrulayıcı | `TemporaryPaymentPlanValidator` | Geçici ödeme planı ve taksitlerinin iş kurallarına uygunluğunu denetleyen saf sınıf |

## Yükümlülük ve Ödeme Takvimi

| Türkçe | Kod | Tanım |
|---|---|---|
| yükümlülük türü | `ObligationType` | Borç ve nakit çıkış yükümlülüklerinin tür sınıflandırması (Kredi, Kart, Geçici, Taksitli, Diğer, Büyük Harcama) |
| yükümlülük kalemi | `ObligationItem` | Belirli bir vadede ödenmesi gereken tekil bir borç veya harcama yükümlülüğü sözleşmesi |
| zorunlu ödeme özeti | `MandatoryPaymentSummary` | Dönem içi zorunlu nakit çıkışlarını kategori bazında toplayan ve genel toplamı veren özet modeli |
| vadeli ödeme hesaplayıcısı | `ScheduledPaymentCalculator` | Vadeli borç planlarından ödenmemiş taksitleri ayıklayarak standart yükümlülük kalemlerine dönüştüren saf hesaplayıcı |
| zorunlu ödeme hesaplayıcısı | `MandatoryPaymentCalculator` | Krediler, erken ödemeler, vadeli planlar ve kart ödemelerini takvimde birleştirip zorunlu ödeme özetini üreten saf Domain motoru |
| dönem yükümlülük grubu | `PeriodObligationGroup` | Belirli bir nakit akış dönemine [Start, End) vadesi düşen borç ve harcama yükümlülüklerini doğal dönemsellikle bir arada tutan sözleşme |
| dönem yükümlülük planı | `PeriodObligationPlan` | Tüm projeksiyon dönemleri boyunca gruplanmış yükümlülük dağılımını ve ufuk dışı kalan kalemleri içeren sonuç sözleşmesi |
| dönem yükümlülük gruplayıcısı | `PeriodObligationGrouper` | Borç ve harcama yükümlülüklerini doğal dönemsellik ilkesine göre ilgili nakit akış dönemlerine [Start, End) gruplayan ve ufuk dışı kalemleri ayrıştıran saf Domain motoru |
| yükümlülük yönetim servisi | `IObligationManagementService` | Kredi, vadeli borç planı ve büyük harcama yükümlülüklerinin doğrulanmasını, kaydedilmesini, silinmesini ve açık dönem plan revizyonlarını yöneten kullanım senaryosu portu |

## Planlı Harcama ve Kullanıcı Ayarları

| Türkçe | Kod | Tanım |
|---|---|---|
| planlı büyük harcama | `PlannedLargeExpense` | Belirli bir kesin tarihte yapılması öngörülen tek seferlik nakit çıkışı |
| planlı harcama durumu | `PlannedExpenseStatus` | Harcamanın yaşam döngüsü durumu (Planlandı, Tamamlandı, İptal) |
| kullanıcı ayarları | `UserSettings` | Dönem çapası, yaşam gideri havuzu, açılış durumu ve faiz parametrelerini tutan temel ayarlar |
| harcama doğrulayıcı | `PlannedLargeExpenseValidator` | Planlanan harcama tutarlılık ve geçerlilik denetleyicisi |
| ayar doğrulayıcı | `UserSettingsValidator` | Kullanıcı ayarları tutarlılık ve faiz sınırları denetleyicisi |

## Profil ve yedek

| Türkçe | Kod | Tanım |
|---|---|---|
| profil | `UserProfile` | Bağımsız bir veri kümesi; her biri ayrı `.db3` |
| profil deposu | `IProfileRepository` | Kullanıcı profillerinin meta bilgilerini listeleyen, kaydeden ve silen veri deposu portu |
| profil depo anahtarı | `IProfileStoreSwitch` | Uygulamanın veri deposunu açık profile bağlayan veya erişimi kesen port |
| profil adı doğrulayıcısı | `ProfileNameValidator` | Profil adının uzunluk, boşluk ve Türkçe harf kurallarına göre benzersizliğini denetleyen saf yardımcı |
| profil servisi | `ProfileService` | Profil yaşam döngüsünü (CRUD, oturum açma ve kapatma) yöneten kullanım senaryosu servisi |
| yedek | `Backup` | Tüm profilleri içeren tek arşiv |
| profil yedek arşivi | `IProfileBackupArchive` | Bütün profilleri tek bir zip arşivine yazan, özetini okuyan ve geri açan fiziksel arşivleme portu |
| harici yedek deposu | `IBackupStorage` | Yedek dosyalarının cihazdaki harici depolama konumuna yazılmasını ve silinmesini yöneten port |
| yedekleme servisi portu | `IBackupService` | Yedekleme, geri yükleme ve profil aktarma kullanım senaryolarını sunan dar port |
| yedekleme servisi | `BackupService` | Gece yedekleme görevi, manuel yedekleme, kota temizliği ve geri yükleme orkestrasyonunu yürüten servis |
| yedek saklama kuralları | `BackupRetentionRules` | Dosya adı biçimlendirme, filtreleme, kopya profil adlandırma ve en yeni 7 dosyayı tutma kurallarını yöneten saf yardımcı |
| yedek özeti | `BackupSummary` | Yedek arşivinin oluşturulma zamanını ve içerdiği profil listesini taşıyan sözleşme |
| son yedek durumu | `BackupState` | Son yedeğin zaman damgasını, dosya adını ve veri parmak izini tutan durum kaydı |
| yedekleme seçenekleri | `BackupOptions` | Yedekleme çalışma dizini ve saklanacak azami dosya sayısını yapılandıran ayarlar |
| profil yedek arşivi (adaptör) | `ProfileBackupArchive` | `IProfileBackupArchive`'in Infrastructure adaptörü: profilleri zip'e yazar, parmak izini hesaplar, son yedek kaydını tutar, yedeği tanır ve profilleri hep-ya-hiç geri yükler |
| yedek biçimi | `BackupArchiveFormat` | Yedek zip'inin biçim numarası ve girdi adları; disk yerleşiminden bağımsızdır. Biçim 1 eski uygulamanın, biçim 2 bu uygulamanın yedeğidir (S57) |
| yedek manifesti | `BackupManifest` | Zip'in başındaki içindekiler listesi: biçim, oluşturulma zamanı, şema sürümü ve profiller (`BackupManifestProfile`, verisi olup olmadığıyla) |
| parmak izi | `Fingerprint` | Bütün profillerin adından ve veritabanı içeriğinden hesaplanan SHA-256; veri değişmedikçe aynı kalır, son açılış tarihi ve dosya zamanı girmez (`I34`) |
| veritabanı anlık görüntüsü | `SqliteDatabaseSnapshot` | Açık profil yazarken bile tutarlı kopya üreten `VACUUM INTO` işlemi (`I35`) |
| eski uygulamanın yedeği | `BackupArchiveFormat.LegacyVersion` | Biçim 1 yedek (`com.coinflow.mobile`, şema v17); tanınır ve açık mesajla reddedilir, içe aktarma `G1`'in işidir (`I37`) |
| manifest okuyucu | `BackupManifestReader` | Yedek zip'ini açıp manifesti denetleyen yardımcı; özet okuma ile geri yükleme aynı denetimden geçer (biçim, şema sürümü, profil listesi) |
| yedek veritabanı denetimi | `BackupDatabaseValidator` | Yedekteki veritabanını hazırlığa çıkarıp boyut, bütünlük (`quick_check`) ve `user_version` sınırlarını denetleyen yardımcı (`I38`) |
| hazırlık klasörü | `.restore-*` | Geri yüklenecek veritabanlarının telefondaki profillere dokunmadan çıkarılıp doğrulandığı geçici klasör; iş bitince silinir (`BackupWorkDirectory`) |
| geri yükleme işlemi | `ProfileImportTransaction` | Seçilen profilleri hep-ya-hiç ekleyen akış: hazırla → doğrula → önce veri sonra kayıt taşı → gerekirse geri al (`I36`, `S58`) |
| yedek klasörü | `FolderBackupStorage` | `IBackupStorage`'ın Infrastructure adaptörü: yedekleri uygulama kaldırılınca silinmeyen düz bir klasöre geçici dosya → yerine taşıma ile yazar; klasördeki bütün dosyaları listeler, hangisinin yedek olduğuna `BackupRetentionRules` karar verir (S59) |
| depolama izni | `IStorageAccess` | Yedek klasörüne yazma izninin platforma özgü kısmı; Android uygulaması uygular (V0/V13), klasör işi onsuz test edilir |
| yedek dosya öneki | `BackupRetentionRules.FilePrefix` | `Mizan-yedegi-`: bu uygulamanın yedek adlarının başı. Eski uygulamanın öneki `Mizan-yedek-`'tir; iki önek harf büyüklüğü gözetmeden de birbiriyle başlamaz, böylece iki uygulama aynı klasörde birbirinin yedeğine dokunmaz (`I39`, S59) |

## Tasarım ve Durum Yönetimi

| Türkçe | Kod | Tanım |
|---|---|---|
| ekran durumu | `ScreenState` | Sayfaların ve bileşenlerin sunum durumunu (yükleniyor, içerik, boş veri, hata) temsil eden saf durum modeli (K2) |
| durum bloğu | `StateBlock` | Boş veri veya hata durumlarını kullanıcıya açıklayıp aksiyon aldıran semantik arayüz bileşeni (GS14) |
| iskelet bloğu | `SkeletonBlock` | Veri yüklenirken spinner kullanmadan yerleşim zıplamasını önleyen SurfaceSunken zeminli yer tutucu bileşeni (GS14) |

---

# Yasaklı terimler

Aşağıdaki terimler `src/` ve `tests/` altında **geçemez.** `Mizan.Architecture.Tests`
içindeki `YasakliTerimler_KaynaktaGecemez` testi bu tabloyu okur ve ihlali derlemeyi kıran
bir hataya çevirir (kural **K9**).

Hepsinin ortak sebebi aynı: bir kavramın iki canlı adı olduğu anda, hangisinin doğru olduğu
dosyaya göre değişir ve kimse ikisini birden aramaz. Gerekçeler `SAPMALAR.md` → S11–S16.

## Eşleştirme kuralı

Test **PascalCase token eşleştirmesi** yapar, düz metin araması değil. Tanımlayıcı önce
büyük harf ve alt çizgi sınırlarından parçalara ayrılır, sonra parçalar büyük/küçük harf
gözetmeden karşılaştırılır.

Bu kural üç bilinen yanlış pozitifi kendiliğinden eler:

| Görünürde ihlal | Neden değil |
|---|---|
| `MigratePeriodPlanRevisionSchemaAsync` | Parçalar: `Migrate·Period·Plan·Revision·Schema·Async`. Hiçbiri `maas` değil. |
| `PreviewSettlementAsync` | Parçalar: `Preview·Settlement·Async`. `Preview` ≠ `Review`. |
| `SavingsGoal` | Parçalar: `Savings·Goal`. `Goal` komşuluğu muaf (aşağıda). |

Türkçe `maaş` (ş ile) ayrıca düz metin olarak da aranır — yorumlarda ve kullanıcı
metinlerinde geçer ve `ş` harfi yanlış pozitif üretemez.

Testin kendisinin de testi vardır: yukarıdaki üç örnek `YasakliTerimRegex_YanlisPozitifUretmez`
içinde sabitlenir. Regex gevşetilirse o test kırmızıya düşer.

<!-- YASAKLI-TERIMLER:BASLANGIC -->

| Yasaklı | Yerine | Kapsam | İstisna |
|---|---|---|---|
| `Salary` | `Income` / `RecurringIncome` | src+tests | `src/Mizan.Infrastructure/Imports/EskiSemaV17.cs` |
| `maaş` / `maas` | `gelir` / `Income` | src+tests | yok |
| `IncomeDay` | `PeriodAnchor.DayOfMonth` | src+tests | yok |
| `PaymentAssignment` | `CashFlowAllocation` | src+tests | yok |
| `PaymentAllocation` | `CashFlowAllocation` | src+tests | yok |
| `CoinFlow` | `Mizan` | src+tests | `src/Mizan.Infrastructure/Imports/EskiSemaV17.cs` |
| `Savings` | `Balance` | src+tests | komşu parça `Goal` veya `Target` ise muaf |
| `LivingBudget` | `PeriodVariableExpenseAllowance` | src+tests | yok |
| `Review` | `Settlement` | src | yok |

<!-- YASAKLI-TERIMLER:BITIS -->

Tablo bu iki işaretin arasında durur; test onları sınır olarak kullanır. İşaretler
silinirse test tabloyu bulamadığı için **kırmızıya düşer** — sessizce devre dışı kalmaz.

## Neden bazıları listede yok

| Terim | Neden yasaklanmadı |
|---|---|
| `Gamification` | Yeni şemada o kolon hiç doğmuyor. Kural ölü yük olurdu. |
| `Living` (tek başına) | `ActualLivingSpend`, `ObservedLivingSpend` ayrı kavramlar ve kendi kararlarını hak ediyor. `LivingBudget`'a dahil etmek gürültü üretir, gürültü de testin kapatılmasına yol açar. |
| `Income` | Yasaklanamaz — doğru kelime. Ama şu an **belirsiz**: eski kodda "maaş dışı gelir" demek. Onun yerine pozitif bir invariant: hiçbir tanımlayıcı `OtherIncome` içeremez. |
| `Review` (dokümanlarda) | "Kod incelemesi" anlamında meşru. Bu yüzden kapsamı yalnız `src`. |

## Listeye terim eklerken

1. Satırı yukarıdaki tabloya ekle.
2. `SAPMALAR.md`'ye gerekçesini yaz (hangi kavramın iki adı vardı, hangisi doğru).
3. `dotnet test` çalıştır — mevcut kodda ihlal varsa **önce onu temizle**, sonra commit et.
4. Yanlış pozitif ürettiğini fark edersen, terimi listeden çıkarma; **istisnayı** yaz ve
   `YasakliTerimRegex_YanlisPozitifUretmez` testine o örneği ekle.
