<#
.SYNOPSIS
    Bir sürüm etiketinin yayın koşusunu bitene kadar izler ve imzalı APK'nın Release'e çıktığını doğrular.

.DESCRIPTION
    /surum protokolünün son adımı (.claude/skills/surum/SKILL.md). Etiket pushlandıktan sonra
    release.yml koşusunu bulur, bitene kadar izler; başarılıysa GitHub Release'inde imzalı APK'nın
    durduğunu denetler ve Release adresini yazar.

    Koşu düşerse düşen adımın günlüğünün sonunu yazar ve hata ile biter. Etikete ve Release'e
    dokunmaz: yeniden koşturmak ya da etiketi kaldırmak protokolün kararıdır, betiğin değil.

    Çıkış kodu 0: Release imzalı APK ile yayında. 1: koşu bulunamadı, düştü ya da APK eksik.

.PARAMETER Tag
    Yayın etiketi (vX.Y.Z).

.PARAMETER RunId
    İzlenecek koşu. Verilmezse etiketin en yeni release.yml koşusu aranır; yeniden koşturulan
    bir koşuyu izlerken verilir.

.PARAMETER FindTimeoutSeconds
    Etiket pushlandıktan sonra koşunun GitHub'da görünmesi için beklenecek en uzun süre (saniye).
    Varsayılan: 120
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)][string]$Tag,
    [long]$RunId,
    [int]$FindTimeoutSeconds = 120
)

# gh ilerlemeyi stderr'e yazar; "Stop" bunu hata sayıp izlemeyi keserdi. Sonuç çıkış kodundan okunur.
$ErrorActionPreference = "Continue"

if ($Tag -notmatch '^v\d+\.\d+\.\d+$') {
    Write-Host "Etiket vX.Y.Z biçiminde olmalı: '$Tag'" -ForegroundColor Red
    exit 1
}

# Etiket pushlandıktan hemen sonra koşu birkaç saniye listede görünmeyebilir.
if (-not $RunId) {
    $deadline = (Get-Date).AddSeconds($FindTimeoutSeconds)
    while (-not $RunId) {
        $found = gh run list --workflow release.yml --branch $Tag --limit 1 --json databaseId --jq '.[0].databaseId'
        if ($LASTEXITCODE -eq 0 -and $found) {
            $RunId = [long]$found
        }
        elseif ((Get-Date) -ge $deadline) {
            Write-Host "$Tag için release.yml koşusu $FindTimeoutSeconds saniyede görünmedi. Etiket pushlandı mı?" -ForegroundColor Red
            exit 1
        }
        else {
            Start-Sleep -Seconds 5
        }
    }
}

$runUrl = gh run view $RunId --json url --jq '.url'
Write-Host "İzleniyor: $Tag, koşu $RunId ($runUrl)"

gh run watch $RunId --exit-status --compact --interval 30
if ($LASTEXITCODE -ne 0) {
    Write-Host "Yayın koşusu başarısız: $runUrl" -ForegroundColor Red
    Write-Host "Düşen adımın günlüğünün sonu:" -ForegroundColor Red
    gh run view $RunId --log-failed | Select-Object -Last 80
    exit 1
}

$assets = gh release view $Tag --json assets --jq '.assets[].name'
if ($LASTEXITCODE -ne 0 -or -not ($assets | Where-Object { $_ -like '*-Signed.apk' })) {
    Write-Host "Koşu başarılı ama $Tag Release'inde imzalı APK yok." -ForegroundColor Red
    exit 1
}

$releaseUrl = gh release view $Tag --json url --jq '.url'
Write-Host "Yayında: $releaseUrl" -ForegroundColor Green
exit 0
