---
name: bug-fix
description: Kullanıcının gördüğü bir hata (yanlış davranış ya da yanlış görünüş) için düzeltme protokolü — 9 aşama; teşhis onayı (Kapı A), ekran şeması değişiyorsa blok şeması onayı (Kapı B), XAML'e dokunulduysa emülatörde görsel onay (Kapı C); önce kırmızı test, sonunda sürüm çıkar ve yayın bitene kadar izler.
argument-hint: "<ne gördün>"
disable-model-invocation: true
---

**Bildirim:** $ARGUMENTS

İyi bir bildirim dört parçadır: **ekran · tema · ne gördüm · ne bekliyordum.** Eksik parça
teşhisi gerçekten engelliyorsa **bir** soru sor. Ekran görüntüsü isteme; kullanıcı eklerse kullan.
Bildirim `docs/DURUM.md`'deki bir "Dikkat" maddesini gösteriyorsa (örneğin "0.1.1'deki (a)")
ayrıntıyı oradan oku.

Aşama 1 (teşhis) ile başla; ilk cümlede hatayı kullanıcının diliyle tekrar et.

# İş Akışı — Bir Hata Düzeltmesi

Kullanıcının gördüğü her hata bu protokolden geçer: yanlış **davranış** (eskiden `/duzeltme`
tür B) ve yanlış **görünüş** (eskiden tür G). Terim, kavramsal sapma ve doküman hataları
`/duzeltme`'de kalır; kullanıcıya görünmedikleri için sürüm çıkarmazlar. **Dokuz aşama vardır ve
hiçbiri atlanamaz.**

Tek ilke: **düzeltme, hatayı gösteren testten sonra gelir.** Kırmızıya düşmüş bir test,
hatanın anlaşıldığının tek kanıtıdır; kod izinden çıkarılmış bir teşhis, test kırmızı olana kadar
bir tahmindir.

Kapılar ve ekran sınıfları `/development` ile aynıdır: `.claude/skills/development/SKILL.md` →
"Ekran sınıfı". Kapı A'dan (ekran değişiminde B'den de) önce kod yazılmaz; Kapı C'den önce
commit ve sürüm yok. Son kapıdan sonra iş durmadan sürüme kadar gider.

---

## Aşama 1 — Teşhis  *(üretim kodu yazılmaz)*

- **Yolu bul.** Hangi ekran → hangi ViewModel → hangi servis ya da hesap. Kök nedeni
  **dosya:satır** düzeyinde göster.
- **Kanıt düzeyini ayır.** **Doğrulandı**: var olan bir testle, bir test koşusuyla ya da veriyle
  görüldü. **Çıkarım**: yalnız kod izinden. İkisini karıştırma; 0.1.1'in "Dikkat" maddesi (a)
  "kod izinden çıktı, çalıştırılarak doğrulanmadı" diye yazıldı, bu ayrım her teşhiste yazılır.
- **Ne zamandan beri.** Hangi commit ya da sürümle geldi (`git log`, `git blame`). Eski projede de
  var mı? Varsa söyle; eski projeye dokunma.
- **Kayıtlı veri bozuldu mu?** Kod düzelir ama telefonda yanlış yazılmış veri düzelmez. Bozulduysa
  veri onarımı (bir göç adımı) işin parçasıdır.
- **Aynı sınıf.** Aynı hata başka yerde de olabilir mi? Ara ve **say**, tahmin etme.
- **Görünüş hatasıysa hangi katman.** Yanlış katmanda düzeltmek en pahalı hatadır:

| Belirti | Katman | Ne yapılır |
|---|---|---|
| Token yanlış kullanılmış | XAML | Doğrudan düzelt; GK testinin onu neden yakalamadığını bul |
| Ekran kalabalık ama testler yeşil | Ekran kartı | Bütçe sayımı doğru, niteliği yanlış; kesme kararları yeniden açılır → "ekran değişimi" |
| Konsept başka şey gösteriyor | Karar | `docs/TASARIM-SAPMALARI.md`'ye `GS` kaydı |
| Aynı iş için iki ekranda iki farklı görünüm | Sistem | Yeni token, bileşen ya da grafik primitifi gerekir: bu bir sistem değişikliğidir ve bu protokolün kapsamı dışındadır. Kapı A'da söyle, `/development` öner |

