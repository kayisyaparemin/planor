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
| **Durum** | uygulandı: 1–7 `G1a`'da; 8'in `G1b` yarısı açık |

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
| **Durum** | uygulandı: 1–7 `G1a`'da; 8'in `G1b` yarısı açık |

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
| **Durum** | uygulandı: 1–7 `G1a`'da; 8'in `G1b` yarısı açık |

### S10 — Körlemesine find/replace hasarı

| | |
|---|---|
| **Eski** | Yeniden adlandırma değiştirilen kelimenin büyük harfini koruduğu için PascalCase **yerel değişken ve parametreler** oluşmuş: `int IncomeDay` parametresi, `var IncomeDay = settings.IncomeDay;`, `var VariableExpenseAllowance = ResolveLivingBudget(...)`. C# konvansiyonunu ihlal ediyor ve aynı adlı özelliği gölgeliyor. |
| **Neden yanlış** | Mekanik yeniden adlandırmanın kanıtı ve gizli hata kaynağı. |
| **Yeni** | `.editorconfig` içinde `IDE1006` hata seviyesinde. Bu sınıf hasar derleme zamanında yakalanır, göze bırakılmaz. |
| **Etkiler** | tüm adımlar (mekanizma, `F1`'de kurulur) |
| **Durum** | uygulandı: 1–7 `G1a`'da; 8'in `G1b` yarısı açık |

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
| **Durum** | uygulandı: 1–7 `G1a`'da; 8'in `G1b` yarısı açık |

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

### S65 — Ödeme formu: taksitli ödeme planı ve planlı büyük harcama için bağımsız form sayfaları; seri taksit girişi, düzenleme desteği

| | |
|---|---|
| **Eski** | Eski projede `CommitmentsPage.xaml` içinde satır içi "Geçici ödeme planı" ve "Büyük harcama" bölümleri vardı. Taksitler tek tek tutar ve tarih girilerek ekleniyordu. Taksit sayısı ile periyodik seri taksit üretimi yoktu. Düzenleme desteği yoktu: tıklandığında yalnızca "Sil" seçeneği çıkıyordu. |
| **Neden yanlış** | a) 12 taksitli bir borç veya senet için kullanıcının 12 kere ayrı ayrı tarih ve tutar girmesi hataya açık ve yorucudur. b) Adı, tutarı veya taksiti yanlış girilen bir planı ya da harcamayı düzenleyememek veri kaybına yol açar; silip baştan girmek zorunda bırakır. c) Formların devasa tek sayfada iç içe olması K3 ve K4 ihlali yaratır. |
| **Yeni** | 1) İki müstakil sayfa: `PaymentPlanFormPage` (`Routes.PaymentPlanForm` + `planId`) ve `PlannedExpenseFormPage` (`Routes.PlannedExpenseForm` + `expenseId`); kimliksiz yeni ekleme, kimlikle düzenleme. 2) Finansal Yapı "Ekle" seçicisine "Ödeme planı" ve "Planlı büyük harcama" eklenir. Satır diyaloğuna "Düzenle" seçeneği gelir. 3) Ödeme planı formu: Plan adı, taksitler listesi (en fazla 4 satır + yerinde taşma `GS21`), taksit giriş bloğu. Girişte taksit tutarı, taksit sayısı (1–120) ve ilk vade tarihi (en erken bugün) alınır; `CalendarRules.AddMonthsKeepingDay` ile otomatik seri taksit satırları üretilir. Ödenmemiş taksitler satır tıklamasıyla silinebilir; ödenmiş taksit silinemez. Kaydet planı ve taksitlerini tek `SavePaymentPlanAsync` ile yazar. 4) Büyük harcama formu: Harcama adı, tutar (> 0), harcama tarihi (en erken bugün). Kaydet tek `SavePlannedLargeExpenseAsync` ile yazar. 5) Kaydedilmemiş değişiklik varsa Vazgeç, geri oku ve cihaz geri tuşu onay sorar (`EK-V6b` deseni). 6) Taşınmayanlar: "Geçici / düzenli" ayrımı, açıklama/not alanları, durum rozetleri. |
| **Etkiler** | `V6e`, `EK-V6`, `EK-V6e`, `S62` |
| **Durum** | uygulandı |

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

### S74 — 12 dönem: zincir ana sayfanın dönem sonundan başlar, liste açık dönemden sonraki 12 dönemdir

| | |
|---|---|
| **Eski** | `FutureMonthsViewModel` (315 satır) tanrı cephe `MizanService`'ten beş metot çağırıyordu (`GetFuturePeriodsAsync`, `GetFinancialPlanAsync`, `GetPeriodProgressAsync`, `GetLoanPayoffAdviceAsync`, `FindTargetReachabilityAsync`). Zincir planın çapasından (son kapanış ya da kurulum) kuruluyordu; ilk satır açık dönemdi ve kullanıcının girdiği bakiye zincire girmiyordu ("gözlem projeksiyona girmez"). Aradaki fark sayfanın tepesinde cümleyle söyleniyordu ("Bu projeksiyon … checkpoint'ine dayanıyor. Mevcut dönemde … sapma gözlendi"). Ekranda ayrıca hedef tutar ("Ne zaman karşılayabilirim?"), tahsis metni ("… ödemelerini karşılar"), "dönem gelirinden önce vadesi gelen N ödeme" çipi ve önceki dönemden devreden açık satırı vardı. Kredi kapatma önerisi arka planda koşuyor, hatası sayfanın durum satırına yazılıyordu. |
| **Neden yanlış** | a) Ana sayfa (`V3`, `S72-1`) açık dönemin sonu için tahmini gösteriyor, 12 dönem planı: iki ekran aynı dönem için iki rakam söylüyor ve bütün gelecek dönemler kullanıcının bildiği farkı görmezden geliyor. Uyarı cümlesi bu çelişkiyi açıklıyor, gidermiyor. b) Beş metodun hepsi `S41` / `S49` ile elendi. Alttaki servisleri ViewModel'de birleştirmek 6 bağımlılık eder (M3) ve zincirin nereden başlayacağı gibi bir iş kuralını Presentation'a taşır (kural 01). c) Tahsis metni ve "gelirden önce" çipi yapay tahsisin kalıntısı (`S18`); v2 modelinde karşılıkları yok. d) Devreden açık satırı bir önceki satırın eksi dönem sonunun tekrarı. e) Hedef tutar, listedeki dönem sonlarından okunabilen bir cevabı ayrı bir giriş kartıyla hesaplıyordu; asıl soru ("bu harcamayı şu tarihte yapabilir miyim?") simülatörün planlı büyük harcama senaryosudur. |
| **Yeni** | 1) **Zincir ana sayfanın dönem sonundan başlar:** açılış bakiyesi `ProjectedEndingBalance ?? PlannedEndingBalance` (`S72-1` ile aynı kural), ilk dönemin başı açık dönemin bitişi (`PeriodProgress.PeriodEnd`). Liste açık dönemden **sonraki** 12 dönemdir; açık dönem ana sayfanın işidir. Kapanışı ertelenmiş dönemde de aynı: zincir biten dönemin sonundan başlar. 2) Açık dönem yoksa ya da plan projeksiyon kuramıyorsa (`CanBuildProjection`, `I12`) ekran boştur: ana sayfa boşken 12 dönem bir rakam uydurmaz. 3) Hesap Application'da yeni bir okuma portundadır (`IFutureProjectionService`). Plan `IPlanReader.GetProjectionPlanAsync`'ten gelir, açılış bakiyesi ve çapa 1'e göre yeniden yazılır, hiçbir şey kaydedilmez. Kapanıştan sonraki ilk dönem sınırını (`ProjectionBoundary.FirstUnrealizedPeriodStartDate`) kullanmaz. 4) Sapma uyarısı taşınmaz; 1 onu gereksiz kılar. 5) Hedef tutar taşınmaz: `TargetAmountCalculator` Domain'de kalır, ekranı yoktur; soru `V10`'a not. 6) Tahsis metni, "gelirden önce" çipi ve devreden açık satırı taşınmaz. 7) *(V8b)* Erken kapama önerisi aynı zincirle (aynı açılış, aynı ilk dönem) koşar; açık dönemin içindeki taksit günleri denenmez — "bugün kapatırsam" kredi formundaki kartın sorusudur (`S64`-5). Satıra dokunmak krediyi açar (`Routes.LoanForm`); kapama orada erken ödeme olarak planlanır (`V6c3`). Önerilenden farklı en kârlı gün gösterilmez (`V10`). Önerinin hatası listeyi düşürmez, yalnız kartı gizler. 8) Dönem satırına dokunmak `V9`'da, "Simülatörde dene" `V10`'da bağlanır. |
| **Etkiler** | `V8a` (1–6), `V8b` (7), `V9` (8: satır → dönem ayrıntısı), `V10` (5, 7, 8), `EK-V8`, `GS26` |
| **Durum** | uygulandı (`V8a`: 1–6, 8; `V8b`: 7; `I114`–`I118`); 8: dönem karosu `V9`'da bağlandı (`S75`), "Simülatörde dene" `V10`'da |

