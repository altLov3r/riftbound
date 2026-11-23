param(
    [string]$ServerExe = "Builds/EmberfordServer01/EmberfordServer.exe",
    [string]$ClientExe = "Builds/EmberfordClient01/Riftbound.exe",
    [uint16]$Port = 7777,
    [int]$MaxPlayers = 4,
    [int]$TimeoutSeconds = 95
)

$ErrorActionPreference = "Stop"
$root = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$srvFull = Join-Path $root $ServerExe
$cliFull = Join-Path $root $ClientExe
if (-not (Test-Path -LiteralPath $srvFull)) { throw "Missing Dedicated Server executable: $srvFull" }
if (-not (Test-Path -LiteralPath $cliFull)) { throw "Missing Client executable: $cliFull" }

$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$runDir = Join-Path $root ".code-check/emberford-r1/loopback-$stamp"
$dataDir = Join-Path $runDir "realm-data"
New-Item -ItemType Directory -Force -Path $runDir, $dataDir | Out-Null

# Snapshot offline saves before test so we prove zero mutation
$persistentDir = Join-Path $env:USERPROFILE "AppData\LocalLow\DefaultCompany\Riftbound"
$beforeSaves = @{}
if (Test-Path $persistentDir) {
    Get-ChildItem $persistentDir -Filter "*.json" -File | ForEach-Object {
        $beforeSaves[$_.FullName] = (Get-FileHash $_.FullName -Algorithm SHA256).Hash
    }
}

$srvOut = Join-Path $runDir "server-result.txt"
$srvLog = Join-Path $runDir "server-unity.log"
$realmLog = Join-Path $runDir "emberford-realm.log"
$srvArgs = "-batchmode -nographics -dedicatedServer -realmId emberford -realmName Emberford -realmListen 127.0.0.1 -realmPort $Port -realmMaxPlayers $MaxPlayers -realmDataDir `"$dataDir`" -realmLog `"$realmLog`" -realmSmokeRole server -realmSmokeOut `"$srvOut`" -realmSmokeSeconds $TimeoutSeconds -logFile `"$srvLog`""
$srvProc = Start-Process -FilePath $srvFull -ArgumentList $srvArgs -WorkingDirectory $root -WindowStyle Hidden -PassThru
Write-Host "Started Dedicated Server PID=$($srvProc.Id) on 127.0.0.1:$Port"

# Wait for server socket listen receipt in realm log
$readyLimit = (Get-Date).AddSeconds(30)
while ((Get-Date) -lt $readyLimit -and -not $srvProc.HasExited) {
    if ((Test-Path $realmLog) -and ((Get-Content $realmLog -Raw -ErrorAction SilentlyContinue) -match "StartServer succeeded")) { break }
    Start-Sleep -Milliseconds 300
}
if ($srvProc.HasExited) { throw "Dedicated Server exited early with code $($srvProc.ExitCode)" }

$c1Out = Join-Path $runDir "c1-result.txt"
$c2Out = Join-Path $runDir "c2-result.txt"
$c1RejoinOut = Join-Path $runDir "c1-rejoin-result.txt"

$c1Args = "-batchmode -nographics -realmClient -realmEndpoint 127.0.0.1 -realmPort $Port -realmCharName RealmOne -realmRace Human -realmClass Warrior -realmSmokeRole c1 -realmSmokeOut `"$c1Out`" -realmSmokeSeconds 55 -logFile `"$runDir/c1-unity.log`""
$c2Args = "-batchmode -nographics -realmClient -realmEndpoint 127.0.0.1 -realmPort $Port -realmCharName RealmTwo -realmRace Human -realmClass Mage -realmSmokeRole c2 -realmSmokeOut `"$c2Out`" -realmSmokeSeconds 75 -logFile `"$runDir/c2-unity.log`""

$c1Proc = Start-Process -FilePath $cliFull -ArgumentList $c1Args -WorkingDirectory $root -WindowStyle Hidden -PassThru
Start-Sleep -Milliseconds 600
$c2Proc = Start-Process -FilePath $cliFull -ArgumentList $c2Args -WorkingDirectory $root -WindowStyle Hidden -PassThru
Write-Host "Started Clients c1 PID=$($c1Proc.Id) and c2 PID=$($c2Proc.Id)"

$c1Deadline = (Get-Date).AddSeconds(60)
while ((Get-Date) -lt $c1Deadline -and -not $c1Proc.HasExited) { Start-Sleep -Milliseconds 250 }
if (-not $c1Proc.HasExited) { Stop-Process -Id $c1Proc.Id -Force; throw "c1 timed out" }
$c1Proc.WaitForExit()
Write-Host "c1 exited code=$($c1Proc.ExitCode)"

Start-Sleep -Milliseconds 1200
$c1RejoinArgs = "-batchmode -nographics -realmClient -realmEndpoint 127.0.0.1 -realmPort $Port -realmCharName RealmOne -realmRace Human -realmClass Warrior -realmSmokeRole c1_rejoin -realmSmokeOut `"$c1RejoinOut`" -realmSmokeSeconds 45 -logFile `"$runDir/c1-rejoin-unity.log`""
$c1rProc = Start-Process -FilePath $cliFull -ArgumentList $c1RejoinArgs -WorkingDirectory $root -WindowStyle Hidden -PassThru
Write-Host "Started Rejoin Client c1_rejoin PID=$($c1rProc.Id)"

$allDeadline = (Get-Date).AddSeconds($TimeoutSeconds)
while ((Get-Date) -lt $allDeadline -and (-not $c2Proc.HasExited -or -not $c1rProc.HasExited -or -not $srvProc.HasExited)) {
    Start-Sleep -Milliseconds 300
}

foreach ($p in @($c1rProc, $c2Proc, $srvProc)) {
    if (-not $p.HasExited) {
        Stop-Process -Id $p.Id -Force
        throw "Process PID=$($p.Id) timed out"
    }
    $p.WaitForExit()
}

$afterSaves = @{}
if (Test-Path $persistentDir) {
    Get-ChildItem $persistentDir -Filter "*.json" -File | ForEach-Object {
        $afterSaves[$_.FullName] = (Get-FileHash $_.FullName -Algorithm SHA256).Hash
    }
}
$savesUnchanged = ($beforeSaves.Count -eq $afterSaves.Count)
if ($savesUnchanged) {
    foreach ($k in $beforeSaves.Keys) {
        if ($afterSaves[$k] -ne $beforeSaves[$k]) { $savesUnchanged = $false; break }
    }
}

Write-Host "=== EXITS: Server=$($srvProc.ExitCode) c1=$($c1Proc.ExitCode) c2=$($c2Proc.ExitCode) c1_rejoin=$($c1rProc.ExitCode) OfflineSavesUnchanged=$savesUnchanged ==="
foreach ($f in @($srvOut, $c1Out, $c2Out, $c1RejoinOut)) {
    if (Test-Path $f) { Get-Content $f } else { Write-Host "MISSING $f" }
}

if ($srvProc.ExitCode -ne 0 -or $c1Proc.ExitCode -ne 0 -or $c2Proc.ExitCode -ne 0 -or $c1rProc.ExitCode -ne 0 -or -not $savesUnchanged) {
    exit 1
}
exit 0
