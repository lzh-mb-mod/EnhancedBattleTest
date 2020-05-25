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

            AddEntity("attack", _config.FormationPosition, _config.FormationDirection);
            AddEntity("defender", _config.FormationPosition + _config.Distance * _config.FormationDirection,
                -_config.FormationDirection);

        }

        private void AddEntity(string sideString, Vec2 position, Vec2 direction)
        {
            var name = "sergeant_" + sideString + "_spawn";
            var entity = this.Mission.Scene.GetFirstEntityWithName(name);
            var matrixFrame = Utility.ToMatrixFrame(Mission.Scene, position, direction);
            if (entity != null)
                entity.SetGlobalFrame(matrixFrame);
            else
            {
                entity = GameEntity.Instantiate(this.Mission.Scene, name, matrixFrame);
            }
        }
    }
}