### S75 — Dönem ayrıntısı: 12 dönem zincirinden tek dönem, akış dönem sonuna kuruşu kuruşuna iner, ham değer

| | |
|---|---|
| **Eski** | `CashFlowPeriodDetailViewModel` (123 satır, 4 bağımlılık: sunucu, tanrı cephe `MizanService`, hatırlatıcı VM, gezinme) dönemi sorgu parametresinde **nesne** olarak alıyordu (`SalaryPeriodDetailRequest`: senaryo, baz, simülasyon bayrağı, mevcut dönem bayrağı). Sunucu (`CashFlowPeriodDetailPresenter`, 552 satır / 2 `partial`) bütün metni üretiyordu: kutu etiketleri ("DÖNEM BAŞI"), "… TL" tutarları, gelirin ihtiyacı karşılayıp karşılamadığını anlatan cümleler. Ekran üç yerden açılıyordu: ana sayfa (açık dönem + hatırlatıcı + "ödediklerin"), 12 dönem, simülatör (baz ↔ senaryo kıyası). Kart ekstresinin ödeme şekli sayfada değiştirilip 12 dönem yeniden hesaplanıyordu. Akışta ara toplamlar ("Zorunlular sonrası", "Açık kapatıldıktan sonra kalan"), ayrı bir zorunlu ödeme kategori kartı ve devreden açık kutusu vardı. |
| **Neden yanlış** | a) Nesne parametresi sayfayı yeniden yüklenemez kılıyor: hata hâlinde "Tekrar dene" yok, plan değişince sayfa eski rakamı gösteriyor. b) Metin üretimi `03-mvvm`'in yasağı; A23a'da taşınan sunucu da aynısını yapıyor ve hiçbir ekrana bağlı değil. c) Tahsis rozeti, "… ödemelerini karşılar" penceresi, "Dönemden Önce" çipi ve düzen değişikliği satırları yapay tahsisin kalıntısı (`S18`). d) Kart ödeme şekli kararı Kart Kontrol'de ("Sonraki ödemeler", aynı `SetStatementPaymentModeAsync`) zaten var; iki ekranda aynı karar iki doğru üretir. e) Ara toplamlar, kategori toplamları ve devreden açık kutusu aynı rakamların tekrarı (`S74`-6 ile aynı gerekçe). f) Açık dönemin ayrıntısı başka bir veri (dondurulmuş plan + gözlem) ve başka bir dönem sonu; aynı sayfada iki kaynak, 12 dönem ile ana sayfanın `S74`'te giderilen çelişkisini geri getirir. |
| **Yeni** | 1) **Kaynak:** sayfa dönemi başlangıç tarihiyle alır (`Routes.PeriodStartParameter`, `DateOnly`) ve `IFutureProjectionService.GetAsync()` zincirinden seçer: karodaki dönem sonuyla birebir aynı rakam, sayfa yeniden yüklenebilir. Yeni port yok, hiçbir şey yazılmaz. 2) Zincir kurulamıyorsa ya da dönem artık zincirde değilse (gün değişti, plan değişti) ekran boştur. 3) **Akış:** dönem başı + gelir − ödemeler − yaşam gideri − büyük harcama − eksi bakiye faizi = dönem sonu, kuruşu kuruşuna. Ara toplamlar, kategori toplamları ve devreden açık kutusu taşınmaz; dönem başı eksiyse satır eksi görünür. 4) **Ödemeler:** zorunlu kalemler + büyük harcamalar + tutarı belirlenemeyen kart ekstreleri (`Payment` yok), önce gün sonra ad sırası. Kartın ödeme şekli hiç seçilmemişse (`ProjectionFallback`) tutar "tahmini" işaretlenir; belirlenemeyende tutar yerine "Belirlenmedi". Kategori adı taşınmaz. 5) **Kart faizi** ekstre borcuna eklenir, o dönem nakit çıkmaz: akışta değil, kart başına ayrı listede (yalnız > 0). Eksi bakiye faizi nakittir, akışta. 6) Kart ekstresinin ödeme şekli bu sayfada seçilmez; kart satırına dokunmak o kartın Kart Kontrol'ünü açar (`Routes.CardControl` + `cardId`). 7) Simülasyon blokları ve baz ↔ senaryo kıyası `V10`'a; hatırlatıcı ve "ödediklerin" taşınmaz (ana sayfada var). 8) A23a'nın tüketicisiz `CashFlowPeriodDetailPresenter`'ı, dört modeli (`CashFlowPeriodDetailData`, `DetailPaymentRow`, `DetailComparisonRow`, `DetailDeficitCallout`) ve testleri silinir; ViewModel ham değer sunar, biçim converter'da. `DetailMetric` ve `DetailSemanticType` kalır: simülatörün sunum kodu (A23b) onları kullanıyor, kaderleri `V10`'da. 9) **Ana sayfa "Tümünü Gör"** dönem ayrıntısına gitmez: kalan ödemeler kartı bütün satırları yerinde açar (`GS21` deseni, "+N daha"); açık dönemin ayrıntısı ana sayfanın kendisidir (kullanıcı kararı, 2026-10-03). Gelir kalemleri listelenmez, akışta tek "Gelir" satırı (kullanıcı kararı). |
| **Etkiler** | `V9` (`PeriodDetailViewModel`, `PeriodDetailPage`), `V8` (karoya dokunma), `V3` (9: `DashboardViewModel`, `EK-V3`), `V10` (7), `A23a` (8), `EK-V9`, `GS27` |
| **Durum** | uygulandı (`V9`: 1–9; `I119`–`I121`; `PeriodDetailViewModel`, `PeriodDetailPage`). 7'nin simülatör tarafı `V10`'da |

### S76 — Simülatör: şu anki gidişat 12 dönemin zinciri, açık döneme düşen deneme açılışa eklenir, tek çalışma listesi, uygulama tek işlem

