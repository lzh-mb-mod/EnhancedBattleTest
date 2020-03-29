# Enhanced Battle Test

这是一个骑马与砍杀2：霸主的mod，可以做离线战斗测试。

## 功能
- Test Battle Mode：该模式中，你可以选择出生地点，所有部队都会一次性生成。

- Custom Battle Mode：该模式采用砍二内建的生成部队方式：部队在场景中固定的地点生成。超出人数限制的部队会作为援军稍后加入战场。

- Siege Battle Mode: 该模式中AI为攻城战的AI；加入了部署阶段。

- 地图选择：包括领军地图，冲突地图，死斗地图和部分攻城图

  攻城图目前不稳定，容易崩溃。由于攻城图的AI网格和其它AI用到的实体不完整，攻城图的AI可能表现很差。

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

- 改变AI的战术选项。

- 调整战斗AI：你可以将战斗ai在0-100间调整。

- 自定义玩家角色，详细内容如下。

## 如何安装
1. 复制`bin`和`Modules`两个文件夹到砍二的安装目录下（例如`C:\Program Files\Steam\steamapps\common\Mount & Blade II Bannerlord - Beta`)。

## 如何使用
- 当前联机测试中，官方的加载器禁用了单机模式，所以需要通过点击砍二安装目录下，`bin\Win64_Shipping_Client`中的`EnhancedBattleTest.bat`来启动。若它崩溃，可尝试运行`EnhancedBattleTest-Alternative.bat`.

- 配置界面介绍：
  - 最上方的左右箭头用于切换地图，中间的名称即为地图名称。

  - Soldiers per Row: 每行士兵数量。

  - Formation Position(x, y)：玩家队伍出生位置。

  - Formation Direction(x, y)：玩家队伍朝向。

  - Distance：敌军部队和己方部队的距离。

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

  - Is Player Attacker：玩家是否为攻击方。若取消勾选则玩家为防守方。

  - Player Troop 1/2/3：玩家的一至三号部队的角色。

  - Player Troop 1/2/3 Count：玩家的一至三号部队的人数。

  - Enemy Troop 1/2/3：敌军的一至三号部队的角色。

  - Enemy Troop 1/2/3 Count：敌军的一至三号部队的人数。

  - Enemy Commander：敌军将领的角色。

  - Spawn Enemy Commaner：是否生成敌军将领。

  - More Options：更多选项

    - Enable AI For：选择是否分别为双方队伍启用指挥AI（AI代理指挥）。

      - None：双方队伍均无指挥AI。

      - EnemyOnly：仅敌军有指挥AI。

      - PlayerOnly：仅玩家队伍有指挥AI。

      - Both：双方队伍均有指挥AI。
    
    - Attacker Tactic Options：攻击方启用的战术。右侧列表中高亮选中的战术会被AI不断考虑并选择其中一个执行。

      - Charge：冲锋。

      - FullScaleAttack：全军攻击。

      - DefensiveEngagement：防御性接战。

      - DefensiveLine：防御性线阵。

      - DefensiveRing：防御性圆阵。

      - HoldTheHill：防守高坡。

      - ArchersOnTheHill：远程待在坡上，步兵进攻。

      - RangedHarassmentOffensive：远程骚扰攻势

      - FrontalCavalryCharge：骑兵先行冲锋。

      - CoordinatedRetreat：有序撤退。

    - Switch Team：（仅战斗中）切换玩家队伍。

    - Switch Free Camera：（仅战斗中）切换自由视角。

    - Disable Death：是否禁用死亡。若是则所有角色不掉血。

    - Reset Mission：（仅在Test Battle模式的战斗中）重置关卡。

    - Toggle Pause：（仅战斗中）切换暂停。

    - Reset Speed：（仅战斗中）重置战斗速度。

    - Slow Motion Factor：（仅战斗中）调整战斗速度倍率。

  - Use Realistic Blocking：是否使用更真实的格挡。

  - No Banner on Friendly Troops：关闭队友标识。

  - No Kill Notification：关闭右上角击杀报告。

  - Make Grunt Voice：玩家下令后部下是否出声回应。

  - Has Boundary: 地图是否设置边界。

  - Change Combat AI：是否改变战斗AI。若是则右侧的数值对所有单位生效，若否则所有单位的战斗AI为各自默认的设置。

  - Comabt AI(0-100)：要改变的战斗AI的数值，只有左侧的Change Combat AI选中才生效。有效范围在0-100。

  - Save And Start：保存配置并开始战斗。

  - Save：保存配置。

  - Load Config：从文件中加载配置。

  - Exit：退出，返回到标题画面。

