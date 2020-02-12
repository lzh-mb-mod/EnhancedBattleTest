using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Screens;
using TaleWorlds.MountAndBlade;
using TL = TaleWorlds.Library;

namespace Modbed
{
    public class EnhancedBattleTestMissionController : MissionLogic
    {
        private Game _game;
        public EnhancedBattleTestParams battleTestParams;
        private bool _shouldStart;
        private bool _started;
        private bool _shouldExit;
        public Action<TL.Vec3> freeCameraInitialPos;
        public Agent _playerAgent;
        private Team playerTeam;
        private Team enemyTeam;

        public EnhancedBattleTestMissionController()
        {
            this._game = Game.Current;
            _shouldStart = _started = false;
            _shouldExit = false;
        }

        public override void AfterStart()
        {
            var scene = this.Mission.Scene;
            
            this.Mission.MissionTeamAIType = Mission.MissionTeamAITypeEnum.FieldBattle;
            this.Mission.SetMissionMode(MissionMode.Battle, true);
            playerTeam = this.Mission.Teams.Add(BattleSideEnum.Attacker, 0xff3f51b5, banner:Banner.CreateRandomBanner());
            playerTeam.AddTeamAI(new TeamAIGeneral(this.Mission, playerTeam));
            playerTeam.AddTacticOption(new TacticCharge(playerTeam));
            // playerTeam.AddTacticOption(new TacticFullScaleAttack(playerTeam));
            playerTeam.ExpireAIQuerySystem();
            playerTeam.ResetTactic();
            this.Mission.PlayerTeam = playerTeam;

            enemyTeam = this.Mission.Teams.Add(BattleSideEnum.Defender, 0xffff6090, banner: Banner.CreateRandomBanner());
            enemyTeam.AddTeamAI(new TeamAIGeneral(this.Mission, enemyTeam));
            enemyTeam.AddTacticOption(new TacticCharge(enemyTeam));
            // enemyTeam.AddTacticOption(new TacticFullScaleAttack(enemyTeam));

            enemyTeam.SetIsEnemyOf(playerTeam, true);
            playerTeam.SetIsEnemyOf(enemyTeam, true);
            enemyTeam.ExpireAIQuerySystem();
            enemyTeam.ResetTactic();
        }

        public void ShouldStart(EnhancedBattleTestParams param)
        {
            //this._shouldStart = true;
            this.battleTestParams = param;
            AfterStart2();
        }

        public void ShouldExit()
        {
            _shouldExit = true;
        }

