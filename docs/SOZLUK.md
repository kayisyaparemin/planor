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
| gerçekleşme | `PeriodActual` | Dönem kapanışında ölçülen fiilî durum |
| gözlem | `PeriodObservation` | Dönem içinde kullanıcının girdiği anlık bakiye |
| dönem kapanışı | `PeriodSettlement` | Planın gerçekleşmeyle mutabakatı |

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

> Birden fazla düzenli gelir **toplanır**, birbirini ezmez. Bu, eski projede bir hataydı
> (`SAPMALAR.md` → S2) ve yeni projede bir invariant'tır.

## Dönemsellik ve Bakiye

| Türkçe | Kod | Tanım |
|---|---|---|
| doğal dönemsellik | `period.Contains(date)` | Vadesi dönemin `[Start, End)` aralığına düşen her nakit akışının doğrudan o döneme ait olması ilkesi (S18) |
| açık faizi | `DeficitFinancingInterest` | Negatif bakiyenin maliyeti (KMH) |

## Kredi

| Türkçe | Kod | Tanım |
|---|---|---|
| kredi | `Loan` | Banka kredisi |
| itfa | `Amortization` | Anapara/faiz ayrışması |
| taksit | `Installment` | Aylık ödeme kalemi |
| kalan borç | `RemainingDebt` | Bugün itibarıyla kalan anapara |
| erken ödeme | `Prepayment` | Plan dışı anapara ödemesi |
| erken kapama | `Payoff` | Krediyi tümüyle kapatma |

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

## Vadeli ve Geçici Borç Planı

| Türkçe | Kod | Tanım |
|---|---|---|
| geçici ödeme planı | `TemporaryPaymentPlan` | Kredi ve kredi kartı haricindeki vadeli borç, senet, taksit ve periyodik yükümlülük sözleşmesi |
| plan taksiti | `TemporaryPaymentInstallment` | Geçici ödeme planına ait tekil takvimli ödeme kalemi |
| plan türü | `PaymentPlanKind` | Borç planının niteliği (Geçici, Taksitli, Periyodik veya Diğer) |
| plan doğrulayıcı | `TemporaryPaymentPlanValidator` | Geçici ödeme planı ve taksitlerinin iş kurallarına uygunluğunu denetleyen saf sınıf |

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
| yedek | `Backup` | Tüm profilleri içeren tek arşiv |

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
