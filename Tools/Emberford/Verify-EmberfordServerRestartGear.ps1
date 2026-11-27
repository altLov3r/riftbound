param(
    [string]$ServerExe = "Builds/EmberfordFrontierServer12/EmberfordServer.exe",
    [string]$ClientExe = "Builds/EmberfordFrontierClient12/Riftbound.exe",
    [uint16]$Port = 7808,
    [int]$TimeoutSeconds = 75
)

$ErrorActionPreference = "Stop"
$root = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$srvFull = Join-Path $root $ServerExe
$cliFull = Join-Path $root $ClientExe
if (-not (Test-Path -LiteralPath $srvFull)) { throw "Missing Dedicated Server executable: $srvFull" }
if (-not (Test-Path -LiteralPath $cliFull)) { throw "Missing Client executable: $cliFull" }

$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$runDir = Join-Path $root ".code-check/emberford-r1/restart-gear-$stamp"
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

function Start-GearServer([string]$tag) {
    $srvOut = Join-Path $runDir "$tag-result.txt"
    $srvLog = Join-Path $runDir "$tag-unity.log"
    $realmLog = Join-Path $runDir "$tag-realm.log"
    $srvArgs = "-batchmode -nographics -dedicatedServer -realmId emberford -realmName Emberford -realmListen 127.0.0.1 -realmPort $Port -realmMaxPlayers 4 -realmDataDir `"$dataDir`" -realmLog `"$realmLog`" -realmSmokeRole gear_server -realmSmokeOut `"$srvOut`" -realmSmokeSeconds $TimeoutSeconds -logFile `"$srvLog`""
    $proc = Start-Process -FilePath $srvFull -ArgumentList $srvArgs -WorkingDirectory $root -WindowStyle Hidden -PassThru
    $readyLimit = (Get-Date).AddSeconds(30)
    while ((Get-Date) -lt $readyLimit -and -not $proc.HasExited) {
        if ((Test-Path $realmLog) -and ((Get-Content $realmLog -Raw -ErrorAction SilentlyContinue) -match "StartServer succeeded")) { break }
        Start-Sleep -Milliseconds 300
    }
    if ($proc.HasExited) { throw "$tag exited early with code $($proc.ExitCode)" }
    return @{ Proc = $proc; Out = $srvOut }
}

# Phase 1: Server #1 + Client #1 (gear_seed: loot village-cache -> equip leather-vest -> disconnect)
$s1 = Start-GearServer "server1"
Write-Host "Started Server #1 PID=$($s1.Proc.Id) on 127.0.0.1:$Port"
$seedOut = Join-Path $runDir "gear-seed-result.txt"
$seedArgs = "-batchmode -nographics -realmClient -realmEndpoint 127.0.0.1 -realmPort $Port -realmCharName GearHero -realmRace Human -realmClass Warrior -realmSmokeRole gear_seed -realmSmokeOut `"$seedOut`" -realmSmokeSeconds 45 -logFile `"$runDir/gear-seed-unity.log`""
$c1 = Start-Process -FilePath $cliFull -ArgumentList $seedArgs -WorkingDirectory $root -WindowStyle Hidden -PassThru
$deadline1 = (Get-Date).AddSeconds(55)
while ((Get-Date) -lt $deadline1 -and (-not $c1.HasExited -or -not $s1.Proc.HasExited)) { Start-Sleep -Milliseconds 250 }
foreach ($p in @($c1, $s1.Proc)) {
    if (-not $p.HasExited) { Stop-Process -Id $p.Id -Force; throw "Phase 1 PID=$($p.Id) timed out" }
    $p.WaitForExit()
}
Write-Host "Phase 1 complete: Server1=$($s1.Proc.ExitCode) SeedClient=$($c1.ExitCode)"

# Phase 2: Fresh Server #2 against SAME realm-data dir + Fresh Client #2 (gear_verify: confirm hydrated leather-vest + dup chest blocked)
Start-Sleep -Milliseconds 800
$s2 = Start-GearServer "server2"
Write-Host "Started Server #2 PID=$($s2.Proc.Id) on 127.0.0.1:$Port (same realmDataDir)"
$verifyOut = Join-Path $runDir "gear-verify-result.txt"
$verifyArgs = "-batchmode -nographics -realmClient -realmEndpoint 127.0.0.1 -realmPort $Port -realmCharName GearHero -realmRace Human -realmClass Warrior -realmSmokeRole gear_verify -realmSmokeOut `"$verifyOut`" -realmSmokeSeconds 45 -logFile `"$runDir/gear-verify-unity.log`""
$c2 = Start-Process -FilePath $cliFull -ArgumentList $verifyArgs -WorkingDirectory $root -WindowStyle Hidden -PassThru
$deadline2 = (Get-Date).AddSeconds(55)
while ((Get-Date) -lt $deadline2 -and (-not $c2.HasExited -or -not $s2.Proc.HasExited)) { Start-Sleep -Milliseconds 250 }
foreach ($p in @($c2, $s2.Proc)) {
    if (-not $p.HasExited) { Stop-Process -Id $p.Id -Force; throw "Phase 2 PID=$($p.Id) timed out" }
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

Write-Host "=== RESTART GEAR EXITS: Server1=$($s1.Proc.ExitCode) Seed=$($c1.ExitCode) Server2=$($s2.Proc.ExitCode) Verify=$($c2.ExitCode) OfflineSavesUnchanged=$savesUnchanged ==="
foreach ($f in @($s1.Out, $seedOut, $s2.Out, $verifyOut)) {
    if (Test-Path $f) { Get-Content $f } else { Write-Host "MISSING $f" }
}

if ($s1.Proc.ExitCode -ne 0 -or $c1.ExitCode -ne 0 -or $s2.Proc.ExitCode -ne 0 -or $c2.ExitCode -ne 0 -or -not $savesUnchanged) {
    exit 1
}
exit 0
