using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    class EnhancedMissionCombatantsLogic : MissionCombatantsLogic
    {
        private Mission.MissionTeamAITypeEnum _teamAIType;
        private IEnumerable<IBattleCombatant> _battleCombatants;
        private BattleConfigBase _config;

        public EnhancedMissionCombatantsLogic(
            IEnumerable<IBattleCombatant> battleCombatants,
            IBattleCombatant playerBattleCombatant,
            IBattleCombatant defenderLeaderBattleCombatant,
            IBattleCombatant attackerLeaderBattleCombatant,
            Mission.MissionTeamAITypeEnum teamAIType,
            bool isPlayerSergeant, BattleConfigBase config)
            : base(battleCombatants, playerBattleCombatant, defenderLeaderBattleCombatant,
                attackerLeaderBattleCombatant, teamAIType, isPlayerSergeant)
        {
            _teamAIType = teamAIType;
            if (battleCombatants == null)
                battleCombatants = new IBattleCombatant[2]
                {
                    defenderLeaderBattleCombatant,
                    attackerLeaderBattleCombatant
                };
            this._battleCombatants = battleCombatants;
            _config = config;
        }


        public override void EarlyStart()
        {
            Mission.MissionTeamAIType = this._teamAIType;
            switch (this._teamAIType)
            {
                case Mission.MissionTeamAITypeEnum.FieldBattle:
                    using (IEnumerator<Team> enumerator = Mission.Teams.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            Team current = enumerator.Current;
                            current.AddTeamAI((TeamAIComponent)new TeamAIGeneral(this.Mission, current, 10f, 1f), false);
                        }
                        break;
                    }
                case Mission.MissionTeamAITypeEnum.Siege:
                    using (IEnumerator<Team> enumerator = Mission.Teams.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            Team current = enumerator.Current;
                            if (current.Side == BattleSideEnum.Attacker)
                                current.AddTeamAI((TeamAIComponent)new TeamAISiegeAttacker(this.Mission, current, 5f, 1f), false);
                            if (current.Side == BattleSideEnum.Defender)
                                current.AddTeamAI((TeamAIComponent)new TeamAISiegeDefender(this.Mission, current, 5f, 1f), false);
                        }
                        break;
                    }
                case Mission.MissionTeamAITypeEnum.SallyOut:
                    using (IEnumerator<Team> enumerator = Mission.Teams.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            Team current = enumerator.Current;
                            if (current.Side == BattleSideEnum.Attacker)
                                current.AddTeamAI((TeamAIComponent)new TeamAISallyOutDefender(this.Mission, current, 5f, 1f), false);
                            else
                                current.AddTeamAI((TeamAIComponent)new TeamAISallyOutAttacker(this.Mission, current, 5f, 1f), false);
                        }
                        break;
                    }
            }
            if (!Mission.Teams.Any<Team>())
                return;
            switch (Mission.MissionTeamAIType)
            {
                case Mission.MissionTeamAITypeEnum.NoTeamAI:
                    using (IEnumerator<Team> enumerator = Mission.Teams.Where<Team>((Func<Team, bool>)(t => t.HasTeamAi)).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            Team current = enumerator.Current;
                            current.AddTacticOption((TacticComponent)new TacticCharge(current));
                        }
                        break;
                    }
                case Mission.MissionTeamAITypeEnum.FieldBattle:
                case Mission.MissionTeamAITypeEnum.Siege:
                    using (IEnumerator<Team> enumerator = Mission.Teams.Where<Team>((Func<Team, bool>)(t => t.HasTeamAi)).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            Team team = enumerator.Current;
                            if (team.Side == BattleSideEnum.Defender)
                            {
                                bool flag = false;
                                foreach (var defenderTacticOption in _config.defenderTacticOptions)
                                {
                                    if (defenderTacticOption.isEnabled)
                                    {
                                        flag = true;
                                        TacticOptionHelper.AddTacticComponent(team, defenderTacticOption.tacticOption);
                                    }
                                }
                                if (!flag)
                                    team.AddTacticOption(new TacticCharge(team));
                            }

                            if (team.Side == BattleSideEnum.Attacker)
                            {
                                bool flag = false;
                                foreach (var attackerTacticOption in _config.attackerTacticOptions)
                                {
                                    if (attackerTacticOption.isEnabled)
                                    {
                                        flag = true;
                                        TacticOptionHelper.AddTacticComponent(team, attackerTacticOption.tacticOption);
                                    }
                                }
                                if (!flag)
                                    team.AddTacticOption(new TacticCharge(team));
                            }
                        }
                        break;
                    }
                case Mission.MissionTeamAITypeEnum.SallyOut:
                    using (IEnumerator<Team> enumerator = Mission.Teams.Where<Team>((Func<Team, bool>)(t => t.HasTeamAi)).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            Team current = enumerator.Current;
                            if (current.Side == BattleSideEnum.Defender)
                                current.AddTacticOption((TacticComponent)new TacticSallyOutHitAndRun(current));
                            if (current.Side == BattleSideEnum.Attacker)
                                current.AddTacticOption((TacticComponent)new TacticSallyOutDefense(current));
                            current.AddTacticOption((TacticComponent)new TacticCharge(current));
                        }
                        break;
                    }
            }
            foreach (Team team in (ReadOnlyCollection<Team>)this.Mission.Teams)
            {
                team.ExpireAIQuerySystem();
                team.ResetTactic();
            }
        }
    }
}
