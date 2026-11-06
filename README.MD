# COM3D25_PartsEdit

PartsEdit plugin for **COM3D2.5** (Unity 2022, BepInEx 5). Edit the bones and parts of a maid's clothes and
accessories in game, save the result as reusable *modifications*, group them into *sets*, and let the plugin apply
them automatically - only when the maid wears the items they were made for.

This is a port and extension of earlier PartsEdit work, see [Credits](#credits).

## Features

- Edit the position / rotation / scale of bones of any slot of a maid (Maid Parts mode), also Multi Maids and Studio modes.
- Gizmo editing with bone display, bone picking (hold the select key, default `LeftAlt`, and click a bone).
- **Independent bones** (per slot toggle): shoes, gloves, stockings, headwear and accessories can be moved, rotated and
  scaled freely. The game overwrites the slot bones every frame (`TBodySkin.CopyTrans`) and the root of attached accessories,
  so the plugin edits proxy bones (`<bone>_ShoeAdj`) that follow the real bones but are never overwritten.
- **Library** window (SKM-style): one database file with modifications (one slot each), sets, per-maid assignments and
  auto-load, revert / reset, an Edit button, and a per-maid filter so you only see the modifications of the maid you work on.
- **Conditions** (SKM-style): apply a modification only when the worn item matches - by `.menu` file (wildcards such as
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

## Install

Copy `COM3D25_PartsEdit.dll` into `BepInEx\plugins\PartsEdit\` and the translation file from this repository,
`IMGUITranslationLoader\IMGUIStrings\COM3D25_PartsEdit.txt`, into the same path in the game folder
(the file name must match the assembly name). Settings and data are stored in `BepInEx\config\PartsEdit\`.

## How to

### Quick start: adjust shoes once, reuse them everywhere

1. Open the window with the gear menu button, choose **Maid Parts**, select the maid and the slot `shoes`.
2. Switch on **Independent bones**, hold `LeftAlt`, click a `Bip01 L Foot_ShoeAdj` bone and move / rotate it with the gizmo
   (**Local** coordinates). Do the same for the right foot.
3. Press **Library** (row "Preset"), tab **Maid**, section *Save current slot as a modification*: pick the slot `shoes`,
   type a name and press **Save as modification**. It is saved for this maid and assigned to her.
4. Switch **Auto-load** on. From now on the modification is applied when the maid loads and whenever she changes clothes
   to something that fits (see [Conditions](#conditions-when-does-a-modification-apply)).

### The main screen

- **Config** - *Bone Display* (Hide / Display; `Detailed` has colours and the select key), *Coord. Type* (Local / Global / View)
  and *Gizmo Type* (None / Pos. / Rot. / Scl. / IK). The pressed button is the active one.
- **Obj. Select. Mode** - *Maid Parts* (a slot of a maid), *Multi Maids* and *Studio* (objects that are not maid slots).
- **Operate** - choose the maid and the slot. The row **Preset** has `Library`, `Copy` (copy modifications between maids) and `IK`.
  *Shaking Bone* switches the game's sway physics (yure) of the slot, *Independent bones* is described below, *Bone:* shows
  the selected bone, and the `R` buttons revert the position / rotation / scale of that bone, the slot or the maid.
- **Reset** (bottom left) reverts all changes made with PartsEdit, **End** closes the window; the gear button opens it again.

### Edit the bones of an item (Independent bones)

1. Select the maid and a slot (e.g. `shoes`, `glove`, `stkg`, `accKami_1_`).
2. Switch on **Independent bones** (shown for slots that have bones). The plugin now edits proxy bones named
   `<bone>_ShoeAdj`, which the game never overwrites, and hides the real bones.
3. Hold the select key (default `LeftAlt`) and click a `..._ShoeAdj` bone: the nearest bone to the cursor is selected.
   Use the gizmo (Pos. / Rot. / Scl.). For "relative to the foot / hand" adjustments use **Local** coordinates.
4. `R` reverts one bone, one slot or the whole maid.

Tips

- Adjust each foot separately. The topmost bone of a slot moves everything below it, relative to its own parent.
- Switching Independent bones off and on again keeps your adjustments during the session.
- Accessories: switch Independent bones on and pick the topmost `..._ShoeAdj` bone (e.g. `Bone_center_ShoeAdj`) to move
  the whole accessory. The game keeps resetting the root of attached accessories, so the plain root is not selectable.
- Do not rotate the *object root* of a slot for tilting shoes, see [Shoes tilted for high heels](#shoes-tilted-for-high-heels-object-root-pose).

### Library: save, reuse and organise modifications

Open it with the **Library** button (row "Preset"). Concepts:

- A **modification** is the set of bone changes of **one slot**. Only the bones you actually changed are saved.
- A **set** is an ordered list of modifications (later ones win on the same bone).
- Modifications and sets are **assigned** to maids. A maid with **Auto-load** on gets everything assigned to her applied
  automatically (and kept in sync with her outfit).
- Every modification and set remembers the maid it was made for. The Library and Sets tabs show **only the selected maid's**
  items by default; the switch **[This maid | All]** shows everything (with the name of the maid it was made for).

Tabs

- **Maid** - *Auto-load*, `Apply all`, `Reset maid` (reverts everything PartsEdit changed on this maid, also manual edits),
  the sets and modifications assigned to her (`Apply`, `Revert`, `Edit`, `x`; removing reverts the change),
  `+ Add set` / `+ Add modification` (offer items made for her; tick *Also show ... other maids* to pick from the whole library;
  modifications are filtered to the items she currently wears), and **Save current slot as a modification**.
- **Library** - search, sort (name / date / slot), rename, delete (asks to confirm), `Apply` / `Revert` / `Edit` on the selected maid,
  the **Use conditions** switch with a **Conditions** editor. Without conditions a modification fits any item worn in its slot.
  `Object root pose (...)` appears when a modification contains a root change.
- **Sets** - create sets, add / remove / reorder modifications, apply or assign a set to a maid, rename, delete a set
  (its modifications stay in the library).

Typical workflows

- **Edit an existing modification:** `Edit` applies it, selects its slot on the main screen and returns there. Change the bones,
  then Library > Maid > **Update '<name>' with the current state of its slot**.
- **Undo:** `Revert` undoes one modification or set (and re-applies the other ones of the same slots), `x` removes it from the
  maid and reverts it, `Reset maid` reverts everything.
- **Reuse on another maid:** select her, Maid tab > `+ Add modification` > tick *Also show ... other maids* and pick it.
- **A look made of several items:** put the modifications of shoes, gloves and stockings into one set and assign the set.

Everything is stored in `BepInEx\config\PartsEdit\PartsEdit.db.json` (the previous version is kept as `.bak`). Old preset / set xml
files from earlier versions are imported once at the first start and left untouched.

### Conditions: when does a modification apply

Like in ShapeKeyMaster, a modification can have **conditions** (Library tab, **Conditions** button on the modification).
Without conditions a modification fits **any item worn in its slot**. Switch **Use conditions** on (in the modification's row) and open **Conditions**:

1. Choose **All must match** or **Any matches**.
2. Add conditions (`+ Add condition`, or `+ Add from the worn item`). The kind button cycles through the kinds, **NOT** negates:

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
- With **All**, every condition must be true; with **Any**, one is enough. Using conditions with an empty list means "fits the slot".
- The editor shows `[OK]` / `[--]` (and the matched name) per condition for the **selected maid's current outfit**,
  and a summary line, so you can build a pattern by trial. Hint: start with `+ Add from the worn item` and then generalise
  the pattern by replacing the maid-specific part with `*`.

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
then rotate `Bip01 L/R Foot_ShoeAdj` in Local coordinates.

If you already have a modification with an object root pose (Library tab shows `Object root pose (rot .. deg, move .. cm)`):

1. In the Library tab switch **Object root pose** on, press **Reset maid** (Maid tab) and **Apply** the modification in the pose you tuned it in.
2. Maid tab: choose the slot and press **Convert object root pose into bone adjustments**. The look does not change,
   but the adjustment now follows the bones in every pose.
3. **Save current slot as a modification** and delete the old one.

Saving a slot that has Independent bones switched on does step 2 automatically. If shoes drift after a pose change,
check the Library tab: a modification with an *object root pose* that is switched on is the usual cause; switching it off reverts it.

### Troubleshooting

- **The UI is Japanese** - the translation file is missing or has the wrong name. It must be
  `IMGUITranslationLoader\IMGUIStrings\COM3D25_PartsEdit.txt` (the assembly name, not the old plugin name).
- **A modification does not apply** - read the status line under the tabs: *slot is empty*, *this maid has no such slot* or
  *conditions are not met*. In the Conditions editor `[--]` shows which condition fails for the current outfit.
- **Nothing to save** - only bones changed with PartsEdit are saved; edit the slot first (or apply a modification).
- **Log lines** - messages of the plugin carry the tag `[COM3D25_PartsEdit]` in `BepInEx\LogOutput.log`
  (e.g. `Apply ...`, `Sync ...`, `independent bones enabled ...`). `independent bones drift ...` means a proxy bone is more
  than 1 cm / 2 degrees away from its real bone.

### Known limitations

- Conditions look at the `.menu`, object and model names of **worn items** of one maid; there are no conditions on the maid herself
  (use assignments for that) or on poses / scenes.
- Maids are identified by their game GUID; modifications made before the library existed have no recorded maid and appear in
  the *All* view until you assign them.
- Independent bones edit proxy bones, so presets made with this plugin contain `..._ShoeAdj` bones; they only apply to slots
  whose mesh uses bones with the same names.

## Build

Open `COM3D2.PartsEdit.Plugin.sln` (Debug configuration). The reference paths in the `.csproj` point at the game folder
(`C:\KISS\COM3D2_5`), change them to your own game directory.

## Credits

- **hatena37** - author of the original PartsEdit plugin: https://ux.getuploader.com/hatena37/download/121
- **lilly (lilly1987)** - earlier fork that added the "No UI ScreenShot" fix (the UI is no longer captured by screenshots):
  https://github.com/customordermaid3d2/COM3D2.PartsEdit.Plugin
- **Literally Who? (@LW_Mods)** - upgraded PartsEdit plugin for COM3D2.5 (published 16 Oct 2026). **This project is based on
  that release**: https://x.com/LW_Mods/status/2110934101037563913
- **vonLeebpl** - further development of the LW_Mods version: independent bones, library, conditions, translations, bone picking
  and the other changes listed above.

No license is stated by the original authors, all rights to the original code remain with them.
If you are one of the authors and want something changed or removed, please open an issue.
