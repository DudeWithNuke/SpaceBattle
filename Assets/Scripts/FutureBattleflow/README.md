# FutureBattleflow Scaffold

This folder contains non-integrated scaffolding for future UI/data flow:

- `Models/`
  - `PlayerLoadoutAsset`: player-configured fleet loadout for pre-battle setup.
  - `FleetInstanceModel`: runtime fleet instances derived from loadout.
  - `BattlePhase`, `BattlefieldViewSide`: phase and active-side context.
- `Selection/`
  - `PreparationPlaceableSelectionProvider`: builds ship buttons for preparation phase.
  - `BattlePlaceableSelectionProvider`: builds ability buttons for battle phase.
  - `FutureBattleSelectionPlanner`: facade that selects provider by phase and can build fleet from loadout.

Current gameplay code does not reference this folder yet.
