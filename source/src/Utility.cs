using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    class Utility
    {
        public static void DisplayMessage(string msg)
        {
            InformationManager.DisplayMessage(new InformationMessage(new TaleWorlds.Localization.TextObject(msg).ToString()));
        }

        public static bool IsAgentDead(Agent agent)
        {
            return agent == null || !agent.IsActive();
        }

        public static bool IsPlayerDead()
        {
            return IsAgentDead(Mission.Current.MainAgent);
        }

        public static void SetPlayerAsCommander()
        {
            Mission.Current.PlayerTeam.PlayerOrderController.Owner = Mission.Current.MainAgent;
            Mission.Current.PlayerTeam.SetPlayerRole(true, false);
            foreach (var formation in Mission.Current.PlayerTeam.Formations)
            {
                formation.PlayerOwner = Mission.Current.MainAgent;
            }
            //foreach (MissionBehaviour missionBehaviour in Mission.Current.MissionBehaviours)
            //    missionBehaviour.OnAssignPlayerAsSergeantOfFormation(Mission.Current.MainAgent);
        }

        public static Equipment ApplyPerksFor(ClassInfo info, bool isPlayer)
        {
            MultiplayerClassDivisions.MPHeroClass mpHeroClass =
                MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>(info.classStringId);
            BasicCharacterObject character = isPlayer ? mpHeroClass.HeroCharacter : mpHeroClass.TroopCharacter;
            List<MPPerkObject> selectedPerkList = new List<MPPerkObject>
            {
                mpHeroClass.GetAllAvailablePerksForListIndex(0)[info.selectedFirstPerk],
                mpHeroClass.GetAllAvailablePerksForListIndex(1)[info.selectedSecondPerk]
            };
            Equipment equipment = isPlayer ? character.Equipment.Clone() : Equipment.GetRandomEquipmentElements(character, false, false, MBRandom.RandomInt());
            foreach (PerkEffect perkEffectsForPerk in MPPerkObject.SelectRandomPerkEffectsForPerks(isPlayer, PerkType.PerkAlternativeEquipment, selectedPerkList))
                equipment[perkEffectsForPerk.NewItemIndex] = perkEffectsForPerk.NewItem.EquipmentElement;
            return equipment;
        }
    }
}
