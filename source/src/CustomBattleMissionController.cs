using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;

namespace EnhancedBattleTest
{
    class CustomBattleMissionController : MissionLogic
    {
        protected readonly Game game;
        private BasicCharacterObject _player;
        private bool _spawned = false;
        protected bool IsDeploymentFinished => this.Mission.GetMissionBehaviour<DeploymentHandler>() == null;

        public CustomBattleMissionController(BasicCharacterObject player)
        {
            this.game = Game.Current;
            _player = player;
        }

        public override void AfterStart()
        {
            //this.game.PlayerTroop = _player;
        }
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (!_spawned)
            {
                Agent agent = this.Mission.SpawnAgent(
                    new AgentBuildData(new BasicBattleAgentOrigin(_player))
                        .Team(this.Mission.PlayerTeam)
                        .Formation(this.Mission.PlayerTeam.GetFormation(_player.CurrentFormationClass))
                        .InitialFrame(this.Mission.GetFormationSpawnFrame(this.Mission.PlayerTeam.Side, _player.CurrentFormationClass, false, -1, 0.0f, true).ToGroundMatrixFrame())
                    , false, 0);
                agent.Controller = Agent.ControllerType.Player;
                agent.WieldInitialWeapons();
                agent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
                this.Mission.MainAgent = agent;
                Utility.SetPlayerAsCommander();

                _spawned = true;
                Agent enemyAgent = this.Mission.SpawnAgent(new AgentBuildData(new BasicBattleAgentOrigin(_player))
                    .Team(this.Mission.PlayerEnemyTeam)
                    .Formation(this.Mission.PlayerEnemyTeam.GetFormation(_player.CurrentFormationClass))
                    .InitialFrame(this.Mission.GetFormationSpawnFrame(this.Mission.PlayerEnemyTeam.Side, _player.CurrentFormationClass, false, -1, 0.0f, true).ToGroundMatrixFrame())
                    , false, 0);
                enemyAgent.Controller = Agent.ControllerType.AI;
                enemyAgent.WieldInitialWeapons();
                enemyAgent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
                var switchTeamLogic = this.Mission.GetMissionBehaviour<SwitchTeamLogic>();
                if (switchTeamLogic != null)
                    switchTeamLogic.enemyLeader = enemyAgent;
            }
        }
        public override bool MissionEnded(ref MissionResult missionResult)
        {
            if (!this.IsDeploymentFinished)
                return false;
            //if (this.IsPlayerDead())
            //{
            //    missionResult = MissionResult.CreateDefeated((IMission)this.Mission);
            //    return true;
            //}
            if (this.Mission.GetMemberCountOfSide(BattleSideEnum.Attacker) == 0)
            {
                missionResult = this.Mission.PlayerTeam.Side == BattleSideEnum.Attacker ? MissionResult.CreateDefeated((IMission)this.Mission) : MissionResult.CreateSuccessful((IMission)this.Mission);
                return true;
            }
            if (this.Mission.GetMemberCountOfSide(BattleSideEnum.Defender) != 0)
                return false;
            missionResult = this.Mission.PlayerTeam.Side == BattleSideEnum.Attacker ? MissionResult.CreateSuccessful((IMission)this.Mission) : MissionResult.CreateDefeated((IMission)this.Mission);
            return true;
        }

        
    }
}
