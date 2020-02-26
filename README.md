# Enhanced Battle Test

A mod for Mount&Blade Bannerlord that can test Caption mode battle locally.
### Features
- Test Battle Mode: You can choose where to spawn troops and they will be all spawned instantly.
- Custom Battle Mode: Use built-in mechanism to spawn troops. Troops that exceeds the battle size limit will be spawned later.
- Map selection. Including sergeant maps, skirmish maps, tdm map, etc.
  However, I removed siege maps because they are buggy.
  Custom battle mode contains sergeant maps only, because only sergeant maps contains corrrect spawning positions that custom battle mode requires.
  If you want more maps, you can edit the config file yourself, details below.
- Character selection. You can specify at most three types of troops for each team.
  Also you can select **perks** that is consistent with those in Multiplayer mode.
- Configuration saving. The battle configuration is saved in "(user directory)\Documents\Mount and Blade II Bannerlord\Configs\EnhancedBattleTest\". The configuration for Test Battle mode is saved in "EnhancedTestBattleConfig.xml" and that for Custom Battle mode is saved in "EnhancedCustomBattleConfig.xml". You can modify it to add more maps, but if you edit it incorrectly, the configuration will be reset to default, or the game may crash. I don't guarantee anything.
- Switching player's team. You can switch between player agent and the enemy commander to control their troops respectively.
- Controlling your bots after dead.
- Switching free camera.

## How to install
1. Copy `bin` and `Modules` into Bannerlord installation folder(For example `C:\Program Files\Steam\steamapps\common\Mount & Blade II Bannerlord - Beta`).

## How to use
- Start the mod by clicking `EnhancedBattleTest.bat` in `bin\Win64_Shipping_Client` that you have copied into Bannerlord installation folder.
- You can select troops for each side of teams.
- Test Battle Mode: You can choose where to spawn troops and they will be all spawned instantly.
- Custom Battle Mode: Use built-in mechanism to spawn troops. Troops that exceeds the battle size limit will be spawned later.
- Press and hold `TAB` key for a while to exit the battle scene.
- Press `numpad5` key to switch your team.
- Press `numpad6` key to switch between free camera and main agent camera.
- Press `f` key or `numpad6` key to control one of your troops after you being killed.
- Press `L` key to teleport player when in free camera mode.

## Build from source:
The source code is located in the `source` folder or available at https://gitlab.com/lzh_mb_mod/enhancedbattletest.
1. install .net core sdk
2. modify 6th line of `EnhancedBattleTest.csproj`, change `Mb2Bin` property to your bannerlord installation location
3. open a termial (powershell or cmd), run `dotnet msbuild -t:install`. This step will build `EnhancedBattleTest.dll` and copy it to `bin\Win64_Shipping_Client`

## Bug:
Some people say they can't launch the mod, if you are among them, try building from source code.

I guess there will be compilation error, If you are a programmer, it's not difficult to fix.

If you are a normal user, google the compilation error or post it to forum to ask for help.

If you find the cause of crash, please tell me.

## Contact with me:
* Please mail to: lizhenhuan1019@qq.com

* This mod is originated from mod "Battle Test" written by "Modbed", who does not maintain "Battle Test" anymore.
  
  Way to contact him:
  
  TaleWorlds forum: modbed
  
  youtube: modbed
  
  bilibili: modbed帅
  
  website: modbed.cn
