#define AppVersion "0.1.4"
[Setup]
AppId={{DC981F1E-3B26-4AB2-A5CC-407598784428}
AppName=DockForge
AppVersion={#AppVersion}
AppPublisher=DockForge contributors
DefaultDirName={autopf}\DockForge
DefaultGroupName=DockForge
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog commandline
ArchitecturesAllowed=x64compatible arm64
ArchitecturesInstallIn64BitMode=x64compatible arm64
MinVersion=10.0.19045
OutputDir=..\artifacts\installers
OutputBaseFilename=DockForge-Setup-{#AppVersion}
SetupIconFile=..\src\DockForge\Assets\dockforge.ico
UninstallDisplayIcon={app}\DockForge.exe
LicenseFile=..\LICENSE
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
AppMutex=Local\DockForge
[Files]
Source: "..\artifacts\publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: not IsArm64; Excludes: "sandbox-profile\*,smoke-results\*"
Source: "..\artifacts\publish\win-arm64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: IsArm64; Excludes: "sandbox-profile\*,smoke-results\*"
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
[Icons]
Name: "{group}\DockForge"; Filename: "{app}\DockForge.exe"
Name: "{group}\Restore Windows taskbars"; Filename: "{app}\DockForge.exe"; Parameters: "--restore-taskbars"
[Run]
Filename: "{app}\DockForge.exe"; Description: "Start DockForge"; Flags: nowait postinstall skipifsilent runasoriginaluser
[UninstallRun]
Filename: "{app}\DockForge.exe"; Parameters: "--restore-taskbars"; Flags: runhidden waituntilterminated
[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueName: "DockForge"; Flags: uninsdeletevalue dontcreatekey
