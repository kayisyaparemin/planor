# Durum

Bu repo şu anda **boş iskelet** hâlinde. Hiçbir iş kodu taşınmadı.

## Nerede kalındı

| | |
|---|---|
| Son tamamlanan adım | **F2** — `IClock` + `SystemClock` + takvim kuralları (`CalendarRules`) |
| Sıradaki adım | **F3** — Para ve yuvarlama yardımcıları + `SOZLUK.md`'nin ilk doldurulması |
| Test sayısı | 25 |
| Şema sürümü | — |

## Adım günlüğü

Her taşıma adımından sonra buraya en üste 3–6 satırlık bir giriş eklenir:
ne geldi, hangi kararı verdik, nereye dikkat etmeli.

### F2 — `IClock` + `SystemClock` + takvim kuralları (`CalendarRules`)

Zaman ve takvim altyapısı taşındı. `CalendarRules` saf hesap olarak `Mizan.Domain` altına,
`IClock` portu `Mizan.Application` altına, `SystemClock` adaptörü ise `Mizan.Infrastructure` altına
alındı (T9 düğümü çözüldü, K3 gereği dosyalar ayrıldı). `BR-CALENDAR-01` artık yıl ve ay sonu kenetleme
davranışını koruyan 16 domain testi ve 2 altyapı testi eklendi. Toplam 25 test yeşil.

### F1 — Mimari test kalkanı

`Mizan.Architecture.Tests` altında K1–K8 mimari kurallarını ve tip/dosya boyutu
sınırlarını denetleyen 7 test yazıldı. Test projelerinde xUnit konvansiyonu için
CA1707 uyarısı bastırıldı. 7 test yeşil.

### Bootstrap — iskelet kuruldu

11 proje (5 kaynak + 6 test), kural kitabı, CI ve doküman iskeleti oluşturuldu.
Kod yok. `dotnet build` ve `dotnet test` yeşil, 0 test çalışıyor.

Alınan yapısal kararlar:
- `Mizan.Presentation` MAUI'ye referans vermiyor, bu yüzden ViewModel'ler test edilebilir
  ve UI tipi kullanmak derleme hatası.
- Her katmanın kendi test projesi var, katman ihlali proje referansıyla engelleniyor.
- App id `com.mizan.app`; eski `com.coinflow.mobile` mirası yok, veri taşıma G1 adımında.
