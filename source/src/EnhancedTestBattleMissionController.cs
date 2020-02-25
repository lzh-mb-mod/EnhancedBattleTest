using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Screens;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screen;
using TL = TaleWorlds.Library;

namespace EnhancedBattleTest
{
    public class EnhancedTestBattleMissionController : MissionLogic
    {
        private EnhancedTestBattleConfig _testBattleConfig;

        public EnhancedTestBattleConfig TestBattleConfig
        {
            get => _testBattleConfig;
            set
            {
                _testBattleConfig = value;
                int playerNumber = this.TestBattleConfig.UseFreeCamera ? 0 : 1;
                _shouldCelebrateVictory = (playerNumber + this.TestBattleConfig.playerTroops.Sum(classInfo => classInfo.troopCount)) != 0 &&
                                          this.TestBattleConfig.enemyTroops.Sum(classInfo => classInfo.troopCount) != 0;
            }
        }

        private bool _shouldCelebrateVictory;
        private bool _ended = false;
        public TL.Vec3 initialFreeCameraPos;
        public TL.Vec3 initialFreeCameraTarget;
        private AgentVictoryLogic _victoryLogic;
        private MakeGruntVoiceLogic _makeGruntVoiceLogic;

        public EnhancedTestBattleMissionController(EnhancedTestBattleConfig config)
        {
            this.TestBattleConfig = config;
            _victoryLogic = null;
        }

        public override void AfterStart()
        {
            this.Mission.MissionTeamAIType = Mission.MissionTeamAITypeEnum.FieldBattle;
            this.Mission.SetMissionMode(MissionMode.Battle, true);
            _makeGruntVoiceLogic = this.Mission.GetMissionBehaviour<MakeGruntVoiceLogic>();
            AdjustScene();
            var playerTeamCulture = this.TestBattleConfig.GetPlayerTeamCulture();
            var enemyTeamCulture = this.TestBattleConfig.GetEnemyTeamCulture();
            AddTeams(playerTeamCulture, enemyTeamCulture);
            SpawnPlayerTeamAgents(playerTeamCulture);
            SpawnEnemyTeamAgents(enemyTeamCulture);
        }

        public void AdjustScene()
        {
            var scene = this.Mission.Scene;

            if (this.TestBattleConfig.SkyBrightness >= 0)
            {
                scene.SetSkyBrightness(this.TestBattleConfig.SkyBrightness);
            }

            if (this.TestBattleConfig.RainDensity >= 0)
            {
                scene.SetRainDensity(this.TestBattleConfig.RainDensity);
            }
        }

        public void AddTeams(BasicCultureObject playerTeamCulture, BasicCultureObject enemyTeamCulture)
        {
            // see TaleWorlds.MountAndBlade.dll/MissionMultiplayerFlagDomination.cs : TaleWorlds.MountAndBlade.MissionMultiplayerFlagDomination.AfterStart();
            Banner playerTeamBanner = new Banner(playerTeamCulture.BannerKey,
                playerTeamCulture.BackgroundColor1,
                playerTeamCulture.ForegroundColor1);
            var playerTeam = this.Mission.Teams.Add(BattleSideEnum.Attacker, color: playerTeamCulture.BackgroundColor1, color2: playerTeamCulture.ForegroundColor1, banner: playerTeamBanner);

            Banner enemyTeamBanner = new Banner(enemyTeamCulture.BannerKey,
                enemyTeamCulture.BackgroundColor2, enemyTeamCulture.ForegroundColor2);
            var enemyTeam = this.Mission.Teams.Add(BattleSideEnum.Defender, color: enemyTeamCulture.BackgroundColor2, color2: enemyTeamCulture.ForegroundColor2, banner: enemyTeamBanner);

            playerTeam.AddTeamAI(new TeamAIGeneral(this.Mission, playerTeam));
            playerTeam.AddTacticOption(new TacticCharge(playerTeam));
            playerTeam.ExpireAIQuerySystem();
            playerTeam.ResetTactic();
            playerTeam.OnOrderIssued += OrderIssuedDelegate;

            enemyTeam.AddTeamAI(new TeamAIGeneral(this.Mission, enemyTeam));
            enemyTeam.AddTacticOption(new TacticCharge(enemyTeam));
            enemyTeam.ExpireAIQuerySystem();
            enemyTeam.ResetTactic();
            enemyTeam.OnOrderIssued += OrderIssuedDelegate;

            enemyTeam.SetIsEnemyOf(playerTeam, true);
            playerTeam.SetIsEnemyOf(enemyTeam, true);
            this.Mission.PlayerTeam = playerTeam;
        }

