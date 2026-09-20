# Mizan — Kural Kitabı

Kuralların **tek kaynağı** burasıdır. Başka hiçbir dosyada kural tekrarlanmaz.
Eski projede aynı altı kural sekiz dosyaya dağılmıştı ve birbiriyle çelişiyordu; bu repoda
bir kuralı değiştirmek istiyorsan değiştireceğin tek bir yer var.

## Temel ilke

> **Bir kural, ancak onu bozan kodu kırmızıya düşüren bir test varsa bu kitaba girer.**

Testi olmayan iyi fikirler `docs/MIMARI.md` → "Tavsiyeler" bölümünde durur. Oradan buraya
terfi etmenin tek yolu, onu koruyan testi yazmaktır. Bu kural bu kitabın kendisi için de geçerlidir.

## Sekiz pazarlıksız kural

| # | Kural | Nasıl zorlanıyor |
|---|---|---|
| **K1** | `Mizan.Domain` sıfır NuGet paketi kullanır, sıfır I/O yapar, `DateTime.Now` çağırmaz | `ArchitectureTests.Domain_HicbirPakete_BagliOlamaz` |
| **K2** | `Mizan.Presentation` MAUI'ye referans veremez | Derleyici — proje referansı yok |
| **K3** | Bir dosyada bir public tip; dosya ≤ 200, metot ≤ 40 satır | `ArchitectureTests.Dosyalar_SinirlariAsamaz` |
| **K4** | `partial` yalnız source generator için; dosya bölmek için asla | `ArchitectureTests.Partial_YalnizGeneratorIcin` |
| **K5** | `async void` yok (istisna: XAML event handler, gövdesi tamamen try/catch) | `ArchitectureTests.AsyncVoid_Yasak` |
| **K6** | `IServiceProvider` enjekte edilmez (Service Locator yasak) | `ArchitectureTests.ServiceLocator_Yasak` |
| **K7** | Kaynak dosya metni okuyan test yalnız `Mizan.Architecture.Tests` içinde olabilir | `ArchitectureTests.KaynakTarayanTest_YalnizBuradaOlabilir` |
| **K8** | Her public tipte Türkçe `<summary>`: *ne yaptığı değil, neden var olduğu* | Derleyici — CS1591 = hata |

K1–K8'i bozan bir şey yazmak zorunda kaldıysan, **kodu değil kuralı tartışmaya aç.** Sessizce
istisna yapma; kullanıcıya "şu kural şu sebeple engel oluyor, ne yapalım?" diye sor.

## Modüller

| Dosya | Ne zaman oku |
|---|---|
| `rules/01-mimari.md` | Yeni bir tip, servis veya arayüz eklerken |
| `rules/02-okunabilirlik.md` | Her kod yazışında — en sık ihlal edilen dosya budur |
| `rules/03-mvvm.md` | ViewModel, sayfa, XAML veya AutomationId'e dokunurken |
| `rules/04-test.md` | Test yazarken (yani her zaman) |
| `rules/05-para-ve-tarih.md` | Para, tarih, dönem veya veritabanı şeması işinde |
| `workflows/tasima-adimi.md` | Eski projeden parça taşırken — 7 aşama, atlanamaz |
| `agents/mimari-bekci.md` | İnceleme yaparken kullanılacak alt-ajan tarifi |

## Yerel geçersiz kılmalar

`.antigravity/RULES.local.md` varsa oku; git'e girmez, kullanıcının o anki kişisel
yönergelerini taşır (örn. "şu testi şimdilik atla"). Buradaki sekiz kuralı geçersiz kılamaz.
