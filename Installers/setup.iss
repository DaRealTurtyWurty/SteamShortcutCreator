[Setup]
AppName=SteamShortcutCreator
AppVersion=1.0
DefaultDirName={pf}\SteamShortcutCreator
DefaultGroupName=SteamShortcutCreator
OutputDir=dist
OutputBaseFilename=SteamShortcutCreatorInstaller
Compression=lzma
SolidCompression=yes

[Files]
Source: "..\dist\publish\SteamShortcutCreator.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\dist\publish\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs

[Icons]
Name: "{group}\SteamShortcutCreator"; Filename: "{app}\SteamShortcutCreator.exe"
Name: "{commondesktop}\SteamShortcutCreator"; Filename: "{app}\SteamShortcutCreator.exe"

[Run]
Filename: "{app}\SteamShortcutCreator.exe"; Description: "Launch after install"; Flags: nowait postinstall skipifsilent
