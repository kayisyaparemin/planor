# Durum

Bu repo şu anda **boş iskelet** hâlinde. Hiçbir iş kodu taşınmadı.

## Nerede kalındı

| | |
|---|---|
| Son tamamlanan adım | **F1** — Mimari test kalkanı |
| Sıradaki adım | **F2** — `IClock` + `SystemClock` + takvim kuralları (`CalendarRules`) |
| Test sayısı | 7 |
| Şema sürümü | — |

## Adım günlüğü

Her taşıma adımından sonra buraya en üste 3–6 satırlık bir giriş eklenir:
ne geldi, hangi kararı verdik, nereye dikkat etmeli.

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
