# Override guide

Everything in Neu is a keyed resource, so nothing here requires forking the theme.
The pattern is always the same: **merge a dictionary after `NeuTheme` and restate
the keys you are changing.** Later wins.

```xml
<ResourceDictionary.MergedDictionaries>
  <neu:NeuTheme />
  <ResourceDictionary Source="ms-appx:///Theme/MyOverrides.xaml" />
</ResourceDictionary.MergedDictionaries>
```

**Mind the URI.** A dictionary in *your app* is `ms-appx:///Theme/MyOverrides.xaml`
with no assembly segment. The assembly-qualified form is for dictionaries in a
*library* - `ms-appx:///Neu.Uno/Theme/Neu.HighContrast.xaml` is right because it
comes from this package. Writing `ms-appx:///MyApp/Theme/...` for your own file
does not fall back and does not warn: the resource fails to load during
`Application` construction and **no window is ever created**, which looks like a
crash with no error, because a desktop Uno app is windowed and has nowhere to
print one.

Colours are the one exception with a dedicated hook, because they are the thing
people change first - see below.

---

## Colours

Pass your dictionary to `ColorOverrideSource`. It is merged after the Neu
defaults, so you restate only the keys you are changing:

```xml
<neu:NeuTheme ColorOverrideSource="ms-appx:///Theme/Palette.xaml" />
```

`Palette.xaml` takes the same `ThemeDictionaries` shape as `Tokens.Colors.xaml`.
Give it all three sections - `Light`, `Dark` and `Default` - or the theme you
leave out keeps the Neu value:

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <ResourceDictionary.ThemeDictionaries>
    <ResourceDictionary x:Key="Light">
      <Color x:Key="NeuAccent">#6FAE99</Color>
      <SolidColorBrush x:Key="NeuAccentBrush" Color="#6FAE99" />
    </ResourceDictionary>
    <ResourceDictionary x:Key="Dark">
      <Color x:Key="NeuAccent">#85C0AB</Color>
      <SolidColorBrush x:Key="NeuAccentBrush" Color="#85C0AB" />
    </ResourceDictionary>
    <ResourceDictionary x:Key="Default">
      <Color x:Key="NeuAccent">#6FAE99</Color>
      <SolidColorBrush x:Key="NeuAccentBrush" Color="#6FAE99" />
    </ResourceDictionary>
  </ResourceDictionary.ThemeDictionaries>
</ResourceDictionary>
```

### Changing the surface means changing the shadows

The depth illusion is the surface lit from the top left, and `NeuShadowDark` /
`NeuShadowLight` are **independent tokens** - they do not derive from
`NeuSurface`. Restate the surface alone and the shadows keep the hue of the
palette you left behind.

Measured on a cool slate re-skin (`#DDE3EA`), sampling the darkest shade band
and the canvas from the rendered window:

| Palette | Canvas R-B | Darkest shade R-B |
|---|---|---|
| Shipped plaster | +13 (warm) | +16 (warm) |
| Slate, surface only | **-13 (cool)** | **+1 (neutral)** |
| Slate, surface and shadows | -13 (cool) | -17 (cool) |

The rule the shipped palette follows, and the one to keep: **the shade carries
the same hue direction as the surface.** Leave `NeuShadowDark` behind and the
shade lands neutral under a cool surface, which reads as grey dirt at the edge of
every control rather than as the surface in shadow. Nothing warns you - it still
renders, it just stops looking deliberate.

A surface change is therefore a five-key edit at minimum: `NeuCanvas`,
`NeuSurface`, `NeuShadowDark`, `NeuShadowLight`, and the two inks re-picked
against the new surface for contrast.

**Every colour has two keys, a `Color` and a `SolidColorBrush`.** Templates bind
brushes; the shadow collections need raw `Color`. Restate both or you will change
half the theme - the fills move and the shadows do not.

`Default` mirrors `Light`. It is what resolves when no theme is requested, so
leaving it out is the usual cause of "my override works until I toggle the theme".

### Colour keys

