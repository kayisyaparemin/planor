# Sapmalar — Eski Koddan Bilerek Ayrıldığımız Yerler

Taşıma protokolünün varsayılanı "eskiyi sadakatle taşı"dır. Bu dosya, o varsayılanın
**geçerli olmadığı** yerleri kaydeder.

Bir sapma kaydı üç soruya cevap verir: eski kod ne yapıyordu, **neden yanlıştı**, yeni kod
bunun yerine ne yapacak. Gerekçesi yazılmamış bir sapma, altı ay sonra "acaba neden böyle
yapmıştık?" diye geri alınır.

## Kullanım

- **Aşama 3 (Sapma Kararı)** bu dosyayı okur. Adımı etkileyen bir `S` kaydı varsa karar
  zaten verilmiştir; yeniden tartışılmaz, uygulanır.
- Yeni bir sapma kararı verildiğinde **Aşama 3'te** buraya yazılır, Aşama 8'de değil.
- `Durum` sütunu: `açık` (henüz taşınmadı) · `uygulandı` (ilgili adım tamamlandı) ·
  `iptal` (sonradan vazgeçildi, gerekçesiyle).

## Numaralandırma

`S1`–`S10` kavramsal sapmalar (eski kod yanlış **modellemiş**).
`S11`–`S16` yarım kalmış yeniden adlandırmalar (isim doğru değil).
`S17`+ diğerleri.

---

## Kavramsal sapmalar

### S1 — Dönem çapası tek bir tamsayı gün

| | |
|---|---|
| **Eski** | `UserSettings.IncomeDay` bir `int`. `CashFlowPeriodCalculator`'ın her metodu bunu alıp `AddMonthsKeepingDay(..., 1, IncomeDay)` ile dönem kuruyor. Dönem tanımı: "ayın N'inci gününden sonraki ayın N'inci gününe". |
| **Neden yanlış** | Haftalık, iki haftalık, ayda iki ödeme (15 + 30) ve fatura bazlı serbest çalışan gelirini **yapısal olarak** imkânsız kılıyor. Ürün artık her gelir düzenine hitap ediyor; bu varsayım ürün vaadiyle çelişiyor. |
| **Yeni** | `PeriodAnchor` bir değer nesnesi olur. Bugün yalnız `DayOfMonth` taşır — kapsam bilerek dar — ama tip olduğu için haftalık/iki haftalık sonradan **kırmadan** eklenebilir. `IncomeDay` adı yasaklı (bkz. `SOZLUK.md`). |
| **Etkiler** | `D3`, `D12`, `D19`, `D20`, `D21`, `A9`, `A10`, `A15`, `A17`, `I1` |
| **Durum** | uygulandı |

### S2 — Tek gelir akışı: ikinci gelir birincisini eziyor

| | |
|---|---|
| **Eski** | `IncomeResolver.Resolve` **tek** kayıt döndürüyor: `EffectiveDate`'e göre azalan sırala, `FirstOrDefault`. `IncomeProjectionCalculator` dönem başına en fazla bir gelir kalemi ekliyor. |
| **Neden yanlış** | **Bu bir isimlendirme sorunu değil, bir hata.** `salary_schedule` eşzamanlı gelir akışlarının kümesi değil, tek bir gelirin etkin tarihli geçmişi. İkinci bir düzenli gelir eklemek geliri artırmıyor — birincisini kendi etkin tarihinden itibaren **sessizce eziyor.** Maaş 40.000 (01.01) + kira 10.000 (01.02) giren kullanıcı Şubat'tan itibaren 50.000 değil 10.000 görüyor. Arayüz ise tersini vaat ediyor: onboarding "Örn. Maaş, kira geliri" diyor, "Düzenli gelir**lerini** ekle" yazıyor ve liste gösteriyor. |
| **Yeni** | `IncomeStreamResolver` **çoğul** döndürür. `IncomeProjectionCalculator` o dönemde aktif olan **tüm** akışları toplar. Her akışın kendi etkin tarihli tutar geçmişi olur; "en son kazanır" kuralı akışın *kendi içinde* işler, akışlar arasında değil. |
| **Etkiler** | `D4`, `D12`, `A2`, `I1` — ve `D21` üzerinden tüm projeksiyon zinciri |
| **Durum** | uygulandı |

### S3 — Gelirin kendi ödeme günü yok