- 局内操作：

  - 按`F`键或`F10`键来在玩家死后控制其小兵。

  - 按`F9`键来切换队伍。

  - 按`F10`键来切换自由视角。

  - 按`F11`键来切换不死模式。

  - 按`F12`键来重置关卡（仅在Test Battle模式的战斗中)。

  - 按`O(字母)`键来打开更多设置，如战术AI选项。

  - 按`P`键来暂停游戏。

  - 按住`TAB`键来退出战斗。

  - 按`I`键来获取玩家或相机的位置。

  - 按`L`键在自由视角下让玩家瞬移到镜头位置。

## 如何自定义角色
- 若要自定义角色，你需要编写两个XML元素：`NPCCharacter`和`MPClassDivision`。

- `NPCCharacter`定义了角色名称，它所属的文化，身体属性，装备等等。

- `MPClassDivision`定义了perk，护甲，移速等等在联机中生效的属性。由于这个mod使用和多人模式相同的机制生成角色，你需要定义`MPClassdivision`来让你定义的`NPCCharacter`出现在mod中。

- `NPCCharacter`和`MPClassDivision`的同一性由`id`属性决定：有相同`id`的角色是同一个角色，在文件中之后定义的角色会覆盖先前定义的同一个角色（装备除外，装备列表会合并，最终装备会从装备列表中随机选取）。

  你需要在`MPClassDivision`的`hero`和`troop`属性中填写`NPCCharacter`的id，以将二者关联在一起。

- 若你仅需要自定义少于4个的角色，你可以直接修改`Modules\EnhancedBattleTest\ModuleData\customcharacters.xml`中的`id`为`player_character_1`，`player_character_2`，`player_character_3`的这三个XML元素。

- 若你需要自定义多于3个的元素，那么：

- 在`customcharacters.xml`中，添加带有与已有角色不同的`id`的角色。例如你可以复制粘贴已有的`NPCCharacter`，将`id`改为新的id，并根据需要修改其它属性。

- 在`Modules\EnhancedBattleTest\ModuleData\mpclassdivisions.xml`中，添加一个和你的角色相关联的`MPClassDivision`元素。例如，你可以复制粘贴一个已有的`MPClassDivision`元素，将`id`改为新的id，将`hero`和`troop`属性更改为你的角色的`id`，最后根据需要修改其它属性。

## 从源代码构建
源代码位于`source`文件夹下，在[https://gitlab.com/lzh_mb_mod/enhancedbattletest](https://gitlab.com/lzh_mb_mod/enhancedbattletest) 中也可以获得源代码。

1. 安装.NET core sdk。

2. 将`EnhancedBattleTest.csproj`中第6行的`Mb2Bin`属性修改为你的砍二安装位置。

3. 打开一个终端shell(powershell或者cmd)，运行`dotnet msbuild -t:install`。这一步会构建`EnhancedBattleTest.dll`并将它复制到`bin\Win64_Shipping_Client`中。

## 解决问题
- 若提示"Unable to initialize Steam API":

  - 请先启动Steam，并确保砍二在你登录的Steam账号的库中.

- 若在没有进入主菜单的情况下崩溃：

  - 尝试运行`EnhancedBattleTest-Alternative.bat`而非`EnhancedBattleTest.bat`。

    这个方法已知对一部分无法启动mod的情况有用。或者：
  
  - 尝试重新安装mod。若没用：

  - 等待mod更新。同时你可以将崩溃报告发给我。具体方法见末尾。

- 如果在点击主菜单中的按钮时崩溃：

  - 若你自定义了角色，请确保你的修改语法正确。若你无法修改正确，你可以重装mod以放弃自定义角色。否则：

  - 等待mod更新并将崩溃报告发给我。

- 如果在战斗配置界面中，选择数字时崩溃：

  - 这是由砍二的bug导致。你能做的是避免触发这个bug。

- 若在载入战斗时mod没有显示任何消息就崩溃：

  - 尝试减少士兵数量或降低画质。

- 若除攻城图之外的战斗中mod崩溃

  - 请把崩溃报告打包并通过下面的邮箱发给我。

### 如何将崩溃报告发给我
- 当崩溃提示窗口弹出并显示如下内容时点击是：
  > The application faced a problem. We need to collect necessary files to fix this problem. Would you like to update these files now?

- 在`bin\Win64_Shipping_Client\crashes\`文件夹中，应当有一个文件夹显示了崩溃时间。将该文件夹内的文件发给我，尤其是`dump.dmp`文件。邮箱地址见下文。

## 联系我
* 请发邮件到：lizhenhuan1019@qq.com

* 这个mod起源于Modbed做的"Battle Test"。

  联系他的方法：

  TaleWorlds论坛：modbed

  Youtube：modbed

  bilibili：modbed帅

  website：modbed.cn
