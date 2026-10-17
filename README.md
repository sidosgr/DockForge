# DockForge

DockForge is a Windows desktop toolbar application inspired by Linkbar, implemented in a new C# / WPF codebase under the MIT license.

**Current status: development build 0.1.4. This is not yet the fully verified release described in the product specification.** The implementation builds for x64 and ARM64. Local Windows 11 x64 checks are recorded in `docs/VERIFICATION.md`. Windows 10, ARM hardware, installer execution, and several shell interactions still need verification. Unsupported Explorer integrations are called out explicitly below.

## Download and install

Download **DockForge-Setup-0.1.4.exe** from the [0.1.4 development release](https://github.com/sidosgr/DockForge/releases/tag/v0.1.4). One installer includes both x64 and ARM64 and selects the native build automatically; a separate .NET installation is not required. The release also includes a SHA-256 checksum file.

Run the installer and follow the setup wizard. DockForge targets Windows 10 22H2 or later and Windows 11. This development installer is unsigned. Read the [verification record](docs/VERIFICATION.md) before relying on shell integration features.

## Features implemented

- Edge and floating bars, per-bar launcher/running-app/combined modes, independent or mirrored monitor placement.
- Live custom groups and linked folders, grid/list popups, filters, sorting, and manual ordering.
- Shell icons and installed-app selection, file/folder/web shortcuts, launch arguments, custom icons and labels.
- Running-window discovery, current virtual-desktop filtering, monitor filters, activation, normal close, thumbnail/list pickers, pinning, and MRU switching.
- Native appbar space reservations, auto-hide, floating reveal shortcuts, drag positioning and edge snap previews.
- Per-monitor Windows taskbar visibility, explicit restore commands, and a separate recovery-helper process.
- Central settings, first-run wizard, import/export, local backups, local diagnostics, and GitHub release checks.
- A single self-contained installer containing x64 and ARM64 builds, with per-user/all-user installation choices.

See [the full agreed specification](docs/SPECIFICATION.md) for all requested behavior and [verification notes](docs/VERIFICATION.md) for the distinction between implemented, tested, and outstanding work.

## User guide

Open [the complete illustrated guide](docs/HELP.html) in a browser. It is a single offline HTML file with search, light/dark themes, responsive navigation and printing styles. The app bundles the same file and links to it from a concise formatted Help tab.

## Build

Prerequisites: Windows, .NET 10 SDK, and Inno Setup 6.7 or later. A local `.tools/dotnet/dotnet.exe` is used when present; otherwise the installed `dotnet` command is used.

```powershell
./build.ps1 -InnoCompiler 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe'
```

To build both self-contained applications without installers:

```powershell
./build.ps1 -SkipInstallers
```

Outputs: `artifacts/publish/win-x64`, `artifacts/publish/win-arm64`, and the combined `artifacts/installers/DockForge-Setup-0.1.4.exe`. The installer selects the native architecture automatically.

## Run and test

```powershell
dotnet run --project tests/DockForge.Tests -c Release
dotnet run --project src/DockForge -c Release -- --sandbox --demo --settings
```

The `--sandbox` option uses a separate development profile and suppresses startup registration and Windows taskbar hiding. `--smoke-test` runs the integrated Windows checks with that profile and writes `smoke-results/results.txt` beside the executable. Do not ship a populated sandbox profile as user settings.

```powershell
DockForge.exe --sandbox --smoke-test
DockForge.exe --restore-taskbars
```

Normal settings and local logs live in `%LOCALAPPDATA%\DockForge`. Settings imports never copy or delete the actual files to which shortcuts point.

## Windows integration boundaries

- Windows publicly exposes recent/frequent Jump List destinations. Other applications' custom tasks and pinned Jump List entries are not available through that API. The current implementation reads recent destinations when an AppUserModelID is available.
- Existing applications send progress and overlay badges to Explorer. This build does **not** intercept or display those signals. The requested badge/progress feature remains outstanding; the saved preference is not evidence of support.
- Thumbnail desktop previews use a DWM thumbnail overlay. This is an approximation of Windows Peek, not Explorer's private Peek implementation. Minimized-window previews depend on DWM availability.
- Windows taskbar styling is recreated using native icons, theme colors and composition effects. Pixel-identical appearance across Windows builds has not been established.
- Installers are unsigned development artifacts. No signing certificate has been configured. In Settings, use `sidosgr/DockForge` as the release repository for update checks. The current 0.1.4 build is a prerelease; automatic checks target stable GitHub releases.

## Release preparation

The GitHub workflow runs core checks, builds both self-contained applications, and packages one combined installer with checksums. Workflow runs retain the installer as a downloadable artifact; publishing a release is a separate step. Before a stable release, complete the verification matrix and sign installers if a certificate is available. Automatic update checks notify and request permission to open a download; they never install silently.

## License and reference

MIT. The original reference is [Linkbar](https://github.com/ATGH15102AFMLD/Linkbar). No Linkbar source was copied into this implementation. The anvil/dock icon is supplied as an editable SVG and Windows ICO.