        public void SpawnPlayerTeamAgents(BasicCultureObject playerTeamCulture)
        {
            var scene = this.Mission.Scene;
            // see TaleWorlds.MountAndBlade.dll/FlagDominationSpawningBehaviour.cs: TaleWorlds.MountAndBlade.FlagDominationSpawningBehaviour.SpawnEnemyTeamAgents()
            // see TaleWorlds.MountAndBlade.dll/SpawningBehaviourBase.cs: TaleWorlds.MountAndBlade.SpawningBehaviourBase.OnTick()
            var playerTeamCombatant = CreateBattleCombatant(playerTeamCulture, BattleSideEnum.Attacker);

            var xInterval = this.TestBattleConfig.soldierXInterval;
            var yInterval = this.TestBattleConfig.soldierYInterval;
            var soldiersPerRow = this.TestBattleConfig.SoldiersPerRow;
            var startPos = this.TestBattleConfig.FormationPosition;
            var xDir = this.TestBattleConfig.FormationDirection;
            var yDir = this.TestBattleConfig.FormationDirection.LeftVec();
            var useFreeCamera = this.TestBattleConfig.UseFreeCamera;

            var playerTeam = this.Mission.PlayerTeam;

            if (!useFreeCamera)
            {
                var agentMatrixFrame = GetFormationMatrixFrame(0, true, 0);
                agentMatrixFrame.origin += (xDir * 2).ToVec3();
                MultiplayerClassDivisions.MPHeroClass playerHeroClass = this.TestBattleConfig.PlayerHeroClass;
                BasicCharacterObject playerCharacter = playerHeroClass.HeroCharacter;
                var playerFormation = playerTeam.GetFormation(Utility.CommanderFormationClass());
                playerFormation.SetPositioning(agentMatrixFrame.origin.ToWorldPosition(scene), this.TestBattleConfig.FormationDirection);
                playerFormation.FormOrder = FormOrder.FormOrderCustom(1);
                Agent player = this.SpawnAgent(TestBattleConfig.playerClass,
                    playerCharacter, true, playerFormation, playerTeam,
                    playerTeamCombatant, playerTeamCulture, true, 1, 0, agentMatrixFrame);
                player.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
                player.Controller = Agent.ControllerType.Player;
                player.WieldInitialWeapons();
                player.AllowFirstPersonWideRotation();
                playerTeam.MasterOrderController.ClearSelectedFormations();
                playerTeam.MasterOrderController.SelectFormation(playerFormation);
                playerTeam.MasterOrderController.SetOrderWithPosition(OrderType.Move, agentMatrixFrame.origin.ToWorldPosition());
            }
            else
            {
                var c = this.TestBattleConfig.playerTroops[0].troopCount;
                if (c <= 0)
                {
                    float height = GetSceneHeightForAgent(startPos);
                    initialFreeCameraPos = startPos.ToVec3(height + 5);
                    initialFreeCameraTarget = startPos.ToVec3(height);
                }
                else
                {
                    var rowCount = (c + soldiersPerRow - 1) / soldiersPerRow;
                    var p = startPos + (System.Math.Min(soldiersPerRow, c) - 1) / 2 * yInterval * yDir - rowCount * xInterval * xDir;
                    float height = GetSceneHeightForAgent(p);
                    initialFreeCameraPos = p.ToVec3(height + 5);
                    initialFreeCameraTarget = p.ToVec3(height);
                }

                initialFreeCameraPos -= this._testBattleConfig.FormationDirection.ToVec3();
            }

            float distanceToPreviousFormaiton = 0;

            for (var formationIndex = 0; formationIndex < 3; ++formationIndex)
            {
                BasicCharacterObject playerTroopCharacter = this.TestBattleConfig.GetPlayerTroopHeroClass(formationIndex).TroopCharacter;
                TL.MatrixFrame formationMatrixFrame = GetFormationMatrixFrame(formationIndex, true, distanceToPreviousFormaiton);
                var playerTroopFormation = playerTeam.GetFormation((FormationClass)formationIndex);
                var tuple = SetFormationArea(playerTroopFormation, formationIndex, true, playerTroopCharacter.CurrentFormationClass,
                    formationMatrixFrame);
                distanceToPreviousFormaiton = tuple.Item2;
                int playerTroopCount = TestBattleConfig.playerTroops[formationIndex].troopCount;
                BasicCultureObject troopCulture = !useFreeCamera ? playerTeamCulture : playerTroopCharacter.Culture;
                for (var troopIndex = 0; troopIndex < playerTroopCount; ++troopIndex)
                {
                    var agent = this.SpawnAgent(TestBattleConfig.playerTroops[formationIndex], playerTroopCharacter, false,
                        playerTroopFormation, playerTeam, playerTeamCombatant, troopCulture, true, playerTroopCount,
                        troopIndex);
                    agent.Controller = Agent.ControllerType.AI;
                    agent.WieldInitialWeapons();
                    agent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
                }
                playerTeam.MasterOrderController.ClearSelectedFormations();
                playerTeam.MasterOrderController.SelectFormation(playerTroopFormation);
                playerTeam.MasterOrderController.SetOrderWithPosition(OrderType.Move, formationMatrixFrame.origin.ToWorldPosition());
            }
        }

