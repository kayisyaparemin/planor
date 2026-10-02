---
name: tasima-adimi
description: Eski projeden bu repoya bir parça taşıma protokolü — 8 aşama, atlanamaz. Kullanıcı bir taşıma adımını başlattığında.
argument-hint: "<adım kodu>"
disable-model-invocation: true
---

**Adım kodu:** $ARGUMENTS
**Eski proje:** `C:\Users\kayis\Documents\mizan`

Adımın adını ve tarifini `docs/TASIMA-PLANI.md`'deki satırından, ilgili `S` kayıtlarını oradaki
referanslardan al. Adım kodu verilmemişse ya da planda yoksa dur ve sor; sıradaki adımı kendin
seçme.

Aşama 1 (keşif) ile başla; ilk cümlede adımı plandaki adıyla söyle.

**Kaynağı eski proje olmayan adımlar.** Plandaki satır ya da fazın girişi "kaynak eski proje değil"
diyorsa (Faz T, V3 yenilemesi) eski repoya **hiç bakılmaz**; oradaki kod bu adım için ne ipucu ne
otoritedir. Aşamalar şöyle okunur:

- **Aşama 1:** bu repodaki ilgili kodu oku; raporlanan dosyalar bugünkü dosyalardır.
- **Aşama 2**, 3. soru: "Bugünkü hâlinde ne eksik ya da yanlış?"
- **Aşama 3:** üç şık yerine plandaki satırın listelediği kararlar sorulur; yeni davranış `S` kaydı olur.
- **Aşama 5:** eski testlere bakılmaz.

# İş Akışı — Bir Taşıma Adımı

Eski projeden (`C:\Users\kayis\Documents\mizan`) bu repoya bir parça taşırken izlenecek
protokol. **Sekiz aşama vardır ve hiçbiri atlanamaz.**

Bu protokolün amacı kodu taşımak değil; kodun **sahibi tarafından anlaşılmış olarak** taşınmasını
sağlamaktır. Birinci Mizan'ın problemi eksik koddu değildi, anlaşılmamış koddu.

---

## Aşama 1 — Keşif  *(kod yazılmaz)*

Eski repodaki ilgili dosyaları oku. Şunu raporla:

- Hangi dosyalar, kaç satır
- Bu parça hangi tiplere bağlı, hangi tipler buna bağlı
- `docs/TASIMA-PLANI.md`'de bu adımın gerektirdiği önceki adımlar tamamlanmış mı

Bağımlılığı henüz taşınmamış bir şeye rastladıysan **dur ve söyle.** Sırayı kendi başına değiştirme.

## Aşama 2 — Anlatım  *(DURAKLAMA NOKTASI)*

Kullanıcıya Türkçe anlat:

