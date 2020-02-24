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
    class EnhancedCustomBattleMissionController : MissionLogic
    {
        private EnhancedCustomBattleConfig _config;
        private BasicCharacterObject _player;
        private BasicCharacterObject _enemyGeneral;
        private bool _spawned = false;
        protected bool IsDeploymentFinished => this.Mission.GetMissionBehaviour<DeploymentHandler>() == null;

        public EnhancedCustomBattleMissionController(EnhancedCustomBattleConfig config)
        {
            _config = config;
            _player = config.PlayerHeroClass.HeroCharacter;
            _enemyGeneral = config.EnemyHeroClass.HeroCharacter;
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
                _spawned = true;
                if (!_config.useFreeCamera)
                {
                    Agent playerAgent = SpawnGenerals(this.Mission.PlayerTeam, _player, true, true);
                    Utility.SetPlayerAsCommander();
                }

                var switchTeamLogic = this.Mission.GetMissionBehaviour<SwitchTeamLogic>();
                if (_config.spawnEnemyCommander)
                {
                    Agent enemyAgent = SpawnGenerals(this.Mission.PlayerEnemyTeam, _enemyGeneral, false, false);

                    if (switchTeamLogic != null)
                        switchTeamLogic.enemyLeader = enemyAgent;
                }

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

        private Agent SpawnGenerals(Team team, BasicCharacterObject character, bool isPlayer, bool isPlayerSide)
        {
            var agentBuildData = new AgentBuildData(new BasicBattleAgentOrigin(character))
                .Team(team)
                .Formation(team.GetFormation(FormationClass.HeavyCavalry))
                .Banner(team.Banner).ClothingColor1(isPlayerSide ? character.Culture.Color : character.Culture.ClothAlternativeColor).ClothingColor2(isPlayerSide ? character.Culture.Color2 : character.Culture.ClothAlternativeColor2)
                .InitialFrame(this.Mission
                    .GetFormationSpawnFrame(team.Side, FormationClass.HeavyCavalry, false, -1, 0.0f, true)
                    .ToGroundMatrixFrame());
            Agent agent = this.Mission.SpawnAgent(agentBuildData, false, 0);
            agent.WieldInitialWeapons();
            agent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
            agent.Controller = isPlayer ? Agent.ControllerType.Player : Agent.ControllerType.AI;
            return agent;
        }
    }
}
