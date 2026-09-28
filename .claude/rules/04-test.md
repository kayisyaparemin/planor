---
paths:
  - "tests/**"
---

# 04 — Test Protokolü

## Altı test projesi, altı sınır

| Proje | Referans verdiği | Neyi korur |
|---|---|---|
| `Mizan.Domain.Tests` | **yalnız** Domain | Saf hesap. Servise uzanamaz çünkü göremez. |
| `Mizan.Application.Tests` | **yalnız** Application | Kullanım senaryoları, el yapımı fake portlarla |
| `Mizan.Infrastructure.Tests` | **yalnız** Infrastructure | Gerçek SQLite, geçici dosya |
| `Mizan.Presentation.Tests` | **yalnız** Presentation | ViewModel davranışı |
| `Mizan.Architecture.Tests` | beşi birden | Kural kitabının kendisi (K1–K9) |
| `Mizan.Regression.Tests` | Domain + Application + Infrastructure | Uçtan uca finansal doğruluk |

Bu ayrım kozmetik değil: eski projede tek bir `Mizan.Tests` vardı ve bir domain testi
rahatça `TestFactory.Service()` çağırabiliyordu. Saf hesap çekirdeği bu yüzden servislere dolandı.

## Sahte test yasağı

`File.ReadAllText` ile kaynak kodu veya XAML metnini tarayan test **yalnızca**
`Mizan.Architecture.Tests` içinde olabilir. Orada meşrudur, çünkü işi zaten kuralı denetlemektir.

Başka herhangi bir test projesinde kaynak dosya okumak, `ArchitectureTests.KaynakTarayanTest_YalnizBuradaOlabilir`
testini kırar.

## Yasaklı terim testi (K9)

`Mizan.Architecture.Tests` içindeki `YasakliTerimler_KaynaktaGecemez`, `docs/SOZLUK.md`'deki
yasaklı terimler tablosunu **okur** ve `src/` ile `tests/` altını tarar. Tablo tek kaynak
olduğu için doküman ile test birbirinden kayamaz.

Tablo `<!-- YASAKLI-TERIMLER:BASLANGIC -->` ve `<!-- YASAKLI-TERIMLER:BITIS -->` işaretleri
arasındadır. **İşaretler bulunamazsa test kırmızıya düşer** — sessizce devre dışı kalmaz.

**Eşleştirme PascalCase token bazlıdır**, düz metin araması değil. Tanımlayıcı önce büyük harf
ve alt çizgi sınırlarından parçalara ayrılır, sonra parçalar büyük/küçük harf gözetmeden
karşılaştırılır. Bu, üç bilinen yanlış pozitifi kendiliğinden eler:

| Görünürde ihlal | Parçalar | Neden temiz |
|---|---|---|
| `MigratePeriodPlanRevisionSchemaAsync` | `Migrate·Period·Plan·Revision·Schema·Async` | hiçbiri `maas` değil |
| `PreviewSettlementAsync` | `Preview·Settlement·Async` | `Preview` ≠ `Review` |
| `SavingsGoal` | `Savings·Goal` | `Goal` komşuluğu muaf |

Türkçe `maaş` (ş ile) ayrıca düz metin olarak aranır; `ş` harfi yanlış pozitif üretemez.

**Testin kendisinin de testi vardır.** `YasakliTerimRegex_YanlisPozitifUretmez` yukarıdaki üç
örneği sabitler. Biri eşleştirmeyi gevşetirse o test kırmızıya düşer. Yeni bir yanlış pozitif
bulduğunda terimi listeden çıkarma — **istisnayı** yaz ve örneği bu teste ekle.

## Mocking kütüphanesi yasak

Moq, NSubstitute, FakeItEasy — hiçbiri kurulmaz. Sahteler **elle** yazılır ve ilgili test
projesinin `Fakes/` klasöründe yaşar.

Sebep: bu projenin amacı kodun okunabilmesi. `Substitute.For<IX>()` + `Received(1)` zinciri
testin ne iddia ettiğini gizler; 15 satırlık bir `FakeLoanRepository` ise açıkça okunur ve
gerçek davranışı (çağrı sayısı, gecikme, iptal, hata) modelleyebilir.

```csharp
/// <summary>Testlerde gerçek saat yerine kullanılan sabit saat.</summary>
internal sealed class SabitSaat(DateOnly bugun) : IClock
{
    public DateOnly Today { get; } = bugun;
    public DateTimeOffset UtcNow { get; } = new(bugun.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
}
```

## İsimlendirme ve yapı

`Metot_Senaryo_BeklenenDavranis` — üçü de Türkçe okunabilir olmalı:

```
Hesapla_BakiyeNegatifse_AcikFaiziIsletir
DonemKapat_AcikYukumlulukVarsa_SonrakiDonemIlkGunuyleDevreder
```

Her test üç bölümden oluşur ve bölümler boş satırla ayrılır: **Hazırla / Uygula / Doğrula**
(Arrange / Act / Assert). Bir testte tek bir `Act` bulunur.

## Zorunluluklar

- **`DateTime.Now` / `DateTime.Today` / `DateTimeOffset.Now` hiçbir yerde kullanılmaz.**
  Her zaman `IClock`. Deterministik olmayan test yazılmaz.
- **Her hata düzeltmesi kırmızı bir testle başlar.** Testi yazmadan düzeltmeye başlama.
  Test, hatanın kullanıcı tarafından görülen hâlini tarif eder, düzeltmenin iç yapısını değil.
- **Her invariant'ın onu koruyan bir testi vardır** ve `docs/INVARYANTLAR.md` tablosunda
  testin tam adı yazılıdır. Adı yazılmamış invariant, invariant değildir.
- **Test çıktısı yeşil değilse iş bitmemiştir.** "İlgisiz bir test kırmızı" diye devam edilmez.

## Kapsam eşiği

`.runsettings` ile coverlet toplanır, CI eşiği zorlar:

| Katman | Alt sınır |
|---|---|
| `Mizan.Domain` | **%90** |
| `Mizan.Application` | **%80** |
| `Mizan.Presentation` | **%70** |
| Diğer | eşik yok |

Eski projede coverlet kuruluydu ama hiç toplanmıyordu ve hiçbir eşik yoktu; "600/600 yeşil"
rakamı neyin test edilmediğini söylemiyordu.

## İzolasyon

Veritabanına dokunan testler GUID adlı geçici bir dosya kullanır ve `finally` içinde
`.db3`, `.db3-shm`, `.db3-wal` üçlüsünü siler. Paylaşılan fixture kullanılmaz.

## Çalıştırma

```bash
dotnet test Mizan.sln
```
```bash
dotnet test tests/Mizan.Domain.Tests
```
```bash
dotnet test Mizan.sln --filter "FullyQualifiedName~Regression"
```
