# DockForge privacy policy

Effective date: 21 October 2026.

DockForge stores configuration, backups and diagnostic logs on your computer, normally under `%LOCALAPPDATA%\DockForge`. It reads local shortcuts, selected folders, monitor information and running-window information to provide its taskbar features. It does not send this information to the maintainer, an analytics service or a crash-reporting service.

## Update checks

When **Check GitHub Releases at startup** is enabled, DockForge requests release information from `https://api.github.com/repos/sidosgr/DockForge/releases`. This option is enabled by default and can be disabled under Settings → Backups & updates. **Check now** makes the same request when selected.

These requests send standard network information to GitHub, including your IP address, HTTP headers and a User-Agent containing the DockForge version. They do not include your settings, desktop contents, running applications, file paths, personal identifiers or diagnostic logs. GitHub processes requests according to its [privacy statement](https://docs.github.com/en/site-policy/privacy-policies/github-general-privacy-statement).

Check results, availability, download progress and errors appear in the version area under Settings → Backups & updates. These checks do not open popups. Development previews are included by default in development builds; you can turn that option off to check stable releases only.

Choosing **Download update** downloads the official installer directly from GitHub and its download CDN. These requests send standard network information to GitHub, such as your IP address and DockForge version; no settings or logs are uploaded. Downloaded installers are cached locally in `%LOCALAPPDATA%\DockForge\Updates`. The app checks the release's SHA-256 digest and size and retains Windows' normal security handling for Internet downloads.

Choosing **Install update** saves the current settings, verifies the cached installer again and launches it locally. DockForge closes after the installer starts, restoring Windows taskbar visibility. Windows and the setup wizard may display their normal prompts. Downloads and installation do not start automatically.

## Actions you request

Opening a website shortcut or the online project repository uses your browser and contacts that destination. Opening a linked network folder or launching an application can also contact services selected by you. Those applications and destinations have their own privacy practices.

Exported diagnostics and settings are written to a location you choose. Exporting does not upload them. Review files before sharing them, since diagnostic messages and settings can contain file paths or shortcut targets.

## Removal and contact

You can remove locally stored settings and backups after exiting DockForge. Uninstalling the application restores Windows taskbar visibility; local configuration may remain for reuse. For project questions, use [GitHub Issues](https://github.com/sidosgr/DockForge/issues). Issue reports are public: do not include secrets or private logs.
