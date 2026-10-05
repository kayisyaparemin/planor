# Değişiklik Günlüğü

Planör'ün her sürümünün kullanıcıya dönük notu burada durur. GitHub Release sayfasındaki metin,
etiketin sürümüne ait bölümden **olduğu gibi** okunur (`scripts/release-notes.ps1`, `S82`); bu yüzden
sürümü artıran değişiklik notunu da aynı PR'da getirir.

Biçim: `## [X.Y.Z] - YYYY-AA-GG` başlığı ve altında en az bir `- ` maddesi. Alt başlıklar
(`### Eklendi`, `### Değişti`, `### Düzeltildi`) serbesttir. `## [Yayınlanmamış]` bölümü bir sürüm
değildir ve yayınlanamaz.

## [Yayınlanmamış]

### Düzeltildi

- Kurulum sihirbazı ayın çapa gününden sonraki bir tarihte tamamlandığında ilk dönemin bir sonraki aydan başlatılması ve cari döneme bakiye girilememesi düzeltildi; ilk dönem artık kurulum gününün içinde bulunduğu takvim döneminden başlar.
- 12 Dönem ve Simülatör'ün dönem sonları ızgarasında altı haneli tutarlar dar ekranda alt satıra kırılıyor, ₺ simgesi tek başına aşağı düşüyordu; tutar artık karoda tek satırda kalır ve ₺ rakamdan ayrılmaz.

## [0.3.0] - 2026-10-05

### Eklendi

- Mimari kural K9: Sözlükteki yasaklı terimlerin kaynak ve test kodlarına sızmasını engelleyen otomatik mimari test kalkanı (`YasakliTerimler_KaynaktaGecemez`).

## [0.2.2] - 2026-10-05

### Düzeltildi

- Kredi kartına dönem içinde harcama yapıldığında, bakiye rotasının katedilen kısmındaki ara günler artık dondurulmuş plan tutarıyla değil kartın güncel ödeme tutarıyla hesaplanıyor; kartın güncel borç farkı açıklanamayan yaşam harcaması gibi günlere dağıtılmıyor.

## [0.2.1] - 2026-10-05

### Düzeltildi

- Dönem kapanışında kredi kartının fiilî ödemesi artık dondurulmuş plan tahminiyle değil, dönem içi harcamaları içeren güncel ekstre borcuyla kapatılıyor. Böylece karta haksız devreden bakiye ve gecikme faizi binmiyor; önerilen kapanış bakiyesi ve ödeme sayaçları da doğru yansıtılıyor.

### Değişti

- Ana sayfadaki sütun grafiği kaldırıldı; yerine kartlarının ve KMH faizinin bu dönem için planlanan ve şu anki tutarları yan yana geldi. Planı aşan tutar kırmızı görünür.
- Ana sayfanın yaşam gideri sayfası planlanan ve harcanan tutarı da gösteriyor.
- "Bakiye gir" sayfasındaki önizleme grafiği kaldırıldı; önizleme dönem sonunu ve plana göre farkı rakamla söylüyor.

### Düzeltildi

- Kredi kartı ödeme hatırlatıcısı artık dönem başında planlanan tutarı değil, dönem içinde girilen harcamalarla güncellenen ödemeyi gösteriyor; ana sayfadaki "Şu an" tutarıyla aynı.
- Ana sayfadaki "Kalan ödemeler" listesi, vadesi henüz gelmemiş kredi kartı ödemesini artık dönem başında planlanan tutarla değil, dönem içinde girilen harcamalarla güncellenen tutarla gösteriyor; listenin toplamı da buna göre hesaplanıyor.
- Ana sayfadaki dönem sonu tahmini, kredi kartının son ödeme günü gelince ya da ödemesine "Ödedim" denince artık iyimserleşmiyor: kart ödemesi dönem başında planlanan tutarla değil, dönem içinde girilen harcamalarla güncellenen tutarla düşülüyor. Bakiye ödemeden sonra girildiyse harcanan yaşam gideri de artık kartın bu farkını içermiyor.
- Ödeme günü ana sayfadaki hatırlatıcı kartı artık öğlen kaybolmuyor; vadesi bugün olan ödeme, "Ödedim" ya da "Ertele" denene kadar gün boyu kartta kalıyor.
- Hatırlatıcı kartı artık telefonun saatini kullanıyor: gece yarısından sonra bir önceki günün ödemesini göstermiyor, akşam "Ertele" denen ödeme gece değil ertesi sabah geri geliyor, "Ödedim" cevabının saati de doğru kaydediliyor.

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
