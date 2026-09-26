# İş Akışı — Düzeltme

Taşıma adımının **dışında** fark edilen bir hata için. Kullanıcı dokümanları okurken,
uygulamayı kullanırken ya da eski kodu incelerken bir şey yakaladıysa bu akış işler.

Adımın *içinde* fark edilenler buraya değil, `tasima-adimi.md` Aşama 3'e (Sapma Kararı)
aittir — bir ekran adımındaysan `tasarim-adimi.md` Aşama 3'e (davranış) ya da Aşama 4'e
(görsel bütçe) aittir.

> Bu akış taşıma protokolünü **değiştirmez.** Yanında çalışır ve çıktısını ona besler:
> düzeltme, sapma kaydına veya yasaklı terim listesine dönüşür; protokol onu oradan okur.

---

## Aşama 1 — Türü belirle  *(kod yazılmaz)*

Beş tür vardır ve her biri farklı yoldan gider. Önce hangisi olduğunu **söyle**, sonra devam et.

| Tür | Nasıl anlarsın | Nereye gider |
|---|---|---|
| **T — Terim hatası** | Bir kavramın yanlış ya da eski adı kullanılmış | Sözlük + yasaklı liste + test |
| **K — Kavramsal sapma** | Eski kod bir şeyi **yanlış modellemiş**; isim değil davranış sorunu | `SAPMALAR.md` + etkilenen adımlar |
| **D — Doküman hatası** | Kod doğru, doküman yanlış ya da bayat | Doğrudan düzelt |
| **B — Bug** | Taşınmış kod yanlış davranıyor | Önce kırmızı test |
| **G — Görsel sapma** | Ekran doğru çalışıyor ama yanlış görünüyor; tutarsız token, bütçe aşımı, konseptten kopma | `TASARIM-SAPMALARI.md` + ekran kartı |

Emin değilsen **K** varsay ve kullanıcıya sor. En pahalı hata, kavramsal bir sapmayı terim
hatası sanıp yalnızca adını değiştirmektir — o zaman yanlış model doğru isimle hayatta kalır.

---

## Aşama 2 — Kapsamı ölç  *(DURAKLAMA NOKTASI)*

Düzeltmenin nereye dokunduğunu **say**, tahmin etme:

- Yeni repoda kaç dosya, kaç satır etkileniyor?
- `docs/TASIMA-PLANI.md`'de **tamamlanmış** hangi adımlar geri açılmak zorunda?
- Henüz taşınmamış hangi adımların tarifi değişecek?
- Bu, eski projede de var olan bir sorun mu? (Varsa söyle; eski projeye **dokunma**.)

Sonra kullanıcıya üç şıkla gel:

| Şık | Ne zaman |
|---|---|
| **Şimdi düzelt** | Kapsam dar, ya da yanlış temel üzerine inşa etmeye devam etmek pahalıya patlar |
| **Kaydet, sırası gelince düzelt** | Henüz taşınmamış adımları etkiliyor; kayıt yeterli |
| **Düzeltme** | Bilinçli bir sadeleştirme; `INVARYANTLAR.md` → "Kasıtlı sadeleştirmeler"e yazılır |

**Kullanıcı seçmeden Aşama 3'e geçilmez.**

---

## Aşama 3 — Türüne göre uygula

### T — Terim hatası

1. Doğru adı `docs/SOZLUK.md`'deki ilgili tabloya ekle (yoksa).
2. Eski adı **yasaklı terimler** tablosuna ekle: yerine ne geleceği, kapsamı, istisnası.
3. `docs/SAPMALAR.md`'ye `S` kaydı: kavramın hangi iki adı vardı, hangisi doğru, neden.
4. `dotnet test` — `YasakliTerimler_KaynaktaGecemez` şimdi kırmızı olmalı. **Kırmızı olduğunu
   göster.** Olmuyorsa eşleştirme kuralı terimi yakalamıyordur; önce onu düzelt.
5. Taşınmış kodda adı değiştir, testi yeşile al.
6. Yanlış pozitif ürettiyse terimi listeden çıkarma — istisnayı yaz ve
   `YasakliTerimRegex_YanlisPozitifUretmez` testine örneği ekle.

### K — Kavramsal sapma

1. `docs/SAPMALAR.md`'ye yeni `S` kaydı: eski davranış, **neden yanlış**, yeni davranış,
   etkilenen adımlar, durum.
2. `docs/TASIMA-PLANI.md`'de etkilenen adımların tarifini güncelle ve sapma koduna referans ver.
3. Tamamlanmış bir adım etkileniyorsa kutusunu **geri aç** ve sebebini yaz.
4. Düzeltme şimdi yapılacaksa: kırmızı test → yeşil, `tasima-adimi.md` Aşama 5–7 gibi.
5. Yeni bir invariant doğduysa `docs/INVARYANTLAR.md` — koruyan testin tam adıyla.

