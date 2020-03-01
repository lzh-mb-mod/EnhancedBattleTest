using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    class EnhancedCustomBattleMissionController : MissionLogic
    {
        protected bool IsDeploymentFinished => this.Mission.GetMissionBehaviour<DeploymentHandler>() == null;
        

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

        //private Agent SpawnCommander(Team team, ClassInfo classInfo, bool isPlayer, bool isPlayerSide)
        //{
        //    MultiplayerClassDivisions.MPHeroClass mpHeroClass =
        //        MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>(classInfo.classStringId);
        //    BasicCharacterObject character = mpHeroClass.HeroCharacter;
        //    var agentBuildData = new AgentBuildData(new BasicBattleAgentOrigin(mpHeroClass.HeroCharacter))
        //        .Team(team)
        //        .Formation(team.GetFormation(Utility.CommanderFormationClass()))
        //        .Banner(team.Banner).ClothingColor1(isPlayerSide ? character.Culture.Color : character.Culture.ClothAlternativeColor).ClothingColor2(isPlayerSide ? character.Culture.Color2 : character.Culture.ClothAlternativeColor2)
        //        .InitialFrame(this.Mission
        //            .GetFormationSpawnFrame(team.Side, FormationClass.HeavyCavalry, false, -1, 0.0f, true)
        //            .ToGroundMatrixFrame())
        //        .Equipment(Utility.GetNewEquipmentsForPerks(classInfo, true));
        //    Agent agent = this.Mission.SpawnAgent(agentBuildData, false, 0);
        //    agent.WieldInitialWeapons();
        //    agent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
        //    agent.Controller = isPlayer ? Agent.ControllerType.Player : Agent.ControllerType.AI;
        //    return agent;
        //}
    }
}
