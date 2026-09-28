---
paths:
  - "src/**/*.cs"
  - "tests/**/*.cs"
---

# 01 — Mimari ve Katmanlar

## Bağımlılık yönü

```
Mizan.App  (net8.0-android)   XAML, platform kodu, kompozisyon kökü
   │  ├──► Mizan.Presentation
   │  ├──► Mizan.Application
   │  └──► Mizan.Infrastructure   ◄── YALNIZCA burada; başka hiçbir yerden değil
   │
Mizan.Presentation (net8.0)   ViewModel'ler.  MAUI YOK. Infrastructure YOK.
   │  ├──► Mizan.Application
   │  └──► Mizan.Domain
   │
Mizan.Infrastructure (net8.0) Adaptörler: SQLite, dosya sistemi, PDF, Sentry
   │  └──► Mizan.Application
   │
Mizan.Application (net8.0)    Portlar (arayüzler) + kullanım senaryoları
   │  └──► Mizan.Domain
   │
Mizan.Domain (net8.0)         Saf hesap. Sıfır paket, sıfır I/O, sıfır saat.
```

Yukarı doğru hiçbir ok yoktur. `Mizan.Presentation` MAUI'ye **referans veremez** — bu yüzden
"ViewModel'de `Shell.Current` kullanma" bir kural değil, bir derleme hatasıdır.

## Katman başına ne girer

| Katman | Girer | Kesinlikle girmez |
|---|---|---|
| **Domain** | `record` modeller, enum'lar, saf hesaplayıcılar | NuGet paketi, `async`, dosya/DB, `DateTime.Now`, kültür/format |
| **Application** | Port arayüzleri (`Abstractions/`), kullanım senaryoları (`Services/`), DTO'lar (`Models/`) | MAUI, SQLite, para/tarih formatlama |
| **Infrastructure** | Port implementasyonları, SQL, dosya, PDF, telemetri | **İş kuralı hesabı** |
| **Presentation** | `ViewModel`'ler, `AutomationIds`, sunum DTO'ları | MAUI tipi, `Page`, `Shell`, SQLite |
| **App** | XAML, code-behind, `Platforms/`, `MauiProgram.cs` | İş mantığı, hesap |

## Düğüm çözme kuralları

Bunların hepsi eski projede **gerçekten olmuş** hatalardır. Kod incelemesinde önce bunlara bak.

### M1 — Hesaplayıcı kendi bağımlılığını `new`lemez
Bir hesaplayıcının yapıcısında `X? bagimlilik = null` + içeride `new X(...)` deseni **yasak.**
Eskide `SimulationCalculator` bunu yapıyordu ve aynı süreçte iki farklı itfa hesaplayıcısı
oluşabiliyordu. Parametre zorunlu olur, DI kabı zaten kaydediyor.

### M2 — İş sözlüğü hesaplayıcının içinde tanımlanmaz
`ObligationItem`, `MandatoryPaymentSummary` gibi ortak tipler kendi leaf dosyasında yaşar
(`Calculations/ObligationModels.cs`), hesaplayıcının dosyasında değil. Eskide bu, iki dosya
arasında karşılıklı bağımlılık üretiyordu.

### M3 — Tanrı cephe (god facade) yasak
Tek bir `MizanService` benzeri sınıfın arkasına her şeyi koymak yasaktır. ViewModel'ler
**dar portlara** bağlanır. Eskide 16 ViewModel'in 11'i yalnız `MizanService`'e bağlıydı;
sonuç: hiçbir ekran tek başına ele alınamıyordu.

> Ölçüt: bir sınıfın yapıcısında **5'ten fazla bağımlılık** varsa o sınıf iki iş yapıyordur.

### M4 — Okuma ve yazma ayrı portlardır
Adında "Query" geçen bir servis veri yazamaz. Eskide `FinancialPlanQueryService` 11
bağımlılıkla plan revizyonlarını **yazıyordu** ve 21 yerden çağrılıyordu.
Okuma `IPlanReader`, yazma `IPlanChangeRecorder`. Yazan taraf okuma portunu görmez.

### M5 — Tanrı arayüz yasak
40 metotlu `IMizanStore` gibi bir arayüz olmaz. Dar portlar (`ILoanRepository`,
`ICreditCardRepository` …) yazılır **ve gerçekten kullanılır.** Eskide dar portlar vardı
ama hiçbir servis onları enjekte etmiyordu; ayrıştırma dekoratiftı.

> Ölçüt: bir arayüzde **10'dan fazla metot** varsa bölünmelidir.

### M6 — `Models/` ve `Abstractions/`, `Services/`'e bakamaz
`using Mizan.Application.Services;` satırı bir model veya port dosyasında görünüyorsa,
o tip yanlış yerdedir. Tipi `Models/` altına taşı.

### M7 — Infrastructure iş kuralı hesaplamaz
Persistence katmanı ekstre kesim tarihi hesaplamaz, faiz işletmez, dönem sınırı bulmaz.
Veriyi alır, yazar; okur, verir.

### M8 — Statik yan erişim yasak
Enjekte edilmeyen bir sınıfın statik metodunu çağırmak (`PeriodProgressService.ResolveOpenPlan(...)`)
gizli bir bağımlılıktır. Ortak mantık bağımlılıksız bir yardımcıya çıkarılır ve iki taraf da onu kullanır.

## Kompozisyon kökü

Tüm DI kayıtları **yalnız** `src/Mizan.App/MauiProgram.cs` içindedir. Başka hiçbir yerde
`new` ile servis grafiği kurulmaz — testlerdeki `Fakes/` hariç.