        public void SpawnEnemyTeamAgents(BasicCultureObject enemyTeamCulture)
        {
            var scene = this.Mission.Scene;

            var enemyTeamCombatant = CreateBattleCombatant(enemyTeamCulture, BattleSideEnum.Defender);

            var enemyTeam = this.Mission.PlayerEnemyTeam;

            var xDir = this.TestBattleConfig.FormationDirection;

            if (TestBattleConfig.SpawnEnemyCommander)
            {
                var agentMatrixFrame = GetFormationMatrixFrame(0, false, 0);
                agentMatrixFrame.origin += ((-xDir) * 2).ToVec3();
                MultiplayerClassDivisions.MPHeroClass enemyHeroClass = this.TestBattleConfig.EnemyHeroClass;
                BasicCharacterObject enemyCharacter = enemyHeroClass.HeroCharacter;
                var formation = enemyTeam.GetFormation(Utility.CommanderFormationClass());
                formation.SetPositioning(agentMatrixFrame.origin.ToWorldPosition(scene), -this.TestBattleConfig.FormationDirection);
                formation.FormOrder = FormOrder.FormOrderCustom(1);
                Agent agent = this.SpawnAgent(TestBattleConfig.enemyClass,
                    enemyCharacter, true, formation, enemyTeam,
                    enemyTeamCombatant, enemyTeamCulture, false, 1, 0, agentMatrixFrame);
                agent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
                agent.Controller = Agent.ControllerType.AI;
                agent.WieldInitialWeapons();

                enemyTeam.MasterOrderController.ClearSelectedFormations();
                enemyTeam.MasterOrderController.SelectFormation(formation);
                enemyTeam.MasterOrderController.SetOrderWithPosition(OrderType.Move, agentMatrixFrame.origin.ToWorldPosition());
            }

            float distanceToPreviousFormation = 0;
            for (var formationIndex = 0; formationIndex < 3; ++formationIndex)
            {
                BasicCharacterObject enemyTroopCharacter = this.TestBattleConfig.GetEnemyTroopHeroClass(formationIndex).TroopCharacter;
                TL.MatrixFrame formationMatrixFrame = GetFormationMatrixFrame(formationIndex, false, distanceToPreviousFormation);
                var enemyTroopFormation = enemyTeam.GetFormation((FormationClass)formationIndex);
                var tuple = SetFormationArea(enemyTroopFormation, formationIndex, false, enemyTroopCharacter.CurrentFormationClass,
                    formationMatrixFrame);
                distanceToPreviousFormation = tuple.Item2;
                int enemySoldierCount = this.TestBattleConfig.enemyTroops[formationIndex].troopCount;
                var troopCulture = this.TestBattleConfig.SpawnEnemyCommander
                    ? enemyTeamCulture
                    : enemyTroopCharacter.Culture;
                for (var troopIndex = 0; troopIndex < enemySoldierCount; ++troopIndex)
                {
                    var agent = SpawnAgent(TestBattleConfig.enemyTroops[formationIndex], enemyTroopCharacter, false, enemyTroopFormation,
                        enemyTeam, enemyTeamCombatant, troopCulture, false, enemySoldierCount, troopIndex);
                    agent.Controller = Agent.ControllerType.AI;
                    agent.WieldInitialWeapons();
                    agent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
                }

                enemyTeam.MasterOrderController.ClearSelectedFormations();
                enemyTeam.MasterOrderController.SelectFormation(enemyTroopFormation);
                enemyTeam.MasterOrderController.SetOrderWithPosition(OrderType.Move, formationMatrixFrame.origin.ToWorldPosition());
            }
        }

