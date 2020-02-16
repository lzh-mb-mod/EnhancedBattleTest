using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TL = TaleWorlds.Library;

namespace Modbed
{
    public class EnhancedBattleTestMissionController : MissionLogic
    {
        private Game _game;
        private EnhancedBattleTestParams _battleTestParams;

        public EnhancedBattleTestParams BattleTestParams
        {
            get => _battleTestParams;
            set
            {
                _battleTestParams = value;
                int playerNumber = this.BattleTestParams.useFreeCamera ? 0 : 1;
                _shouldCelebrateVictory = (playerNumber + this.BattleTestParams.playerSoldierCount) != 0 &&
                                          this.BattleTestParams.enemySoldierCount != 0;
            }
        }

        private bool _started;
        private bool _shouldCelebrateVictory;
        private bool _ended = false;
        public Action<TL.Vec3> freeCameraInitialPos;
        public Agent _playerAgent;
        private Team playerTeam;
        private Team enemyTeam;
        private AgentVictoryLogic _victoryLogic;
        private EnhancedBattleTestMakeGruntLogic _makeGruntLogic;

        public EnhancedBattleTestMissionController()
        {
            this._game = Game.Current;
            _started = false;
            _victoryLogic = null;
        }

        public override void AfterStart()
        {
            this.Mission.MissionTeamAIType = Mission.MissionTeamAITypeEnum.FieldBattle;
            this.Mission.SetMissionMode(MissionMode.Battle, true);
            _makeGruntLogic = this.Mission.GetMissionBehaviour<EnhancedBattleTestMakeGruntLogic>();
            this.Mission.AllowAiTicking = false;
        }
        
        public void AddTeams()
        {
            // see TaleWorlds.MountAndBlade.dll/MissionMultiplayerFlagDomination.cs : TaleWorlds.MountAndBlade.MissionMultiplayerFlagDomination.AfterStart();
            var playerTeamCulture = this.BattleTestParams.playerSoldierCount == 0 ? this.BattleTestParams.PlayerHeroClass.Culture : this.BattleTestParams.PlayerTroopHeroClass.Culture;
            Banner playerTeamBanner = new Banner(playerTeamCulture.BannerKey,
                playerTeamCulture.BackgroundColor1,
                playerTeamCulture.ForegroundColor1);
            playerTeam = this.Mission.Teams.Add(BattleSideEnum.Attacker, color: playerTeamCulture.BackgroundColor1, color2: playerTeamCulture.ForegroundColor1, banner: playerTeamBanner);
            playerTeam.AddTeamAI(new TeamAIGeneral(this.Mission, playerTeam, 0f, 0f));
            playerTeam.AddTacticOption(new TacticCharge(playerTeam));
            playerTeam.ExpireAIQuerySystem();
            playerTeam.ResetTactic();
            playerTeam.OnOrderIssued += (type, formations, param) =>
            {
                foreach (var formation in formations)
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

                    _makeGruntLogic?.AddFormation(formation, 1f);
                }
                _victoryLogic = null;
            };
            this.Mission.PlayerTeam = playerTeam;

            var enemyTeamCulture = this.BattleTestParams.EnemyTroopHeroClass.Culture;
            Banner enemyTeamBanner = new Banner(enemyTeamCulture.BannerKey,
                enemyTeamCulture.BackgroundColor2, enemyTeamCulture.ForegroundColor2);
            enemyTeam = this.Mission.Teams.Add(BattleSideEnum.Defender, color: enemyTeamCulture.BackgroundColor2, color2: enemyTeamCulture.ForegroundColor2, banner: enemyTeamBanner);
            enemyTeam.AddTeamAI(new TeamAIGeneral(this.Mission, enemyTeam, 0f, 0f));
            enemyTeam.AddTacticOption(new TacticCharge(enemyTeam));
            enemyTeam.ExpireAIQuerySystem();
            enemyTeam.ResetTactic();

            enemyTeam.SetIsEnemyOf(playerTeam, true);
            playerTeam.SetIsEnemyOf(enemyTeam, true);

            this.Mission.AllowAiTicking = true;
        }

