using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.AI;
using TaleWorlds.MountAndBlade.Missions;
using TaleWorlds.MountAndBlade.Missions.Handlers;

namespace EnhancedBattleTest
{
    class EnhancedSiegeMissionController : SiegeMissionController
    {
        private SiegeDeploymentHandler _siegeDeploymentHandler;
        private MissionBoundaryPlacer _missionBoundaryPlacer;
        private MissionAgentSpawnLogic _missionAgentSpawnLogic;
        private readonly Dictionary<Type, int> _availableSiegeWeaponsOfAttackers;
        private readonly Dictionary<Type, int> _availableSiegeWeaponsOfDefenders;
        private readonly bool _isPlayerAttacker;
        private bool _teamSetupOver;

        public EnhancedSiegeMissionController(
          Dictionary<Type, int> siegeWeaponsCountOfAttackers,
          Dictionary<Type, int> siegeWeaponsCountOfDefenders,
          bool isPlayerAttacker,
          bool isSallyOut)
            : base(siegeWeaponsCountOfAttackers, siegeWeaponsCountOfDefenders, isPlayerAttacker, isSallyOut)
        {
            this._availableSiegeWeaponsOfAttackers = siegeWeaponsCountOfAttackers;
            this._availableSiegeWeaponsOfDefenders = siegeWeaponsCountOfDefenders;
            this._isPlayerAttacker = isPlayerAttacker;
        }

        public override void OnBehaviourInitialize()
        {
            base.OnBehaviourInitialize();
            this._siegeDeploymentHandler = this.Mission.GetMissionBehaviour<SiegeDeploymentHandler>();
            this._missionBoundaryPlacer = this.Mission.GetMissionBehaviour<MissionBoundaryPlacer>();
            this._missionAgentSpawnLogic = this.Mission.GetMissionBehaviour<MissionAgentSpawnLogic>();
        }

        public override void AfterStart()
        {
            Mission.AllowAiTicking = false;
            this.Mission.GetMissionBehaviour<DeploymentHandler>().InitializeDeploymentPoints();
            bool isSallyOut = this.IsSallyOut;
            for (int index = 0; index < 2; ++index)
            {
                this._missionAgentSpawnLogic.SetSpawnHorses((BattleSideEnum)index, isSallyOut);
                this._missionAgentSpawnLogic.SetSpawnTroops((BattleSideEnum)index, false, false);
            }
        }

        private void SetupTeams()
        {
            if (this._isPlayerAttacker)
            {
                this.SetupTeam(this.Mission.AttackerTeam);
            }
            else
            {
                this.SetupTeam(this.Mission.AttackerTeam);
                this.OnTeamDeploymentFinish(BattleSideEnum.Attacker);
                this.SetupTeam(this.Mission.DefenderTeam);
            }
            this.SetupAllyTeam(this.Mission.PlayerAllyTeam);
            this._teamSetupOver = true;
        }

        public override void OnMissionTick(float dt)
        {
            if (this._teamSetupOver)
                return;
            this.SetupTeams();
            foreach (Team team in (ReadOnlyCollection<Team>)this.Mission.Teams)
            {
                foreach (Formation formation in team.Formations)
                    formation.QuerySystem.EvaluateAllPreliminaryQueryData();
            }
        }

        [Conditional("DEBUG")]
        private void DebugTick()
        {
            if (!Input.DebugInput.IsHotKeyPressed("SwapToEnemy"))
                return;
            this.Mission.MainAgent.Controller = Agent.ControllerType.AI;
            this.Mission.PlayerEnemyTeam.Leader.Controller = Agent.ControllerType.Player;
            this.SwapTeams();
        }

        private void SwapTeams()
        {
            this.Mission.PlayerTeam = this.Mission.PlayerEnemyTeam;
        }

        private void SetupAllyTeam(Team team)
        {
            if (team == null)
                return;
            foreach (Formation formation in team.FormationsIncludingSpecial)
                formation.IsAIControlled = true;
            team.ExpireAIQuerySystem();
        }

        private void RemoveUnavailableDeploymentPoints(BattleSideEnum side)
        {
            SiegeWeaponCollection weapons = new SiegeWeaponCollection(side == BattleSideEnum.Attacker ? this._availableSiegeWeaponsOfAttackers : this._availableSiegeWeaponsOfDefenders);
            foreach (DeploymentPoint deploymentPoint in this.Mission.ActiveMissionObjects.FindAllWithType<DeploymentPoint>().Where<DeploymentPoint>((Func<DeploymentPoint, bool>)(dp => dp.Side == side)).ToArray<DeploymentPoint>())
            {
                if (!deploymentPoint.DeployableWeaponTypes.Any<Type>((Func<Type, bool>)(wt => weapons.GetMaxDeployableWeaponCount(wt) > 0)))
                {
                    foreach (SynchedMissionObject synchedMissionObject in deploymentPoint.DeployableWeapons.Select<SynchedMissionObject, SiegeWeapon>((Func<SynchedMissionObject, SiegeWeapon>)(sw => sw as SiegeWeapon)))
                        synchedMissionObject.SetDisabledSynched();
                    deploymentPoint.SetDisabledSynched();
                }
            }
        }

