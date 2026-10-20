# DockForge verification record

Date: 2026-10-17. Local host: Windows 11 x64, build 26300, two 2560 × 1440 displays. SDK: .NET 10.0.402.

This record is deliberately separate from the product specification. Compiling a feature is not proof of its interaction behavior on every supported system.

## Passed locally

The application compiles with zero warnings and zero errors.

Core checks cover:
- Negative-coordinate monitor geometry and vertical placement.
- Bottom-edge full-width layout and floating-position clamping.
- Overflow length clamping, one-bar-per-edge validation, and multiple floating bars.
- Rejecting floating workspace reservations and nested groups.
- Deep settings snapshots, configuration round trips, atomic save/load, eight-backup retention, and preserving settings after an invalid import.

Integrated Windows checks cover:
- Display discovery and hardware monitor identifiers.
- Mirrored top-edge, vertical-edge and floating bars on both connected displays.
- Floating activation-tab hide/reveal dimensions.
- Immediate linked-folder contents, shortcut/extension filters and manual ordering.
- Group popup, central settings, setup wizard and window picker opening.
- Installed-app enumeration and Windows shell icon extraction.
- Recovery helper readiness handshake and termination after its monitored parent exits.
- Settings snapshot restoration.

Computer-use checks on the real Windows UI additionally verified:
- Creating an empty custom group, adding 7-Zip through the installed-app picker, and persisting items with Apply.
- Launching 7-Zip from its pin, merging its running window with the pin across monitors, and closing that test window through its menu.
- Cancel discarding an unsaved duplicated bar.
- Dragging the 7-Zip pin before Web moves the intended pin and persists the order. Fixed drag initiation so only the button that received the press may start the drag; periodic refresh no longer replaces buttons during a press. The same source guard was applied to group items; group drag remains to be exercised.
- Starting a second sandbox instance with --settings opens Settings in the existing instance.
- Live icon-size changes resize the actual bar; Cancel restores its previous size.
- The Settings preview now updates when icon size changes (28 to 40 tested), and its website/group symbols have readable contrast on the dark background.
- Hide on secondary displays selects Display 2 and leaves the primary unchecked. This was tested in sandbox mode; actual Windows taskbar visibility was not changed.

The current build completes 23 core assertions and 103 integrated Windows checks. Initial re-entrant popup closing and drag-source issues found during testing were fixed. The sandbox-only --ui-test flag exposes bars to the computer-use window inventory; normal production bars remain tool windows.

## Outstanding execution tests

- Real Windows taskbar hiding/restoration on each monitor, crash restoration of hidden Explorer taskbars, and Explorer restart behavior. The helper lifecycle test used an empty recovery journal; it does not establish end-to-end taskbar restoration.
- Installer installation, upgrade and uninstallation in current-user and all-user modes.
- Windows 10 22H2 execution and ARM64 hardware execution.
- Mixed DPI, monitor unplug/replug, work-area reservation conflicts, sleep/resume, virtual-desktop switching and fullscreen games.
- Remaining drag/drop workflows: group reorder, cross-bar movement, file-to-app opening, drag-hover activation, dock/undock gestures and locking. Basic pin reordering passed.
- App switching/minimizing, middle-click closing, live thumbnail contents and desktop preview restoration against controlled fixture windows. Launch and menu-close passed with 7-Zip.
- File context-menu extension commands and recent Jump List activation.
- Keyboard-only operation, screen-reader behavior, theme/high-contrast changes and shortcut-conflict presentation. Live size cancellation passed.
- GitHub update download flow after a real repository and release exist.

Computer-use testing resumed successfully in the subsequent authorized turn. All interaction tests above used the isolated sandbox profile.

## Known implementation limits and follow-up work

- Progress indicators and app notification badges from Explorer are not implemented. No supported general observation API was found; no injection hook was added.
- Jump Lists currently expose recent destinations where an application identity is available. Custom tasks and pinned destinations are not exposed by the public API used here.
- Desktop preview is a DWM overlay approximation. Native Explorer Peek parity is unverified.
- Shortcuts that conflict with another application are logged. The settings UI validates syntax and duplicate DockForge assignments, but needs a clearer actionable conflict result for externally occupied shortcuts.
- Visual refinement remains: full Windows 10/11 taskbar parity and bar overflow-arrow visibility. Settings now uses inherited Fluent control templates and system light/dark mode.
- Code review and additional integration fixes are still required before calling the full agreed feature set complete.

## Evidence locations

- `src/DockForge/bin/Release/net10.0-windows/smoke-results/results.txt`
- `src/DockForge/bin/Release/net10.0-windows/smoke-results/bar.png`
- `src/DockForge/bin/Release/net10.0-windows/smoke-results/group.png`
- `src/DockForge/bin/Release/net10.0-windows/smoke-results/settings.png`
- `src/DockForge/bin/Release/net10.0-windows/smoke-results/wizard.png`

