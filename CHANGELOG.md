# Changelog

## [0.5.4] - 2020-03-15
### Fixed
- Keep compatible with b0.8.5.

### Added
- Add more maps.

- Add html page for README.

### Changed
- Change "Enemy Charge" to "Charge" that let both sides to charge.

- Change UI text.

## [0.5.3] - 2020-03-04
### Added
- Add more maps.

### Fixed
- Keep compatible with b0.8.1.

- Fix a bug of order UI.

## [0.5.2] - 2020-03-01
### Added
- Add readme file for Chinese.

### Changed
- Changed some UI texts to make them more consistent.

### Fixed
- Fix wrong default spawning position in map `mp_skirmish_map_005` and `mp_skirmish_map_008`.

## Removed
- Removed mission boundary in Test Battle mode.

## [0.5.1] - 2020-02-28
### Removed
- Remove map boundary in test battle mode as suggested.

### Fixed
- Fix calvary spawning bug.

## [0.5.0] - 2020-02-28
### Added
- Add no dying mode: enable by pressing `numpad 7`.

- Add support for customizing character. Please read `README.md` first before modifying.

## [0.4.7] - 2020-02-27
### Fixed
- Fix the bug that player can not run initially in test battle mode.

## [0.4.6] - 2020-02-26
### Added
- Press L can teleport in battle test mode now.

### Fixed
- Fix crash when number of troops is 0.

- Fix wrong spawning interval between calvary and infrantry.

## [0.4.5] - 2020-02-26

## Fixed
- Fix a bug in config UI;

- fix wrong spawning behaviour of calvary in test battle mode.

## [0.4.4] - 2020-02-25

### Changed
- Troops will not charge at the begining in test battle mode.

### Fixed
- Fix spawning behaviour in test battle mode.


## [0.4.3] - 2020-02-25

### Added
- Add perks to Custom Battle mode.

### Changed
- Now player and enemy general are added to formation 4.

### Fixed
- Fix the bug that adjusting troop count does not work.

- Fix the bug that battle ends when only player is alive in player team in custom battle mode.

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
