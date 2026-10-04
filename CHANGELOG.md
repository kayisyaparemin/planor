# Değişiklik Günlüğü

Planör'ün her sürümünün kullanıcıya dönük notu burada durur. GitHub Release sayfasındaki metin,
etiketin sürümüne ait bölümden **olduğu gibi** okunur (`scripts/release-notes.ps1`, `S82`); bu yüzden
sürümü artıran değişiklik notunu da aynı PR'da getirir.

Biçim: `## [X.Y.Z] - YYYY-AA-GG` başlığı ve altında en az bir `- ` maddesi. Alt başlıklar
(`### Eklendi`, `### Değişti`, `### Düzeltildi`) serbesttir. `## [Yayınlanmamış]` bölümü bir sürüm
değildir ve yayınlanamaz.

## [Yayınlanmamış]

## [0.1.0] - 2026-10-05

### Eklendi

- Dönem bazlı nakit akışı planı: gelir, kart, kredi ve ödeme kayıtlarından 12 dönemlik gidişat.
- Ana sayfa, finansal yapı, simülatör, geçmiş ve ayarlar ekranları; dönem kapanışı özeti.
- Tamamen çevrimdışı çalışma: ağ izni yok, veri yalnız cihazdaki SQLite veritabanında durur.
- Ayarlardan yerel yedekleme.
