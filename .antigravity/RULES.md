# Mizan — Kural Kitabı

Kuralların **tek kaynağı** burasıdır. Başka hiçbir dosyada kural tekrarlanmaz.
Eski projede aynı altı kural sekiz dosyaya dağılmıştı ve birbiriyle çelişiyordu; bu repoda
bir kuralı değiştirmek istiyorsan değiştireceğin tek bir yer var.

## Temel ilke

> **Bir kural, ancak onu bozan kodu kırmızıya düşüren bir test varsa bu kitaba girer.**

Testi olmayan iyi fikirler `docs/MIMARI.md` → "Tavsiyeler" bölümünde durur. Oradan buraya
terfi etmenin tek yolu, onu koruyan testi yazmaktır. Bu kural bu kitabın kendisi için de geçerlidir.

## Dokuz pazarlıksız kural

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
| **K9** | `docs/SOZLUK.md`'deki yasaklı terimler `src/` ve `tests/` altında geçemez | `ArchitectureTests.YasakliTerimler_KaynaktaGecemez` |

K1–K9'u bozan bir şey yazmak zorunda kaldıysan, **kodu değil kuralı tartışmaya aç.** Sessizce
istisna yapma; kullanıcıya "şu kural şu sebeple engel oluyor, ne yapalım?" diye sor.

## Modüller

| Dosya | Ne zaman oku |
|---|---|
| `rules/01-mimari.md` | Yeni bir tip, servis veya arayüz eklerken |
| `rules/02-okunabilirlik.md` | Her kod yazışında — en sık ihlal edilen dosya budur |
| `rules/03-mvvm.md` | ViewModel, sayfa, XAML veya AutomationId'e dokunurken |
| `rules/04-test.md` | Test yazarken (yani her zaman) |
| `rules/05-para-ve-tarih.md` | Para, tarih, dönem veya veritabanı şeması işinde |
| `rules/06-tasarim.md` | XAML, renk, punto, ikon, grafik veya ekran yerleşimine dokunurken |
| `workflows/tasima-adimi.md` | Eski projeden parça taşırken — 8 aşama, atlanamaz |
| `workflows/tasarim-adimi.md` | Bir **ekran** adımında (Faz V) — 10 aşama, iki onay kapısı |
| `workflows/duzeltme.md` | Adım **dışında** bir hata yakalandığında — terim, sapma, doküman, bug, görsel |
| `agents/mimari-bekci.md` | İnceleme yaparken kullanılacak alt-ajan tarifi |

## On bir görsel kural

Ürünün görünen adı **Planör**; kod adı `Mizan` kalır (`docs/TASARIM-SAPMALARI.md` § GS6).

Görsel katmanın kuralları `rules/06-tasarim.md`'de **GK1–GK11** olarak durur ve aynı ilkeye
tabidir: her birinin onu bozan XAML'i kırmızıya düşüren bir testi var.

| # | Kural |
|---|---|
| **GK1** | XAML ham renk içermez; token adı rolü söyler, boyayı söylemez |
| **GK2** | `FontSize` yalnız yedi kademeli tip skalasından |
| **GK3** | `Margin`, `Padding`, `Spacing`, `CornerRadius` token'dan |
| **GK4** | Görsel bütçe: sayfa başına ≤ 1 hero, ≤ 4 kart, ≤ 1 grafik, ≤ 28 `<Label>` |
| **GK5** | Cümle bütçesi: sayfa başına ≤ 3 açıklama cümlesi, her biri ≤ 90 karakter |
| **GK6** | İkon tek kaynaktan (`Icons.cs`); ham glif yok |
| **GK7** | Grafik yalnız dört primitiften biri; verisi ham seri olarak gelir |
| **GK8** | İki tema, tek mekanizma: renk yalnız `DynamicResource`, `AppThemeBinding` yasak |
| **GK9** | Her sayfanın `docs/EKRAN-KARTLARI.md`'de bir kartı vardır |
| **GK10** | `docs/TASARIM-SISTEMI.md`'deki kontrast çiftleri iki temada da eşiği geçer |
| **GK11** | Görünen ad `Planör`; eski ad görünen metinde yok; platform renkleri token'la aynı |

Tanımların tek kaynağı `docs/TASARIM-SISTEMI.md`'dir ve o dosya **testler tarafından
okunur** — `docs/SOZLUK.md`'deki yasaklı terim tablosunun okunduğu gibi.

## Konsept otorite değildir

Ekran adımlarının varsayılanı "konsepti sadakatle uygula"dır, ama konsept **doğru olduğu
için** değil, aksi kanıtlanana kadar öyle varsayıldığı için uygulanır. Konseptin veremediği
ya da vermemesi gereken şeyler `docs/TASARIM-SAPMALARI.md`'de `GS` kaydı olarak durur ve
Aşama 4 (Görsel Bütçe) her ekranda onu okur.

Çatışma hâlinde **GK4 konseptten üstündür**: bütçe ölçülmüş bir problemden geliyor, konsept
panelleri ise tek bir bakış için üretildi.

## Eski kod otorite değildir

Taşıma protokolünün varsayılanı "eskiyi sadakatle taşı"dır, ama eski kod **doğru olduğu için**
değil, aksi kanıtlanana kadar öyle varsayıldığı için taşınır. Yanlış modellenmiş yerler
`docs/SAPMALAR.md`'de kayıtlıdır ve Aşama 3 (Sapma Kararı) her adımda onu okur.

Eski koddaki bir isim `docs/SOZLUK.md`'deki yasaklı terimler tablosundaysa, o isim taşınmaz —
K9 zaten derlemeyi kırar.

## Yerel geçersiz kılmalar

`.antigravity/RULES.local.md` varsa oku; git'e girmez, kullanıcının o anki kişisel
yönergelerini taşır (örn. "şu testi şimdilik atla"). Buradaki sekiz kuralı geçersiz kılamaz.
