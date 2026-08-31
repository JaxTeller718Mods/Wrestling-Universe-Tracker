param(
    [Parameter(Mandatory=$true)][string]$SourceDatabase,
    [Parameter(Mandatory=$true)][string]$OutputDatabase
)

$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$source = (Resolve-Path -LiteralPath $SourceDatabase).Path
$output = [IO.Path]::GetFullPath($OutputDatabase)
[IO.File]::Copy($source, $output, $true)

$nativeDir = Join-Path $projectRoot 'Assets\Plugins\x86_64'
$env:PATH = $nativeDir + ';' + $env:PATH
Add-Type -Path (Join-Path $projectRoot 'Assets\Plugins\Mono.Data.Sqlite.dll')

$bytes = [IO.File]::ReadAllBytes($source)
$raw = [Text.Encoding]::GetEncoding(28591).GetString($bytes)
$universes = @(
    'c4b89a11cb544f949ad7ebce9646325a',
    '9ce89baeb1d345e1952fa769ca65fcf3',
    '38ece9dffa59441a95866b6f88cf71af'
)
$now = [DateTime]::UtcNow.ToString('O')
$connection = New-Object Mono.Data.Sqlite.SqliteConnection("URI=file:$output")
$connection.Open()
$transaction = $connection.BeginTransaction()

function Exec([string]$sql, [hashtable]$values) {
    $command = $connection.CreateCommand(); $command.Transaction = $transaction; $command.CommandText = $sql
    foreach ($entry in $values.GetEnumerator()) { $parameter = $command.CreateParameter(); $parameter.ParameterName = $entry.Key; $parameter.Value = if ($null -eq $entry.Value) {[DBNull]::Value} else {$entry.Value}; [void]$command.Parameters.Add($parameter) }
    [void]$command.ExecuteNonQuery(); $command.Dispose()
}

function TailAfter([string]$needle, [int]$maximum=1200) {
    $position = $raw.IndexOf($needle, [StringComparison]::Ordinal)
    if ($position -lt 0) { return $null }
    return $raw.Substring($position, [Math]::Min($maximum, $raw.Length - $position))
}

function IsoDates([string]$value) {
    return [regex]::Matches($value, '20\d\d-\d\d-\d\dT\d\d:\d\d:\d\d\.\d+Z') | ForEach-Object Value
}

$imageRoot = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) '..\LocalLow\DefaultCompany\WrestlingUniverseTracker\UniverseImages'
$imageRoot = [IO.Path]::GetFullPath($imageRoot)
$knownBrands = @('Unassigned','Smackdown','RAW','WWF')
$wrestlerIds = @{}

