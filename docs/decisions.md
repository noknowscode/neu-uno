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

**Decided:** two tokens beyond the Design Brief table, both `#1F1F1E` in *both* themes.

**Reason:** the amber fill is nearly as light in dark theme as in light. Dark-theme
`NeuInk` (`#ECE7E1`) on dark-theme `NeuAccent` (`#D9A441`) measures **1.83:1**, so
"ink on the accent fill" cannot mean the theme's own ink. Fixing the on-fill ink at
the light-theme dark ink gives 6.11:1 (light) and 7.33:1 (dark).

**Tradeoff:** one token pair that does not flip with the theme, which reads as an
exception in `Tokens.Colors.xaml` and is commented as such.

---

## Open — three token pairs are below their contrast floor

`tools/contrast-check.ps1` exits 1 on the step-2 tokens. All three failures involve
`NeuAlert`, and fixing any of them means changing a value in `Tokens.Colors.xaml`,
which is a Stop Check. Recorded here, not fixed:

| Theme | Pair | Ratio | Floor |
|---|---|---|---|
| Light | `NeuAlert` on `NeuSurface` | 4.14:1 | 4.5 |
| Dark  | `NeuAlert` on `NeuSurface` | 4.09:1 | 4.5 |
| Light | `NeuOnAlert` on `NeuAlert` | 3.13:1 | 4.5 |

The first two matter: the Interaction Brief puts the input error message in `NeuAlert`
ink on the canvas, so `NeuAlert` is used as text and owes 4.5:1. Both themes need the
red about one step darker (light) and one step deeper (dark).

The third only matters if `NeuAlert` is ever used as a *fill* under text. It is
"semantic only" in the brief — ring color and message ink — so the pairing may simply
be forbidden instead of fixed. Light `NeuAlert` fails with either ink (3.13:1 with dark,
4.29:1 with light), so no choice of `NeuOnAlert` rescues it at the current red.

Decide before step 7, which is the first step that renders an error state.

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
