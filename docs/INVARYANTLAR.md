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
| — | *(taşıma ilerledikçe dolacak)* | — | — |

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
