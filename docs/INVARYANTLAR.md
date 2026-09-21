# Invariantlar

Bu uygulamanın **hiçbir koşulda bozulmaması gereken** davranışsal sözleşmeleri.

## İki kural

1. **Tek numaralandırma.** Bir kod (`I3`) bu dosyada tam olarak bir şeyi işaret eder.
   Eski projede `I16` iki farklı invariant'ı gösteriyordu ve hangisinin kastedildiği
   konuşulan dosyaya göre değişiyordu.
2. **Testi yazılmamış invariant, invariant değildir.** "Koruyan test" sütunu boş
   kalamaz. Boşsa satır bu tabloya girmez.

## Tablo

| Kod | Kural | Koruyan test | Adım |
|---|---|---|---|
| `I1` | Ay sonu ve artık yıl geçişlerinde tercih edilen gün hafızada tutulur; kısa aylarda ay sonuna kenetlenir, uzun aylara geçildiğinde orijinal gün geri kazanılır (BR-CALENDAR-01). | `Mizan.Domain.Tests.Calculations.CalendarRulesTests.AddMonthsKeepingDay_KisaAydanSonraUzunAyaGecildiginde_TercihEdilenGunuGeriKazanir` | `F2` |
| `I2` | Para her zaman 2 ondalık basamakla ve `MidpointRounding.AwayFromZero` ile yuvarlanır; taksit ve eşit bölüştürmelerde kuruş artığı son parçaya eklenerek para kuruşu kuruşuna korunur (BR-MONEY-01). | `Mizan.Domain.Tests.Calculations.MoneyRulesTests.Distribute_TamBolunmeyenTutar_KurusArtiginiSonTaksiteEkler` | `F3` |
| `I3` | Nakit akış dönemleri yarı açık aralıktır: [başlangıç, bitiş). Başlangıç günü döneme dahil, bitiş günü dahil değildir; ardışık dönemlerin birleşiminde boşluk veya çakışma oluşamaz. | `Mizan.Domain.Tests.Models.CashFlowPeriodTests.Contains_YariAcikAralikKuraliniUygular` | `D3` |
| `I4` | Çoklu düzenli gelir akışları bağımsızdır ve birbirini ezmez; her akışın kendi etkin tarihli tutar geçmişi taranır ve dönem başlangıcı itibarıyla (EffectiveDate <= Period.Start) yürürlükte olan en güncel tutar çözümlenir, dönem içi zamlar o dönemi etkilemez (BR-INCOME-01). | `Mizan.Domain.Tests.Calculations.IncomeResolverTests.Resolve_BirdenFazlaAktifAkisVarsa_HerIkiGeliriDeCozumler_BirincisiEzilmez` | `D4` |
| `I5` | Kredi kartı güncel toplam borcu (KnownTotalDebt), ekstre kesilmişse ekstre tutarı ile ekstre tarihinden sonraki harcamaların toplamıdır; ekstre yoksa devreden bakiye, dönem içi harcama ve tüm gelecek harcamaların toplamıdır (BR-CARD-01). | `Mizan.Domain.Tests.Models.CreditCardTests.KnownTotalDebt_KesilmisEkstreVarken_EkstreTutariniVeYalnizcaSonrakiHarcamalariToplar` | `D7` |
| `I6` | Kredi kartı asgari ödeme oranı yasal mevzuata (BDDK) tabidir; kart limiti 25.000 TL ve altında ise %20, 25.000 TL üzerinde ise %40 olarak çözümlenir (BR-CARD-04). | `Mizan.Domain.Tests.Calculations.CreditCardRulesTests.ResolveMinimumPaymentRate_LimitSinirinaGore_DogruBddkOraniniUretir` | `D7` |
| `I7` | Kredi ve kart haricindeki vadeli ödeme planlarının kalan borcu (RemainingAmount), yalnızca henüz ödenmemiş (IsPaid == false) taksitlerin toplamıdır; ödenen taksitler anında borçtan düşer ve tüm taksitler ödendiğinde plan tamamlanmış sayılır. | `Mizan.Domain.Tests.Models.TemporaryPaymentPlanTests.RemainingAmount_YalnizcaOdenmemisTaksitleriToplar` | `D8` |
| `I8` | Kredi kartı ekstre ödeme tercihleri append-only tarihçe olarak saklanır; belirli bir ekstre kesim tarihi için yürürlükteki tercih, EffectiveFromStatementDate <= statementDate şartını sağlayan en güncel kayıttır; aynı tarihte birden fazla varsa en son kaydedilen (CreatedAt) kazanır. | `Mizan.Domain.Tests.Calculations.CreditCardPaymentPreferenceResolverTests.Resolve_AyniGundeBirdenFazlaKayitVarsa_EnSonOlusturulanKazanir` | `D11` |
| `I9` | Projeksiyonun ilk döneminde, çapa tarihi ile ilk dönem başlangıcı arasındaki bekleme penceresine [prePeriodIncomeStart, period.Start) düşen tek seferlik arızi gelirler ilk döneme dahil edilir; çapa öncesindeki (< prePeriodIncomeStart) gelirler ise açılış bakiyesinde kabul edilerek mükerrer sayımı önlemek için projeksiyondan hariç tutulur (BR-INCOME-01). | `Mizan.Domain.Tests.Calculations.IncomeProjectionCalculatorTests.Calculate_CapaOncesiPencereVerildiginde_PencereyeDusenTekSeferlikGelirleriDahilEder` | `D12` |
| `I10` | Kredi taksiti ödendiğinde anaparadan taksitin tamamı değil, yalnızca aylık faiz düşüldükten sonra kalan anapara payı düşülür; taksit o ayın faizinden azsa anapara büyür (BR-LOAN-01). Erken kapama ücreti 6502 sayılı Kanun uyarınca tüketici kredisinde %0, değişken faizli konut kredisinde %0, sabit faizli konut kredisinde vadesine göre %1 veya %2 olarak sınırlandırılır (md. 27, 37). | `Mizan.Domain.Tests.Calculations.LoanAmortizationCalculatorTests.PrincipalAfterPayment_TaksitOdendiginde_YalnizAnaparaPayiniDuser` | `D13` |