| | |
|---|---|
| **Eski** | `SalaryScheduleEntry` yalnız `Amount`, `EffectiveDate`, `Description` taşıyor. Ne zaman yattığı `Settings.IncomeDay`'den, yani **global** bir ayardan geliyor. Projeksiyon kalemi `period.Start` ile damgalanıyor, kendi tarihiyle değil. |
| **Neden yanlış** | Gelir ile dönem sınırı aynı kavram hâline gelmiş. Bu yüzden ikinci bir akış prensipte bile temsil edilemiyor (S2'nin kökü burası), ve "dönem 1'inde başlıyor ama gelir 5'inde yatıyor" modellenemiyor. `OneTimeIncome`'ın tarihi var ama tekrarı yok; iki gelir tipi asimetrik. |
| **Yeni** | `RecurringIncome` kendi `PaymentDay`'ini taşır. Dönem çapası (`PeriodAnchor`) ile gelir tarihi **ayrı** kavramlar olur. |
| **Etkiler** | `D4`, `D12`, `I1` |
| **Durum** | uygulandı |

### S4 — Dönem kapanışı geliri takip ediyor

| | |
|---|---|
| **Eski** | `GetNextSettlementDate(snapshotDate, IncomeDay)` dönemin bitişini döndürüyor; yani mutabakat tarihi **gelir tarihiyle aynı şey**. `ProjectionBoundaryResolver` sınırı `FirstUnrealizedSalaryDate` diye adlandırıyor. |
| **Neden yanlış** | Üç ayda bir KDV ödeyen ya da aylık mutabakat yapan biri için "dönemin döndüğü an" ile "paranın yattığı an" farklı şeyler. |
| **Yeni** | `PeriodAnchor` (dönem ne zaman döner) ile gelir olayı ayrı tutulur. Mutabakat çapadan türer, gelirden değil. |
| **Etkiler** | `D3`, `A10`, `A16`, `A21` |
| **Durum** | açık |

### S5 — "Diğer gelir" maaşa göre tanımlı

| | |
|---|---|
| **Eski** | `OtherIncomes`, `other_incomes` tablosu, `IIncomeRepository.GetOtherIncomesAsync`, `IncomeProjectionSummary.PrimaryIncome` / `OtherIncome`, arayüzde "Diğer gelir". Dahası `ISalaryRepository` (düzenli gelir) ile `IIncomeRepository` (tek seferlik gelir) ayrı ayrı var. |
| **Neden yanlış** | "Diğer" olması için ayrıcalıklı bir "birincil" olması gerekir. **`Income` kelimesinin kendisi şu anda "maaş dışı gelir" anlamına geliyor** — eksen, kelimenin anlamında hayatta kalmış. |
| **Yeni** | `RecurringIncome` / `AdHocIncome`. Ayrıcalıklı gelir yok. Hiçbir tanımlayıcı `OtherIncome` içeremez (pozitif invariant, mimari testle korunur). Portlar `IRecurringIncomeRepository` + `IAdHocIncomeRepository`. |
| **Etkiler** | `D4`, `D12`, `A2` |
| **Durum** | uygulandı |

### S6 — İlk gelir kaydı tüm planı başlatıyor

| | |
|---|---|
| **Eski** | İlk `SalaryScheduleEntry` kaydedildiğinde `ProjectionAnchorDate` `clock.Today` yapılıyor ve kullanım düzeni kurulumu tetikleniyor. Arayüz bunu açıkça söylüyor: *"İlk gelir kaydı gelir kullanım düzeni kurulumunu başlatır."* |
| **Neden yanlış** | İki varsayım: ayrıcalıklı bir "ilk" gelir var, ve tüm tahsis stratejisi ona asılı. Üç akış giren düzensiz kazanan için "ilki" keyfî. |
| **Yeni** | Çapa başlatma **dönem yapılandırması** adımına ait, ilk gelir satırına değil. |
| **Etkiler** | `A20`, `V4` |
| **Durum** | açık |

### S7 — Yaşam gideri aylık, dönem ise aylık değil

| | |
|---|---|
| **Eski** | `MonthlyVariableExpenseAllowance` **takvim ayı** başına; dönem ise `[IncomeDay, sonraki IncomeDay)`. Uyuşmazlık `ResolveLivingBudget` içinde oranlayarak kapatılıyor ve bu özel kural olarak belgelenmiş. |
| **Neden yanlış** | Dönem aylık olmaktan çıktığı anda tutarsız hâle gelir. Tahsisat **dönem başına** olmalı. |
| **Yeni** | `PeriodVariableExpenseAllowance` — dönem başına. Oranlama gerekmez. |
| **Etkiler** | `D9`, `A10`, `A15` |
| **Durum** | uygulandı |

### S8 — Simülatör geliri "maaş zammı" olarak modelliyor

| | |
|---|---|
| **Eski** | `SimulationScenarioType.SalaryChange` mevcut listeye yeni bir kayıt ekliyor ve S2'nin "en son kazanır" semantiğine güveniyor. Doğrulama, **aynı tarihte iki gelir değişikliğini reddediyor.** |
| **Neden yanlış** | Bu kısıt yalnız tek akış varsa tutarlı. Çoklu akışta "aynı gün iki gelir değişikliği" normaldir (A akışına zam, B akışına yeni müşteri). |
| **Yeni** | `IncomeChange` senaryosu **hangi akışa** uygulandığını taşır. Çakışma kontrolü akış bazında yapılır. |
| **Etkiler** | `D23`, `D24`, `A19` |
| **Durum** | uygulandı |

### S9 — Arayüz motorun veremediği bir genelliği vaat ediyor

| | |
|---|---|
| **Eski** | Ayarlar "Dönem günü" diyor ve "seçilen gün ayda yoksa ayın son günü kullanılır" açıklaması var — çapayı genelmiş gibi sunuyor ama arkasında hâlâ tek bir ayın günü var. Finansal yapı boş durumu ise *"Maaşını eklediğinde…"* diyor. Kayıt türü ayrımı `"salary"` sihirli metniyle yapılıyor. |
| **Neden yanlış** | Arayüz ile motor farklı şeyler söylüyor. Kullanıcı arayüze inanıyor ve yanlış sonuç alıyor (S2). |
| **Yeni** | Arayüz motorun gerçekten yaptığını söyler. Kayıt türü ayrımı `enum`, sihirli metin değil. |
| **Etkiler** | `V4`, `V6`, `V13` |
| **Durum** | açık |

### S10 — Körlemesine find/replace hasarı

| | |
|---|---|
| **Eski** | Yeniden adlandırma değiştirilen kelimenin büyük harfini koruduğu için PascalCase **yerel değişken ve parametreler** oluşmuş: `int IncomeDay` parametresi, `var IncomeDay = settings.IncomeDay;`, `var VariableExpenseAllowance = ResolveLivingBudget(...)`. C# konvansiyonunu ihlal ediyor ve aynı adlı özelliği gölgeliyor. |
| **Neden yanlış** | Mekanik yeniden adlandırmanın kanıtı ve gizli hata kaynağı. |
| **Yeni** | `.editorconfig` içinde `IDE1006` hata seviyesinde. Bu sınıf hasar derleme zamanında yakalanır, göze bırakılmaz. |
| **Etkiler** | tüm adımlar (mekanizma, `F1`'de kurulur) |
| **Durum** | açık |

---

## Yarım kalmış yeniden adlandırmalar

Hepsinin ortak gerekçesi: bir kavramın **iki canlı adı var**. Yeniden yazımda doğru ad
baştan kullanılır; eski ad yasaklı terim listesindedir ve mimari test onu engeller.

| Kod | Eski ad | Yeni ad | Eskide kalan | Not |
|---|---|---|---:|---|
| **S11** | `Salary` | `Income` / `RecurringIncome` | 324 satır | Eksenin kendisi. Ayrıca 115 satır Türkçe `maaş`, 11'i kullanıcının okuduğu metin. |
| **S12** | `Review` (dönem kapanışı) | `Settlement` | 442 satır | En büyük yarım rename. `Settlement` yalnız üç `[Column]` takma adında yaşıyor. |
| **S13** | `Savings` | `Balance` | 116 satır | Yalnız bayat değil **yanlış**: değer negatif olabiliyor, arayüz `AllowNegative="True"` diyor. |
| **S14** | `PaymentAssignment` | `CashFlowAllocation` | 83 satır | Kavramın **üç** adı var; üçüncüsü `PaymentAllocationStrategyResolver`. İkisi de yasaklı. |
| **S15** | `CoinFlow` | `Mizan` | 73 satır | App id, Android bileşen adları, `coinflow.db3` dosya adı. Yeni projede app id `com.mizan.app`. |
| **S16** | `LivingBudget` | `PeriodVariableExpenseAllowance` | 26 satır | "Bütçe" metaforu da yanlış: bu bir oran değil, bir havuz. Ayrıca artık ay başına değil **dönem** başına (S7). |

`Gamification` ve günlük harcama takibi **bilerek listede yok** — yeni şemada o kolonlar hiç
doğmayacağı için yasak ölü yük olur. Onlar `Hiç taşıma` kararıyla (Aşama 3) elenir.

---

## Diğer

### S17 — Yayındaki uygulamada S2 hatası duruyor

| | |
|---|---|
| **Durum** | **eski projede açık — bilerek dokunulmuyor** |
| **Ne** | S2'deki hata (`IncomeResolver` tekil) şu anda yayındaki `com.coinflow.mobile` sürümünde de var. İkinci bir düzenli gelir ekleyen kullanıcı, birincisinin sessizce ezildiğini fark etmiyor. |
| **Karar** | Kullanıcı "kaydet, şimdi dokunma" dedi. Eski repoda **hiçbir değişiklik yapılmayacak**; yeni proje zaten doğru kurulacak. |
| **Risk** | Eski uygulama kullanılmaya devam ederse yanlış projeksiyon görülür. Veri kaybı yok — kayıtlar duruyor, yalnızca toplanmıyorlar. |
| **İlgili** | `G1` (eski yedekten içe aktarma) bu durumu bilmeli: eski veritabanındaki birden fazla `salary_schedule` satırı, yeni modelde **ayrı akışlar mı yoksa tek akışın geçmişi mi** olduğuna karar vermek zorunda. Bu, `G1`'in Aşama 3'ünde konuşulacak açık bir sorudur. |

### S18 — Yapay dönem kullanım düzeni (Upcoming/Previous) elendi, doğal dönemsellik benimsendi

| | |
|---|---|
| **Eski** | `CashFlowAllocationMode` (`UpcomingPeriod` / `PreviousPeriod`), `CashFlowAllocationStrategy`, `PaymentAllocationStrategyResolver`, `CashFlowAllocationPlanner`. Kullanıcının harcamaları "maaş öncesi / maaş sonrası" psikolojik algısına göre yapay olarak bir önceki veya bir sonraki döneme kaydırılıyordu. Mod değişiminde arada kalan veya mükerrerleşen ödemeleri yönetmek için `TransitionCatchUp`, `TransitionForward`, `ForwardFundedAmount` gibi 600+ satırlık devasa bir mekanizma kurgulanmıştı. |
| **Neden yanlış** | Mizan v2'de dönem çapası gelirden bağımsızlaştırıldı (`PeriodAnchor`, `S1`) ve birden fazla düzenli gelir akışı (`RecurringIncome`, `S2`) tanındı. Artık tek bir "maaş günü" olmadığı için harcamayı "maaş öncesi / sonrası" diye kaydırmak matematiksel olarak da anlamsızlaştı. Ayrıca Kasım ayında vadesi gelen bir gideri yapay olarak Aralık bütçesine bağlamak dönemsellik muhasebe ilkesini bozar ve kullanıcının gerçek dönem dengesini saklar. Vade ile gelir günü arasındaki uyumsuzluk (örn. 5'inde kira, 15'inde maaş) bir dönem tahsisi sorunu değil; likidite / dönem içi günlük bakiye (KMH) sorunudur. |
| **Yeni** | Doğal dönemsellik (`period.Contains(obligation.DueDate)`). Dönemin yarı açık aralığı `[PeriodStart, PeriodEnd)` içine vadesi düşen her gelir ve gider istisnasız o döneme aittir. `CashFlowAllocationMode`, `CashFlowAllocationStrategy`, `CashFlowAllocationStrategyResolver` (`D10`) ve `CashFlowAllocationPlanner` (`D19`) taşınmadan elenmiştir (Taşımama hakkı). |
| **Etkiler** | `D1`, `D10`, `D19`, `D20`, `D21`, `A17`, `A20`, `V13`, `I1` |
| **Durum** | uygulandı |

### S19 — İlk dönem kurulum çapa gününe kenetlenir (hayalet dönem kapanışı elendi)

| | |
|---|---|
| **Eski** | Kullanıcı uygulamayı örneğin ayın 5'inde kurduğunda ve çapa günü ayın 10'u olduğunda, `ProjectionAnchorDate = clock.Today` (5'i) yapılıyor ve ilk dönem `[5'i, 10'u)` olarak 5 günlük yapay bir dilim açılıyordu. 5 gün sonra ayın 10'u geldiğinde `SettlementAvailableFrom` tetiklenip kullanıcıya daha hiçbir gerçek dönem yaşamadan "Geçmiş dönemi kaydet / kapat" uyarısı çıkarılıyordu. |
| **Neden yanlış** | Kullanıcı uygulamayı sadece 5 gün önce kurmuşken ve ortada bir maaş veya tam dönem bütçesi yokken 5 günlük yapay bir dönemi kapatmaya zorlanması kafa karıştırıcı ve anlamsızdır. Kullanıcının planladığı ilk tam dönem çapa gününde başlar. |
| **Yeni** | İlk resmi nakit akış dönemi, kurulum tarihi veya sonrasındaki ilk çapa gününe kenetlenir (`GetFirstPeriodStartOnOrAfter(installDate, anchor)`). Örneğin 5 Eylül'de kurulum yapılıp çapa 10 seçildiyse ilk dönem `[10 Eylül, 10 Ekim)` olur; ilk mutabakat/kapanış istemi 10 Eylül'de değil, 10 Ekim'de gelir. Kurulum ile çapa arasındaki ara dönem (varsa) sadece bekleme penceresidir. `UserSettings.ProjectionOpeningBalance` ise ilk çapa gününün açılış bakiyesidir ve Ayarlar UI ekranından düzenlenmez. |
| **Etkiler** | `D9`, `A9`, `A10`, `A15`, `A16`, `A21`, `V4` |
| **Durum** | açık |

### S20 — Dönem içi plansız akış girme spekülatif özelliği elendi (Expense Tracker modeli reddedildi)

| | |
|---|---|
| **Eski** | Şema v13'te `period_observation_flows` tablosu (`PeriodObservationFlowRow`), Domain'de `PeriodObservationFlow` ve `PeriodObservation.Flows` tanımlanmış; `PeriodWorkflowService` kapanış taslağında bu listeyi okuyacak mapping kodu bırakmıştı. |
| **Neden yanlış** | Mizan bir "expense tracker" (günlük fiş girilen harcama defteri) değildir. Dönem içinde yapılan borç ve enstrüman harcamaları (örn. 6 taksitli harcama) doğrudan Finansal Yapı'ya (kredi kartına / borçlara) kaydedilir ve sisteme otomatik yansır. Günlük yaşam harcamaları ise tek tek fiş girilerek değil, dönem başında ayrılan serbest yaşam havuzundan (`PeriodVariableExpenseAllowance`) düşülerek ve kullanıcının ara bakiye girmesiyle (`ObservedBalance`) bakiye farkından tersine mühendislikle türetilir. Dönem içinde ana sayfadan münferit akış girmek mükerrerlik ve arayüz karmaşası yaratıyordu; üstelik eski projede de UI ve servis seviyesinde hiç tamamlanmamış, ölü tablo ve model olarak kalmıştı. |
| **Yeni** | `PeriodObservationFlow` ve `period_observation_flows` tablosu bütünüyle elendi (Taşımama hakkı). `PeriodObservation` yalnızca anlık bakiye (`ObservedBalance`), türetilen yaşam harcaması (`ObservedLivingSpend`) ve plandaki borç ödemelerinin ara işaretlenmesini (`Payments`) barındırır. Arızi ve beklenmedik akışlar (`ActualFlow`) yalnızca dönem kapanış mutabakatında (`PeriodSettlement`) gerekirse sorulur. Sonraki katmanlarda (Application, Infrastructure, UI) `PeriodObservationFlow` ile ilgili hiçbir kod, port veya veritabanı tablosu üretilmeyecek; eski kalıntılar temizlenecektir. |
| **Etkiler** | `H3`, `A4`, `A15`, `A21`, `I1`, `V3` |
| **Durum** | uygulandı |

### S21 — Banka ekstresinden otomatik içe aktarma (PDF Statement Import) özelliği elendi

| | |
|---|---|
| **Eski** | `ICreditCardStatementImport`, `CreditCardStatementImportModels`, `CreditCardStatementImportWorkflow`, `CreditCardStatementImporter`, `CreditCardStatementParsers`. Yalnızca 2 banka (Akbank Axess ve Garanti Bonus) için PDF ayrıştırma, confidence puanlama ve kart kontrol UI'ında PDF yükleme diyaloğu vardı. Taşınan Domain modellerinde `CreditCardStatementSource` enum'ı (`Manual`, `PdfImport`), `CreditCardStatement.SourceDocumentFingerprint`, `CreditCardStatement.ImportedAt`, `CreditCardStatementProjection.StatementSource` ve `CreditCardDateResolver`'da `importedExactDate` alanları bulunuyordu. |
| **Neden yanlış** | Bankaların sağladığı PDF/ekstre veri formatları birbirinden radikal düzeyde farklıdır, standart bir şablon yoktur ve bankalar arayüzlerini/PDF mizanpajlarını sık sık değiştirmektedir. Tüm bankalar analiz edilmemiştir ve eski MVP uygulamasında da bu özellik kırılgan regex'lerle yarım çalışıyordu. Olgunlaşmamış ve yüksek bakım maliyetli bir özelliğin v2'ye taşınması mimariyi gereksiz kirletmekte ve karmaşıklaştırmaktadır. |
| **Yeni** | Ekstreden otomatik içe aktarma özelliği bütünüyle elendi (Taşımama hakkı). Kredi kartı ekstreleri kullanıcının doğrudan tutar, asgari tutar ve vade bilgilerini girdiği veya dönemsel mutabakatla onayladığı yalın ve güvenilir bir sözleşmeye indirgendi. `CreditCardStatementSource` enum'ı, `SourceDocumentFingerprint`, `ImportedAt` ve `StatementSource` alanları ile `importedExactDate` parametreleri silindi; `PdfPig` NuGet paketi kaldırıldı. `A25` ve `I5` adımları iptal edildi. |
| **Etkiler** | `D1`, `D7`, `D14`, `A25`, `I5`, `V7` |
### S22 — Profil deposundan tek veritabanı geçiş metotları (AdoptLegacyDatabase) elendi

| | |
|---|---|
| **Eski** | `IProfileRepository.HasLegacyDatabase`, `IProfileRepository.AdoptLegacyDatabaseAsync`, `ProfileService.GetProfilesAsync` içindeki otomatik taşıma bloğu. Eski tek veritabanlı sürümlerden profil mimarisine geçerken `coinflow.db3` dosyasını ilk açılışta `Profilim` adlı profile aktarıyordu. |
| **Neden yanlış** | Mizan v2 tamamen yeni bir uygulama kimliği (`com.mizan.app`) ile kurulmaktadır; eski uygulamanın (`com.coinflow.mobile`) tek veritabanı yolu cihazdaki bu yolda hiçbir zaman var olmayacaktır. Eski şema v17 verileri `TASIMA-PLANI.md`'deki `G1` adımında (yedek arşivinden tek seferlik içe aktarma) taşınacaktır. Eski tek dosya geçiş mantığı yeni mimaride ölü koddur. |
| **Yeni** | `HasLegacyDatabase` ve `AdoptLegacyDatabaseAsync` metotları `IProfileRepository` ve `ProfileService` sözleşmesinden tamamen elendi (Taşımama hakkı). Profil deposu yalnızca mevcut profillerin CRUD işlemlerine odaklanır. |
| **Etkiler** | `A3`, `I3`, `G1` |
| **Durum** | uygulandı |

### S23 — Tarihçe deposundan eski onboarding batch metodu (ApplyOnboardingSetup) elendi

| | |
|---|---|
| **Eski** | `IFinancialSnapshotRepository.ApplyOnboardingSetupAsync(OnboardingPersistenceBatch batch)`. Eski şemadaki yasaklı terimleri (`Salaries`, `OtherIncomes`, `PaymentAssignmentStrategies`) ve yapay tahsis modellerini taşıyan monolitik bir batch alıyordu. |
| **Neden yanlış** | Mizan v2'de çoklu gelir akışı (`S2`), dönem çapası (`S1`), doğal dönemsellik (`S18`) benimsendi ve eski yapay tahsis stratejileri tamamen elendi. Eski onboarding batch'i bu ölü alanları barındırıyordu. Ayrıca Mizan v2'de kurulum sihirbazı (onboarding) dar portlar (`IUserSettingsRepository`, `IRecurringIncomeRepository`, `IPeriodHistoryRepository` vb.) üzerinden Faz V ve A20 adımlarında temiz modellerle kurgulanacaktır. |
| **Yeni** | `ApplyOnboardingSetupAsync` metodu tarihçe portundan tamamen elendi (Taşımama hakkı). Tarihçe portu yalnızca dondurulmuş planların, durumların ve dönem mutabakat taahhütlerinin yönetimine odaklanır. |
| **Etkiler** | `A4`, `I2`, `V4` |
| **Durum** | uygulandı |

### S24 — Gözlem defteri ile hatırlatıcı yanıtları portları ayrıştırıldı

| | |
|---|---|
| **Eski** | `IObservationRepository` içinde hem dönem içi ara gözlem defteri (`PeriodObservation`) hem de bildirim/hatırlatıcı yanıtları (`PaymentReminderResponse`) metotları tek bir arayüzde toplanmıştı. |
| **Neden yanlış** | İki tamamen bağımsız iş yeteneğinin tek portta birleşmesi Kural M5'i (dar odaklı portlar) zorlar ve ilgisiz servislerin birbirine bağımlı olmasına yol açar. Gözlem defteri açık dönemin serbest nakit ve borç gerçekleşmesini tutarken, hatırlatıcılar bildirim yaşam döngüsüne aittir. |
| **Yeni** | `IPeriodObservationRepository` yalnızca dönem gözlem defteri işlemlerine (`GetPeriodObservationAsync`, `UpsertPeriodObservationAsync`, `DeletePeriodObservationAsync`) odaklandı. Hatırlatıcı yanıtları ve politikaları müstakil olarak `A5` adımında ele alınacaktır. |
| **Etkiler** | `A4`, `A5`, `I2` |
| **Durum** | uygulandı |

### S25 — Simülasyon taslağı portu ile plan uygulama batch'i ayrıştırıldı

| | |
|---|---|
| **Eski** | `ISimulationRepository` içinde hem taslak yönetimi (`GetSimulationDraftsAsync`, `UpsertSimulationDraftAsync`, `DeleteSimulationDraftAsync`) hem de plan uygulama batch'i (`ApplySimulationBatchAsync(SimulationPersistenceBatch)`) yer alıyordu. `SimulationPersistenceBatch` modeli ise eski şemanın yasaklı terimlerini (`SalaryScheduleEntry`, `PaymentAssignmentStrategies`, `OneTimeIncome`) barındırıyordu. |
| **Neden yanlış** | İki tamamen farklı sorumluluğun tek arayüzde birleşmesi Kural M5'i (dar portlar) ihlal eder. Taslaklar `simulation_drafts` ve `simulation_draft_conditions` tablolarında izole saklanan varsayımsal deneme paketleridir; oysa simülasyonu plana uygulamak A2'de taşınan temel enstrüman depolarının (`ILoanRepository`, `ICreditCardRepository`, `IRecurringIncomeRepository` vb.) ve `A19` iş akışının sorumluluğudur. |
| **Yeni** | Port yalnızca taslak yönetimine odaklanır ve sorumluluğunu açıkça belirtecek şekilde `ISimulationDraftRepository` olarak adlandırılır. `ApplySimulationBatchAsync` porttan elenir (Taşımama hakkı). Plan uygulama mekanizması `A19` adımında temiz port kompozisyonuyla kurgulanacaktır. |
| **Etkiler** | `A6`, `A19`, `I2` |
| **Durum** | uygulandı |

### S26 — IMizanStore tanrı arayüzü elendi, dar portlar zorunlu kılındı (Düğüm T10)

| | |
|---|---|
| **Eski** | `IMizanStore` 10 ayrı depoyu (`ISettingsRepository`, `ISalaryRepository`, `IIncomeRepository`, `ILoanRepository`, `IPaymentPlanRepository`, `ICreditCardRepository`, `IExpenseRepository`, `IObservationRepository`, `ISimulationRepository`, `IFinancialSnapshotRepository`) tek bir arayüzde birleştiriyordu. Ayrıca veritabanı ilklendirme (`InitializeAsync`), tüm finansal verileri temizleme (`ClearAllFinancialDataAsync`) ve canonical test tohumu yükleme (`LoadCanonicalDevelopmentDataAsync`) metotlarını taşıyordu. 11 servis ve `MizanService` doğrudan `IMizanStore` alıyordu. |
| **Neden yanlış** | Kural M5 (tanrı arayüz yasak) ve Düğüm T10 (ayrıştırma dekoratif) ihlalidir. 10 repo arayüzü tanımlanmış olmasına rağmen servislerin hiçbirinin bunları kullanmaması mimari ayrıştırmayı yalancı kılıyordu. Bir servisin tek bir tabloya ihtiyaç duyduğunda bile 40 metotlu tanrı arayüze erişmesi bağımlılık grafiğini çorba yapıyor, test sahtelerini aşırı şişiriyordu. Veritabanı ilklendirmesi profil açılışının (`IProfileStoreSwitch`), tohumlama ise geliştirme altyapısının konusudur. |
| **Yeni** | `IMizanStore` taşınmadan elendi (Taşımama hakkı). A2–A6 adımlarında taşınan 12 bağımsız dar port kullanılır. Servisler yalnızca ihtiyaç duydukları dar portları (en fazla 1–3 adet) enjekte eder (Kural M3). `IMizanStore` veya birden fazla depoyu birleştiren kompozit arayüzlerin türemesi mimari testle (`ArchitectureTests.KompozitDepoArayuzu_Ve_IMizanStore_Yasak`) kesin olarak engellenir. |
| **Etkiler** | `A7`, `A9`–`A21`, `I2` |
| **Durum** | uygulandı |

### S27 — Ödenmeyen yükümlülük devir tarihi (carryDate) doğal dönemsellik gereği newAnchor olur

| | |
|---|---|
| **Eski** | `carryDate = newAnchor.AddDays(1)`. Ödenmeyen taksit, borç ve harcamalar yeni dönemin ilk günü yerine ikinci gününe öteleniyordu. |
| **Neden yanlış** | Eski projede dönemler `(checkpoint, nextCheckpoint]` (sol açık, sağ kapalı) şeklinde modellenmişti. Bu nedenle `checkpoint` gününün kendisi yeni dönemin DIŞINDA kalıyor ve borcu yeni döneme sokabilmek için zorunlu olarak `+1 gün` ekleniyordu. |
| **Yeni** | Mizan v2'de `CashFlowPeriod` yarı açık aralığı `[Start, End)` şeklindedir (S18 doğal dönemsellik). Kapanan dönem `[oldAnchor, newAnchor)` iken, yeni dönemin başlangıcı tam olarak `PeriodStart = newAnchor`'dur (`period.Contains(newAnchor) == true`). Ödenmeyen borçlar yapay olarak bir gün ötelenmez, doğrudan yeni dönemin açılış/ilk gününe (`carryDate = newAnchor`) devredilir. |
| **Etkiler** | `A12`, `A16` |
| **Durum** | uygulandı |

### S28 — Kredi analizi engel metni Application'dan çıkar, kaydetme hataları kültür taşımaz

| | |
|---|---|
| **Eski** | `LoanPayoffOverview.IssueMessage` (bir model) `LoanPayoffService.DescribeIssue` statik metodunu çağırarak `LoanAnalysisIssue` için Türkçe ekran metni üretiyordu. `LoanPayoffService.PrepareForSave` hata mesajlarında tutarı ve tarihi `CultureInfo.GetCultureInfo("tr-TR")` ile biçimliyordu (`130.000 TL`, `18.09.2026`). |
| **Neden yanlış** | Model servise bakıyordu (M6) ve enjekte edilmeyen bir sınıfın statik metodunu çağırıyordu (M8). Ekran metni, kararı veren Domain enum'undan (`LoanAnalysisIssue`) sunum kenarında türetilebilecekken uygulama katmanına gömülmüştü. Kural 05'e göre kültür yalnız sunum kenarında geçer. |
| **Yeni** | `DescribeIssue` ve `IssueMessage` taşınmaz (Taşımama hakkı); `LoanPayoffOverview` yalnız `Analysis.Issue` enum'unu taşır, metin eşlemesi `V6`'da sunum katmanında yapılır. `PrepareForSave` Türkçe `InvalidOperationException` mesajlarını korur (Domain `LoanPrepaymentValidator` emsali) ama `CultureInfo` kullanmaz: tarih kültürden bağımsız sabit `dd.MM.yyyy` deseniyle yazılır, para tutarı mesaja gömülmez. |
| **Etkiler** | `A13`, `A20`, `V6` |
| **Durum** | uygulandı |

### S29 — Son dönemler özeti bakiye değil, net değişim toplar

| | |
|---|---|
| **Eski** | `HistoryQueryService.GetRecentSummaryAsync` son N dönemin `PlannedEndingBalance` ve `ActualEndingSavings` değerlerini topluyordu; `HistoryViewModel` bunu "Dönem sonu • son 3 dönem" başlığıyla "Planlanan / Gerçekleşen / Fark" olarak gösteriyordu. |
| **Neden yanlış** | Dönem sonu bakiyesi bir **stoktur**; dönemler arasında toplanamaz. Üç dönemi 50.000 TL planlanıp 49.000 TL kapanan kullanıcı "Planlanan 150.000 / Gerçekleşen 147.000" görüyordu. Bu sayılar kullanıcının hesabında hiçbir zaman olmadı. Yalnız fark toplamı anlamlıydı, çünkü her dönemin kendi sapmasıdır. |
| **Yeni** | `HistorySummary(PlannedNetChange, ActualNetChange, Difference, PeriodCount)`. Planlanan net değişim `Σ (nihai planın kapanış bakiyesi − plan açılış bakiyesi)`, fiilî net değişim `Σ (teyitli kapanış − plan açılış bakiyesi)`. Açılış iki tarafta aynı olduğu için (`I21`) fark eskisiyle kuruşu kuruşuna aynı çıkar. Ekran "son 3 dönemde X TL artış planladın, Y TL gerçekleşti" diyebilir. |
| **Etkiler** | `A14`, `V12` |
| **Durum** | uygulandı |

### S30 — Mevcut dönemde kartın güncel ödemesi yarı açık dönem penceresiyle seçilir

| | |
|---|---|
| **Eski** | `PeriodProgressService.CurrentCardPayments`, kartın bu dönemdeki güncel ödemesini `PaymentDueDate > PeriodStart && PaymentDueDate <= PeriodEnd` penceresiyle, yani `(başlangıç, bitiş]` aralığıyla topluyordu. |
| **Neden yanlış** | Eski projenin sol-açık dönem modelinden kalma (bkz. `S27`). v2'de dönem `[Start, End)` ve dondurulan plan satırları `period.Contains` ile seçiliyor. Eski pencere korunursa vadesi dönemin ilk gününe düşen kart ödemesi gidişattan **düşer** (güncel tutar bulunamaz), vadesi dönem sonu gününe (yani sonraki dönemin ilk gününe) düşen ödeme ise bu döneme **sızar**. Plan satırı ile güncel tutar farklı dönemlere bakmış olur. |
| **Yeni** | Kartın güncel ödemesi, dondurulan planla aynı kuralla seçilir: `period.Contains(statement.PaymentDueDate)`. |
| **Etkiler** | `A15c-2` |
| **Durum** | uygulandı |

### S31 — Gidişat, gelirin dönem içindeki tarihini bilmeli

| | |
|---|---|
| **Eski** | `PeriodProgressService.Build` dönem başı pozisyonunu `OpeningBalance + PlannedIncome` olarak alıyordu: gözlenen bakiyeden yaşam harcamasını `açılış + gelir − yapılan ödemeler − bakiye` diye geri çözüyordu. Dondurulan plan (`PeriodPlanSnapshot`) geliri yalnız **toplam** olarak saklıyor; gelirin ne zaman yattığı plan içinde yok. |
| **Neden yanlış** | Eskide doğruydu, çünkü dönem gelir gününde başlıyordu. `S3` ile gelir kendi ödeme gününe kavuştu ve `S4` ile dönem çapası gelirden ayrıldı; artık gelir dönemin ortasında yatabiliyor. Çapa ayın 1'i, gelir 15'i olan kullanıcı 12'sinde bakiyesini girerse, henüz yatmamış 40.000 TL'lik gelir "harcanmış" sayılır: gerçekte 7.000 TL harcamış kullanıcıya "47.000 TL harcadın, havuzu 32.000 TL aştın" denir. Çapa günü ile gelir günü farklı olan her kullanıcıda, gelirden önceki her gözlem yanlış çıkar. |
| **Yeni** | Dondurulan plan gelir kalemlerini de **satır satır** taşır (projeksiyon onları `IncomeItems` olarak zaten tarihleriyle üretiyor). Gidişat gelirleri de ödeme satırları gibi ayırır: gözlem gününden önce yatan gelir bakiyenin içindedir; sonra yatacak olan dönem sonuna eklenir. Karar: `A15c`'den önce dondurulan plana gelir satırlarını ekleyen ayrı bir düzeltme yapılır; `A15c` bir kez ve bu modelle yazılır. Canlı plandan gelir takvimi hesaplamak reddedildi: dondurulmuş planın dışına çıkar ve bağımlılığı büyütür. |
| **Uygulama (plan yarısı)** | `PeriodPlanSnapshot` ve `PeriodPlanRevision` yeni `IncomeLines` koleksiyonunu (`PeriodPlanIncomeLine`: tür, kaynak gelir kimliği, ad, tarih, tutar) taşır; satırlar projeksiyonun o döneme saydığı gelir kalemlerinin kendisidir, toplamları `PlannedIncome`'a kuruşu kuruşuna eşittir (`I28`). Gelir `PaymentLines`'a yeni bir `PlanPaymentSourceType` olarak **girmez**: `ActualPayment` ve `PeriodObservationPayment` ödeme satırına bağlanıyor, gelir orada olsaydı dönem kapanışında ve gözlemde "ödenmemiş ödeme" gibi görünürdü. Plan revizyonu imzası gelir satırlarını da karşılaştırır; toplam aynı kalıp yalnız yatış günü değişse de revizyon doğar. |
| **Karar (gözlem günü)** | `A15c` Aşama 3'te verildi: gözlem **günü** yatan gelir bakiyenin içinde sayılır (`PlannedDate <= ObservedOn`). Kullanıcı bakiyesine en çok gelir günü bakıyor ve gelir genelde gece ya da sabah yatıyor. Varsayım yanlış çıkarsa hata kötümser tarafta kalır: o gün harcama fazla, dönem sonu düşük görünür. Ödemelerle aynı kural (`<`) reddedildi: gelir yatmış hâlde bakiye giren kullanıcıda gelir iki kez sayılır, dönem sonu iyimser şişerdi. Ödeme satırlarının eski kuralı (gözlem günü düşen ödeme henüz yansımamış sayılır) değişmez. |
| **Etkiler** | `H1`, `A10`, `A11`, `A15c-2`, `I1`, `G1` |
| **İlgili** | `G1` bunu bilmeli: eski veritabanındaki açık dönem planında gelir satırı yoktur. İçe aktarıcı ya açık dönem planını yeniden dondurmalı ya da boş `IncomeLines` gidişatta tanımlı bir davranışa bağlanmalı. Bu, `G1`'in Aşama 3'ünde konuşulacak açık bir sorudur. `A15c` satırsız plan için geri dönüş kuralı **eklemez**: satırı olmayan planda gidişat geliri görmez. |
| **Durum** | uygulandı — plan yarısı S31 düzeltmesinde, gidişat yarısı `A15c-2`de |

### S32 — Mevcut dönem modelinden kopya alan ve ekran bayrakları ayıklandı

| | |
|---|---|
| **Eski** | `PeriodProgress` 24 parametreli bir record'du. `PlanFrozenOn` her zaman `PeriodStart` ile dolduruluyordu. `HasObservation`, `HasRemainingLines`, `WasRevised`, `ElapsedRatio` ve `HasDeficitFinancing` yalnız ana sayfanın hangi bölümü göstereceğine karar veriyordu. Ayrıca `ProjectedEndingSavings` (`S13`) ve `RemainingLivingBudget` (`S16`) yasaklı terimleri taşıyordu. |
| **Neden yanlış** | `PlanFrozenOn` ayrı bir bilgi gibi görünen bir kopya: plan her zaman dönem başında dondurulduğu için başka bir değer alamıyor, ama okuyan kişi iki tarihin farklı olabileceğini sanıyor. Görünürlük bayrakları ise bir ekran kararı; eskide ViewModel test edilemediği için Application modeline sığınmışlardı. `Mizan.Presentation` test edilebildiği için bu gerekçe kalmadı. |
| **Yeni** | `PlanFrozenOn` taşınmaz; ekran `PeriodStart`'ı kullanır. Görünürlük bayrakları `V3` ViewModel'inde türetilir. İş kuralı taşıyan türetmeler (`LivingOverspend`, `EndingDeviation`, `DeficitInterestDeviation`, `IsSnoozed`) modelde kalır. Adlar: `ProjectedEndingBalance`, `RemainingVariableExpenseAllowance`. |
| **Etkiler** | `A15c-2`, `V3` |
| **Durum** | uygulandı |

### S33 — Gözlem defterindeki açık işaret plan revizyonundan sonra da geçerli kalır

| | |
|---|---|
| **Eski** | `PeriodProgressService.Build`, gözlem defterindeki açık işareti (`PeriodObservationPayment.PeriodPlanPaymentLineId`) güncel plan satırlarıyla **satır kimliği** üzerinden eşliyordu. Hatırlatıcı cevapları ise aynı metotta kaynak + vade anahtarıyla eşleniyordu; yorum sebebini söylüyordu: *"revizyon satır kimliklerini yenilediği için kimlikle eşlenmez"*. |
| **Neden yanlış** | Aynı sebep açık işarete de geçerli, ama işaret o dersi almamış. v2'de de revizyon her satıra yeni kimlik veriyor (`HistoricalPlanRevisionService`). Kullanıcı kirayı "ödendi" işaretler, sonra kart ödeme tercihini değiştirir ve bir revizyon doğar: işaret hiçbir güncel satırla eşleşmez, kira yeniden "kalan"a düşer ve dönem sonundan ikinci kez çıkarılır. Hata sessizdir; kullanıcı yalnız dönem sonunun kötüleştiğini görür. |
| **Yeni** | İşaret, konduğu satırın kaynak + vadesine çevrilir (hatırlatıcının `PaymentReminderPlanner.DueKey`'i) ve güncel satırlarla bu anahtarla eşlenir. Aynı ödemeye birden fazla plan sürümünde işaret konmuşsa en yeni sürümdeki geçerlidir. Domain modeli değişmez. Revizyon ödemenin vadesini de değiştirdiyse eşleşme bulunamaz ve işaret yok sayılır — hatırlatıcı cevaplarıyla aynı sınır. |
| **Etkiler** | `A15c-1`, `A16` *(kapanış taslağı aynı işareti okuyacak; aynı eşleşmeyi kullanmalı)* |
| **Durum** | uygulandı |

### S34 — Mevcut dönem modelinden iki alan daha ayıklandı

| | |
|---|---|
| **Eski** | `PeriodProgress.ObservedBalance`, `PeriodProgress.Observation.ObservedBalance` ile aynı değerle dolduruluyordu. `PeriodCardComparison.Difference` (güncel − planlanan) tanımlıydı. |
| **Neden yanlış** | `ObservedBalance`, `S32`'deki `PlanFrozenOn` ile aynı tuzak: ayrı bir bilgi gibi görünen bir kopya; eski ana sayfa değeri zaten `Observation` üzerinden okuyordu. `Difference` hiçbir ekranda kullanılmıyordu. |
| **Yeni** | İkisi de taşınmaz. Gözlenen bakiye `Observation.ObservedBalance`'tan okunur; kart farkı ekran isterse `V3`'te türetilir. |
| **Etkiler** | `A15c-2`, `V3` |
| **Durum** | uygulandı |

### S35 — Dönem mutabakatı ve kapanışı terminolojisi tamamlandı (Review -> Settlement, Savings -> Balance)

| | |
|---|---|
| **Eski** | `PeriodReviewService`, `FinancialReviewModels`, `PeriodReviewDraft`, `PeriodReviewAvailability`, `PeriodReviewContext`, `PeriodReviewPreview`, `FinancialReviewResult`, `SuggestedStartingSavings`, `ConfirmedStartingSavings`. |
| **Neden yanlış** | `Review` (`S12`) ve `Savings` (`S13`) yasaklı terimlerdir (`SOZLUK.md`, kural K9). `Review` yerine `Settlement`, `Savings` yerine `Balance` kullanılmalıdır. |
| **Yeni** | `PeriodSettlementService`, `PeriodSettlementDraft`, `PeriodSettlementAvailability`, `PeriodSettlementContext`, `PeriodSettlementPreview`, `PeriodSettlementResult`, `SuggestedStartingBalance`, `ConfirmedEndingBalance`. |
| **Etkiler** | `A16`, `V11` |
| **Durum** | uygulandı |

### S36 — Dönem mutabakatı uygunluk modeli ekran metninden ve kültürden arındırıldı

| | |
|---|---|
| **Eski** | `PeriodReviewAvailability.Message` servisin içinde `CultureInfo.GetCultureInfo("tr-TR")` ile biçimlendirilmiş Türkçe ekran metni taşıyordu (`"10 Eylül dönemi güncellenmeye hazır"`). |
| **Neden yanlış** | Kural 01 ve `S28` emsali uyarınca Application katmanı ekran metni ve kültür/tarih formatlama taşıyamaz. Kültür yalnız sunum kenarında (Presentation/ViewModel) geçerlidir. |
| **Yeni** | `PeriodSettlementAvailability` saf tarihleri ve bayrakları taşır (`HasCurrentSnapshot`, `IsDue`, `CurrentSnapshot`, `PendingPlan`, `LastUpdatedDate`); ekrana gösterilecek kullanıcı metni `V11` veya `V3` sunum katmanında üretilir. |
| **Etkiler** | `A16`, `V3`, `V11` |
| **Durum** | uygulandı |

### S37 — Fiilî mutabakat hareket tarihleri doğal dönemselliğe [Start, End) bağlandı

| | |
|---|---|
| **Eski** | `draft.Flows.Any(x => x.Date <= snapshot.SnapshotDate || x.Date > plan.SettlementAvailableFrom)` ve ödeme tarihleri için sol-açık, sağ-kapalı `(Start, End]` aralığı denetleniyordu. |
| **Neden yanlış** | Eski projede dönemler sol-açık modellenmişti (`S27`). Mizan v2'de dönemler `[PeriodStart, PeriodEnd)` yarı açık aralığıdır (`S18`). |
| **Yeni** | Dönem içi fiilî akış ve ödeme tarihleri `x.Date < plan.PeriodStart || x.Date >= plan.PeriodEnd` kuralıyla (`[PeriodStart, PeriodEnd)`) denetlenir. |
| **Etkiler** | `A16` |
| **Durum** | uygulandı |

### S38 — Kapanış koordinasyonu ile fiilî durum inşası ayrıştırıldı

| | |
|---|---|
| **Eski** | `PeriodReviewService` iki partial dosyaya yayılmış 452 satırlık bir monolitti; tek bir aritmetik hesaplama için `FinancialStateReconciliationService`'e bağlanıyordu (6 bağımlılık). |
| **Neden yanlış** | K3 (dosya ≤ 200, metot ≤ 40), K4 (partial yasağı) ve M3 (≤ 5 bağımlılık) kurallarının ihlali. |
| **Yeni** | Servis koordinasyonu `PeriodSettlementService` (5 dar bağımlılık, 167 satır) ve bağımsız saf yardımcı `PeriodActualBuilder` (185 satır) olarak ayrıldı. `FinancialStateReconciliationService` bağımlılığı elendi ve türetilen bakiye hesabı inşa ediciye alındı. |
| **Etkiler** | `A16` |
| **Durum** | uygulandı |

### S39 — Nihai revizyon seçimi ortaklaştırıldı (T6, M8)

| | |
|---|---|
| **Eski** | Dönemin nihai plan revizyonunu seçme kuralı (`SelectFinalRevision`, `IsValidForFinalPlan`) hem `HistoryQueryService` hem de `PeriodReviewService` içine kopyalanmıştı. |
| **Neden yanlış** | Düğüm T6 (kod kopyalama) ve Kural M8 (statik yan erişim/veri sorgusu ayrışması) ihlalidir. |
| **Yeni** | Nihai revizyon sorgusu verinin kendisine verildi: `FinancialHistoryData.FindFinalRevisions(PeriodPlanSnapshot plan)`. Hem `HistoryQueryService` hem de `PeriodSettlementService` doğrudan bu metodu çağırır. |
| **Etkiler** | `A14`, `A16` |
| **Durum** | uygulandı |

### S40 — Plan okuma ve plan yazma kesin olarak ayrıldı (Düğüm T5, Kural M4)

| | |
|---|---|
| **Eski** | `FinancialPlanQueryService` hem 7 tablodan plan okuyor hem de adı "Query" olmasına rağmen `GetFinancialPlanAsync` ve `CapturePlanningChangeAsync` içinde veritabanına başlangıç snapshot'ı ve açık plan revizyonu yazıyordu (21 çağrı noktası). |
| **Neden yanlış** | Okuma işlemi yan etki üretemez (CQRS/M4). Bir ekran planı okurken arkada veritabanına revizyon yazılması beklenmeyen yan etkilere yol açar. Ayrıca yazan servislerin okuma servisine bağımlı olması mimariyi düğümler. |
| **Yeni** | Salt okuma portu `IPlanReader` (sıfır yan etki, snapshot/revizyon yazmaz) ve plan değişikliği kayıt portu `IPlanChangeRecorder` (`RecordChangeAsync`) olarak ayrıştırıldı. CRUD servisleri okuma portunu görmez, sadece `IPlanChangeRecorder`'ı tetikler. |
| **Etkiler** | `A17`, `A18`, `A19`, `A20`, `A21` |
| **Durum** | uygulandı |

### S41 — Projeksiyon ve tavsiye sorgu cephesi elendi; PlanReader yalnız plan okur

| | |
|---|---|
| **Eski** | `FinancialPlanQueryService` içinde `GetDashboardAsync`, `GetFuturePeriodsAsync`, `GetLoanPayoffAdviceAsync`, `FindTargetPeriodAsync`, `FindTargetReachabilityAsync` gibi 8 adet hesaplayıcı delegasyon metodu vardı. |
| **Neden yanlış** | Kural M3 ve M5 ihlalidir. Bu metotlar servisi 11 bağımlılıklı bir tanrı cepheye dönüştürüyordu. Oysa `FinancialProjectionService`, `LoanPayoffAdvisor` ve `TargetAmountCalculator` zaten bağımsız ve test edilebilir servislerdir. |
| **Yeni** | Bu delegasyon metotları `IPlanReader`'a taşınmaz (Taşımama hakkı). `IPlanReader` yalnızca `GetPlanAsync` ve `GetProjectionPlanAsync` sunar; hesaplamayı yapacak servisler bu planı girdi olarak alır. |
| **Etkiler** | `A17`, `A22`, `V3`, `V8`, `V10` |
| **Durum** | uygulandı |

### S42 — 7 Depo portu M3 kuralı için kompozisyonla bağlandı

| | |
|---|---|
| **Eski** | Tek bir tanrı arayüz (`IMizanStore`) üzerinden 7 tablo okunuyordu. |
| **Neden yanlış** | v2'de `IMizanStore` kalktı (`S26`). 7 ayrı dar depo portunu doğrudan tek bir sınıfın yapıcısına koymak M3 kuralını (yapıcıda en fazla 5 parametre) ve `TypeSafetyRules.CheckTypeSizeLimits` mimari testini bozar. |
| **Yeni** | Borç enstrümanları `FinancialInstrumentReader` (4 repo: kredi, kart, vadeli plan, büyük harcama) ve gelir akışları `IncomePlanReader` (2 repo) altında toplandı; `PlanReader` ise 3 repo (ayarlar, gelir okuyucu, enstrüman okuyucu) + tarihçe + sınır çözücü olmak üzere tam 5 parametreyle M3 sınırında tutuldu. |
| **Etkiler** | `A17` |
| **Durum** | uygulandı |

### S43 — Kart yükümlülük servisi dar portlara bağlandı ve içe aktarma kalıntıları temizlendi

| | |
|---|---|
| **Eski** | `CreditCardObligationService` `IMizanStore` tanrı arayüzüne ve `IFinancialPlanQueryService` okuma cephesine bağlıydı; ekstre kaydetmede PDF içe aktarma kontrolü (`CreditCardStatementSource.PdfImport`, `ImportedAt`) yapılıyordu; servisin arayüzü bulunmuyordu. |
| **Neden yanlış** | Düğüm T10 / Kural M5 (tanrı arayüz), Düğüm T5 / Kural M4 (okuma/yazma ayrışması), Kural M3 (ViewModel'ler somut servise değil dar porta bağlanmalı) ve S21 (PDF içe aktarma elendi). |
| **Yeni** | 1. Yalnızca dar depo portu `ICreditCardRepository` ve yazma portu `IPlanChangeRecorder` enjekte edilir (toplam 4 bağımlılık ≤ 5, Kural M3).<br>2. S21 uyarınca `Source` ve `ImportedAt` kontrolleri kaldırıldı.<br>3. `ICreditCardObligationService` arayüzü `Abstractions/` altına eklendi. |
| **Etkiler** | `A18`, `A20`, `V7`, `MauiProgram.cs` |
| **Durum** | uygulandı |

### S44 — Simülasyon uygulama sorumluluğu dar yazıcı kompozisyonuna bağlandı

| | |
|---|---|
| **Eski** | `IMizanStore.ApplySimulationBatchAsync(SimulationPersistenceBatch)` tanrı depoda tek bir monolitik batch çalıştırıyordu; `SimulationPersistenceBatchBuilder` ise eski şema modellerini taşıyordu. |
| **Neden yanlış** | Kural M5 / Düğüm T10 (tanrı arayüz) ve S25 (batch ve tanrı arayüz elendi). Ayrıca 6 dar repo doğrudan servise enjekte edilirse M3 kuralı (yapıcıda en fazla 5 bağımlılık) ve mimari test ihlal edilir. |
| **Yeni** | Gelir depoları `IncomePlanWriter` (2 repo), finansal enstrüman depoları `FinancialInstrumentWriter` (4 repo) altında toplandı; `SimulationPlanApplier` bu iki yazıcıyı, `ScenarioPlanBuilder`'ı ve `IPlanChangeRecorder`'ı kullanarak senaryoları mevcut plana uygular (4 bağımlılık). `SimulationWorkflowService` ise 5 bağımlılıkla (`IClock`, `IPlanReader`, `ISimulationDraftRepository`, `SimulationCalculator`, `ISimulationPlanApplier`) tam M3 sınırında kalır. |
| **Etkiler** | `A19`, `A20` |
| **Durum** | uygulandı |

### S45 — SimulationApplyDestination modelleri yasaklı terimlerden ve yapay tahsisten arındırıldı

| | |
|---|---|
| **Eski** | `SimulationApplyDestination.SalaryHistory` ve `SimulationApplyDestination.Settings`. |
| **Neden yanlış** | `Salary` (`S11`) yasaklı terimdir; `Settings` ise simülatör üzerinden `PaymentStrategyChange` (yapay tahsis) uygulandığında dönüyordu ve `S18` uyarınca yapay tahsis modu bütünüyle elendi. |
| **Yeni** | `SimulationApplyDestination.IncomeHistory` kullanılır; `Settings` enum değeri elenir (Taşımama hakkı). |
| **Etkiler** | `A19`, `V6`, `V10` |
| **Durum** | uygulandı |

### S46 — Simülasyon harcama havuzu parametresi C# ve dönem standardına uyarlandı

| | |
|---|---|
| **Eski** | `ISimulationWorkflowService.SimulateAsync` metodunda `decimal? MonthlyVariableExpenseAllowanceOverride = null` PascalCase parametre adı. |
| **Neden yanlış** | C# parametre isimlendirme kuralı camelCase olmalıdır (S10). Ayrıca yaşam harcaması artık aylık değil dönemliktir (S7, S16). |
| **Yeni** | Parametre `decimal? variableExpenseAllowanceOverride = null` olarak adlandırılır. |
| **Etkiler** | `A19`, `V10` |
### S47 — IObligationManagementService ayrıştırıldı ve odaklandı (Tanrı arayüz ve partial elendi)

| | |
|---|---|
| **Eski** | `IObligationManagementService` içinde kredi, vadeli plan, büyük harcama, kredi kartı delegasyonu, düzenli maaş, tek seferlik gelir, yapay tahsis stratejileri, kullanıcı ayarları ve onboarding tek arayüzde (25 metot, 2 partial dosya, 8 bağımlılık) toplanmıştı. |
| **Neden yanlış** | Kural K4 (`partial` yasağı), Kural K3 (dosya ≤ 200 satır), Kural M3 (≤ 5 bağımlılık) ve Kural M5 (arayüz ≤ 10 metot) ihlalidir. Ayrıca gelir bir "yükümlülük" (obligation/borç) değildir; nakit girişidir. Kart metotları zaten A18'de taşınmışken burada mükerrer delegasyon yapmak yalancı cephedir (T7). |
| **Yeni** | 1. **Yapay tahsis metotları elendi (S18):** `SaveSalaryAsync` (dönüşü), `GetInitialPaymentStrategySetupAsync`, `CompleteInitialPaymentStrategySetupAsync`, `SaveCashFlowAllocationStrategyAsync`, `DeleteCashFlowAllocationStrategyAsync` taşınmaz (Taşımama hakkı).<br>2. **Kart metotları mükerrerliği elendi (M3, M5):** Kart işlemleri doğrudan `A18`'de taşınan `ICreditCardObligationService` üzerinden yürütülür; bu servisten ayıklanır (Taşımama hakkı).<br>3. **Çekirdek borç yükümlülükleri `IObligationManagementService`'te toplandı:** Krediler, vadeli planlar ve büyük harcamalar (7 metot, 5 dar bağımlılık: `ILoanRepository`, `ITemporaryPaymentPlanRepository`, `IPlannedLargeExpenseRepository`, `LoanPayoffService`, `IPlanChangeRecorder`).<br>4. **Gelir yönetimi `IIncomePlanService` dar portuna ayrıldı:** `RecurringIncome` ve `AdHocIncome` ekleme/silme (6 metot, 3 dar bağımlılık: `IRecurringIncomeRepository`, `IAdHocIncomeRepository`, `IPlanChangeRecorder`).<br>5. **Ayarlar ve Onboarding:** `IUserSettingsService` (V13) ve `IOnboardingService` (V4) kendi ekran adımlarında odaklı dar portlar olarak kurgulanır. |
| **Etkiler** | `A20`, `V4`, `V6`, `V13`, `MauiProgram.cs` |
| **Durum** | uygulandı |

### S48 — IPeriodWorkflowService tanrı arayüzü ayrıştırıldı (Dönem Mutabakatı ve Hatırlatıcı Servisleri)

| | |
|---|---|
| **Eski** | `IPeriodWorkflowService` içinde hem dönem kapanış/gözlem operasyonları hem de bildirim/hatırlatıcı operasyonları tek bir arayüzde (16 metot, 353 satır) toplanmıştı. İçeride tanrı `IMizanStore` ve tanrı cephe `IFinancialPlanQueryService` kullanılıyordu. |
| **Neden yanlış** | Kural M5 (arayüzde en fazla 10 metot), Kural M3 (sınıfta en fazla 5 bağımlılık) ve Kural K3 (dosyada en fazla 200 satır) ihlalidir. Dönem kapatma sihirbazı ile hatırlatıcı bildirim çarkının hiçbir kesişimi yoktur; ikisinin tek sınıfta birleşmesi bağımlılıkları şişirir (7 bağımlılık) ve test edilebilirliği zedeler. |
| **Yeni** | 1. **`IPeriodWorkflowService` / `PeriodWorkflowService` (7 metot, 5 bağımlılık):** Kapanış uygunluğu, kapanış bağlamı, gözlemden kapanış taslağı derleme, önizleme, kesinleştirme (gözlem defterini silme dahil), anlık bakiye gözlemi, ara ödeme gözlemi (`IPeriodHistoryRepository`, `IPeriodObservationRepository`, `IPlanReader`, `PeriodSettlementService`, `IClock`).<br>2. **`IPaymentReminderService` / `PaymentReminderService` (8 metot, 3/5 bağımlılık):** Bildirim modu, bildirim panosu, kullanıcı yanıtları, ufuktaki yaklaşan vadeler (`IPaymentReminderRepository`, `IPeriodHistoryRepository`, `PaymentDueCollector`).<br>3. **`PaymentDueCollector`:** 35 günlük ufukta beklenen ödemeleri açık plandan ve projeksiyondan derleyen bağımsız yardımcı servis (`IPeriodObservationRepository`, `IPlanReader`, `FinancialProjectionService`).<br>4. **Yalancı delegasyon elendi (T7, M3):** `GetPeriodProgressAsync` elendi; ViewModel'ler gidişat için doğrudan `PeriodProgressService`'e bağlanır.<br>5. **Doğal dönemsellik (S20, S33, S35):** Kapanış taslağında `Flows` boş liste döner; ödeme gözlemi son plan revizyonunu da denetler; yasaklı terimler (`Review`, `Savings`) `Settlement` ve `Balance` olarak düzeltilmiştir. |
| **Etkiler** | `A21`, `V3`, `V11`, `PaymentReminderCoordinator`, `MauiProgram.cs` |
| **Durum** | uygulandı |

### S49 — Tanrı cephe MizanService taşınmaz, ViewModel'ler dar portlara bağlanır (Düğüm T7, Kural M3)

| | |
|---|---|
| **Eski** | `MizanService` (515 satır, 2 partial dosya, 40+ metot), `IMizanStore` ve 5 servisi birleştirerek 16 ViewModel'in 11'ine tek kapı olarak hizmet veriyordu. İkincil yapıcısında 16 parametre alıp içeride gizli `new` ile servis dünyası kuruyordu. |
| **Neden yanlış** | Kural M3 (tanrı cephe yasak), Kural M1 (hesaplayıcı/servis bağımlılığını new'leyemez), Kural K3 (≤ 200 satır) ve Kural K4 (partial yasağı) ihlalidir. 11 ekranın tek bir sınıfa kilitlenmesi ViewModel'lerin izole edilmesini ve test edilebilirliğini engelliyordu. |
| **Yeni** | `MizanService` bütünüyle elendi (Taşımama hakkı). Üretim kodunda hiçbir `MizanService` veya benzeri god facade sınıfı bulunamaz (`ArchitectureTests.GodFacade_Ve_MizanService_Yasak` ile zorlanır). ViewModel'ler Faz V'te doğrudan ihtiyaç duydukları dar portlara bağlanır. |
| **Etkiler** | `A22`, `V3`–`V13`, `MauiProgram.cs` |
| **Durum** | uygulandı |

### S50 — Sunum yardımcıları Presentation katmanına taşındı; yapay tahsis ve yasaklı terimler ayıklandı

| | |
|---|---|
| **Eski** | `CashFlowPeriodDetailPresenter` ve `SimulatorInsightService` `Mizan.Application` içinde yer alıyor, kültürlü para formatlamaları (`tr-TR`, `N2 TL`) ve ekran metinleri taşıyordu. İçlerinde `SalaryPeriodDetailData`, `SalaryText` (`S11`), `Savings` (`S13`) ve yapay tahsis/geçiş pencereleri (`TransitionCatchUp`, `ForwardFunded`, `PaymentWindowText` — `S18`) bulunuyordu. |
| **Neden yanlış** | Kural 01 ve 05 gereğince Application katmanı kültür, para formatlama ve ekran metni taşıyamaz. `Mizan.Presentation` MAUI görmeyen ancak sunum modellerini ve formatlamalarını üstlenen doğru katmandır. Ayrıca S18 ile elenen yapay tahsis alanları v2 projeksiyon modelinde zaten mevcut değildir. |
| **Yeni** | Sunum yardımcıları ve DTO'ları `Mizan.Presentation` katmanına taşındı. Yapay tahsis alanları ve geçiş blokları elendi. `Salary` yerine `Period`, `Savings` yerine `Surplus` kullanıldı. K3/K4 sınırları için tekil leaf tipler kurgulandı; 5'ten fazla özellik taşıyan modellerde M3 sınırına uyuldu. |
| **Etkiler** | `A23` (`A23a`, `A23b`), `V8`, `V9`, `V10` |
| **Durum** | uygulandı |

### S51 — Senaryo ve kayıt katalogları yasaklı terimlerden, yapay tahsis kalıntılarından arındırıldı ve tekil dosyalara bölündü

| | |
|---|---|
| **Eski** | `SimulationScenarioCatalog.cs` ve `FinancialRecordEntryCatalog.cs` dosyalarında dörder adet public tip tek dosyada toplanmıştı (toplam 242 satır). İçeride `Salary` (`SalaryChange`, `RecordEntryForm.Salary`, `ScenarioEntryHome.SalaryForm`, `"Maaş / gelir değişikliği"`) yasaklı terimleri ve yapay tahsis (`PaymentStrategyChange`, `PaymentStrategy`, `"Gelir kullanım düzeni"`, `ScenarioEntryHome.Settings`) yer alıyordu. |
| **Neden yanlış** | Kural K3 (bir dosyada tek public tip; dosya ≤ 200, metot ≤ 40 satır) ihlalidir. `Salary` ve `Maaş` `SOZLUK.md` uyarınca yasaklıdır (`S11`). S18 ile yapay tahsis elendiğinden Domain'de `SimulationScenarioType.PaymentStrategyChange` bulunmamaktadır (`S45`). |
| **Yeni** | 1. Her tip kendi leaf dosyasına ayrıldı (8 ayrı dosya, K3 kuralına tam uyum).<br>2. `Salary` yerine `Income` (`IncomeChange`, `RecordEntryForm.Income`, `ScenarioEntryHome.IncomeForm`) kullanıldı; kullanıcı metinlerindeki "Maaş" ifadeleri temizlendi.<br>3. `PaymentStrategyChange`, `PaymentStrategy` ve `ScenarioEntryHome.Settings` taşınmadı (Taşımama hakkı).<br>4. Kural M3 yapıcı kuralı için `ScenarioOption` ve `RecordEntryOption` `required init` özellikleri ile refactor edildi.<br>5. `SimulationDirectEntryValidator` doğrulaması `SimulationScenarioCatalog.IsDirectEntry` ile ortaklaştırıldı. |
| **Etkiler** | `A24`, `A19`, `V6`, `V10` |
| **Durum** | uygulandı |

### S52 — Yedekleme orkestrasyonu K3 ve M3 kurallarına uyarlandı, dosya kuralları saf sınıfa ayrıldı

| | |
|---|---|
| **Eski** | `BackupService.cs` (302 satır) içinde servis orkestrasyonu, dosya adlandırma, filtreleme ve saklama kotası mantığı iç içeydi. `BackupModels.cs` ve `IBackup.cs` dosyalarında 12 adet public tip tek dosyalara yığılmıştı. `RestoreAsync` içinde `profiles.HasLegacyDatabase` kontrolü vardı. Servisin kullanım senaryosu portu (`IBackupService`) yoktu. |
| **Neden yanlış** | Kural K3 (dosya ≤ 200 satır, metot ≤ 40 satır, tek public tip), Kural M3 (ViewModel'ler somut servise değil dar porta bağlanmalı), Kural M5 (arayüzde en fazla 10 metot) ve S22 (legacy tek dosya geçişi elendi). |
| **Yeni** | 1. **K3 uyumu:** `BackupService` (198 satır) ve saf kurallar sınıfı `BackupRetentionRules` (89 satır) olarak ayrıldı. 12 tip müstakil leaf dosyalarına çıkarıldı.<br>2. **M3 ve M5 uyumu:** Ayarlar ekranı ve arka plan işleri için `IBackupService` portu eklendi; Kural M5 uyarınca 10 metot sınırında tutuldu.<br>3. **S22 uyumu:** `HasLegacyDatabase` kontrolü kaldırıldı (Taşımama hakkı); yalnızca profil mevcudiyeti (`profiles.GetProfilesAsync().Count > 0`) denetlenir.<br>4. **İsimlendirme entegrasyonu:** Kopya profil adlandırması ve numaralandırma A3 adımında oluşturulan `ProfileNameValidator.GenerateUniqueName` saf yardımcısına bağlandı. |
| **Etkiler** | `A26`, `V13`, `NightlyBackupJob`, `MauiProgram.cs` |
| **Durum** | uygulandı |

### S53 — Temiz şema v1: 30 tablo, PRAGMA user_version = 1, gerçek yabancı anahtarlar ve yalancı/ölü kolonların arındırılması

| | |
|---|---|
| **Eski** | Eski SQLite şeması v1'den v17'ye kadar 17 migration zinciriyle büyümüştü. `PRAGMA foreign_keys = ON;` kapalıydı ve ilişkiler zorlanmıyordu; çocuk tablolar yetim kalabiliyordu. 16 yalancı kolon (kodda okunmayan/kullanılmayan kolonlar) ve 12 ölü kolon (eski adımlarda atılan `SalaryDay`, `StartDate`, `EndDate`, `CurrentTotalDebt`, `SchemaVersion`, `ActualSnapshotDate` vb.) tablolarda yer işgal ediyordu. Dokümandaki mekanik `29 + 2 = 31` hesabı, elenen iki tablo (`payment_assignment_strategies` — S18, `period_observation_flows` — S20) hesaba katılmadan yapılmıştı. |
| **Neden yanlış** | Temiz bir v2 kurulumunda 17 migration çalıştırılmaz. `PRAGMA user_version = 1` doğrudan temiz v1 olarak oluşturulmalıdır. Yabancı anahtarların (`FOREIGN KEY ... REFERENCES ... ON DELETE CASCADE`) aktif olmaması veri bütünlüğünü bozar ve yetim kayıtlar üretir. Kullanılmayan veya Domain kurallarıyla çelişen ölü kolonların şemada bulunması kafa karıştırır. |
| **Yeni** | 1. **30 Temiz Tablo:** 29 eski tablo - S18 `payment_assignment_strategies` - S20 `period_observation_flows` + S2/S5 `income_amount_histories` + S31 `period_plan_income_lines` + S31 `period_plan_revision_income_lines` = 30 tablo.<br>2. **İlişkisel Bütünlük:** `PRAGMA foreign_keys = ON;` zorunlu kılınmıştır; tüm ilişkiler gerçek `FOREIGN KEY` ve `ON DELETE CASCADE` ile bağlanmıştır.<br>3. **Temiz Başlangıç:** `PRAGMA user_version = 1` doğrudan set edilir; migration zincirleri taşınmaz.<br>4. **Ölü ve Yalancı Kolonlar Arındırıldı:** `StartDate`/`EndDate` (plan snapshot), `CurrentTotalDebt` (kredi kartları), `SalaryDay` (gelirler), `ActualSnapshotDate` (gerçekleşme) ve `SchemaVersion` (anlık görüntü) kolonları şemadan temizlendi; testlerle yoklukları kalkan altına alındı.<br>5. **K3 Uyumu:** DDL komutları 200 satır sınırını aşmamak için 4 odaklı iç sınıfa bölündü (`SchemaIncomeLoanTables`, `SchemaCardTables`, `SchemaSnapshotTables`, `SchemaActualAndObservationTables`). |
| **Etkiler** | `I1`, `I2`, `G1`, `DatabaseSchema`, `SqliteConnectionFactory` |
| **Durum** | uygulandı |

### S55 — Profil dosya ve veritabanı disk yerleşimi IProfileFileLayout arayüzü ile soyutlandı (Düğüm T8)

| | |
|---|---|
| **Eski** | `ProfileBackupArchive` ve diğer bileşenler dosya ve dizin yollarını almak için somut `FileSystemProfileRepository` sınıfına doğrudan bağımlıydı. |
| **Neden yanlış** | Somut sınıfa doğrudan bağımlılık bağımlılıkların tersine çevrilmesi (DIP) ilkesini bozar ve birim testlerinde dosya sistemini taklit etmeyi imkânsızlaştırır (Düğüm T8). |
| **Yeni** | `IProfileFileLayout` arayüzü (`RootDirectory`, `GetProfileDirectory`, `GetDatabasePath`) tanımlandı. `FileSystemProfileRepository` bu arayüzü uygular; yedekleme arşivi (I4) ve bağlantı anahtarı bu soyutlamaya bağlanır. |
| **Etkiler** | `I3`, `I4`, `Mizan.Infrastructure` |
| **Durum** | uygulandı |

### S56 — ProfileScopedMizanStore tanrı sınıfı elendi; profil başına dinamik bağlantı ISqliteConnectionProvider ile sağlandı

| | |
|---|---|
| **Eski** | 285 satırlık `ProfileScopedMizanStore`, `IMizanStore` tanrı arayüzünün 40 metodunu elle açık profile delege ediyordu. |
| **Neden yanlış** | `IMizanStore` tanrı arayüzü S26 ve S49 ile elenmiş ve mimari testle yasaklanmıştır. 11 dar deponun her biri için ayrı ayrı sarmalayıcı (wrapper) yazmak gereksiz kod tekrarı ve bakım yükü oluşturur. |
| **Yeni** | `SqliteProfileStoreSwitch` sınıfı `IProfileStoreSwitch` ve `ISqliteConnectionProvider` arayüzlerini uygular. `OpenAsync` ile açılan profilin SQLite bağlantısı tutulur; `CloseAsync` ile bağlantı kapatılır ve sıfırlanır. Depo sınıfları (`SqliteLoanRepository` vb.) `ISqliteConnectionProvider` üzerinden o an açık olan bağlantıya dinamik olarak erişir. Profil kapalıyken veya henüz seçilmemişken erişildiğinde `InvalidOperationException("Açık bir profil yok. Devam etmek için bir profil seç.")` fırlatılır. |
| **Etkiler** | `I3`, `Mizan.Infrastructure`, `Mizan.App` |
| **Durum** | uygulandı |

### S57 — Yedek arşivi biçim 2 ile başlar; eski uygulamanın yedeği biçim numarasıyla ayırt edilir

| | |
|---|---|
| **Eski** | `ProfileBackupArchive` (3 partial, 480 satır) manifeste `Format = 1` ve `SqliteMizanStore.CurrentSchemaVersion` (v17) yazıyordu; veritabanı girdisinin adını disk sabitinden (`FileSystemProfileRepository.DatabaseFileName` = `coinflow.db3`) türetiyordu. Manifestteki şema sürümü yazılıyor ama hiç okunmuyordu; geri yüklemede sürüm veritabanının `settings.SchemaVersion` kolonundan okunuyordu. `VACUUM INTO` hedef yolu elle tek tırnak kaçışıyla SQL metnine gömülüyordu. Parmak izi yorumu içerik tabanlı olmayı "store her açılışta ayar satırını aynı değerlerle yeniden yazıyor" diye gerekçelendiriyordu. |
| **Neden yanlış** | v2 aynı manifest alanlarını biçim 1 ile yazsaydı eski uygulamanın (`com.coinflow.mobile`, şema v17) yedeği v2'nin manifest denetiminden geçer, kullanıcı eski profillerini listede görür, seçer ve "verisi yedekte yok" gibi yanıltıcı bir hata alırdı; manifestteki v17 şema sürümü okunsaydı "daha yeni bir sürümden alınmış" diye **tersini** söylerdi. Girdi adını disk sabitinden türetmek, disk yerleşimi değiştiğinde yedek biçimini sessizce değiştirir. Yazılıp okunmayan alan `S53`'teki ölü kolonla aynı kokudur. Parmak izi gerekçesi v2'de geçersiz: şema sürümü `PRAGMA user_version`'da (`S53`), açılışta ayar satırı yazılmıyor. |
| **Yeni** | a) v2 yedeği **biçim 2** yazar; biçim 1 eski uygulamanın yedeğidir ve `I4b` onu özet aşamasında "eski uygulamanın yedeği" diye açık mesajla reddeder (içe aktarma `G1`'in işi). b) Parmak izi içerik tabanlı kalır, gerekçesi düzeltilir: dosyanın değişiklik zamanı aynı değeri yeniden kaydetmek, günlük (journal) işlemleri ve dosya kopyalamakla oynar; "değişiklik" kullanıcının verisidir. c) `VACUUM INTO ?` parametre bağlamayla çağrılır. d) Veritabanı girdisinin adı (`profiles/{id:N}/mizan.db3`) arşiv biçiminin kendi sabitidir (`BackupArchiveFormat`), disk sabitine bağlı değildir. e) Manifestteki şema sürümü `DatabaseConstants.CurrentSchemaVersion`'dır ve `I4b`'de özet okunurken erken denetimde kullanılır; asıl otorite veritabanının `user_version`'ıdır. f) Profil yokken yazma eskisi gibi denetlenmez; kural `BackupService`'tedir (`NothingToBackUp`, `A26`). |
| **Etkiler** | `I4a`, `I4b`, `G1` |
| **İlgili** | `I4c`: v2 yedeği de depolamanın üstündeki `Mizan` klasörüne ve aynı `Mizan-yedek-` önekiyle yazarsa iki uygulama aynı günün dosyasını birbirinin üzerine yazar, v2'nin "en yeni 7" temizliği eski uygulamanın yedeklerini (G1'in girdisini) siler. Klasör ve önek kararı `I4c`'nin Aşama 3'ünde verildi: `S59`. |
| **Durum** | uygulandı — `I4a` yazma yarısını (a, b, c, d, f), `I4b` okuma yarısını (a, e) uyguladı |

### S58 — Geri yükleme sürümü `user_version`'dan iki uçtan denetler; hazırlık yalnız veritabanlarını taşır, profil kaydını port yazar

| | |
|---|---|
| **Eski** | `ProfileBackupArchive.ImportAsync` veritabanının sürümünü `SELECT SchemaVersion FROM settings LIMIT 1` ile okuyor ve yalnız üst sınırı denetliyordu; ayar satırı olmayan bir veritabanı `0` döndürüp geçiyordu. Manifestte yalnız `Format > 1` reddediliyordu; biçim alanı olmayan (`0`) bir JSON "Mizan yedeği" sayılıyordu. Hazırlık klasöründe `new FileSystemProfileRepository(stagingRoot)` kurup profil meta dosyasını oraya yazıyor, sonra bütün profil klasörünü `Directory.Move` ile yerine taşıyordu. |
| **Neden yanlış** | v2'de `SchemaVersion` kolonu yok (`S53`), sürüm `PRAGMA user_version`'dadır. Sürümü `0` olan bir veritabanını (boş bir SQLite dosyası ya da eski uygulamanın verisi) `DatabaseSchema.EnsureInitializedAsync` "yeni veritabanı" sanar, tabloları üstüne kurar ve profil **boş** açılır: kullanıcı verisinin kaybolduğunu sanır. Biçim alanı olmayan bir dosya Mizan yedeği değildir; biçim 1 ise artık eski uygulama demektir (`S57`). Somut depoyu `new`lemek `S55`'in kapattığı bağımlılığı geri açar (M1) ve meta dosyasının biçimini iki yerin bilmesi demektir. |
| **Yeni** | 1. Geri yüklenen her veritabanında `1 ≤ user_version ≤ DatabaseConstants.CurrentSchemaVersion` aranır: büyükse "daha yeni sürüm", küçükse "profilin verisi tanınmıyor".<br>2. Manifestte `Format ≤ 0` "Mizan yedeği değil", `1` "eski uygulamanın yedeği" (şema sürümünden **önce** bakılır; eski yedek v17 taşıdığı için yoksa "daha yeni" denirdi), `> 2` "daha yeni sürüm".<br>3. Hazırlık klasörüne (`.restore-{guid}`) yalnız veritabanları çıkarılır ve doğrulanır. Hepsi geçerse ve hedef klasörlerin hiçbiri yoksa her profil için önce veritabanı `IProfileFileLayout.GetDatabasePath` yerine taşınır, sonra meta `IProfileRepository.SaveProfileAsync` ile yazılır; herhangi bir adım başarısız olursa bu çağrıda oluşturulan profil klasörleri silinir. **Sıra bilerek önce veritabanı, sonra meta:** süreç tam arada öldürülürse `I3`'ün kurtarma kuralı profili verisiyle "Profilim" adıyla listeler, veri kaybolmaz; ters sırada adı doğru ama verisi boş bir profil kalır, kullanıcı geri yüklemenin başarılı olduğunu sanırdı. |
| **Etkiler** | `I4b`, `G1` |
| **Durum** | uygulandı |

### S59 — v2 yedeği eski uygulamanın yedeklerinden dosya adının önekiyle ayrılır; klasör deposu bütün dosyaları listeler

| | |
|---|---|
| **Eski** | Yedekler depolamanın en üstündeki `Mizan` klasörüne `Mizan-yedek-yyyy-MM-dd.zip` adıyla yazılıyordu. `FolderBackupStorage.ListAsync` klasördeki `*.zip` dosyalarını listeliyor, `BackupService` bunları `StartsWith("Mizan-yedek-", Ordinal)` süzgecinden geçirip en yeni 7'sini tutuyor, gerisini siliyordu. Klasör adı `AndroidStorageAccess.FolderName`'deydi; aynı telefonda yan yana kurulan geliştirme sürümü için ayrı klasör (`Mizan Dev`) kullanılıyordu. |
| **Neden yanlış** | Eski uygulama (`com.coinflow.mobile`) ile v2 (`com.mizan.app`) yan yana kurulacak (`02-NEDEN-BU-YAPI`, "Neden yeni app id") ve eski uygulama değiştirilemez (`S17`). v2 aynı klasöre aynı önekle yazsaydı: aynı günün dosyası iki uygulama arasında karşılıklı ezilir; v2'nin "en yeni 7" temizliği eski uygulamanın yedeklerini — `G1`'in girdisini — siler; eski uygulamanın temizliği de v2'nin yedeklerini siler. Android'in paylaşılan depolaması harf büyüklüğüne duyarsız olduğu için yalnız büyük/küçük harfle ayrışan bir ad (`Mizan-Yedek-`) aynı dosyadır, çözüm değildir. Ayrıca yalnız `*.zip` listelemek, hangi dosyanın yedek olduğuna Infrastructure'da karar vermekti (M7) ve port belgesiyle ("tüm dosyaları listeler") çelişiyordu; Android'de `.ZIP` uzantılı dosya listelenmiyor ama Application süzgeci onu kabul ediyordu. |
| **Yeni** | a) v2 yedeğinin öneki **`Mizan-yedegi-`**'dir (`Mizan-yedegi-2026-09-26.zip`). Kural: iki önekten hiçbiri diğeriyle başlamaz, karşılaştırma harf büyüklüğü gözetmeden yapılır; böylece iki uygulamanın süzgeci birbirinin dosyasını hiç görmez — listelemez, ezmez, silmez. Ayrım klasörde değil önekte yapılır, çünkü iki uygulamanın listeleme ve temizliği klasöre değil öneke bakıyor, önek Application'da ve bugün test edilebiliyor (klasör adı App katmanında, V0'a kadar hiçbir test koruyamaz), dosyalar klasörler arasında taşınsa da ayrım bozulmuyor. Yalnız ayrı klasör (test edilemez) ve `Mizan2-yedek-` (kalıcı ada geçici "2" gömer) reddedildi. Eski önek G1'e kadar üretim kodunda yer almaz; testte metin sabitidir. b) `FolderBackupStorage` klasördeki bütün dosyaları listeler; hangisinin Mizan yedeği olduğuna `BackupRetentionRules.OnlyMizanBackups` karar verir. c) `IStorageAccess` kendi dosyasına ayrıldı (K3), ikisi `Mizan.Infrastructure.Backup` altında. |
| **Etkiler** | `I4c`, `A26` (`BackupRetentionRules.FilePrefix` — onaylı katman istisnası), `V0`, `V13`, `G1` |
| **İlgili** | Klasör adı V0/V13'ün (`AndroidStorageAccess`, kompozisyon kökü) kararıdır; önekler ayrıldığı için v2'nin de `Mizan` klasörünü paylaşması güvenlidir ve `G1` eski yedekleri aynı yerde bulur. v2'nin geliştirme ve kararlı sürümü aynı telefonda yan yana kurulacaksa ikisi aynı öneki paylaşır; aralarındaki ayrım yine klasörle (eskideki `Mizan Dev`) yapılmalıdır. |
| **Durum** | uygulandı |

### S60 — Sentry taşınmadı; telemetri cihazdan çıkmaz, uygulama ağ izni istemez ve bulut yedeğine kapalıdır

| | |
|---|---|
| **Eski** | Üç dosya, 122 satır. `SentryTelemetryService` ile `SentryPiiMasker` App katmanındaydı, `NullTelemetryService` Infrastructure'da. `UseSentry` şu ayarlarla çağrılıyordu: DSN `Environment.GetEnvironmentVariable("MIZAN_SENTRY_DSN")`, yoksa Sentry'nin örnek adresi `https://examplePublicKey@o0.ingest.sentry.io/0`; `TracesSampleRate = 1.0`; `AttachScreenshot = true`. Maske yalnız `event.Message.Formatted` alanına para birimli tutar regex'i uyguluyordu. `ITelemetryService` yalnız DI'a kaydediliyordu. Manifestte internet izni yazılı değildi. |
| **Neden yanlış** | a) **Hiç çalışmadı.** Android'de uygulamaya ortam değişkeni verilmez ve repoda DSN'i dolduran bir derleme adımı yoktu. Olaylar hep örnek adrese gitti; hiçbiri bir yere ulaşmadı. b) **Hiç çağrılmadı.** Port yalnız DI'a kaydediliyordu; hiçbir sınıf onu enjekte etmiyordu. c) **İnternet izninin tek sebebiydi.** `sentry-android-core-7.16.0.aar`'ın manifesti `android.permission.INTERNET` bildiriyor ve bu izin birleşmede uygulamaya geçiyordu. "Bulut yok" diyen (`MIMARI.md`) bir finans uygulaması yalnız bu yüzden internete çıkabiliyordu. d) **Maske baktığı tek alanda da bozuktu.** Aynı regex ile denendi: `1.500 TL` maskelendi; `15000 TL`, `1.500 ₺`, `₺1.500`, `amount=1500` maskelenmedi. `\b`, `₺`/`$`/`€` sembollerinin yanında sınır bulamıyor. Oysa port `details` alanında tasarım gereği çıplak sayı taşır. İstisna metni, breadcrumb, extra, ekran görüntüsü, performans izleri ve Android'in kendi (Java/NDK) çökme olayları hiç maskelenmiyordu. e) Maske App katmanında olduğu için hiçbir test projesi onu göremiyordu; bu yüzden bozukluğu kimse fark etmedi. f) Kullanıcının girdiği adlar (profil, kredi etiketi) hiçbir regex'le tanınamaz. Maskeleme ilkesel olarak eksik kalır; tam garanti yalnız göndermemektir. |
| **Yeni** | a) Sentry taşınmaz. `Sentry.Maui` merkezî paket listesinden silindi. Hiçbir proje Sentry'ye başvuramaz (`ArchitectureTests.MerkeziPaketler_SentryIcermez`). b) Telemetri portunun tek adaptörü `Mizan.Infrastructure.Telemetry.NullTelemetryService`'tir: bildirimi yutar ve asla istisna fırlatmaz. Port duruyor, çünkü ekranların hata yakalayan blokları (K5) bildirimi bir yere vermek zorunda. Eskideki statik `Instance` taşınmadı (M8); DI kaydı V0'da. c) Manifestten `INTERNET` ve `ACCESS_NETWORK_STATE` (MAUI şablon kalıntısı) silindi, `allowBackup="false"` yapıldı. Eski uygulamada da `false` idi. v2'nin şablonundaki `true`, profil veritabanlarını Google hesabına otomatik yedekletirdi. Onaylı katman istisnası: manifest App katmanında, ama onu koruyan invaryant (`I40`) bu adımda doğdu. d) Doğrulandı: Debug derlemesinde .NET, hata ayıklayıcı için INTERNET'i kendisi ekliyor. Release derlemesinin birleşmiş manifestinde INTERNET yok ve `allowBackup="false"`. |
| **Etkiler** | `I6`, `A27` (port değişmedi, adaptörü artık tek), `V0` (DI kaydı: `AddSingleton<ITelemetryService, NullTelemetryService>()`; `UseSentry` yok), `K2` |
| **İlgili** | Kaynak manifest testi, bir kütüphanenin derlemede kendiliğinden eklediği izni göremez; eski uygulamada Sentry tam olarak bunu yapıyordu. Birleşmiş release manifestinde ağ izni olmadığını denetlemek `K2`'nin APK doğrulamasına eklenmeli. Android 12 ve üstünde `allowBackup="false"`, cihazdan cihaza aktarımı (D2D) kapatmaz; bu kullanıcının kendi telefonları arasındadır, istenirse V0'da `dataExtractionRules` ile kapatılabilir. Play "Veri güvenliği" formu "veri toplanmıyor" olarak doldurulabilir. KVKK md. 9'daki yurt dışına aktarım sorusu hiç doğmaz. Yayındaki çökmeler Play Console'un Android vitals ekranında SDK olmadan görünür. Bedeli: telefonda yakalanıp susturulan hatalar görünmez. |

