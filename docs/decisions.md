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

## D10 - Implicit styles are opt-in, and the keyed style is the primary form

**Decided:** every control style is keyed. `Styles/Neu.Implicit.xaml` holds one
implicit entry per control, each `BasedOn` its keyed style, and it is merged only
when `NeuTheme.UseImplicitStyles` is set.

**Reason:** a theme library that restyles an app's controls the moment it is
referenced is harder to adopt than one that waits to be asked - it cannot be
tried on one screen. Keeping the implicit entries `BasedOn` rather than
duplicating the templates means a consumer who overrides a keyed style changes
both paths at once.

**Tradeoff:** the common case costs a line of markup. And an implicit style
matches on the exact type, so a derived control - `class MyDialog : ContentDialog`
being the usual one - still needs the keyed style by hand, which is a trap that
the opt-in form makes no better.

Sub-part styles (`NeuSliderThumbStyle`, `NeuNumberBoxSpinButtonStyle`,
`NeuPasswordRevealButtonStyle`, `NeuTextBoxDeleteButtonStyle`,
`NeuExpanderHeaderStyle`) are deliberately not in the implicit dictionary: each is
applied by name inside the template that owns it, and an explicit style on a
template part beats an implicit one, so the implicit `Button` and `ToggleButton`
entries do not reach them.

---

## D11 - High contrast is a swap of the depth collection names, not a template variant

**Decided:** `Theme/Neu.HighContrast.xaml` restates the six depth collections
(`NeuRaisedSm/Md/Lg`, `NeuInsetSm/Md`, `NeuFlat`) as one hard-edged `NeuInk`
outline, and the two rings one step wider. Merge it after `NeuTheme`. It touches
nothing else - no template, no color.

**Reason:** rule 2 already forces every template to reference depth by name, so
restating the names reaches every control at once. No template in the library
knows the file exists, and a consumer's own Neu-anatomy control is covered by it
for free.

**Tradeoff:** three deviations, all consequences of the collections being shared:

1. The outline lands on every Neu surface, not only on interactives as the
   Interaction Brief says. A dictionary swap cannot tell a Button from a
   NeuSurface, and with depth gone an unoutlined surface has no edge at all.
2. Every depth step becomes the same outline, `NeuFlat` included, so a pressed
   or checked control keeps its silhouette. It stays readable because rule 5
   already requires a second channel for every state - depth was never carrying
   one alone.
3. The focus and error rings widen from spread 2 to 3 rather than staying put.
   At equal width they would differ from the resting outline by hue alone, which
   is the one channel high contrast can least afford to spend.

**Not wired to the OS.** Merging is the consumer's call; nothing here reads the
system high-contrast setting. Doing that from a `HighContrast` theme dictionary
is the obvious next move and is left out of step 9 on purpose - it is untested
behavior on this stack and would have shipped unverified.

---

## D12 - The determinate ring is driven by an attached property, not a converter

**Decided:** `Helpers/NeuProgressArc.cs` is an attached property that writes a
shape's `StrokeDashArray` from a `ProgressRing`'s own `Minimum`, `Maximum` and
`Value`. `NeuProgressArcConverter` is deleted.

**Reason:** a converter sees one value. It could not know the ring's range, so it
was given `Minimum`/`Maximum` properties defaulting to the control's own 0-100 -
and any app that set a different range got an arc scaled by the difference, with
no error. `Maximum="1"`, the natural choice for a fraction, drew nothing. The
consumer could not fix it either: the converter instance lived inside the
library's own template as a keyed resource, so correcting it meant retemplating
the control. The attached property reads the range off the ring and re-reads it
whenever any of the three change.

**Tradeoff:** a public static class with an attached property instead of a
converter, and a subscription per ring rather than a pure function. It also takes
its geometry from the shape it is attached to - `Width` and `StrokeThickness` -
which removes the second footgun in the converter's docs, that a resized ring
needed its geometry restated by hand.

