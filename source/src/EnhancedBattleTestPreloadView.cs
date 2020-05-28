using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;

namespace EnhancedBattleTest
{
    class EnhancedBattleTestPreloadView : MissionView
    {
        public override void OnPreMissionTick(float dt)
        {
            MissionCombatantsLogic missionBehaviour = this.Mission.GetMissionBehaviour<MissionCombatantsLogic>();
            List<BasicCharacterObject> characters = new List<BasicCharacterObject>();
            foreach (IBattleCombatant allCombatant in missionBehaviour.GetAllCombatants())
                characters.AddRange(((IEnhancedBattleTestCombatant)allCombatant).Characters);
            MissionPreloadHelper.PreloadCharacters(characters);
            this.Mission.RemoveMissionBehaviour(this);
        }
    }
}
