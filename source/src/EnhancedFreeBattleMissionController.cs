using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TL = TaleWorlds.Library;

namespace EnhancedBattleTest
{
    public class EnhancedFreeBattleMissionController : MissionLogic
    {
        private EnhancedFreeBattleConfig _freeBattleConfig;
        public EnhancedFreeBattleConfig FreeBattleConfig
        {
            get => _freeBattleConfig;
            set => _freeBattleConfig = value;
        }

        //private bool _ended = false;
        public TL.Vec3 initialFreeCameraPos;
        public TL.Vec3 initialFreeCameraTarget;
        private AgentVictoryLogic _victoryLogic;
        private MakeGruntVoiceLogic _makeGruntVoiceLogic;
        public bool spawned = false;
        public int NumberOfActiveAttackerTroops { get; set; }
        public int NumberOfActiveDefenderTroops { get; set; }

        public EnhancedFreeBattleMissionController(EnhancedFreeBattleConfig config)
        {
            this.FreeBattleConfig = config;
            NumberOfActiveAttackerTroops = config.GetTotalNumberForSide(BattleSideEnum.Attacker);
            NumberOfActiveDefenderTroops = config.GetTotalNumberForSide(BattleSideEnum.Defender);
            _victoryLogic = null;
        }

        public override void EarlyStart()
        {
            this.Mission.MissionTeamAIType = Mission.MissionTeamAITypeEnum.FieldBattle;
            this.Mission.SetMissionMode(MissionMode.Battle, true);
            _makeGruntVoiceLogic = this.Mission.GetMissionBehaviour<MakeGruntVoiceLogic>();
        }

        public override void AfterStart()
        {
            base.AfterStart();

            foreach (var team in Mission.Teams)
            {
                team.OnOrderIssued += OrderIssuedDelegate;
            }
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);


            Team team = affectedAgent.Team;
            BattleSideEnum battleSideEnum = team != null ? team.Side : BattleSideEnum.None;
            switch (battleSideEnum)
            {
                case BattleSideEnum.Attacker:
                    --NumberOfActiveAttackerTroops;
                    break;
                case BattleSideEnum.Defender:
                    --NumberOfActiveDefenderTroops;
                    break;
            }
        }

        public void SpawnAgents()
        {

            var playerTeamCulture = this.FreeBattleConfig.GetPlayerTeamCulture();
            var enemyTeamCulture = this.FreeBattleConfig.GetEnemyTeamCulture();
            SpawnPlayerTeamAgents(playerTeamCulture);
            SpawnEnemyTeamAgents(enemyTeamCulture);
        }