### S61 — Kart kontrol: elle ekstre girişi korunur; geçmiş, döküm ve harcama girişi ekrandan çıkar

| | |
|---|---|
| **Eski** | `CardControlPage.xaml` (491 satır, 73 `<Label>`) ve `CardControlViewModel` (866 satır, 3 partial). Kart `LoadAsync(cardId)` içinde `Single` ile aranıyordu; bulunamazsa istisna fırlıyordu. Ekranda kesilmiş ekstre (PDF'den içe aktarma veya elle giriş taslağı), ekstre ödeme kararı, ödeme tercihi geçmişi, "Devreden • finansman • bilinen yeni harcama" döküm cümlesi, 6 gelecek ekstrenin her satırında üç düğme, gelecek kart harcamaları formu (ekle / sil / kaydet), kartın varsayılan ödeme şekli ve "karar vermediğin ekstrelerde varsayım" vardı. Sayfa yalnız `CommitmentsPage` ve `SimulationPage`'ten kart kimliğiyle açılıyordu. |
| **Neden yanlış** | a) Kart bulunamayınca sayfa çöküyordu (profil değişince ya da kart silinince). b) Ödeme tercihi geçmişi ve döküm cümlesi hiçbir kullanıcı sorusuna bağlanmıyor (`EK-V7` Aşama 4). c) Gelecek harcamalar kartın **tanımına** aittir; kart kontrolünde bir karar değil veri girişidir. d) V7'nin ilk denemesi `S21`'i "ekstre girişi yok" diye okuyup elle girişi de çıkardı; kurulumda eklenen kartlar ekstresiz yazıldığı için ödeme kararı hiç görünmedi. `S21` yalnız PDF'den okumayı eledi. |
| **Yeni** | 1) Ekranın merkezi **sıradaki ödemedir**: kesilmiş ekstre varsa onun, yoksa kartın döngüsünden hesaplanan tahmini ekstrenin vadesi ve tutarı. Asgari / Tamamı / Özel kararı iki durumda da verilir: kesilmiş ekstrede ekstrenin planına, tahminde o vadeye özel plana yazılır (`SetStatementPaymentModeAsync`, `SaveCreditCardPaymentPlanAsync`). Eskide karar yalnız kesilmiş ekstrede veriliyordu. 2) Kararın bedeli görünür: ödeme ekstreden azsa devreden tutar ve sonraki ekstreye binen faiz tek satırda. 3) Elle ekstre girişi kalır: tutar, asgari, kesim ve son ödeme tarihi; PDF yok (`S21`). Yeni ekstrenin ödeme şekli kartın varsayılanından başlar (Asgari → Asgari, Tamamı → Tamamı, Sabit tutar → Özel, Her ekstrede sor → Asgari). 4) Sayfa kimliksiz açılırsa en yakın ödemesi olan kart açılır; hiçbirinde ödeme yoksa ilk kart, kart yoksa boş durum. Kimlik bulunamazsa çökme yok. 5) Tutar ve tarih kuralları tek yerde, `CreditCardValidator`'da kalır; ekran ihlali diyalogla gösterir. 6) Ödeme tercihi geçmişi, döküm cümlesi ve toplam borç / limit çubuğu ekrandan çıkar; limit tek satıra iner. Tarihçe kaydı ve veri değişmez. 7) Gelecek harcama girişi `V6`'ya, kart düzenlemenin yanına taşınır. 8) `V6` gelene kadar ekran sol menüdeki **geçici** bir öğeyle açılır; `V6` bu öğeyi kaldırır ve sayfayı `Routes.CardControl` + `cardId` ile açar. |
| **Etkiler** | `V7`, `V6`, `EK-V7` |
| **Durum** | uygulandı (V7: 1–6 · V6a: 8 · V6b2: 7) |

