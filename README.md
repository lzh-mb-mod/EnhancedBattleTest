# Enhanced Battle Test

A mod for Mount&Blade Bannerlord that can test battle locally.

## Features
- Test Battle Mode: You can choose where to spawn troops and they will be all spawned instantly.

- Custom Battle Mode: Use built-in mechanism to spawn troops. Troops that exceeds the battle size limit will be spawned later.

- Siege Battle Mode: Use AI for siege battle. Add deployment stage.

- Map selection. Including sergeant maps, skirmish maps, tdm maps and some siege maps.

  Siege maps may crash. Due to incompleteness of AI navigation mesh and other entities in siege maps, AI may perform poorly in those maps.

  Custom battle mode contains sergeant maps only, because only sergeant maps contains corrrect spawning positions that custom battle mode requires.

  If you want more maps, you can edit the config file yourself, details below.

- Character selection. You can specify at most three types of troops for each team.

  Also you can select **perks** that is consistent with those in Multiplayer mode.

- Configuration saving. The battle configuration is saved in "(user directory)\Documents\Mount and Blade II Bannerlord\Configs\EnhancedBattleTest\".

  The configuration for Test Battle mode is saved in "EnhancedTestBattleConfig.xml" and that for Custom Battle mode is saved in "EnhancedCustomBattleConfig.xml".

  You can modify it to add more maps, but if you edit it incorrectly, the configuration will be reset to default, or the game may crash. I don't guarantee anything.

- Switching player's team. You can switch between player agent and the enemy commander to control their troops respectively.

- Controlling your bots after dead.

- Switching free camera.

- Undead mode. Any agent will not die after switched on.

- Changing AI tactic options.

- Adjusting combat AI between 0 and 100.

- Customizing player characters, details below.

## How to install
1. Copy `bin` and `Modules` into Bannerlord installation folder(For example `C:\Program Files\Steam\steamapps\common\Mount & Blade II Bannerlord - Beta`).

### Note
- Due to a bug in official code, if a game update changes multiplayer perks (so that it updates `Native\ModuleData\mpclassdivisions.xml`), the mod will crash.
  
  You can wait for mod update or play multiplayer mode. If you want to play this mod instantly, you should remove spaces between xml elements in `mpclassdivisions.xml`(I use vscode with `XML tools` extension to automatically do that) and make a backup up of it. You should then reinstall the mod and overwrite this file using the backup.

- Please read `How to customizing characters` section for details and reason.

## How to use
- Start the mod by clicking `EnhancedBattleTest.bat` in `bin\Win64_Shipping_Client` that you have copied into Bannerlord installation folder. If it crashed, try to run `EnhancedBattleTest-Alternative.bat` instead.

- You can select troops for each side of teams.

- Press `F` key or `F10` key to control one of your troops after you being killed.

- Press `F9` to switch your team.

- Press `F10` to switch between free camera and main agent camera.

- Press `F11` to disable dying.

- Press `F12` to reset mission (available in `Test Battle mode` only).

- Press `O(letter)` to adjust more settings including team AI tactic options, etc.

- Press `P` key to pause game.

- Press and hold `TAB` key for a while to exit the battle scene.

- Press `I` key to get player or camera position.

- Press `L` key to teleport player when in free camera mode.

## How to add more maps
- You can go to `Modules\Native\SceneObj` to find available maps.

