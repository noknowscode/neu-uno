# Neu.Uno — Neumorphism Theme Library for Uno Platform

Status: spec locked 2026-09-02. Decisions: ShadowContainer as sole depth mechanism, package `Neu.Uno`, Skia-only heads.

## Scope

A NuGet-able theme library that gives stock WinUI/Uno controls a neumorphic visual identity through tokens, a depth scale, and retemplated controls, plus a gallery app that doubles as the runtime verification surface.

Out of scope for v1: WinAppSDK (Windows composition) head, custom-shaped silhouettes (text engraving, alpha images), custom controls beyond three small depth primitives, shipped fonts.

## Feasibility (validated)

- `Uno.Toolkit.Skia.WinUI` ships `ShadowContainer`: layered inner and outer shadows, any number of `Shadow` objects (Color, OffsetX/Y, Opacity, BlurRadius, Spread, IsInner). Skia draws them, so all Skia heads render identically.
- Uno Gallery already contains neumorphic styles for most controls; Uno's Jan 2026 ConfPass sample is built the same way. The approach is proven.
- Constraint 1: ShadowContainer mimics its content by size and corner radius only. Everything is a rounded rect or pill.
- Constraint 2: for inner shadows to paint, the child has no Background/Fill; the surface brush goes on the ShadowContainer itself.
- Constraint 3: shadow order matters (later overlaps earlier). Depth is a fixed set of named `ShadowCollection` resources; dark first, light second.
- Constraint 4: blur has a Skia cost. Lists of blurred containers are the perf cliff; tested in step 8.
- Toolkit issue #697 (VSM shadow swap not applying) was Windows-composition-specific. With Skia-only heads it is expected to be moot; step 4 confirms on desktop.

## Architecture Brief

### Module structure

```
src/
  Neu.Uno/                         # class library, net10.0 multi-target
    Theme/
      NeuTheme.cs                  # markup entry; mirrors Uno Themes MaterialTheme pattern
      Tokens.Colors.xaml           # ThemeDictionaries: Light/Dark color tokens
      Tokens.Metrics.xaml          # x:Double distance/blur/radius/spacing
      Tokens.Shadows.xaml          # ShadowCollections per theme + depth
      Tokens.Type.xaml             # type ramp (sizes/weights only)
    Styles/
      Neu.Button.xaml, Neu.ToggleButton.xaml, ...   # one file per control
      Neu.Implicit.xaml            # optional implicit-style dictionary
    Primitives/
      NeuSurface.cs                # ContentControl wrapper exposing Depth
      NeuGroove.xaml               # 1px inset divider
      NeuWell.xaml                 # inset container
    Helpers/
      NeuDepth.cs                  # attached property fallback (only if step 4 fails)
    Themes/Generic.xaml
samples/
  Neu.Gallery/                     # heads: desktop, browserwasm, android, ios
tests/
  Neu.Gallery.UITests/             # screenshot diffs, small
tools/
  contrast-check.ps1               # token pair contrast report
```

### Targets

`net10.0-desktop`, `net10.0-browserwasm`, `net10.0-android`, `net10.0-ios`. Windows users run desktop (Skia).

### State model

Stateless library. `NeuTheme` init options: `ColorOverrideSource`, `UseImplicitStyles`. Gallery is plain code-behind; no MVUX.

### Navigation model (gallery)

Single flat scroll per platform with a section jump list. No NavigationView.

### Dependencies

`Uno.Toolkit.Skia.WinUI` only. `Uno.Themes` is not referenced; Neu is a peer theme, not a Material overlay.

### Data flow

App.xaml → `<neu:NeuTheme ColorOverrideSource="..."/>` → merges tokens + styles → templates resolve `ThemeResource NeuSurface` etc. Lightweight styling via resource overrides, same convention as Uno Themes.

### Platform constraints

Single Skia render path. Library must be trimming/AOT-clean: attached properties and dependency properties only, no reflection.

### Testing / validation

- Gallery screenshots per head, light and dark; the four Skia heads are asserted pixel-identical.
- `tools/contrast-check.ps1` over all ink/surface pairs.
- 200-row ListView scroll test on android and browserwasm.
- Runtime theme switch verifies `Shadow.Color` re-resolution.
- Keyboard-only focus pass on desktop.

### Decisions

```
Decision: ShadowContainer (Uno.Toolkit.Skia.WinUI) is the sole shadow mechanism.
Reason: only cross-platform primitive with inner + layered shadows; identical Skia output.
Tradeoff: new dependency; rounded-rect silhouettes only.

Decision: Skia-only heads.
Reason: one render path; shadows identical everywhere; the only known VSM issue was Windows-composition-specific.
Tradeoff: apps needing WinAppSDK-only APIs cannot take Neu.Uno.

Decision: Depth = named ShadowCollection resources swapped by VSM Setter.
Reason: templates stay pure XAML and overridable.
Tradeoff: if the Skia swap misbehaves (step 4), NeuDepth attached property takes over and template authors must use it.

Decision: Surface brush lives on ShadowContainer in every template.
Reason: documented requirement for inset shadows.
Tradeoff: templates diverge from stock WinUI anatomy.

Decision: Library ships no fonts.
Reason: wrong layer for a theme library; ramp tokens only.
Tradeoff: gallery picks its own faces and presents them as a recommendation.
```

