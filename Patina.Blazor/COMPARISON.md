# Uno Platform vs MAUI Blazor Hybrid: Patina

Running log. Uno baseline figures are from the shipped v1.0.0 (`../SPEC.md`, `../HANDOFF.md`). Blazor figures fill
in as each step lands. Lines are physical lines of UI-layer source (XAML + models + controls + services + styles
for Uno; `.razor` + `.razor.cs` + `.css` + services for Blazor); Core is excluded on both sides because it is shared.

## Scope parity

| Capability | Uno | Blazor | Notes |
|---|---|---|---|
| Collection list, search, filters, stats | done | done | `Collection.razor`, bUnit-tested |
| Map with condition-coloured pins | done (Mapsui 5.1) | done (Leaflet 1.9.4) | JS interop, OSM tiles muted by CSS filter |
| Artwork dossier | done | done | split layout at Wide |
| Survey with findings, photos, live grade, draft | done | done | autosave on every finding/photo change, bUnit-tested |
| Treatment queue and detail workflow | done | done | stepper, validation, log; bUnit-tested |
| Settings: surveyor, theme, reduced motion, language | done | done | theme/motion as `<html>` attributes |
| Export / import `.patina` archive | done | done | same zip format; download (web) / share sheet (MAUI); import via `InputFile` |
| Camera capture | Android, iOS | MAUI `MediaPicker` (incl. Windows); web `InputFile capture` | one seam, `IPhotoCapture` |
| EN / FR | done (`.resw`, 222 strings) | done (`.resx` generated from the `.resw`, 231 strings) | `IStringLocalizer<Strings>`; 9 extra keys for names Uno hard-coded in XAML |
| Light / dark | done | done | `prefers-color-scheme` + explicit `data-theme` |
| Heads | desktop (Win/mac/Linux), WASM, Android, iOS | Windows, Mac Catalyst, Android, iOS (MAUI), Blazor Server | Blazor Server verified; MAUI heads need the Windows build |

## Size per screen (UI-layer lines)

| Screen | Uno | Blazor |
|---|---|---|
| Shell / Main / nav | 124 + Shell + ShellModel | 169 (two layouts + css) |
| Collection | 212 xaml + CollectionModel (54) | 165 |
| Artwork | 192 xaml + ArtworkModel (28) | 153 + 22 (row) |
| Survey | 226 xaml + 299 model | 364 |
| Queue | 146 xaml + QueueModel (30) | 98 |
| Treatment | 172 xaml + 90 model | 172 |
| Settings | 124 xaml + 149 model | 191 |
| ConditionStrip / TombstoneLabel | 103 + ~40 | 21 + 28 |
| Map control | 207 + 68 (tile source) + 62 (callout) | 97 + 25 (callout) + 38 (page) + 60 (js) |
| Platform services | 182 + 101 + 49 + 37 | 438 shared (document file, settings, photos, transfer, text) + 249 host (MAUI capture/share/webview, web download/program) |
| Styles / tokens | 7 dictionaries | 204 css + 154 generated css |
| **Total UI layer** | **~2,950** | **~2,920** (plus 222 lines of generators, 470 of tests) |

## Framework-supplied vs hand-built

| Concern | Uno | Blazor |
|---|---|---|
| Async list states (loading / empty / error / retry) | `FeedView` templates | hand-built `StateView` (57 lines) |
| Responsive breakpoints | `{utu:Responsive}` + `AdaptiveTrigger` | CSS media queries |
| Design tokens | generated XAML dictionaries | generated CSS custom properties |
| Navigation with data | `DataViewMap`, regions | Router + route parameters |
| Dialogs | `INavigator.ShowMessageDialogAsync` | hand-built `ConfirmDialog` (73 lines) |
| Localization | `Uno.Extensions.Localization` | `IStringLocalizer` |
| Theme switch | `IThemeService` | `data-theme` attribute |
| UI thread marshalling for pickers | window dispatcher (MVUX commands may be off-thread) | `MainThread.InvokeOnMainThreadAsync` for MediaPicker/Share; pickers are `InputFile`, no marshalling |
| Store change propagation | `Feed.AsyncEnumerable(store.Watch())` | hand-built `StoreComponent` base (117 lines) |
| Settings store | `ApplicationData.LocalSettings` | MAUI `Preferences`; JSON file on the server |