| | |
|---|---|
| **Eski** | `SimulationViewModel` 1.034 satır / 4 `partial`, 4 bağımlılık (tanrı cephe `MizanService` dahil) + `ScenarioConditionForm` 403 satır / 2 `partial`; `SimulationPage` kod arkası onay ve gezinme akışını taşıyordu. Motor (`SimulationWorkflowService.SimulateAsync`) zinciri planın çapasından kuruyor (`ProjectionBoundary.FirstUnrealizedPeriodStartDate`), ilk satır açık dönem. Koşullar yalnız ekranda yaşıyor, uygulama kapanınca kayboluyordu; yanında adla saklanan "geçici planlar" (kaydet, yükle, sil). Sonuç "Simülasyonu Yap" ile geliyor, koşul değişince "Plan değişti" uyarısı çıkıyordu. Sonuç tarafı: anlatı cümleleri, içgörü çipleri, "Öne Çıkanlar", faiz karşılaştırma tablosu, kredi faizi tasarrufu, hedef tutar ve 12 dönem kartı ("Detayı Gör" → baz ↔ senaryo dönem ayrıntısı). `A23b`'de taşınan sunucular (`SimulatorInsightService` …) bu metni üretiyor ve hiçbir ekrana bağlı değil; `SimulationCalculator` Domain'de `FriendlySummary` cümlesi kuruyor. "Planı Uygula" altı ayrı depo yazımı yapıyor (`SimulationPlanApplier`); eski ekran "bir koşul kaydedilemezse hiçbir değişiklik yapılmaz" diyordu. |
| **Neden yanlış** | a) Çapadan kurulan zincir 12 Dönem (`S74`-1) ve ana sayfayla aynı dönem için başka rakam söyler; kullanıcının girdiği bakiye denemeye girmez. Ama `S74` zinciri açık dönemi içermez: oraya geçilirse bugüne tarihli bir harcama hiç görünmez. b) Ekranda yaşayan liste kayboluyor; adlı planlar ikinci bir yönetim yüzeyi açıyor. Sorun listenin kaybolmasıysa çözüm listeyi saklamaktır (kullanıcı kararı, 2026-10-03). c) Liste günlerce yaşayınca tarihi geçen ya da kaydı silinen deneme ya sessizce yanlış hesap verir ya da bütün hesabı düşürür (`ScenarioPlanBuilder` "kart bulunamadı" fırlatır). d) Metin üretimi `03-mvvm`'in yasağı; Domain'de kültürlü biçim kural `01`'in yasağı. e) Hedef tutar, 12 Dönem ızgarasından okunabilen bir cevabı ayrı kartla hesaplıyordu (`S74`-5). f) Ayrı depo yazımları yarıda kalırsa planın bir kısmı uygulanmış olur; tekrar denemek "bir kısmı uygulanmış" hatasına düşer. |
| **Yeni** | 1) **Şu anki gidişat 12 Dönem'in zinciridir:** açılış ana sayfanın dönem sonu (`ProjectedEndingBalance ?? PlannedEndingBalance`), ilk dönem açık dönemin bitişi, 12 dönem (`S74`-1). Zinciri kuran kod 12 Dönem ile ortaktır (M8: bağımlılıksız ortak yardımcı); simülatörün şu anki çizgisi 12 Dönem'in grafiği ve ızgarasıyla kuruşu kuruşuna aynıdır. Açık dönem yoksa ya da zincir kurulamıyorsa simülatör boştur (`S74`-2). 2) **Açık döneme düşen deneme kaybolmaz:** denemenin bugün ile açık dönemin bitişi arasına düşen etkisi senaryo zincirinin açılışına eklenir. Etki, açık dönem planla ve aynı açılıştan iki kez hesaplanarak bulunur: denemeli dönem sonu − denemesiz dönem sonu. Sonraki dönemlere düşen kısım (kart ekstresi, taksitler, gelir değişikliği) zincirin kendisindedir. Grafiğin ve ızgaranın ilk noktası bu yüzden iki çizgide ayrışabilir. 3) **Deneme tarihi en erken bugündür** (işlem, ilk ödeme, geçerlilik tarihi); geçmiş, kaydın işidir. 4) **Tek çalışma listesi:** denemeler var olan taslak tablosunda tek kayıtta saklanır; ekleme, düzenleme, silme ve aç/kapa anında yazılır. Adlı planlar, "Bu planı sakla", yükle / sil taşınmaz. Port adlı taslak metotlarının yerine listeyi okuma ve yazma alır; tek istekli `SimulateAsync` / `ApplySimulationAsync` aşırı yüklemeleri çıkar. 5) **Geçersizleşen deneme** (tarihi bugünden önce kalan; kartı, kredisi ya da geliri silinen) hesaba girmez ve uygulanmaz; satırda işaretlenir, düzenlenir ya da silinir. Kural Application'dadır, ekran yalnız işareti gösterir. 6) **Canlı hesap:** liste değişince ve sayfa her görününce sonuç yeniden hesaplanır; üst üste binen hesaplarda son istek kazanır (`S73`-2). "Simülasyonu Yap", "Plan değişti" ve "Hiçbir koşul açık değil" taşınmaz; açık ve geçerli deneme yoksa yalnız şu anki gidişat görünür. 7) **Dokuz deneme türü** (katalogdaki dokuz seçenek): nakit ödeme, kartla harcama (taksit 1 = tek çekim), düzenli ödeme, kredi çekme, taksitli nakit borç, krediye erken ödeme (tamamen kapat / vadeyi kısalt / taksiti azalt), tek seferlik gelir, gelir değişikliği, kart ödeme şekli. Tür "Ekle" seçicisinden seçilir; form yalnız o türün alanlarını gösterir; düzenlemede tür değişmez. *(2026-10-04, `S77` V10 notları i: üst kaydı — kart, kredi, düzenli gelir — da seçici çözer; formda açılır liste yok, düzenlemede üst kayıt değişmez.)* Katalogdaki açıklama metinleri taşınmaz (GK5). Kart ödeme şekli uygulanırken Kart Kontrol'ün yazdığı kurala uyar (`V7`, `S61`; ayrıntı `V10c`). 8) **Ad zorunludur:** uygulanınca kaydın adı olur (`S67` d). 9) **Sonuç:** denemeyle en düşük dönem sonu, hangi dönem ve şu anki gidişata göre farkı; 12 dönem sonra fark; 12 dönemde faiz (şu anki ve denemeyle toplam; kırılım taşınmaz); krediye erken ödeme varsa kredinin ömrü boyunca faiz kazancı, kredi çekme varsa çekilen kredinin maliyeti. Dönem sonları 12 Dönem'in ızgarasında, denemeyle (kullanıcı kararı, 2026-10-03: "12 dönemin aynısı"). Karo dokunulmaz: dönem ayrıntısı (`S75`-1) denemeyi bilmez; `S75`-7'deki dönem kıyası taşınmaz. 10) **Hedef tutar taşınmaz;** `TargetAmountCalculator` ve testleri silinir (`S74`-5: "şu tarihte X harcarsam?" bir nakit ödeme denemesidir). 11) **Metin üreten kod silinir:** `A23b`'nin sunucuları (`SimulatorInsightService`, `SimulatorTimelineNarrative`, `SimulatorInterestPresenter`, `SimulatorProjectionMath`) ve modelleri (`SimulatorPeriodView`, `SimulatorSummaryMetric`, `SimulatorProjectionSummary`, `SimulatorInterestRow`) ile `DetailMetric` / `DetailSemanticType` (`S75`-8'in kalanı); Domain'deki `SimulationResult.FriendlySummary`. ViewModel ham değer sunar, biçim converter'dadır. 12) **"Planıma ekle" tek işlemdir:** açık ve geçerli denemeler tek işlemde gerçek plana yazılır, bir yazma düşerse hiçbiri kalmaz. Kısa onay sorulur; uygulananlar listeden düşer, kapalılar kalır. Sonuç yeniden hesaplanınca senaryo çizgisi şu anki gidişata oturur; "Finansal Yapı'da gör" diyaloğu taşınmaz. Kayıtlar Finansal Yapı'da görünür ve silinir (`S64`-13, `S67`-5). 13) **"Simülatörde dene" açılmaz:** 12 Dönem'in erken kapama satırı krediyi açmaya devam eder (`V8b`); önerilenden farklı en kârlı gün gösterilmez (`S74`-7) (kullanıcı kararı, 2026-10-03). 14) Kod arkasındaki onay ve gezinme ViewModel'e taşınır (`03-mvvm`, `IDialogService`). |
| **Etkiler** | `V10a` (3, 4, 5'in tarih işareti, 7'nin nakit ödemesi, 8, 11'in `A23b` kısmı, 14), `V10b` (1, 2, 5'in hesap kısmı, 6, 9, 10, 11'in `FriendlySummary` kısmı), `V10c` (7: kartla harcama, düzenli ödeme, kart ödeme şekli), `V10d` (7: kredi çekme, nakit borç, erken ödeme; 9'un kredi satırları), `V10e` (7: iki gelir türü), `V10f` (12), `S74` (5, 7, 8), `S75` (7, 8), `EK-V10`, `GS28`, `G1` (eski adlı taslakların hangisinin çalışma listesi olacağı orada kararlaştırılır) |
| **Durum** | uygulandı (`V10a`–`V10f`; `I122`–`I130`, `I138`–`I154`; `SimulatorPage`, `SimulatorViewModel`, `ISimulationResultService`, `SimulationResultViewModel`, `ISimulationBatchWriter`, `SqliteSimulationBatchWriter`, `SimulationPlanApplier`). Tekil `ApplySimulationAsync` aşırı yüklemeleri kaldırıldı; atomik transaction adaptörü tamamlandı |
| **V10a notları** | a) Çalışma listesi taslak tablosunda sabit kimlikli tek kayıttır; adı ("Çalışma listesi") hiçbir ekranda görünmez. b) Tarih kuralı Domain'de tek yerdedir (`SimulationConditionRules.IsDatePassed`): form yeni denemeyi, servis okunan listeyi aynı kuralla değerlendirir. c) Form yazmadan hemen önce listeyi yeniden okur: form açıkken simülatörde aç/kapa ya da silme olmuş olabilir. d) Silme ikinci bir onay sormaz: deneme gerçek kayıt değildir, diyaloğun "Sil" düğmesi yeter. e) Liste yazılamazsa ekran diskteki listeye döner. |
| **V10b notları** | *(2026-10-03, Kapı A; `V10b` Aşama 1'de ~500 satır çıktığı için `V10b1` + `V10b2`; ikisi de aşağıdaki kararlara uyar)* a) **Zinciri kuran kod ortaktır:** açık dönemin bitişi, ana sayfanın dönem sonu ve plan, `FutureProjectionService` ile simülatörün sonuç servisinin paylaştığı saf bir kurucudadır (`ProjectionChainBuilder`, M8); iki ekranın zinciri aynı koddan çıkar. Sonuç yeni dar bir okuma portundadır (`ISimulationResultService`): `FutureProjectionService` da `SimulationWorkflowService` de 5 bağımlılıkta, M3 yeni mantığı onlara koydurmaz. b) **Açık dönem etkisi akış farkıdır, KMH faizi gerçek rakam üzerinden işler** (`S76`-2'nin hesabı): denemeli − denemesiz açık dönem akışı plandan bulunur (açılışa bağlı değildir); ana sayfadaki dönem sonuna faiz öncesi hâli (dönem sonu + KMH faizi) alınır, akış farkı eklenir, KMH faizi o toplam üzerinden yeniden işletilir, etki iki sonun farkıdır. Deneme yoksa ya da denemeler açık dönemden sonraya düşüyorsa etki tam sıfırdır ve zincirin açılışı ana sayfanın dönem sonuyla kuruşu kuruşuna aynıdır. Açık dönemin başı `PeriodProgress.PeriodStart`'tır, `ProjectionBoundaryResolver`'dan alınmaz (`V8a` notu). c) **"Şu anki gidişata göre fark" aynı dönemdedir:** denemeyle en düşük dönem sonunun denemesiz zincirdeki o dönemin sonuyla farkı; en düşük ↔ en düşük karşılaştırılmaz. d) **Hesaba yalnız açık ve tarihi geçmemiş denemeler girer;** geçerlilik bugüne göre serviste yeniden değerlendirilir (ekranın taşıdığı işarete güvenilmez). Hesap, ViewModel'in elindeki listeyle istenir: yazmanın bitmesi beklenmez, son istek kazanır (`S73`-2). Açık ve geçerli deneme yoksa yalnız şu anki gidişat hesaplanır (senaryo çizgisi yok). e) **Hesap düşerse** (kart bulunamadı, kural ihlali) yalnız sonuç kartı hata gösterir, liste kullanılabilir kalır. f) **Boş durum** (açık dönem yok ya da zincir kurulamıyor) sonuç kartının yerindedir; deneme listesi ve "Ekle" kalır, çünkü kullanıcı denemelerini silmek isteyebilir. g) `SimulationRiskSummary` ve `SimulationImpactRow` bu adımdan sonra hiçbir ekrana bağlı kalmaz; S76'nın silme listesinde olmadıkları için dokunulmaz, `V10d`'de (kredinin maliyeti `Risk.FinancingCost`'tan okunuyor olabilir) karara bağlanır. |

