# Mizan — Mimari

Bu dosya mimariyi **anlatır**. Bağlayıcı kurallar `CLAUDE.md` ve `.claude/rules/` altındadır.
İkisi çelişirse kural kitabı geçerlidir ve bu dosya düzeltilir.

## Katmanlar

| Proje | Hedef | Sorumluluk |
|---|---|---|
| `Mizan.Domain` | `net8.0` | Saf finansal hesap. Sıfır paket, sıfır I/O, sıfır saat. |
| `Mizan.Application` | `net8.0` | Portlar (arayüzler) ve kullanım senaryoları. |
| `Mizan.Infrastructure` | `net8.0` | SQLite, dosya sistemi, telemetri adaptörleri. |
| `Mizan.Presentation` | `net8.0` | ViewModel'ler. **MAUI referansı yok.** |
| `Mizan.App` | `net8.0-android` | XAML, platform kodu, kompozisyon kökü. |

Bağımlılık oku yalnızca aşağı doğrudur. `Mizan.Infrastructure`'a yalnızca `Mizan.App`
referans verir, ve yalnız `MauiProgram.cs` içinde kullanır.

## Neden beş katman

Dört katmanla başlanıp `Mizan.Presentation` sonradan eklenmedi; **baştan var**, çünkü
birinci Mizan'ın en pahalı yapısal hatası ViewModel'lerin MAUI projesi içinde yaşaması ve
bu yüzden hiç test edilememesiydi. ViewModel'leri MAUI göremeyen bir kütüphaneye koymak,
bir belge kuralını bir derleyici kuralına çevirir.

## Test projeleri

Altı ayrı test projesi vardır ve her biri yalnızca denetlediği katmanı görür
(`.claude/rules/04-test.md`). Bir domain testinin bir servise uzanamaması,
disiplinle değil proje referanslarıyla sağlanır.

## Veri

Tek cihaz, çevrimdışı, SQLite. Bulut yok, hesap yok, senkronizasyon yok.
Çok kiracılık **profil başına bir veritabanı dosyası** ile sağlanır.
Yedek, cihaz depolamasının üstünde `Mizan` klasöründe durur (uygulama silinse de kalır).

Şema `PRAGMA user_version` ile sürümlenir ve `v1`'den başlar.

## Tavsiyeler  *(kural değil)*

Buradakiler henüz kural değil çünkü onları koruyan bir test yok. Testi yazılırsa
`.claude/rules/` altına terfi ederler.

- Para değerlerini tam sayı kuruş olarak saklamak `decimal`'e göre daha güvenli olabilir.
- Ekranlar arası paylaşılan çocuk ViewModel'ler yerine olay tabanlı bir yaklaşım
  denenebilir.
- Uzun süredir dokunulmamış bir hesaplayıcı için property-based test (FsCheck vb.) değerli olabilir.
