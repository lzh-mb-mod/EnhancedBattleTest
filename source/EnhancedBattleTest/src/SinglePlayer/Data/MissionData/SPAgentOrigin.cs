using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data;
using EnhancedBattleTest.Data.MissionData;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.SinglePlayer.Data.MissionData
{
    public class SPAgentOrigin : EnhancedBattleTestAgentOrigin
    {
        public override BasicCharacterObject Troop => SPCharacter.Character;
        public override ISpawnableCharacter SpawnableCharacter => SPCharacter;

        public override FormationClass FormationIndex => SPCharacter.FormationIndex;

        public SPSpawnableCharacter SPCharacter { get; }

        public PartyAgentOrigin PartyAgentOrigin;
        public IBattleCombatant CultureCombatant { get; }

        public override Banner Banner => CultureCombatant.Banner;

        public override uint FactionColor => CultureCombatant.PrimaryColorPair.Item1;

        public override uint FactionColor2 => CultureCombatant.PrimaryColorPair.Item2;

        public SPAgentOrigin(SPCombatant combatant, SPSpawnableCharacter character, IEnhancedBattleTestTroopSupplier troopSupplier, BattleSideEnum side, int rank = -1, UniqueTroopDescriptor uniqueNo = default)
            : base(combatant.Combatant, troopSupplier, side, rank, uniqueNo)
        {
            SPCharacter = character;
            CultureCombatant = combatant;
            PartyAgentOrigin = new PartyAgentOrigin(combatant.Combatant, character.CharacterObject, rank,
                uniqueNo);
        }

        public override Agent SpawnTroop(BattleSideEnum side, bool hasFormation, bool spawnWithHorse, bool isReinforcement,
            int formationTroopCount, int formationTroopIndex, bool isAlarmed,
            bool wieldInitialWeapons, bool forceDismounted = false,
            Vec3? initialPosition = null, Vec2? initialDirection = null, string specialActionSet = null,
            ItemObject bannerItem = null,
            FormationClass formationIndex = FormationClass.NumberOfAllFormations)
        {
            BasicCharacterObject troop = Troop;
            var agentTeam = IsUnderPlayersCommand ? Mission.Current.PlayerTeam : Mission.Current.PlayerEnemyTeam;
            //MatrixFrame frame = initFrame ?? Mission.Current
            //    .GetFormationSpawnFrame(team.Side, FormationClass.NumberOfRegularFormations, false).ToGroundMatrixFrame();
            if (SPCharacter.IsPlayer && !forceDismounted)
                spawnWithHorse = true;
            AgentBuildData agentBuildData = new AgentBuildData(this)
                .Team(agentTeam).Banner(Banner)
                .ClothingColor1(agentTeam.Color).ClothingColor2(agentTeam.Color2)
                .NoHorses(!spawnWithHorse).CivilianEquipment(Mission.Current.DoesMissionRequireCivilianEquipment);
            agentBuildData.IsFemale(SPCharacter.IsFemale);
            if (hasFormation)
            {
                Formation formation = formationIndex != FormationClass.NumberOfAllFormations ? agentTeam.GetFormation(formationIndex) : agentTeam.GetFormation(Mission.Current.GetAgentTroopClass(agentTeam.Side, troop));
                agentBuildData.Formation(formation);
                agentBuildData.FormationTroopSpawnCount(formationTroopCount).FormationTroopSpawnIndex(formationTroopIndex);
            }
            if (!SPCharacter.IsPlayer)
                agentBuildData.IsReinforcement(isReinforcement);
            if (bannerItem != null)
            {
                if (bannerItem.IsBannerItem && bannerItem.BannerComponent != null)
                {
                    agentBuildData.BannerItem(bannerItem);
                    ItemObject replacementWeapon = MissionGameModels.Current.BattleBannerBearersModel.GetBannerBearerReplacementWeapon(troop);
                    agentBuildData.BannerReplacementWeaponItem(replacementWeapon);
                }
                else
                {
                    TaleWorlds.Library.Debug.FailedAssert($"Passed banner item with name: {(object)bannerItem.Name} is not a proper banner item", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\Mission.cs", nameof(SpawnTroop), 4253);
                    TaleWorlds.Library.Debug.Print($"Invalid banner item: {(object)bannerItem.Name} is passed to a troop to be spawned", color: TaleWorlds.Library.Debug.DebugColor.Yellow);
                }
            }
            if (initialPosition.HasValue)
            {
                agentBuildData.InitialPosition(initialPosition.Value);
                agentBuildData.InitialDirection(initialDirection.Value);
            }
            if (spawnWithHorse)
                agentBuildData.MountKey(MountCreationKey.GetRandomMountKeyString(
                    troop.Equipment[EquipmentIndex.Horse].Item, troop.GetMountKeySeed()));
            //if (hasFormation && !SPCharacter.IsPlayer)
            //{
            //    Formation formation = team.GetFormation(SPCharacter.FormationIndex);
            //    agentBuildData.Formation(formation);
            //    agentBuildData.FormationTroopCount(formationTroopCount).FormationTroopIndex(formationTroopIndex);
            //}
            if (SPCharacter.IsPlayer)
                agentBuildData.Controller(Agent.ControllerType.Player);
            Agent agent = Mission.Current.SpawnAgent(agentBuildData);
            if (troop.IsHero)
            {
                agent.SetAgentFlags(agent.GetAgentFlags() | AgentFlag.IsUnique);
                //agentBuildData.Equipment(troop.GetFirstEquipment(agentBuildData.AgentCivilianEquipment).Clone(false));
            }
            //else
            //{
            //    var equipmentModifierType = BattleConfig.Instance.BattleTypeConfig.EquipmentModifierType;
            //    var equipment = Equipment.GetRandomEquipmentElements(troop, equipmentModifierType == EquipmentModifierType.Random,
            //        agentBuildData.AgentCivilianEquipment,
            //        agentBuildData.AgentEquipmentSeed);
            //    if (equipmentModifierType == EquipmentModifierType.Average)
            //    {
            //        for (EquipmentIndex index = EquipmentIndex.Weapon0;
            //            index < EquipmentIndex.NumEquipmentSetSlots;
            //            ++index)
            //        {
            //            var equipmentElement = equipment.GetEquipmentFromSlot(index);
            //            if (equipmentElement.Item != null)
            //            {
            //                if (equipmentElement.Item.HasArmorComponent)
            //                    equipmentElement.SetModifier(
            //                        Utility.AverageItemModifier(equipmentElement.Item.ArmorComponent
            //                            .ItemModifierGroup));
            //                else if (equipmentElement.Item.HasHorseComponent)
            //                    equipmentElement.SetModifier(
            //                        Utility.AverageItemModifier(equipmentElement.Item.HorseComponent
            //                            .ItemModifierGroup));
            //            }
            //        }
            //    }

            //    agentBuildData.Equipment(equipment);
            //}
            //Agent agent = Mission.Current.SpawnAgent(agentBuildData, false, formationTroopCount);
            if (agent.IsAIControlled & isAlarmed)
                agent.SetWatchState(Agent.WatchState.Alarmed);
            if (wieldInitialWeapons)
                agent.WieldInitialWeapons();
            if (!string.IsNullOrEmpty(specialActionSet))
            {
                AnimationSystemData animationSystemData =
                    agentBuildData.AgentMonster.FillAnimationSystemData(MBGlobals.GetActionSet(specialActionSet),
                        agent.Character.GetStepSize(), false);
                agent.SetActionSet(ref animationSystemData);
            }
            return agent;
        }
    }
}