### S77 — Kayıt ekleme: düz diyalog yerine listenin dört grubunda tür seçici; seçenek, seçiciyle yer değiştiren formu açar

| | |
|---|---|
| **Eski** | `EntryTypePickerView.xaml` (78 satır, 3 `<Label>`) + `RecordEntryPicker` (76 satır, bağımlılıksız) + `ScenarioChoiceViews` (59 satır). Finansal Yapı'nın "+ Ekle"si satır içi bir alan açıyordu: üstte yan yana grup çipleri (Harcama · Borç / Kredi · Gelir · Hesap), altında seçili grubun en fazla üç seçeneği; her seçenek başlık + kısa özet + ●/○ işareti. Seçilen seçeneğin formu aynı sayfada, seçicinin altında açılıyordu. Simülatörün koşul formu aynı görünümü kullanıyordu. Katalog (`FinancialRecordEntryCatalog`) on bir seçenek sunuyordu; altısı simülatörün ortak formundan (`SharedForm`) giriliyordu. |
| **Neden yanlış** | Eskide yanlış olan seçici değil, **altındaki satır içi formdu** (`S62`-a: 86 etiket). V6'da ikisi birlikte çıkarıldı; "Ekle" düz sistem diyaloğuna indi (`S62`-7, `EK-V6`; simülatörde `EK-V10` "Plan türü grup seçici → Çıkar"). Diyalog altı satırı ayrımsız alt alta diziyor ve türün ne işe yaradığını seçimden önce söylemiyor; kullanıcı eski seçiciyi geri istedi (2026-10-03). Eski katalog da Planör'e uymuyor: `SharedForm` Finansal Yapı'da yok (`S62`-7), tür metinleri Application'da (kural 03, GK5) ve "düzenli gelir" ile "tek seferlik gelir"in ayrı formları kataloğa girmemiş. |
| **Yeni** | 1) "Ekle" ayrı bir sayfa açar ("Ne eklemek istiyorsun?"): dört grup aynı anda, her biri başlık ve altında karolar; her karo ikon + başlık + ≤ 24 harflik alt satır (yerleşim `GS29`). ●/○ işareti ve satır içi form taşınmaz: karoya dokunmak formu açar. 2) **Seçici formla yer değiştirir** (`../<rota>`): formdaki Kaydet, Vazgeç, geri ok ve cihazın geri tuşu Finansal Yapı'ya döner, seçiciye değil. 3) **Gruplar Finansal Yapı listesinin gruplarıdır**, eklenen kayıt listede aynı adlı grupta görünür. Katalog (seçenek → form): **Gelir** — Düzenli gelir, Tek seferlik gelir, Gelir değişikliği → gelirin formu (tutar değişiklikleri, `S67`-5) · **Kart** — Kredi kartı, Kartla harcama → kartın formu (gelecek harcama, `S63`-5) · **Kredi** — Bankadaki kredi, Krediye erken ödeme → kredinin formu (erken ödemeler, `S64`-6) · **Ödeme** — Nakit ödeme → planlı büyük harcama, Düzenli ödeme, Taksitli borç ve Ödeme planı → ödeme planı. Bir grupta en fazla dört seçenek (iki satır karo). Aynı forma birden fazla seçenek gidebilir: seçici kullanıcının niyetini, liste plandakini gösterir. 4) "Kredi / finansman çek" ortak formu gerektirir (para bugün gelir + taksitli geri ödeme); `V10d`'de ortak form doğunca eklenir (`S62`-7 orada karara bağlanır). 5) Üst kaydı gereken seçenekler (kart, kredi, gelir): kayıt yoksa önce ekleme önerilir, tek kayıt varsa doğrudan formu açılır, birden fazlaysa aynı sayfada ikinci seviye liste ("Hangi kart?") — açılır liste yok (`V6f2`). Liste Finansal Yapı'daki süzgeçlerle aynıdır (`S62`-5). 6) Katalog Application'da metinsiz kalır (anahtar, grup, form); başlık ve alt satır App'teki metin kataloğundadır. Ortak anahtarlar simülatör kataloğuyla aynıdır (`cash`, `card`, `recurring`, …) ki `V10c`'de çevirici ortak olsun. Eski kataloğun `SharedForm` türetmesi ve açıklama metinleri taşınmaz. 7) Seçici iki ekranın bileşenidir (`EntryTypeTiles`, `GS29`); simülatör `V10c`'de geçer ve `S76`-7'deki "tür 'Ekle' seçicisinden seçilir" bu bileşen olur. |
| **Etkiler** | `V6f1` (1, 2, 3'ün doğrudan formu olan sekiz seçeneği, 6, 7'nin bileşeni), `V6f2` (3'ün üst kayıtlı üç seçeneği, 5), `V10c` (7: simülatör geçişi; V10 notları i: kart), `V10d` (4; V10 notları i: kredi), `V10e` (V10 notları i: düzenli gelir), `S62`-7, `S76`-7, `EK-V6`, `EK-V6f`, `EK-V10`, `GS29` |
| **V6f notları** | *(2026-10-03, Kapı A)* a) Formlar kendi başlığını korur: "Nakit ödeme" "Planlı Büyük Harcama"yı, "Düzenli ödeme" ve "Taksitli borç" "Yeni Ödeme Planı"nı açar. Başlık kaydın ne olarak saklandığını ve listede hangi adla göründüğünü söyler; formlara dokunulmaz. b) Alt satırlar ≤ 24 harflik `Etiket_` metinleridir (`EK-V6f` tablosu); GK5 değişmez. c) *(Kapı C, iki tur)* Eski grup adları (Harcama · Borç / Kredi · Gelir · Hesap) kullanıcıya bir şey söylemiyordu: "Hesap" boştu, kredi iki grupta geçiyordu, bölü işareti iki kavramın zorla birleştiğini gösteriyordu. Gruplar listenin gruplarına döndü (3). Grup şeridi + seçili grup ekranın altını boş bıraktı; yerleşim konseptin ızgarasına geçti (`GS29`). "Taksitli nakit borç" konseptteki gibi "Taksitli borç" oldu. |
| **V6f2 notları** | *(2026-10-04, Kapı A)* d) Eski kodun üç yanlışı düzelir: gelir değişikliği tek bir maaş geçmişine yazılıyordu ("hangi gelir?" sorusu yoktu; Planör'de gelir çoğul), kart ve kredi formun içinde açılır listeden seçiliyordu (tek kayıtta bile), kaydın olmadığı tür seçilip form açıldıktan sonra kırmızı uyarıyla öğreniliyordu. Eski `salary-change` anahtarı yasaklı terim taşır; anahtar simülatörünkü gibi `income-change`. e) **Kayıt yoksa onay diyaloğu:** "önce ekle" kabul edilirse üst kaydın boş formu seçicinin yerine açılır (kart ve kredi formunda harcama / erken ödeme yeni kayıtta da girilir); vazgeçilirse seçici yerinde kalır. f) **İkinci seviye yerinde değişimdir:** başlık soruya döner ("Hangi kart?", "Hangi kredi?", "Hangi gelir?"), dört grup gizlenir, adaylar tam genişlik karolarda (ad + Finansal Yapı'daki bağlam satırı). Geri ok ve cihazın geri tuşu karolara döner, listeye değil. g) **Form olduğu gibi açılır:** ilgili bölüm (gelecek harcama, erken ödeme, tutar değişikliği) formun altındadır; formlara dokunulmaz (a). h) **Adaylar karoya dokununca okunur:** seçici V6f1'deki gibi anında dolu açılır, iskelet yok; okuma hatasında diyalog, seçici yerinde kalır. Aday listesi Finansal Yapı'nın süzgecidir: aktif kart, taksiti kalmış aktif kredi, aktif düzenli gelir (tek seferlik gelir aday değil). |
| **V10 notları** | *(2026-10-04, kullanıcı kararı; adım dışında verildi, kod değişmedi)* i) **Simülatör de üst kaydı seçicide çözer** (5'in simülatör hâli). `EK-V10`'un deneme formu için planlanan kart / kredi / gelir açılır listesi (V10 Kapı B, 2026-10-03; 5'ten önce onaylanmıştı) kalkar. Üst kayıt isteyen dört deneme türü — kartla harcama ve kart ödeme şekli (kart, `V10c`), krediye erken ödeme (kredi, `V10d`), gelir değişikliği (düzenli gelir, `V10e`) — simülatörün tür seçicisinde Finansal Yapı'daki gibi çözülür: adaylar Finansal Yapı'nın süzgecinden, tek adayda sorulmaz, birden fazlasında yerinde "Hangi kart?" / "Hangi kredi?" / "Hangi gelir?", geri karolara döner. **Aday yoksa yalnız uyarı:** form açılmaz ve üst kaydın formu önerilmez, çünkü simülatör planı değiştirmez (`S76`; plana yalnız "Planıma ekle" yazar, `V10f`). Deneme formu üst kaydın kimliğiyle açılır ve adını salt okunur gösterir; **düzenlemede tür gibi üst kayıt da değişmez** (başka kart için deneme silinip yeniden eklenir). Çözme mantığı `V10c`'de `RecordEntryPickerViewModel`'den iki seçicinin ortak kullandığı bir Presentation yardımcısına taşınır (o ViewModel 196 / 200 satırda); yardımcının adı ve aday karolarının bileşen olup olmayacağı `V10c` Aşama 4–5'te kararlaştırılır. `I134`, `I135`, `I137`'nin simülatör karşılıkları o adımlarda testle gelir. |
| **Durum** | `V6f1` uygulandı (2026-10-03; 1, 2, 3'ün doğrudan formu olan sekiz seçeneği, 6, 7'nin bileşeni; `I131`–`I133`); `V6f2` uygulandı (2026-10-04; 3'ün üst kayıtlı üç seçeneği, 5, V6f2 notları d–h; `I134`–`I137`). 4 `V10d`'de; 7'nin simülatör geçişi `V10c`'de; V10 notları i `V10c` (kart), `V10d` (kredi), `V10e` (düzenli gelir) adımlarında açık |

### S78 — Dönem kapanışı: özet sayfası, tek sayfada mutabakat, gelir gerçekleşmesi sorulmaz, ara bakiye gözlemi yazılmaz

| | |
|---|---|
| **Eski** | Eski `PeriodReviewPage` (515 satır XAML / 50+ Label) 5 adımlı bir sihirbazdı: gelirlerin gerçekleşmesi, kart ve kredi ödemelerinin durumu, yaşam gideri fişleri, faizler ve bakiye mutabakatı kullanıcıya tek tek soruluyordu. |
| **Neden yanlış** | a) Kullanıcı fiş ve harcama kalemi girmez (`S20`), yaşam gideri bakiyeden çözülür. b) Gelirin fiilen ne zaman ve ne kadar yattığı kullanıcıdan sorulmaz, planlandığı tarihte ve tutarda yattığı varsayılır (`S31`). c) 5 adımlı sihirbaz bilişsel yük yaratır; oysa dönem sonu kullanıcının tek ihtiyacı "kaçla kapandı, plana göre neredeyim, fark neden oldu" sorularının cevabıdır (`ana-sayfa-rota-tempo-kapanis.png`). |
| **Yeni** | 1) **Özet sayfası:** Dönem kapanışı tek sayfadır (`PeriodSettlementPage`, `PeriodSettlementViewModel`). 2) **Gelir satırı yoktur:** Gelirler planlanan gün ve tutarla yatar kabul edilir (`S31`); eksik/fazla yatan gelir yaşam gideri sapması olarak görünür (bilinen sınır, kullanıcı kararı). 3) **Otomatik açılış ve erteleme:** Çapa günü geldiğinde (`today >= plan.SettlementAvailableFrom`) uygulama açıldığında kapanış sayfası otomatik açılır. Kullanıcı `✕` ile sayfayı terk ederse kapanış ertelenmiş olur; ana sayfada "Dönemi kapat" butonu görünür ve aynı sayfayı açar. 4) **Bakiye Değiştirme (`S68-5`):** Kapanış bakiyesini değiştirmek ara gözlem yazmaz ve "Bakiye gir" sayfasını açmaz; doğrudan taslağın `ConfirmedEndingBalance` değerini günceller. 5) **Ödemeler ve Yaşam Gideri gerçekleşmesi:** Açık dönemin ödeme işaretleri (`PaymentMarks`) ve son gözlem bakiyesinden taslak otomatik üretilir (`GetObservedSettlementDraftAsync`). 6) **Dönem Kesinleştirme:** "Dönemi Kapat" komutu `IPeriodWorkflowService.FinalizeSettlementAsync` çalıştırır; yeni dönem planı dondurulur, eski dönem kapanmış snapshot'a geçer. 7) **Birden fazla bitmiş dönem:** Kullanıcı uzun süre girmediyse en eski açık dönemden başlanarak sırayla kapatılır. |
| **Etkiler** | `V11` (`PeriodSettlementViewModel`, `PeriodSettlementPage`), `V3` (`DashboardViewModel`), `EK-V11`, `GS31` |
| **Durum** | uygulandı |