| Key | Light | Dark | Role |
|---|---|---|---|
| `NeuCanvas` | `#E9E3DC` | `#2C2B29` | the page behind everything |
| `NeuSurface` | `#E9E3DC` | `#2C2B29` | control fill - same colour as the canvas, on purpose |
| `NeuShadowLight` | `#FFFFFF` | `#3D3B38` | the highlight of every pair |
| `NeuShadowDark` | `#B9AFA4` | `#151412` | the shade of every pair |
| `NeuInk` | `#1F1F1E` | `#ECE7E1` | text and glyphs |
| `NeuInkSecondary` | `#5F5A55` | `#A39D96` | captions and hints |
| `NeuAccent` | `#C99436` | `#D9A441` | the one filled action per view; checked and selected markers |
| `NeuAccentHover` | `#CB983E` | `#DBA84A` | accent under the pointer |
| `NeuAlert` | `#A64134` | `#E5786A` | semantic only - error rings and message ink |
| `NeuOnAccent` | `#1F1F1E` | `#1F1F1E` | ink on the amber fill. **Does not flip** |
| `NeuOnAlert` | `#ECE7E1` | `#1F1F1E` | ink on the red fill |
| `NeuSurfaceHover` | `#EDE8E2` | `#31302E` | surface under the pointer |
| `NeuScrim` | `#66000000` | `#99000000` | behind a dialog. The only token with alpha |

Two opacities travel with the shadow colours and are theme-scoped the same way:
`NeuShadowLightOpacity` (`0.72` / `0.55`) and `NeuShadowDarkOpacity`
(`0.55` / `0.70`).

`NeuOnAccent` being dark in both themes is deliberate and is the one place the
palette breaks its own symmetry: the amber is nearly as light in dark theme as in
light, so the dark theme's own ink on it measures 1.83:1. The reasoning is D6 in
[`decisions.md`](decisions.md).

**That constrains the accent more than it looks.** `NeuAccent` is a fill with
`NeuOnAccent` sitting on it, and that pair owes 4.5:1 in *both* themes - so the
accent has to stay light enough for dark ink in both, which is why the greens
above are mid-light rather than the obvious deep green. A mid-tone fails twice
over: `#3F7A6B` with the shipped dark ink measures 3.31:1, and swapping
`NeuOnAccent` to the light ink only moves it to 4.06:1. Either keep the accent
light and the ink dark, as here, or take it genuinely dark and restate
`NeuOnAccent` light - the middle is where both fail. Run the check.

### After changing a colour

```powershell
pwsh tools/contrast-check.ps1
```

It measures every ink/surface pair in both themes against the floors in
`CLAUDE.md` - `NeuInk` at 7:1 light and 10:1 dark, `NeuInkSecondary` and
`NeuAlert` at 4.5:1 - and exits non-zero on a failure.

It reads the hex values straight out of a token file, so by default it only sees
the library's palette, not yours. Point it at your override dictionary to check
the palette you actually ship:

```powershell
pwsh tools/contrast-check.ps1 -TokensPath src/MyApp/Theme/Palette.xaml
```

That works when your file restates the full set of ink and surface keys - the
easiest way is to copy `Tokens.Colors.xaml` and edit it. A file that restates only
a few keys gives the script no pair to build; it says so and exits non-zero rather
than reporting a pass on nothing. The success line names how many pairs it
actually measured, and ten is the full set:

```
All enforced contrast floors met (10 pair(s) checked).
```

---

## Metrics, radii and spacing

These are theme-invariant - one flat dictionary, no `ThemeDictionaries` - so an
override file for them is just a list of keys:

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <CornerRadius x:Key="NeuRadiusMd">8</CornerRadius>
  <x:Double x:Key="NeuRadiusMdValue">8</x:Double>