foreach ($uid in $universes) {
    $folder = Join-Path $imageRoot $uid
    if (!(Test-Path -LiteralPath $folder)) { continue }

    foreach ($file in Get-ChildItem -LiteralPath $folder -File -Filter 'brand_*.png') {
        $id = $file.BaseName.Substring(6); $tail = TailAfter ($id + $uid)
        if (!$tail) { continue }
        $pathMarker = $tail.IndexOf('C:/'); if ($pathMarker -lt 1) { continue }
        $name = $tail.Substring(64, $pathMarker - 64)
        $after = $tail.Substring($pathMarker); $dates = IsoDates $after
        $color = ([regex]::Match($after, '#[0-9A-Fa-f]{6}')).Value; if (!$color) {$color='#FFFFFF'}
        Exec 'INSERT OR IGNORE INTO brands(id,universe_id,name,image_path,color_hex,created_utc,updated_utc) VALUES(@id,@uid,@name,@path,@color,@created,@updated)' @{ '@id'=$id;'@uid'=$uid;'@name'=$name;'@path'=$file.FullName;'@color'=$color;'@created'=if($dates.Count){$dates[0]}else{$now};'@updated'=$now }
        if ($knownBrands -notcontains $name) { $knownBrands += $name }
    }

    foreach ($file in Get-ChildItem -LiteralPath $folder -File -Filter 'wrestler_*.png') {
        $id = $file.BaseName.Substring(9); $tail = TailAfter ($id + $uid)
        if (!$tail) { continue }
        $pathMarker = $tail.IndexOf('C:/'); if ($pathMarker -lt 1) { continue }
        $prefix = $tail.Substring(64, $pathMarker - 64)
        $match = [regex]::Match($prefix, '^(.*?)(Face|Heel|Neutral)(Male|Female|Neutral)(Lower Card|Mid-Card|Upper Card|Main Event)(.)$', 'Singleline')
        if (!$match.Success) { continue }
        $nameBrand=$match.Groups[1].Value; $brand='Unassigned'; $name=$nameBrand
        foreach($candidate in ($knownBrands | Sort-Object Length -Descending)) { if($nameBrand.EndsWith($candidate,[StringComparison]::Ordinal)){ $brand=$candidate; $name=$nameBrand.Substring(0,$nameBrand.Length-$candidate.Length); break } }
        $overall=[int][char]$match.Groups[5].Value[0]; if($overall -lt 1 -or $overall -gt 100){$overall=50}
        $dates=IsoDates $tail.Substring($pathMarker)
        Exec 'INSERT OR IGNORE INTO wrestlers(id,universe_id,name,brand,disposition,gender,tier,overall,photo_path,created_utc,updated_utc) VALUES(@id,@uid,@name,@brand,@disp,@gender,@tier,@overall,@path,@created,@updated)' @{ '@id'=$id;'@uid'=$uid;'@name'=$name;'@brand'=$brand;'@disp'=$match.Groups[2].Value;'@gender'=$match.Groups[3].Value;'@tier'=$match.Groups[4].Value;'@overall'=$overall;'@path'=$file.FullName;'@created'=if($dates.Count){$dates[0]}else{$now};'@updated'=$now }
        $wrestlerIds[$id]=$uid
    }

    foreach ($file in Get-ChildItem -LiteralPath $folder -File -Filter 'team_*.png') {
        $id=$file.BaseName.Substring(5); $tail=TailAfter ($id+$uid); if(!$tail){continue}; $pathMarker=$tail.IndexOf('C:/'); if($pathMarker -lt 1){continue}
        $prefix=$tail.Substring(64,$pathMarker-64); $dateAt=$prefix.IndexOf('20'); if($dateAt -gt 0){$prefix=$prefix.Substring(0,$dateAt)}
        $match=[regex]::Match($prefix,'^(.*?)(Unassigned|Smackdown|RAW|WWF)(Face|Heel|Neutral)$','Singleline'); if(!$match.Success){continue}
        $dates=IsoDates $tail.Substring($pathMarker)
        Exec 'INSERT OR IGNORE INTO teams(id,universe_id,name,brand,disposition,photo_path,created_utc,updated_utc) VALUES(@id,@uid,@name,@brand,@disp,@path,@created,@updated)' @{ '@id'=$id;'@uid'=$uid;'@name'=$match.Groups[1].Value;'@brand'=$match.Groups[2].Value;'@disp'=$match.Groups[3].Value;'@path'=$file.FullName;'@created'=if($dates.Count){$dates[0]}else{$now};'@updated'=$now }
    }

    foreach ($file in Get-ChildItem -LiteralPath $folder -File -Filter 'title_*.png') {
        $id=$file.BaseName.Substring(6); $tail=TailAfter ($id+$uid); if(!$tail){continue}; $pathMarker=$tail.IndexOf('C:/'); if($pathMarker -lt 1){continue}
        $prefix=$tail.Substring(64,$pathMarker-64); $holder=$null
        if($prefix -match '([0-9a-f]{32})$'){ $holder=$Matches[1]; $prefix=$prefix.Substring(0,$prefix.Length-32) }
        $brand='Unassigned'; $name=$prefix
        foreach($candidate in ($knownBrands | Sort-Object Length -Descending)){if($prefix.EndsWith($candidate,[StringComparison]::Ordinal)){$brand=$candidate;$name=$prefix.Substring(0,$prefix.Length-$candidate.Length);break}}
        $after=$tail.Substring($pathMarker); $dates=IsoDates $after; $division=([regex]::Match($after,"(Men's|Women's|Tag Team)")).Value; if(!$division){$division="Men's"}
        if($holder -and !$wrestlerIds.ContainsKey($holder)){$holder=$null}
        Exec 'INSERT OR IGNORE INTO titles(id,universe_id,name,brand,division,holder_wrestler_id,image_path,created_utc,updated_utc) VALUES(@id,@uid,@name,@brand,@division,@holder,@path,@created,@updated)' @{ '@id'=$id;'@uid'=$uid;'@name'=$name;'@brand'=$brand;'@division'=$division;'@holder'=$holder;'@path'=$file.FullName;'@created'=if($dates.Count){$dates[0]}else{$now};'@updated'=$now }
    }

    foreach ($file in Get-ChildItem -LiteralPath $folder -File -Filter 'tvshow_*.png') {
        $id=$file.BaseName.Substring(7); $tail=TailAfter ($id+$uid); if(!$tail){continue}; $pathMarker=$tail.IndexOf('C:/'); if($pathMarker -lt 1){continue}
        $prefix=$tail.Substring(64,$pathMarker-64); $match=[regex]::Match($prefix,'^(.*?)(Weekly|Bi-Weekly|Monthly|Special)(Sunday|Monday|Tuesday|Wednesday|Thursday|Friday|Saturday)$','Singleline'); if(!$match.Success){continue}; $dates=IsoDates $tail.Substring($pathMarker)
        Exec 'INSERT OR IGNORE INTO tv_shows(id,universe_id,name,frequency,day_of_week,image_path,created_utc,updated_utc) VALUES(@id,@uid,@name,@frequency,@day,@path,@created,@updated)' @{ '@id'=$id;'@uid'=$uid;'@name'=$match.Groups[1].Value;'@frequency'=$match.Groups[2].Value;'@day'=$match.Groups[3].Value;'@path'=$file.FullName;'@created'=if($dates.Count){$dates[0]}else{$now};'@updated'=$now }
    }
}