### S62 — Finansal yapı: satır içi form ve karışık gruplar yerine dört gruplu liste; satır aksiyonu tek diyalogda

| | |
|---|---|
| **Eski** | `CommitmentsPage.xaml` (450 satır, 86 `<Label>`, 19 buton, 22 `Entry`, 11 seçici, 2 spinner) ve `CommitmentsViewModel` (1.344 satır, 6 `partial`). Tek sayfada beş grup liste (gelir, kart, kredi + erken ödeme, düzenli ödeme, tek seferlik/geçici ödeme), satır başına Kartı Aç / Düzenle / Sil düğmeleri, "+ Ekle" ile açılan 4 gruplu 11 seçenekli seçici, beş türde satır içi form, PDF ekstre okuma ve ilk ödeme düzeni penceresi vardı. Kart satırının tutarı kesilmiş ekstre tutarıydı; ekstre yoksa "—". Kredi satırı faiz ve kapatma bedeli cümlesi taşıyordu. `EditCardAsync` / `EditLoanAsync` kaydı `Single` ile arıyordu. |
| **Neden yanlış** | a) Liste ile beş formun aynı sayfada durması ekranı 86 etikete çıkardı; hiçbir form listedeki soruyu cevaplamıyor. b) Kurulumdan gelen kartlarda ekstre olmadığı için kart satırları "—" gösteriyordu; kartın ne kadar tutacağı cevapsızdı (V7'deki hatanın listedeki karşılığı). c) Pasif gelir, tarihi geçmiş tek seferlik gelir, tamamlanmış plan ve harcama, plana girmedikleri hâlde plandaymış gibi listeleniyordu. d) "Düzenli" ile "tek seferlik / geçici" ayrımı plan türünden (`PaymentPlanKind`) geliyordu; kullanıcıya bir şey söylemiyor. e) Kayıt bulunamazsa sayfa çöküyordu (`S61`-a ile aynı). f) Aynı ekranın kodda iki adı vardı: `Commitments` (rota, menü kimliği, komut) ve "Finansal Yapı". |
| **Yeni** | 1) Ekran dört gruplu **listedir**: Gelirler, Kartlar, Krediler, Ödemeler (ödeme planları ile planlı büyük harcamalar birlikte). Boş grup görünmez; hiç kayıt yoksa boş durum. 2) Formlar ayrı sayfalara iner ve alt adımlarda gelir: `V6b` kart (+ gelecek kart harcamaları, `S61`-7), `V6c` kredi (+ faiz, bugün kapatma bedeli, planlı erken ödemeler), `V6d` gelir, `V6e` ödeme planı ve büyük harcama. Her form adımı kendi türünü "Ekle" seçicisine, satır diyaloğuna da "Düzenle" olarak ekler. 3) Satıra dokunmak **tek diyalog** açar. Kart: "Ödemeyi yönet" (`Routes.CardControl` + `cardId`) ve "Sil". Diğer türler: "Sil". Silme önce onay sorar ve türüne göre dar portla yapılır (`ICreditCardObligationService`, `IObligationManagementService`, `IIncomePlanService`). 4) Kart satırının tutarı **sıradaki ödemedir**; kart kontroldeki hero rakamla aynı hesap (ekstre yoksa tahmini). Toplam borç ve limit listede yok. 5) Listede yalnız plana giren kayıtlar durur: aktif gelir, bugün ve sonrası tek seferlik gelir, aktif kart, taksiti kalmış aktif kredi, tamamlanmamış plan, `Planned` durumundaki büyük harcama. Taksiti hiç olmayan plan tutarsız satır olarak görünür ve silinebilir. 6) Kayıt bulunamazsa çökme yok; liste yeniden yüklenir. 7) Ortak senaryo formu (`SharedForm`) `V10`'da simülatörle birlikte doğar; Finansal Yapı'ya eklenip eklenmeyeceği orada kararlaştırılır. PDF (`S21`) ve ilk ödeme düzeni penceresi (`S18`, `D10`) taşınmaz. 8) Ad birliği: `Routes.Commitments` → `Routes.FinancialStructure` (`//financial-structure`); menü kimliği ve ana sayfa komutu da. 9) Sol menüdeki geçici "Kart Kontrol" öğesi ve `cards` rotası kalkar (`S61`-8). |
| **Etkiler** | `V6a`–`V6e`, `V3` (komut adı), `V7` (açılış yolu), `V10`, `V5`, `EK-V6` |
| **Durum** | uygulandı (`V6a`: 1, 3–6, 8, 9 · `V6b1`: 2'nin kart kısmı — "Ekle" ve "Düzenle" · `V6c1`: 2'nin kredi kısmı · `V6d1`: 2'nin düzenli gelir kısmı); 2 `V6d3`–`V6e`'de, 7 `V10`'da açık |

### S63 — Kart formu: kartın tanımı ayrı sayfada; ekstre ve ödeme kararları kart kontrolde; gelecek harcama taksitle girilir

| | |
|---|---|
| **Eski** | `CommitmentsPage.xaml`'deki satır içi kart formu (158 satır; 28 `<Label>`, 7 buton, 20 giriş) ve `CommitmentsViewModel.Cards` / `.Entry` / `.Plans`. Form kartın tanımını (ad, banka, limit, kesim ve son ödeme günü, asgari %) kesilmiş ekstre girişiyle (PDF dahil), o ekstrenin ödeme planıyla, sonraki tahmini tarihlerle, devreden bakiye + ekstreleşmemiş + bakiye tarihi üçlüsüyle ve varsayılan ödeme şekli + sabit tutar + "karar vermediğin ekstrelerde varsayım" ayarlarıyla karıştırıyordu. Gelecek kart harcaması yalnız tarih ve tutarla ekleniyordu, açıklaması hep "Gelecek taksit"ti; taksitli bir alışveriş taksit taksit giriliyordu. Kart kontrolde açıklamalı ikinci bir harcama formu vardı. `EditCardAsync` kartı `Single` ile arıyordu. |
| **Neden yanlış** | a) Ekstre ve ödeme kararları `S61` ile kart kontrole taşındı; formda da durmaları aynı kararın iki yerden değişmesi demek. b) Kart bulunamazsa sayfa çöküyordu (`S61`-a, `S62`-e). c) Asgari oran BDDK kuralına bağlıdır (`I6`) ama elle soruluyordu; kurulum (`V4`) ise hiç yazmıyor, oran 0 kalıyor ve tahmini asgari 0 ₺ çıkıyor. d) "Güncel borç" faizli devreden bakiyeye (`CarriedBalance`, sözlükte "ödenmeyip faize kalan tutar") yazılırsa ilk tahmini ekstreye akdi faiz biner; ekstresi kesilmemiş kartta borç faizsiz dönem içi harcamadır. `V4` bugün böyle yazıyor. e) 12 taksitli bir alışveriş 12 ayrı girişti; açıklamasız liste hangi harcamanın ne olduğunu söylemiyordu (gerçek veride bir kartta 11 farklı açıklamalı harcama var). |
| **Yeni** | 1) Kart formu ayrı sayfadır (`CardFormPage`, `Routes.CardForm`): kimliksiz açılırsa yeni kart, `cardId` ile açılırsa düzenleme. Finansal Yapı başlığındaki "Ekle" tek seçenekli ("Kredi kartı") seçiciyi açar; `V6c`–`V6e` kendi seçeneklerini ekler. Kart satırının diyaloğuna "Düzenle" gelir. 2) Form yalnız kartın tanımını tutar: ad, banka, limit, kesim günü, son ödeme günü, güncel borç. Ekstre, ödeme şekli ve varsayım `EK-V7`'dedir. Yeni kartın ödeme şekli "Tamamı"dır (`V4` ile aynı). 3) Asgari oran sorulmaz: eklemede limitten çözülür (`CreditCardRules`, `I6`); düzenlemede kayıtlı oran korunur, limit değişirse ya da oran 0 ise limitten yeniden çözülür. 4) Güncel borç yalnız kesilmiş ekstre yokken görünür ve devreden bakiye + ekstreleşmemiş harcamanın toplamını gösterir. Değer değiştiyse faizsiz ekstreleşmemiş harcama (`UnbilledSpending`) olarak yazılır, devreden bakiye 0 ve bakiye tarihi bugün olur; değişmediyse üçü de korunur. 5) Gelecek harcama: açıklama (zorunlu), aylık tutar, taksit sayısı (1–120, varsayılan 1), ilk taksit tarihi (bugünden önce olamaz). Taksit sayısı birden fazlaysa her ay aynı tutarla N harcama yazılır; adları "Açıklama (i/N)" (`ScenarioPlanBuilder` adlandırması), tarihleri `CalendarRules.AddMonthsKeepingDay`. Satıra dokunmak diyalogla siler. 6) Kaydet kartı ve harcamaları tek `SaveCreditCardAsync` ile yazar; ekstre, vade planları, tercih geçmişi ve bilinen sonraki tarihler korunur; sonra Finansal Yapı'ya dönülür. 7) Düzenlenecek kart bulunamazsa diyalog ve geri dönüş; çökme yok. 8) Alan ve tutar kuralları `CreditCardValidator`'dadır; ihlal diyalogla gösterilir. PDF yok (`S21`). 9) Açık dönemin içine tarihlenen harcama ana sayfa tahminine girmez (`I16`, `I24`: dönem içi kart harcaması yaşam giderinden sayılır), kart kontrolün sıradaki ödemesine girer; bu adım motoru değiştirmez. |
| **Etkiler** | `V6b1`, `V6b2`, `EK-V6`, `EK-V6b`, `V4` (aynı iki hata — oran 0, faizli borç — ayrı düzeltme oturumunda) |
| **Durum** | uygulandı (`V6b1`: 1–4, 6–8 · `V6b2`: 5, 9) |

