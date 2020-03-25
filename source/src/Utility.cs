using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
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
            var mission = Mission.Current;
            if (mission == null)
                return;
            mission.PlayerTeam.PlayerOrderController.Owner = mission.MainAgent;
            foreach (var formation in mission.PlayerTeam.FormationsIncludingEmpty)
            {
                bool isAIControlled = formation.IsAIControlled;
                formation.PlayerOwner = mission.MainAgent;
                formation.IsAIControlled = isAIControlled;
            }
            //DisplayMessage("Set player as commander");
            foreach (var formation in mission.PlayerTeam.Formations)
            {
                DisplayMessage("Formation " + formation.FormationIndex.ToString() + ": IsAIControlled = " +
                               formation.IsAIControlled.ToString());
            }
        }

        public static void CancelPlayerCommander()
        {
            var mission = Mission.Current;
            //DisplayMessage("Cancel player commander:");
            //foreach (var formation in mission.PlayerTeam.Formations)
            //{
            //    DisplayMessage("Formation " + formation.FormationIndex.ToString() + ": IsAIControlled = " +
            //                   formation.IsAIControlled.ToString());
            //}
        }

        public static void ApplyTeamAIEnabled(BattleConfigBase config)
        {
            var mission = Mission.Current;
            switch (config.aiEnableType)
            {
                case AIEnableType.None:
                    Utility.DisplayMessage("Team AI is disabled.");
                    Utility.SetTeamAIEnabled(mission.PlayerTeam, false);
                    Utility.SetTeamAIEnabled(mission.PlayerEnemyTeam, false);
                    break;
                case AIEnableType.EnemyOnly:
                    Utility.DisplayMessage("Team AI is enabled for enemy only");
                    Utility.SetTeamAIEnabled(mission.PlayerTeam, false);
                    Utility.SetTeamAIEnabled(mission.PlayerEnemyTeam, true);
                    break;
                case AIEnableType.PlayerOnly:
                    Utility.DisplayMessage("Team AI is enabled for player team only");
                    Utility.SetTeamAIEnabled(mission.PlayerTeam, true);
                    Utility.SetTeamAIEnabled(mission.PlayerEnemyTeam, false);
                    break;
                case AIEnableType.Both:
                    Utility.DisplayMessage("Team AI is enabled for both sides");
                    Utility.SetTeamAIEnabled(mission.PlayerTeam, true);
                    Utility.SetTeamAIEnabled(mission.PlayerEnemyTeam, true);
                    break;
            }
        }

        public static void SetTeamAIEnabled(Team team, bool enabled)
        {
            if (team == null)
                return;
            foreach (var formation in team.FormationsIncludingEmpty)
            {
                formation.IsAIControlled = enabled;
            }
        }

        public static List<MPPerkObject> GetAllSelectedPerks(MultiplayerClassDivisions.MPHeroClass mpHeroClass,
            int[] selectedPerks)
        {
            List<MPPerkObject> selectedPerkList = new List<MPPerkObject>();
            for (int i = 0; i < selectedPerks.Length; ++i)
            {
                var perks = mpHeroClass.GetAllAvailablePerksForListIndex(i);
                if (perks.IsEmpty())
                    continue;
                selectedPerkList.Add(perks[selectedPerks[i]]);
            }

            return selectedPerkList;
        }

        public static IEnumerable<PerkEffect> SelectRandomPerkEffectsForPerks(MultiplayerClassDivisions.MPHeroClass mpHeroClass, bool isPlayer, PerkType perkType, int[] selectedPerks)
        {
            var selectedPerkList = GetAllSelectedPerks(mpHeroClass, selectedPerks);
            return MPPerkObject.SelectRandomPerkEffectsForPerks(isPlayer, perkType, selectedPerkList);
        }

        public static Equipment GetNewEquipmentsForPerks(MultiplayerClassDivisions.MPHeroClass mpHeroClass, ClassInfo info, bool isPlayer)
        {
            BasicCharacterObject character = isPlayer ? mpHeroClass.HeroCharacter : mpHeroClass.TroopCharacter;
            Equipment equipment = isPlayer ? character.Equipment.Clone() : Equipment.GetRandomEquipmentElements(character, true, false, MBRandom.RandomInt());
            foreach (PerkEffect perkEffectsForPerk in SelectRandomPerkEffectsForPerks(mpHeroClass, isPlayer,
                PerkType.PerkAlternativeEquipment, new[] { info.selectedFirstPerk, info.selectedSecondPerk }))
                equipment[perkEffectsForPerk.NewItemIndex] = perkEffectsForPerk.NewItem.EquipmentElement;
            return equipment;
        }

        public static void OverrideEquipment(AgentBuildData buildData, ClassInfo info, bool isPlayer)
        {
            MultiplayerClassDivisions.MPHeroClass mpHeroClass =
                MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>(info.classStringId);
            BasicCharacterObject character = isPlayer ? mpHeroClass.HeroCharacter : mpHeroClass.TroopCharacter;
            var equipment = GetNewEquipmentsForPerks(mpHeroClass, info, isPlayer);
            buildData
                .Equipment(equipment)
                .MountKey(MountCreationKey.GetRandomMountKey(equipment[EquipmentIndex.ArmorItemEndSlot].Item,
                    character.GetMountKeySeed()));
        }

        public static BasicCharacterObject ApplyPerks(ClassInfo info, bool isHero)
        {
            MultiplayerClassDivisions.MPHeroClass mpHeroClass =
                MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>(info.classStringId);
            BasicCharacterObject sourceCharacter = isHero ? mpHeroClass.HeroCharacter : mpHeroClass.TroopCharacter;
            if (mpHeroClass.GetAllAvailablePerksForListIndex(0).IsEmpty() ||
                mpHeroClass.GetAllAvailablePerksForListIndex(1).IsEmpty())
                return sourceCharacter;
            var equipment = GetNewEquipmentsForPerks(mpHeroClass, info, isHero);
            if (equipment == null)
                return sourceCharacter;
            var character = NewCharacter(sourceCharacter);
            character.InitializeEquipmentsOnLoad(new List<Equipment> { equipment });
            character.SetIsHero(isHero);
            return character;
        }

        public static MultiplayerClassDivisions.MPHeroClass GetMPHeroClassForCharacter(BasicCharacterObject character)
        {
            string id = character.StringId;
            string suffix = "_WithPerkApplied";
            if (id.EndsWith(suffix))
                id = id.Substring(0, id.Length - suffix.Length);
            BasicCharacterObject originalCharacter = MBObjectManager.Instance.GetObject<BasicCharacterObject>(id);
            return MultiplayerClassDivisions.GetMPHeroClassForCharacter(originalCharacter);
        }

        public static EnhancedBattleTestCharacter NewCharacter(BasicCharacterObject sourceCharacter)
        {
            var character = new EnhancedBattleTestCharacter();

            character.UpdatePlayerCharacterBodyProperties(sourceCharacter.GetBodyPropertiesMax(), sourceCharacter.IsFemale);
            character.InitializeHeroBasicCharacterOnAfterLoad(sourceCharacter, sourceCharacter.Name);
            character.StringId = sourceCharacter.StringId + "_WithPerkApplied";
            character.Name = sourceCharacter.Name;
            character.Age = sourceCharacter.Age;
            character.FaceDirtAmount = sourceCharacter.FaceDirtAmount;
            character.Level = sourceCharacter.Level;
            return character;
        }
        public static FormationClass CommanderFormationClass()
        {
            return FormationClass.HorseArcher;
        }

        public static void AddCharacter(CustomBattleCombatant combatant, ClassInfo info, bool isHero,
            FormationClass formationClass, bool isPlayer = false)
        {

            BasicCharacterObject character = Utility.ApplyPerks(info, isHero);
            character.CurrentFormationClass = formationClass;
            if (isPlayer)
                Game.Current.PlayerTroop = character;
            combatant.AddCharacter(character, info.troopCount);
        }

        public static MatrixFrame ToMatrixFrame(Scene scene, Vec3 position, Vec2 direction)
        {
            var defaultDir = new Vec2(0, 1);
            var mat = Mat3.Identity;
            mat.RotateAboutUp(defaultDir.AngleBetween(direction));
            return new MatrixFrame(mat, position);
        }

        public static MatrixFrame ToMatrixFrame(Scene scene, Vec2 position, Vec2 direction)
        {
            var defaultDir = new Vec2(0, 1);
            var mat = Mat3.Identity;
            mat.RotateAboutUp(defaultDir.AngleBetween(direction));
            return new MatrixFrame(mat, position.ToVec3(GetSceneHeightForAgent(scene, position)));
        }
        public static float GetSceneHeightForAgent(Scene scene, Vec2 pos)
        {
            float result = 0;
            scene.GetHeightAtPoint(pos, BodyFlags.CommonCollisionExcludeFlagsForAgent, ref result);
            return result;
        }

        public static Tuple<float, float> GetFormationArea(FormationClass formationClass, int troopCount, int soldiersPerRow)
        {
            var mounted = formationClass == FormationClass.Cavalry || formationClass == FormationClass.HorseArcher;
            var unitDiameter = Formation.GetDefaultUnitDiameter(mounted);
            var unitSpacing = 1;
            var interval = mounted ? Formation.CavalryInterval(unitSpacing) : Formation.InfantryInterval(unitSpacing);
            var actualSoldiersPerRow = System.Math.Min(soldiersPerRow, troopCount);
            var width = (actualSoldiersPerRow) * (unitDiameter + interval) - interval;
            if (mounted)
                unitDiameter *= 1.8f;
            float length = ((int)Math.Ceiling((float)troopCount / actualSoldiersPerRow)) * (unitDiameter + interval) + 1.5f;
            return new Tuple<float, float>(width, length);
        }

        public static void SetInitialCameraPos(Camera camera, Vec2 formationPosition, Vec2 formationDirection)
        {
            Vec3 position = formationPosition.ToVec3(GetSceneHeightForAgent(Mission.Current.Scene, formationPosition) + 5);
            Vec3 direction = formationPosition.ToVec3(-1).NormalizedCopy();
            camera.LookAt(position, position + direction, Vec3.Up);
        }

        public static IAgentOriginBase CreateOrigin(
            CustomBattleCombatant customBattleCombatant,
            BasicCharacterObject characterObject,
            int rank = -1,
            EnhancedTroopSupplier troopSupplier = null)
        {
            UniqueTroopDescriptor uniqueNo = new UniqueTroopDescriptor(Game.Current.NextUniqueTroopSeed);
            return new EnhancedTestBattleAgentOrigin(customBattleCombatant, troopSupplier, characterObject, rank,
                uniqueNo);
        }
    }
}