Found by building an app against the library rather than by reading it. See
Verification 6.

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
> **Corrected in step 7 - the second half of this heading is wrong.** `Focus()`
> does succeed from a headless run and the Focused state *is* entered: the
> gallery now reports `Focus()=True state=Keyboard` and the amber ring paints on
> both a Button and a TextBox. What actually produced the step 5 negative was
> two capture hooks calling `Focus()` on the same tick, so whichever ran last
> took focus and the other specimen rendered at rest. Nothing in the templates
> needed fixing. The paragraph below is kept as written, because the two dead
> ends it records (blur 0, and the ring being buried) are still true findings.
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

### Step 6 — ToggleSwitch, Slider, ProgressBar, ProgressRing

**Result: all four retemplate cleanly against the stock part names.** Rest, On/Off,
determinate, indeterminate, error and disabled all render in both themes; four heads
build. Part and state names were read off the running app with a `detail=full`
visual-tree snapshot of the stock controls rather than from WinUI's `generic.xaml`,
which does not match Uno's. Four findings.

**1. A ShadowContainer's Background and shadows ignore `UIElement.Opacity`.**
Both are painted by the container's own Skia canvas, which sits outside the XAML
opacity pipeline. The disabled ToggleSwitch was the tell: `Opacity` 0.38 on the
subtree faded the label and the knob but left the amber track at full strength, so
the disabled switch read as the *most* saturated specimen on the sheet. **A disabled
state that needs to mute a Neu surface has to swap the brush or the collection -
opacity will not do it.** The On fill is now a plain `Rectangle` laid over the groove
(inset 3, so the inner shadow still rings it) instead of the container's Background,
because a plain shape does obey opacity.

**2. `NeuFlat` erases a control that has no content of its own.**
Canvas and surface are one colour, so a surface-coloured shape with no shadow is
invisible. That is fine for Button or CheckBox, which still show ink or a glyph, but
a Slider thumb or a switch knob *is* only its shadow: `Flat` deleted it from the
sheet. Both now go to `NeuRaisedSm` when disabled - shallower, not gone - and
finding 1 rules out dimming them back into view. The Interaction Brief's
"Disabled: Flat" holds only for controls that carry content.

**3. ProgressBar never sizes its indeterminate indicators.**
It sizes `DeterminateProgressBarIndicator` and leaves `IndeterminateProgressBarIndicator`
and `...2` at width 0, which draws nothing; the first indeterminate bar was an
invisible groove. Their lengths are fixed in the template (72 and 32). The
*positions* still come from the control's own `TemplateSettings`
(`ContainerAnimationStartPosition` / `EndPosition` and the `Container2` pair), which
it recomputes on resize, so those stay bound.

**4. ProgressRing had to lose its Lottie player.**
Stock draws itself with an `AnimatedVisualPlayer` hosting a `LottieSKCanvasElement`;
there is no way to give that a neumorphic surface, so the template replaces it and
owns the motion. The ring is a 48px circular `InsetSm` well with a stroked `Ellipse`
arc - a dash pattern, not a `Path`, since Uno's path mini-language throws on
elliptical arcs at parse time. Indeterminate and determinate are two separate
Ellipses so a VisualState setter and a binding never land on the same property.
Determinate needs value-to-sweep arithmetic, which is
`Helpers/NeuProgressArcConverter.cs` - the library's first converter, and its
geometry defaults must match the template.

**Perf, carried from the depth work:** both indeterminate visuals repeat forever, and
a `RepeatBehavior="Forever"` storyboard holds the Skia compositor at display refresh
for as long as it runs - roughly a third of a core each - and keeps costing while the
target is `Collapsed`. Both are scoped to the state that needs them (`Indeterminate`,
`Active`) so leaving the state stops them. Hide an indeterminate control by turning it
off, not by collapsing it.

**Not verified:** that a click actually toggles the switch, and that dragging moves the
slider thumb. Synthesised pointer input cannot be aimed at this window from a headless
run - it lands in Hot Design - so both were verified by rendering their states, not by
driving them. Same gap as the focus ring in step 5, and the same fix: a human pass.

### Step 7 — TextBox, PasswordBox, NumberBox, ComboBox

**Result: the focus ring layer is real and verified.** Every input is an InsetSm
well with a second ShadowContainer behind it carrying `NeuFocusRing` at spread 2,
blur 0. Confirmed on the running app in both themes, with the gallery reporting
the live `FocusState` next to the specimen rather than leaving a capture to be
read as evidence. Four heads build; contrast floors pass.