## Satır eklerken

- **Kural** kullanıcının görebileceği bir davranış olarak yazılır, iç yapı olarak değil.
  ✅ "Dönem içi plansız harcama, dönem başında dondurulan planı değiştirmez."
  ❌ "`PeriodPlanSnapshotService.Freeze` çağrılmaz."
- **Koruyan test** testin tam adıdır: `Mizan.Regression.Tests.DonemPlaniTests.DonemIciHarcama_DondurulmusPlaniDegistirmez`
- **Adım** `docs/TASIMA-PLANI.md`'deki adım kodudur (örn. `A10`).

## Kasıtlı sadeleştirmeler

Hata sanılıp "düzeltilmemesi" gereken bilinçli kararlar buraya yazılır.

| Karar | Gerekçe |
|---|---|
| Yapay dönem kullanım düzeni (`UpcomingPeriod` / `PreviousPeriod`) ve harcama kaydırma mekanizması (`CashFlowAllocationPlanner`) elendi. Harcamalar ve gelirler doğrudan vadesinin düştüğü yarı açık aralıktaki `[PeriodStart, PeriodEnd)` döneme aittir (doğal dönemsellik). | Mizan v2'de çoklu gelir akışı (`S2`) ve bağımsız dönem çapası (`S1`) benimsendiği için harcamayı yapay olarak maaş öncesi/sonrası döneme kaydırmak dönemsellik muhasebe ilkesini bozuyor ve 600+ satırlık yapay karmaşa (catch-up, forward-funded) üretiyordu. Likidite farkı dönem içi bakiye (KMH) konusudur, bütçe tahsis konusu değildir (`S18`). |