        public void SpawnAgents()
        {
            var playerTeamCulture = this.BattleTestParams.playerSoldierCount == 0 ? this.BattleTestParams.PlayerHeroClass.Culture : this.BattleTestParams.PlayerTroopHeroClass.Culture;
            var enemyTeamCulture = this.BattleTestParams.EnemyTroopHeroClass.Culture;

            // see TaleWorlds.MountAndBlade.dll/FlagDominationSpawningBehaviour.cs: TaleWorlds.MountAndBlade.FlagDominationSpawningBehaviour.SpawnAgents()
            // see TaleWorlds.MountAndBlade.dll/SpawningBehaviourBase.cs: TaleWorlds.MountAndBlade.SpawningBehaviourBase.OnTick()
            this._started = true;
            var scene = this.Mission.Scene;

            if (this.BattleTestParams.skyBrightness >= 0)
            {
                scene.SetSkyBrightness(this.BattleTestParams.skyBrightness);
            }

            if (this.BattleTestParams.rainDensity >= 0)
            {
                scene.SetRainDensity(this.BattleTestParams.rainDensity);
            }

            var xInterval = this.BattleTestParams.soldierXInterval;
            var yInterval = this.BattleTestParams.soldierYInterval;
            var soldiersPerRow = this.BattleTestParams.soldiersPerRow;

            var startPos = this.BattleTestParams.formationPosition;
            var xDir = this.BattleTestParams.formationDirection;
            var yDir = this.BattleTestParams.formationDirection.LeftVec();
            var agentDefaultDir = new TL.Vec2(0, 1);
            var useFreeCamera = this.BattleTestParams.useFreeCamera;

            BasicCharacterObject playerTroopCharacter = this.BattleTestParams.PlayerTroopHeroClass.TroopCharacter;
            var playerTroopFormationClass = playerTroopCharacter.CurrentFormationClass;
            var playerTroopFormation = playerTeam.GetFormation(playerTroopFormationClass);

            var playerPosVec2 = startPos + xDir * -10 + yDir * -10;
            var playerPos = new TL.Vec3(playerPosVec2.x, playerPosVec2.y, this.Mission.Scene.GetTerrainHeight(playerPosVec2));
            if (!useFreeCamera)
            {
                var playerMat = TL.Mat3.Identity;
                playerMat.RotateAboutUp(agentDefaultDir.AngleBetween(xDir));
                BasicCultureObject playerCulture = this.BattleTestParams.PlayerHeroClass.Culture;
                BasicCharacterObject playerCharacter = this.BattleTestParams.PlayerHeroClass.HeroCharacter;
                Equipment equipment = applyPerkFor(this.BattleTestParams.PlayerHeroClass, true,
                    this.BattleTestParams.playerSelectedPerk);
                AgentBuildData agentBuildData = new AgentBuildData(new BasicBattleAgentOrigin(playerCharacter))
                    .Team(playerTeam)
                    .Banner(playerTeam.Banner)
                    .ClothingColor1(playerCulture.Color)
                    .ClothingColor2(playerCulture.Color2)
                    .IsFemale(false)
                    .InitialFrame(new TL.MatrixFrame(playerMat, playerPos))
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
                var c = this.BattleTestParams.playerSoldierCount;
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
            
            for (var i = 0; i < this.BattleTestParams.playerSoldierCount; i += 1)
            {
                Equipment equipment = applyPerkFor(this.BattleTestParams.PlayerTroopHeroClass, false,
                    this.BattleTestParams.playerTroopSelectedPerk);
                AgentBuildData soldierBuildData = new AgentBuildData(new BasicBattleAgentOrigin(playerTroopCharacter))
                    .Team(playerTeam)
                    .ClothingColor1(playerTeamCulture.Color)
                    .ClothingColor2(playerTeamCulture.Color2)
                    .Banner(playerTeam.Banner)
                    .IsFemale(false)
                    .Formation(playerTroopFormation)
                    .Equipment(equipment);

                if (!mapHasNavMesh)
                {
                    var x = i / soldiersPerRow;
                    var y = i % soldiersPerRow;
                    var mat = TL.Mat3.Identity;
                    var pos = startPos + xDir * (-xInterval * x) + yDir * yInterval * y;
                    mat.RotateAboutUp(agentDefaultDir.AngleBetween(xDir));
                    var agentFrame = new TaleWorlds.Library.MatrixFrame(mat, new TL.Vec3(pos.x, pos.y, this.Mission.Scene.GetTerrainHeight(pos)));
                    soldierBuildData.InitialFrame(agentFrame);
                }

                var agent = this.Mission.SpawnAgent(soldierBuildData);
                agent.WieldInitialWeapons();
                agent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
            }
            BasicCharacterObject enemyCharacter = this.BattleTestParams.EnemyTroopHeroClass.TroopCharacter;
            
            var enemyFormationClass = enemyCharacter.CurrentFormationClass;
            var enemyFormation = enemyTeam.GetFormation(enemyFormationClass);
            {
                float width = this.getInitialFormationWidth(enemyTeam, enemyFormationClass);
                var centerPos = startPos + yDir * (width / 2) + xDir * this.BattleTestParams.distance;
                var wp = new WorldPosition(scene, centerPos.ToVec3());
                enemyFormation.SetPositioning(wp, -xDir, null);
                enemyFormation.FormOrder = FormOrder.FormOrderCustom(width);
            }
            
            for (var i = 0; i < this.BattleTestParams.enemySoldierCount; i += 1)
            {
                Equipment equipment = applyPerkFor(this.BattleTestParams.EnemyTroopHeroClass, false,
                    this.BattleTestParams.enemyTroopSelectedPerk);
                AgentBuildData enemyBuildData = new AgentBuildData(new BasicBattleAgentOrigin(enemyCharacter))
                    .Team(enemyTeam)
                    .ClothingColor1(enemyTeamCulture.ClothAlternativeColor)
                    .ClothingColor2(enemyTeamCulture.ClothAlternativeColor2)
                    .Banner(enemyTeam.Banner)
                    .Formation(enemyFormation)
                    .Equipment(equipment);;

                if (!mapHasNavMesh)
                {
                    var x = i / soldiersPerRow;
                    var y = i % soldiersPerRow;
                    var mat = TL.Mat3.Identity;
                    mat.RotateAboutUp(agentDefaultDir.AngleBetween(-xDir));
                    var pos = startPos + xDir * this.BattleTestParams.distance + xDir * xInterval * x + yDir * yInterval * y;
                    var agentFrame = new TaleWorlds.Library.MatrixFrame(TaleWorlds.Library.Mat3.Identity, new TL.Vec3(pos.x, pos.y, this.Mission.Scene.GetTerrainHeight(pos)));
                    enemyBuildData.InitialFrame(agentFrame);
                }
                var agent = this.Mission.SpawnAgent(enemyBuildData);
                agent.WieldInitialWeapons();
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
            var bp = this.BattleTestParams;
            var formation = team.GetFormation(fc);
            var mounted = fc == FormationClass.Cavalry || fc == FormationClass.HorseArcher;
            var unitDiameter = Formation.GetDefaultUnitDiameter(mounted);
            var unitSpacing = 1;
            var interval = mounted ? Formation.CavalryInterval(unitSpacing) : Formation.InfantryInterval(unitSpacing);
            var actualSoldiersPerRow = System.Math.Min(bp.soldiersPerRow, team == playerTeam ? bp.playerSoldierCount : bp.enemySoldierCount);
            var width = (actualSoldiersPerRow - 1) * (unitDiameter + interval) + unitDiameter + 0.1f;
            return width;
        }

        public override bool MissionEnded(ref MissionResult missionResult)
        {
            return true;
        }

        public override void OnMissionTick(float dt)
        {
            if (this._started)
            {
                CheckVictory();
                if (this.Mission.InputManager.IsKeyPressed(TaleWorlds.InputSystem.InputKey.C))
                {
                    this.SwitchCamera();
                }
                if (this.Mission.InputManager.IsKeyPressed(TaleWorlds.InputSystem.InputKey.F))
                {
                    if (this._playerAgent == null || !this._playerAgent.IsActive())
                    {
                        try
                        {
                            Agent closestAllyAgent = this.Mission.GetClosestAllyAgent(this.Mission.PlayerTeam, new WorldPosition(this.Mission.Scene, this.Mission.Scene.LastFinalRenderCameraPosition).GetGroundVec3(), 20f);
                            if (closestAllyAgent != null)
                            {
                                this.displayMessage("Taking control of ally troops nearby.");
                                if (this._playerAgent != null && this._playerAgent.IsActive())
                                    this._playerAgent.Controller = Agent.ControllerType.AI;
                                closestAllyAgent.Controller = Agent.ControllerType.Player;
                                this.Mission.MainAgent = closestAllyAgent;
                                this._playerAgent = closestAllyAgent;
                                foreach (var formation in this.playerTeam.Formations)
                                {
                                    formation.PlayerOwner = closestAllyAgent;
                                    playerTeam.PlayerOrderController.Owner = closestAllyAgent;
                                }

                                foreach (var missionBehaviour in Mission.Current.MissionBehaviours)
                                {
                                    missionBehaviour.OnAssignPlayerAsSergeantOfFormation(closestAllyAgent);
                                }
                                return;
                            }
                            this.displayMessage("No ally troop nearby.");
                            return;
                        }
                        catch (Exception ex)
                        {
                            this.displayMessage(ex.Message);
                            return;
                        }
                    }
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
                equipment[perkEffectsForPerk.NewItemIndex] = perkEffectsForPerk.NewItem.EquipmentElement;
            return equipment;
        }

        private void CheckVictory()
        {
            if (!_ended && _shouldCelebrateVictory)
            {
                bool playerVictory = this.enemyTeam.ActiveAgents.IsEmpty();
                bool enemyVictory = this.playerTeam.ActiveAgents.IsEmpty();
                if (!playerVictory && !enemyVictory)
                    return;
                _ended = true;
                _victoryLogic = this.Mission.GetMissionBehaviour<AgentVictoryLogic>();
                if (_victoryLogic == null)
                    return;
                if (enemyVictory)
                {
                    _ended = true;
                    _victoryLogic.SetTimersOfVictoryReactions(this.enemyTeam.Side);
                }
                else
                {
                    _ended = true;
                    _victoryLogic.SetTimersOfVictoryReactions(this.playerTeam.Side);
                }
            }
        }

        private void SwitchCamera()
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
                this.Mission.MainAgent = this._playerAgent;
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