## Design Brief

### Subject

A single molded slab, plaster or matte polymer, from which every control is extruded or into which it is pressed. Canvas and control share one color; depth carries the meaning.

### Visual direction

The default neumorphism cluster is cool grey `#E0E5EC`, saturated blue accent, everything pill-shaped. Neu leaves that cluster on all three axes: warm plaster, amber accent, pill radii reserved for interactives while surfaces stay squarer so extrusions read as parts.

### Color tokens

| Token | Light | Dark | Role |
|---|---|---|---|
| `NeuCanvas` = `NeuSurface` | `#E9E3DC` | `#2C2B29` | canvas and control share one color |
| `NeuShadowLight` | `#FFFFFF` @ .72 | `#3D3B38` @ .55 | top-left highlight |
| `NeuShadowDark` | `#B9AFA4` @ .55 | `#151412` @ .70 | bottom-right shade |
| `NeuInk` | `#1F1F1E` | `#ECE7E1` | text, glyphs |
| `NeuInkSecondary` | `#5F5A55` | `#A39D96` | captions, hints |
| `NeuAccent` | `#C99436` | `#D9A441` | one filled action per view; checked/selected marker |
| `NeuAlert` | `#A64134` | `#E5786A` | semantic only |
| `NeuOnAccent` | `#1F1F1E` | `#1F1F1E` | ink on the amber fill; does not flip |
| `NeuOnAlert` | `#ECE7E1` | `#1F1F1E` | ink on the red fill |

Contrast floors: `NeuInk` on `NeuSurface` ≥ 7:1 light, ≥ 10:1 dark; `NeuInkSecondary` ≥ 4.5:1; accent is a fill under ink text, never text itself. `NeuAlert` is message ink as well as a ring color, so it owes 4.5:1 on the canvas too.

The `NeuAlert` values and the two `NeuOn*` tokens are amendments to the original brief, made in step 2 to clear those floors. Rationale in `docs/decisions.md` (D6, D7); `tools/contrast-check.ps1` is the check.

### Depth scale

| Collection | Offset | Blur | Spread | Use |
|---|---|---|---|---|
| `NeuRaisedSm` | 3 | 6 | 0 | list items, chips, inline inputs |
| `NeuRaisedMd` | 6 | 12 | 0 | buttons, toggles, cards |
| `NeuRaisedLg` | 10 | 24 | -2 | dialogs, floating surfaces |
| `NeuInsetSm` | 3 | 6 | 0 | pressed, checked, fields |
| `NeuInsetMd` | 6 | 12 | 0 | wells, tracks |
| `NeuFlat` | — | — | — | disabled, high contrast |

Each collection contains exactly two shadows: `NeuShadowDark` at (+offset, +offset) first, `NeuShadowLight` at (−offset, −offset) second. Inset collections set `IsInner="True"` on both.

### Radius

`NeuRadiusSm 8`, `NeuRadiusMd 13`, `NeuRadiusLg 21`, `NeuRadiusPill 999`. Surfaces use Sm/Md; interactives use Md/Pill.

### Spacing

Fibonacci: 3, 5, 8, 13, 21, 34. Rule: shadow offset < gap between sibling containers. Md offset 6 requires a minimum 13 gap.

### Typography (ramp only)

| Role | Size/Line | Weight | Notes |
|---|---|---|---|
| Display | 34/40 | 600 | |
| Title | 21/28 | 600 | |
| Body | 16/24 | 400 | |
| Label | 13/16 | 500 | tracking +0.4 |
| Data | 13/16 | 400 | mono, optional |

Gallery default: platform sans for display/body, Martian Mono for token values. Swappable later.

### Composition archetype (gallery): the specimen sheet

Derived from a material sample board. One continuous slab; specimens in a loose left-aligned grid; small ink caption under each; sections divided by a `NeuGroove`. No cards, no header band, no side rail.

```
WIDE                                          NARROW
┌────────────────────────────────────────┐    ┌──────────────────┐
│ Neu · Buttons                          │    │ Neu · Buttons     │
│ [Raised] [Accent] (Pill)  (o) (◦)      │    │ [Raised] [Accent] │
│ raised   filled   pill    tog  tog     │    │ (Pill) (o) (◦)    │
│ ═══════════════════ groove ═══════════ │    │ ═══ groove ═══    │
│ Neu · Inputs                           │    │ Neu · Inputs      │
│ [___________]  [___________ ▾]         │    │ [_____________]   │
│ inset field    combo                   │    │ [_________ ▾]     │
└────────────────────────────────────────┘    └──────────────────┘
```

