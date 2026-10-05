# Değişiklik Günlüğü

Planör'ün her sürümünün kullanıcıya dönük notu burada durur. GitHub Release sayfasındaki metin,
etiketin sürümüne ait bölümden **olduğu gibi** okunur (`scripts/release-notes.ps1`, `S82`); bu yüzden
sürümü artıran değişiklik notunu da aynı PR'da getirir.

Biçim: `## [X.Y.Z] - YYYY-AA-GG` başlığı ve altında en az bir `- ` maddesi. Alt başlıklar
(`### Eklendi`, `### Değişti`, `### Düzeltildi`) serbesttir. `## [Yayınlanmamış]` bölümü bir sürüm
değildir ve yayınlanamaz.

## [Yayınlanmamış]

### Değişti

- Ana sayfadaki sütun grafiği kaldırıldı; yerine kartlarının ve KMH faizinin bu dönem için planlanan ve şu anki tutarları yan yana geldi. Planı aşan tutar kırmızı görünür.
- Ana sayfanın yaşam gideri sayfası planlanan ve harcanan tutarı da gösteriyor.
- "Bakiye gir" sayfasındaki önizleme grafiği kaldırıldı; önizleme dönem sonunu ve plana göre farkı rakamla söylüyor.

## [0.1.1] - 2026-10-05

### Düzeltildi

- Ödeme hatırlatıcısı, vadesi içinde bulunulan dönemin bittiği güne (sonraki dönemin ilk günü) denk gelen ödemeleri (kredi, kart, taksit) ve planlı büyük harcamaları artık atlamıyor.
- Dönemin ilk gününe vadeli bir ödemeye "Ertele" denince takip hatırlatması artık kuruluyor ve ödeme ertelenmiş görünüyor.
- Eski uygulamadan alınan verilerde, ödendi işaretlenmiş bir taksit yeniden hatırlatılmıyor; eski uygulamanın kapanmış dönemlerine ait ertelemeler de "ertelendi" kartı olarak geri gelmiyor.

## [0.1.0] - 2026-10-05

### Eklendi

- Dönem bazlı nakit akışı planı: gelir, kart, kredi ve ödeme kayıtlarından 12 dönemlik gidişat.
- Ana sayfa, finansal yapı, simülatör, geçmiş ve ayarlar ekranları; dönem kapanışı özeti.
- Tamamen çevrimdışı çalışma: ağ izni yok, veri yalnız cihazdaki SQLite veritabanında durur.
- Ayarlardan yerel yedekleme.