### S79 — Ayarlar: yapay tahsis düzeni elendi, bildirim modu devralındı, yalın form ritmi

| | |
|---|---|
| **Eski** | Eski `SettingsPage` (169 satır XAML / 41 Label) ve `StrategyChangePage` (56 satır XAML / 11 Label); genel planlama ayarlarının yanında yapay gelir kullanım düzeni (`CashFlowAllocationMode`), düzen geçmişi ve düzen geçişi önizleme motorunu barındırıyordu. Ayrıca geliştirici için verileri silme ve test verisi yükleme butonları arayüzdeydi. Bildirim modu ise ana sayfa hatırlatıcı kartında sıkıştırılmıştı (`S4`). |
| **Neden yanlış** | a) Yapay tahsis düzeni (Upcoming/Previous) S18 ile tamamen elendi ve doğal dönemsellik ilkesi benimsendi; arayüzde düzen seçimi veya düzen modalı kalmasının bir işlevi yoktur. b) Geliştirici test operasyonları kullanıcı ekranlarında yer alamaz. c) Hatırlatıcı bildirim modu (`PaymentReminderMode`) ve izin yönetimi operasyonel kartta değil, sistemik ayarlar ekranında (`S4`) bulunmalıdır. d) Eski ekranda doğrulama hataları kırmızı metin etiketiyle gösteriliyordu; v2'de `UserSettingsValidator` kuralları çalışır ve geri bildirimler `IDialogService` ile verilir. |
| **Yeni** | 1) **Yalın form ritmi:** Ayarlar tek sayfadır (`SettingsPage`, `SettingsViewModel`). Kart kutuları kullanılmaz, alanlar dikey form ritmi ve ayırıcılarla gruplanır (`GS33`). 2) **Planlama ayarları:** Dönem çapa günü (`AnchorDay`, 1–31), dönemsel yaşam gideri havuzu (`PeriodVariableExpenseAllowance`), kredi kartı devreden borç faizi (%) ve finansman açığı KMH faiz oranı (%) düzenlenir. Açılış bakiyesi S19 uyarınca burada düzenlenmez. 3) **Hatırlatıcı bildirim modu:** `IPaymentReminderService` üzerinden Kapalı, Rahat ve Agresif modları tek dokunuşla seçilir (`S4`). 4) **Yedekleme:** `IBackupService` üzerinden son yedekleme zamanı ve dosya adı gösterilir; kullanıcı tek tıkla "Şimdi Yedekle" çalıştırabilir. 5) **Hakkında:** Ürün adı Planör (`GK11`), sürüm bilgisi ve yerel depolama güvencesi sunulur. |
| **Etkiler** | `V13` (`SettingsViewModel`, `SettingsPage`), `EK-V13`, `GS33` |
| **Durum** | uygulandı |