        private void SpawnPlayerTeamAgents(BasicCultureObject playerTeamCulture)
        {
            var scene = this.Mission.Scene;
            // see TaleWorlds.MountAndBlade.dll/FlagDominationSpawningBehaviour.cs: TaleWorlds.MountAndBlade.FlagDominationSpawningBehaviour.SpawnEnemyTeamAgents()
            // see TaleWorlds.MountAndBlade.dll/SpawningBehaviourBase.cs: TaleWorlds.MountAndBlade.SpawningBehaviourBase.OnTick()
            var playerTeamCombatant = CreateBattleCombatant(playerTeamCulture, BattleSideEnum.Attacker);

            var xInterval = this.FreeBattleConfig.soldierXInterval;
            var yInterval = this.FreeBattleConfig.soldierYInterval;
            var soldiersPerRow = this.FreeBattleConfig.SoldiersPerRow;
            var startPos = this.FreeBattleConfig.FormationPosition;
            var xDir = this.FreeBattleConfig.FormationDirection;
            var yDir = this.FreeBattleConfig.FormationDirection.LeftVec();
            var spawnPlayer = this.FreeBattleConfig.SpawnPlayer;

            var playerTeam = this.Mission.PlayerTeam;

            if (spawnPlayer)
            {
                MultiplayerClassDivisions.MPHeroClass playerHeroClass = this.FreeBattleConfig.PlayerHeroClass;
                BasicCharacterObject playerCharacter = playerHeroClass.HeroCharacter;
                Game.Current.PlayerTroop = playerCharacter;
                var playerFormation = playerTeam.GetFormation((FormationClass)_freeBattleConfig.playerFormation);
                SetFormationRegion(playerFormation, 1, true, -2);
                Agent player = this.SpawnAgent(FreeBattleConfig.playerClass, true,
                    playerCharacter, true, playerFormation, playerTeam,
                    playerTeamCombatant, playerTeamCulture, true, 1, 0);

                player.AllowFirstPersonWideRotation();
            }
            else
            {
                var c = this.FreeBattleConfig.playerTroops[0].troopCount;
                if (c <= 0)
                {
                    initialFreeCameraTarget = startPos.ToVec3(Utility.GetSceneHeightForAgent(Mission.Scene, startPos));
                    initialFreeCameraPos = initialFreeCameraTarget + new TL.Vec3(0, 0, 10) - xDir.ToVec3();
                }
                else
                {
                    var rowCount = (c + soldiersPerRow - 1) / soldiersPerRow;
                    var p = startPos + (System.Math.Min(soldiersPerRow, c) - 1) / 2 * yInterval * yDir - rowCount * xInterval * xDir;
                    initialFreeCameraTarget = p.ToVec3(Utility.GetSceneHeightForAgent(Mission.Scene, p));
                    initialFreeCameraPos = initialFreeCameraTarget + new TL.Vec3(0, 0, 10) - xDir.ToVec3();
                }

                initialFreeCameraPos -= this._freeBattleConfig.FormationDirection.ToVec3();
            }

            float distanceToInitialPosition = 0;

            for (var formationIndex = 0; formationIndex < 3; ++formationIndex)
            {
                int playerTroopCount = FreeBattleConfig.playerTroops[formationIndex].troopCount;
                if (playerTroopCount <= 0)
                    continue;
                BasicCharacterObject playerTroopCharacter = this.FreeBattleConfig.GetPlayerTroopHeroClass(formationIndex).TroopCharacter;
                var playerTroopFormation = playerTeam.GetFormation((FormationClass)formationIndex);
                var (width, length) =
                    GetInitialFormationArea(formationIndex, true, playerTroopCharacter.CurrentFormationClass);
                SetFormationRegion(playerTroopFormation, width, true, distanceToInitialPosition);
                distanceToInitialPosition += length;
                BasicCultureObject troopCulture = spawnPlayer ? playerTeamCulture : playerTroopCharacter.Culture;
                for (var troopIndex = 0; troopIndex < playerTroopCount; ++troopIndex)
                {
                    var agent = this.SpawnAgent(FreeBattleConfig.playerTroops[formationIndex], false, playerTroopCharacter, false,
                        playerTroopFormation, playerTeam, playerTeamCombatant, troopCulture, true, playerTroopCount,
                        troopIndex);
                }
            }

            // just test item spawning.
            //var matrixFrame = Utility.ToMatrixFrame(Mission.Scene, GetFormationPosition(true, -2), TL.Vec2.Forward);

            //MissionWeapon weapon = new MissionWeapon(MBObjectManager.Instance.GetObject<ItemObject>("mp_hatchet_axe"), (Banner)null);
            //Mission.Current.SpawnWeaponWithNewEntity(ref weapon, Mission.WeaponSpawnFlags.WithPhysics, matrixFrame);

            //var hat = MBObjectManager.Instance.GetObject<ItemObject>("mp_northern_padded_cloth");
            //var gameEntity = GameEntity.CreateEmpty(Mission.Scene);
            //gameEntity.AddMesh(Mesh.GetFromResource(hat.MultiMeshName));
            //gameEntity.SetGlobalFrame(matrixFrame);
            //Mission.Scene.AttachEntity(gameEntity);

            //var horse = MBObjectManager.Instance.GetObject<ItemObject>("mp_sturgia_horse");

            //Mission.Scene.AddEntityWithMesh(Mesh.GetFromResource(horse.MultiMeshName), ref matrixFrame);

            //var house = GameEntity.Instantiate(Mission.Scene, "hawk_stand_b", matrixFrame);
            //Mission.Scene.AttachEntity(house);
        }

        private void SpawnEnemyTeamAgents(BasicCultureObject enemyTeamCulture)
        {
            var scene = this.Mission.Scene;

            var enemyTeamCombatant = CreateBattleCombatant(enemyTeamCulture, BattleSideEnum.Defender);

            var enemyTeam = this.Mission.PlayerEnemyTeam;

            var xDir = this.FreeBattleConfig.FormationDirection;

            if (FreeBattleConfig.SpawnEnemyCommander)
            {
                MultiplayerClassDivisions.MPHeroClass enemyHeroClass = this.FreeBattleConfig.EnemyHeroClass;
                BasicCharacterObject enemyCharacter = enemyHeroClass.HeroCharacter;
                var formation = enemyTeam.GetFormation(Utility.CommanderFormationClass());
                SetFormationRegion(formation, 1, false, -2);
                Agent agent = this.SpawnAgent(FreeBattleConfig.enemyClass, false,
                    enemyCharacter, true, formation, enemyTeam,
                    enemyTeamCombatant, enemyTeamCulture, false, 1, 0);
            }

            float distanceToInitialPosition = 0;
            for (var formationIndex = 0; formationIndex < 3; ++formationIndex)
            {
                int enemyTroopCount = this.FreeBattleConfig.enemyTroops[formationIndex].troopCount;
                if (enemyTroopCount <= 0)
                    continue;
                BasicCharacterObject enemyTroopCharacter = this.FreeBattleConfig.GetEnemyTroopHeroClass(formationIndex).TroopCharacter;
                var enemyTroopFormation = enemyTeam.GetFormation((FormationClass)formationIndex);
                var (width, length) =
                    GetInitialFormationArea(formationIndex, false, enemyTroopCharacter.CurrentFormationClass);
                SetFormationRegion(enemyTroopFormation, width, false, distanceToInitialPosition);
                distanceToInitialPosition += length;
                var troopCulture = this.FreeBattleConfig.SpawnEnemyCommander
                    ? enemyTeamCulture
                    : enemyTroopCharacter.Culture;
                for (var troopIndex = 0; troopIndex < enemyTroopCount; ++troopIndex)
                {
                    var agent = SpawnAgent(FreeBattleConfig.enemyTroops[formationIndex], false, enemyTroopCharacter, false, enemyTroopFormation,
                        enemyTeam, enemyTeamCombatant, troopCulture, false, enemyTroopCount, troopIndex);
                }
            }
        }

