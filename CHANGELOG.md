# Changelog

## [0.4.2] - 2020-02-25

### Added
- Now each team has three formations.

- Add enemy team commander.

### Changed
- Now Numpad6 can also let player control troop after dead.

- Changed banner and cloth color choosing logic.

### Fixed
- Fix the bug that player agent is not alarmed after switch to free camera.

## [0.4.1] - 2020-02-23

### Changed
- Only keeps sergeant map in custom battle.

### Fixed
- Fix bug that pressing `esc` in config view will not return to main menu.

## [0.4] - 2020-02-23

### Added
- Add custom battle: Spawn troops in built-in way.
- Add switch team function.

### Fixed
- Fix the bug that Battle Config shown does not correctly reflect the actual change after clicking `Reset to Default`.

## [0.3.9] - 2020-02-17

### Changed
- Released file structure is adjusted to make it easier to install.

## [0.3.8] - 2020-02-17

### Added
- Battle Config can be saved to file now. Save path is `(User Home)\Documents\Mount and Blade II Bannerlord\Configs\EnhancedBattleTest\Param.xml`

- Rain density can now be adjusted.

## [0.3.7] - 2020-02-16

### Added
- Now player can choose different maps. Spawning positions are all elaborately choosed.

- Now player can specify sky brightness.

### Changed
- Ending mission will return to battle test menu now.
- Restore AI think time and apply time. Set them is useless because the engine will overwrite them.

### Fixed
- Fix more crashing cases.

## [0.3.6] - 2020-02-15

### Added
- Now troops will make grunt after being issued an order.

### Changed
- Set AI think time and apply time both to 0.

### Fixed
- Fix crashing when player troop number is 0.

## [0.3.5] - 2020-02-15

### Added
- Now player can control one of the troops by pressing `F` after being killed.

### Changed
- Change kill notification to single player style.
- Use built-in way to leave mission.

### Fixed
- Fix a bug that soldiers will not recover from victory celebration after player issueing an order when soldiers only have one weapon in slots.

## [0.3.4] - 2020-02-14

### Changed
- Keep compatible with Bannerlord `b0.7.1`.
- Change map to `mp_sergeant_map_001` and adjust default formation position.

### Fixed
- Fix a bug that changing formation position does not work.

## [0.3.3] - 2020-02-13

### Added
- Add victory celebration for victory troops. Celebration will end after issueing orders.
- Add agent label which is missed after v0.3.1.
- Add escape menu and option UI.

## [0.3.2] - 2020-02-13

### Changed
- Change team color to the color of culture.
- Do not use any banner.

### Fixed
- Fix the bug that the selected perk is displayed incorrectly after changing selected character.