### S80 - CI: tek APK doğrulayıcı, PR'da da çalışır; eski app id, prerelease ve v1.0.0 sürekliliği taşınmaz

| | |
|---|---|
| **Eski** | `dev-build.yml` (175 satır) ve `release.yml` (240 satır) yalnız `push`/tag ile çalışıyordu. APK doğrulaması iki dosyada kopyalanmış ~40 satırlık satır içi bash'ti: `apksigner verify`, `CN=Android Debug` reddi, `aapt dump badging` ile package id (`com.coinflow.mobile(.dev)`), versionCode (dev: run numarası, release: 100000 + run), versionName ve label (`Mizan` / `Mizan Dev`) karşılaştırması, `unzip -t`. Release ayrıca v1.0.0 APK'sıyla imza SHA-256 sürekliliğini denetliyor, `dev-latest` prerelease'ini yeniden yaratıyor, sürüm notlarını iş akışı içinde `echo` ile üretiyordu. |
| **Neden yanlış** | a) Doğrulama test edilmiyordu; bir `sed` deseni bozulsa kontrol boş değerle sessizce çalışırdı ve iki kopya birbirinden ayrışabilirdi. b) Birleşmiş manifestteki izinlere bakılmıyordu; Sentry'nin getirdiği `INTERNET` izni tam bu açıktan girdi (`S60`). c) v2 yeni app id'dir (`com.mizan.app`); v1.0.0 sürekliliği eski anahtarın işidir. d) Sürüm notları ve prerelease yayını hiçbir kalkan sorusuna bağlanmıyor. e) v2'nin `ci.yml`'si `maui-android` workload'unu ve JDK'yı kurmuyordu; `net8.0-android` hedefli `Mizan.App` yüzünden restore Ubuntu'da düşerdi. |
| **Yeni** | 1) **Tek doğrulayıcı:** APK kuralları C#'ta saf bir kural sınıfıdır (`aapt2 dump badging`, `apksigner --print-certs` ve `aapt2 dump xmltree` çıktısını alır, ihlal listesi döner); negatif fixture'larla `Mizan.Regression.Tests`'te test edilir. CI ve yerel kullanım aynı kuralları `tools/Mizan.ApkVerifier` konsol aracı üzerinden, `scripts/verify-apk.ps1` sarmalayıcısıyla koşar. 2) **Kontroller:** imza geçerli; package id `com.mizan.app`; label `Planör` (`GS6`); versionCode ve versionName csproj ile (release'de tag `vX.Y.Z` ile) eşleşir; birleşmiş manifestte `INTERNET`/`ACCESS_NETWORK_STATE` yok (`uses-permission` ve `uses-permission-sdk-23` ikisi de) ve `allowBackup="false"` (`S60`, `I40`); `-RequireReleaseSignature` verildiğinde debug sertifikası reddedilir. 3) **PR kapısı:** `ci.yml` PR ve push'ta workload + JDK kurar, derler, test eder, kapsamı denetler, Release APK üretip doğrular. PR APK'sı debug imzalıdır; debug reddi yalnız tag'le çalışan `release.yml`'dedir ve imza secret'ları zorunludur. 4) **Kapsam eşikleri yükseldi:** Domain %95, Application %95, Presentation %90, Infrastructure %80 (yeni). 5) **Sürüm otoritesi csproj'dur;** CI sürüm üretmez, tutarlılığı denetler. 6) Taşınmayanlar: `dev-latest` prerelease, gömülü sürüm notları, v1.0.0 imza sürekliliği, dev varyantı (`.dev` app id). 7) **Eksik katman kırmızıdır:** kapsam betiği dört katmanın dördünü de ölçülmüş görmek zorundadır; 0 satır ölçülen ya da hiç görünmeyen katman eşiği atlatmaz, kapıyı düşürür (K2a'nın "okunamayan değer ihlaldir" ilkesi). 8) **`release.yml` test adımlarını kopyalamaz:** `ci.yml` `workflow_call` ile çağrılabilir; `release.yml` önce onu çağırır, geçerse imzalı APK'yı üretir ve `-RequireReleaseSignature` ile doğrular. |
| **Etkiler** | `K2a` (doğrulayıcı, 1–2), `K2b` (iş akışları ve eşikler, 3–8), `S60` (birleşmiş manifest açığı kapandı), `I40`, `F4` |
| **İlgili** | Repoda henüz git remote yok; kapının zorlanması (branch protection: `main`'e doğrudan push kapalı, `CI` zorunlu durum denetimi) remote açıldığında GitHub ayarlarından elle yapılır. Release secret'ları: `MIZAN_KEYSTORE_BASE64`, `MIZAN_KEYSTORE_PASSWORD`, `MIZAN_KEY_ALIAS`, `MIZAN_KEY_PASSWORD`. |
| **Durum** | uygulandı: 1–2 `K2a`'da; 3–8 `K2b`'de |

### S81 — Emülatör regresyonu: tıklayan Maestro'dur, betik yalnız hazırlar ve sonucu olduğu gibi döner

| | |
|---|---|
| **Eski** | `run-emulator-regression-tests.ps1` (688 satır) kendi tıklama ve doğrulama motorunu taşıyordu: `uiautomator dump` çıktısını regex'le tarar, düğüm bulunamazsa sabit koordinata (`input tap 75 136`, `438 1400`, `538 <y>`) düşer, her senaryoyu `try/catch` içinde `Record-Test` ile kaydeder. Doğrulamalar `$xml -match "Kredi\|Kart\|TL\|Net"` gibi hemen her ekranda doğru çıkan desenlerdi. App id eski (`com.coinflow.mobile.dev`), veri koruması yoktu. |
| **Neden yanlış** | a) Yedek koordinatla tıklayıp sonucu koşulsuz başarılı saymak, kırık bir ekranı yeşil gösterir; kalkanın tek işi bunu yakalamaktı. b) Gevşek regex bir ekranın *var olduğunu* değil *bir şey çizildiğini* sınar. c) K1 aynı akışı `AutomationIds` sembollerinden derleme zamanında tip-güvenli üretiyor; ikinci bir elle yazılmış akış onunla ayrışır. d) K1 akışı `clearState: true` ile başlıyor: korumasız koşarsa emülatördeki profiller (telefon verisi dahil) silinir (V7'deki kayıp). |
| **Yeni** | 1) **Tek akış, tek tıklayıcı:** gezinme ve doğrulama yalnız `.maestro/flows/full_regression_flow.yaml`'dadır (K1 üretir); betik `maestro test` çağırır, koordinat, `input tap/swipe` ve ekran dökümü regex'i içermez. 2) **Sonuç yutulmaz:** betiğin çıkış kodu Maestro'nun çıkış koduyla birebir aynıdır; başarı iletisi yalnız kod 0 ise yazılır. 3) **Veri koruması:** hazırlık `scripts/emulatorde-ac.ps1`'e bırakılır (yeni APK, profil yedeği); profil varken yeni yedek oluşmadıysa akış hiç koşmaz. Akıştan sonra (hata olsa da, `finally`) `files/profiles` boşaltılıp yedek `-GeriYukle` ile geri yüklenir; akışın açtığı test profili emülatörde kalmaz. 4) **Maestro CLI kullanıcının kurulumudur;** yoksa betik net bir hata ile durur, kendisi indirmez. 5) Taşınmayanlar: `Record-Test`/`Capture-Screen` altyapısı, `Navigate-Flyout` koordinat tablosu, `matrix` ve Firebase betikleri, `MizanDevBuild`/`.dev` app id. |
| **Etkiler** | `K3`, `K1` (akışın ilk gerçek koşusu), `emulatorde-ac.ps1` (değişmez, çağrılır) |
| **İlgili** | Betik korumasını `EmulatorScriptTests` kaynak metninden sınar (K7); gerçek koşu Maestro kurulumunu ve emülatörü gerektirir, CI'da koşmaz. |
| **Durum** | uygulandı: `K3` (gerçek emülatör koşusu Maestro kurulunca) |

