# Enhanced Battle Test

A mod for Mount&Blade Bannerlord that provides more powerful custom battle.

## Features
- You can choose multiplayer characters or singleplayer characters that are used in campaign mode.

  You can choose at most 8 groups of troops for each side.

- For multiplayer battle test, you can choose perks, which is the same as in multiplayer mode.

- For singleplayer battle test, you can choose all the characters in campaign mode, except companions, as they are generated dynamically.

- You can adjust gender ratio for each group of troops.

- You can customize the banner of each team in banner editor in singleplayer battle test, with copy-paste feature.

- Configuration saving. The battle configuration is saved in "(user directory)\Documents\Mount and Blade II Bannerlord\Configs\EnhancedBattleTest\".

  The configuration for multiplayer battle test is saved in "mpconfig.xml" and that for singleplayer battle test is saved in "spconfig.xml".

## How to install
1. Copy `Modules` folder into Bannerlord installation folder(For example `C:\Program Files\Steam\steamapps\common\Mount & Blade II Bannerlord - Beta`). It should be merged with `Modules` of the game. Or use Vortex to install it automatically.

   Note that the other files should not be installed. They are source files used to build the mod and are for those who want to modify the mod.

## How to use
- Start the launcher and choose Single player mode. In `Mods` panel select `EnhancedBattleTest` mod and click `PLAY`.

- After starting:
  - Select a mode to play in main menu. Singpleplayer battle test may be slow to load because it would start a campaign game to load all the data. This problem may be resolved in the future.

  - You can select troops for each side of teams.

  - Click `Start` to enjoy the battle.

## FAQ
- Q: What does the "Tactic Level" option do?

- A: Tactic level determines what tactics the AI will use, such as "Charge", "Protect flank", "Forming a skirmish line", etc. It has different effect when the value is in 0-20, 20-50 and higher than 50, respectively.

- Q: What does the "Soldier Equipment Modifier" option do?

- A: In singleplayer mode, including campaign mode and custom battle mode, a random modifier may be added to every equipment of each soldier when it's spawned, such as "fine", "heavy", "balanced", or no modifier at all.

  - If you select "Random", then the behavior is the same as it is in Campaign mode: modifier will be randomly chosen and applied.

  - If you select "Average", then the bonus of the applied modifier will be the mathematical expectation of bonus of all possible modifiers. In this way, all the soldiers with the same equipments will have the same armor. No random behavior any more. 

  - If you select "None", then no modifier will be applied to equipment of soldiers.

## Troubleshoot
- If the launcher can not start:

  - Uninstall all the third-party mods and reinstall them one by one to detect which one cause the launcher cannot start.

- If it shows "Unable to initialize Steam API":

  - Please start steam first, and make sure that Bannerlord is in your steam account.

- If the game crashed after starting:

  - Please uncheck the mod in launcher and wait for mod update.

    Optionally you can tell me the step to reproduce the crash.

- If the game crashed when loading a siege battle or clicking "Begin assault", try to change the scene level. Some scenes lacks data for some scene level, so this may help.

## Source code
The source code is available at [GitLab](https://gitlab.com/lzh_mb_mod/enhancedbattletest).

## Contact with me
* Please mail to: lizhenhuan1019@qq.com

* This mod is originated from mod "Battle Test" written by "Modbed".
  
  Way to contact him:
  
  TaleWorlds forum: modbed
  
  youtube: modbed
  
  bilibili: modbed帅
  
  website: modbed.cn
