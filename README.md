# Enhanced Battle Test

A mod for Mount&Blade Bannerlord that can test Caption mode battle locally.

## How to install
1. Copy `bin` and `Modules` into Bannerlord installation folder(For example `C:\Program Files\Steam\steamapps\common\Mount & Blade II Bannerlord - Beta`).

## How to use
1. start the mod by clicking `EnhancedBattleTest.bat`.
2. press and hold `TAB` key for a while to exit the battle scene.
3. press `c` key to switch between free camera and main agent camera.
4. press `f` key to control one of your troops after you being killed.

## Build from source:
The source code is located in the `source` folder.
1. install .net core sdk
2. modify 6th line of `BattleTest.csproj`, change `Mb2Bin` property to your bannerlord installation location
3. open a termial (powershell or cmd), run `dotnet msbuild -t:install`. This step will build `BattleTest.dll` and copy it to `bin\Win64_Shipping_Client`

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
  
  bilibili: 月光暖大床
  
  website: modbed.cn
