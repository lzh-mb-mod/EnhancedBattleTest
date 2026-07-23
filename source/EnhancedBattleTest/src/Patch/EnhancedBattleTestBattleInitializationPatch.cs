using EnhancedBattleTest.Data;
using HarmonyLib;
using SandBox.GameComponents;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;

namespace EnhancedBattleTest.Patch
{
    public static class EnhancedBattleTestBattleInitializationPatch
    {
        [HarmonyPatch(
            typeof(SandboxBattleInitializationModel),
            "CanPlayerSideDeployWithOrderOfBattleAux")]
        private static class CanPlayerSideDeployPatch
        {
            private static bool Prefix(ref bool __result)
            {
                EnhancedBattleTestPartyController.BattleContext context =
                    EnhancedBattleTestPartyController.Current;
                if (context == null)
                    return true;

                __result = context.PlayerParty.Party.NumberOfHealthyMembers >= 20;
                return false;
            }
        }

        [HarmonyPatch(
            typeof(SandboxBattleInitializationModel),
            nameof(SandboxBattleInitializationModel.GetAllAvailableTroopTypes))]
        private static class GetAllAvailableTroopTypesPatch
        {
            private static bool Prefix(ref List<FormationClass> __result)
            {
                EnhancedBattleTestPartyController.BattleContext context =
                    EnhancedBattleTestPartyController.Current;
                if (context == null)
                    return true;

                __result = GetAvailableTroopTypes(context.PlayerParty.Party);
                return false;
            }
        }

        private static List<FormationClass> GetAvailableTroopTypes(PartyBase party)
        {
            var troopTypes = new List<FormationClass>();
            foreach (TroopRosterElement element in party.MemberRoster.GetTroopRoster())
            {
                if (element.Number <= element.WoundedNumber)
                    continue;

                CharacterObject character = element.Character;
                AddTroopType(
                    troopTypes,
                    character.IsInfantry && !character.IsMounted,
                    FormationClass.Infantry);
                AddTroopType(
                    troopTypes,
                    character.IsRanged && !character.IsMounted,
                    FormationClass.Ranged);
                AddTroopType(
                    troopTypes,
                    character.IsMounted && !character.IsRanged,
                    FormationClass.Cavalry);
                AddTroopType(
                    troopTypes,
                    character.IsMounted && character.IsRanged,
                    FormationClass.HorseArcher);

                if (troopTypes.Count == 4)
                    break;
            }

            return troopTypes;
        }

        private static void AddTroopType(
            ICollection<FormationClass> troopTypes,
            bool condition,
            FormationClass troopType)
        {
            if (condition && !troopTypes.Contains(troopType))
                troopTypes.Add(troopType);
        }
    }
}
