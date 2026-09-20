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
| dönem | `CashFlowPeriod` | İki maaş arasındaki yarı açık aralık `[başlangıç, sonraki)` |
| dönem başlangıcı | `PeriodStart` | Maaşın yattığı gün, döneme **dahil** |
| dönem sonu | `PeriodEnd` | Sonraki maaş günü, döneme **dahil değil** |
| açılış bakiyesi | `OpeningBalance` | Dönemin ilk günündeki nakit |
| kapanış bakiyesi | `EndingBalance` | Dönemin son anındaki nakit; sonraki dönemin açılışına eşittir |
| projeksiyon | `Projection` | İleriye dönük dönem dizisi hesabı |
| yükümlülük | `Obligation` | Bu dönemde ödenmesi gereken her kalem |
| zorunlu çıkış | `MandatoryOutflow` | Yükümlülüklerin toplamı |
| yaşam gideri | `VariableExpenseAllowance` | Dönem için ayrılan serbest harcama bütçesi |
| dönem planı | `PeriodPlanSnapshot` | Dönem başında **dondurulan** taahhüt |
| gerçekleşme | `PeriodActual` | Dönem kapanışında ölçülen fiilî durum |
| gözlem | `PeriodObservation` | Dönem içinde kullanıcının girdiği anlık bakiye |
| dönem kapanışı | `PeriodSettlement` | Planın gerçekleşmeyle mutabakatı |

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

## Gelir ve düzen

| Türkçe | Kod | Tanım |
|---|---|---|
| maaş | `SalaryScheduleEntry` | Etkin tarihli maaş kaydı |
| ek gelir | `OneTimeIncome` | Tek seferlik gelir |
| kullanım düzeni | `CashFlowAllocationStrategy` | Paranın hangi döneme yazılacağı kararı |
| açık faizi | `DeficitFinancingInterest` | Negatif bakiyenin maliyeti (KMH) |

## Profil ve yedek

| Türkçe | Kod | Tanım |
|---|---|---|
| profil | `UserProfile` | Bağımsız bir veri kümesi; her biri ayrı `.db3` |
| yedek | `Backup` | Tüm profilleri içeren tek arşiv |
