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

            if (modifierType.HasValue
                && modifierType.Value != EquipmentModifierType.Random
                && !character.IsHero)
            {
                agentBuildData.Equipment(
                    Equipment.GetRandomEquipmentElements(
                        character,
                        randomEquipmentModifier: false,
                        agentBuildData.AgentCivilianEquipment,
                        agentBuildData.AgentEquipmentSeed));
            }

            if (EnhancedBattleTestPartyController.TryGetFemaleRatio(
                    origin.Party,
                    character,
                    out float femaleRatio))
            {
                bool isFemale = MBRandom.RandomFloat < femaleRatio;
                if (isFemale != character.IsFemale)
                    agentBuildData.IsFemale(isFemale);
            }
        }
    }
}
