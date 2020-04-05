using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class AddEntityLogic : MissionLogic
    {
        private BattleConfigBase _config;
        public AddEntityLogic(BattleConfigBase config)
        {
            _config = config;
        }

        public override void EarlyStart()
        {
            base.EarlyStart();

            AddEntity("attacker", _config.FormationPosition, _config.FormationDirection,
                _config.playerTroops, _config.SoldiersPerRow);
            AddEntity("defender", _config.FormationPosition + _config.Distance * _config.FormationDirection,
                -_config.FormationDirection, _config.enemyTroops, _config.SoldiersPerRow);
        }

        private void AddEntity(string sideString, Vec2 position, Vec2 direction, ClassInfo[] classInfos, int soldiersPerRow)
        {
            {
                var name = "sp_" + sideString + "_" + Utility.CommanderFormationClass().ToString().ToLower();
                var entity = this.Mission.Scene.GetFirstEntityWithName(name);
                var matrixFrame = Utility.ToMatrixFrame(Mission.Scene, position + 2 * direction, direction);
                if (entity != null)
                    entity.SetGlobalFrame(matrixFrame);
                else
                    GameEntity.Instantiate(this.Mission.Scene, name, matrixFrame);
            }
            float distance = 0;
            for (int i = 0; i < 3; ++i)
            {
                var name = "sp_" + sideString + "_" + Utility.GetActualFormationClass(classInfos[i], (FormationClass)i).ToString().ToLower();
                var entity = this.Mission.Scene.GetFirstEntityWithName(name);
                var matrixFrame = Utility.ToMatrixFrame(Mission.Scene, position - distance * direction, direction);
                if (entity != null)
                    entity.SetGlobalFrame(matrixFrame);
                else
                    GameEntity.Instantiate(this.Mission.Scene, name, matrixFrame);
                var region = Utility.GetFormationArea(FormationClass.Infantry, classInfos[i].troopCount,
                    soldiersPerRow);
                distance += region.Item2;
            }
        }
    }
}