        public override void OnMissionTick(float dt)
        {
            CheckVictory();
            if (this.Mission.InputManager.IsKeyPressed(TaleWorlds.InputSystem.InputKey.I))
            {
                Agent mainAgent = this.Mission.MainAgent;
                TL.Vec3 position = mainAgent != null ? mainAgent.Position : this.Mission.Scene.LastFinalRenderCameraPosition;
                string str = new WorldPosition(this.Mission.Scene, position).GetNavMesh().ToString() ?? "";
                Utility.DisplayMessage(
                    $"Position: {(object)position} | Navmesh: {(object)str} | Time: {(object)this.Mission.Time}");
                ModuleLogger.Log("INFO Position: {0}, Navigation Mesh: {1}", (object)position, (object)str);
            }
        }

        private void CheckVictory()
        {
            if (!_ended && _shouldCelebrateVictory)
            {
                bool playerVictory = this.Mission.PlayerEnemyTeam.ActiveAgents.IsEmpty();
                bool enemyVictory = this.Mission.PlayerTeam.ActiveAgents.IsEmpty();
                if (!playerVictory && !enemyVictory)
                    return;
                _ended = true;
                _victoryLogic = this.Mission.GetMissionBehaviour<AgentVictoryLogic>();
                if (_victoryLogic == null)
                    return;
                if (enemyVictory)
                {
                    _ended = true;
                    _victoryLogic.SetTimersOfVictoryReactions(this.Mission.PlayerEnemyTeam.Side);
                }
                else
                {
                    _ended = true;
                    _victoryLogic.SetTimersOfVictoryReactions(this.Mission.PlayerTeam.Side);
                }
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
                            EquipmentIndex index = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                            agent.TryToSheathWeaponInHand(Agent.HandIndex.MainHand, Agent.WeaponWieldActionType.Instant);
                            agent.TryToWieldWeaponInSlot(index, Agent.WeaponWieldActionType.WithAnimation, false);
                        }
                    }
                }