        private void AfterStart2()
        {
            // see TaleWorlds.MountAndBlade.dll/FlagDominationSpawningBehaviour.cs: TaleWorlds.MountAndBlade.FlagDominationSpawningBehaviour.SpawnAgents()
            this._started = true;
            var scene = this.Mission.Scene;

            if (this.battleTestParams.skyBrightness >= 0)
            {
                scene.SetSkyBrightness(this.battleTestParams.skyBrightness);
            }

            if (this.battleTestParams.rainDensity >= 0)
            {
                scene.SetRainDensity(this.battleTestParams.rainDensity);
            }

            var xInterval = this.battleTestParams.soldierXInterval;
            var yInterval = this.battleTestParams.soldierYInterval;
            var soldiersPerRow = this.battleTestParams.soldiersPerRow;

            var startPos = this.battleTestParams.formationPosition;
            var xDir = this.battleTestParams.formationDirection;
            var yDir = this.battleTestParams.formationDirection.LeftVec();
            var agentDefaultDir = new TL.Vec2(0, 1);
            var useFreeCamera = this.battleTestParams.useFreeCamera;


            BasicCharacterObject soldierCharacter = this.battleTestParams.PlayerTroopHeroClass.TroopCharacter;
            var playerTroopFormationClass = FormationClass.Infantry;
            var playerTroopFormation = playerTeam.GetFormation(playerTroopFormationClass);

            var playerPosVec2 = startPos + xDir * -10 + yDir * -10;
            var playerPos = new TL.Vec3(playerPosVec2.x, playerPosVec2.y, 30);
            if (!useFreeCamera)
            {
                var playerMat = TL.Mat3.Identity;
                playerMat.RotateAboutUp(agentDefaultDir.AngleBetween(xDir));
                BasicCharacterObject playerCharacter = this.battleTestParams.PlayerHeroClass.HeroCharacter;
                Equipment equipment = applyPerkFor(this.battleTestParams.PlayerHeroClass, true,
                    this.battleTestParams.playerSelectedPerk);
                AgentBuildData agentBuildData = new AgentBuildData(new BasicBattleAgentOrigin(playerCharacter))
                    .ClothingColor1(0xff3f51b5)
                    .ClothingColor2(0xff3f51b5)
                    .IsFemale(false)
                    .InitialFrame(new TL.MatrixFrame(playerMat, playerPos))
                    .Team(playerTeam)
                    .Equipment(equipment);

                Agent player = this.Mission.SpawnAgent(agentBuildData, false, 0);
                player.Controller = Agent.ControllerType.Player;
                player.WieldInitialWeapons();
                player.AllowFirstPersonWideRotation();

                Mission.MainAgent = player;
                playerTroopFormation.PlayerOwner = player;
                playerTeam.PlayerOrderController.Owner = player;
                this._playerAgent = player;
            }
            else
            {
                var c = this.battleTestParams.playerSoldierCount;
                if (c <= 0)
                {
                    this.freeCameraInitialPos(new TL.Vec3(startPos.x, startPos.y, 30));
                }
                else
                {
                    var rowCount = (c + soldiersPerRow - 1) / soldiersPerRow;
                    var p = startPos + (System.Math.Min(soldiersPerRow, c) - 1) / 2 * yInterval * yDir - rowCount * xInterval * xDir;
                    this.freeCameraInitialPos(new TL.Vec3(p.x, p.y, 5));
                }
            }

            var mapHasNavMesh = false;
            {
                var width = this.getInitialFormationWidth(playerTeam, playerTroopFormationClass);
                var centerPos = startPos + yDir * (width / 2);
                var wp = new WorldPosition(scene, centerPos.ToVec3());
                playerTroopFormation.SetPositioning(wp, xDir, null);
                playerTroopFormation.FormOrder = FormOrder.FormOrderCustom(width);
                mapHasNavMesh = wp.GetNavMesh() != System.UIntPtr.Zero;
            }
            
            for (var i = 0; i < this.battleTestParams.playerSoldierCount; i += 1)
            {

                Equipment equipment = applyPerkFor(this.battleTestParams.PlayerTroopHeroClass, false,
                    this.battleTestParams.playerTroopSelectedPerk);
                AgentBuildData soldierBuildData = new AgentBuildData(new BasicBattleAgentOrigin(soldierCharacter))
                    .ClothingColor1(playerTeam.Color)
                    .ClothingColor2(playerTeam.Color2)
                    .Banner(playerTeam.Banner)
                    .IsFemale(false)   
                    .Team(playerTeam)
                    .Formation(playerTroopFormation)
                    .Equipment(equipment);

                if (!mapHasNavMesh)
                {
                    var x = i / soldiersPerRow;
                    var y = i % soldiersPerRow;
                    var mat = TL.Mat3.Identity;
                    var pos = startPos + xDir * (-xInterval * x) + yDir * yInterval * y;
                    mat.RotateAboutUp(agentDefaultDir.AngleBetween(xDir));
                    var agentFrame = new TaleWorlds.Library.MatrixFrame(mat, new TL.Vec3(pos.x, pos.y, 30));
                    soldierBuildData.InitialFrame(agentFrame);
                }

                var agent = this.Mission.SpawnAgent(soldierBuildData);
                agent.SetTeam(playerTeam, true);
                agent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
            }

            BasicCharacterObject enemyCharacter = this.battleTestParams.EnemyTroopHeroClass.TroopCharacter;
            
            var enemyFormationClass = FormationClass.Ranged;
            var enemyFormation = enemyTeam.GetFormation(enemyFormationClass);
            {
                float width = this.getInitialFormationWidth(enemyTeam, enemyFormationClass);
                var centerPos = startPos + yDir * (width / 2) + xDir * this.battleTestParams.distance;
                var wp = new WorldPosition(scene, centerPos.ToVec3());
                enemyFormation.SetPositioning(wp, -xDir, null);
                enemyFormation.FormOrder = FormOrder.FormOrderCustom(width);
            }

            for (var i = 0; i < this.battleTestParams.enemySoldierCount; i += 1)
            {
                Equipment equipment = applyPerkFor(this.battleTestParams.EnemyTroopHeroClass, false,
                    this.battleTestParams.enemyTroopSelectedPerk);
                AgentBuildData enemyBuildData = new AgentBuildData(new BasicBattleAgentOrigin(enemyCharacter))
                    .ClothingColor1(enemyTeam.Color)
                    .ClothingColor2(enemyTeam.Color2)
                    .Banner(enemyTeam.Banner)
                    .Team(enemyTeam)
                    .Formation(enemyFormation)
                    .Equipment(equipment);;

                if (!mapHasNavMesh)
                {
                    var x = i / soldiersPerRow;
                    var y = i % soldiersPerRow;
                    var mat = TL.Mat3.Identity;
                    mat.RotateAboutUp(agentDefaultDir.AngleBetween(-xDir));
                    var pos = startPos + xDir * this.battleTestParams.distance + xDir * xInterval * x + yDir * yInterval * y;
                    var agentFrame = new TaleWorlds.Library.MatrixFrame(TaleWorlds.Library.Mat3.Identity, new TL.Vec3(pos.x, pos.y, 30));
                    enemyBuildData.InitialFrame(agentFrame);
                }
                var agent = this.Mission.SpawnAgent(enemyBuildData);
                agent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
            }

            {
                var a = this.Mission.IsOrderShoutingAllowed();
                var b = this.Mission.IsAgentInteractionAllowed();
                var c = GameNetwork.IsClientOrReplay;
                var d = playerTeam.PlayerOrderController.Owner == null;
                ModuleLogger.Log("mission allowed shouting: {0} interaction: {1} {2} {3}", a, b, c, d);
            }
        }

