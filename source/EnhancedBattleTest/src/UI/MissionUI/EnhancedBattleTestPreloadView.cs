using System.Collections.Generic;
using EnhancedBattleTest.Data.MissionData;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;

namespace EnhancedBattleTest.UI.MissionUI
{
    class EnhancedBattleTestPreloadView : MissionView
    {
        private readonly MissionPreloadHelper _helperInstance = new MissionPreloadHelper();
        private bool _preloadDone;
        public override void OnPreMissionTick(float dt)
        {
            if (_preloadDone)
                return;
            MissionCombatantsLogic missionBehaviour = Mission.GetMissionBehaviour<MissionCombatantsLogic>();
            List<BasicCharacterObject> characters = new List<BasicCharacterObject>();
            foreach (IBattleCombatant allCombatant in missionBehaviour.GetAllCombatants())
                characters.AddRange(((IEnhancedBattleTestCombatant)allCombatant).Characters);

            _helperInstance.PreloadCharacters(characters);
            _preloadDone = true;
        }

        public override void OnMissionDeactivate()
        {
            base.OnMissionDeactivate();
            _helperInstance.Clear();
        }
    }
}