### Signature

The press: every interactive is an extrusion that inverts to an inset on press, through a brief flat midpoint. One signature, applied everywhere. Everything else stays quiet: no strokes, no hover elevation, one amber fill per view.

### Responsive

Gallery grid collapses at 640 and 400 via `ResponsiveExtension`. Depth tokens never change with width.

## Interaction Brief

### User flows

Library consumer: add package → `<neu:NeuTheme/>` in App.xaml → optional override dictionary → optional implicit styles.
Gallery: scroll, light/dark toggle, "show depth guides" overlay.

### Input behavior per control

| Control | Rest | PointerOver | Pressed / Checked / Selected | Disabled |
|---|---|---|---|---|
| Button | RaisedMd | RaisedMd, surface +2% lightness | InsetSm | Flat, ink 38% |
| ToggleButton / CheckBox / RadioButton | RaisedSm | same | InsetSm + amber ink glyph | Flat |
| ToggleSwitch | track InsetMd, thumb raised pill | — | amber fill under thumb | Flat |
| Slider | track InsetMd, thumb RaisedMd, amber fill | — | thumb InsetSm | Flat |
| TextBox / PasswordBox / NumberBox / ComboBox | InsetSm well | — | focus: 2px amber outer ring (second ShadowContainer, spread 2, blur 0) | Flat |
| ProgressBar / ProgressRing | InsetSm track, raised amber bar | — | — | — |
| ListViewItem / Card | RaisedSm | same | InsetSm + amber leading bar | — |
| TabBar / Chip / Expander | RaisedSm | same | InsetSm + amber marker | Flat |
| ContentDialog | RaisedLg on dimmed canvas | — | — | — |

Error on inputs: ring swaps to `NeuAlert`, message text in `NeuAlert` ink below.

### Empty / loading / error states (gallery)

Empty specimen slot: caption "No sample yet" in secondary ink. Loading: Neu ProgressRing. Head render failure: ink caption naming the head.

### Animations / transitions

Press: Raised → Flat → Inset; release reverses. Durations and easing come from `xaml-design-polish` when loaded. Gated on `UISettings.AnimationsEnabled`; reduced motion = instant swap.

### Feedback states

Every state change pairs depth with a second channel: glyph, ink change, ring, or bar. State is never shadow-only.

### Accessibility

- Contrast floors as above; enforced by `tools/contrast-check.ps1`.
- Focus visual: 2px amber ring on every interactive.
- Touch targets ≥ 44px.
- `NeuHighContrast` dictionary: all depth → `NeuFlat` + 1px `NeuInk` stroke on interactives.

### Runtime verification steps

1. Step 4 spike: Button with VSM `Setter` on `Shadows`, desktop head. Record result.
2. Screenshot grid: every control × light/dark × 4 heads; assert identical across heads.
3. Contrast script over token pairs, light and dark.
4. Perf: 200-row ListView of RaisedSm cards on android and browserwasm. If it stutters, list blur drops to 4 or list items go Flat.
5. Runtime theme switch: confirm `Shadow.Color` re-resolves under `ThemeResource`; otherwise split collections per theme.
6. Keyboard-only pass on desktop: focus visible everywhere.

## Implementation Plan

1. `feat: scaffold Neu.Uno library + gallery (desktop, wasm, android, ios)` — package refs, empty `NeuTheme`, build green on every head.
2. `feat: color, metric, radius, type tokens` — ThemeDictionaries light/dark; contrast script.
3. `feat: depth ShadowCollections + NeuSurface/groove/well primitives` — gallery page showing all six collections. Verification 5 runs here.
4. `spike: VSM shadow swap on desktop` — outcome recorded in `docs/decisions.md`; decides whether `NeuDepth` is needed.
5. `feat: Button, ToggleButton, CheckBox, RadioButton` — press signature established; screenshot baseline.
6. `feat: ToggleSwitch, Slider, ProgressBar, ProgressRing`.
7. `feat: TextBox, PasswordBox, NumberBox, ComboBox` — focus ring layer.
8. `feat: ListViewItem, Card, Chip, TabBar, Expander, ContentDialog` — verification 4 after ListViewItem.
9. `feat: implicit-style dictionary + NeuHighContrast`.
10. `docs: README, override guide, gallery captions`.

Each step: build all heads, screenshot, commit.

## Unresolved Questions

- `Shadow.Color` + `ThemeResource` re-resolution on theme switch (answered in step 3).
- Gallery typefaces beyond the platform-sans + Martian Mono default.
- Whether the article draft is part of this repo or the blog pipeline.
