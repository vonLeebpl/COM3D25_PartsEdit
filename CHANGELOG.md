# Changelog

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
