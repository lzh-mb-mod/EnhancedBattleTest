using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data;
using HarmonyLib;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.Patch
{
    [HarmonyPatch(typeof(Mission), nameof(Mission.SpawnAgent))]
    public static class EnhancedBattleTestEquipmentPatch
    {
        private static void Prefix(AgentBuildData agentBuildData)
        {
            EquipmentModifierType? modifierType =
                EnhancedBattleTestPartyController.Current?.EquipmentModifierType;
            if (!modifierType.HasValue)
                return;

            BasicCharacterObject character = agentBuildData?.AgentCharacter;
            if (character == null || character.IsHero)
                return;
            if (!(agentBuildData.AgentOrigin is PartyGroupAgentOrigin origin)
                || !EnhancedBattleTestPartyController.IsTestParty(origin.Party))
                return;

            agentBuildData.Equipment(
                Equipment.GetRandomEquipmentElements(
                    character,
                    modifierType.Value == EquipmentModifierType.Random,
                    agentBuildData.AgentCivilianEquipment,
                    agentBuildData.AgentEquipmentSeed));
        }
    }
}
