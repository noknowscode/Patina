# Patina.Blazor

A second UI for Patina, built with .NET MAUI Blazor Hybrid (desktop and mobile) and Blazor Server (web), sharing the
untouched `../Patina.Core` with the Uno Platform app. The point is the comparison: same domain, same rules, same
69 tests, two UI stacks. `COMPARISON.md` is the running log.

No WebAssembly anywhere: the MAUI head runs Razor components natively in a `BlazorWebView`, the web head renders
over a SignalR circuit.

## Projects

| Project | Role |
|---|---|
| `Patina.Blazor.Shared` | Razor Class Library: pages, components, CSS, the platform seam interfaces. References `Patina.Core`. |
| `Patina.Blazor.Maui` | MAUI host (`BlazorWebView`) for Windows, Android, iOS, Mac Catalyst. Implements the seams with MAUI Essentials. |
| `Patina.Blazor.Web` | Blazor Web App, `InteractiveServer` render mode. Implements the seams server-side. |
| `Patina.Blazor.Shared.Tests` | NUnit tests for the Shared services. |
| `../Patina.Core`, `../Patina.Tests` | Shared with the Uno app, unchanged. |

Package versions are managed centrally in the repo root `Directory.Packages.props`.

## Build, run, test

```bash
dotnet test ../Patina.Tests/Patina.Tests.csproj
dotnet test Patina.Blazor.Shared.Tests/Patina.Blazor.Shared.Tests.csproj

# Web (Blazor Server)
dotnet run --project Patina.Blazor.Web

# Windows desktop
dotnet build Patina.Blazor.Maui/Patina.Blazor.Maui.csproj -f net10.0-windows10.0.19041.0 -t:Run

# Android (emulator or device attached)
dotnet build Patina.Blazor.Maui/Patina.Blazor.Maui.csproj -f net10.0-android -t:Run
```

Prerequisites: .NET SDK 10.0.300+ and `dotnet workload install maui`.

Data: MAUI keeps `patina.json` in the app data directory; the web host keeps it in `App_Data/` (override with
`Patina:DataFolder`). The web host has one collection per server, so "local-first" there means local to the server.

## Plan

1. Scaffold, Core referenced, tests green (done)
2. Seams: `IAppSettings` in Shared, `FileSystemDocumentFile` shared by both hosts, `Preferences` on MAUI, JSON file on the server, `AddPatinaCore` DI, smoke page over the store (done)
3. Theme: `tools/tokens_to_css.py` generates `css/tokens.css` from `../docs/DESIGN.md` and the DesignMd2Uno dictionaries; IBM Plex; grade colours; motion (done)
4. Components: `ConditionStrip`, `TombstoneLabel`, `StateView`, `StoreComponent`, `ConfirmDialog`, `PhotoStrip` (done)
5. Shell: router, rail / bottom tabs, `tools/resw_to_resx.py` strings EN/FR (done)
6. Collection, Artwork, Survey, Queue, Treatment, Settings (done)
7. Photos (`MediaPicker` / `InputFile`), export / import, theme and language switching (done)
8. Map (Leaflet via JS interop) (done)

Verified on the Blazor Server head: every route renders with the sample collection, EN and FR, light and dark;
21 bUnit/NUnit tests cover the survey, treatment, queue, settings and service paths. The MAUI head is built and
run on Windows (see Build, run, test); Android and iOS follow the same `BlazorWebView` and are untested here.

## Regenerating

```bash
python tools/tokens_to_css.py     # after docs/DESIGN.md or Patina/Styles/*.xaml change
python tools/resw_to_resx.py      # after tools/make_resw.py has rewritten Patina/Strings/*/Resources.resw
```
