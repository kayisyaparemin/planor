<#
.SYNOPSIS
    Cobertura kapsam raporlarını ayrıştırarak katman bazlı kapsam eşiklerini denetler.

.DESCRIPTION
    Bu betik, TestResults dizinindeki coverage.cobertura.xml dosyalarını tarar,
    Mizan.Domain (%90), Mizan.Application (%80) ve Mizan.Presentation (%70) katmanlarının
    birleşik satır kapsamını (merged line coverage) hesaplar ve eşiklerin altında kalınması
    durumunda çıkış kodu 1 ile süreci sonlandırır.

.PARAMETER ResultsDirectory
    Kapsam XML dosyalarının aranacağı dizin. Varsayılan: "TestResults"

.PARAMETER DomainThreshold
    Mizan.Domain katmanı için minimum kapsam yüzdesi. Varsayılan: 90

.PARAMETER ApplicationThreshold
    Mizan.Application katmanı için minimum kapsam yüzdesi. Varsayılan: 80

.PARAMETER PresentationThreshold
    Mizan.Presentation katmanı için minimum kapsam yüzdesi. Varsayılan: 70
#>
[CmdletBinding()]
param (
    [string]$ResultsDirectory = "TestResults",
    [double]$DomainThreshold = 90.0,
    [double]$ApplicationThreshold = 80.0,
    [double]$PresentationThreshold = 70.0
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $ResultsDirectory)) {
    Write-Error "HATA: Sonuç dizini bulunamadı: $ResultsDirectory"
    exit 1
}

$coverageFiles = Get-ChildItem -Path $ResultsDirectory -Filter "coverage.cobertura.xml" -Recurse
if ($coverageFiles.Count -eq 0) {
    Write-Error "HATA: '$ResultsDirectory' altında hiç coverage.cobertura.xml dosyası bulunamadı."
    exit 1
}

Write-Host "Kapsam dosyaları taranıyor ($($coverageFiles.Count) dosya bulundu)..." -ForegroundColor Cyan

# Katman bazında satır durumlarını takip etmek için sözlükler
# Anahtar: "SinifAdi:SatirNo", Değer: boolean (en az bir kez çalıştı mı)
$layerCoverage = @{
    "Mizan.Domain"       = @{ "Threshold" = $DomainThreshold;       "Lines" = @{} }
    "Mizan.Application"  = @{ "Threshold" = $ApplicationThreshold;  "Lines" = @{} }
    "Mizan.Presentation" = @{ "Threshold" = $PresentationThreshold; "Lines" = @{} }
}

foreach ($file in $coverageFiles) {
    [xml]$xml = Get-Content -Path $file.FullName
    $packages = $xml.SelectNodes("//package")
    if ($null -eq $packages) { continue }

    foreach ($pkg in $packages) {
        $pkgName = $pkg.name
        if (-not $layerCoverage.ContainsKey($pkgName)) {
            continue
        }

        $classes = $pkg.SelectNodes(".//class")
        if ($null -eq $classes) { continue }

        foreach ($cls in $classes) {
            $className = $cls.name
            $lines = $cls.SelectNodes(".//line")
            if ($null -eq $lines) { continue }

            foreach ($line in $lines) {
                $lineNum = $line.number
                $hits = [int]$line.hits
                $key = "$($className):$($lineNum)"

                $existing = $layerCoverage[$pkgName]["Lines"][$key]
                if ($null -eq $existing -or (-not $existing)) {
                    $layerCoverage[$pkgName]["Lines"][$key] = ($hits -gt 0)
                }
            }
        }
    }
}

$hasFailure = $false
Write-Host "`n--- Kapsam Eşik Kontrolü ---" -ForegroundColor Yellow

foreach ($layerName in $layerCoverage.Keys | Sort-Object) {
    $info = $layerCoverage[$layerName]
    $linesDict = $info["Lines"]
    $threshold = [double]$info["Threshold"]
    $totalLines = $linesDict.Count

    if ($totalLines -eq 0) {
        Write-Host "[-] $layerName : Henüz ölçülebilir kod taşınmadı (0 satır). Eşik atlandı." -ForegroundColor DarkGray
        continue
    }

    $coveredLines = 0
    foreach ($k in $linesDict.Keys) {
        if ($linesDict[$k]) {
            $coveredLines++
        }
    }

    $rate = [Math]::Round(($coveredLines / $totalLines) * 100.0, 2)

    if ($rate -ge $threshold) {
        Write-Host "[OK] $layerName : %$rate >= %$threshold ($coveredLines/$totalLines satır)" -ForegroundColor Green
    } else {
        Write-Host "[FAIL] $layerName : %$rate < %$threshold ($coveredLines/$totalLines satır)" -ForegroundColor Red
        $hasFailure = $true
    }
}

Write-Host "----------------------------`n" -ForegroundColor Yellow

if ($hasFailure) {
    Write-Host "Kapsam eşiği sağlanamadı! CI derlemesi durduruluyor." -ForegroundColor Red
    exit 1
}

Write-Host "Tüm katmanlar kapsam eşiklerini başarıyla sağladı." -ForegroundColor Green
exit 0
