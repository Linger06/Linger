<#!
.SYNOPSIS
  按指定规则为受影响项目集合进行语义化版本提升（major/minor/patch），写回 .csproj 的 <Version>。

.PARAMETER AffectedJson
  compute-affected.ps1 的输出 JSON 文件路径。

.PARAMETER Level
  提升级别：major | minor | patch（默认 patch）。

.PARAMETER Commit
  是否自动创建一次提交（默认 false）。

.OUTPUTS
  将新版本打印到日志，并把变更过的项目写入 artifacts/bumped.json（[{Name,CsprojPath,OldVersion,NewVersion}]）。
#>

param(
  [Parameter(Mandatory=$true)]
  [string]$AffectedJson,
  [ValidateSet('major','minor','patch')]
  [string]$Level = 'patch',
  [switch]$Commit
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-XmlProject([string]$csprojPath) {
  [xml]$xml = Get-Content -Path $csprojPath -Raw
  return $xml
}

function Get-OrCreate-PropertyGroup($xml) {
  $pg = $xml.Project.PropertyGroup | Select-Object -First 1
  if (-not $pg) {
    $pg = $xml.CreateElement('PropertyGroup')
    [void]$xml.Project.AppendChild($pg)
  }
  return $pg
}

function Get-ProjectVersion([xml]$xml) {
  $v = $xml.Project.PropertyGroup.Version | Select-Object -First 1
  if ($v -and $v.'#text') { return $v.'#text' }
  return '0.0.0'
}

function Set-ProjectVersion([xml]$xml, [string]$newVersion) {
  $pg = Get-OrCreate-PropertyGroup $xml
  $v = $xml.Project.PropertyGroup.Version | Select-Object -First 1
  if (-not $v) {
    $v = $xml.CreateElement('Version')
    [void]$pg.AppendChild($v)
  }
  $v.InnerText = $newVersion
}

function Update-Version([string]$ver, [string]$level) {
  # 解析 x.y.z[-pre]
  $main,$pre = $ver -split '-',2
  $parts = $main -split '\\.'
  while ($parts.Count -lt 3) { $parts += '0' }
  [int]$maj = $parts[0]; [int]$min = $parts[1]; [int]$pat = $parts[2]
  switch ($level) {
    'major' { $maj++; $min=0; $pat=0 }
    'minor' { $min++; $pat=0 }
    default { $pat++ }
  }
  return "$maj.$min.$pat"
}

if (-not (Test-Path $AffectedJson)) { throw "Affected JSON not found: $AffectedJson" }
$affected = Get-Content -Path $AffectedJson -Raw | ConvertFrom-Json
if (-not $affected -or $affected.Count -eq 0) {
  Write-Host "No affected projects, nothing to bump."
  exit 0
}

$bumped = @()
foreach ($p in $affected) {
  $csproj = $p.CsprojPath
  if (-not (Test-Path $csproj)) { continue }
  $xml = Get-XmlProject $csproj
  $old = Get-ProjectVersion $xml
  $new = Update-Version $old $Level
  if ($new -ne $old) {
    Set-ProjectVersion $xml $new
    $xml.Save($csproj)
    Write-Host "Bumped $($p.Name): $old -> $new"
    $bumped += [pscustomobject]@{ Name=$p.Name; CsprojPath=$csproj; OldVersion=$old; NewVersion=$new }
    if ($Commit.IsPresent) {
      git add -- $csproj | Out-Null
    }
  }
}

if ($Commit.IsPresent -and $bumped.Count -gt 0) {
  git commit -m "chore(release): bump versions ($Level) for affected projects" | Out-Null
}

if ($bumped.Count -gt 0) {
  New-Item -ItemType Directory -Force -Path 'artifacts' | Out-Null
  $bumped | ConvertTo-Json -Depth 5 | Out-File -FilePath 'artifacts/bumped.json' -Encoding utf8
}
