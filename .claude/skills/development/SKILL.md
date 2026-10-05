---
name: development
description: Yeni özellik ya da davranış değişikliği protokolü — 9 aşama; analiz onayı (Kapı A), ekran şeması değişiyorsa blok şeması onayı (Kapı B), XAML'e dokunulduysa emülatörde görsel onay (Kapı C); sonunda sürüm çıkar ve yayın bitene kadar izler.
argument-hint: "<ne istiyorsun>"
disable-model-invocation: true
---

**İstek:** $ARGUMENTS

İstek boşsa ya da ne istendiği tek cümleyle söylenemiyorsa dur ve **bir** soru sor. İstek
`docs/TASIMA-PLANI.md`'de açık bir kutunun koduysa (`T7` gibi) tarifini oradan al.

Aşama 1 (keşif) ile başla; ilk cümlede isteği kendi cümlenle, kullanıcının diliyle söyle.

# İş Akışı — Bir Geliştirme

Göç bitti; Planör artık eski projeden taşınarak değil, yeni istekle büyüyor. Bu protokol yeni bir
özelliği ya da var olan bir davranışın değişmesini yürütür. **Dokuz aşama vardır ve hiçbiri
atlanamaz.** Ekrana dokunmayan işte Aşama 3 ve 6 "yok" diye geçilir; bu atlamak sayılmaz.

Taşıma protokollerinin ilkesi aynen geçerli: kod **sahibi tarafından anlaşılmış olarak** girer.
Fark, kaynağın eski kod değil kullanıcının isteği olması ve işin telefona giden bir sürümle
bitmesi. Aşamaların çoğu `/tasima-adimi` ve `/tasarim-adimi`'dekiyle aynı mantıktadır ve oraya
gönderir; burada tekrarlanmaz.

| Kapı | Ne zaman | Onaysız ne yapılmaz |
|---|---|---|
| **A** — Analiz (Aşama 2) | Her zaman | Tek satır kod |
| **B** — Düzen sözleşmesi (Aşama 3) | Sınıf "ekran değişimi" ise | Tek satır kod |
| **C** — Görsel onay (Aşama 7) | XAML'e dokunulduysa | Commit ve sürüm |

Son kapıdan sonra iş **durmadan** sürüme kadar gider; yayın koşusu bitip doğrulanınca protokol
biter.

## Ekran sınıfı

Kapı A'da her iş üç sınıftan birine konur; kapıları bu belirler. `/bug-fix` de bu tabloyu kullanır.

| Sınıf | Ne demek | Kapılar |
|---|---|---|
| **Ekrana dokunmuyor** | XAML değişmez. Var olan ekran yeni veriyi aynı bloklarla gösterebilir | A |
| **Görünüş** | XAML değişir ama blok şeması (`EK` kartı) aynı kalır: token, boşluk, bağlama, durum hâli | A, C |
| **Ekran değişimi** | Blok eklenir, çıkar ya da yer değiştirir; görünen bir metin ya da sayı eklenir veya çıkar; yeni sayfa | A, B, C |

Tereddüt varsa üst sınıfı seç.

---

## Aşama 1 — Keşif  *(kod yazılmaz)*

- İsteğin dokunduğu kodu oku: hangi katmanlar, hangi dosyalar, kaç satır.
- İlgili kayıtları oku: `docs/SAPMALAR.md` (`S`), `docs/INVARYANTLAR.md` (`I`), `docs/SOZLUK.md`;
  ekrana dokunuyorsa `docs/EKRAN-KARTLARI.md` (`EK`) ve `docs/TASARIM-SAPMALARI.md` (`GS`).
- İstek var olan bir `S`, `I` ya da `GS` kaydıyla çelişiyor mu? Çelişiyorsa bu bir **karar
  değişikliğidir**: Kapı A'da açıkça söylenir, sessizce ezilmez.
- Veri şeması değişiyor mu?
- Büyüklük sınırı aşılıyor mu (aşağıda "Büyüklük")?

## Aşama 2 — Analiz  *(ONAY KAPISI A)*

Tek mesajda, bu sırayla:

