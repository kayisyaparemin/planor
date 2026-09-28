# Mizan — Kural Kitabı

Kuralların **tek kaynağı** bu dosya ve `.claude/rules/` altıdır. Başka hiçbir dosyada kural
tekrarlanmaz. Eski projede aynı altı kural sekiz dosyaya dağılmıştı ve birbiriyle çelişiyordu; bu
repoda bir kuralı değiştirmek istiyorsan değiştireceğin tek bir yer var.

## Bu proje nedir

Mizan, Türkiye'deki bir kullanıcının dönem bazlı nakit akışını planlayan, **çevrimdışı çalışan**
kişisel finans uygulamasıdır. .NET 8 / MAUI (yalnız Android), SQLite, tek cihaz, bulut yok.

Kod İngilizce; yorum, doküman ve kullanıcıya görünen her metin **Türkçe**.

## Bu repo nasıl doğdu

Bu, ikinci deneme. Birinci proje çalışıyordu ama **sahibi kendi kodunu okuyamıyordu.** Her parça
buraya tek tek, anlaşılarak ve testiyle birlikte taşınıyor. Bu yüzden:

- **Hız burada bir erdem değil.** Bir adımda bir şey taşınır, en fazla ~300 satır.
- **Anlatılmamış kod yazılmaz.** Taşıma protokolünün 2. aşaması (anlatım) atlanamaz.

## Kullanıcıyla çalışma biçimi

- Dil Türkçe. Kullanıcı deneyimli bir .NET backend geliştiricisi, MAUI tarafında yeni.
- "Bu ne?" diye sorulduğunda **kod yazma, anlat.** Açıklamayı gerçek dosya yollarıyla bağla.
- Ürün kararlarını 2–4 seçenekli soru olarak sor; önerdiğini ilk sıraya koy ve "(Önerilen)" yaz.
- Değişikliği ancak kullanıcı istediğinde yap.
- Doküman otorite değil ipucudur. Kod ile doküman çelişirse dört yollu triyaj yap
  (kod hatası / doküman eski / kural değişti / karar eksik) ve farkı kullanıcıya söyle.

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
| **K9** | `docs/SOZLUK.md`'deki yasaklı terimler `src/` ve `tests/` altında geçemez | `ArchitectureTests.YasakliTerimler_KaynaktaGecemez` — ⚠️ **test henüz yok** (`docs/TASIMA-PLANI.md` → F1) |

K1–K9'u bozan bir şey yazmak zorunda kaldıysan, **kodu değil kuralı tartışmaya aç.** Sessizce
istisna yapma; kullanıcıya "şu kural şu sebeple engel oluyor, ne yapalım?" diye sor.

## On iki görsel kural

Ürünün görünen adı **Planör**; kod adı `Mizan` kalır (`docs/TASARIM-SAPMALARI.md` § GS6).

Görsel katmanın kuralları `.claude/rules/06-tasarim.md`'de **GK1–GK12** olarak durur ve aynı
ilkeye tabidir: her birinin onu bozan XAML'i kırmızıya düşüren bir testi var. Tanımların tek
kaynağı `docs/TASARIM-SISTEMI.md`'dir ve o dosya **testler tarafından okunur** —
`docs/SOZLUK.md`'deki yasaklı terim tablosunun okunduğu gibi.

## Modüller

`.claude/rules/` altındaki dosyalar ilgili kaynak dosyayı okuduğunda kendiliğinden yüklenir.
Yeni bir dosya yazmadan önce aynı klasördeki komşusunu oku.

| Nerede | Ne zaman |
|---|---|
| `.claude/rules/01-mimari.md` | Yeni bir tip, servis veya arayüz eklerken |
| `.claude/rules/02-okunabilirlik.md` | Her kod yazışında — en sık ihlal edilen dosya budur |
| `.claude/rules/03-mvvm.md` | ViewModel, sayfa, XAML veya AutomationId'e dokunurken |
| `.claude/rules/04-test.md` | Test yazarken (yani her zaman) |
| `.claude/rules/05-para-ve-tarih.md` | Para, tarih, dönem veya veritabanı şeması işinde |
| `.claude/rules/06-tasarim.md` | XAML, renk, punto, ikon, grafik veya ekran yerleşimine dokunurken |
| `/tasima-adimi` | Eski projeden parça taşırken — 8 aşama, atlanamaz |
| `/tasarim-adimi` | Bir **ekran** adımında (Faz V) — 10 aşama, üç onay kapısı |
| `/duzeltme` | Adım **dışında** bir hata yakalandığında — terim, sapma, doküman, bug, görsel |
| `mimari-bekci` ajanı | İnceleme yaparken |
| `docs/TASIMA-PLANI.md`, `docs/DURUM.md` | Nereye kadar geldik? |
| `docs/SOZLUK.md` | Bir terim ne demek? |

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

## Komutlar

- **Bir komut = bir binary + argümanları.** Çok adımlı iş komut değil dosya olur: `scripts/`
  altına yaz, tek satırla çağır.
- İzin listesi `.claude/settings.json`'dadır. `git checkout`, `git restore` ve `git stash`
  bilerek listede yok: aynı çalışma ağacında birden fazla oturum çalışabiliyor, başkasının işini
  geri alma.

## Bitirme ölçütü

Bir görev, şunların hepsi sağlanmadan "tamamlandı" sayılamaz:

```
dotnet build Mizan.sln    → 0 hata, 0 uyarı
dotnet test  Mizan.sln    → tamamı yeşil
```

ve `docs/TASIMA-PLANI.md` ile `docs/DURUM.md` güncellenmiş olmalı.

## Yerel geçersiz kılmalar

`CLAUDE.local.md` varsa kendiliğinden yüklenir; git'e girmez, kullanıcının o anki kişisel
yönergelerini taşır (örn. "şu testi şimdilik atla"). Buradaki dokuz kuralı geçersiz kılamaz.
