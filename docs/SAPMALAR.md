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