# WWE was the source of the surviving EWCF import, so identical image hashes provide
# an exact identity map for any WWE wrestler/team rows whose deleted record was fragmented.
$sourceUid='2de4aaa008fe4355965a2b8d22bf9c17'; $targetUid='c4b89a11cb544f949ad7ebce9646325a'
function HashMap([string]$uid,[string]$pattern){ $map=@{}; $folder=Join-Path $imageRoot $uid; if(Test-Path $folder){foreach($file in Get-ChildItem $folder -File -Filter $pattern){$map[(Get-FileHash $file.FullName -Algorithm SHA256).Hash]=$file}}; return $map }
$sourceWrestlerImages=HashMap $sourceUid 'wrestler_*.png'; $targetWrestlerImages=HashMap $targetUid 'wrestler_*.png'
$wrestlerMap=@{}
foreach($hash in $targetWrestlerImages.Keys){if(!$sourceWrestlerImages.ContainsKey($hash)){continue};$sourceId=$sourceWrestlerImages[$hash].BaseName.Substring(9);$targetId=$targetWrestlerImages[$hash].BaseName.Substring(9);$wrestlerMap[$sourceId]=$targetId
    Exec "INSERT OR IGNORE INTO wrestlers(id,universe_id,name,brand,disposition,gender,tier,overall,photo_path,created_utc,updated_utc) SELECT @target,@targetUid,name,'Unassigned',disposition,gender,tier,overall,@path,created_utc,@now FROM wrestlers WHERE id=@source" @{ '@target'=$targetId;'@targetUid'=$targetUid;'@path'=$targetWrestlerImages[$hash].FullName;'@now'=$now;'@source'=$sourceId }
    $wrestlerIds[$targetId]=$targetUid
}
$sourceTeamImages=HashMap $sourceUid 'team_*.png'; $targetTeamImages=HashMap $targetUid 'team_*.png'; $teamMap=@{}
foreach($hash in $targetTeamImages.Keys){if(!$sourceTeamImages.ContainsKey($hash)){continue};$sourceId=$sourceTeamImages[$hash].BaseName.Substring(5);$targetId=$targetTeamImages[$hash].BaseName.Substring(5);$teamMap[$sourceId]=$targetId
    Exec "INSERT OR IGNORE INTO teams(id,universe_id,name,brand,disposition,photo_path,created_utc,updated_utc) SELECT @target,@targetUid,name,'Unassigned',disposition,@path,created_utc,@now FROM teams WHERE id=@source" @{ '@target'=$targetId;'@targetUid'=$targetUid;'@path'=$targetTeamImages[$hash].FullName;'@now'=$now;'@source'=$sourceId }
}
foreach($sourceTeam in $teamMap.Keys){$cmd=$connection.CreateCommand();$cmd.Transaction=$transaction;$cmd.CommandText='SELECT wrestler_id,position FROM team_members WHERE team_id=@id';$p=$cmd.CreateParameter();$p.ParameterName='@id';$p.Value=$sourceTeam;[void]$cmd.Parameters.Add($p);$reader=$cmd.ExecuteReader();while($reader.Read()){if($wrestlerMap.ContainsKey($reader.GetString(0))){Exec 'INSERT OR IGNORE INTO team_members(team_id,wrestler_id,position) VALUES(@team,@wrestler,@position)' @{ '@team'=$teamMap[$sourceTeam];'@wrestler'=$wrestlerMap[$reader.GetString(0)];'@position'=$reader.GetInt32(1) }}};$reader.Close();$cmd.Dispose()}