**1. The step 5 focus finding was wrong, and this is how it was caught.**
Two capture hooks were calling `Focus()` on the same 1.5s tick - one in the
Buttons row, one in the Inputs row - so whichever ran last took focus and the
other specimen photographed at rest. In step 5 only the Buttons hook existed, but
the same class of mistake (reading "no ring in the capture" as "the state is
never entered") produced a confident and incorrect conclusion. **A capture that
shows the absence of something is not evidence until the mechanism reports its
own state.** The gallery now prints `Focus()=<bool> state=<FocusState>` beside
the focused specimen, and `NEU_GALLERY_FOCUS` picks which single row may take
focus in a run.

**2. Neither container may carry an explicit size.**
Both the ring and the well fill one single-cell Grid sized by the well's content.
A ShadowContainer that fills a cell stretches its child, so the empty ring Grid
comes out exactly the field's size - whereas a container with an explicit size
does not stretch its child at all (the RadioButton finding in step 5). This is
why the input family needs no hard-coded dimensions while the radio and switch do.

**3. Required parts are typed, not just named.**
`ContentElement` must be a ScrollViewer and `BorderElement` a Border; TextBox and
PasswordBox resolve both by name *and* cast. NumberBox resolves `InputBox` as a
TextBox and drives text, selection and validation through it, so it stays a
TextBox - stripped to `BorderThickness=0` and a transparent Background rather
than given the Neu TextBox style, which would nest a second well inside the
first. ComboBox resolves `Popup`, `PopupBorder`, `ScrollViewer` and
`ItemsPresenter`. All part names were read off the running stock controls.

**4. Error is composed by the app, not by the control.**
WinUI inputs have no error visual state and no error property, so there is
nothing for a template to react to. The library ships the `NeuAlertRing`
collection and the gallery demonstrates the composition - the field wrapped in a
container wearing that ring, with the message below in `NeuAlert` ink. **A
per-instance mechanism (an attached `HasError`) would be the better answer and is
deliberately not taken here:** it is public API on the library, which is a
decision to make explicitly rather than as a side effect of step 7.

**Deviation from the Interaction Brief:** disabled inputs keep the well at
InsetSm and fade only the ink. `Flat` would erase an empty field completely -
the same reason the slider thumb and switch knob stay raised when disabled
(step 6, finding 2). "Disabled: Flat" holds only for controls that carry content.

**Also replaced:** ComboBox's chevron is a Path, not the stock `AnimatedIcon`,
which needs a generated Lottie source; it rotates 180 degrees while open, which
is the open state's second channel. ComboBox item selection is an amber leading
bar rather than a filled row, so the drop-down does not spend the view's one
amber fill.

### Step 8 - ListViewItem, Card, Chip, TabBar, Expander, ContentDialog

**Result: all six render in both themes.** The library builds for all four TFMs.
Four of the six needed something the control demands that no documentation states,
and three of those failed *silently* or *fatally* rather than visibly.

**1. Overriding TabBar's ItemsPanel kills the app at startup.**
TabBar requires its own `TabBarListPanel`; giving it a plain horizontal
`StackPanel` - to stop it equalising item widths - throws during page
construction. **No window is ever created, `MainWindowHandle` stays 0, and
because a desktop Uno app is windowed there is no console output to say why** -
running it through `dotnet` rather than the exe produces an empty log too. Found
by bisecting the specimen section, not by reading an error. The panel is left
alone; a Neu TabBar is therefore sized by its container rather than hugging its
tabs, and the gallery gives it a width.
*This contradicts a note carried in from another project saying a StackPanel
ItemsPanel works on TabBar. It does not work here.*

**2. Expander needs two things the control does not do for itself.**
`ExpanderHeader` must have `IsChecked` bound two-way to `IsExpanded` in the
template - Expander does not wire it - or the header never enters its Checked
states and the chevron never turns. Separately, **a Neu-templated Expander never
enters any expand visual state at all.** Proved by defaulting the body to Visible
and watching the *collapsed* specimen show its content: if the states were
firing, `Collapsed` would have hidden it. Two group names were tried first
(`ExpandStates`, `ExpansionStates`) and neither fired, which is what forced the
measurement instead of a third guess. The body binds to `IsExpanded` through a
converter, so the reveal no longer depends on state names that are not part of
the control's contract.

**3. ContentDialog: BackgroundElement must be a Border.**
It is resolved by name *and type* and dereferenced without a null check, so a
`ShadowContainer` under that name throws `NullReferenceException` from inside
`ContentDialog.OnApplyTemplate` before the dialog appears - and an `async void`
handler swallows it, so the dialog simply never opens. Diagnosed by catching the
exception and printing its first stack frame into the gallery, which named the
method and line directly. The ShadowContainer now wraps a Border carrying that
name.
Also confirmed: **`ButtonsVisibilityStates` does not hide the button parts**, so
a dialog with no secondary button renders an empty one. Each button binds its own
`Visibility` to its own `*ButtonText` instead, which is self-maintaining.

**4. Card has no default template and is not a ContentControl.**
The Toolkit ships Card's template only in its Material and Cupertino packages; an
unstyled Card measures to zero height. Neu is a peer theme, so Card's anatomy is
written from scratch. It has no `Content` property at all - the body slot is
`SupportingContent`, and `CardContentControl` is the type to use when arbitrary
content is the point.

**5. List rows need a gap, not just a shadow.**
Rule 7 is offset < gap, and rows in a list are otherwise adjacent, so at RaisedSm
touching rows print their shade on each other. Rows carry a 13 bottom margin -
the smallest Fibonacci gap that clears an offset of 3 - which makes a Neu list
inherently airier than a Fluent one.

**Also:** TabBar inverts the selection rule on purpose. Everywhere else selected
means pressed in, but the bar is already an InsetMd channel and a tab sitting in
it has nowhere further to sink, so there the selected tab *rises*.
`NeuScrim` is a new token, the only one with alpha and the only one outside the
plaster palette. `NeuVisibilityConverter` is **internal** - it works around two
controls, and is not part of the library's surface.

### Verification 4 - the 200-row list

**Measured on desktop; not measured on android or browserwasm.**

| List | Scrolling, continuously driven |
|---|---|
| 200 stock rows | **58% of one core** |
| 200 Neu rows (every row a ShadowContainer) | **114.6% of one core** |

Debug build, 16-core machine, the sheet's two indeterminate specimens turned off
for the run so their Forever storyboards could not dominate the number. Driven by
a timer stepping the ScrollViewer, not by synthesized input, and not by
`CompositionTarget.Rendering`, which would have been measuring itself.

**So Neu rows cost about 2x stock rows to scroll.** That is a constant factor on
the shared Skia render path rather than a cliff, and it is bounded by
virtualization: a 200-row list never realises 200 ShadowContainers, only the
handful in view. The documented fallbacks (drop the blur, or take rows to
`NeuFlat` and let the amber bar carry selection) are not needed on desktop.

**Not run, and why.** No Android device or emulator is attached (`adb devices` is
empty), so the android leg could not run at all. The browserwasm leg was not
attempted. Both remain open, and 2x on a 16-core desktop says nothing certain
about a phone - **this verification is half done and should not be recorded as
passed.**

**Environment note:** the gallery's *android packaging* step now fails with a JVM
`os::commit_memory` failure ("the paging file is too small"), not with anything
from this step - the same head built in steps 5 through 7, the Neu.Uno library
still builds for `net10.0-android`, and only the app's Java d8/r8 step is
affected. A `Release` desktop build fails separately on
`Microsoft.NETCore.App.Runtime.Mono.win-x64 10.0.1` not being restorable, which is
why the perf numbers above are from a Debug build.

### Step 9 - implicit-style dictionary and NeuHighContrast

**Result: both work, and high contrast needed one change the design did not
anticipate.** Verified on the desktop head in both themes; all four heads build;
contrast floors pass.

**1. The implicit styles are proved by the app reporting on itself, not by a
capture.** The gallery's Implicit section is five controls with no `Style`
attribute anywhere, and its caption prints
`IMPLICIT=<flag> bare Button templated Neu=<bool>` - the second half walks the
bare Button's visual tree looking for a `ShadowContainer`, because the flag alone
would only prove the setter ran, not that a style matched. Off:
`IMPLICIT=False ... Neu=False` and all five render Fluent. On:
`IMPLICIT=True ... Neu=True` and all five render Neu. This is the step 7 lesson
applied up front rather than after a wrong conclusion.

**2. A hard-edged outline drawn outward loses the edges that have no layout
slack.** The first high-contrast pass used an ordinary (outer) shadow at
spread 1, blur 0. Most controls were fine, but the expanded Expander body
rendered with its left and right edges, its four corner arcs, and **no top or
bottom** - a shape that reads as a rendering bug rather than a layout one. The
cause is that `ShadowContainer` does not reserve layout space for an outward
shadow: the body is sized exactly to its content vertically and had slack
horizontally, so the ring survived on the axis with room. A blurred shadow hides
this because its falloff has no edge to lose. **The outline is now
`IsInner="True"`,** which puts it inside the surface's own bounds where nothing
can clip it, and the Expander body closes.

The two rings stay outward: they wear on a second `ShadowContainer` layered
behind the surface, which is the field's size with the ring free to spread past
it, and that has been the arrangement since step 7. Confirmed at spread 3 on a
really-focused button - `Focus()=True state=Keyboard`, and the amber band is
visibly wider than the resting ink outline.

**3. A disabled control's outline is full strength, and that is correct.** It
looks fainter on the sheet, which is the faded label reading back onto the
border - sampling the pixels shows the same ink as its neighbours.
`ShadowContainer` ignores `UIElement.Opacity` (step 6, finding 1), so the
disabled state cannot dim its own outline even if it wanted to.

**Verified by capture:** buttons, toggles, inputs, Expander, Card, Chip, TabBar,
the depth specimens (all six collections render as the same outline, which is the
proof every key was restated) and the five bare controls, light and dark.

**Not verified:** that the OS high-contrast setting turns any of this on - it
does not, by decision (D11). And nothing here was run on android or
browserwasm, which have been open since verification 4.

### Step 10 - README, override guide, gallery captions

**Result: the plan in `docs/SPEC.md` is complete.** `README.md`, `docs/overrides.md`
and three captured images; three missing gallery captions filled; one real defect
found in `tools/contrast-check.ps1` while writing the guide.

**1. Writing the override guide found a bug in the contrast tool.** The guide
documents `-TokensPath`, so an app can check the palette it actually ships rather
than the library's. Testing that claim rather than asserting it turned up the
failure mode: a token file restating only a couple of keys builds **no floored
pair at all**, and the script printed an empty table, said "All enforced contrast
floors met" and exited 0. A partial override file therefore got a green light
that meant nothing - the worst possible answer from an accessibility check, and
invisible unless you counted the rows. The script now tracks how many enforced
pairs it measured, refuses to report a pass on zero, and prints the count on
success (`10 pair(s) checked` is the full set).

**2. The guide's own example failed the check it recommends.** The first draft
overrode `NeuAccent` to a deep green `#3F7A6B` and left `NeuOnAccent` alone -
exactly the trap the same page warns about, three paragraphs later. It measures
3.31:1. Swapping `NeuOnAccent` to the light ink only reaches 4.06:1, because a
mid-tone accent is too dark for dark ink and too light for light ink at once.
The example is now `#6FAE99` / `#85C0AB`, which clears both themes with the
shipped dark ink and matches the amber's own logic - a light fill carrying dark
ink. **Verified by running the tool on the example, not by reading it.** The
constraint this exposes is now stated in the guide: overriding `NeuAccent` is
more constrained than it looks, and the middle of the lightness range fails
twice.

**3. Three specimens had no caption.** The Card (whose sibling
`CardContentControl` had one), the Chip row and the TabBar. The sheet's rule is
one caption per specimen or per row group - the list specimen already carried a
single caption for a multi-state row - so the Chip and TabBar rows take one each
rather than one per item, which would have printed "Raised" directly under a chip
that says "Raised".

**No empty-state caption was added.** The Design Brief specifies "No sample yet"
in secondary ink for an empty specimen slot; all eighteen controls are
implemented, so there is no empty slot to caption and inventing one would be
theatre.

**Images.** `docs/images/neu-{light,dark,high-contrast}.png` are real captures of
the running desktop head at scroll 2100 with `NEU_GALLERY_STATE=1`, so the press
signature is visible in the hero shot. `NEU_GALLERY_FOCUS` is deliberately *not*
set for them: it rewrites the caption under the focused specimen to
`Focus()=True state=Keyboard`, which is verification instrumentation and has no
business in a README.

### Verification 6 - an external consumer app

**A separate app outside this repo, `../steep`, scaffolded with
`dotnet new unoapp -preset blank`, referencing `Neu.Uno` by project and following
`README.md` verbatim.** A tea timer: header, two surfaces, ComboBox, Slider,
ProgressBar, ProgressRing, two buttons, a ListView and a ContentDialog. Almost no
control carries a `Style` - the app sets `UseImplicitStyles="True"` - so it also
tests the step 9 path from the outside.

**Result: the theme works from outside, and consuming it found two library bugs
and two documentation bugs that the gallery could not.** The gallery is inside the
repo and shares its `global.json`, its solution and its habits; it turns out that
hides things.

**1. The determinate ProgressRing assumed a 0-100 range.** The app's ring is
`Minimum="0" Maximum="1"` because it shows a fraction. It drew nothing at 70%.
The value converter had been told the range up front and defaulted to 0-100, so
it computed 0.7% and drew a hairline. Fixed by D12. Confirmed both ways: with
`Maximum="100"` the same app drew the arc correctly, and after the fix the 0-1
ring draws 70% correctly.

**Why the gallery missed it.** Its ring specimens are "Indeterminate",
"Determinate 65" and "Inactive", all on the control's default range. A specimen
sheet naturally uses defaults; an app naturally does not.

**2. A ring at its minimum drew a dot.** `StrokeDashCap="Round"` on a zero-length
dash paints a full-stroke dot, so an active ring at 0 showed about two percent of
progress that did not exist. The determinate arc is now `Flat`, which has no
extent at zero. The indeterminate arc keeps `Round` - its dash is never zero.

**Why the gallery missed it too.** Its only zero-progress specimen is *inactive*,
and the inactive state hides the arc outright. An active ring sitting at zero -
what every timer shows before you start it - was never on the sheet.

**3. `docs/overrides.md` gave the wrong `ms-appx` URI for a consumer's own
dictionary.** It showed `ms-appx:///MyApp/Theme/Palette.xaml`. The
assembly-qualified form is for dictionaries in a *library*; an app's own
dictionary is `ms-appx:///Theme/Palette.xaml`. The wrong form does not warn and
does not fall back - the resource fails during `Application` construction and **no
window is ever created**, which on a windowed desktop app looks like a silent
crash. Clean A/B: same file, same build, only the URI changed. The guide now says
which form is which and what the failure looks like. `ms-appx:///Neu.Uno/...` for
the high-contrast dictionary was verified correct from the consumer side in the
same pass.

**4. The guide still named `NeuProgressArcConverter`,** which D12 deleted.

**Verified working from the consumer, in both themes:** implicit styles reaching
stock markup; `ColorOverrideSource` with a partial palette (a green accent, the
one from the guide, which the contrast tool passes); `Neu.HighContrast.xaml`
merged over the whole app; and a `ContentDialog` constructed in C# picking up the
Neu template from the implicit style - a plain `ContentDialog`, not a subclass,
which is the case the implicit style does reach.

**One thing that was the app's fault, not the library's,** recorded because it
will happen to others: the app put `Background="{ThemeResource NeuCanvasBrush}"`
on its `Page` while scoping `RequestedTheme` to a `Grid` inside it. A theme scope
reaches descendants only, so in dark theme every control went dark and the canvas
behind them stayed light. The brush belongs on the same element that carries the
scope.

**Environment note.** Captures of this app were intermittently disturbed - a run
would come back with the timer started, the slider moved, or the window
minimised, none of which the harness does. The seed hook now sets every input it
reads and prints its own percentage into the caption, so a disturbed run is
obvious in the image rather than being mistaken for a rendering bug. It was
mistaken for one once, and cost a wrong reading of the ring fix.