### D — Doküman hatası

1. Dokümanı düzelt.
2. **Aynı hata başka dosyalarda da var mı?** Ara. Dokümanlar kopyalanarak yazılır, hata da
   kopyalanır. Tek yerde düzeltip geçme.
3. Yanlış bilgi bir karara yol açmış mıysa — yani biri ona güvenerek kod yazmışsa — bu
   aslında **K**'dır; tür kararını düzelt ve oradan devam et.

### B — Bug

1. Hatayı **kullanıcının gördüğü hâliyle** tarif eden bir test yaz. Testi çalıştır, kırmızı
   olduğunu göster. İç yapıyı değil davranışı iddia et.
2. Düzelt, yeşile al.
3. `docs/INVARYANTLAR.md`'ye satır ekle: kural + **koruyan testin tam adı**.
4. Aynı sınıf hata başka yerde de olabilir mi? Bak ve söyle.

### G — Görsel sapma

Önce **hangi katmanın** sorunu olduğunu ayır — yanlış katmanda düzeltmek en pahalı hata:

| Belirti | Katman | Ne yapılır |
|---|---|---|
| Aynı iş için iki ekranda iki farklı görünüm | Sistem | `TASARIM-SISTEMI.md` § Bileşenler'e bakılır; eksikse bileşen doğar (iki ekran kuralı) |
| Bir ekran kalabalık ama testler yeşil | Ekran kartı | Bütçe sayımı doğru, **niteliği** yanlış; Aşama 4 kararları yeniden açılır |
| Token yanlış kullanılmış | XAML | Doğrudan düzelt; GK testi neden yakalamadığını söyle |
| Konsept başka şey gösteriyor | Karar | `TASARIM-SAPMALARI.md`'ye `GS` kaydı |

Sonra:

1. `docs/TASARIM-SAPMALARI.md`'ye `GS` kaydı: konsept ne vaat ediyor, **neden
   uygulanmıyor**, yerine ne yapılıyor, etkilenen ekran kartları.
2. Etkilenen `docs/EKRAN-KARTLARI.md` kartlarını güncelle — kesme kararı değiştiyse bütçe
   sayımı da değişir.
3. Tamamlanmış bir ekran etkileniyorsa `docs/TASIMA-PLANI.md`'de kutusunu **geri aç** ve
   sebebini yaz.
4. Bir GK testi bu sapmayı yakalayamadıysa **testi güçlendir** — sapmayı elle düzeltip
   geçmek, aynı sapmanın bir sonraki ekranda tekrar doğmasına izin vermek demektir.
5. Yeni bir token, bileşen ya da grafik primitifi gerekiyorsa bu bir **sistem değişikliğidir**:
   `TASARIM-SISTEMI.md` + testi birlikte değişir, ve bu ayrı bir adımdır.

> En pahalı görsel hata, bir sistem sorununu ekran sorunu sanıp tek ekranda düzeltmek —
> o zaman tasarım sistemi ekran ekran çatallanır ve eski projenin 14 farklı görsel diline
> geri dönülür.

---

## Aşama 4 — Kayıt

1. `docs/DURUM.md` — "Düzeltme" başlığı altına ne değişti, neden.
2. Etkilenen kayıtlar güncel mi: `SOZLUK.md`, `SAPMALAR.md`, `INVARYANTLAR.md`,
   `TASIMA-PLANI.md` — görsel düzeltmede ayrıca `TASARIM-SAPMALARI.md`, `EKRAN-KARTLARI.md`,
   `TASARIM-SISTEMI.md`.
3. `dotnet build` + `dotnet test` — 0 hata, 0 uyarı, tümü yeşil.
4. Tür **B** ya da **G** ise uygulamayı aç ve bırak: `./scripts/emulatorde-ac.ps1`. Kullanıcı
   düzeltmeyi kendi gözüyle onaylamadan commit atılmaz. Emülatöre dokunma kuralı
   `tasarim-adimi.md` Aşama 9 ile aynıdır.
5. **Tek commit.** `fix(...)` ya da `docs(...)`; gövdede düzeltmenin türü ve sapma kodu.

```
fix(sozluk): period definition no longer assumes a salary

Dönem tanımı "iki maaş arasındaki aralık" diyordu. Ürün her gelir
düzenine hitap ettiği için bu tanım yanlış; dönem çapası gelirden
ayrı bir kavram.

Tür: T (terim) → S11
```

---

## Eski projeye dokunma

Bu akış **yalnız yeni repoda** çalışır. Eski projede de aynı hata varsa:

- `SAPMALAR.md`'ye not düş (bkz. S17 biçimi),
- kullanıcıya söyle,
- ve orada bırak.

Eski proje salt okunur bir referanstır. Onu düzeltmek ayrı bir karardır ve kullanıcının
açıkça istemesi gerekir.