## Workarounds and gotchas hit

| # | Uno | Blazor |
|---|---|---|
| 1 | `AdaptiveTrigger` on cached pages (ResponsiveExtension reconnect bug, uno.toolkit.ui#1646) | none; CSS media queries have no reconnect problem |
| 2 | Dialog awaited result unreliable; `DialogAction` callbacks | none; own dialog returns a `Task<bool>` |
| 3 | `UISettings.AnimationsEnabled` always true on Skia desktop; in-app reduced-motion toggle | `prefers-reduced-motion` works; the in-app toggle is kept for parity |
| 4 | `Data.X` TwoWay inside FeedView; bind top-level states | Razor `Param="Name"` on a string parameter is the literal text, needs `@Name` (cost one debug cycle) |
| 5 | Variable fonts render default instance on Skia; static TTFs | none; same static TTFs via `@font-face` |

## Runtime

| Metric | Uno | Blazor |
|---|---|---|
| Cold start, Windows desktop | | |
| Cold start, Android emulator | | |
| Web first load | 7.5 s (WASM, headless Edge) | prerendered HTML on the first request; measure on Matt's machine |
| Package size, Windows | | |
| Package size, Android | | |

## Observations

- Steps 3–8: the whole UI layer landed at roughly the same size as the Uno one (~2,920 vs ~2,950 lines), with the bulk shifting from XAML + models to Razor + CSS. The three things MVUX/Toolkit supplied for free (FeedView states, store watching, dialogs) cost 247 hand-written lines in Blazor; the responsive layout cost nothing beyond media queries.
- Step 3: both UIs now read one design source. `tools/tokens_to_css.py` turns the DesignMd2Uno dictionaries into CSS custom properties, `tools/resw_to_resx.py` turns the Uno string table into `.resx`. Changing a colour or a string in the Uno pipeline flows to Blazor on regeneration.
- Step 6: Blazor Server prerendering means every page renders its data on the first HTTP response; the circuit then takes over. Pages that write (Survey) read the store once; pages that read derive from `StoreComponent` and re-render on every commit.
- Step 7: photo picking needs no platform seam at all in Blazor, because `<InputFile>` opens the native picker inside a BlazorWebView. Only camera capture (MAUI `MediaPicker`) and export (share sheet vs download) stay host-specific. Photos are served at `photos/` by both hosts (a composite `IFileProvider` on MAUI, static files on the server), so the `img` markup is identical.
- Step 8: Leaflet via JS interop replaced Mapsui at about half the lines; the muted tiles became a CSS filter instead of a colour matrix on the tile source.
- Testing: bUnit renders the real pages over the real service graph in-process, so the Survey submit path (finding → grade → treatment proposed) and the Treatment stepper are covered by 21 component/service tests without a browser. The Uno app covers the same rules in Core only.
- Gotcha worth remembering: `Click()` in bUnit returns before an async handler finishes its store write; assertions after a click need `WaitForAssertion`. Same shape as MVUX commands running off-thread.

- Step 2: the document file is one plain-.NET class shared by both hosts (only the folder differs). The Uno version depends on `Windows.Storage`, so it lives in the app head; it is the same 37 lines either way.
- Step 2: Blazor Server forces a storage decision the Uno app never faced: there is no per-device store, so the web head owns one collection per server. Theme and language also move into `IAppSettings` because Blazor has no framework service for either.
- Step 2: the Core store, seed and queries ran unchanged on the server head at the first request (18 artworks, 10 needing attention), which is the whole point of the shared Core.

