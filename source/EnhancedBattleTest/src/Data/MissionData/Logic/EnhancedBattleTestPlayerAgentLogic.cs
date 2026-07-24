using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.Data.MissionData.Logic
{
    public sealed class EnhancedBattleTestPlayerAgentLogic : MissionLogic
    {
        private readonly CharacterObject _playerCharacter;
        private readonly PartyBase _playerParty;
        private Agent _playerAgent;

        public EnhancedBattleTestPlayerAgentLogic(
            CharacterObject playerCharacter,
            PartyBase playerParty)
        {
            _playerCharacter = playerCharacter;
            _playerParty = playerParty;
        }

        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            TrySetPlayerAgent(agent);
        }

        public override void OnAgentTeamChanged(
            Team previousTeam,
            Team newTeam,
            Agent agent)
        {
            TrySetPlayerAgent(agent);
        }

        public override void OnDeploymentFinished()
        {
            SetPlayerController();
        }

        private void TrySetPlayerAgent(Agent agent)
        {
            if (_playerCharacter == null
                || agent?.Character == null
                || agent.Team == null
                || agent.Character != _playerCharacter
                || agent.Team != Mission.PlayerTeam)
            {
                return;
            }
            if (!(agent.Origin is PartyGroupAgentOrigin origin)
                || origin.Party != _playerParty)
                return;

            _playerAgent = agent;
            SetPlayerController();
        }

        private void SetPlayerController()
        {
            if (_playerAgent == null || !_playerAgent.IsActive())
                return;

            Agent currentMainAgent = Mission.MainAgent;
            if (currentMainAgent != null
                && currentMainAgent != _playerAgent
                && currentMainAgent.IsActive())
            {
                currentMainAgent.Controller = Agent.ControllerType.AI;
            }

            _playerAgent.Controller = Agent.ControllerType.Player;
        }
    }
}