### S82 — Sürüm notu veridir: `CHANGELOG.md`'den okunur, iş akışında `echo` ile üretilmez

| | |
|---|---|
| **Eski** | `release.yml` (eski repo) sürüm notunu 8 ardışık `echo ... >> release-notes.md` ile üretiyordu; başlık ("Dondurulmuş Plan Kilitleme…"), madde metinleri ve "588/588 birim testi" gibi rakamlar iş akışına gömülüydü. Eski repoda `CHANGELOG.md` yoktu. v2'de geçici çözüm `gh release create --generate-notes` (`S80`-6). |
| **Neden yanlış** | a) Not bir sürüme bağlı değildi: sonraki etiket atıldığında iş akışı eski sürümün notunu, yeni sürümün başlığıyla yayınlardı ve bunu hiçbir şey yakalamazdı. b) Not kodun içinde olduğu için sürümü artıran PR notu da getirmek zorunda değildi; metin YAML'da kayboluyordu. c) `--generate-notes` commit başlıklarının dökümüdür; kullanıcıya dönük değildir ve Türkçe konvansiyonla uyuşmaz. d) Okuyucu yoksa "boş not" ve "yanlış sürümün notu" test edilemez. |
| **Yeni** | 1) **Not `CHANGELOG.md`'dedir** (Keep a Changelog, Türkçe): her sürüm `## [X.Y.Z] - YYYY-AA-GG` başlığı ve altında en az bir `- ` maddesi taşır; alt başlıklar (`### Eklendi`, `### Değişti`, `### Düzeltildi`) serbesttir. `## [Yayınlanmamış]` bölümü bir sürüm değildir ve yayınlanamaz. 2) **Tek okuyucu:** `tools/Mizan.ReleaseNotes` içindeki saf `ChangelogReader` (metin + sürüm → not) bölümü bulur; **bölüm yoksa, boşsa (madde yoksa), başlığı ya da tarihi okunamıyorsa ya da aynı sürüm iki kez geçiyorsa ihlaldir** (`K2a`'daki "okunamayan değer ihlaldir" ilkesi). Negatif fixture'larla `Mizan.Regression.Tests`'te sınanır. 3) **`release.yml` notu `scripts/release-notes.ps1` ile üretir** ve `gh release create --notes-file` verir; `--generate-notes` ve `echo >> release-notes` yasaktır. Not eksikse imzalama başlamadan önce durur. 4) **Eksiklik PR'da yakalanır:** `Mizan.Architecture.Tests` içindeki bir test csproj'daki `ApplicationDisplayVersion`'ın `CHANGELOG.md`'de dolu bir bölümü olmasını arar; `ci.yml` zaten `dotnet test Mizan.sln` koştuğu için ayrı bir CI adımı gerekmez, sürümü artıran PR notu da getirmek zorundadır. 5) Sürüm otoritesi csproj'dur (`S80`-5); okuyucu sürüm üretmez, yalnız o sürümün notunu okur. |
| **Etkiler** | `K4`, `S80`-6 (gömülü sürüm notları taşınmaz → notun yeni yeri), `release.yml` |
| **İlgili** | Etiket ile csproj sürümünün eşleşmesini `verify-apk.ps1 -Tag` denetler (`S80`-5); okuyucu etiketin sürümünü alır, böylece not etiketle, etiket csproj'la bağlanır. |
| **Durum** | uygulandı: `K4` |