### S64 — Kredi formu: ödeme günü tarihten çözülür, düzenleme kaydı korur, faiz canlı görünür, erken ödemeler krediyle birlikte kaydedilir

| | |
|---|---|
| **Eski** | `CommitmentsPage.xaml`'deki satır içi kredi formu (35 satır; 11 `<Label>`, ortak başlık / açıklama / ad alanıyla 14; 9 giriş) ve `CommitmentsViewModel.Loans` (120 satır). Form ödeme gününü ve "İlk / sonraki ödeme tarihi"ni iki ayrı alanda soruyordu; kalan anapara ve bankanın erken kapama tutarı için üç yardım cümlesi vardı. Faiz ve bugün kapatma bedeli yalnız listedeki satırda iki satırlık cümleydi. Planlı erken ödemeler Krediler listesinde ayrı satırlardı; "+ Ekle" → "Krediye erken ödeme" ortak senaryo formundan ya da simülatörden giriliyordu. `EditLoanAsync` krediyi `Single` ile arıyor, `BuildLoan` krediyi sıfırdan kuruyordu. |
| **Neden yanlış** | a) Gün ve tarih ayrı alanlar birbiriyle çelişebilir: gün 15, tarih 20 → ilk taksit ayın 20'sinde, sonrakiler 15'inde; motor bunu sessizce kabul eder. b) `BuildLoan` yeni bir `Loan` kuruyordu: `FinalPaymentAmount` (vade kısaltan ara ödemeden sonra küçülen son taksit) ve `IsActive` düşüyordu; yalnız adı düzeltmek bile son taksiti büyütüyordu. c) Kredi bulunamazsa sayfa çöküyordu (`S62`-e). d) Faiz ve kapatma bedeli ancak kaydedip listeye dönünce görünüyordu; kullanıcı girdiği anaparanın ya da tutarın taksitlerle uyuşup uyuşmadığını formda göremiyordu. İki alan birlikte doluyken anapara sessizce kapatma tutarından çözülenle eziliyordu. e) Erken ödeme kredinin sayfasından ayrı, kredisi seçilerek giriliyordu; hangi kredinin hangi planı olduğu listede dağınıktı. f) Kurulum (`V4`) krediyi sonraki taksit tarihi = kurulum günü ile yazıyor (`OnboardingDraftBuilder.CreateLoan`) ve anapara sormuyor: ilk taksit kurulum gününe düşüyor, faiz hiç çözülemiyor. g) *(V6c2 Aşama 1)* Faiz görünümü (`LoanPayoffService.Describe` → `Analyze`) bankanın kapatma tutarı taksitlerle uyuşmayınca sessizce kalan anaparaya düşer; kayıt (`PrepareForSave`) aynı krediyi reddeder. Canlı kart `Describe`'a bağlansaydı "faiz %2,1" derken Kaydet hata verirdi. h) Form ViewModel'i 5 bağımlılıkta; kapatma servisi altıncı olurdu (Kural M3). i) *(V6c3 Aşama 1)* `SqliteLoanRepository.UpsertLoanAsync` krediyi `INSERT OR REPLACE` ile yazıyor; SQLite çakışan satırı silip yeniden eklediği için yabancı anahtarın `ON DELETE CASCADE`'i tetikleniyor ve kaydedilen kredinin erken ödemeleri siliniyor (sqlite3 ile doğrulandı). 3'teki "erken ödemeler korunur" bu yüzden bugün tutmuyor. Kredi ve erken ödemeler ayrı ayrı yazılırsa kredi yazıldığı anda erken ödemeler gider. j) `LoanPrepaymentValidator` faiz çözülemeyince "Finansal Yapı'dan krediyi düzenle" diyor. Bu simülatörde doğru, kredinin kendi formunda yanlış bağlam. k) `ObligationManagementService` 5 bağımlılıkta; doğrulayıcı altıncı olurdu (M3). `LoanPayoffService` 166 satır; eklenecek önizleme ve doğrulama K3'ün 200 sınırını aşar. |
| **Yeni** | 1) Kredi formu ayrı sayfadır (`LoanFormPage`, `Routes.LoanForm` + `loanId`): kimliksiz yeni kredi, kimlikle düzenleme. Finansal Yapı'daki "Ekle" seçicisine "Kredi", kredi satırının diyaloğuna "Düzenle" gelir. 2) Tanım: ad (zorunlu), banka, aylık taksit, kalan taksit (≥ 1), sonraki taksit tarihi, kredi türü (İhtiyaç / taşıt · Konut, sabit faiz · Konut, değişken faiz). **Ödeme günü sorulmaz**, sonraki taksit tarihinin günüdür; düzenlemede kayıtlı gün o ayda aynı tarihe kenetleniyorsa korunur (31 → 30 Eylül). Geçmiş tarih serbesttir (dönem kapanışı tarihi ilerletir, `I4`). Yeni kredide tarih bir ay sonrası, tür İhtiyaç / taşıttır. 3) Düzenleme yüklenen kredinin üstüne kurulur: aktiflik ve erken ödemeler korunur; son taksit farkı (`FinalPaymentAmount`) aylık taksit ve kalan taksit değişmedikçe korunur, biri değişirse sıfırlanır. 4) Kalan anapara ve bankanın bugünkü kapatma tutarı isteğe bağlıdır; ikisi birden girilirse kapatma tutarı otoritedir (`I25`). Düzenlemede yalnız biri dolu gelir: kayıtlı kapatma tutarı güncelse (son ödenen taksitten önce alınmamışsa) o, değilse kalan anapara. Tutarın tarihi sorulmaz: değiştiyse bugünle, değişmediyse kendi tarihiyle saklanır. 5) "Bugün kapatırsan" formun o anki değerlerinden **canlı** hesaplanır: aylık faiz (vergi dahil), bugün ödenecek tutar, varsa içindeki erken ödeme ücreti, kurtulunacak faiz. Faiz çözülemezse tek cümle; engel metni sunumda kurulur (`S28`). 6) Planlı erken ödemeler kredinin sayfasında tarih sırasıyla, o günkü tutarıyla (form değerleriyle yeniden hesaplanır) listelenir. Giriş: tarih, şekil (tamamen kapat · ara ödeme, vade kısalsın · ara ödeme, taksit azalsın), ara ödemede anaparadan düşecek tutar. Satıra dokunmak diyalogla siler. Giriş ve silme Kaydet'e kadar bekler (`S63`-6 ile aynı); Kaydet krediyi ve erken ödemelerini **tek seferde** yazar (tek plan revizyonu) ve erken ödemeleri kredinin yeni hâline göre `LoanPrepaymentValidator` ile yeniden doğrular. 7) Düzenlenecek kredi bulunamazsa diyalog ve geri dönüş. Ad, taksit ve kalan taksit Presentation'da (`OnboardingValidator.ValidateLoan`), kalan kurallar `SaveLoanAsync` / `PrepareForSave`'de doğrulanır; ihlal diyalogla gösterilir. 8) Taşınmayanlar: form açıklaması ve üç yardım cümlesi, "banka tutarından" işareti, başarı mesajı, erken ödemenin "plan adı" ve "Simülatörden uygulandı" notu. Erken kapama önerisi (`LoanPayoffAdvisor`) `V8`'e aittir (eskide 12 dönem ekranı). Ortak senaryo formunun yeri `V10`'da kararlaştırılır (`S62`-7); kredi erken ödemesi artık kredinin sayfasında girilir. 9) *(V6c2 Kapı A)* Canlı kart kaydın kurallarıyla çalışır: `PrepareForSave`'in kuralları hata fırlatmayan tek bir çekirdeğe iner, kayıt ve önizleme onu paylaşır. İki tutar birlikte girilmişse kapatma tutarı otoritedir; uyuşmazsa kart anaparadan faiz göstermez, "uyuşmuyor" cümlesini gösterir. Önizleme `IObligationManagementService.PreviewLoan` ile açılır (7 → 8 metot); form ViewModel'i 5 bağımlılıkta kalır, faiz kartı `LoanPayoffViewModel` çocuğundadır. 10) Bayat kapatma tutarı: formu açarken son ödenen taksitten önce alınmışsa gösterilmez ve kaydedince düşer (`I67`). Form açıkken tarih değişikliğiyle bayatlarsa artık sessizce düşmez — alan görünürken görüneni silmek yanlış olur: kart "uyuşmuyor" der, Kaydet servisin mesajını gösterir. 11) Kenarlar: kapatılacak taksit kalmamışsa (geçmiş tarihli son taksit) yalnız aylık faiz görünür; kurtulunacak faiz ≤ 0 ise (kısa vadeli sabit faizli konutta ücret, 31 günlük ayda gün faizi) satır gizlenir; erken ödeme ücreti yalnız > 0 ise görünür. Geçersiz tutar ("abc", 0, eksi) kartta "tutar gir" cümlesiyle gösterilir, Kaydet diyalogla reddeder. 12) *(V6c3 Kapı A)* Tek kayıt tek işlemdir: `ILoanRepository.UpsertLoanWithPrepaymentsAsync` krediyi ve erken ödemelerinin tamamını tek transaction'da yazar. Listede olmayanlar silinir, başka kredininkine dokunulmaz. Kaydet tek plan revizyonu üretir. 13) Liste kaynağından bağımsızdır: simülatörden uygulanan erken ödemeler de (`SimulationPlanApplier`) burada görünür ve silinir. Bir erken ödemenin görülüp silinebildiği tek yer kredinin sayfasıdır. 14) Kaydın kurallarıyla: taslak kredi önce kayda hazırlanır (kapatma tutarından anapara çözülür); erken ödemeler bu hâle göre doğrulanır ve tutarları bu hâlden hesaplanır. Kredi kayıtta reddedilecekse satır tutarları "—" olur, "Ekle" kaydın mesajını gösterir (`I68`'in erken ödeme karşılığı). Faiz çözülemiyorsa mesaj formun diliyle verilir: "Erken ödemeyi hesaplamak için kalan anaparayı ya da bankanın kapatma tutarını gir." Kaydet'te reddedilen erken ödemenin tarihi mesajın başına eklenir. `LoanPrepaymentValidator`'a hata fırlatmayan bir çekirdek (`Check`) gelir, `Validate` onu sarar (9'daki `Prepare` deseni). 15) Giriş: şekil varsayılanı "Tamamen kapatma"; tarih kredinin bugünden sonraki ilk taksit günü (tek taksit kaldıysa bugün) ve bugünden önce olamaz; ara ödemede anaparadan düşecek tutar zorunlu (> 0), tamamen kapatmada sorulmaz. Henüz kaydedilmemiş yeni kredide de girilebilir; krediye bağlama işini Application yapar. Şekil adları seçicide ve satırda aynıdır (≤ 24 karakter): "Tamamen kapatma", "Ara ödeme · vade kısalır", "Ara ödeme · taksit düşer". 16) Yer: doğrulama ve önizleme `LoanPayoffService`'e girer (bağımlılık 3 → 4); `ObligationManagementService` 5 bağımlılıkta kalır. Port 8 → 9 metot: `SaveLoanAsync` erken ödemeleri de alır; `PreviewLoanPrepayments` ve `ValidateLoanPrepayment` gelir; çağıranı kalmayan `DeleteLoanPrepaymentAsync` çıkar. `LoanPayoffService.DescribePrepayments(FinancialPlan)` çıkar (eski Krediler listesinin artığı, src'de çağıranı yok); yerine kredi başına önizleme gelir. Erken ödemenin kazancı (kurtulunan faiz, yeni bitiş / yeni taksit) `V10` simülatörün sorusudur, bu listede gösterilmez. |
| **Etkiler** | `V6c1` (1–3, 7), `V6c2` (4, 5, 9–11; `I67` metni "formu açarken" diye daraldı), `V6c3` (6, 12–16; Application ve depoya krediyi ve erken ödemelerini tek işlemde yazan metot), `EK-V6`, `EK-V6c`, `V4` (f — kurulum düzeltme oturumunda), `V10` (`S62`-7; simülatörden uygulanan erken ödeme kredinin sayfasında görünür) |
| **Durum** | uygulandı (`V6c1`: 1–3, 7 · `V6c2`: 4, 5, 9–11; `I68`, `I69` · `V6c3`: 6, 12–16; `I70`–`I72`). Aşama 1 bulgusu i'nin form dışındaki yolları (dönem kapanışı, gelir kaydı) `/duzeltme` işi |

### S66 — Hatırlatıcı kartı operasyonel aksiyona odaklandı, ayar ve takvim ayrıştırıldı

| | |
|---|---|
| **Eski** | `PaymentReminderCardViewModel` (419 satır, 2 partial) ve `PaymentReminderCardView.xaml` (141 satır); vadesi gelen hatırlatıcının yanında 35 günlük takvim listesini (`Upcoming`), geçmişte ertelenen ve ödenenlerin listelerini (`Snoozed`, `Paid`), bildirim modu seçici çiplerini (Kapalı/Rahat/Agresif), deneme bildirimi gönderme butonunu ve bildirim izin uyarısını tek bir kartta ana sayfada barındırıyordu. |
| **Neden yanlış** | Hatırlatıcı kartının asıl varoluş sebebi anlık operasyonel aksiyondur ("bu borcu şimdi ödedim veya 3 saat ertele"). Bildirim modu ve izin yönetimi bir defalık sistem ayarıdır ve yeri `EK-V13` (Ayarlar) ekranıdır. 35 günlük gelecek takvim ise `EK-V3`'teki `ListCard` (kalan ödemeler) ve `EK-V9` (dönem ayrıntısı) ekranlarında zaten bulunmaktadır. Tüm bu sorumlulukların tek bir kartta toplanması aşırı bilişsel yük, XAML şişmesi ve mimari kural ihlallerine (K3, K4) yol açıyordu. |
| **Yeni** | `ReminderCardViewModel` odaklı bir sayfasız çocuk ViewModel olur. Yalnızca vadesi gelmiş veya erteleme süresi dolmuş en öncelikli aktif ödemeyi `src/Mizan.App/Components/ReminderCard.xaml` bileşeni üzerinden sunar. İki net aksiyon çalıştırır: "Ödedim" (`PrimaryAction`) ve "Ertele" (`SecondaryAction`). Aktif bir hatırlatıcı yoksa kart gizlenir (`HasActiveReminder == false`), ana sayfada boşluk veya yerleşim zıplaması yaratmaz. Bildirim modu ve izin yönetimi `EK-V13`'e (Ayarlar) devredilir. |
| **Etkiler** | `V2`, `V3`, `V13`, `EK-V2`, `EK-V3` |
| **Durum** | uygulandı |

### S67 — Gelir formu: zam yeni gelir değil tutar değişikliğidir; gelir ve tutarı tek işlemde yazılır; tutar yatış gününe göre çözülür

| | |
|---|---|
| **Eski** | `CommitmentsPage.xaml`'deki satır içi "düzenli gelir" formu (ad, tutar, geçerlilik tarihi; ortak başlık, açıklama ve durum mesajıyla ~7 `<Label>`) her kaydı ayrı bir `SalaryScheduleEntry` olarak yazıyordu; ödeme günü yoktu, düzenleme yoktu, zam da ikinci gelir de yeni kayıttı. Tek seferlik gelir ortak senaryo formundan ("Plan adı", ~7 `<Label>`) giriliyordu, düzenlenemiyordu. Listede her tutar ayrı "gelir" satırıydı ("Geçerli: tarih", "Planlanan gelir" rozeti). v2'de model doğru (`S2`, `S3`, `S5`: akış + etkin tarihli tutar geçmişi) ama port yeni geliri iki ayrı çağrıyla yazıyor (`SaveRecurringIncomeAsync` + `SaveIncomeAmountHistoryAsync`) ve motor tutarı dönem başında çözüyor (`I4`). |
| **Neden yanlış** | a) Zam ile ikinci gelir ekranda ayırt edilemiyordu (`S2`'nin arayüzdeki karşılığı); "şu an ne kadar, ileride ne kadar" sorusu cevapsızdı. b) İki çağrı iki plan revizyonu üretir ve arada tutarı olmayan bir gelir bırakır; ikinci yazma düşerse gelir projeksiyonda görünmez. c) `I4` gelirin tutarını **dönem başında** yürürlükte olan kayıttan çözüyor ("dönem içi zamlar o dönemi etkilemez"). Dönem gelir gününde başladığında doğruydu; `S3`/`S4` ile gelir kendi gününe kavuşunca iki yerde yanlış çıkar: çapa 1, ayın 12'sinde eklenen "her ayın 20'si 10.000 ₺" bu dönem hiç görünmez (ilk tutar bugünden, dönem 1'inde başlıyor); gelir günü 15 iken "15 Ocak'tan itibaren 50.000 ₺" 15 Ocak yatışına uygulanmaz, Şubat'tan başlar. Form "şu tarihten itibaren" derken motor başka bir şey yapar (`S9`). `S31` ile aynı gerekçe. d) Tek seferlik gelirin açıklaması boş bırakılabiliyordu; Finansal Yapı satırı adsız kalıyordu. |
| **Yeni** | 1) İki sayfa: `IncomeFormPage` (`Routes.IncomeForm` + `incomeId`) düzenli gelir, `AdHocIncomeFormPage` (`Routes.AdHocIncomeForm` + kimlik) tek seferlik gelir; kimliksiz yeni, kimlikle düzenleme. Finansal Yapı "Ekle" seçicisi listenin grup sırasıyla açılır: Düzenli gelir, Tek seferlik gelir, Kredi kartı, Kredi. Gelir satırlarının diyaloğuna "Düzenle" gelir. 2) Düzenli gelirin tanımı: ad (zorunlu), ödeme günü (1–31); yeni gelirde aylık net tutar (> 0). Etiketler kurulumdakiyle aynı ("GELİR ADI", "AYLIK NET TUTAR", "ÖDEME GÜNÜ"). İlk tutar bugünden yürürlüğe girer. Düzenleme yüklenen gelirin üstüne `with` ile kurulur; aktiflik ve tutar geçmişi korunur (`I73`). 3) Tek kayıt tek işlemdir: gelir ve yeni tutarları tek transaction'da, tek plan revizyonuyla yazılır. Tutar kayıtları ekleme usulüdür; var olan kayıt değiştirilmez (kural 05). 4) **Tutar yatış gününe göre çözülür:** bir dönemdeki yatışın tutarı, yatış gününde yürürlükte olan (etkin tarihi ≤ yatış günü) en güncel kayıttır. `I4` bu kuralla yeniden yazılır; dondurulmuş dönemler değişmez. Kurulum (ilk tutar kurulum günü, ilk dönem o gün ya da sonra başlar) ve simülatörün gelir değişikliği aynı kurala geçer. 5) Tutar değişiklikleri yalnız kayıtlı gelirde: liste yürürlükteki tutarı ve ileri tarihli değişiklikleri tarih · tutar olarak gösterir (en fazla 4 satır + "+N daha", `GS21`); yürürlükten kalkmış tutarlar gösterilmez, veride kalır. Giriş: yeni tutar (> 0), geçerlilik tarihi (en erken bugün, varsayılan sonraki ödeme günü); aynı gelirde aynı tarihte ikinci tutar yok (`I19` ile aynı kural). Silme yalnız bugün ve sonrası yürürlüğe girenlerde; en az bir tutar kalır. Giriş ve silme Kaydet'e kadar bekler. Simülatörden gelenler de görünür ve silinir. Tutar kaydının açıklaması sorulmaz, gösterilmez. 6) Tek seferlik gelir: açıklama (zorunlu), tutar (> 0), tarih (en erken bugün, varsayılan bugün); ekle / düzenle `with` ile. 7) Kayıt bulunamazsa diyalog ve geri dönüş; hatalar diyalogla; Vazgeç, geri ok ve cihazın geri tuşu değişiklik varsa onay sorar (`EK-V6b` deseni). 8) Port: `SaveRecurringIncomeAsync` gelirle birlikte eklenecek tutarları (`V6d2`'de silinecek tutarların kimliklerini de) alır; çağıranı kalmayan `SaveIncomeAmountHistoryAsync` ve `DeleteIncomeAmountHistoryAsync` çıkar (6 → 4 metot). 9) Taşınmayanlar: form açıklaması, "Plan adı", başarı ve durum mesajı, "Planlanan gelir" rozeti, tutar açıklaması, toplam gelir, geliri sonlandırma (pasife alan bir yol yok; bitince listeden silinir). |
| **Etkiler** | `V6d1` (1–4, 7, 8'in tek işlem kısmı), `V6d2` (5, 8'in kalanı), `V6d3` (1'in tek seferlik kısmı, 6), `D4`/`D12` (`I4`), `V4` (kurulum aynı kurala geçer, değişiklik gerekmez), `V10` (simülatörün gelir değişikliği), `EK-V6`, `EK-V6d`, `S9` (V6 payı) |
| **V6d2 notları** | *(Kapı A, 2026-10-02.)* a) "En az bir tutar kalır" **Kaydet'te** denetlenir, silerken değil: bugün yürürlüğe giren tek tutar da silinebilir ve yerine aynı gün yenisi girilebilir. V6d1'de düzenleme tutar sormadığı için yeni gelirin yanlış girilen ilk tutarı yalnız bu yoldan düzelir. b) Girişin varsayılan geçerlilik tarihi **bugünden sonraki** ilk ödeme günüdür (ödeme günü bugünse gelecek ay); gün kayıtlı gelirin ödeme gününden alınır, formda değiştirilen gün izlenmez. c) Kurallar Domain'de tek yerdedir (`IncomeAmountRules`): silinen tutar bu gelirin ve bugün ya da sonra yürürlüğe giriyor, eklenen tutar > 0 ve en erken bugün, aynı tarihte iki tutar yok, en az bir tutar kalıyor. Ekran ve `IncomePlanService` aynı kuralı kullanır; servis saati alır (3 → 4 bağımlılık). d) Kayıt sırası: önce silme, sonra ekleme, ikisi de gelirle aynı işlemde; depo yalnız o gelirin tutarını siler. e) Silme diyaloğunun başlığı tarih değil sabit "Tutar değişikliği": ViewModel tarih biçimlendirmez (kural 03). f) Açık: `income_amount_histories (RecurringIncomeId, EffectiveDate)` indeksi UNIQUE değil; kural 05 "en fazla bir tane"yi UNIQUE indeksle istiyor. Şema göçü bu adımın işi değil, ayrı iş. |
| **Durum** | uygulandı (`V6d1`: 1'in düzenli gelir kısmı, 2–4, 7, 8'in tek işlem kısmı ve çağıranı kalmayan iki metodun çıkması; `I4` yeniden yazıldı, `I78`, `I79`; `V6d2`: 5, 8'in silme ve tutar yönetimi kısmı; `V6d3`: 1'in tek seferlik kısmı, 6 ve 7) |

