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

**Status:** unverified. The step-4 spike on the desktop head decides this, and its
outcome is recorded below. Steps 5-9 are gated on it.

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

## Spike outcomes

### Step 4 — VSM shadow swap on desktop

Not yet run. Records whether D3 holds or `NeuDepth` is required.
