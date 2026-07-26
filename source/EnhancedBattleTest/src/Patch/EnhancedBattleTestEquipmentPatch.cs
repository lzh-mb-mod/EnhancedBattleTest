using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data;
using HarmonyLib;
using System;
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
            if (!(agentBuildData?.AgentOrigin is PartyGroupAgentOrigin origin)
                || !EnhancedBattleTestPartyController.IsTestParty(origin.Party))
                return;

            if (EnhancedBattleTestPartyController.TryGetTemporaryPartyAppearance(
                    origin.Party,
                    out _,
                    out Tuple<uint, uint> colors))
            {
                agentBuildData
                    .ClothingColor1(colors.Item1)
                    .ClothingColor2(colors.Item2);
            }

            EquipmentModifierType? modifierType =
                EnhancedBattleTestPartyController.Current?.EquipmentModifierType;
            BasicCharacterObject character = agentBuildData?.AgentCharacter;
            if (character == null)
                return;

            if (modifierType.HasValue && !character.IsHero)
            {
                agentBuildData.Equipment(
                    Equipment.GetRandomEquipmentElements(
                        character,
                        modifierType.Value == EquipmentModifierType.Random,
                        agentBuildData.AgentCivilianEquipment,
                        agentBuildData.AgentEquipmentSeed));
            }

            if (EnhancedBattleTestPartyController.TryGetFemaleRatio(
                    origin.Party,
                    character,
                    out float femaleRatio))
            {
                bool isFemale = MBRandom.RandomFloat < femaleRatio;
                agentBuildData.IsFemale(isFemale);
            }
        }
    }
}
