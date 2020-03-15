# Enhanced Battle Test

这是一个骑马与砍杀2：霸主的mod，可以做离线战斗测试。

## 功能
- Test Battle Mode：该模式中，你可以选择出生地点，所有部队都会一次性生成。

- Custom Battle Mode：该模式采用砍二内建的生成部队方式：部队在场景中固定的地点生成。超出人数限制的部队会作为援军稍后加入战场。

- 地图选择：包括领军地图，冲突地图，死斗地图和部分攻城图。攻城图目前不稳定，容易崩溃。

  Custom Battle Mode只包含领军地图，因为只有领军地图有该模式要求的军队出生点信息。

  如果你想要体验更多地图，你可以自己编辑配置文件，详细内容见后文。

- 角色选择：你可以为每方队伍选择最多3种部队。你还可以选择和联机一致的perks。

- 保存配置：保存战斗配置的文件夹为"(user directory)\Documents\Mount and Blade II Bannerlord\Configs\EnhancedBattleTest\"。

  Test Battle Mode的配置保存在"EnhancedTestBattleConfig.xml"文件中，Custom Battle Mode的配置保存在"EnhancedCustomBattleConfig.xml"文件中。

  比如你可以修改配置来添加更多地图，但如果你编辑有误，配置可能会被初始化为默认内容，或者游戏可能会崩溃，我对可能发生的情况不做任何保证。

- 切换玩家所在的队伍：你可以在控制玩家队伍的领队和控制敌军的领队之间切换，从而做到对两方部队轮流下令。

- 玩家死后可以控制其小兵。该小兵将会称为当前队伍的领队。

- 切换自由视角。

- 不死模式：开启后任何单位都不会掉血和死亡。

- 调整战斗AI：你可以将战斗ai在0-100间调整。

- 自定义玩家角色，详细内容如下。

## 如何安装
1. 复制`bin`和`Modules`两个文件夹到砍二的安装目录下（例如`C:\Program Files\Steam\steamapps\common\Mount & Blade II Bannerlord - Beta`)。

2. 游戏更新后如果mod出现问题，请尝试重装mod。若得不到解决，可以联系我并/或等待mod更新。

## 如何使用
- 当前联机测试中，官方的加载器禁用了单机模式，所以需要通过点击砍二安装目录下，`bin\Win64_Shipping_Client`中的`EnhancedBattleTest.bat`来启动。

- 配置界面介绍：
  - 最上方的左右箭头用于切换地图，中间的名称即为地图名称。

  - Soldiers per Row: 每行士兵数量。

  - Formation Position(x, y)：玩家队伍出生位置。

  - Formation Direction(x, y)：玩家队伍朝向。

  - Sky Brightness：天空亮度。-1表示不做修改，采用场景的默认设置。

  - Rain Density：下雨程度。-1表示不做修改，采用场景的默认设置。

  - Player Character：选择玩家控制的角色。

    - 点击右侧按钮后，进入角色选择页面。角色选择页面从左至右依次为：

      - 联机阵营。

      - 士兵类型。Infantry为步兵，Ranged为射手，Cavalry为骑兵，HorseArcher为骑射手。

      - 兵种名称。

      - 兵种的第一个可选装备。

      - 兵种的第二个可选装备。

    - 选择完成后，点击界面下方的done确认。

  - Spawn Player：开局是否生成玩家角色。若不生成玩家角色，玩家仍可按F键控制下属士兵。

  - Player Troop 1/2/3：玩家的一至三号部队的角色。

  - Player Troop 1/2/3 Count：玩家的一至三号部队的人数。

  - Enemy Troop 1/2/3：敌军的一至三号部队的角色。

  - Enemy Troop 1/2/3 Count：敌军的一至三号部队的人数。

  - Enemy Commander：敌军将领的角色。

  - Spawn Enemy Commaner：是否生成敌军将领。

  - Distance：敌军部队和己方部队的距离。

  - Enemy Charge：敌军是否开局冲锋。

  - Disable Dying：是否禁用死亡。若是则所有角色不掉血。

  - Change Combat AI：是否改变战斗AI。若是则右侧的数值对所有单位生效，若否则所有单位的战斗AI为各自默认的设置。

  - Comabt AI(0-100)：要改变的战斗AI的数值，只有左侧的Change Combat AI选中才生效。有效范围在0-100。

  - Save And Start：保存配置并开始战斗。

  - Save：保存配置。

  - Load Config：从文件中加载配置。

  - Exit：退出，返回到标题画面。

