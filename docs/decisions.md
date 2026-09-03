# Decisions

Architectural decisions for Neu.Uno, carried forward from `docs/SPEC.md`.
One entry per decision: what was decided, why, and what it costs.
Spike outcomes are appended as they are recorded.

---

## D1 — ShadowContainer is the sole shadow mechanism

**Decided:** `ShadowContainer` from `Uno.Toolkit.Skia.WinUI` provides all depth in Neu.Uno.

**Reason:** it is the only cross-platform primitive offering both inner and layered
outer shadows, and Skia draws them, so every head renders identically.

**Tradeoff:** a new dependency, and silhouettes are limited to rounded rects and pills.

---

## D2 — Skia-only heads

**Decided:** `net10.0-desktop`, `net10.0-browserwasm`, `net10.0-android`, `net10.0-ios`.
No WinAppSDK head.

**Reason:** one render path, so shadows are identical everywhere. The only known
VSM shadow-swap issue (Toolkit #697) was specific to Windows composition.

**Tradeoff:** apps that need WinAppSDK-only APIs cannot take Neu.Uno.

---

## D3 — Depth is named ShadowCollection resources swapped by VSM Setter

**Decided:** depth is a fixed set of named `ShadowCollection` resources; visual states
swap them with `<Setter Target="Surface.Shadows" .../>`.

**Reason:** templates stay pure XAML and remain overridable by consumers.

**Tradeoff:** if the Skia swap misbehaves, the `NeuDepth` attached property takes
over and template authors must use it instead.

**Status:** verified on the desktop head, step 4. See the spike outcome below.
`Helpers/NeuDepth.cs` is not needed and the folder stays empty.

---

## D4 — The surface brush lives on the ShadowContainer in every template

**Decided:** every `utu:ShadowContainer` carries the `Background`; its child carries none.

**Reason:** documented Toolkit requirement — inner shadows do not paint if the child
has its own Background or Fill.

**Tradeoff:** Neu templates diverge from stock WinUI control anatomy.

---

## D5 — The library ships no fonts

**Decided:** type tokens express size, weight and line height only.

**Reason:** a theme library is the wrong layer to ship typefaces.

**Tradeoff:** the gallery picks its own faces and presents them as a recommendation
rather than a guarantee.

---

## D6 — NeuOnAccent / NeuOnAlert carry the ink that sits on a fill

**Decided:** two tokens beyond the Design Brief table, naming the ink that goes on
top of a fill. `NeuOnAccent` is `#1F1F1E` in *both* themes; `NeuOnAlert` flips
normally (`#ECE7E1` light, `#1F1F1E` dark).

**Reason:** the amber fill is nearly as light in dark theme as in light. Dark-theme
`NeuInk` (`#ECE7E1`) on dark-theme `NeuAccent` (`#D9A441`) measures **1.83:1**, so
"ink on the accent fill" cannot mean the theme's own ink. Pinning it dark gives
6.11:1 (light) and 7.33:1 (dark). The two reds land on opposite sides of the ink,
so `NeuOnAlert` needs no such exception: 4.99:1 light, 5.68:1 dark.

**Tradeoff:** `NeuOnAccent` does not flip with the theme, which reads as an
exception in `Tokens.Colors.xaml` and is commented as such.

---

## D7 — NeuAlert is one step off the Design Brief table

**Decided:** `NeuAlert` is `#A64134` light (was `#B5493A`) and `#E5786A` dark
(was `#D9685A`).

**Reason:** the Interaction Brief puts input error messages in `NeuAlert` ink on the
canvas, so the token is text and owes 4.5:1. The tabled reds measured **4.14:1** and
**4.09:1**. The replacements measure 4.81:1 and 4.87:1, keeping the hue and moving
only lightness — darker in light theme, brighter in dark.

**Tradeoff:** the palette no longer matches the `docs/SPEC.md` Design Brief table
verbatim; this entry is the reconciliation. It also changed which ink sits on the
red, which is why `NeuOnAlert` flips by theme (see D6).

`tools/contrast-check.ps1` now exits 0 on every enforced floor, both themes.

---

## Note — NeuRadiusPill renders as an oval on Skia

`CornerRadius="999"` is reduced per axis rather than clamped to a circle, so an
89x55 specimen gets a 44.5x27.5 ellipse rather than a stadium. Confirmed on the
desktop head, step 2. The token stays at 999 as the declared value; pill-shaped
templates set half their own height instead. The gallery shows both side by side.

---

## D8 — ShadowCollections are duplicated per theme, not shared with a ThemeResource color

**Decided:** `Tokens.Shadows.xaml` declares all six collections once per theme
dictionary. Consumers bind `Shadows="{ThemeResource NeuRaisedMd}"`.

**Reason:** this is what the Toolkit documents for theme switching - the
`ShadowCollection` itself has to live in the theme dictionary. Sharing one collection
and giving each `Shadow` a `Color="{ThemeResource NeuShadowDark}"` is the obvious
alternative and is not the supported path. This answers the SPEC's first unresolved
question without needing to discover it at runtime.

**Tradeoff:** three copies of the same five collections. The numbers and colors are
`StaticResource` lookups into `Tokens.Metrics.xaml` and `Tokens.Colors.xaml`, so the
duplication is structural only - no value is written twice.

---

## D9 — NeuGroove carries no shadow

**Decided:** the groove is two 1px borders, shade over highlight, drawn from the shadow
token colors and their opacities. It is not a `ShadowContainer`.

**Reason:** a blurred inset shadow on a 2px box has nothing to fall across. The scored
line is the honest primitive for the effect.

**Tradeoff:** the one place in the library where depth is not a named
`ShadowCollection`, which is a documented exception to CLAUDE.md rule 2 rather than a
violation of it - it carries no shadow at all.

---

## Spike outcomes

### Step 4 — VSM shadow swap on desktop

**Result: D3 holds.** A `<Setter Target="Surface.Shadows" Value="{ThemeResource ...}" />`
inside a `VisualState` applies on the Skia desktop head, both at template time and on a
state change long after first paint. Toolkit #697 does not reproduce here, as D2 predicted.
`NeuDepth` as an attached-property fallback is not required; steps 5-9 proceed on the
VSM swap.

**How it was measured.** `NEU_GALLERY_PRESS=1` flips one `NeuSurface` from `RaisedMd` to
`InsetSm` on a 1.5s `DispatcherTimer` after `Loaded`, then the window is captured and
diffed against the same build without the flag:

| Region | Pixels changed | Max delta |
|---|---|---|
| Swap specimen | 9,020 | 24 |
| Control specimen (`Extruded`, never changes) | 0 | - |

A max delta of 24 is shadow magnitude. The first attempt read 202 and looked like a
failure: the handler also rewrote the caption, the wider caption widened the
`StackPanel`, and the centered fixed-width specimen shifted - so the diff was measuring
layout, not depth. **A before/after pixel diff of a depth change is only valid if
nothing else on the row can move.** The specimen is now `HorizontalAlignment="Left"`
with a fixed caption.

### Verification 5 — theme switch re-resolves the shadow colors

**Result: it works, via per-theme collections (D8), not via a `ThemeResource` color.**
Measured on the depth specimens, sampling the brightest pixel in the top-left highlight
band of `NeuRaisedLg`:

| Theme | Canvas | Brightest highlight | Darkest shade |
|---|---|---|---|
| Light | 233 | 242 (toward `#FFFFFF`) | 219 (toward `#B9AFA4`) |
| Dark | 44 | 49 (toward `#3D3B38`) | 36 (toward `#151412`) |

The dark highlight tops out at 49, not near 255, so the dark collection resolved the
dark theme's `NeuShadowLight` rather than the ambient one. This is the check that would
catch a `StaticResource` inside a theme dictionary being resolved against the wrong
theme at parse time, which it is not.

### Step 5 — Button, ToggleButton, CheckBox, RadioButton

**Result: the press signature and the depth-swap states hold on all four controls.**
Rest, PointerOver, Pressed, Checked, Indeterminate and Disabled render correctly in
both themes on the desktop head; all four heads build clean. Three findings are
worth carrying forward.

**1. A ShadowContainer draws its shadow around its CONTENT, not around itself.**
The shadow follows `PART_ShadowOwner`, which takes the child's natural size. When the
container has an explicit `Width`/`Height` plus `VerticalAlignment="Center"`, the child
is *not* stretched to fill it, so the shadow is drawn around whatever the child happens
to measure. Measured on the RadioButton, whose `Box` is a 21x21 container:

| Radio | Box slot | `PART_ShadowOwner` | Rendered |
|---|---|---|---|
| Checked | 21x21 | 21x8 (the dot's height) | a horizontal pill |
| Unchecked | 21x21 | 21x0 (empty Grid) | nothing at all |

The fix is to repeat the explicit size on the inner Grid, which is what the CheckBox
already did. **A ShadowContainer with an explicit size needs its child sized to match.**
The Button and ToggleButton are unaffected because their containers carry no explicit
size and fill a Grid cell, which does stretch the child.

**2. The focus ring renders; entering the Focused state headlessly does not.**
The ring anatomy is sound - proved by making `NeuFocusRing` the FocusRing container's
default shadow and capturing: a crisp 2px amber band appears around every button,
clearly legible over the Surface's own RaisedMd shadow. So the layering (ring container
behind an opaque surface), `spread 2 / blur 0`, and the collection all work.

What could not be verified headlessly is the `Focused` VisualState being *entered*.
Neither `Focus(FocusState.Keyboard)` nor an explicit `GoToState(..., "Focused")` takes:
Uno's focus manager re-asserts `Unfocused` because the window is never foreground when
the app is launched from a background session. Two dead ends along the way, recorded so
they are not retried: `BlurRadius="0"` is *not* the cause (blur 4 renders no differently),
and the ring is not being buried under the Surface's shadow. **Confirming that a real
keyboard focus lights the ring is deferred to step 7,** which owns the focus ring layer
and introduces the controls where focus is the primary state.

**3. `VisualTransition.GeneratedDuration` is Uno0001 not-implemented.**
It appears as a warning on every template that declares a release transition. It is
harmless here because each `VisualTransition` supplies an explicit `Storyboard`, which
does run; `GeneratedDuration` only governs the tween Uno would synthesise in its
absence. Do not rely on it alone to time a transition.