                _makeGruntVoiceLogic?.AddFormation(formation, 1f);
            }
        }
        private float GetSceneHeightForAgent(TL.Vec2 pos)
        {
            float result = 0;
            //if (this.Mission.Scene.IsAtmosphereIndoor)
            //    result = this.Mission.Scene.GetTerrainHeight(pos);
            //else
            //    this.Mission.Scene.GetHeightAtPoint(pos, BodyFlags.CommonCollisionExcludeFlagsForAgent, ref result);

            this.Mission.Scene.GetHeightAtPoint(pos, BodyFlags.CommonCollisionExcludeFlagsForAgent, ref result);
            return result;
        }

        private CustomBattleCombatant CreateBattleCombatant(BasicCultureObject culture, BattleSideEnum side)
        {
            return new CustomBattleCombatant(culture.Name, culture,
                new Banner(culture.BannerKey, culture.BackgroundColor1, culture.ForegroundColor1))
            {
                Side = side
            };
        }

        private IAgentOriginBase CreateOrigin(
            CustomBattleCombatant customBattleCombatant,
            BasicCharacterObject characterObject,
            bool isPlayerSide,
            int rank = -1)
        {
            UniqueTroopDescriptor uniqueNo = new UniqueTroopDescriptor(Game.Current.NextUniqueTroopSeed);
            return new EnhancedTestBattleAgentOrigin(customBattleCombatant, characterObject, isPlayerSide, rank,
                uniqueNo);
        }

        private Agent SpawnAgent(ClassInfo classInfo, BasicCharacterObject character, bool isHero, Formation formation, Team team, CustomBattleCombatant combatant, BasicCultureObject culture, bool isPlayerSide, int formationTroopCount, int formationTroopIndex, TL.MatrixFrame? matrix = null)
        {
            Equipment equipment = Utility.GetNewEquipmentsForPerks(classInfo, isHero);
            AgentBuildData agentBuildData = new AgentBuildData(CreateOrigin(combatant, character, isPlayerSide, -1))
                .Team(team)
                .Formation(formation)
                .FormationTroopCount(formationTroopCount).FormationTroopIndex(formationTroopIndex)
                .Banner(team.Banner)
                .ClothingColor1(isPlayerSide ? culture.Color : culture.ClothAlternativeColor)
                .ClothingColor2(isPlayerSide ? culture.Color2 : culture.ClothAlternativeColor2)
                .IsFemale(false)
                .Equipment(equipment)
                .MountKey(MountCreationKey.GetRandomMountKey(equipment[EquipmentIndex.ArmorItemEndSlot].Item, character.GetMountKeySeed()));
            if (matrix.HasValue)
                agentBuildData
                .InitialFrame(matrix.Value);
            return this.Mission.SpawnAgent(agentBuildData, false, 0);
        }


        private Tuple<float, float> SetFormationArea(Formation formation, int formationIndex, bool isPlayerSide, FormationClass formationClass, TL.MatrixFrame matrixFrame)
        {
            var area = this.GetInitialFormationArea(formationIndex, isPlayerSide, formationClass);
            var direction = isPlayerSide
                ? this.TestBattleConfig.FormationDirection
                : -this.TestBattleConfig.FormationDirection;
            formation.SetPositioning(matrixFrame.origin.ToWorldPosition(this.Mission.Scene), direction);
            formation.FormOrder = FormOrder.FormOrderCustom(area.Item1);
            return area;
        }

        private Tuple<float, float> GetInitialFormationArea(int formationIndex, bool isPlayerSide, FormationClass fc)
        {
            var config = this.TestBattleConfig;
            int troopCount = isPlayerSide
                ? config.playerTroops[formationIndex].troopCount
                : config.enemyTroops[formationIndex].troopCount;
            var mounted = fc == FormationClass.Cavalry || fc == FormationClass.HorseArcher;
            var unitDiameter = Formation.GetDefaultUnitDiameter(mounted);
            var unitSpacing = 1;
            var interval = mounted ? Formation.CavalryInterval(unitSpacing) : Formation.InfantryInterval(unitSpacing);
            var actualSoldiersPerRow = System.Math.Min(config.SoldiersPerRow, troopCount);
            var width = (actualSoldiersPerRow - 1) * (unitDiameter + interval) + unitDiameter + 0.1f;
            float length = ((int)Math.Ceiling((float)troopCount / config.SoldiersPerRow) - 1) * (unitDiameter + interval);
            return new Tuple<float, float>(width, length);
        }

        private TL.MatrixFrame GetFormationMatrixFrame(int formationIndex, bool isPlayerSide, float distanceToPreviousFormation)
        {
            var agentDefaultDir = new TL.Vec2(0, 1);
            var xDir = this.TestBattleConfig.FormationDirection;
            var mat = TL.Mat3.Identity;
            mat.RotateAboutUp(agentDefaultDir.AngleBetween(isPlayerSide ? xDir : -xDir));
            var pos = GetFormationPosition(formationIndex, isPlayerSide, distanceToPreviousFormation);
            return new TL.MatrixFrame(mat, pos);
        }

        private TL.Vec3 GetFormationPosition(int formationIndex, bool isPlayerSide, float distanceToPreviousFormation)
        {
            var xDir = this.TestBattleConfig.FormationDirection;
            var pos = this.TestBattleConfig.FormationPosition
                      + formationIndex * distanceToPreviousFormation * (isPlayerSide ? -xDir : xDir);
            if (!isPlayerSide)
                pos += xDir * this.TestBattleConfig.distance;
            return pos.ToVec3(GetSceneHeightForAgent(pos));
        }
    }
}