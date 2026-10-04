<#
.SYNOPSIS
    Mizan'ı emülatörde Maestro akışıyla uçtan uca sınar (K3, S81).

.DESCRIPTION
    Hazırlığı scripts/emulatorde-ac.ps1 yapar (emülatör, yeni APK, profil yedeği). Gezinmeyi ve
    doğrulamayı yalnız .maestro/flows/full_regression_flow.yaml yapar; K1 onu AutomationIds
    sembollerinden üretir. Bu betik tıklamaz, ekrana bakmaz, sonucu yorumlamaz: çıkış kodu
    Maestro'nunkiyle aynıdır.

    Akış clearState ile başlar ve uygulamanın bütün verisini siler. Bu yüzden emülatörde profil
    varken yeni bir yedek oluşmadan akış koşmaz; akıştan sonra (hata olsa da) akışın açtığı test
    profili silinir ve yedek geri yüklenir.

    Maestro CLI kullanıcının kurulumudur (https://maestro.mobile.dev). Betik onu indirmez.

.PARAMETER Avd
    Çalışan emülatör yoksa başlatılacak AVD. Varsayılan: "mizan_emulator"

.PARAMETER Akis
    Koşulacak Maestro akışı. Varsayılan: .maestro/flows/full_regression_flow.yaml (betiğe göre)

.PARAMETER YedekKlasoru
    emulatorde-ac.ps1'in profil yedeklerini yazdığı klasör (içinde kişisel veri var, repo dışında durur).
#>
[CmdletBinding()]
param (
    [string]$Avd = "mizan_emulator",
    [string]$Akis,
    [string]$YedekKlasoru = (Join-Path ([Environment]::GetFolderPath("MyDocuments")) "planor-emulator-yedek\emulator\otomatik")
)

# adb stderr'e bilgi satırı yazar; "Stop" bunları hata sayıp betiği keserdi.
$ErrorActionPreference = "Continue"

$packageId = "com.mizan.app"

# $PSScriptRoot parametre varsayılanında bazı ortamlarda boş gelir; bu yüzden gövdede çözülür.
if (-not $Akis) {
    $Akis = Join-Path $PSScriptRoot "..\.maestro\flows\full_regression_flow.yaml"
}

function Stop-WithError([string]$message) {
    Write-Host "HATA: $message" -ForegroundColor Red
    exit 1
}

function Get-RunningEmulator {
    $line = & adb devices 2>$null | Where-Object { $_ -match '^emulator-\d+\s+device$' } | Select-Object -First 1
    if ($null -eq $line) { return $null }
    return ($line -split '\s+')[0]
}

function Test-DeviceHasProfiles([string]$serial) {
    & adb -s $serial shell run-as $packageId ls files/profiles 2>&1 | Out-Null
    return $LASTEXITCODE -eq 0
}

foreach ($tool in "adb", "emulator", "dotnet", "maestro") {
    if ($null -eq (Get-Command $tool -ErrorAction SilentlyContinue)) {
        Stop-WithError "'$tool' bulunamadı. Maestro için: https://maestro.mobile.dev"
    }
}
if (-not (Test-Path $Akis)) {
    Stop-WithError "Akış dosyası yok: $Akis (üretmek için: dotnet test tests/Mizan.Regression.Tests)"
}

# 1. Emülatör, yeni APK ve profil yedeği
$baslangic = Get-Date
& "$PSScriptRoot\emulatorde-ac.ps1" -Avd $Avd -YedekKlasoru $YedekKlasoru
if ($LASTEXITCODE -ne 0) {
    Stop-WithError "Emülatör hazırlanamadı; akış koşmadı."
}

$serial = Get-RunningEmulator
if ($null -eq $serial) {
    Stop-WithError "Çalışan emülatör bulunamadı."
}

# 2. Veri varsa yedek şart: akış clearState ile hepsini siler
$yedek = $null
if (Test-DeviceHasProfiles $serial) {
    $yedek = Get-ChildItem $YedekKlasoru -Filter "profiller-*.tar" -ErrorAction SilentlyContinue |
        Where-Object { $_.LastWriteTime -ge $baslangic } |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($null -eq $yedek) {
        Stop-WithError "Emülatörde profil var ama bu koşunun yedeği yok; akış koşmadı."
    }
}

# 3. Akışı koş; sonuç ne olursa olsun emülatörü eski hâline döndür
$maestroKodu = 1
try {
    & maestro --device $serial test $Akis
    $maestroKodu = $LASTEXITCODE
}
finally {
    & adb -s $serial shell am force-stop $packageId 2>&1 | Out-Null
    & adb -s $serial shell run-as $packageId rm -rf files/profiles 2>&1 | Out-Null
    if ($null -ne $yedek) {
        & "$PSScriptRoot\emulatorde-ac.ps1" -GeriYukle $yedek.FullName
    }
}

if ($maestroKodu -eq 0) {
    Write-Host "Regresyon akışı geçti." -ForegroundColor Green
} else {
    Write-Host "Regresyon akışı BAŞARISIZ (Maestro çıkış kodu $maestroKodu)." -ForegroundColor Red
}
exit $maestroKodu
