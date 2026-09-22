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