        float getInitialFormationWidth(Team team, FormationClass fc)
        {
            var bp = this.battleTestParams;
            var formation = team.GetFormation(fc);
            var mounted = fc == FormationClass.Cavalry || fc == FormationClass.HorseArcher;
            var unitDiameter = Formation.GetDefaultUnitDiameter(mounted);
            var unitSpacing = 1;
            var interval = mounted ? Formation.CavalryInterval(unitSpacing) : Formation.InfantryInterval(unitSpacing);
            var actualSoldiersPerRow = System.Math.Min(bp.soldiersPerRow, bp.playerSoldierCount);
            var width = (actualSoldiersPerRow - 1) * (unitDiameter + interval) + unitDiameter + 0.1f;
            return width;
        }

        public override bool MissionEnded(ref MissionResult missionResult)
        {
            return _shouldExit;
        }

        public override void OnMissionTick(float dt)
        {
            if (!this._started)
            {
                if (this._shouldStart)
                {
                    try
                    {
                        this.AfterStart2();
                        this._shouldStart = false;
                    }
                    catch (System.Exception e)
                    {
                        ModuleLogger.Log("{0}", e);
                    }
                }
            }
            else
            {
                var scene = this.Mission.Scene;
                if (this.Mission.InputManager.IsKeyPressed(TaleWorlds.InputSystem.InputKey.C))
                {
                    this.SwitchCamera();
                }

                if (this.Mission.InputManager.IsGameKeyPressed((int) GameKeyDefinition.Leave))
                {
                    _shouldExit = true;
                    this.Mission.EndMission();
                }
            }
        }

        private Equipment applyPerkFor(MultiplayerClassDivisions.MPHeroClass heroClass, bool isPlayer, int selectedPerk)
        {
            BasicCharacterObject character = isPlayer ? heroClass.HeroCharacter : heroClass.TroopCharacter;
            List<MPPerkObject> selectedPerkList = new List<MPPerkObject>();
            selectedPerkList.Add(heroClass.GetAllAvailablePerksForListIndex(1)[selectedPerk]);
            Equipment equipment = isPlayer ? character.Equipment.Clone() : Equipment.GetRandomEquipmentElements(character, false, false, MBRandom.RandomInt());
            foreach (PerkEffect perkEffectsForPerk in MPPerkObject.SelectRandomPerkEffectsForPerks(isPlayer, PerkType.PerkAlternativeEquipment, selectedPerkList))
                equipment[perkEffectsForPerk.NewItemIndex] = perkEffectsForPerk.NewItem;
            return equipment;
        }

        void SwitchCamera()
        {
            ModuleLogger.Log("SwitchCamera");
            if (this._playerAgent == null || !this._playerAgent.IsActive())
            {
                this.displayMessage("no player agent");
                return;
            }
            if (this.Mission.MainAgent == null)
            {
                this._playerAgent.Controller = Agent.ControllerType.Player;
                this.displayMessage("switch to player agent");
            }
            else
            {
                this.Mission.MainAgent = null;
                this._playerAgent.Controller = Agent.ControllerType.AI;
                var wp = this._playerAgent.GetWorldPosition();
                this._playerAgent.SetScriptedPosition(ref wp, Agent.AIScriptedFrameFlags.DoNotRun, "camera switch");
                this.displayMessage("switch to free camera");
            }
        }

        void displayMessage(string msg)
        {
            InformationManager.DisplayMessage(new InformationMessage(new TaleWorlds.Localization.TextObject(msg, null).ToString()));
        }
    }
}