These images are internal WPF render captures from synthetic test content. They do not verify DWM composition or capture the full Windows desktop.


## 0.1.1 visual refresh

- Removed the native frame from running-window previews. Each card uses an application icon, trimmed window title, unobtrusive close control and live thumbnail.
- Preview overflow uses page controls; no ScrollViewer or scrollbar is present. The standard and maximum popup bounds are checked.
- Settings inherits the system Fluent theme, including dark controls and title bar. Verified on this PC with AppsUseLightTheme=0 and SystemUsesLightTheme=0.
- Settings fields reflow from two columns to one below 620 logical pixels of content width. Appearance, Colors and Auto-hide have separate compact pages. Item editor fields also reflow.
- Real UI observation confirmed the standard placement page fits without scrolling; automated checks confirmed Appearance fits without scrolling and a 720 by 650 window reflows while retaining its footer.
- The live 7-Zip thumbnail was observed through computer use: no caption bar or scrollbar, readable title and app icon, dark surface and flat close control.
- A single version 0.1.1 setup bundles x64 and ARM64 payloads and chooses the architecture at installation. Installer execution and ARM64 hardware remain unverified.

## 0.1.2 alignment and auto-hide corrections

- Start/Center/End now align icon contents within the bar, in addition to positioning shorter bars along their display edge.
- Windows checks verified icon alignment on all four edges of both connected displays, including centered full-height vertical bars.
- Hidden edge bars are actually hidden native windows, removing their borders, background and DWM shadows. Edge reveal remains cursor-polled across their configured span.
- The optional activation-area guide is visible only while Settings is open. Tests verified guide appearance, thin width and immediate removal after Settings is hidden.
- Edge bars slide with 180 ms cubic easing, using a native clip region to avoid spilling onto adjacent monitors. Reversals begin from the current progress. Windows' disabled animation preference is respected.
- Integration checks verified intermediate animation progress, final visibility, reversal and floating-tab behavior. Four core checks verify clip geometry stays inside each edge's bar.
- Overflow arrows now appear only when needed. Symmetric internal spacing keeps centered icons centered independently of the drag handle. A real Windows UI capture confirmed the icon cluster is centered in a full-height left bar.
- An initial runtime test caught an unsupported Window clipping property; native region clipping is used instead. The corrected build passed the integration suite.
- Distribution: one architecture-selecting DockForge-Setup-0.1.2.exe. Windows 10, ARM64 hardware and installer execution remain outside the verified host coverage.

## 0.1.3 animation artifact correction (2026-10-18)

Reviewed the supplied phone video, including successive frames around the first reveal. It shows a bright striped surface before the icons arrive. The likely race was in the 0.1.2 animation path: native window-region mutations and retained WPF transforms were updated separately, and the native backdrop was shown before the first clip update.

- Removed the SetWindowRgn animation path entirely.
- A temporary per-pixel-transparent WPF presentation now moves a rendered image of the bar inside a fixed clipped viewport. Content and clipping use the same rendering pipeline. Native blur remains on the resting bar.
- Reveal prepares the real bar while hidden. The presentation remains over it for a short handoff after it is shown, avoiding an exposed unprepared native render target.
- Hide waits for the presentation's first painted frame before hiding the resting bar. Reversals reuse the current presentation; obsolete completion callbacks cannot close a newer transition.
- Regression checks sample 0%, 5%, 25%, 50% and 100% frames on all four edges. A dark fixture must contain no unexpected bright pixels and have the expected opaque area. The initially hidden frame must be transparent.
- A runtime check verifies the native backdrop stays hidden during a composited reveal. Existing guide, alignment, reversal, floating, settings and recovery checks still pass.
- Result: 18 core assertions and 87 Windows integration checks; zero build warnings/errors. Sample frame PNGs are stored in smoke-results/animation-left-*.png.

These render checks verify the composited visual contents and transition state on the local Windows 11 host. They are not an independent high-speed GPU capture of the affected PC and do not establish behavior on every GPU, driver or mixed-DPI configuration.

## 0.1.4 groups, organization and documentation

