# COM3D25_PartsEdit

PartsEdit plugin for **COM3D2.5** (Unity 2022, BepInEx 5). Edit the bones and parts of a maid's clothes and
accessories in game, save the result as reusable *modifications*, group them into *sets*, and let the plugin apply
them automatically - only when the maid wears the items they were made for.

This is a port and extension of earlier PartsEdit work, see [Credits](#credits).

**Contents:** [What's new](#whats-new-in-03) - [Features](#features) - [Requirements](#requirements) - [Install and update](#install-and-update) -
[How to](#how-to) - [Where the data is](#where-the-data-is-and-how-to-back-it-up) - [Troubleshooting](#troubleshooting) -
[Known limitations](#known-limitations) - [Build](#build) - [Credits](#credits) - [Changelog](CHANGELOG.md)

## What's new in 0.3

- **Library window**: a database of modifications (one slot each), sets, per-maid assignments, auto-load, revert / reset, Edit / Update.
- **Conditions** (like ShapeKeyMaster): menu / object / model name patterns with `*`, "slot is worn", NOT, all / any; maids with
  auto-load are kept in sync with their outfit.
- **Per-maid view**: the Library and Sets tabs show only the selected maid's items by default.
- **Independent bones** also for hair, dresses and accessories; adjustments survive switching the option off and on.
- Fixes: items drifting after a pose change (object root pose), accessories that could not be moved, window state after End.
- New look: grey highlight colour, button groups instead of radio buttons, areas with their own background, button font size control.
- The old Import / Export / Set screens are replaced by the Library for Maid Parts (old data is imported once).
- The proxy bones are now named `<bone>_Adj` (were `<bone>_ShoeAdj`); saved data is converted automatically.

## Features

- Edit the position / rotation / scale of bones of any slot of a maid (Maid Parts mode), also Multi Maids and Studio modes.
- Gizmo editing with bone display, bone picking (hold the select key, default `LeftAlt`, and click a bone).
- **Independent bones** (per slot switch): shoes, gloves, stockings, headwear, hair, dresses and accessories can be moved, rotated and
  scaled freely. The game overwrites the slot bones every frame (`TBodySkin.CopyTrans`) and the root of attached accessories,
  so the plugin edits proxy bones (`<bone>_Adj`) that follow the real bones but are never overwritten.
- **Library** window: one database file with modifications (one slot each), sets, per-maid assignments and
  auto-load, revert / reset, an Edit button, and a per-maid filter so you only see the modifications of the maid you work on.
- **Conditions**: apply a modification only when the worn item matches - by `.menu` file (wildcards such as
  `*heel.menu`), object name, model file or "slot is worn", each optionally negated, combined with *all* / *any*.
  Maids with auto-load are kept in sync with their outfit whenever the clothes change.
- Import / Export of presets for Multi Maids and Studio objects, Copy between maids, IK (from the original plugin).
- Japanese UI (the source language, as in the original plugin) with an English translation file
  (`IMGUITranslationLoader\IMGUIStrings\COM3D25_PartsEdit.txt`, named after the assembly). Without the file the UI is Japanese;
  the button names used in this document are the English ones.

## Requirements

- COM3D2.5 with BepInEx 5.4.x
- `COM3D2.API.dll` (SystemShortcutAPI, gear menu button) and `BepInEx.UnityInjectorLoader` / `ExIni.dll`
- Optional: IMGUITranslationLoader (English UI strings)

## Install and update

**Install.** Copy `COM3D25_PartsEdit.dll` into `BepInEx\plugins\PartsEdit\` and the translation file
`IMGUITranslationLoader\IMGUIStrings\COM3D25_PartsEdit.txt` into the same path in the game folder (the file name must match the
assembly name). The release zip contains both files in this layout, so you can extract it into the game folder.
Settings and data are stored in `BepInEx\config\PartsEdit\`.

**Update from 0.2.**

1. Close the game. Replace `COM3D25_PartsEdit.dll` and the translation file (the new file has many more entries).
2. Start the game. At the first start the plugin creates `PartsEdit.db.json` and imports your old preset / set xml files and
   auto-load assignments from `BepInEx\config\PartsEdit\Presets` into it (the xml files stay untouched).
3. Bones saved with the old `_ShoeAdj` names are converted to `_Adj` when the data is loaded; presets and scenes made with 0.2
   keep working.
4. The old Import / Export / Set screens are gone for Maid Parts: use the **Library** button instead. Import / Export remain for
   Multi Maids and Studio objects.

## How to

### Quick start: adjust shoes once, reuse them everywhere

1. Open the window with the gear menu button, choose **Maid Parts**, select the maid and the slot `shoes`.
2. Switch on **Independent bones**, hold `LeftAlt`, click a `Bip01 L Foot_Adj` bone and move / rotate it with the gizmo
   (**Local** coordinates). Do the same for the right foot.
3. Press **Library** (row "Preset"), tab **Maid**, section *Save current slot as a modification*: pick the slot `shoes`,
   type a name and press **Save as modification**. It is saved for this maid and assigned to her.
4. Switch **Auto-load** on. From now on the modification is applied when the maid loads and whenever she changes clothes
   to something that fits (see [Conditions](#conditions-when-does-a-modification-apply)).

### The main screen

- **Config** - *Bone Display* (Hide / Display; `Detailed` has colours and the select key), *Coord. Type* (Local / Global / View)
  and *Gizmo Type* (None / Pos. / Rot. / Scl. / IK). The highlighted button of a group is the active one.
- **Obj. Select. Mode** - *Maid Parts* (a slot of a maid), *Multi Maids* and *Studio* (objects that are not maid slots).
- **Operate** - choose the maid and the slot. The row **Preset** has `Library`, `Copy` (copy modifications between maids) and `IK`.
  *Shaking Bone* switches the game's sway physics (yure) of the slot, *Independent bones* is described below, *Bone:* shows
  the selected bone, and the `R` buttons revert the position / rotation / scale of that bone, the slot or the maid.
- **Reset** (bottom left) reverts all changes made with PartsEdit, **End** closes the window; the gear button opens it again.
  The size of the window is saved when the game closes (you can also drag its edges).

### Edit the bones of an item (Independent bones)

1. Select the maid and a slot (e.g. `shoes`, `glove`, `stkg`, `accKami_1_`).
2. Switch on **Independent bones** (shown for slots that have bones). The plugin now edits proxy bones named
   `<bone>_Adj`, which the game never overwrites, and hides the real bones.
3. Hold the select key (default `LeftAlt`) and click a `..._Adj` bone: the nearest bone to the cursor is selected.
   Use the gizmo (Pos. / Rot. / Scl.). For "relative to the foot / hand" adjustments use **Local** coordinates.
4. `R` reverts one bone, one slot or the whole maid.

Tips

- Adjust each foot separately. The topmost bone of a slot moves everything below it, relative to its own parent.
- Switching Independent bones off and on again keeps your adjustments during the session.
- Accessories: switch Independent bones on and pick the topmost `..._Adj` bone (e.g. `Bone_center_Adj`) to move
  the whole accessory. The game keeps resetting the root of attached accessories, so the plain root is not selectable.
- Hair and dresses work the same way. Hair has many `_yure_` bones driven by the game's sway physics; if they fight with your
  adjustments, switch **Shaking Bone** off for the slot while you edit.
- Do not rotate the *object root* of a slot for tilting shoes, see [Shoes tilted for high heels](#shoes-tilted-for-high-heels-object-root-pose).

### The Library window

Open it with the **Library** button (row "Preset"); **Back** returns to the main screen. `A-` / `A+` in its header change the
font size of the buttons (saved in the settings).

**Concepts**

- A **modification** is the set of bone changes of **one slot**. Only the bones you actually changed are saved.
- A **set** is an ordered list of modifications (later ones win on the same bone).
- Modifications and sets are **assigned** to maids. A maid with **Auto-load** on gets everything assigned to her applied
  automatically and kept in sync with her outfit.
- Every modification and set remembers the maid it was made for. The Library and Sets tabs show **only the selected maid's**
  items by default; the switch **[This maid | All]** shows everything, with the name of the maid it was made for. The two tabs have
  separate switches, and the counters in the tab titles follow them.

**Maid tab** (the maid chosen at the top of the window). It is made of four areas on different backgrounds:

1. *Auto-load*, `Apply all` (applies everything assigned to her) and `Reset maid` (reverts everything PartsEdit changed on this maid,
   also manual edits).
2. **Sets of this maid** - `Apply`, `Revert`, `x` (removes the set from the maid and reverts it). `+ Add set` opens a list of sets made
   for this maid; tick *Also show sets made for other maids* to pick from all of them.
3. **Modifications of this maid** - `Apply`, `Revert`, `Edit`, `x`. `+ Add modification` opens a searchable list of the modifications
   made for this maid that fit what she wears now (tick *Also show modifications made for other maids* for the whole library);
   choosing one assigns and applies it.
4. **Save current slot as a modification** - pick the slot (`Slot: ...` opens the list of her worn slots), type a name,
   `Save as modification` (also assigns it to the maid). `Update '<name>' ...` appears after `Edit`. `Convert object root pose into
   bone adjustments` is described [below](#shoes-tilted-for-high-heels-object-root-pose).

**Library tab** - every modification is a row with a different background:

- name `[slot]` and `Rename`;
- the **Use conditions** switch, a short summary and the **Conditions** button that opens the editor;
- `Apply`, `Revert`, `Edit` and `Delete` (asks to confirm); `Apply` and `Revert` need a selected maid;
- `Object root pose (...)` appears when the modification contains a change of the object root (can be switched off);
- search, sort (name / date / slot) and paging for long lists.

**Sets tab** - create sets (`New set`), open a set to rename it, add / remove / reorder its modifications (`^` `v` `x`,
`+ Add modification to set`), `Apply to maid`, `Revert`, `Assign to maid` / `Unassign from maid` (unassigning reverts), `Delete set`
(its modifications stay in the library).

**Typical workflows**

- **Edit an existing modification:** `Edit` applies it, selects its slot on the main screen and returns there. Change the bones,
  then Library > Maid > **Update '<name>' with the current state of its slot**.
- **Undo:** `Revert` undoes one modification or set (and re-applies the other ones of the same slots), `x` removes it from the
  maid and reverts it, `Reset maid` reverts everything.
- **Reuse on another maid:** select her, Maid tab > `+ Add modification` > tick *Also show ... other maids* and pick it.
- **A look made of several items:** put the modifications of shoes, gloves and stockings into one set and assign the set.
- **A modification only for one kind of item:** use [Conditions](#conditions-when-does-a-modification-apply).

### Conditions: when does a modification apply

Like in ShapeKeyMaster, a modification can have **conditions**. Without conditions a modification fits **any item worn in its
slot**. Switch **Use conditions** on (in the modification's row in the Library tab) and open **Conditions**:

1. Choose **All must match** or **Any matches**.
2. Add conditions with `+ Add condition`, or with `+ Add from the worn item` (opens a list of the selected maid's current item in
   that slot: its menu file, object name and model file; clicking one adds a condition). The kind button of a condition cycles through
   the kinds, **NOT** negates it, `x` removes it:

| Kind | True when |
| --- | --- |
| **Slot worn** | something is worn in the slot (the pattern is ignored) |
| **Menu file** | the `.menu` file name of the worn item matches the pattern, e.g. `*heel.menu` |
| **Object name** | the object name of the item matches, e.g. `_SM_hhmod*heel` |
| **Model file** | the `.model` file name of the item matches |

3. The **slot** field of a condition says where to look: empty = the modification's own slot, `*` = any worn item,
   or a slot name (`stkg`, `glove`, ...).

How matching works

- Patterns are **case-insensitive**. `*` is any text (also nothing), `?` is exactly one character. A pattern without `*` / `?`
  must match the **whole** name. An empty pattern never matches (so a NOT of it is always true).
- A modification can only apply when **something is worn in its own slot** - an empty slot never matches.
- Only items the game currently shows count as worn (the visibility flag of the slot); an empty or hidden slot counts as not worn.
- With **All**, every condition must be true; with **Any**, one is enough. Conditions switched on with an empty list mean
  "fits the slot".
- The editor shows `[OK]` / `[--]` (and the matched name) per condition for the **selected maid's current outfit**,
  and a summary line, so you can build a pattern by trial. Hint: start with `+ Add from the worn item` and then generalise
  the pattern by replacing the maid-specific part with `*`.
- Modifications created with the old "Same item only" option are converted to an *Object name* condition.

Examples

| Goal | Conditions |
| --- | --- |
| Only for high-heel shoes | Menu file `*heel*.menu` |
| One specific model only | Object name `_SM_hhmod_ch1_1503t_strap_heel` |
| Several families of shoes | **Any**: Menu file `*heel.menu`, Menu file `*pumps.menu` |
| Gloves only when the maid also wears stockings | Slot worn, slot `stkg` |
| Not when the stockings are the thin ones | NOT Menu file `*thin*.menu`, slot `stkg` |
| Whenever any outfit part matches a brand | Menu file, slot `*`, pattern `brand_*.menu` |

When are conditions evaluated

- **Manual `Apply`** (and `Edit`) respects the conditions: if the worn item does not fit, nothing is applied and the status says
  *conditions are not met*.
- For maids with **Auto-load** on, the plugin watches the outfit. About a quarter of a second after the clothes stop changing it
  **applies** the modifications whose conditions are met to the new item (once per item) and **reverts** those that stopped matching.
  Editing conditions triggers a re-check at once.
- Modifications live on the item object: when an item is changed or taken off, its adjustments disappear with it, and the
  plugin re-applies matching modifications to the new item.
- Maids without Auto-load are not synchronised; use `Apply` / `Apply all` when you want to apply by hand.

### Shoes tilted for high heels (object root pose)

Rotating the *object root* of a slot turns the whole slot skeleton like one rigid body around a single point, so it only
looks right in the pose it was set in and displaces the item (and its bones) in other poses. For shoes made for high-heel
feet (e.g. `hhmod` models used with the HighHeel plugin), tilt each shoe **relative to its foot** instead: Independent bones,
then rotate `Bip01 L/R Foot_Adj` in Local coordinates.

If you already have a modification with an object root pose (Library tab shows `Object root pose (rot .. deg, move .. cm)`):

1. In the Library tab switch **Object root pose** on, press **Reset maid** (Maid tab) and **Apply** the modification in the pose you tuned it in.
2. Maid tab: choose the slot and press **Convert object root pose into bone adjustments**. The look does not change,
   but the adjustment now follows the bones in every pose.
3. **Save current slot as a modification** and delete the old one.

Saving a slot that has Independent bones switched on does step 2 automatically. If shoes drift after a pose change,
check the Library tab: a modification with an *object root pose* that is switched on is the usual cause; switching it off reverts it.

### Appearance and settings

- **Colours** - the highlight colour (selected buttons, selected list items) is a neutral grey by default. Change it in
  Bone Display > `Detailed` (*Selection highlight colour*). The colours of the drawn bones are there too.
- **Button font size** - `A-` / `A+` in the Library header (steps of 0.05, range 0.6 - 1.3, default 0.80).
- **Window size** - drag the edges; it is saved when the game closes.
- The settings file is `BepInEx\config\PartsEdit\COM3D2.PartsEdit.Plugin.ini` (the name comes from the original plugin and is
  kept so your settings carry over). Relevant keys: `[Window] Width`, `Height`, `ButtonFontScale` and `[BoneDisplay] HighlightColor`.
  An old red highlight colour (`#B80000`) from 0.2 is replaced by the new grey default.
- **Language** - the UI follows the translation file; remove or rename `COM3D25_PartsEdit.txt` to see the Japanese source texts.

## Where the data is and how to back it up

Everything lives in `BepInEx\config\PartsEdit\`:

| File / folder | What it is |
| --- | --- |
| `PartsEdit.db.json` | the Library: modifications, sets, maid assignments, conditions (human-readable JSON) |
| `PartsEdit.db.json.bak` | the previous version of the database (kept on every save) |
| `PartsEdit.db.json.corrupt-<time>` | an unreadable database moved aside; the plugin then starts with an empty one |
| `COM3D2.PartsEdit.Plugin.ini` | settings |
| `Presets\` | old preset / set xml files from 0.2 (imported once, not used afterwards) and `Presets\IK\` for IK data |

To back up, copy the folder. To start over, close the game and delete `PartsEdit.db.json` (the old xml files are imported again).
A database written by a newer version of the plugin is used read-only and is never overwritten.

## Troubleshooting

- **The UI is Japanese** - the translation file is missing or has the wrong name. It must be
  `IMGUITranslationLoader\IMGUIStrings\COM3D25_PartsEdit.txt` (the assembly name, not the old plugin name).
- **A modification does not apply** - read the status line under the tabs: *slot is empty*, *this maid has no such slot* or
  *conditions are not met*. In the Conditions editor `[--]` shows which condition fails for the current outfit.
- **Nothing to save** - only bones changed with PartsEdit are saved; edit the slot first (or apply a modification).
- **Shoes / items are displaced after a pose change** - see [Shoes tilted for high heels](#shoes-tilted-for-high-heels-object-root-pose).
- **A bone cannot be picked** - hold the select key (`LeftAlt` by default) and click close to the bone line; with Independent
  bones on only the `_Adj` bones can be picked.
- **Log lines** - messages of the plugin carry the tag `[COM3D25_PartsEdit]` in `BepInEx\LogOutput.log`
  (e.g. `Apply ...`, `Sync ...`, `independent bones enabled ...`). `independent bones drift ...` means a proxy bone is more
  than 1 cm / 2 degrees away from its real bone.

## Known limitations

- Conditions look at the `.menu`, object and model names of **worn items** of one maid; there are no conditions on the maid herself
  (use assignments for that) or on poses / scenes.
- Maids are identified by their game GUID; modifications made before the library existed have no recorded maid and appear in
  the *All* view until you assign them.
- Independent bones edit proxy bones, so modifications contain `..._Adj` bones; they only apply to slots whose mesh uses bones
  with the same names.
- The Library covers maid slots. Multi Maids and Studio objects still use the Import / Export of the original plugin.

## Build

Open `COM3D2.PartsEdit.Plugin.sln` (Debug or Release configuration). The reference paths in the `.csproj` point at the game folder
(`C:\KISS\COM3D2_5`), change them to your own game directory. The offline tests of the data layer (`Db\`, no game needed)
compile with plain `csc` against `Newtonsoft.Json` - the layer does not depend on Unity or BepInEx.

## Credits

- **hatena37** - author of the original PartsEdit plugin: https://ux.getuploader.com/hatena37/download/121
- **lilly (lilly1987)** - earlier fork that added the "No UI ScreenShot" fix (the UI is no longer captured by screenshots):
  https://github.com/customordermaid3d2/COM3D2.PartsEdit.Plugin
- **Literally Who? (@LW_Mods)** - upgraded PartsEdit plugin for COM3D2.5 (published 16 Oct 2026). **This project is based on
  that release**: https://x.com/LW_Mods/status/2110934101037563913
- **vonLeebpl** - further development of the LW_Mods version: independent bones, library, conditions, translations, bone picking
  and the other changes listed above.

The idea of conditions with menu-file wildcards comes from **ShapeKeyMaster** (SKM).

No license is stated by the original authors, all rights to the original code remain with them.
If you are one of the authors and want something changed or removed, please open an issue.
