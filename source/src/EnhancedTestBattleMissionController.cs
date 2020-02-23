using System;
using System.Collections.Generic;
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
        private Game _game;
        private EnhancedTestBattleConfig _testBattleConfig;

        public EnhancedTestBattleConfig TestBattleConfig
        {
            get => _testBattleConfig;
            set
            {
                _testBattleConfig = value;
                int playerNumber = this.TestBattleConfig.useFreeCamera ? 0 : 1;
                _shouldCelebrateVictory = (playerNumber + this.TestBattleConfig.playerSoldierCount) != 0 &&
                                          this.TestBattleConfig.enemySoldierCount != 0;
            }
        }
        
        private bool _shouldCelebrateVictory;
        private bool _ended = false;
        private bool _mapHasNavMesh = false;
        public TL.Vec3 initialFreeCameraPos;
        public TL.Vec3 initialFreeCameraTarget;
        private AgentVictoryLogic _victoryLogic;
        private MakeGruntVoiceLogic _makeGruntVoiceLogic;

        public EnhancedTestBattleMissionController(EnhancedTestBattleConfig config)
        {
            this._game = Game.Current;
            this.TestBattleConfig = config;
            _victoryLogic = null;
        }

        public override void AfterStart()
        {
            this.Mission.MissionTeamAIType = Mission.MissionTeamAITypeEnum.FieldBattle;
            this.Mission.SetMissionMode(MissionMode.Battle, true);
            _makeGruntVoiceLogic = this.Mission.GetMissionBehaviour<MakeGruntVoiceLogic>();
            AdjustScene();
            AddTeams();
            SpawnPlayerTeamAgents();
            SpawnEnemyTeamAgents();
        }
        
        public void AddTeams()
        {
            // see TaleWorlds.MountAndBlade.dll/MissionMultiplayerFlagDomination.cs : TaleWorlds.MountAndBlade.MissionMultiplayerFlagDomination.AfterStart();
            var playerTeamCulture = this.TestBattleConfig.playerSoldierCount == 0 ? this.TestBattleConfig.PlayerHeroClass.Culture : this.TestBattleConfig.PlayerTroopHeroClass.Culture;
            Banner playerTeamBanner = new Banner(playerTeamCulture.BannerKey,
                playerTeamCulture.BackgroundColor1,
                playerTeamCulture.ForegroundColor1);
            var playerTeam = this.Mission.Teams.Add(BattleSideEnum.Attacker, color: playerTeamCulture.BackgroundColor1, color2: playerTeamCulture.ForegroundColor1, banner: playerTeamBanner);

            var enemyTeamCulture = this.TestBattleConfig.EnemyTroopHeroClass.Culture;
            Banner enemyTeamBanner = new Banner(enemyTeamCulture.BannerKey,
                enemyTeamCulture.BackgroundColor2, enemyTeamCulture.ForegroundColor2);
            var enemyTeam = this.Mission.Teams.Add(BattleSideEnum.Defender, color: enemyTeamCulture.BackgroundColor2, color2: enemyTeamCulture.ForegroundColor2, banner: enemyTeamBanner);

            if (this.TestBattleConfig.playerSoldierCount > 0)
            {
                playerTeam.AddTeamAI(new TeamAIGeneral(this.Mission, playerTeam));
                playerTeam.AddTacticOption(new TacticCharge(playerTeam));
                playerTeam.ExpireAIQuerySystem();
                playerTeam.ResetTactic();
                playerTeam.OnOrderIssued += OrderIssuedDelegate;
            }

            if (this.TestBattleConfig.enemySoldierCount > 0)
            {
                enemyTeam.AddTeamAI(new TeamAIGeneral(this.Mission, enemyTeam));
                enemyTeam.AddTacticOption(new TacticCharge(enemyTeam));
                enemyTeam.ExpireAIQuerySystem();
                enemyTeam.ResetTactic();
                enemyTeam.OnOrderIssued += OrderIssuedDelegate;
            }
            enemyTeam.SetIsEnemyOf(playerTeam, true);
            playerTeam.SetIsEnemyOf(enemyTeam, true);
            this.Mission.PlayerTeam = playerTeam;
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

        public void SpawnPlayerTeamAgents()
        {
            var scene = this.Mission.Scene;
            var playerTeamCulture = this.TestBattleConfig.playerSoldierCount == 0 ? this.TestBattleConfig.PlayerHeroClass.Culture : this.TestBattleConfig.PlayerTroopHeroClass.Culture;
            // see TaleWorlds.MountAndBlade.dll/FlagDominationSpawningBehaviour.cs: TaleWorlds.MountAndBlade.FlagDominationSpawningBehaviour.SpawnEnemyTeamAgents()
            // see TaleWorlds.MountAndBlade.dll/SpawningBehaviourBase.cs: TaleWorlds.MountAndBlade.SpawningBehaviourBase.OnTick()
            var playerTeamCombatant = CreateBattleCombatant(playerTeamCulture, BattleSideEnum.Attacker);

            var xInterval = this.TestBattleConfig.soldierXInterval;
            var yInterval = this.TestBattleConfig.soldierYInterval;
            var soldiersPerRow = this.TestBattleConfig.SoldiersPerRow;
            var startPos = this.TestBattleConfig.FormationPosition;
            var xDir = this.TestBattleConfig.FormationDirection;
            var yDir = this.TestBattleConfig.FormationDirection.LeftVec();
            var agentDefaultDir = new TL.Vec2(0, 1);
            var useFreeCamera = this.TestBattleConfig.useFreeCamera;

            var playerTeam = this.Mission.PlayerTeam;
            BasicCharacterObject playerTroopCharacter = this.TestBattleConfig.PlayerTroopHeroClass.TroopCharacter;
            var playerTroopFormationClass = playerTroopCharacter.CurrentFormationClass;
            var playerTroopFormation = playerTeam.GetFormation(playerTroopFormationClass);

            var playerPosVec2 = startPos + xDir * -5 + yDir * -5;
            var playerPos = new TL.Vec3(playerPosVec2.x, playerPosVec2.y, GetSceneHeightForAgent(playerPosVec2));
            if (!useFreeCamera)
            {
                var playerMat = GetTroopMatrixFrame(0, playerPosVec2, true);
                BasicCharacterObject playerCharacter = this.TestBattleConfig.PlayerHeroClass.HeroCharacter;
                var playerFormation = playerTeam.GetFormation(playerCharacter.CurrentFormationClass);
                playerFormation.SetPositioning(playerPos.ToWorldPosition(scene), xDir, null);
                Agent player = this.SpawnAgent(TestBattleConfig.playerClass,
                    TestBattleConfig.PlayerHeroClass.HeroCharacter, true, playerFormation, playerTeam,
                    playerTeamCombatant, playerTeamCulture, true, -1, playerMat);
                player.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
                player.Controller = Agent.ControllerType.Player;
                player.WieldInitialWeapons();
                player.AllowFirstPersonWideRotation(); 
                
                Utility.SetPlayerAsCommander();
            }
            else
            {
                var c = this.TestBattleConfig.playerSoldierCount;
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
            {
                var width = this.GetInitialFormationWidth(playerTeam, playerTroopFormationClass);
                var centerPos = startPos + yDir * (width / 2);
                var wp = centerPos.ToVec3().ToWorldPosition(scene);
                playerTroopFormation.SetPositioning(wp, xDir, null);
                playerTroopFormation.FormOrder = FormOrder.FormOrderCustom(width);
                _mapHasNavMesh = wp.GetNavMesh() != System.UIntPtr.Zero;
            }

            for (var i = 0; i < this.TestBattleConfig.playerSoldierCount; ++i)
            {
                TL.MatrixFrame? agentFrame = null;
                if (!_mapHasNavMesh)
                {
                    agentFrame = GetTroopMatrixFrame(this.TestBattleConfig.playerSoldierCount - i - 1, startPos, true);
                }

                var agent = this.SpawnAgent(TestBattleConfig.playerTroopClass, playerTroopCharacter, false,
                    playerTroopFormation, playerTeam, playerTeamCombatant, playerTeamCulture, true, i, agentFrame);
                agent.Controller = Agent.ControllerType.AI;
                agent.WieldInitialWeapons();
                agent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
            }
        }

        public void SpawnEnemyTeamAgents()
        {
            var scene = this.Mission.Scene;

            var enemyTeamCulture = this.TestBattleConfig.EnemyTroopHeroClass.Culture;
            var enemyTeamCombatant = CreateBattleCombatant(enemyTeamCulture, BattleSideEnum.Defender);

            var xInterval = this.TestBattleConfig.soldierXInterval;
            var yInterval = this.TestBattleConfig.soldierYInterval;
            var soldiersPerRow = this.TestBattleConfig.SoldiersPerRow;
            var startPos = this.TestBattleConfig.FormationPosition;
            var xDir = this.TestBattleConfig.FormationDirection;
            var yDir = this.TestBattleConfig.FormationDirection.LeftVec();
            var agentDefaultDir = new TL.Vec2(0, 1);

            BasicCharacterObject enemyTroopCharacter = this.TestBattleConfig.EnemyTroopHeroClass.TroopCharacter;
            
            var enemyFormationClass = enemyTroopCharacter.CurrentFormationClass;
            var enemyTeam = this.Mission.PlayerEnemyTeam;
            var enemyFormation = enemyTeam.GetFormation(enemyFormationClass);
            {
                float width = this.GetInitialFormationWidth(enemyTeam, enemyFormationClass);
                var centerPos = startPos + yDir * (width / 2) + xDir * this.TestBattleConfig.distance;
                var wp = new WorldPosition(scene, centerPos.ToVec3());
                enemyFormation.SetPositioning(wp, -xDir, null);
                enemyFormation.FormOrder = FormOrder.FormOrderCustom(width);
            }
            
            for (var i = 0; i < this.TestBattleConfig.enemySoldierCount; ++i)
            {
                TL.MatrixFrame? agentFrame = null;
                if (!_mapHasNavMesh)
                {
                    agentFrame = GetTroopMatrixFrame(i, startPos, false);
                }

                var agent = SpawnAgent(TestBattleConfig.enemyTroopClass, enemyTroopCharacter, false, enemyFormation,
                    enemyTeam, enemyTeamCombatant, enemyTeamCulture, false, i, agentFrame);
                agent.Controller = Agent.ControllerType.AI;
                agent.WieldInitialWeapons();
                agent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
            }
            
        }

        float GetInitialFormationWidth(Team team, FormationClass fc)
        {
            var bp = this.TestBattleConfig;
            var formation = team.GetFormation(fc);
            var mounted = fc == FormationClass.Cavalry || fc == FormationClass.HorseArcher;
            var unitDiameter = Formation.GetDefaultUnitDiameter(mounted);
            var unitSpacing = 1;
            var interval = mounted ? Formation.CavalryInterval(unitSpacing) : Formation.InfantryInterval(unitSpacing);
            var actualSoldiersPerRow = System.Math.Min(bp.SoldiersPerRow, team == this.Mission.PlayerTeam ? bp.playerSoldierCount : bp.enemySoldierCount);
            var width = (actualSoldiersPerRow - 1) * (unitDiameter + interval) + unitDiameter + 0.1f;
            return width;
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
                    $"Position: {(object) position} | Navmesh: {(object) str} | Time: {(object) this.Mission.Time}");
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

        private Agent SpawnAgent(ClassInfo classInfo, BasicCharacterObject character, bool isPlayer, Formation formation, Team team, CustomBattleCombatant combatant, BasicCultureObject culture, bool isPlayerSide, int rank, TL.MatrixFrame? matrix)
        {
            Equipment equipment = Utility.GetNewEquipmentsForPerks(classInfo, isPlayer);
            AgentBuildData agentBuildData = new AgentBuildData(CreateOrigin(combatant, character, isPlayerSide, -1))
                .Team(team)
                .Formation(formation)
                .Banner(team.Banner)
                .ClothingColor1(culture.Color)
                .ClothingColor2(culture.Color2)
                .IsFemale(false)
                .Equipment(equipment)
                .MountKey(MountCreationKey.GetRandomMountKey(equipment[EquipmentIndex.ArmorItemEndSlot].Item, character.GetMountKeySeed()));
            if (matrix.HasValue)
                agentBuildData.InitialFrame(matrix.Value);
            return this.Mission.SpawnAgent(agentBuildData, false, 0);
        }

        private TL.MatrixFrame GetTroopMatrixFrame(int i, TL.Vec2 startPos, bool isPlayerSide)
        {
            var xInterval = this.TestBattleConfig.soldierXInterval;
            var yInterval = this.TestBattleConfig.soldierYInterval;
            var agentDefaultDir = new TL.Vec2(0, 1);
            var x = i / TestBattleConfig.SoldiersPerRow;
            var y = i % TestBattleConfig.SoldiersPerRow;
            var xDir = this.TestBattleConfig.FormationDirection;
            var yDir = this.TestBattleConfig.FormationDirection.LeftVec();
            var mat = TL.Mat3.Identity;
            mat.RotateAboutUp(agentDefaultDir.AngleBetween(isPlayerSide ? xDir : -xDir));
            var pos = startPos + xDir * xInterval * x + yDir * yInterval * y;
            if (!isPlayerSide)
                pos += xDir * this.TestBattleConfig.distance;
            return new TL.MatrixFrame(mat, new TL.Vec3(pos.x, pos.y, GetSceneHeightForAgent(pos)));
        }
    }
}