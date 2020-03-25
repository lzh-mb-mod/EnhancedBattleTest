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
        private EnhancedSiegeBattleConfig _config;
        public AddEntityLogic(EnhancedSiegeBattleConfig config)
        {
            _config = config;
        }

        public override void EarlyStart()
        {
            base.EarlyStart();

            AddEntity("attacker", _config.FormationPosition, _config.FormationDirection,
                _config.playerTroops.Select(info => info.troopCount).ToArray(), _config.SoldiersPerRow);
            AddEntity("defender", _config.FormationPosition + _config.Distance * _config.FormationDirection,
                -_config.FormationDirection, _config.enemyTroops.Select(info => info.troopCount).ToArray(),
                _config.SoldiersPerRow);
        }

        private void AddEntity(string sideString, Vec2 position, Vec2 direction, int[] troopCount, int soldiersPerRow)
        {
            GameEntity.Instantiate(this.Mission.Scene, "sp_" + sideString + "_horsearcher",
                Utility.ToMatrixFrame(Mission.Scene, position + 2 * direction, direction));
            GameEntity.Instantiate(this.Mission.Scene, "sp_" + sideString + "_infantry",
                Utility.ToMatrixFrame(Mission.Scene, position, direction));
            var region1 = Utility.GetFormationArea(FormationClass.Infantry, troopCount[0],
                soldiersPerRow);
            GameEntity.Instantiate(this.Mission.Scene, "sp_" + sideString + "_archer",
                Utility.ToMatrixFrame(Mission.Scene,
                    position - direction * region1.Item2,
                    direction));
            var region2 = Utility.GetFormationArea(FormationClass.Ranged, troopCount[1],
                soldiersPerRow);
            GameEntity.Instantiate(this.Mission.Scene, "sp_" + sideString + "_cavalry",
                Utility.ToMatrixFrame(Mission.Scene,
                    position - direction * (region1.Item2 + region2.Item2),
                    direction));
        }
    }
}
