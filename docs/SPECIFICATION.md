# DockForge — agreed product specification

Status: implementation contract, not a claim of completed verification.

## Product and distribution
- New C# codebase inspired by Linkbar's desktop-toolbar behavior; MIT licensed.
- Windows 10 22H2 or later and Windows 11; x64 and ARM64 installer builds.
- Standard installer/uninstaller, current-user installation by default and optional all-users installation. Per-user settings. No portable release.
- English interface. No telemetry. Local rotating diagnostics and user-triggered export.
- Start at sign-in by default, user can disable it.
- GitHub Releases updates use sidosgr/DockForge. Check results, availability, progress and errors appear inline in the version area. Download and verify installers inside the app; installation starts only after the explicit Install update action, saving settings and closing DockForge after setup starts. Never silently install updates.
- Anvil with dock motif app icon. Offline Help in Settings.
- Full feature set requested before final handover; implementation and platform limitations must remain explicitly tracked.

## Bars and monitors
- Per-bar launcher, running-app taskbar, or combined mode.
- Left/right/top/bottom edge placement, one edge bar per edge per monitor. Any number of floating bars; horizontal or vertical.
- Independent bars or mirrored bars on selected monitors. Mirrored contents, appearance and behavior synchronized; local-monitor task lists reflect each instance's monitor.
- Disconnected monitor bars inactive until reconnection, with manual reassignment.
- Full edge length, fit contents or custom length; start/center/end alignment.
- Single row/column; mouse-wheel and arrow overflow scrolling.
- Reserve workspace, overlay, or auto-hide per edge bar. Floating overlay or auto-hide.
- Auto-hide edge activation only within the bar span; reveal/hide delays and optional thin strip. Floating tab, reveal shortcut, or both.
- Suppress bars and mouse activation during fullscreen on their monitor.
- Hold auto-hide open while popup, picker, context menu or drag interaction is active.
- Drag handle to move floaters, edge docking preview and snap on release; drag edge handle out to undock. Lock prevents layout/content edits.
- Duplicate a bar as independent copy. Dividers and adjustable spacers.
- Same setup across virtual desktops; running windows restricted to current virtual desktop.

## Appearance and accessibility
- Default style matches OS; Windows 10 or Windows 11 style selectable per bar.
- Follow Windows light/dark theme or choose fixed theme. Customize icon size, thickness, spacing, alignment, transparency, blur, colors, corners, hover and accent.
- Label modes: icons only, all labels, running-window labels. Tooltips for hidden/truncated names.
- Running/active/attention indicators configurable, matching selected Windows style; app-provided badges/progress where accessible.
- Keyboard navigation and screen reader names; high contrast and reduced-motion preferences respected.
- Central Settings window with bar list and preview; appearance applies live, Cancel restores editing-session state.

## Items and groups
- Drag/drop and context menus plus centralized editor. Installed-app picker includes desktop and Store apps.
- Apps, files, folders, websites; website editor with name, URL and optional icon.
- Custom labels/icons; desktop launch arguments and working directory.
- Missing targets retained and flagged, locate/remove actions.
- Custom collections or live linked folders with all-content, shortcuts-only or extension filters.
- One level of groups only. Child folders open Explorer.
- Group popup grid/list, icon size and dimensions configurable. Automatic screen-constrained positioning.
- Click or delayed hover activation per group. Close after launch by default, configurable. Closed on startup.
- Sort by name/type/date ascending/descending or manual ordering, for both group kinds.
- Custom group default collage of up to four icons; custom icon override.
- Standard Windows file/folder context menus plus DockForge item actions.
- Drop files on an app to open them, hover over running app during a file drag to activate its window; empty space drops add shortcuts.

## Running apps
- Current monitor/all monitors per bar. Optional include/exclude app lists.
- Combined bars merge pinned/running identity; unpinned running apps follow pinned items.
- Grouped icon or separate windows per bar. Reorder running apps and persist order.
- Click running pinned app switches by default, optional launch new. Multiple-window click activates MRU window.
- Active-window click minimizes by default, per-bar override. Shift-click requests new instance.
- Middle-click configurable, default normal close. Multiple windows offer picker; never force terminate.
- Hover picker live thumbnails by default or compact list, titles and close buttons. Thumbnail hover desktop preview restores previous view on exit.
- App context menu switch, close, close-all with confirmation and don't-ask-again, new instance, pin/unpin; Jump Lists where supported.

## Shell, settings and recovery
- No Start/Search/clock/system controls on bars.
- Keep Windows taskbar by default; hide per monitor, all monitors or secondary only.
- Restore Windows taskbars in every bar context menu, Settings and tray menu.
- Restore on normal exit and via separate recovery helper after crash.
- Tray icon with Settings/Exit. First-run wizard chooses monitor, edge/mode and empty or selected apps; no presets.
- Keyboard navigation and floating reveal shortcuts only; no search box or extra launch shortcuts.
- Export/import complete setup, flag missing targets. Rotating backup before imports/schema upgrades, Settings restore.

## Architecture and platform boundaries
- WPF on .NET 10, self-contained distribution, Win32 appbar reservations and DWM thumbnails.
- Public IApplicationDocumentLists only reads recent/frequent destinations; pinned/custom Jump List tasks are not publicly retrievable.
- Windows exposes taskbar progress/overlay setters to applications, not a supported general observer API. Do not fabricate badges, install global injection hooks or claim unsupported integration.
- Native Windows taskbar appearance is emulated with native icons, theme colors, effects and metrics; Explorer's internal control tree is not embedded.
- Tests distinguish local x64 execution, cross-compilation and untested OS/hardware combinations.
