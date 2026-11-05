# COM3D25_PartsEdit

PartsEdit plugin for **COM3D2.5** (Unity 2022, BepInEx 5). Edit the bones and parts of a maid's clothes and
accessories in game, save the result as presets / sets and load them back.

This is a port and extension of earlier PartsEdit work, see [Credits](#credits).

## Features

- Edit the position / rotation / scale of bones of any slot of a maid (Maid Parts mode), also Multi Maids and Studio modes.
- Gizmo editing with bone display, bone picking (hold the select key, default `LeftAlt`, and click a bone).
- **Independent bones** (per slot toggle): shoes, gloves, stockings, headwear and accessories can be moved, rotated and
  scaled freely. The game overwrites the slot bones every frame (`TBodySkin.CopyTrans`) and the root of attached accessories,
  so the plugin edits proxy bones (`<bone>_ShoeAdj`) that follow the real bones but are never overwritten.
- Import / Export of presets, Sets (several slots in one file) with auto-load for a given maid, Copy between maids, IK.
- English UI through the same translation file that `IMGUITranslationLoader` uses
  (`IMGUITranslationLoader\IMGUIStrings\COM3D25_PartsEdit.txt`, named after the assembly).

## Requirements

- COM3D2.5 with BepInEx 5.4.x
- `COM3D2.API.dll` (SystemShortcutAPI, gear menu button) and `BepInEx.UnityInjectorLoader` / `ExIni.dll`
- Optional: IMGUITranslationLoader (English UI strings)

## Install

Copy `COM3D25_PartsEdit.dll` into `BepInEx\plugins\PartsEdit\` and the translation file from this repository,
`IMGUITranslationLoader\IMGUIStrings\COM3D25_PartsEdit.txt`, into the same path in the game folder
(the file name must match the assembly name). Settings and presets are stored in `BepInEx\config\PartsEdit\`.

## Build

Open `COM3D2.PartsEdit.Plugin.sln` (Debug configuration). The reference paths in the `.csproj` point at the game folder
(`C:\KISS\COM3D2_5`), change them to your own game directory.

## Credits

- **hatena37** - author of the original PartsEdit plugin: https://ux.getuploader.com/hatena37/download/121
- **lilly (lilly1987)** - earlier fork that added the "No UI ScreenShot" fix (the UI is no longer captured by screenshots):
  https://github.com/customordermaid3d2/COM3D2.PartsEdit.Plugin
- **Literally Who? (@LW_Mods)** - upgraded PartsEdit plugin for COM3D2.5 (published 16 Oct 2026). **This project is based on
  that release**: https://x.com/LW_Mods/status/2110934101037563913
- **vonLeebpl** - further development of the LW_Mods version: independent bones, sets, translations, bone picking and the
  other changes listed above.

No license is stated by the original authors, all rights to the original code remain with them.
If you are one of the authors and want something changed or removed, please open an issue.
