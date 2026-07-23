using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace EnhancedBattleTest.UI.MissionUI
{
    internal sealed class EnhancedBattleTestPreloadView : MissionView
    {
        private readonly PreloadHelper _helperInstance = new PreloadHelper();
        private bool _preloadDone;

        public override void OnPreMissionTick(float dt)
        {
            if (_preloadDone)
                return;

            var characters = new List<BasicCharacterObject>();
            MissionCombatantsLogic combatantsLogic =
                Mission.GetMissionBehavior<MissionCombatantsLogic>();
            foreach (IBattleCombatant combatant in combatantsLogic.GetAllCombatants())
            {
                if (!(combatant is PartyBase party))
                    continue;

                foreach (TroopRosterElement element in party.MemberRoster.GetTroopRoster())
                {
                    for (int index = 0; index < element.Number; index++)
                        characters.Add(element.Character);
                }
            }

            _helperInstance.PreloadCharacters(characters);
            _preloadDone = true;
        }

        public override void OnSceneRenderingStarted()
        {
            _helperInstance.WaitForMeshesToBeLoaded();
        }

        public override void OnMissionStateDeactivated()
        {
            base.OnMissionStateDeactivated();
            _helperInstance.Clear();
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            _helperInstance.Clear();
        }
    }
}
