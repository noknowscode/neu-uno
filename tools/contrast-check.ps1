#Requires -Version 7.0
<#
.SYNOPSIS
  Reports the WCAG 2.x contrast ratio of every Neu ink/surface pair, per theme,
  against the floors in CLAUDE.md rule 12.

.DESCRIPTION
  Reads the color tokens straight out of src/Neu.Uno/Theme/Tokens.Colors.xaml so
  the report cannot drift from the tokens. Run it after any token change.

  Enforced floors (CLAUDE.md rule 12 and the Design Brief in docs/SPEC.md):
    NeuInk           on NeuSurface   >= 7:1 light, >= 10:1 dark
    NeuInkSecondary  on NeuSurface   >= 4.5:1
    NeuAlert         on NeuSurface   >= 4.5:1   (error message ink)
    NeuOnAccent      on NeuAccent    >= 4.5:1   (ink on the amber fill)
    NeuOnAlert       on NeuAlert     >= 4.5:1

  Every other ink/surface combination is reported without a floor, so a template
  author can see what a pairing would cost before using it.

.PARAMETER TokensPath
  Path to Tokens.Colors.xaml. Defaults to the file in this repo.

.PARAMETER Theme
  Limit the report to one theme dictionary (Light or Dark).

.OUTPUTS
  A table per theme. Exit code 0 if every enforced floor is met, 1 otherwise.