### S83 — Eski yedekten içe aktarma: dönüşüm v1'e yapılır, eski adlar tek klasörde yaşar, taslaklar taşınmaz, planın geliri toplamdan türetilir

| | |
|---|---|
| **Eski** | Eski uygulamanın yedeği (`Format = 1`, `profiles/{id:N}/coinflow.db3`, şema v17) v2'de yalnız tanınıp reddediliyordu (`S57`, `I37`). Telefon verisi 2026-09-27'de tek seferlik, elle yazılmış bir SQL'le (`v17-to-v1.sql`, depo dışı) v1'e çevrilmişti: gelir akışının adı "Maaş", eski simülasyon taslakları boş taşınmıştı. Eski kolon adları (`SalaryDay`, `OpeningSavings`, `ReviewAvailableFrom`, `StrategyUsed`, `ProjectionStartingSavings` …) yasaklı terimler tablosuna (K9) takılır. |
| **Neden yanlış** | a) Elle SQL kalıcı bir yol değil: testi yok, kullanıcı kendi telefonunda çalıştıramaz. b) Dönüşüm hedefi güncel sürüm (v3) olsaydı, her yeni şema adımı içe aktarıcıyı da kırardı; v1 dondurulmuştur (`SchemaMigrationsTests.YayimlanmisAdimlar_Degismez`). c) Eski taslakların koşulları v2'de tek çalışma listesine sığmaz: bir kısmı v2'de olmayan türlerdir (maaş değişimi, strateji değişimi), tarihleri de içe aktarma günü geçmiş olur (`S76`-3); `S76` listeyi saklamayı karara bağlamıştı, eski adlı planları taşımayı değil. d) Eski adları yasaklı terim testine karşı dağınık bırakmak ya testi gevşetir ya da her dosyada istisna ister. e) "Maaş" adı yasaklı `maaş` terimidir (`S2`: gelir akışı maaşla sınırlı değil). |
| **Yeni** | 1) **Hedef v1:** her eski profil önce `SchemaMigrations.All[0]` ile kurulmuş boş bir v1 veritabanına dönüştürülür, sonra `ProfileImportTransaction`'ın kullandığı `DatabaseSchema` onu güncel sürüme yükseltir. 2) **Tek klasör:** eski şemanın adlarını (tablo, kolon, `coinflow.db3`) yalnız `src/Mizan.Infrastructure/LegacyImport/` içindeki dosyalar ve onları sınayan `tests/.../LegacyImport/` taşır; `docs/SOZLUK.md` yasaklı terim tablosunda `Salary`, `CoinFlow`, `Savings` ve `Review` satırlarının istisnası bu klasördür. Üretim kodunun geri kalanında eski ad geçemez. 3) **Taslaklar taşınmaz** (Hiç taşıma): `simulation_drafts` ve koşulları atlanır, çalışma listesi boş başlar. 4) **`S31`:** her plan ve revizyon için toplam `PlannedIncome > 0` ise tek gelir satırı türetilir (tarih: `StrategyUsed` 0 → dönem başı, 1 → dönem sonu); satırların toplamı plan gelirine kuruşu kuruşuna eşittir (`I28`). Açık plan yeniden dondurulmaz: bu Infrastructure'da iş kuralı hesaplamak olurdu (M7). 5) **`S2`/`S3`:** `salary_schedule` boş değilse tek bir düzenli gelir akışı, adı **"Gelir"** (yasaklı `maaş` yerine), ödeme günü eski global gün; her `salary_schedule` satırı o akışın tutar geçmişi satırıdır. Boşsa akış kurulmaz. 6) **Yetim satırlar** (karşı tablosu olmayan çocuklar) elenir; eski şemada yabancı anahtar zorlanmıyordu. 7) **Normal geri yükleme eskisi gibi** biçim 1'i reddeder (`I37`); içe aktarma ayrı bir port (`ILegacyBackupImporter`) ve ayrı bir giriştir. 8) **İki adım:** `G1a` Infrastructure içe aktarıcısı + port + testler; `G1b` profil seçimi ekranında giriş. |
| **Etkiler** | `G1a`, `G1b`, `F1` (K9 testi `LegacyImport/` istisnasını tablodan okur), `S57`, `S58`, `S76` |
| **İlgili** | Telefon verisinin 27 Eylül'deki elle dönüşümü yalnız ad ("Maaş") ve taslaklar bakımından bundan ayrılır; kalıcı yol artık budur. |
| **Durum** | uygulandı: 1–7 `G1a`'da; 8'in `G1b` yarısı açık |
