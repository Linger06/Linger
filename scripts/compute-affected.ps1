<#!
.SYNOPSIS
  计算本次改动影响到的可打包项目列表（含反向依赖闭包），输出 JSON。

.DESCRIPTION
  <#!
  .SYNOPSIS
    使用 NuGet Restore 图（DGSpec）解析依赖，计算本次改动影响到的可打包项目列表，输出 JSON。

  .DESCRIPTION
    - 基于 git diff 定位 src/** 下受影响的 .csproj（直接受影响）。
    - 通过 dotnet msbuild -t:GenerateRestoreGraphFile 生成 DGSpec（JSON），从中读取项目依赖，构建反向依赖图。
    - 可选包含反向依赖闭包（-IncludeReverse）：把“依赖改动项目”的上游项目也纳入受影响集合。
    - 过滤仅保留 src/** 下 IsPackable != false 且非测试项目（IsTestProject != true）。
    - 以 JSON 数组输出：[{ Name, CsprojPath, Directory }]

  .PARAMETER Base
    比较的基线（git ref），默认为 origin/develop；如果不存在则使用 HEAD~1。

  .PARAMETER Head
    比较的目标（git ref），默认为 HEAD。

  .PARAMETER IncludeReverse
    是否包含反向依赖闭包（上游依赖）。默认不包含。

  .OUTPUTS
    JSON 到标准输出。

  .EXAMPLE
    pwsh ./scripts/compute-affected.ps1 -Base origin/main -Head HEAD -IncludeReverse > artifacts/affected.json
  #>

  param(
    [string]$Base = "origin/develop",
    [string]$Head = "HEAD",
    [switch]$IncludeReverse
  )

  Set-StrictMode -Version Latest
  $ErrorActionPreference = 'Stop'

  function Get-AllCsprojInSrc {
    $root = (Get-Location).Path
    Get-ChildItem -Path (Join-Path $root 'src') -Recurse -Filter '*.csproj' -File | ForEach-Object { $_.FullName }
  }

  function Get-NearestCsprojInSrc([string]$filePath, [string[]]$allCsprojs) {
    $full = [System.IO.Path]::GetFullPath($filePath)
    $dir = Split-Path -Path $full -Parent
    while ($dir -and (Split-Path -Path $dir -Parent) -ne $dir) {
      $candidates = $allCsprojs | Where-Object { (Split-Path -Path $_ -Parent) -eq $dir }
      if ($candidates) { return ($candidates | Select-Object -First 1) }
      $dir = Split-Path -Path $dir -Parent
    }
    return $null
  }

  function Get-IsPackable([string]$csprojPath) {
    try { [xml]$xml = Get-Content -Path $csprojPath -Raw } catch { return $true }
    $isTest = $xml.Project.PropertyGroup.IsTestProject | Select-Object -First 1
    if ($isTest -and $isTest.'#text' -and [System.String]::Compare($isTest.'#text','true',$true) -eq 0) { return $false }
    $val = $xml.Project.PropertyGroup.IsPackable | Select-Object -First 1
    if ($null -eq $val -or [string]::IsNullOrWhiteSpace($val.'#text')) { return $true }
    return [System.String]::Compare($val.'#text','false',$true) -ne 0
  }

  function Get-SolutionPath {
    $root = (Get-Location).Path
    $slnx = Get-ChildItem -Path $root -Filter '*.slnx' -File | Select-Object -First 1
    if ($slnx) { return $slnx.FullName }
    $sln = Get-ChildItem -Path $root -Filter '*.sln' -File | Select-Object -First 1
    if ($sln) { return $sln.FullName }
    throw 'Solution file (*.slnx or *.sln) not found in repository root.'
  }

  function New-DgSpec([string]$solutionPath, [string]$outputPath) {
    $outDir = Split-Path -Path $outputPath -Parent
    if ($outDir) { New-Item -ItemType Directory -Force -Path $outDir | Out-Null }
    $psi = @(
      'msbuild',
      '-t:GenerateRestoreGraphFile',
      "-p:RestoreGraphOutputPath=$outputPath",
      $solutionPath
    )
    dotnet @psi | Write-Host
    if (-not (Test-Path $outputPath)) { throw "DGSpec not generated at $outputPath" }
  }

  function Get-GraphsFromDg([string]$dgPath) {
    $json = Get-Content -Path $dgPath -Raw | ConvertFrom-Json
    $deps = @{}
    $rev = @{}
    if (-not $json.projects) { return @{ deps = $deps; rev = $rev } }
    
    function Get-Prop($obj, [string]$name) {
      if ($null -eq $obj) { return $null }
      $p = $obj.PSObject.Properties[$name]
      if ($p) { return $p.Value }
      return $null
    }

    foreach ($entry in $json.projects.PSObject.Properties) {
      $projNode = $entry.Value
      $projPath = Get-Prop $projNode 'projectPath'
      if (-not $projPath) { $projPath = $entry.Name }
      if (-not $projPath) { continue }
      $projFull = [System.IO.Path]::GetFullPath($projPath)
      if (-not $deps.ContainsKey($projFull)) { $deps[$projFull] = @() }
      $refs = @()

      $projRefsNode = Get-Prop $projNode 'projectReferences'
      if ($projRefsNode) {
        # Case 1: projectReferences is a dictionary/object: keys are referenced project paths
        if ($projRefsNode -is [System.Collections.IDictionary]) {
          foreach ($rp in $projRefsNode.PSObject.Properties) {
            $rPath = $rp.Name
            if ([string]::IsNullOrWhiteSpace($rPath)) { continue }
            $refs += [System.IO.Path]::GetFullPath($rPath)
          }
        }
        # Case 2: projectReferences is an array of objects
        elseif ($projRefsNode -is [System.Array]) {
          foreach ($r in $projRefsNode) {
            $rPath = Get-Prop $r 'projectPath'
            if (-not $rPath) { $rPath = Get-Prop $r 'projectUniqueName' }
            if (-not [string]::IsNullOrWhiteSpace($rPath)) {
              $refs += [System.IO.Path]::GetFullPath($rPath)
            }
          }
        }
        # Case 3: PSCustomObject but not IDictionary (treat properties as keys)
        else {
          foreach ($rp in $projRefsNode.PSObject.Properties) {
            $rPath = $rp.Name
            if (-not [string]::IsNullOrWhiteSpace($rPath)) {
              $refs += [System.IO.Path]::GetFullPath($rPath)
            }
          }
        }
      }

      $deps[$projFull] = $refs
      foreach ($r in $refs) {
        if (-not $rev.ContainsKey($r)) { $rev[$r] = @() }
        $rev[$r] += $projFull
      }
    }
    return @{ deps = $deps; rev = $rev }
  }

  function Get-ReverseClosure([string[]]$seeds, $revGraph) {
    $visited = New-Object System.Collections.Generic.HashSet[string]
    $queue = New-Object System.Collections.Generic.Queue[string]
    foreach ($s in $seeds) { if ($visited.Add($s)) { $queue.Enqueue($s) } }
    while ($queue.Count -gt 0) {
      $cur = $queue.Dequeue()
      if ($revGraph.ContainsKey($cur)) {
        foreach ($pred in $revGraph[$cur]) {
          if ($visited.Add($pred)) { $queue.Enqueue($pred) }
        }
      }
    }
  return @($visited)
  }

  # 1) 计算 diff 文件
  $baseRef = $Base
  try { git rev-parse --verify $baseRef *> $null } catch { $baseRef = 'HEAD~1' }
  $diffFiles = (git diff --name-only $baseRef $Head) -split "`n" | Where-Object { $_ -and $_.Trim() -ne '' }

  # If no diffs found, fail fast per policy (no fallback)
  if (-not $diffFiles -or $diffFiles.Count -eq 0) {
    Write-Host "Changed files considered (final):"
    Write-Host "  <none>"
    throw "No changes detected between $baseRef and $Head. Aborting as requested."
  }

    # Print diff files for diagnostics (Write-Host won't pollute the JSON pipeline)
    Write-Host "Changed files considered (final):"
    foreach ($f in $diffFiles) { Write-Host "  $f" }

  # 2) 列出 src 下所有 csproj，映射改动文件到直接受影响 csproj
  $allCsprojs = Get-AllCsprojInSrc
  $directAffected = New-Object System.Collections.Generic.HashSet[string]
  foreach ($f in $diffFiles) {
    if ($f -notmatch '^src[\\/]') { continue }
    $cs = Get-NearestCsprojInSrc -filePath $f -allCsprojs $allCsprojs
    if ($cs) { $directAffected.Add([System.IO.Path]::GetFullPath($cs)) | Out-Null }
  }

  if ($directAffected.Count -eq 0) { '[]' | Write-Output; exit 0 }

  # 3) 生成 DGSpec 并读取依赖图
  $solution = Get-SolutionPath
  $dgOut = Join-Path (Join-Path (Get-Location) 'artifacts') 'restore.dg.json'
  New-DgSpec -solutionPath $solution -outputPath $dgOut
  $graphs = Get-GraphsFromDg -dgPath $dgOut

  # 4) 计算候选集合
  $candidates = @($directAffected)
  if ($IncludeReverse.IsPresent) {
    $candidates = Get-ReverseClosure -seeds $candidates -revGraph $graphs.rev
  }

  # 5) 仅保留可打包项目（IsPackable != false & 非测试）
  $packable = @()
  foreach ($p in $candidates) { if (Get-IsPackable $p) { $packable += $p } }

  # 6) 输出 JSON
  $result = @()
  foreach ($p in ($packable | Sort-Object -Unique)) {
    $name = [System.IO.Path]::GetFileNameWithoutExtension($p)
    $dir = Split-Path -Path $p -Parent
    $result += [pscustomobject]@{ Name = $name; CsprojPath = $p; Directory = $dir }
  }
  $result | ConvertTo-Json -Depth 5 | Write-Output