- 局内操作：
  - 按住`TAB`键来退出战斗。

  - 按小键盘5键来切换队伍。

  - 按小键盘6键来切换自由视角。

  - 按`F`键或小键盘6键来在玩家死后控制其小兵。

  - 按小键盘7键来切换不死模式。

  - 按`L`键在自由视角下让玩家瞬移到镜头位置。

## 如何自定义角色
- 你可以通过修改`Modules\EnhancedBattleTest\ModuleData\mpcharacters.xml`文件中，id为`player_character_1`，`player_character_2`和`player_character_3`的xml元素来自定义角色。

- 这个角色在`Modules\EnhancedBattleTest\ModuleData\mpclassdivisions.xml`中被引用，该文件定义了角色的护甲、移速和其它属性。

- **然而**，自从砍二b0.8.0版本开始（也许更早），将第三方mod中的`mpclassdivisions.xml`和`Native`中的`mpclassdivisions.xml`合并，并解析读取，实现得**不正确**：

  xml元素间的空格未被忽略，游戏会因此崩溃。

  这是砍二自身的bug，临时的解决方案是移除`Native`和本mod中的两个`mpclassdivisions.xml`文件里的xml元素间的所有换行和空格。

  我已经帮你把这些做好了。所以如果你不修改这两个文件，你不需要关心这些。

  如果你需要修改这两个文件中的任何一个，记得修改完毕后将其中xml元素间的所有换行和空格删除。

  我用的是vscode的xml插件来自动删除空格。

- 因此如果你修改了其中的文件，或者游戏更新了（从而更新了Native中的`mpclassdivisions.xml`），若mod不能启动了，请尝试重装mod。

  若重装不起作用，你可以尝试在`Modules\EnhancedBattleTest\SubModule.xml`文件中移除下面的内容：
  ```
  <XmlNode>
	<XmlName id="MPClassDivisions" path="mpclassdivisions"/>
  </XmlNode>
  ```
  这样，游戏就应当不会再加载本mod中的`mpclassdivisions.xml`文件，也就不会将它和`Native`的对应文件合并，从而不可能再触发这个bug。

  但这样做会导致无法自定义角色。

- 别怪我，请怪TaleWorlds写的代码(bug)。

- 希望这个bug早日被TaleWorlds修复。

## 从源代码构建
源代码位于`source`文件夹下，在[https://gitlab.com/lzh_mb_mod/enhancedbattletest](https://gitlab.com/lzh_mb_mod/enhancedbattletest) 中也可以获得源代码。

1. 安装.NET core sdk。

2. 将`EnhancedBattleTest.csproj`中第6行的`Mb2Bin`属性修改为你的砍二安装位置。

3. 打开一个终端shell(powershell或者cmd)，运行`dotnet msbuild -t:install`。这一步会构建`EnhancedBattleTest.dll`并将它复制到`bin\Win64_Shipping_Client`中。

## 解决问题
- 若遇到无法进入mod的情况，首先请在steam中校验文件完整性，之后再**重新安装**mod。
  
  - 注意每次游戏更新后（尤其是校验文件完整性之后的游戏更新后），若你想启动mod，你都应当先重新安装mod。

- 若提示"Unable to initialize Steam API":

  - 请先启动Steam，并确保砍二在你登录的Steam账号的库中.

- 若在没有进入主菜单的情况下崩溃：
  
  - mod很可能没有正确安装，请重新安装mod。若没用：

  - 尝试将`EnhancedBattleTest.bat`中的`ManagedStarter.exe`替换为`NativeStarter.exe`。

    这个方法已知对一部分无法启动mod的情况有用。

- 如果在点击主菜单中的按钮时崩溃：

  - 重装mod。请阅读`如何自定义角色`小节以了解原因。

- 如果在战斗配置界面中，选择数字时崩溃：

  - 这是由砍二的bug导致。你能做的是避免触发这个bug。

- 若除攻城图之外的战斗中mod崩溃

  - 请把崩溃报告打包并通过下面的邮箱发给我。（崩溃报告位于`bin\Win64_Shipping_Client\`中，以崩溃时间为文件夹的名称）

## 联系我
* 请发邮件到：lizhenhuan1019@qq.com

* 这个mod起源于Modbed做的"Battle Test"。

  联系他的方法：

  TaleWorlds论坛：modbed

  Youtube：modbed

  bilibili：modbed帅

  website：modbed.cn
