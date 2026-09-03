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

## Spike outcomes

### Step 4 — VSM shadow swap on desktop

Not yet run. Records whether D3 holds or `NeuDepth` is required.
