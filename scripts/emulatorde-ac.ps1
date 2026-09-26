<#
.SYNOPSIS
    Mizan'ı Android emülatöründe derleyip kurar, açar ve emülatörü açık bırakır.

.DESCRIPTION
    Ekran adımlarının (tasarim-adimi.md Aşama 9) tek emülatör komutu. Çalışan bir emülatör
    yoksa verilen AVD'yi ayrı bir pencerede başlatır ve açılmasını bekler; sonra uygulamayı
    Debug olarak derleyip kurar ve ön planda başlatır.

    Betik gezinmez, tıklamaz, ekran görüntüsü almaz, tema değiştirmez. Ekrana bakmak
    kullanıcının işidir.

.PARAMETER Avd
    Çalışan emülatör yoksa başlatılacak AVD. Varsayılan: "mizan_emulator"

.PARAMETER BootTimeoutSeconds
    Emülatörün açılması için beklenecek en uzun süre (saniye). Varsayılan: 300
#>
[CmdletBinding()]
param (
    [string]$Avd = "mizan_emulator",
    [int]$BootTimeoutSeconds = 300
)

# adb stderr'e bilgi satırı yazar; "Stop" bunları hata sayıp betiği keserdi.
$ErrorActionPreference = "Continue"

$packageId = "com.mizan.app"
$project = Join-Path $PSScriptRoot "..\src\Mizan.App\Mizan.App.csproj"

function Stop-WithError([string]$message) {
    Write-Host "HATA: $message" -ForegroundColor Red
    exit 1
}

function Get-RunningEmulator {
    $line = & adb devices 2>$null | Where-Object { $_ -match '^emulator-\d+\s+device$' } | Select-Object -First 1
    if ($null -eq $line) { return $null }
    return ($line -split '\s+')[0]
}

foreach ($tool in "adb", "emulator", "dotnet") {
    if ($null -eq (Get-Command $tool -ErrorAction SilentlyContinue)) {
        Stop-WithError "'$tool' PATH'te bulunamadı."
    }
}

# 1. Emülatörü bul ya da başlat
$serial = Get-RunningEmulator
if ($null -eq $serial) {
    Write-Host "Emülatör başlatılıyor: $Avd" -ForegroundColor Cyan
    Start-Process -FilePath (Get-Command emulator).Source -ArgumentList "-avd", $Avd
}

# 2. Açılışı bekle
$deadline = (Get-Date).AddSeconds($BootTimeoutSeconds)
$booted = $false
while ((Get-Date) -lt $deadline) {
    if ($null -eq $serial) { $serial = Get-RunningEmulator }
    if ($null -ne $serial) {
        $bootCompleted = & adb -s $serial shell getprop sys.boot_completed 2>$null
        if ("$bootCompleted".Trim() -eq "1") { $booted = $true; break }
    }
    Start-Sleep -Seconds 2
}
if (-not $booted) {
    Stop-WithError "Emülatör $BootTimeoutSeconds sn içinde açılmadı."
}

# 3. Derle ve kur
Write-Host "Derleniyor ve kuruluyor ($serial)..." -ForegroundColor Cyan
& dotnet build $project -f net8.0-android -c Debug -t:Install "-p:AdbTarget=-s $serial" -nologo -v q
if ($LASTEXITCODE -ne 0) {
    Stop-WithError "Derleme ya da kurulum başarısız."
}

# 4. Güncel APK'yı ön planda başlat
& adb -s $serial shell am force-stop $packageId 2>&1 | Out-Null
& adb -s $serial shell monkey -p $packageId -c android.intent.category.LAUNCHER 1 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Stop-WithError "Uygulama başlatılamadı."
}

Write-Host "Mizan emülatörde açık ($serial)." -ForegroundColor Green
exit 0