        public override void OnMissionTick(float dt)
        {
            if (!spawned)
            {
                spawned = true;
                SpawnAgents();
            }
        }


        void OrderIssuedDelegate(
            OrderType orderType,
            IEnumerable<Formation> appliedFormations,
            params object[] delegateParams)
        {
            foreach (var formation in appliedFormations)
            {
                if (_victoryLogic != null)
                {
                    foreach (var agent in formation.Units)
                    {
                        _victoryLogic.OnAgentDeleted(agent);
                        if (!agent.IsPlayerControlled)
                        {
                            agent.SetActionChannel(1, ActionIndexCache.act_none, true);
                        }
                    }
                }

                _makeGruntVoiceLogic?.AddFormation(formation, 1f);
            }
        }

        private CustomBattleCombatant CreateBattleCombatant(BasicCultureObject culture, BattleSideEnum side)
        {
            return new CustomBattleCombatant(culture.Name, culture,
                new Banner(culture.BannerKey, culture.BackgroundColor1, culture.ForegroundColor1))
            {
                Side = side
            };
        }

        private Agent SpawnAgent(ClassInfo classInfo, bool isPlayer, BasicCharacterObject character, bool isHero, Formation formation, Team team, CustomBattleCombatant combatant, BasicCultureObject culture, bool isPlayerSide, int formationTroopCount, int formationTroopIndex, TL.MatrixFrame? matrix = null)
        {
            bool isAttacker = isPlayerSide ? FreeBattleConfig.isPlayerAttacker : !FreeBattleConfig.isPlayerAttacker;
            AgentBuildData agentBuildData = new AgentBuildData(Utility.CreateOrigin(combatant, character))
                .Team(team)
                .Formation(formation)
                .FormationTroopCount(formationTroopCount).FormationTroopIndex(formationTroopIndex)
                .Banner(team.Banner)
                .ClothingColor1(isAttacker ? culture.Color : culture.ClothAlternativeColor)
                .ClothingColor2(isAttacker ? culture.Color2 : culture.ClothAlternativeColor2);
            Utility.OverrideEquipment(agentBuildData, classInfo, isHero);
            if (matrix.HasValue)
                agentBuildData.InitialFrame(matrix.Value);
            var agent = this.Mission.SpawnAgent(agentBuildData, false, formationTroopCount);
            agent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
            agent.WieldInitialWeapons();
            agent.Controller = isPlayer ? Agent.ControllerType.Player : Agent.ControllerType.AI;
            return agent;
        }


        private void SetFormationRegion(Formation formation, float width, bool isPlayerSide, float distanceToInitialPosition)
        {
            bool isAttackerSide = isPlayerSide ? FreeBattleConfig.isPlayerAttacker : !FreeBattleConfig.isPlayerAttacker;
            var position = GetFormationPosition(isAttackerSide, distanceToInitialPosition);
            var direction = GetFormationDirection(isAttackerSide);
            formation.SetPositioning(position.ToWorldPosition(Mission.Scene), direction);
            formation.FormOrder = FormOrder.FormOrderCustom(width);
        }

        private Tuple<float, float> GetInitialFormationArea(int formationIndex, bool isPlayerSide, FormationClass fc)
        {
            var config = this.FreeBattleConfig;
            int troopCount = isPlayerSide
                ? config.playerTroops[formationIndex].troopCount
                : config.enemyTroops[formationIndex].troopCount;
            return Utility.GetFormationArea(fc, troopCount, config.SoldiersPerRow);
        }

        private TL.Vec2 GetFormationDirection(bool isAttackerSide)
        {
            return isAttackerSide ? FreeBattleConfig.FormationDirection : -FreeBattleConfig.FormationDirection;
        }

        private TL.Vec3 GetFormationPosition(bool isAttackerSide, float distanceToInitialPosition)
        {
            var xDir = this.FreeBattleConfig.FormationDirection;
            var pos = this.FreeBattleConfig.FormationPosition
                      + distanceToInitialPosition * (isAttackerSide ? -xDir : xDir);
            if (!isAttackerSide)
                pos += xDir * this.FreeBattleConfig.Distance;
            return pos.ToVec3(Utility.GetSceneHeightForAgent(Mission.Scene, pos));
        }

    }
}