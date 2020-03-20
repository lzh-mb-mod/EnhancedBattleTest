using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class TeamAIEnableLogic : MissionLogic
    {
        private BattleConfigBase _config;

        public TeamAIEnableLogic(BattleConfigBase config)
        {
            _config = config;
        }
        public override void OnBehaviourInitialize()
        {
            base.OnBehaviourInitialize();

            Mission.Teams.OnPlayerTeamChanged += (oldTeam, newTeam) =>
            {
                Utility.ApplyTeamAIEnabled(_config);
            };
        }
    }
}