1. **Bu parça ne işe yarıyor?** İş dilinde, kod terimiyle değil.
   ("Kullanıcı bankadan aldığı taksit tutarını biliyor ama faiz oranını bilmiyor; bu sınıf
   oranı geri çözüyor, çünkü erken kapama tutarı oran olmadan hesaplanamıyor.")
2. **Hangi iş kuralını taşıyor?** Varsa mevzuat maddesi, varsa `docs/INVARYANTLAR.md` kodu.
3. **Eski hâlinde ne sorunluydu?** `C:\Users\kayis\Documents\mizan\docs\v2\02-NEDEN-BU-YAPI.md`'deki (eski depo, arşiv) düğümlerden birine
   dokunuyor mu; yeni hâlinde ne değişecek.
4. **Yeni hâlinde hangi dosyalar oluşacak?** Yol yol liste.

Anlatımı bitirince **durma, Aşama 3'ü de aynı mesajda sun** — ikisi tek bir onay konuşmasıdır.

## Aşama 3 — Sapma Kararı  *(DURAKLAMA NOKTASI)*

Eski kodun doğru olduğu varsayılamaz. Bu aşama, "sadakatle taşı" ile "düzelterek taşı"
arasındaki kararı **açıkça** verdirir. Kararsız geçilemez.

Sırayla kontrol et:

1. **Kayıtlı sapma var mı?** `docs/SAPMALAR.md`'de bu adımı etkileyen bir `S` kaydı var mı?
   Varsa oku ve uygula — o karar zaten verilmiş, yeniden tartışma.
2. **Eski kod bu parçayı yanlış mı modellemiş?** İsim değil, **davranış** sor:
   - Tekil olması gereken bir şey çoğul mu, çoğul olması gereken tekil mi?
   - Bir kavram başka bir kavrama gereksizce yapışık mı?
   - Arayüzün kullanıcıya vaat ettiği şeyi motor gerçekten yapıyor mu?
3. **Yasaklı terim dokunuyor mu?** `docs/SOZLUK.md`'deki yasaklı terim tablosuna bak.
   Eski koddaki isim yasaklıysa, yeni adı **şimdi** kararlaştır — Aşama 4'te değil.
4. **Kullanıcının bu adım için ayrıca istediği bir refactor var mı?** Sor.

Sonra üç şıktan **birini** öner ve gerekçelendir:

| Şık | Ne zaman |
|---|---|
| **Olduğu gibi taşı** | Eski kod doğru; yalnızca yeni kural kitabına uyarlanacak |
| **Şu sapmayla taşı** | Eski kod yanlış modellemiş ya da yasaklı terim taşıyor |
| **Hiç taşıma** | Parça artık gereksiz (ölü kolon, kaldırılmış özellik kalıntısı, eski şema göçü) |

> **Kullanıcı onaylamadan Aşama 4'e geçilmez.** Aşama 2 ve 3 birlikte sunulur, tek bir onay
> alınır. Kullanıcı soru sorarsa cevapla ve beklemeye devam et. Onay gelmeden tek satır kod yazma.

Yeni bir sapma kararı verildiyse `docs/SAPMALAR.md`'ye **şimdi** yaz — Aşama 8'de değil.
Sebep: Aşama 4'teki sözleşme doğrudan bu karara dayanacak, ve karar kaydedilmeden yazılan
bir imza kimsenin hatırlamadığı bir varsayım hâline gelir.

## Aşama 4 — Sözleşme

Önce genel API'yi yaz, gövdeleri boş bırak:

```csharp
/// <summary>… neden var olduğu …</summary>
public sealed class LoanAmortizationCalculator
{
    public LoanAmortization Calculate(Loan loan) => throw new NotImplementedException();
}
```

Bu aşamada: tipler, arayüzler, `record`'lar, metot imzaları. İş mantığı **yok.**
Derlenmeli. Burada dur ve sözleşmenin doğru olduğundan emin ol — yanlış bir imza,
yanlış yazılmış yirmi testten daha pahalıdır.

## Aşama 5 — Kırmızı test

Davranışı tarif eden testleri yaz. Çalıştır. **Kırmızı olduklarını göster.**

- Testler `NotImplementedException` ile değil, **beklenen değerle** başarısız olmalı
  (yani gerçekten davranış iddia etmeli).
- Sınır durumları dahil et: sıfır, negatif, dönemin ilk ve son günü, tek taksit kalmış kredi.
- Eski projedeki mevcut testlerden **kopyalama**; onları oku, ama iddiayı yeniden yaz.
  Eski testlerin bir kısmı eski hataları dondurmuş olabilir.

## Aşama 6 — Yeşil

En yalın implementasyonu yaz. Testler geçsin.

Burada "ileride lazım olur" diye bir şey ekleme. `docs/TASIMA-PLANI.md`'de sonraki adımda
gelecek olan şey sonraki adımda gelir.

## Aşama 7 — Kalkan

```bash
dotnet build Mizan.sln
```
```bash
dotnet test Mizan.sln
```

Hepsi geçmeli: **0 hata, 0 uyarı, tüm testler yeşil.** Mimari testler (K1–K9) dahil.
Kapsam eşiği düştüyse test ekle, eşiği düşürme.

Yasaklı terim testi (K9) kırmızıysa, Aşama 3'te verilen sapma kararı eksik uygulanmış demektir.
Terimi değiştir; testi gevşetme.

Bir mimari test kırmızıya düştüyse: **kuralı esnetme, kodu düzelt.** Kural gerçekten yanlışsa
kullanıcıya söyle ve kural değişikliğini ayrı bir iş olarak ele al.

## Aşama 8 — Kayıt

Sırayla:

1. `docs/TASIMA-PLANI.md` — adımın kutusunu işaretle
2. `docs/DURUM.md` — "Bu adım ne getirdi" başlığı altına 3–6 satır
3. Yeni bir invariant doğduysa `docs/INVARYANTLAR.md` — kod, kural, **koruyan testin tam adı**
4. Yeni bir kavram adlandırdıysan `docs/SOZLUK.md`
5. **Tek commit.** Konvansiyon: İngilizce özet satırı + Türkçe gövde.

```
feat(domain): add loan amortization calculator

Bankanın açıkladığı taksit tutarından örtük faiz oranını geri çözen
hesaplayıcı taşındı (eski: LoanAmortizationCalculator.cs, 376 satır).

Eski hâlinde SimulationCalculator bu sınıfı kendi içinde new'liyordu;
artık zorunlu yapıcı parametresi (kural M1).

Adım: D13 — kredi itfası
```

---

## Adım büyüklüğü

- Bir adım en fazla **~300 satır üretim kodu** getirir.
- Bir adım **tek katmana** dokunur.
- Bir adım **tek iş yeteneği** taşır.

Üçünden biri aşılıyorsa adım büyük demektir: Aşama 1'de dur, bölünme öner, kullanıcıya sor.

## Taşımama hakkı

Eski koddaki her şey taşınmak zorunda değil. Bu, Aşama 3'ün üçüncü şıkkıdır (**hiç taşıma**) ve
kullanmaktan çekinme. Özellikle şunlar adaydır: ölü kolonlar, kullanılmayan ayar bayrakları,
eski şema göçleri, `*SourceTests` karşılıkları, kaldırılmış özelliklerin kalıntıları.

## Adım dışında fark edilenler

Bu protokol bir taşıma adımının *içinde* fark edilenleri çözer. Kullanıcı dokümanları okurken
ya da uygulamayı kullanırken bir hata yakalarsa — yani adım dışında — o iş buraya değil
`/duzeltme` akışına gider.