#>
[CmdletBinding()]
param(
    [string] $TokensPath = (Join-Path $PSScriptRoot '..' 'src' 'Neu.Uno' 'Theme' 'Tokens.Colors.xaml'),
    [ValidateSet('Light', 'Dark')]
    [string] $Theme
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# --- WCAG 2.x relative luminance and contrast ---------------------------------

function Get-Channel {
    param([double] $Srgb)
    if ($Srgb -le 0.03928) { return $Srgb / 12.92 }
    return [Math]::Pow((($Srgb + 0.055) / 1.055), 2.4)
}

function Get-RelativeLuminance {
    param([string] $Hex)
    $h = $Hex.TrimStart('#')
    if ($h.Length -eq 8) { $h = $h.Substring(2) }   # drop AA from #AARRGGBB
    if ($h.Length -ne 6) { throw "Unsupported color literal '$Hex'." }

    $r = [Convert]::ToInt32($h.Substring(0, 2), 16) / 255.0
    $g = [Convert]::ToInt32($h.Substring(2, 2), 16) / 255.0
    $b = [Convert]::ToInt32($h.Substring(4, 2), 16) / 255.0

    return 0.2126 * (Get-Channel $r) + 0.7152 * (Get-Channel $g) + 0.0722 * (Get-Channel $b)
}

function Get-ContrastRatio {
    param([string] $Foreground, [string] $Background)
    $l1 = Get-RelativeLuminance $Foreground
    $l2 = Get-RelativeLuminance $Background
    if ($l2 -gt $l1) { $t = $l1; $l1 = $l2; $l2 = $t }
    return ($l1 + 0.05) / ($l2 + 0.05)
}

# --- Token extraction ---------------------------------------------------------

if (-not (Test-Path $TokensPath)) { throw "Tokens file not found: $TokensPath" }

[xml] $doc = Get-Content -Raw -Path $TokensPath
$xKey = 'http://schemas.microsoft.com/winfx/2006/xaml'

$themes = [ordered] @{}
foreach ($node in $doc.DocumentElement.ChildNodes) {
    if ($node.LocalName -ne 'ResourceDictionary.ThemeDictionaries') { continue }
    foreach ($dict in $node.ChildNodes) {
        if ($dict.LocalName -ne 'ResourceDictionary') { continue }
        $name = $dict.GetAttribute('Key', $xKey)
        $colors = [ordered] @{}
        foreach ($entry in $dict.ChildNodes) {
            if ($entry.LocalName -ne 'Color') { continue }
            $colors[$entry.GetAttribute('Key', $xKey)] = $entry.InnerText.Trim()
        }
        $themes[$name] = $colors
    }
}

# "Default" mirrors "Light" as a HighContrast fallback; reporting it twice is noise.
$reported = @($themes.Keys | Where-Object { $_ -ne 'Default' })
if ($Theme) { $reported = @($reported | Where-Object { $_ -eq $Theme }) }
if ($reported.Count -eq 0) { throw "No theme dictionaries found in $TokensPath." }

# --- Pairs and floors ---------------------------------------------------------

$inks = 'NeuInk', 'NeuInkSecondary', 'NeuOnAccent', 'NeuOnAlert', 'NeuAccent', 'NeuAlert'
$surfaces = 'NeuSurface', 'NeuAccent', 'NeuAlert'

$floors = @{
    'Light|NeuInk|NeuSurface' = 7.0
    'Dark|NeuInk|NeuSurface' = 10.0
    'Light|NeuInkSecondary|NeuSurface' = 4.5
    'Dark|NeuInkSecondary|NeuSurface' = 4.5
    'Light|NeuAlert|NeuSurface' = 4.5
    'Dark|NeuAlert|NeuSurface' = 4.5
    'Light|NeuOnAccent|NeuAccent' = 4.5
    'Dark|NeuOnAccent|NeuAccent' = 4.5
    'Light|NeuOnAlert|NeuAlert' = 4.5
    'Dark|NeuOnAlert|NeuAlert' = 4.5
}

# --- Report -------------------------------------------------------------------

$failures = @()
$enforced = 0

foreach ($themeName in $reported) {
    $colors = $themes[$themeName]

    Write-Host ''
    Write-Host "$themeName" -ForegroundColor Cyan
    Write-Host ('{0,-16} {1,-9} {2,-12} {3,-9} {4,7}  {5,-6} {6}' -f `
        'Ink', 'Hex', 'On surface', 'Hex', 'Ratio', 'Floor', 'Result')
    Write-Host ('-' * 78)

    foreach ($bg in $surfaces) {
        foreach ($fg in $inks) {
            if ($fg -eq $bg) { continue }
            if (-not $colors.Contains($fg) -or -not $colors.Contains($bg)) { continue }

            $fgHex = $colors[$fg]
            $bgHex = $colors[$bg]
            $ratio = Get-ContrastRatio $fgHex $bgHex

            $key = "$themeName|$fg|$bg"
            $floor = if ($floors.ContainsKey($key)) { $floors[$key] } else { $null }

            if ($null -eq $floor) {
                $floorText = '-'
                $result = 'info'
                $color = 'DarkGray'
            }
            elseif ($ratio -ge $floor) {
                $floorText = ('{0:0.0}' -f $floor)
                $result = 'PASS'
                $color = 'Green'
                $enforced++
            }
            else {
                $floorText = ('{0:0.0}' -f $floor)
                $result = 'FAIL'
                $color = 'Red'
                $enforced++
                $failures += [pscustomobject] @{
                    Theme = $themeName; Ink = $fg; Surface = $bg
                    Ratio = $ratio; Floor = $floor
                }
            }

            Write-Host ('{0,-16} {1,-9} {2,-12} {3,-9} {4,6:0.00}:1  {5,-6} {6}' -f `
                $fg, $fgHex, $bg, $bgHex, $ratio, $floorText, $result) -ForegroundColor $color
        }
    }
}

Write-Host ''

# A token file that restates only a few keys builds no pairs at all, and without
# this it would print an empty table and report a pass. Silence is not a pass.
if ($enforced -eq 0) {
    Write-Host 'No enforced pair could be measured - nothing was checked.' -ForegroundColor Red
    Write-Host "  $TokensPath defines no theme dictionary holding both halves of a floored pair." -ForegroundColor Red
    Write-Host '  An override file has to restate the full ink and surface set to be checkable;' -ForegroundColor Red
    Write-Host '  the simplest way is to copy Tokens.Colors.xaml and edit it.' -ForegroundColor Red
    exit 1
}

if ($failures.Count -eq 0) {
    Write-Host "All enforced contrast floors met ($enforced pair(s) checked)." -ForegroundColor Green
    exit 0
}

Write-Host "$($failures.Count) pair(s) below their floor:" -ForegroundColor Red
foreach ($f in $failures) {
    Write-Host ("  {0,-5} {1} on {2}: {3:0.00}:1, needs {4:0.0}:1" -f `
        $f.Theme, $f.Ink, $f.Surface, $f.Ratio, $f.Floor) -ForegroundColor Red
}
exit 1