- To add more maps, you need to edit the configuartion file(in folder "(user directory)\Documents\Mount and Blade II Bannerlord\Configs\EnhancedBattleTest\").

- The maps available in mod are under the xml element `sceneList`.

  The element `SceneInfo` represents a map and related config.

  So you need to add a `SceneInfo` element as a child of `sceneList`, just like the other maps.

  For example you can copy the text between `<SceneInfo>` and `</SceneInfo>`, then replace the text between `<name>` and `</name>` with the folder name of the map you want to add.

  The other config like formation positions can be configured in the game.

## How to customize characters
- You can customize your characters by modifying the xml elements with id `player_character_1`, `player_character_2` and `player_character_3` in `Modules\EnhancedBattleTest\ModuleData\mpcharacters.xml`.

- These characters are referred in another file `Modules\EnhancedBattleTest\ModuleData\mpclassdivisions.xml`, in which armors, movement speed, perks and other properties of the character are defined.

- **However**, since Bannerlord b0.8.0(maybe since earlier version, I don't know), merging those two `mpclassdivisions.xml` files (one in `native` and one in this mod), and parsing them is **NOT** correctly implemented: the spaces between xml elements are not ignored, and the game will crash.

  This is a bug in Bannerlord, and the work-around is to remove all the spaces between xml elements in both files.
  
  I have done this for you. So you don't need to worry about it if you don't modify those two `mpclassdivisions.xml` files.

  If you need to modify any of those two files, please remember to remove all the spaces between the xml elements just like I do.

  I use vscode with xml extension to remove spaces automatically.

  You can install vscode, then open it and install `XML Tools` extension. Open the file, right click and select `Minify xml` to remove all the spaces.

- If you modified those files or the game updated(so `mpclassdivisions.xml` in Native may be updated), and the mod could not start, you can wait for mod update or go to play multiplayer mode.
  
  If you want to play this mod instantly, you can remove all the spaces in `mpclassdivisions.xml`, make backup of them and overwrite them using backups when reinstalling the mod.

  Or if you don't need character customization, you can remove the following content in `Modules\EnhancedBattleTest\SubModule.xml`:
  ```
  <XmlNode>
		<XmlName id="MPClassDivisions" path="mpclassdivisions"/>
	</XmlNode>
  ```
  and try to start the mod again.
  
  This should make the game not to load `mpclassdivisions.xml` in this mod anymore, and do not merge it with the one in `Native`. So it shound be impossible to trigger the bug mentioned above.
  
  However, the characters defined in `mpcharacters.xml` can no longer be spawned in the game.

- Don't blame me, blame the code that TaleWorlds wrote.

- Hope to see this bug fixed soon.


## Build from source
The source code is located in the `source` folder or available at [https://gitlab.com/lzh_mb_mod/enhancedbattletest](https://gitlab.com/lzh_mb_mod/enhancedbattletest).

1. install .net core sdk

2. modify 6th line of `EnhancedBattleTest.csproj`, change `Mb2Bin` property to your bannerlord installation location

3. open a termial (powershell or cmd), run `dotnet msbuild -t:install`. This step will build `EnhancedBattleTest.dll` and copy it to `bin\Win64_Shipping_Client`

## Troubleshoot
- If it shows "Unable to initialize Steam API":

  - Please start steam first, and make sure that Bannerlord is in your steam account.

- If the mod crashed without entering Main menu:

  - If the game updated, wait for mod update. Or:

  - Reinstall the mod. If not working:

  - Try to run `EnhancedBattleTest-Alternative.bat` instead of `EnhancedBattleTest.bat`.

    This works for some people.

- If the mod crashed when clicking buttons in main menu:
  
  - Please read `How to customize characters` section and follow instructions in it. Or you can wait for mod update.

- If the mod crashed when selecting numbers in battle config UI:
  
  - This is caused by bugs in Bannerlord. Maybe you can avoid to trigger this bug.

- If the mod exited without showing any message when loading battles:

  - Try to reduce soldier numbers or graphic quality.

- If the mod crashed in battle (except in siege map):

  - Please pack and send the crash report in folder `bin\Win64_Shipping_Client\(crash time)` to me via email below.

## Contact with me
* Please mail to: lizhenhuan1019@qq.com

* This mod is originated from mod "Battle Test" written by "Modbed", who does not maintain "Battle Test" anymore.
  
  Way to contact him:
  
  TaleWorlds forum: modbed
  
  youtube: modbed
  
  bilibili: modbed帅
  
  website: modbed.cn