</ResourceDictionary>
```

| Group | Keys |
|---|---|
| Depth offsets | `NeuDepth{Sm,Md,Lg}Offset`, `NeuDepth{Sm,Md,Lg}OffsetNegative` |
| Depth blur | `NeuDepth{Sm,Md,Lg}Blur` |
| Depth spread | `NeuDepth{Sm,Md,Lg}Spread` |
| Rings | `NeuFocusRingSpread`, `NeuFocusRingBlur` |
| High contrast | `NeuHighContrastOutlineSpread`, `NeuHighContrastRingSpread`, `NeuHighContrastBlur` |
| Radius | `NeuRadius{Sm,Md,Lg,Pill}` and `NeuRadius{Sm,Md,Lg,Pill}Value` |
| Spacing | `NeuSpace{3,5,8,13,21,34}` |
| Misc | `NeuInkDisabledOpacity`, `NeuTouchTargetMin` |
| Motion | `NeuEaseSmooth`, `NeuDurationFast`, `NeuDurationNormal`, `NeuPressScale` |

Every radius has a `CornerRadius` and a matching `x:Double` companion, for
templates that need the number rather than the struct. Restate both.

**Changing a depth offset does not change the depth.** The shadow collections
resolve these by `StaticResource` at parse time, inside `Tokens.Shadows.xaml`,
which is merged before your override file - so the collections are already built
from the old numbers. To change depth, restate the collection (below).

---

## The depth collections

Restating a collection is the highest-leverage override in the library: templates
reference depth only by name, so one dictionary reaches every control at once,
including controls you wrote yourself following the same anatomy.

The collections live in `ThemeDictionaries`, one copy per theme, because that is
what the Toolkit requires for theme switching - a single collection shared across
themes with a `ThemeResource` colour does not re-resolve. Restate them the same
way:

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:utu="using:Uno.Toolkit.UI">
  <ResourceDictionary.ThemeDictionaries>
    <ResourceDictionary x:Key="Light">
      <!-- Half the blur, for a crisper slab. -->
      <utu:ShadowCollection x:Key="NeuRaisedMd">
        <utu:Shadow OffsetX="6" OffsetY="6" BlurRadius="6" Spread="0"
                    Color="{StaticResource NeuShadowDark}"
                    Opacity="{StaticResource NeuShadowDarkOpacity}" />
        <utu:Shadow OffsetX="-6" OffsetY="-6" BlurRadius="6" Spread="0"
                    Color="{StaticResource NeuShadowLight}"
                    Opacity="{StaticResource NeuShadowLightOpacity}" />
      </utu:ShadowCollection>
    </ResourceDictionary>
    <!-- and the same for Dark and Default -->
  </ResourceDictionary.ThemeDictionaries>
</ResourceDictionary>
```

`{StaticResource NeuShadowDark}` inside a theme dictionary resolves against that
theme, which is verified behaviour on this stack, not an assumption - see
Verification 5 in [`decisions.md`](decisions.md).

Rules that keep a restated collection looking like Neu rather than like a drop
shadow:

- **Exactly two shadows, dark first.** Order matters, later overlaps earlier, so
  the highlight has to be second or the shade prints over it.
- **Symmetric offsets.** `(+d, +d)` and `(-d, -d)`. One light source, top left.
- **Inset collections set `IsInner="True"` on both** shadows, not one.
- **Offset stays under the gap** between sibling containers. `NeuRaisedMd` has an
  offset of 6, so siblings need at least 13 between them, or they print their
  shade on each other. Raise the offset and you have to raise the gaps too.
- **Blur is the Skia cost.** If a long list stutters, drop the blur before you
  drop the shadow.

The full set of keys: `NeuRaisedSm`, `NeuRaisedMd`, `NeuRaisedLg`, `NeuInsetSm`,
`NeuInsetMd`, `NeuFlat`, `NeuFocusRing`, `NeuAlertRing`.

`NeuFlat` is an empty collection. It is what "no depth" means, and templates swap
to it rather than clearing `Shadows`, so restating it is how you make "flat" mean
something else - which is exactly what the high-contrast dictionary does.

---

## Type

The ramp is sizes, weights and line heights - no font families, because a theme
library is the wrong layer to ship typefaces.

| Style | Size / line | Weight |
|---|---|---|
| `NeuDisplay` | 34 / 40 | SemiBold |
| `NeuTitle` | 21 / 28 | SemiBold |
| `NeuBody` | 16 / 24 | Normal |
| `NeuLabel` | 13 / 16 | Medium |
| `NeuData` | 13 / 16 | Normal |

Each has `Neu<Role>FontSize` and `Neu<Role>LineHeight` keys behind it, so you can
move one size without restating the style.

To give the theme a typeface, set `FontFamily` in your own implicit `TextBlock`
style or on the page root - the Neu styles do not set it, so it inherits.

`NeuLabelCharacterSpacing` exists and is **inert on the Skia text stack**:
`TextBlock.CharacterSpacing` measures to zero effect there. The token is kept so
the intent is recorded, not because it does anything.

---

## Control styles

Every control style is keyed. To adjust one, base a style on it:

```xml
<Style x:Key="MyWideButton" TargetType="Button" BasedOn="{StaticResource NeuButtonStyle}">
  <Setter Property="MinWidth" Value="144" />
</Style>
```

To change a control everywhere, restate the implicit entry after `NeuTheme`:

```xml
<Style TargetType="Button" BasedOn="{StaticResource NeuButtonStyle}">
  <Setter Property="MinWidth" Value="144" />
</Style>
```

### Keys

