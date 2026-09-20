# Mizan — Ajan Giriş Kapısı

Bu dosya kısadır ve kısa kalacaktır. Kuralların tamamı `.antigravity/` altındadır.

## Her oturumun başında oku

| Ne zaman | Oku |
|---|---|
| **Her zaman** | `.antigravity/RULES.md` — 8 pazarlıksız kural |
| Eski projeden parça taşıyorken | `.antigravity/workflows/tasima-adimi.md` — 7 aşamalı protokol |
| Kod yazmadan önce | İlgili katmanın kuralı: `.antigravity/rules/01-mimari.md` |
| Nereye kadar geldik? | `docs/TASIMA-PLANI.md` ve `docs/DURUM.md` |
| Bir terim ne demek? | `docs/SOZLUK.md` |

## Bu proje nedir

Mizan, Türkiye'deki bir kullanıcının maaş dönemi bazlı nakit akışını planlayan, **çevrimdışı çalışan**
kişisel finans uygulamasıdır. .NET 8 / MAUI (yalnız Android), SQLite, tek cihaz, bulut yok.

Kod İngilizce; yorum, doküman ve kullanıcıya görünen her metin **Türkçe**.

## Bu repo nasıl doğdu

Bu, ikinci deneme. Birinci proje çalışıyordu ama **sahibi kendi kodunu okuyamıyordu.** Her parça
buraya tek tek, anlaşılarak ve testiyle birlikte taşınıyor. Bu yüzden:

- **Hız burada bir erdem değil.** Bir adımda bir şey taşınır, en fazla ~300 satır.
- **Anlatılmamış kod yazılmaz.** Taşıma protokolünün 2. aşaması (anlatım) atlanamaz.
- **Kural ancak testi varsa kuraldır.** Bir kuralı savunan test yoksa o kural
  `docs/MIMARI.md`'de "tavsiye" bölümünde durur, `.antigravity/rules/` altında değil.

## Kullanıcıyla çalışma biçimi

- Dil Türkçe. Kullanıcı deneyimli bir .NET backend geliştiricisi, MAUI tarafında yeni.
- "Bu ne?" diye sorulduğunda **kod yazma, anlat.** Açıklamayı gerçek dosya yollarıyla bağla.
- Ürün kararlarını 2–4 seçenekli soru olarak sor; önerdiğini ilk sıraya koy ve "(Önerilen)" yaz.
- Değişikliği ancak kullanıcı istediğinde yap.
- Doküman otorite değil ipucudur. Kod ile doküman çelişirse dört yollu triyaj yap
  (kod hatası / doküman eski / kural değişti / karar eksik) ve farkı kullanıcıya söyle.

## Bitirme ölçütü

Bir görev, şunların hepsi sağlanmadan "tamamlandı" sayılamaz:

```
dotnet build Mizan.sln    → 0 hata, 0 uyarı
dotnet test  Mizan.sln    → tamamı yeşil
```

ve `docs/TASIMA-PLANI.md` ile `docs/DURUM.md` güncellenmiş olmalı.
