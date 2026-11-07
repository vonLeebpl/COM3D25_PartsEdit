# Changelog

## 0.3.1

### Added
- Settings are now in the BepInEx config file `BepInEx\config\com3d25.partsedit.cfg` and can be edited in game in the F1 Configuration
  Manager (window size, font sizes, colours, keys, ...). Window size and font sizes apply at once. The old `COM3D2.PartsEdit.Plugin.ini`
  is read once and its values are migrated to the cfg; it is no longer written.
- `LabelFontScale` (default 0.85): font size of the static texts and toggle labels, next to the existing `ButtonFontScale`.
- Copy panel: the source list now contains characters from the library (with sets / modifications) and from the scene, with a filter
  "Only maids in the scene". Library sets and modifications of the source are copied to the selected maid and applied; edits of the
  slots of a maid in the scene can still be copied as before. The destination is always the maid selected on the main screen.
- Back button in the header of the Library window (also kept at the bottom).

### Fixed
- Drop-down lists open right under their button (not at the click position / in the corner of the window), are sized from their
  contents and have a reliable background.
- Window size changed by the user is saved to the cfg when the mouse button is released (was saved only when the game closed).

## 0.3

### Added
- Library window: database of modifications (one slot each), sets, per-maid assignments, auto-load, Revert / Reset maid, Edit / Update.
- Conditions (ShapeKeyMaster style): slot worn, menu file, object name, model file; wildcards `*` and `?`; NOT; all / any.
  Maids with auto-load are kept in sync with their outfit.
- Per-maid filter ([This maid | All]) separately for the Library and Sets tabs.
- Independent bones for hair, dresses and accessories; adjustments survive switching the option off and on.
- Object root pose handling: per-modification switch and conversion into bone adjustments.
- Button font size control (`A-` / `A+`), saved in the settings.
- English translation file named after the assembly (`COM3D25_PartsEdit.txt`).

### Changed
- Proxy bones are named `<bone>_Adj` / `<bone>_Follow` (were `_ShoeAdj` / `_ShoeFollow`). Saved data (database, xml presets,
  sets, scenes) is converted automatically.
- Grey highlight colour (`#6F7078`) replaces the red one; button groups instead of radio buttons; area backgrounds.
- Old Import / Export / Set screens for Maid Parts are replaced by the Library (old files are imported once).
- Version 0.3.

### Fixed
- Items drifting after a pose change.
- Accessories that could not be moved.
- Window state after End.
- GUIStyle warnings.

## 0.2
- Port of the LW_Mods upgrade of PartsEdit to COM3D2.5, independent bones for shoes, bone picking, translations.