        private void SetupTeam(Team team)
        {
            BattleSideEnum side = team.Side;
            //this._siegeDeploymentHandler.RemoveAllBoundaries();
            this._siegeDeploymentHandler.SetDeploymentBoundary(side);
            if (team == this.Mission.PlayerTeam)
            {
                this.RemoveUnavailableDeploymentPoints(side);
                foreach (DeploymentPoint deploymentPoint in this.Mission.ActiveMissionObjects.FindAllWithType<DeploymentPoint>().Where<DeploymentPoint>((Func<DeploymentPoint, bool>)(dp => !dp.IsDisabled && dp.Side == side)))
                    deploymentPoint.Show();
            }
            else
                this.DeploySiegeWeaponsByAi(side);
            this._missionAgentSpawnLogic.SetSpawnTroops(side, true, true);
            foreach (Agent agent in team.FormationsIncludingSpecial.SelectMany<Formation, Agent>((Func<Formation, IEnumerable<Agent>>)(f => (IEnumerable<Agent>)f.Units)).Where<Agent>((Func<Agent, bool>)(u => u.IsAIControlled)))
                agent.SetIsAIPaused(true);
        }

        private void DeploySiegeWeaponsByAi(BattleSideEnum side)
        {
            new SiegeWeaponDeploymentAI(this.Mission.ActiveMissionObjects.FindAllWithType<DeploymentPoint>().Where<DeploymentPoint>((Func<DeploymentPoint, bool>)(dp => dp.Side == side)).ToList<DeploymentPoint>(), side == BattleSideEnum.Attacker ? this._availableSiegeWeaponsOfAttackers : this._availableSiegeWeaponsOfDefenders).DeployAll(this.Mission, side);
            this.RemoveDeploymentPoints(side);
        }

        private void RemoveDeploymentPoints(BattleSideEnum side)
        {
            foreach (DeploymentPoint deploymentPoint in this.Mission.ActiveMissionObjects.FindAllWithType<DeploymentPoint>().Where<DeploymentPoint>((Func<DeploymentPoint, bool>)(dp => dp.Side == side)).ToArray<DeploymentPoint>())
            {
                foreach (SynchedMissionObject synchedMissionObject in deploymentPoint.DeployableWeapons.ToArray<SynchedMissionObject>())
                {
                    if ((deploymentPoint.DeployedWeapon == null || !synchedMissionObject.GameEntity.IsVisibleIncludeParents()) && synchedMissionObject is SiegeWeapon siegeWeapon)
                        siegeWeapon.SetDisabledSynched();
                }
                deploymentPoint.SetDisabledSynched();
            }
        }

        private void OnTeamDeploymentFinish(BattleSideEnum side)
        {
            this.RemoveDeploymentPoints(side);
            foreach (SynchedMissionObject synchedMissionObject in Mission.ActiveMissionObjects.FindAllWithType<SiegeLadder>().Where<SiegeLadder>((Func<SiegeLadder, bool>)(sl => !sl.GameEntity.IsVisibleIncludeParents())).ToList<SiegeLadder>())
                synchedMissionObject.SetDisabledSynched();
            Team team = side == BattleSideEnum.Attacker ? Mission.AttackerTeam : Mission.DefenderTeam;
            if (side != Mission.PlayerTeam.Side)
                this.DeployFormationsOfTeam(team);
            this._siegeDeploymentHandler.RemoveAllBoundaries();
            this._missionBoundaryPlacer.AddBoundaries();
            foreach (Formation formation in (side == BattleSideEnum.Attacker ? Mission.AttackerTeam : Mission.DefenderTeam).FormationsIncludingSpecialAndEmpty)
                formation.IsAIControlled = true;
        }

        private void DeployFormationsOfTeam(Team team)
        {
            foreach (Formation formation in team.Formations)
                formation.IsAIControlled = true;
            Mission.AllowAiTicking = true;
            Mission.ForceTickOccasionally = true;
            team.ResetTactic();
            bool teleportingAgents = Mission.IsTeleportingAgents;
            Mission.IsTeleportingAgents = true;
            team.Tick(0.0f);
            Mission.IsTeleportingAgents = teleportingAgents;
            Mission.AllowAiTicking = false;
            Mission.ForceTickOccasionally = false;
        }
    }
}