### S68 — Dönem içinde birden fazla gözlem: her bakiye girişi bir nokta, ödeme işareti gözlemden ayrılır

| | |
|---|---|
| **Eski** | *(Kaynak eski proje değil, bugünkü v2 kodu.)* `period_observations.PeriodPlanSnapshotId` UNIQUE; `PeriodWorkflowService.ObserveCurrentBalanceAsync` her bakiye girişinde dönemin tek kaydının üzerine yazıyor. Ödeme işaretleri (`PeriodObservationPayment`) bu kaydın çocuğu; `ObservePaymentAsync` işaret koyarken gözlemin gününü ödemenin gününe çekiyor ve zaman damgasını yeniliyor, sınıflandırıcı işareti her zaman bakiyeye yansımış sayıyor. `ObservedLivingSpend` hiçbir yerde hesaplanıp yazılmıyor (hep 0; kapanış taslağına 0 gidiyor), `Note`'u hiçbir ekran yazmıyor, `ObservedBalance` yalnız işaret taşıyan bakiyesiz gözlem için `null` olabiliyor. Kapanış (`FinalizeSettlementAsync`) dönemin gözlemini siliyor. Kapanış ertelenmişken girilen bakiye bugünün tarihiyle, dönemin aralığı dışına yazılıyor. |
| **Neden yanlış** | Sözlük gözlemi "dönem içinde kullanıcının girdiği anlık bakiye" diye tanımlıyor; tek kayıtla uygulama yalnız son bakiyeyi biliyor, ana sayfa grafiğinin (`V3`) çizeceği bakiye yolu yok. İşaret gözleme yapışık: 14 Eylül'de 65.000 girip "kirayı 12'sinde ödedim" diye işaretlemek gözlemi 12'sine çeker; arada yatan gelir "henüz yatmamış" sayılır ve tahmine ikinci kez eklenir (`S31`). Ertelenmiş kapanışta 3 Ekim'de girilen bakiyede Ekim maaşı var; biten döneme yazılırsa o dönem bir maaş kadar iyi görünür. Kapanışta silmek geçmiş ekranında (`V12`) dönemin eğrisini imkânsız kılıyor. Hesaplanmış yaşam harcamasını saklamak kural 05'e aykırı ve zaten bayat (0). |
| **Yeni** | 1) Her bakiye girişi yeni bir gözlemdir, grafikte bir noktadır. Dönem planın açılış bakiyesiyle başlar; kullanıcı ilk noktayı girmez. 2) **Gün başına tek gözlem:** aynı gün ikinci giriş öncekinin yerine geçer (yanlış tutar aynı gün düzeltilir); şemada `(PeriodPlanSnapshotId, ObservedOn)` UNIQUE. 3) **Son gözlem en geç tarihli olandır,** giriş sırasına bakılmaz; gidişat ondan hesaplanır. 4) **Gözlem günü açık dönemin içindedir ve bugünden sonra olamaz. Dönem bitmiş ama kapanmamışsa (kapanış ertelendi) gözlem yazılmaz:** "Bakiye gir" önce kapanışı ister; kapanıştan sonraki giriş yeni dönemin ilk gözlemi olur. 5) Biten dönemin sonunu gözlem değil **kapanış** belirler: "planlandığı gibi gitti" denirse planın dönem sonu, "şu şu farklı oldu" denirse o farklardan çıkan bakiye (`PeriodActual.ConfirmedEndingBalance`). `V11`'deki "Değiştir" "Bakiye gir"i açmaz; kapanış bakiyesi kapanışın içinde değişir. 6) Çizgi noktaların arasında ve son noktadan sonra **plandan** çizilir: her gelir ve ödeme kendi gününde bakiyeyi değiştirir, kalan yaşam gideri dönem sonuna iner; hiç gözlem yoksa çizgi baştan sona plandır. 7) Kapanış gözlemleri silmez; dönemin gözlemleri tarihçede kalır (plan silinirse yabancı anahtarla gider). 8) **Ödeme işareti gözlemden ayrılır,** dönemin dondurulan planına bağlanır; adı "ödeme işareti" `PeriodPaymentMark` olur (kod ve belgeler zaten "açık işaret" diyor: `I30`, `S33`). Bakiyeye yansıyıp yansımadığı, ödeme günü son gözlemin gününden önce mi diye bulunur — vade kuralıyla aynı; aynı gün yansımamış sayılır (kötümser). İşaret koymak hiçbir gözlemi değiştirmez. 9) Gözlemin şekli: bakiye zorunlu (`decimal`); `ObservedLivingSpend` (hesaplanmış, hep 0) ve `Note` (yazan ekran yok) taşınmaz; iki zaman damgası yerine tek kayıt zamanı — gözlem değişmez, aynı gün yeni giriş kendi zamanıyla yerine geçer. |
| **Açık notlar** | a) Kurulumun yazdığı gözlem (`OnboardingService`) çapa günü kurulum gününden sonraysa dönemden önceki güne düşüyor ve 4 ile çelişiyor; 1'e göre ilk nokta zaten plandan geliyor (`A28`). b) Hatırlatıcı "Ödedim" cevabının bakiyeden önce mi sonra mı verildiği bugün gözlemin zaman damgasıyla kıyaslanıyor; geriye tarihli gözlemde kayıt zamanı yanıltır (`A28`). |
| **Etkiler** | `H5` (2–4'ün kuralları: `PeriodObservationRules`), `I7b` (2'nin UNIQUE'i, 8'in tablosu ve adı, 9 — model şekli şemayla birlikte değişir), `A28` (1, 3, 4, 7'nin ve 8'in sınıflandırıcı kuralının uygulaması, açık notlar), `V3` / `T10` (1, 6), `V11` (4, 5), `V12` (6, 7). Sıra: `H5 → I7a → I7b → A28`. |
| **Durum** | kısmen uygulandı (`H5`: 2–4'ün kuralları, `I80`–`I82`; `I7b1`: 8'in model ve şema şekli, `I87`–`I89`; `I7b2`: 2'nin UNIQUE'i ve 9'un gözlem şekli, şema v3, `I90`–`I91`; `A28`: 1, 3, 4, 7, 8'in vade kuralı ve iki açık not, `I96`–`I101`; `A30`: 6'nın verisi — bakiye rotası, `S71`); ekranlar: `T10` ve `V3` (`V3a` ana sayfa, `V3b` "Bakiye gir": 1, 2, 4) uygulandı; `V11` / `V12`'de açık |
| **I7b1 notları** | 8'in şekli: `PeriodPaymentMark` (`period_payment_marks`, v2), `(PeriodPlanSnapshotId, PeriodPlanPaymentLineId)` UNIQUE; port ayrı açılmadı, `IPeriodObservationRepository`'ye iki metot eklendi (`PeriodWorkflowService` 6 bağımlılığa çıkmasın, M3). Kapanış işaretleri silmez (7'nin yönü); gözlem silmeye devam eder, `A28`'e kadar. Sınıflandırıcı işareti hâlâ "bakiyeye yansımış" sayar; vade kuralı `A28`'de. Bakiyesiz eski gözlem satırları `I7b2`'de (bakiye zorunlu olunca) elenir. |
| **I7b2 notları** | 2 ve 9'un şekli (v3): `period_observations` yeniden kurulur, `(PeriodPlanSnapshotId, ObservedOn)` UNIQUE, `ObservedBalance` `NOT NULL`; `ObservedLivingSpend`, `Note`, `CreatedAtUtc` çıkar, `UpdatedAtUtc` `RecordedAtUtc` adıyla tek damga olur (göçte kayıt zamanı eski `UpdatedAtUtc`'den gelir; eski `ObservePaymentAsync` işaret koyarken onu yeniliyordu, bu geri alınamaz). Bakiyesiz eski satırlar göçte elenir. `ix_period_observations_plan` yeniden kurulmaz: UNIQUE'in öneki `PeriodPlanSnapshotId`'yi zaten indeksler. Port listeyle çalışır (`GetPeriodObservationsAsync`); aynı (plan, gün) için `Upsert` öncekinin yerine geçer, yeni giriş kendi `Id`'sini getirir. Application yalnız derlenecek kadar uyarlandı: kapanış taslağının yaşam harcaması 0, notu boş (eskiden de hep öyleydi); kapanış hâlâ gözlemleri siler (`A28`). |
| **A28 notları** | Uygulama kararları: a) Geçersiz gözlem günü (dönem dışı, gelecek, kapanışı ertelenmiş dönem) `InvalidOperationException` ile reddedilir; ertelenmiş kapanışta mesaj "önce kapanış" der. b) Açık not a: `OnboardingService` gözlem yazmaz; ilk nokta plandan gelir (1), kurulumdan sonra gözlem yokken gidişat rakamları `null`'dır. c) Açık not b: "Ödedim" cevabı, cevabın günü gözlem gününden önceyse bakiyeye yansımış sayılır; aynı gün yansımamış (kötümser, 8 ile aynı yön); kayıt zamanına bakılmaz. d) Açık işaret: `ActualPaymentDate` son gözlem gününden önceyse yansımış, aynı gün ya da sonraysa yansımamış; ödenmedi işareti kalan satırdır. e) Kapanış gözlem silmez (7); çağıranı kalmayan `DeletePeriodObservationAsync` portta ve depoda durur, çıkarılması Infrastructure'a dokunan ayrı bir iştir. f) Önizleme `IPeriodProgressService.PreviewAsync`: taslak gözlemi `PeriodObservationRules.Record` ile deftere ekler, hiçbir şey yazmaz; nokta serisi `V3`'ün. |

