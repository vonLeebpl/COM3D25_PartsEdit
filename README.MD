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
- **Library** window (SKM-style): one database file with modifications (one slot each), sets, per-maid assignments and auto-load,
  revert / reset, per-modification "same item only" matching. The old Import / Export / Set screens still work on xml files.
- Import / Export of presets, Sets (several slots in one file) with auto-load for a given maid, Copy between maids, IK.
- Japanese UI (the source language, as in the original plugin) with an English translation file
  (`IMGUITranslationLoader\IMGUIStrings\COM3D25_PartsEdit.txt`, named after the assembly). Without the file the UI is Japanese;
  the button names used in the How-to below are the English ones.

## Requirements

- COM3D2.5 with BepInEx 5.4.x
- `COM3D2.API.dll` (SystemShortcutAPI, gear menu button) and `BepInEx.UnityInjectorLoader` / `ExIni.dll`
- Optional: IMGUITranslationLoader (English UI strings)

## Install

Copy `COM3D25_PartsEdit.dll` into `BepInEx\plugins\PartsEdit\` and the translation file from this repository,
`IMGUITranslationLoader\IMGUIStrings\COM3D25_PartsEdit.txt`, into the same path in the game folder
(the file name must match the assembly name). Settings and presets are stored in `BepInEx\config\PartsEdit\`.

## How to

### Edit the bones of an item (Independent bones)

1. Open the window with the gear menu button, choose **Maid Parts**, select the maid and a slot (e.g. `shoes`, `glove`, `accKami_1_`).
2. Switch on **Independent bones** (shown for slots that have bones). The plugin now edits proxy bones named
   `<bone>_ShoeAdj`, which the game never overwrites.
3. Hold the select key (default `LeftAlt`) and click a `..._ShoeAdj` bone. Use the gizmo (Pos. / Rot. / Scl.).
   For "relative to the foot / hand" adjustments use **Local** coordinates.
4. The `R` buttons revert one bone, one slot or the whole maid.

Tips: adjust each foot separately. The topmost bone moves everything below it, relative to its own parent.
Switching Independent bones off and on again keeps your adjustments during the session.

### Library: save, reuse and organise modifications

Open the **Library** button on the main screen. A *modification* is the set of bone changes of **one slot**.

- **Maid tab** - auto-load toggle, the sets and modifications assigned to this maid (Apply / Revert / remove; removing reverts),
  `+ Add modification` (offers the ones that fit the worn items) and **Save current slot as a modification**.
  Only the bones you actually changed are saved. **Reset maid** reverts everything PartsEdit changed on this maid.
- **Library tab** - all modifications: search, sort, rename, delete, Apply / Revert on the selected maid.
  *Any item in slot / Same item only* decides whether a modification fits every item of that slot or only the item it was made for.
- **Sets tab** - create sets, add / remove / reorder modifications (later ones win on the same bone), apply or assign a set to a maid.

Everything is stored in `BepInEx\config\PartsEdit\PartsEdit.db.json`. Old preset / set xml files are imported once at the first start.

### Conditions: when does a modification apply

Like in ShapeKeyMaster, a modification can have **conditions** (Library tab, **Conditions** button on the modification).
Without conditions it uses the simple *Any item in slot / Same item only* switch. With **Use conditions** on:

- Choose **All must match** or **Any matches**, then add conditions. Each one can be negated (**NOT**):
  - **Slot worn** - something is worn in a slot (empty slot field = the modification's own slot, `*` = any slot),
  - **Menu file** - the `.menu` file name of the worn item matches a pattern, e.g. `*heel.menu`,
  - **Object name** - e.g. `_SM_hhmod*heel`,
  - **Model file** - the `.model` file name.
- Patterns are case-insensitive; `*` is any text and `?` one character. A pattern without them must match the whole name.
- **+ Add from the worn item** fills a condition from what the selected maid wears now, and the editor shows whether the
  modification matches the current outfit (`[OK]` / `[--]` per condition).
- Maids with **Auto-load** on are kept in sync with their outfit: when the clothes change, modifications whose conditions
  are met are applied to the new item and the ones that no longer match are reverted. Manual **Apply** respects conditions too.

### Shoes tilted for high heels (object root pose)

Rotating the *object root* of a slot turns the whole slot skeleton like one rigid body around a single point, so it only
looks right in the pose it was set in and displaces the item (and its bones) in other poses. For shoes made for high-heel
feet, tilt each shoe **relative to its foot** instead: Independent bones, then rotate `Bip01 L/R Foot_ShoeAdj` in Local coordinates.

If you already have a modification with an object root pose (Library tab shows `Object root pose (rot .. deg, move .. cm)`):

1. In the Library tab switch **Object root pose** on, press **Reset maid** (Maid tab) and **Apply** the modification in the pose you tuned it in.
2. Maid tab: choose the slot and press **Convert object root pose into bone adjustments**. The look does not change,
   but the adjustment now follows the bones in every pose.
3. **Save current slot as a modification** and delete the old one.

Saving a slot that has Independent bones switched on does step 2 automatically.

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
