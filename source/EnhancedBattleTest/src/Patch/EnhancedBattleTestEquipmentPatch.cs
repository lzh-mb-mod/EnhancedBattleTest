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

            if (EnhancedBattleTestPartyController.TryGetEquipmentSet(
                    origin.Party,
                    character,
                    agentBuildData.AgentEquipmentSeed,
                    out Equipment selectedEquipment))
            {
                agentBuildData.Equipment(
                    ApplyModifierType(selectedEquipment, modifierType));
            }
            else if (modifierType.HasValue
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

        private static Equipment ApplyModifierType(
            Equipment equipment,
            EquipmentModifierType? modifierType)
        {
            if (!modifierType.HasValue)
                return equipment;

            var result = new Equipment(equipment);
            for (int i = 0; i < 12; i++)
            {
                EquipmentElement element = result[i];
                ItemModifier modifier = modifierType.Value
                    == EquipmentModifierType.Random
                    ? element.Item?.ItemComponent?.ItemModifierGroup
                        ?.GetRandomItemModifierLootScoreBased()
                    : null;
                result[i] = new EquipmentElement(
                    element.Item,
                    modifier,
                    element.CosmeticItem,
                    element.IsQuestItem);
            }
            return result;
        }
    }
}