- Icon-size edits reset thickness to a computed minimum in both directions. The thickness control stays synchronized, and the user can increase the value afterward. Extra spacing also receives adequate thickness.
- Groups use borderless, resizable panels with an in-content heading. Right-click Rename Group and F2 edit the heading inline; Enter commits and Escape cancels. Linked-folder renaming changes only the display label.
- Per-group item-label and shortcut-badge options are available in the group editor and heading menu. Labels default on; badge hiding defaults on. Standard desktop shortcuts use custom or resolved target icons. Internet shortcuts use an unbadged custom icon or website glyph; unresolved shortcuts fall back to neutral symbols.
- Hover previews close synchronously on app-icon MouseLeave. Explicit multi-window close pickers remain interactive after leaving the bar.
- Internal icon drags show an accent insertion marker. The midpoint chooses before/after; the center of a custom group is highlighted as an add-to-group target. Locked bars suppress the cue, and drag completion clears cues across bars.
- The bar menu contains one New submenu with eight creation choices and readable labels.
- Help is a formatted quick-start card layout. Its complete-guide button opens the bundled Help.html. docs/HELP.html is the identical single-file manual intended for app distribution and future repository reuse.
- The HTML guide contains eleven sections, local search, dark/light themes, responsive navigation, expandable troubleshooting and print styles. Browser checks passed for search, anchor navigation, expansion, both themes and 390-pixel viewport overflow. No remote asset references or broken internal anchors were found.
- Real Windows UI checks verified a borderless group, right-click inline rename from Work to Design tools, immediate label hiding, the formatted Help tab and the compact New submenu.
- Integration fixtures now use a dedicated smoke-profile store and a fresh folder per run, so repeated checks do not overwrite the ordinary sandbox setup or inherit stale shortcut fixtures.
- Result: 23 core assertions and 103 Windows integration checks, zero warnings/errors. The first repeat checks exposed stale-fixture and overly broad glyph-versus-label assertions; those test issues were corrected before accepting the final suite.

Evidence includes group-icons-only.png, bar-drop-cue.png and results.txt in smoke-results, plus artifacts/guide/guide-preview.png. GitHub setup and upload are deliberately deferred until requested after account/repository setup.

## 0.1.5 official updates and signing preparation

Date: 18 October 2026. The public repository is now https://github.com/sidosgr/DockForge.

- Update requests use the fixed official repository, including when loading older settings with an empty or custom UpdateRepository field. Existing CheckUpdates=false preferences remain disabled. There is no editable update repository field.
- The release list supports development previews (enabled by default) and a stable-only option. Drafts, current/older versions, unsupported architectures, incompatible asset names and download URLs outside the official repository are excluded. The combined installer is preferred, with a legacy architecture-specific fallback.
- The installed assembly supplies the displayed and compared version; update checks no longer use a separately hard-coded version string. Empty release lists are handled normally. Development sandbox startup does not make automatic update requests.
- 36 offline core assertions passed; two additional live GitHub feed checks also passed (38 total). The application compiled with zero warnings and zero errors.
- 107 isolated Windows integration checks passed. The update settings show sidosgr/DockForge, expose no repository edit field, apply the preview preference and restore it on Cancel. The rest of the existing bar/group/preview/recovery checks also passed. Evidence: artifacts/publish/win-x64/smoke-results/results.txt and settings-updates.png.
- The signing gate rejects unsigned payloads. Both proposed SignPath artifact XML files validate against the provider's published schema, and workflow PowerShell blocks pass syntax parsing. Metadata restrictions were checked against the actual app and Inno installer version resources.
- The maintainer submitted the SignPath Foundation application; the application page displayed “Form submitted” on 18 October 2026. Approval, a production signing identity and live signing execution remain pending. No real signed artifact has been produced or verified. The manual signing workflow remains disabled until onboarding is completed.
- The prepared signing workflow covers the six owned application binaries and the outer installer. Generated-uninstaller signing requires a separate integration; see docs/CODE-SIGNING.md. It must not be described as fully signed installation lifecycle coverage.

Windows 10, ARM64 hardware and the full installation/upgrade/uninstallation matrix remain unverified. The 0.1.5 installer is still an unsigned development build.

## 0.1.6 Windows XP bars and creator credit

Date: 21 October 2026.

- Added the per-bar Windows XP option with a blue Luna-style gradient, Tahoma labels, beveled running-app buttons and distinct hover, active, pressed and keyboard-focus states. Horizontal, vertical and floating bars share the appearance implementation with the live Settings preview and animation snapshots.
- XP shading follows the bar's thickness. Default corners are square and composition blur is disabled for XP; explicit background colors, corner radius and opacity remain configurable. High contrast bypasses decorative XP colors, although a real high-contrast Windows session has not been exercised.
- Added a small “Created by Sidos” line in Help, the offline guide footer, and the README attribution. Bars have no creator watermark.
- 38 core assertions passed, including persistence of the XP selection and compatibility of existing style identifiers. The application built with zero warnings and zero errors.
- 119 isolated Windows integration checks passed, including XP horizontal/vertical/floating shading, Tahoma/foreground/chrome, hover and active states, custom colors, switching back to System, live Settings selection and auto-hide/reveal palette restoration. Existing placement, drag, group, preview and recovery checks also passed.
- Visually inspected bar-xp-horizontal.png and settings-appearance-xp.png under src/DockForge/bin/Release/net10.0-windows/smoke-results. The README includes the actual rendered bar as docs/images/windows-xp-bar.png.

Windows 10, ARM64 hardware and the full installer lifecycle still need execution testing. Trusted signing remains pending onboarding; the development installer is unsigned.