| Control | Key |
|---|---|
| Button | `NeuButtonStyle`, `NeuAccentButtonStyle`, `NeuPillButtonStyle` |
| ToggleButton | `NeuToggleButtonStyle` |
| CheckBox / RadioButton | `NeuCheckBoxStyle`, `NeuRadioButtonStyle` |
| ToggleSwitch / Slider | `NeuToggleSwitchStyle`, `NeuSliderStyle` |
| ProgressBar / ProgressRing | `NeuProgressBarStyle`, `NeuProgressRingStyle` |
| TextBox / PasswordBox | `NeuTextBoxStyle`, `NeuPasswordBoxStyle` |
| NumberBox / ComboBox | `NeuNumberBoxStyle`, `NeuComboBoxStyle`, `NeuComboBoxItemStyle` |
| List | `NeuListViewStyle`, `NeuListViewItemStyle` |
| Card | `NeuCardStyle`, `NeuCardClickableStyle` |
| Chip / TabBar | `NeuChipStyle`, `NeuTabBarStyle`, `NeuTabBarItemStyle` |
| Expander / Dialog | `NeuExpanderStyle`, `NeuContentDialogStyle` |

Sub-part styles - `NeuSliderThumbStyle`, `NeuNumberBoxSpinButtonStyle`,
`NeuPasswordRevealButtonStyle`, `NeuTextBoxDeleteButtonStyle`,
`NeuExpanderHeaderStyle` - are public and overridable, but each is applied **by
name inside the template that owns it**, so restating one changes that part
everywhere it appears and nothing else. They are not in the implicit dictionary
for that reason.

### Writing a template of your own

If you retemplate a control and want it to sit with the rest:

```xml
<utu:ShadowContainer x:Name="Surface"
                     Background="{ThemeResource NeuSurfaceBrush}"
                     CornerRadius="{ThemeResource NeuRadiusMd}"
                     Shadows="{ThemeResource NeuRaisedMd}">
  <Grid>
    <ContentPresenter ... />
  </Grid>
</utu:ShadowContainer>
```

Four things that will cost you an afternoon otherwise:

1. **The surface brush goes on the `ShadowContainer`, and the child carries no
   `Background` or `Fill`.** Inner shadows do not paint otherwise - the child
   covers them.
2. **Reference depth by name.** `Shadows="{ThemeResource NeuRaisedMd}"`, never an
   inline `<utu:Shadow>`. That is what makes the high-contrast swap and any
   collection override reach your control.
3. **A `ShadowContainer` with an explicit `Width`/`Height` does not stretch its
   child,** and the shadow follows the *child*. Give the child the same explicit
   size or the shadow is drawn around whatever the content happens to measure - an
   empty child gets no shadow at all. A container that fills a Grid cell does
   stretch, so most templates need no sizes.
4. **Swap depth from a `VisualState` with a setter**, not from code:
   `<Setter Target="Surface.Shadows" Value="{ThemeResource NeuInsetSm}" />`. This
   is verified on the Skia heads (D3).

And one design rule worth keeping: **state is never depth alone.** Pair every
depth change with a second channel - a glyph, an ink change, a ring, a bar - or
the control disappears under high contrast, where all six collections are the same
outline.

---

## High contrast

```xml
<ResourceDictionary.MergedDictionaries>
  <neu:NeuTheme />
  <ResourceDictionary Source="ms-appx:///Neu.Uno/Theme/Neu.HighContrast.xaml" />
</ResourceDictionary.MergedDictionaries>
```

It restates the six depth collections as a single 1px `NeuInk` outline and widens
the two rings from spread 2 to 3. Nothing else - no colour, no template.

Nothing reads the OS high-contrast setting; merging is your call, and you can
merge it conditionally at startup like any other dictionary.

The outline is an **inner** shadow. Drawn outward it needs a pixel of layout slack
on every side and silently loses the edges that have none - which showed up as an
Expander body with sides and corners but no top or bottom. If you write a
high-contrast variant of your own, keep it inner.

---

## What you cannot override

- **Silhouettes.** `ShadowContainer` mimics its content by size and corner radius.
  Rounded rectangles and pills, nothing else.
- **`Opacity` on depth.** A `ShadowContainer` paints its background and shadows on
  its own canvas, outside the XAML opacity pipeline. Fading a subtree fades the
  content and leaves the surface at full strength. Swap the brush or the
  collection instead.
- **`NeuVisibilityConverter`** is internal. It works around two controls and is
  not part of the library's surface.
- **`NeuProgressArc`** is public - the attached property that draws the
  determinate ring. It reads the ring's own `Minimum`, `Maximum` and `Value`, and
  takes its geometry from the shape it is attached to, so a resized ring needs no
  configuration. Give that shape `StrokeDashCap="Flat"`: a round cap on a
  zero-length dash paints a dot, so a ring at its minimum would show progress it
  does not have.