# Recover team membership rows by locating surviving adjacent team/wrestler IDs in deleted pages.
$teamCommand=$connection.CreateCommand(); $teamCommand.Transaction=$transaction; $teamCommand.CommandText='SELECT id,universe_id FROM teams'; $reader=$teamCommand.ExecuteReader(); $teams=@(); while($reader.Read()){$teams += ,@($reader.GetString(0),$reader.GetString(1))}; $reader.Close(); $teamCommand.Dispose()
foreach($team in $teams){ $position=0; foreach($wid in $wrestlerIds.Keys){ if($wrestlerIds[$wid] -ne $team[1]){continue}; if($raw.Contains($team[0]+$wid) -or $raw.Contains($wid+$team[0])){ Exec 'INSERT OR IGNORE INTO team_members(team_id,wrestler_id,position) VALUES(@team,@wrestler,@position)' @{ '@team'=$team[0];'@wrestler'=$wid;'@position'=$position }; $position++ } } }

$transaction.Commit(); $connection.Close()

$verify=New-Object Mono.Data.Sqlite.SqliteConnection("URI=file:$output"); $verify.Open()
foreach($uid in $universes){ $parts=@(); foreach($table in @('wrestlers','teams','brands','titles','tv_shows','specials')){ $cmd=$verify.CreateCommand();$cmd.CommandText="SELECT COUNT(*) FROM $table WHERE universe_id=@uid";$p=$cmd.CreateParameter();$p.ParameterName='@uid';$p.Value=$uid;[void]$cmd.Parameters.Add($p);$parts += "$table=$($cmd.ExecuteScalar())";$cmd.Dispose() }; Write-Output "$uid $($parts -join ' ')" }
$check=$verify.CreateCommand();$check.CommandText='PRAGMA foreign_key_check';$bad=$check.ExecuteReader();$violations=0;while($bad.Read()){$violations++};$bad.Close();$check.Dispose();$verify.Close();Write-Output "foreign_key_violations=$violations"
