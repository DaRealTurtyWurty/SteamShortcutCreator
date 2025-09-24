# SteamShortcutCreator

[![Build & Release](https://github.com/DaRealTurtyWurty/SteamStartMenu/actions/workflows/release.yml/badge.svg)](https://github.com/DaRealTurtyWurty/SteamStartMenu/actions)

Create and manage Steam shortcuts with a clean, modern Windows 11–style interface.

---

## ✨ Features

* 📂 Automatically detects your Steam installation
* 🎮 Lists all installed Steam games with icons
* 📌 Create desktop & start menu shortcuts
* 🌙 Modern **WPF + Fluent Design** UI (Mica, rounded corners, dark/light theme)
* 🛠 Installable via MSI (**WiX**) or EXE wizard (**Inno Setup**)

---

## 📥 Installation

Download the latest release from the [Releases page](https://github.com/DaRealTurtyWurty/SteamStartMenu/releases).

* **Windows Installer (.msi)** → integrates with Windows, supports enterprise installs
* **Setup Wizard (.exe)** → friendly installer with wizard UI

After installation, launch **SteamShortcutCreator** from the Start Menu or Desktop.

---

## 🚀 Usage

1. Open **SteamShortcutCreator**
2. Select your Steam installation folder (first launch only)
3. Choose a game from the list
4. Click **Create Shortcut** → a desktop/start menu shortcut is created

---

## 🛠 Building from source

Requirements:

* [.NET 9 SDK](https://dotnet.microsoft.com/)
* [WiX Toolset](https://wixtoolset.org/) (for MSI)
* [Inno Setup](https://jrsoftware.org/isinfo.php) (for EXE installer)

Clone and build:

```bash
git clone https://github.com/DaRealTurtyWurty/SteamStartMenu.git
cd SteamStartMenu
dotnet build
```

Publish a standalone build:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -o dist/publish
```

Create installers (optional, requires WiX & Inno):

```powershell
# WiX
candle.exe SteamShortcutCreator.wxs -out dist/SteamShortcutCreator.wixobj
light.exe dist/SteamShortcutCreator.wixobj -o dist/SteamShortcutCreator.msi -b dist/publish

# Inno
iscc.exe setup.iss /Odist
```

Or simply let GitHub Actions build them for you — check the [Releases](https://github.com/DaRealTurtyWurty/SteamStartMenu/releases).

---

## 📜 License

MIT License – see [LICENSE](LICENSE) for details.

---

## 🙌 Acknowledgements

* [Wpf.Ui](https://github.com/lepoco/wpfui) for Fluent UI controls
* [WiX Toolset](https://wixtoolset.org/)
* [Inno Setup](https://jrsoftware.org/)
