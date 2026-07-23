# Enhanced Battle Test repository instructions

## Build, test, and lint

- This is a Windows-only C# 8 project targeting .NET Framework 4.7.2. Build it from a Visual Studio Developer PowerShell/Command Prompt so `MSBuild.exe` and `nuget.exe` are available.
- The build requires a local Mount & Blade II: Bannerlord installation because the project references TaleWorlds and module assemblies directly from `$(GamePath)`.
- Decompiled sources for the referenced game DLLs are available at `D:\develop\src\Bannerlord.Binaries`. Use them to verify TaleWorlds API behavior, mission behavior composition, private member names, and version-sensitive signatures before relying on guesses or changing reflection/Harmony code.
- Restore and build the same way as CI:

  ```powershell
  nuget restore .\source\EnhancedBattleTest.sln
  MSBuild.exe .\source\EnhancedBattleTest.sln /p:Configuration=Release /p:GamePath="C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\"
  ```

- `GamePath` must include the trailing backslash. The default is defined in `source\EnhancedBattleTest\GamePath.props`; prefer overriding it on the command line for machine-specific paths.
- Be aware that a successful build is not side-effect free: the project deletes the previous `source\package\EnhancedBattleTest` directory, recreates the package, and copies it into `$(GamePath)Modules\EnhancedBattleTest`.
- `TaleWorlds.PlayerServices` currently has a hard-coded Steam installation path in the project file instead of using `$(GamePath)`, which may affect non-default installations.
- There is no automated test project, test runner, or repository lint command. Consequently there is no single-test command; validate changes with the targeted solution build and, for runtime behavior, by loading the packaged module in Bannerlord.

## Architecture

- `EnhancedBattleTestSubModule` is the Bannerlord entry point declared by `Modules\EnhancedBattleTest\SubModule.xml`. It adds the campaign behavior to normal Sandbox and StoryMode games and installs the map-menu Harmony patch.
- `EnhancedBattleTestCampaignBehavior` registers the campaign menu, and `EnhancedBattleTestMapMenuPatch` uses `MapScreen.CurrentVisualOfTooltip` because v1.2.12 passes `null` as the selected visual when the main party is clicked. If CampMenuMod's `camp_menu` exists, EBT adds its option there and both click patches target that menu. The setup state returns to the existing campaign.
- `EnhancedBattleTestState` assembles map data from the mod scene XML, Custom Battle scenes, and single-player battle scenes. It is rendered by `EnhancedBattleTestScreen`, which loads the `EnhancedBattleTestScreen` Gauntlet movie and creates the mode-specific character collection.
- The main setup flow is `EnhancedBattleTestScreen` -> `EnhancedBattleTestVM` -> nested side, troop, battle-type, and map view models. These view models edit the same `BattleConfig` object that is loaded from and saved to the user's Bannerlord configuration directory.
- Starting a field battle creates isolated temporary native `MobileParty` objects, a native `MapEvent`, `PartyGroupTroopSupplier` instances, and native `PartyGroupAgentOrigin` objects. This keeps mission objects recognizable to other campaign mods; `EnhancedBattleTestCleanupLogic` removes the temporary campaign objects after the mission.
- Character selection is single-player only. Campaign `CharacterObject` instances populate the temporary native party rosters; the obsolete multiplayer character/config/combatant hierarchy has been removed.
- Runtime assets are part of the implementation, not generated output: Gauntlet prefabs/brushes live under `Modules\EnhancedBattleTest\GUI`, scene and string data under `ModuleData`, and `SubModule.xml` defines game dependencies and the DLL entry point. The post-build target packages these assets alongside the compiled DLL.

## Repository conventions

- Gauntlet XML bindings must match view-model members exactly. Bindable properties use `[DataSourceProperty]` and call `OnPropertyChanged`/`OnPropertyChangedWithValue`; XML commands such as `Command.Click="ExecuteSwapTeam"` map directly to public methods on the current data source.
- Movie and prefab names are convention-based. Calls such as `LoadMovie(nameof(EnhancedBattleTestScreen), ...)` and `LoadMovie(nameof(CharacterSelectionView), ...)` require matching XML prefab names.
- UI state is deliberately backed by config objects rather than copied into a separate model. When adding or removing UI controls, update the config type, VM synchronization/recovery logic, XML binding, defaults/reset behavior, and XML serialization together.
- `BattleConfig` is serialized with `XmlSerializer`. Keep serializable config types public and parameterless, and preserve compatible public fields/properties when possible. The current filename is `spconfig-v2.xml`.
- Each team has one general group plus a fixed array of eight regular troop groups. General groups retain at least one troop row in the UI, while regular groups may be empty.
- Localized UI text normally starts with a `str_ebt_*` entry in `module_strings.xml`, whose `{=EnhancedBattleTest_*}` identifier is then defined in the language files. Preserve that identifier link and update relevant translations when adding user-visible text.
- Reflection and Harmony patches depend on Bannerlord private members and method signatures. Versioned decompiled sources are under `D:\develop\src\Bannerlord.Binaries\<version>`; audit `SubModule.xml`, assembly references, `EnhancedBattleTestSubModule`, and files under `src\Patch` together when changing supported game versions.
- Treat `source\package`, `bin`, and `obj` as generated output. Edit source assets under `source\EnhancedBattleTest\Modules`, not their packaged copies.
