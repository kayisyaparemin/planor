# 05 — Para, Tarih ve Şema

Bu üçü bu uygulamanın konusudur; buradaki her kural bir kuruş sapmasını ya da bir günlük
kaymayı engellemek için vardır.

## Para

- Tip **her zaman `decimal`**. `double` / `float` para için yasak.
- Yuvarlama **her zaman** `Math.Round(deger, 2, MidpointRounding.AwayFromZero)`.
- Yuvarlama **en sonda** yapılır; ara hesaplar tam hassasiyette taşınır.
- Taksit bölüştürmede kuruş artığı **yalnız son taksite** eklenir; her taksite dağıtılmaz.
- Para korunumu: bir dönemin kapanış bakiyesi, sonraki dönemin açılış bakiyesine
  **kuruşu kuruşuna** eşittir. Sapma (drift) bir hatadır, tolerans değildir.
- Hesaplanmış bir değer veritabanına **yazılmaz.** Eskide `credit_cards.CurrentTotalDebt`
  türetilmiş bir değerdi ama saklanıyordu; bayatladı.

## Tarih ve dönem

- Tip **`DateOnly`** (saat bileşeni olan bir şey değilse). Zaman damgası `DateTimeOffset`.
- **Dönem yarı açıktır: `[başlangıç, sonraki başlangıç)`.** Bitiş günü döneme dahil değildir.
  Bir aralık testi yazarken her iki ucu da ayrıca doğrula.
- `DateTime.Now` hiçbir yerde yok; her zaman `IClock`.
- Veritabanında tarih `yyyy-MM-dd` metni olarak durur (sıralanabilir ve kültürden bağımsız).
- Geçmiş yeniden hesaplanmaz. Dondurulmuş bir dönem planı sonradan değişmez.
- Etkin tarihli (effective-dated) kararlar — maaş, ödeme düzeni, kart ödeme tercihi —
  **değişmezdir ve üzerine yazılmaz**; yeni bir kayıt eklenir, eskisi tarihçede kalır.

## Veritabanı şeması

Yeni repo **temiz `v1`** ile başlar. Eski projenin migration'ları taşınmaz.

- **Şema sürümü `PRAGMA user_version`'da tutulur**, kullanıcı verisinin içinde değil.
  Eskide `settings.SchemaVersion` tek satırlık bir tabloda duruyordu ve her ayar kaydında
  yeniden yazılıyordu; yedek parmak izi bu yüzden içerik tabanlı olmak zorunda kalmıştı.
- **Kolon adı alanı yanıltmaz.** Eskide 16 kolon yalan söylüyordu — `loans.StartDate` aslında
  `NextPaymentDate` tutuyordu, `card_installments.DueDate` aslında `PostingDate` idi.
  `[Column("...")]` takma adı **yasak**; C# özellik adı ile SQL kolon adı birebir aynıdır.
- **Ölü kolon yazılmaz.** Kullanılmayan bir alan silinir, `null` yazılarak yaşatılmaz.
- **Her parent/child bağı gerçek bir foreign key'dir.** Eskide tek bir gerçek kısıt vardı;
  referans bütünlüğü elle sürdürülüyordu ve `DeleteLoanAsync` gibi metotlara gömülüydü.
- **"En fazla bir tane" kuralı UNIQUE indeksle söylenir**, yorumla değil.
- **Yaz-sil-yeniden-yaz deseni yasak.** Bir kartı güncellerken ekstrelerini silip yeniden
  eklemek, tarihçeyi geri dönülmez biçimde yok eder. Etkin tarihli tablolar append-only'dir.
- **Zaman damgası ayrıştırması sessizce yutmaz.** Bozuk veri `MinValue`'ya düşmez, hata verir.
- Açılışta `CREATE TABLE IF NOT EXISTS` turu atılmaz; kurulum `user_version` ile kapılıdır.

## Kültür

Tek kültür: `tr-TR`. Ama **yalnızca sunum kenarında** — `IValueConverter` içinde.
Domain, Application, Infrastructure ve Presentation katmanlarında kültür geçmez;
oralarda para `decimal`, tarih `DateOnly`'dir.