### S69 — Şema bir göç listesidir: boş veritabanı da v1'den başlar, yükseltme tek işlemdir, yedekten gelen eski sürüm hazırlıkta yükseltilir

| | |
|---|---|
| **Eski** | *(Kaynak eski proje değil, bugünkü v2 kodu.)* `DatabaseSchema.EnsureInitializedAsync` yalnız iki durumu tanıyor: `user_version = 0` ise 30 tabloyu işlem dışında tek tek kurup sürümü `DatabaseConstants.CurrentSchemaVersion`'a (1) çekiyor, sürüm bundan büyükse reddediyor; aradaki her sürümü "hazır" sayıyor. Geri yüklemede `BackupDatabaseValidator` `1 ≤ user_version ≤ güncel` aralığını kabul ediyor ama eski sürümü yükselten bir yol yok. |
| **Neden yanlış** | İlk şema değişikliğinde (`I7b`, v2) v1 veritabanı sessizce açılır ve olmayan tabloya sorgu atınca çöker. Kurulum yarıda kalırsa (süreç öldürülürse) yarım şema ve `user_version = 0` kalır; sonraki açılış "table already exists" ile düşer, profil bir daha açılmaz. v2 çıktıktan sonra hiç açılmamış bir profil yeni alınan yedekte de v1 olarak durur; yedek yükseltilmeden yerine konursa hata, geri yükleme "başarılı" dendikten sonra profil açılırken çıkar. Sürüm bir sabitte, adımlar bir listede durursa ikisi birbirinden kayar. |
| **Yeni** | 1) **Şema bir göç listesidir** (`SchemaMigrations`): her adım (`SchemaMigration`) hedef sürümünü ve SQL komutlarını taşır; sürümler 1'den başlar, birer birer artar. Güncel sürüm listenin son adımıdır; `DatabaseConstants.CurrentSchemaVersion` sabiti çıkar. 2) **Boş veritabanı da aynı yoldan kurulur:** v1 bugünkü 30 tablonun DDL'idir ve dondurulmuştur; sonraki her değişiklik yeni bir adımdır. Yeni kurulum ile yükseltilen profil tek yoldan geçtiği için birbirinden kayamaz, bütün depo testleri göçleri de dener. Bedeli: şema dosyaları v1'i gösterir, bugünkü şekil v1 + adımlardır. **Yayımlanmış bir adım değiştirilmez;** bir test her adımın özetini sabitler. 3) **Yükseltme tek işlemdir:** bekleyen bütün adımlar ve `user_version` aynı işlemde yazılır; bir adım düşerse veritabanı açılıştaki sürümünde, dokunulmamış kalır. Boş veritabanının kurulumu da böylece tek işlem olur. 4) **Göç kayıt silmez:** yabancı anahtar denetimi işlemden önce kapatılır — SQLite onu işlem içinde değiştirmeye izin vermez, açıkken bir tabloyu yeniden kurmak (`CREATE` yeni → kopyala → `DROP` eski → `RENAME`) `ON DELETE CASCADE` ile çocuk kayıtları siler. İşlem kapanmadan `PRAGMA foreign_key_check` bütün veritabanını denetler; kopuk bağ varsa her şey geri alınır. Denetim sonunda, hata olsa da, yeniden açılır. 5) **Yedekten gelen eski sürüm hazırlıkta yükseltilir,** profil yerine konmadan: yükseltilemezse geri yükleme hep-ya-hiç düşer (`I36`), kullanıcıya "profilin verisi bu sürüme yükseltilemedi" denir. Manifestin ve veritabanının üst sınırı listenin güncel sürümüdür (`S58` değişmez). |
| **Etkiler** | `I7a` (1–5), `I7b` (v2 ilk yeni adım olarak yazılır, `period_observations` 4'teki yeniden kurmayla değişir), kural `05` (yayımlanmış adım değiştirilmez), `S53` (v1 temel olarak kalır), `S58` |
| **Durum** | uygulandı (`I7a`: 1–5, `I83`–`I86`; kural `05` → `SchemaMigrationsTests.YayimlanmisAdimlar_Degismez`) |

### S70 — Harcama temposu: harcanan oran ile geçen süre oranı aynı güne, son gözlemin gününe göre

| | |
|---|---|
| **Eski** | *(Kaynak eski proje değil, bugünkü v2 kodu.)* `PeriodProgress` yaşam havuzundan harcananı ve kalanı tutar olarak taşıyor; harcamanın süreye göre hızlı mı yavaş mı olduğunu söylemiyor. Geçen süre (`ElapsedDays`) bugüne göre, harcama son gözleme göre hesaplanıyor. `DashboardViewModel.PeriodElapsedRatio` da bugünkü `ElapsedDays`'ten kuruluyor. |
| **Neden yanlış** | İki oran farklı günlere dayanırsa kıyas yanıltır: kullanıcı bakiye girmedikçe harcama donar, süre ilerler ve ekran "harcama geride" der; yanlış güven verir. |
| **Yeni** | 1) **Tempo tek kayıttır** (`SpendingPace`, `PeriodProgress.Pace`): harcanan oran, geçen süre oranı ve gözlem günü. Gözlem yoksa `Pace` `null`; üç alanın null'luğu tek kontrolde toplanır. 2) **İki oran da son gözlemin gününe göre:** harcanan oran = `ObservedLivingSpend / PlannedVariableExpenseAllowance`; geçen süre oranı = (gözlem günü − dönem başı) / dönem gün sayısı, `ElapsedDays` ile aynı gün sayma biçimi. 3) **Fark işaretli puandır:** (harcanan − geçen) × 100; pozitif harcama önde, negatif geride; yuvarlama Presentation'da. 4) **Havuz 0 ise tempo yok** (`Pace` `null`): oran tanımsızdır. 5) **Aşım kırpılmaz:** havuz aşılınca harcanan oran 1'i geçer; halkanın 1'de doyması Presentation'ın işidir. |
| **Etkiler** | `A29` (uygulama), `V3` (tempo cümlesi ve halka `Pace`'ten beslenir; `PeriodElapsedRatio` bugüne göre olduğu için kalkar ya da `Pace.ElapsedRatio`'ya bağlanır), `T10` (`RingGauge` zaman işareti, `I95`) |
| **Durum** | uygulandı (`A29`: 1–5, `I102`–`I104`); ekran `V3`'te açık |

