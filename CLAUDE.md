# Neu.Uno — project rules for Claude Code

Read `docs/SPEC.md` before any implementation. It is the source of truth for tokens, depth scale, control behavior, and the plan. This file holds the conventions that must survive every edit.

## Project

- Neumorphism theme library for Uno Platform. Package id `Neu.Uno`, root namespace `Neu.Uno`, XAML prefix `neu`.
- Targets: `net10.0-desktop`, `net10.0-browserwasm`, `net10.0-android`, `net10.0-ios`. Skia only. Do not add a WinAppSDK head.
- .NET 10. Do not scaffold or reason against .NET 9.
- One dependency: `Uno.Toolkit.Skia.WinUI`. Adding any other package is a Stop Check.
- Environment: Windows 11, PowerShell. Use `dotnet build -f net10.0-desktop` for the fast loop; build all heads before committing a control.

## Hard conventions (violations are bugs)

1. **Surface brush on the container.** Every `utu:ShadowContainer` carries the `Background`; its child has no Background/Fill. Inner shadows will not paint otherwise.
2. **Depth is a named collection.** Templates reference only `NeuRaisedSm/Md/Lg`, `NeuInsetSm/Md`, `NeuFlat`. No inline `<utu:Shadow>` in a control template.
3. **Collection contents are fixed.** Two shadows per collection: `NeuShadowDark` at (+d,+d) first, `NeuShadowLight` at (−d,−d) second. Inset collections set `IsInner="True"` on both.
4. **Rounded rects and pills only.** ShadowContainer follows size + CornerRadius. No engraved text, no shaped silhouettes.
5. **State is never shadow-only.** Pressed, checked, selected, focused, and error each add a second channel: glyph, ink change, amber ring, or amber bar.
6. **One amber fill per view.** `NeuAccent` is for the primary action and state markers. `NeuAlert` is semantic only. No other saturated fills.
7. **Offset < gap.** Sibling containers using `Md` depth (offset 6) need at least 13 of spacing between them.
8. **Spacing is Fibonacci.** 3, 5, 8, 13, 21, 34. Radii are `NeuRadiusSm/Md/Lg/Pill` tokens.
9. **Tokens only.** No hex values in Styles/. Colors, metrics, radii, and type come from `Tokens.*.xaml`.
10. **No fonts shipped.** Type tokens are size/weight/line only. Gallery may reference fonts; the library may not.
11. **AOT/trim clean.** Dependency properties and attached properties only. No reflection, no dynamic resource lookup in C#.
12. **Contrast floors.** `NeuInk`/`NeuSurface` ≥ 7:1 light, ≥ 10:1 dark; `NeuInkSecondary` ≥ 4.5:1. Run `tools/contrast-check.ps1` after any token change.

## Layout

```
src/Neu.Uno/Theme/        NeuTheme.cs, Tokens.Colors|Metrics|Shadows|Type.xaml
src/Neu.Uno/Styles/       one file per control: Neu.<Control>.xaml
src/Neu.Uno/Primitives/   NeuSurface, NeuGroove, NeuWell
src/Neu.Uno/Helpers/      NeuDepth.cs (only if the step-4 spike requires it)
samples/Neu.Gallery/      specimen-sheet gallery, all heads
tests/                    screenshot diffs
tools/                    contrast-check.ps1
docs/                     SPEC.md, decisions.md
```

## Template anatomy

```xml
<!-- Every interactive follows this shape -->
<utu:ShadowContainer x:Name="Surface"
                     Background="{ThemeResource NeuSurfaceBrush}"
                     CornerRadius="{ThemeResource NeuRadiusMd}"
                     Shadows="{ThemeResource NeuRaisedMd}">
  <Grid>  <!-- no Background here -->
    <ContentPresenter .../>
  </Grid>
</utu:ShadowContainer>

<!-- Focus ring is a second container layered behind, spread 2, blur 0 -->
```

VisualStates swap `Shadows` via `Setter Target="Surface.Shadows"`. Press sequence is Raised → Flat → Inset. Motion durations come from `xaml-design-polish` when loaded; gate on `UISettings.AnimationsEnabled`.

## Workflow

- Follow the numbered plan in `docs/SPEC.md`. One step per commit, conventional commit messages.
- Before templating a control: `Skill/Rule:` block naming `xaml-art-direction` (direction), `xaml-design-polish` (motion), `userinterface-wiki-uno` (per-detail correctness) as applicable.
- After each control: build all heads, capture light/dark screenshots on desktop, commit.
- Step 4 (VSM shadow-swap spike on desktop) gates steps 5–9. Record the result in `docs/decisions.md`.
- Perf check after `ListViewItem` (step 8): 200-row list on android and browserwasm.
- Two failed fix cycles → Debug Checkpoint. Check toolkit source and docs before a third attempt, especially for ShadowContainer rendering.

## Stop Checks (pause and ask)

- New package or dependency.
- Adding a head or changing TFMs.
- New custom control beyond `NeuSurface`, `NeuGroove`, `NeuWell`.
- Changing any token value in `Tokens.Colors.xaml` or the depth scale.
- Public API change on `NeuTheme`.
- Deleting or moving style files.

## Evidence order

Existing project code → this file and `docs/SPEC.md` → loaded skills → Uno Toolkit docs (`ShadowContainer.md`) → Uno docs → toolkit source → build/runtime output → general knowledge. Runtime output wins on rendering questions.

## Writing

Direct, developer-facing, low-drama. Code before prose. Gallery captions are plain: "Raised", "Inset", "Pressed", "Disabled". Empty state copy: "No sample yet".