1. **Ürün dili önce.** "Bu değişiklikten sonra kullanıcı şunu yapabilecek: …" — günlük dille,
   iki-üç cümle. Bir şey kalkıyor ya da davranış değişiyorsa sonucunu somut söyle: bugünkü veri
   ne olur, kim görür, kim siler? (`/tasarim-adimi` Aşama 2'deki V6c3 dersi.)
2. **Neden.** Hangi ihtiyaç ya da sorun; kullanıcı bugün bunu nasıl yapıyor ya da neden
   yapamıyor.
3. **Ne değişecek.** Davranış kararları, her biri tek satırda ve ilgili `S` / `I` / `GS`
   koduyla. Yeni bir karar `S` kaydı olacaksa önerisini yaz. Açık kalan ürün kararını 2–4 şıklı
   soru olarak sor, önerdiğini ilk sıraya koy ve "(Önerilen)" yaz.
4. **Nasıl.** Katman katman, yol yol dosya listesi: yeni mi değişen mi, yaklaşık satır, neden o
   katmanda.
5. **Riskler.** Dokunulan invariantlar (`I` kodlarıyla). Veri şeması değişiyorsa: göç adımı
   (`SchemaMigrations`), telefondaki gerçek verinin bu göçten geçeceği, yeni sürümün yedeğinin
   eski sürümde açılamayacağı, eski uygulamadan içe aktarmanın (`G1`) etkilenip etkilenmediği.
6. **Ekran sınıfı** (yukarıdaki tablo) ve gerekçesi. **Ekran değişimi** ise değişen bölüm için
   `/tasarim-adimi` Aşama 2'nin soru listesi ve Aşama 4'ün görsel bütçe tablosu, **önce / sonra**
   sütunlarıyla. Bütçe aşılıyorsa çıkar, küçültme. Konseptle çelişki `GS` kaydıdır.
7. **Sürüm.** Bu iş minor sürüm çıkarır. **CHANGELOG maddesinin taslağı**: Release sayfasında
   aynen görünecek, kullanıcıya dönük, teknik terimsiz bir-iki madde ve alt başlığı
   (`### Eklendi` ya da `### Değişti`).

Onaylanan `S` ve `GS` kayıtları **şimdi** yazılır, Aşama 8'de değil (gerekçe `/tasima-adimi`
Aşama 3'te).

> **Kullanıcı onaylamadan Aşama 3'e geçilmez.** Soru sorarsa cevapla ve beklemeye devam et.
> Kullanıcı planın bir kısmını değiştirirse değişmiş planı tek mesajda yeniden sun; onay planın
> tamamına verilir.

## Aşama 3 — Düzen Sözleşmesi  *(ONAY KAPISI B — yalnız "ekran değişimi")*

`/tasarim-adimi` Aşama 5 ile birebir aynı: onaylanan bütçe blok şemasına çevrilir, her bloğun
sağında cevapladığı soru kodu (`← S1`) durur, her metin "kullanıcıya yeni bir şey söylüyor mu"
sınavından geçer, boş / yükleniyor / hata hâlleri tek cümleyle tanımlanır.

Kart `docs/EKRAN-KARTLARI.md`'de yazılır:

- Var olan ekran: kartı güncellenir, değişen bloklar işaretlenir (`← yeni`, `← çıktı`, `← taşındı`).
- Var olan bir ekrandan açılan yeni sayfa: o kartın içinde "Sayfa N" bölümü (`EK-V3` → "Sayfa 2"
  gibi).
- Bağımsız yeni ekran: sıradaki boş `EK-V<n>` anahtarı (bugün `EK-V14`). GK9 testi kartı yalnız
  bu biçimde tanır.

> **Kullanıcı onaylamadan Aşama 4'e geçilmez.** Yerleşimi gördükten sonra fikir değişir; bu kapı
> o yüzden var.

## Aşama 4 — Sözleşme + Kırmızı test

`/tasima-adimi` Aşama 4–5; ViewModel için `/tasarim-adimi` Aşama 6. Önce genel API, gövdeler
boş; sonra davranışı tarif eden testler, **kırmızı oldukları gösterilir**: beklenen değerle,
`NotImplementedException` ile değil.

Var olan bir davranış değişiyorsa önce onu koruyan testleri bul. Değişen davranışın testi
**güncellenir**, silinmez; neden değiştiği Kapı A'daki karar kodudur.

## Aşama 5 — Yeşil

En yalın implementasyon. "İleride lazım olur" yok.

Kapı A'daki planın dışına çıkma. Çıkmak gerekiyorsa (planda olmayan bir dosya, başka bir
katman, yeni bir ekran metni, yeni bir bileşen) **dur ve sor**; sessizce genişletme.

## Aşama 6 — XAML  *(yalnız "görünüş" ve "ekran değişimi")*