### S71 — Bakiye rotası: her gün bir nokta, günün başındaki bakiye; aralar plandan, fark günlere yayılır

| | |
|---|---|
| **Eski** | *(Kaynak eski proje değil, bugünkü v2 kodu.)* `PeriodProgress` yalnız son gözlemi taşıyor; dönemin gözlem listesi ve grafiğin çizeceği yol yok. `S68-6` ("çizgi noktaların arasında ve son noktadan sonra plandan çizilir") hiçbir adımda uygulanmadı: `T10` günlüğü onu `A28`'e, `A28` notu f `V3`'e bıraktı. |
| **Neden yanlış** | Grafik ya noktaları düz çizgiyle birleştirir ya hiç çizilmez. Düz çizgi yanlış hikâye anlatır: 97.000'den 65.000'e düşüşün 25.000'i kira günündeki tek ödemeyse çizgi o gün kırılmalı, yedi güne yayılmamalı. Hesap her gelir ve ödemeyi kendi gününe koyduğu için iş kuralıdır; Presentation'da yapılamaz (kural 01). |
| **Yeni** | 1) **Rota günlüktür:** dönem başından bitiş gününe (`PeriodEnd`, dahil) her gün bir nokta. 2) **Nokta günün başındaki bakiyedir:** önceki günlerin bütün hareketleri içindedir. İlk nokta açılış bakiyesi, son nokta (`PeriodEnd`) dönem sonu — sonraki dönemin açılışıyla aynı gün, aynı tutar (kural 05, para korunumu). Bir hareket kendi gününün ertesi noktasında görünür. 3) **Gözlem günü nokta girilen tutardır:** o gün yatan gelir içinde, o gün düşen ödeme değil (`S31`, `S68-8`). Dönem başına düşen gözlem açılış bakiyesinin yerine geçer. 4) **İki bağ noktası (açılış ya da gözlem) arası:** gelir ve ödemeler planlanan gün ve tutarlarıyla; kalan satırlar (ertelenen, "ödenmedi" işaretli) ödenmediği için düşülmez. Hareketlerin açıklayamadığı fark (yaşam harcaması ve planda olmayan her şey) aradaki günlere eşit yayılır; çizgi iki noktaya da tam oturur. Fark eksi olabilir (plansız gelir), çizgi o zaman yükselir. 5) **Son gözlemden sonra** gidişatın terimleri günlere dağıtılır: gelecek gelirler kendi günlerinde; kalan satırlar kendi günlerinde, kart satırı kartın bugünkü tutarıyla (`I23`); vadesi gözlemden önce olan kalan satırlar ve gözlemden sonra yapılan ödemeler gözlem gününde; kalan yaşam havuzu dönem sonuna kadar günlere eşit; KMH faizi son noktada. Son nokta `ProjectedEndingBalance`'tır. 6) **Gözlem yoksa rota saf plandır:** açılıştan, güncel planın satırları planlanan tutarlarıyla, havuzun tamamı eşit yayılır, planlanan KMH faizi son noktada; son nokta `PlannedEndingBalance`'tır. 7) **Rota iki parçadır:** `Travelled` dönem başından son bağ noktasına (düz çizgi), `Ahead` son bağ noktasından dönem sonuna (kesikli; ilk noktası son bağ noktası). Gözlem yoksa `Travelled` yalnız açılış noktasıdır. 8) **Yuvarlama en sonda:** yayma birikimli oranla hesaplanır, her nokta kuruşa en son yuvarlanır; kuruş artığı birikmez. Son nokta hesaplanır, rakamdan kopyalanmaz; ekrandaki rakama eşitliğini test korur, sapma hatadır. 9) `PeriodProgress` dönemin gözlemlerini (`Observations`, grafikteki nokta işaretleri) ve rotayı (`Path`) taşır; önizleme aynı hesaptan geçer, taslak gözlem rotada nokta olur. |
| **Açık notlar** | a) `A28`'den önce dönem dışına tarihli gözlem yazılabiliyordu: kurulum dönem başından önceye, ertelenmiş kapanışta giriş dönem sonrasına (`S68` "Eski"). Gidişat böyle bir gözlemi son gözlem olarak kullanıyor. Rota onu dönemin en yakın gününe (ilk ya da son gün) çekerek bağ noktası yapar; önümüzdeki yolda hiçbir hareket bağ noktasından önceye ya da son günden sonraya düşmez. Böylece son nokta gidişatla tutarlı kalır. b) Grafiğin sağ ucundaki tarih etiketi (`PeriodEnd` mi, bir gün öncesi mi) ekranın kararıdır (`V3a`). |
| **Etkiler** | `A30` (uygulama), `S68` (6'nın verisi), `V3a` (ana sayfa grafiği `Travelled` → `Series`, `Ahead` → `ProjectionSeries`), `V3b` (önizleme grafiği), `V12` (kapanan dönemin çizgisi aynı kurala dayanabilir) |
| **Durum** | uygulandı (`A30`: 1–9, `I105`–`I107`); ekranlar `V3a` (ana sayfa) ve `V3b` ("Bakiye gir" önizlemesi) uyguladı; iki ekran grafiği `BalancePathTrend`'ten kurar |
| **A30 notları** | a) Rota iki yoldan kurulur: gözlem yoksa `FromPlan`, varsa `FromObservations` (gidişatın kalan havuzu ve KMH faiziyle). b) Kalan satırın sayılan tutarı (I23) `PeriodProgressCalculator`'ın içinden `ProjectedPaymentAmount` yardımcısına çıktı; gidişat da rota da oradan alır (M8: statik yan erişim yerine ortak bağımlılıksız yardımcı). c) Gözlemden sonra yapılmış ödemelerin günü sınıflandırıcıda bilinmez (yalnız toplam); gözlem gününe düşer, ertesi noktada görünür. |

### S72 — Ana sayfa: dönem sonu plana düşer, halka tempoyu gösterir, biten dönemde bakiye yerine kapanış

| | |
|---|---|
| **Eski** | *(Kaynak eski proje değil, bugünkü v2 kodu: `DashboardViewModel`.)* Hero `ProjectedEndingBalance`'ı gösteriyor; bakiye girilmemişse `null`, ekranda tire. Halka kalan yaşam havuzunun oranını gösteriyor, geçen süre ayrı bir çubukta ve **bugüne** göre (`PeriodElapsedRatio`), harcama ise son gözleme göre. Kapanış iki kaynaktan okunuyor: `PeriodProgress.IsClosable` ve `IPeriodWorkflowService.GetSettlementAvailabilityAsync().IsDue` (ikisi de `Today >= SettlementAvailableFrom`); ikincisi (`HasActiveAlert`) ekranda kullanılmıyor. Son girilen bakiye ve tarihi ViewModel'de duruyor, ekrana bağlı değil. Bakiye sayfanın içinde, her zaman bugüne giriliyor. |
| **Neden yanlış** | Plan varken tire göstermek "bilmiyorum" demek; kullanıcı dönemin ilk gününde de planın dediğini görmeli. Halka ile süre farklı günlere bakarsa bakiye girilmedikçe harcama donar, süre ilerler ve ekran yanlış güven verir (`S70`). Aynı kuralın iki kaynağı bir gün ayrışır. Biten ama kapanmamış döneme bakiye yazılamaz (`S68-4`); o durumda "Bakiye gir" göstermek kullanıcıyı çıkmaz bir yola sokar. |
| **Yeni** | 1) **Dönem sonu rakamı:** bakiye girildiyse `ProjectedEndingBalance` ("Dönem sonu tahmini"), girilmediyse `PlannedEndingBalance` ("Planlanan dönem sonu"). Grafik bakiye girilmemişken planın rotasını kesikli çizer (`S71-6`). 2) **Plana göre fark** (`EndingDeviation`) ve plan tutarı yalnız bakiye girildiyse görünür. 3) **Halka tempoyu gösterir:** dolgu `Pace.SpentRatio` (1'de doyar), süre işareti `Pace.ElapsedRatio`; ikisi de son gözlemin gününe göre. `Pace` yoksa (bakiye girilmedi ya da yaşam havuzu 0) halka boş, işaret yok, tempo cümlesi yok. Bugüne göre `PeriodElapsedRatio` kalkar. 4) **Tempo puanı** `GapPoints` tam sayıya yuvarlanır (`MidpointRounding.AwayFromZero`); pozitif "önünde", negatif "gerisinde", 0 "aynı hızda". Harcanan oran ekranda kırpılmaz (%112 görünebilir). 5) **Dönem bitti** bilgisi tek kaynaktan: `IsClosable`. `GetSettlementAvailabilityAsync` çağrısı ve `HasActiveAlert` kalkar; ana sayfa `IPeriodWorkflowService`'e bağlı olmaz. 6) **Biten dönemde** bakiye kartının yerinde "Dönem bitti" + dönemin son günü + "Dönemi kapat" durur; "Bakiye gir" görünmez (`S68-4`). Kartta cümle yok: kapanış bakiye girmeyi gerektirmez (`V11`). 7) **Dönemin son günü** `PeriodEnd − 1`'dir: başlık "10 Eylül – 9 Ekim", grafiğin sağ ucundaki etiket de aynı gün (`S71` açık not b). Rotanın son noktası yine `PeriodEnd`'dedir: o günün başındaki bakiye, son günün akşamıdır. 8) **Bakiye girişi** ayrı sayfadır (`V3b`); ana sayfa yalnız son girilen bakiyeyi, tarihini ve "Bakiye gir" düğmesini gösterir. `V3a` ile `V3b` arasında düğmenin sayfası yoktur (kullanıcı kararı, 2026-09-30). 9) Kalan ödeme satırı ad, vade ve tutar taşır; ekranda olmayan `IsOverdue`, `IsSnoozed`, `Detail` kalkar. |
| **Etkiler** | `V3a` (uygulama: `DashboardViewModel`, `DashboardRemainingItem`), `V3b` (8: "Bakiye gir" sayfası), `V11` (6: "Dönemi kapat" özet sayfasını açar), `GS24` (ekran tarafı) |
| **Durum** | uygulandı (`V3a`: 1–7 ve 9, `I108`, `I109`); 8 `V3b`'de uygulandı (`S73`) |

### S73 — "Bakiye gir": tarih dönemle sınırlı, önizleme canlı ve sessiz, kapanışı bekleyen dönemde önce kapanış

| | |
|---|---|
| **Eski** | *(Kaynak eski proje değil, bugünkü v2 kodu.)* `V3a`'dan önce bakiye ana sayfadaki `HeroInputCard`'dan, her zaman bugüne giriliyordu; önizleme yoktu, etki ancak kaydettikten sonra görülüyordu. `V3a` bu kutucuğu kaldırdı, "Bakiye Gir" `Routes.BalanceEntry`'ye gidiyor ama sayfası yok. Tutar okuyan ortak `StatementEntryViewModel.TryParseAmount` yalnız noktalı binlik ayraçlı eksi tutarı ("−12.500") okuyamıyor; bugünkü formlarda eksi tutar zaten geçersiz. |
| **Neden yanlış** | Bakiye geriye tarihli girilebiliyor (`A28`) ama bunu seçecek bir yüzey yok. Kaydetmeden önce "bu girişle dönem sonu ne olur?" sorusu cevapsız; önizleme portu (`PreviewAsync`) hazır ama çağıranı yok. Banka bakiyesi KMH'de eksiye düşebilir; eksi tutar okunamazsa kullanıcı gerçek bakiyesini giremez. |
| **Yeni** | 1) **Tarih:** varsayılan bugün (`PeriodProgress.Today`); seçici `[dönem başı, bugün]` aralığıyla sınırlı, geçersiz gün seçilemez. Kural yine `ObservationDayGuard`'da; seçicinin sınırı yalnız kolaylıktır. 2) **Canlı önizleme:** geçerli tutar yazıldıkça ya da tarih değiştikçe `PreviewAsync` çağrılır; üst üste binen isteklerde **son istek kazanır**, eski cevap atılır. Tutar geçersizse ya da önizleme hata verirse önizleme gizlenir, uyarı çıkmaz. 3) **Tutar:** eksi (KMH) ve sıfır kabul edilir; eksi işareti sayfada ayrılıp kalanı ortak okuyucudan geçer, diğer formlara dokunulmaz. 4) **Kaydet:** `ObserveCurrentBalanceAsync(tutar, gün)` ve ana sayfaya dönüş (ana sayfa her görünüşte yeniden yüklenir). Geçersiz tutarda uyarı, kayıt yok; servis reddederse (ör. dönem gece yarısı bitti) servisin mesajı gösterilir, girilen tutar yerinde kalır. 5) **Kapanışı bekleyen dönem (`S68-4`):** sayfa bakiye yerine "önce kapanış" durumunu ve "Dönemi Kapat"ı gösterir (rota `V11`'in; ana sayfadaki düğmeyle aynı). Ana sayfa bu hâlde "Bakiye Gir"i zaten göstermiyor; bu, uygulama dönem sonunda açık kaldığında kalkan. Açık dönem yoksa sayfa geri döner. 6) **Önceki tahmin** yalnız daha önce bakiye girildiyse gösterilir; girilmediyse "önceki" planın dönem sonudur ve "Plana göre" satırı onu zaten söyler. 7) **Çıkış:** ✕ ve cihazın geri tuşu onay sormadan kapatır (kullanıcı kararı, 2026-09-30: tek alanlı sayfa). 8) Aynı güne ikinci giriş öncekinin yerine geçer (`S68-2`); sayfa bunu ayrıca söylemez, önizleme grafiği gösterir. |
| **Etkiler** | `V3b` (`BalanceEntryViewModel`, `BalanceEntryPage`), `S68` (4, 2), `S72` (8), `GS25` (ekran tarafı) |
| **Durum** | uygulandı (`V3b`: 1–8, `I111`–`I113`; `BalanceEntryViewModel`, önizleme kartı `BalancePreviewViewModel`) |
