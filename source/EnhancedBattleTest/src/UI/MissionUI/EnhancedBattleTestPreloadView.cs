using EnhancedBattleTest.Data.MissionData;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace EnhancedBattleTest.UI.MissionUI
{
    class EnhancedBattleTestPreloadView : MissionView
    {
        private readonly PreloadHelper _helperInstance = new PreloadHelper();
        private bool _preloadDone;
        public override void OnPreMissionTick(float dt)
        {
            if (_preloadDone)
                return;
            MissionCombatantsLogic missionBehaviour = Mission.GetMissionBehavior<MissionCombatantsLogic>();
            List<BasicCharacterObject> characters = new List<BasicCharacterObject>();
            foreach (IBattleCombatant allCombatant in missionBehaviour.GetAllCombatants())
                characters.AddRange(((IEnhancedBattleTestCombatant)allCombatant).Characters);

            _helperInstance.PreloadCharacters(characters);
            _preloadDone = true;
        }

        public override void OnMissionStateDeactivated()
        {
            base.OnMissionStateDeactivated();
            _helperInstance.Clear();
        }

    }
}
