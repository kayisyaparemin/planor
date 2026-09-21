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
| *(henüz yok)* | |