`/tasarim-adimi` Aşama 8'in kuralları aynen. Bitince bütçe sayımını tekrar yap ve Kapı A'daki
"sonra" sütunuyla karşılaştır; sapma varsa sebebini söyle.

## Aşama 7 — Kalkan  *(XAML'e dokunulduysa ONAY KAPISI C)*

```bash
dotnet build Mizan.sln -warnaserror -nologo -v q
```
```bash
dotnet test Mizan.sln -nologo -v q
```

0 hata, 0 uyarı, tümü yeşil; mimari (K1–K9) ve görsel (GK1–GK12) testler dahil. Kırmızıda
kuralı esnetme, kodu düzelt. Kural gerçekten yanlışsa kullanıcıya söyle; kural değişikliği ayrı
bir iştir.

XAML'e dokunulduysa Kalkan yeşilken `/tasarim-adimi` Aşama 9'un ikinci yarısı aynen uygulanır:
`./scripts/emulatorde-ac.ps1` bir kez çalışır, ajan emülatöre bir daha dokunmaz, kullanıcıya
görsel kontrol listesi (Yol · Bak · Tema) bırakılır, sorun bildirilirse oradaki döngü işler.

> **ONAY KAPISI C:** Kullanıcı "tamam" demeden Aşama 8'e geçilmez.

## Aşama 8 — Kayıt

Sırayla:

1. `CHANGELOG.md` — Kapı A'da onaylanan madde `## [Yayınlanmamış]` altına, onaylanan alt
   başlıkla.
2. `docs/DURUM.md` — en üste `### Geliştirme — <kısa ad>; kararlar <kodlar>` ve 3–6 satır: ne
   geldi, hangi karar verildi, nereye dikkat. Ekrana dokunulduysa bütçe sayımı ve "Görsel
   kontrol: kullanıcı onayladı (koyu + açık)." "Nerede kalındı" tablosunu güncelle.
3. `docs/SAPMALAR.md`, `docs/TASARIM-SAPMALARI.md`, `docs/EKRAN-KARTLARI.md` — Aşama 2–3'te
   yazıldılar; son hâlleriyle uyuşuyor mu bak.
4. Yeni invariant doğduysa `docs/INVARYANTLAR.md`: kural ve **koruyan testin tam adı**. Kodu
   tablodaki son koddan devam ettir.
5. Yeni kavram adlandırıldıysa `docs/SOZLUK.md`.
6. İstek `docs/TASIMA-PLANI.md`'de açık bir kutuysa kutuyu işaretle.
7. **Tek commit.** İngilizce özet, Türkçe gövde, son satır `Geliştirme: <kısa ad>`.

```
feat(app): add category totals to period detail

Dönem ayrıntısına ... eklendi; ... (kısa gerekçe, karar kodu).

Bütçe: hero 1/1, kart 3/4, label 24/28, cümle 2/3.

Geliştirme: dönem ayrıntısında kategori toplamları
```

## Aşama 9 — Sürüm

`.claude/skills/surum/SKILL.md`'yi oku ve uygula. Kapısı yoktur; onay Kapı A'da sürüm notuyla
birlikte alındı. Protokol, yayın koşusu bitip Release'in imzalı APK ile çıktığı doğrulanınca
biter.

---

## Büyüklük

- Bir çalıştırma en fazla **~300 satır üretim kodu** getirir (testler hariç).
- Aşılıyorsa Aşama 1'de dur ve Kapı A'da bölünme öner. Parçalar **dikey** kesilir: her parça
  kendi başına kullanıcıya bir şey getirir ve kendi sürümünü çıkarır. Katman katman (önce
  Domain, sonra Application…) kesilmez; yarım bir özellik telefona gitmemeli.
- Dikey kesilemeyen bir altyapı parçası varsa (yalnız bir şema göçü gibi) Kapı A'da açıkça
  söylenir: o parça CHANGELOG'a madde yazmaz, `/surum` Aşama 1'de "yayınlanacak bir şey yok" der
  ve sürüm kullanıcıya görünen ilk parçayla çıkar.

## İş dışında fark edilenler

Geliştirme sırasında bu işin parçası olmayan bir hata görürsen düzeltme: not al ve Aşama 9'dan
sonra kullanıcıya söyle. Kullanıcının gördüğü bir hataysa `/bug-fix`, terim, kavram ya da
doküman hatasıysa `/duzeltme` işidir.
