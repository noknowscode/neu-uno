#Requires -Version 7.0
<#
.SYNOPSIS
    Reports WCAG contrast ratios for the Neu color token pairs.

.DESCRIPTION
    STUB (step 1). Locates and parses Tokens.Colors.xaml and reports which
    tokens it found. The ratio maths and the pass/fail gate land in step 2,
    once the tokens exist.

    Contrast floors from docs/SPEC.md, enforced once implemented:
      NeuInk on NeuSurface           >= 7:1  (light)   >= 10:1 (dark)
      NeuInkSecondary on NeuSurface  >= 4.5:1 (both themes)
      NeuAccent is a fill under ink text, never text itself.

.PARAMETER TokensPath
    Path to Tokens.Colors.xaml. Defaults to the library location.

.EXAMPLE
    pwsh tools/contrast-check.ps1
#>
[CmdletBinding()]
param(
    [string] $TokensPath = (Join-Path $PSScriptRoot '..' 'src' 'Neu.Uno' 'Theme' 'Tokens.Colors.xaml')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Test-Path $TokensPath)) {
    Write-Host "contrast-check: Tokens.Colors.xaml not found at" -NoNewline
    Write-Host " $TokensPath" -ForegroundColor Yellow
    Write-Host "Color tokens land in step 2. Nothing to check yet."
    exit 0
}

$resolved = (Resolve-Path $TokensPath).Path
Write-Host "contrast-check: reading $resolved"

[xml] $doc = Get-Content -Path $resolved -Raw

# Collect every <Color x:Key="Neu*"> across all ThemeDictionaries.
$ns = @{ x = 'http://schemas.microsoft.com/winfx/2006/xaml' }
$colors = $doc.SelectNodes('//*[local-name()="Color"]') | ForEach-Object {
    [pscustomobject]@{
        Key   = $_.GetAttribute('Key', $ns.x)
        Value = $_.InnerText.Trim()
    }
} | Where-Object { $_.Key -like 'Neu*' }

if (-not $colors) {
    Write-Host "No Neu* Color tokens declared yet." -ForegroundColor Yellow
    exit 0
}

Write-Host "Found $($colors.Count) Neu* color token(s):"
$colors | Format-Table -AutoSize

Write-Warning 'Contrast ratio calculation is not implemented until step 2.'
exit 0