> Bir sistem sorununu ekran sorunu sanıp tek ekranda düzeltmek, tasarım sistemini ekran ekran
> çatallar; eski projenin 14 farklı görsel dili böyle doğdu.

## Aşama 2 — Teşhis anlatımı  *(ONAY KAPISI A)*

Tek mesajda, bu sırayla:

1. **Ne oluyor.** Kullanıcının diliyle bir-iki cümle. ("Ayarlar'da dönem gününü değiştirince bir
   sonraki dönem kapanışı hata verip duruyor.")
2. **Neden oluyor.** Önce mekanizma günlük dille, sonra kök neden `dosya:satır`, sonra kanıt
   düzeyi: **Doğrulandı** ya da **Çıkarım**. Çıkarımsa çözümden önce söyle: Aşama 4'teki kırmızı
   test teşhisin sınavıdır.
3. **Etkisi.** Kim, hangi veriyle, ne sıklıkla karşılaşır; kayıtlı veri bozuldu mu; ne
   zamandan beri.
4. **Çözüm.** Ne değişecek, hangi dosyalar, yaklaşık satır; neden bu yol (düşünülüp elenen bir
   yol varsa tek satırla). Düzeltme bir `S`, `I` ya da `GS` kararını değiştiriyorsa açıkça söyle.
5. **Kırmızı test.** Adı ve kullanıcının diliyle neyi iddia ettiği. İç yapıyı değil davranışı
   iddia eder.
6. **Aynı sınıf.** Başka yerde de varsa bu düzeltmeye mi girsin, ayrı bir `/bug-fix` mi olsun:
   şık olarak sor, önerdiğini ilk sıraya koy.
7. **Ekran sınıfı** ve gerekçesi. Ekran değişimiyse değişen bölümün bütçesi, önce / sonra.
8. **Sürüm.** Bu iş patch sürüm çıkarır. **CHANGELOG maddesinin taslağı** (`### Düzeltildi`):
   kullanıcının gördüğü belirtiyle ve "artık …" diliyle, 0.1.1'in notları gibi.

Karar değiştiyse `S` / `GS` kaydı **şimdi** yazılır.

> **Kullanıcı onaylamadan Aşama 3'e geçilmez.** Soru sorarsa cevapla ve beklemeye devam et.

## Aşama 3 — Düzen Sözleşmesi  *(ONAY KAPISI B — yalnız "ekran değişimi")*

`/development` Aşama 3 ile birebir aynı.

## Aşama 4 — Kırmızı test

Hatayı **kullanıcının gördüğü hâliyle** tarif eden testi yaz, çalıştır, **kırmızı olduğunu
göster.**

**Test kırmızıya düşmüyorsa teşhis yanlıştır.** Düzeltmeye geçme: dur, ne öğrendiğini söyle ve
Aşama 1'e dön. Yeni teşhis, yeni bir Kapı A'dır.

Görünüş hatasında kırmızı test, hatayı yakalaması gereken GK testidir: yakalayamadıysa **testi
güçlendir** ve güçlenmiş testin bugünkü XAML'de kırmızıya düştüğünü göster. Sapmayı elle düzeltip
geçmek, aynı sapmanın bir sonraki ekranda yeniden doğmasına izin vermektir. Hiçbir testin
yakalayamayacağı bir görünüş hatası (açık temada kaybolan ince bir ayırıcı gibi) Kapı A'da böyle
söylenir; o durumda kanıt Kapı C'dir.

Veri onarımı gerekiyorsa onun testi de burada yazılır: bozuk veriyle kurulmuş veritabanı, göçten
sonra doğru.

## Aşama 5 — Düzelt

Kapı A'daki çözüm, en dar hâliyle. Kırmızı test yeşile döner.

Başka bir test değişmek zorunda kalırsa o test hatayı dondurmuş demektir: hangisi olduğunu ve
neden değiştiğini not et, Kayıt'ta yaz. Planın dışına çıkmak gerekiyorsa **dur ve sor.**

## Aşama 6 — XAML  *(yalnız "görünüş" ve "ekran değişimi")*

`/tasarim-adimi` Aşama 8'in kuralları aynen.

## Aşama 7 — Kalkan  *(XAML'e dokunulduysa ONAY KAPISI C)*

`/development` Aşama 7 ile birebir aynı: iki komut, 0 hata, 0 uyarı, tümü yeşil; XAML'e
dokunulduysa `./scripts/emulatorde-ac.ps1` ve görsel kontrol listesi. Listenin "Bak" kısmının ilk
maddesi bildirilen hatanın kendisidir.

> **ONAY KAPISI C:** Kullanıcı "tamam" demeden Aşama 8'e geçilmez.

## Aşama 8 — Kayıt

Sırayla:

1. `CHANGELOG.md` — Kapı A'da onaylanan madde `## [Yayınlanmamış]` altına, `### Düzeltildi`
   başlığıyla.
2. `docs/INVARYANTLAR.md` — her düzeltilen hata bir invariant bırakır: kural ve **koruyan testin
   tam adı**. Kodu tablodaki son koddan devam ettir; `DURUM.md`'de anılıp tabloda olmayan kodları
   atla.
3. `docs/DURUM.md` — en üste `### Hata düzeltme — <kısa ad>; kararlar <kodlar>` ve 3–6 satır:
   belirti, kök neden, düzeltme, koruyan test, aynı sınıf için ne yapıldı, hatayı dondurmuş bir
   test değiştiyse hangisi. Hata `DURUM.md`'deki bir "Dikkat" maddesinden geldiyse o maddenin
   yanına "düzeltildi → <başlık>" yaz. "Nerede kalındı" tablosunu güncelle.
4. Karar değiştiyse `docs/SAPMALAR.md`, `docs/TASARIM-SAPMALARI.md`, `docs/EKRAN-KARTLARI.md`
   son hâlleriyle uyuşuyor mu bak.
5. Tamamlanmış bir taşıma adımının iddiası bozulduysa `docs/TASIMA-PLANI.md`'de o satıra not düş.
6. **Tek commit.** `fix(<kapsam>): …`, İngilizce özet, Türkçe gövde, son satır
   `Hata: <kısa ad>`.

```
fix(application): settlement no longer fails after period day change

Ayarlar'da dönem günü değişince sonraki kapanış InvalidOperationException
ile düşüyordu: FinancialSnapshotService.Build yeni çapayı eski PeriodEnd'e
uyguluyordu. ... (düzeltme, koruyan test, I kodu).

Hata: dönem günü değişince kapanış
```

## Aşama 9 — Sürüm

`.claude/skills/surum/SKILL.md`'yi oku ve uygula. Kapısı yoktur; onay Kapı A'da sürüm notuyla
birlikte alındı. Protokol, yayın koşusu bitip Release'in imzalı APK ile çıktığı doğrulanınca
biter.

---

## Büyüklük

Bir çalıştırma **tek kök neden** düzeltir ve ~300 satır üretim kodunu aşmaz. Teşhis iki kök neden
çıkardıysa ya da düzeltme sınırı aşıyorsa Kapı A'da bölünme öner; her kök neden kendi
`/bug-fix`'idir.

## Birden fazla hata birden

Kullanıcı birkaç hata birden bildirdiyse (`DURUM.md`'deki (a)–(d) gibi) Kapı A'dan önce sıra
öner: veriyi bozan → uygulamayı durduran → yanlış gösteren → görünüş. Her hata Aşama 1–8'i kendi
başına geçer ve kendi commit'ini atar. Kullanıcı isterse Aşama 9 her birinin sonunda değil
**yalnız sonuncusunda** bir kez koşar ve hepsi tek sürümde çıkar; bunu Kapı A'da sor.

## İş dışında fark edilenler

Teşhis sırasında bu hatanın parçası olmayan bir şey görürsen düzeltme: not al ve Aşama 9'dan
sonra kullanıcıya söyle. Başka bir hataysa ayrı `/bug-fix`, yeni bir istekse `/development`,
terim, kavram ya da doküman hatasıysa `/duzeltme`.
