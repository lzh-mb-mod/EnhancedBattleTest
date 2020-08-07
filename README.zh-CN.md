# Enhanced Battle Test

这是一个骑马与砍杀2：霸主的mod，提供了功能更丰富的自定义战斗。

## 功能
- 你可用选择多人模式中的角色或单人战役模式中的角色。

  你可以为每方最多选择8组部队。

- 对于多人战斗测试，你可以选择和联机模式相同的perk。

- 对于单人战斗测试，你可以选择战役模式中的所有角色，除了同伴，因为同伴是动态生成的。

- 你可以调整每个部队的性别比例。

- 你可用自定义每个队伍的纹章，支持复制粘贴。

- 保存配置：保存战斗配置的文件夹为"(user directory)\Documents\Mount and Blade II Bannerlord\Configs\EnhancedBattleTest\"。

  多人战斗测试的配置保存在"mpconfig.xml"文件中，单人战斗测试的配置保存在"spconfig.xml"文件中。

## 如何安装
1. 复制`Modules`文件夹到砍二的安装目录下（例如`C:\Program Files\Steam\steamapps\common\Mount & Blade II Bannerlord - Beta`)，和砍二本体的Modules文件夹合并。或者你可以用Vortex自动安装。

   注意其它文件不应当被安装。这些文件是用来构建mod的源文件，只为需要修改mod的人准备。

## 如何使用
- 启动游戏启动器，并选择单人模式(Singleplayer)。在Mods选项卡中勾选`EnhancedBattleTest`并点击`Play`。

- 启动后：

  - 在主菜单中选择一个模式进入。单人战斗测试加载可能较慢，因为它会通过创建战役模式的游戏来加载所需数据。这个问题可能会在将来解决。

  - 你可以为每方选择部队。

  - 点击`开始`以开始战斗。

## 常见问题
- 问：“战术等级”选项的作用是什么？

- 答：战术等级决定了AI指挥时会采用哪些战术，例如“冲锋”，“保护侧翼”，“组织散兵线”等等。战术等级在0-20，20-50和50以上三个区间中有不同的效果。

- 问：“士兵装备前缀”选项的作用是什么？

- 答：在单人模式中（包括战役模式和自定义战斗模式），每个士兵进入战场时，身上的每个装备可能会加上一个随机的前缀，例如“优质”，“重”，“平衡的”等等，或者不加任何前缀。

  - 若你选择“随机”，那么士兵进入战场时，会按照单机原有行为来随机选取前缀。

  - 若你选择“平均”，那么士兵进入战场时，会将所有可能的前缀结合其选取概率，计算加成的期望，然后将期望应用到士兵的装备上。这样同并在

  - 若你选择“无”，则士兵进入战场时装备不会加上任何前缀。


## 解决问题
- 若启动器无法启动：

  - 卸载所有第三方mod，然后一个个重装它们来找出哪个mod导致了启动器不能启动。

- 若提示"Unable to initialize Steam API":

  - 请先启动Steam，并确保砍二在你登录的Steam账号的库中.

- 若mod启动后游戏崩溃：

  - 请取消载入该mod并等待mod更新。

    你可以选择告诉我重现崩溃的步骤。

- 若在载入战斗时mod没有显示任何消息就崩溃：

  - 尝试减少士兵数量或降低画质。

## 源代码
源代码可从[GitLab](https://gitlab.com/lzh_mb_mod/enhancedbattletest)获得。


## 联系我
* 请发邮件到：lizhenhuan1019@qq.com

* 这个mod起源于Modbed做的"Battle Test"。

  他的联系方式：

  TaleWorlds论坛：modbed

  Youtube：modbed

  bilibili：modbed帅

  website：modbed.cn

## 致谢
* 感谢 Murat Mansur Çıkıntoğlu 为本mod提供土耳其语的翻译。
