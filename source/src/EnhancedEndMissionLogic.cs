using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.Source.Missions;

namespace EnhancedBattleTest
{
    class EnhancedEndMissionLogic : MissionLogic, IBattleEndLogic
    {
        private MissionTime _enemiesNotYetRetreatingTime;
        private BasicTimer _checkRetreatingTimer;
        private BattleConfigBase _config;
        private bool _shouldCelebrate;
        private bool _isEnemySideRetreating;
        private bool _isEnemySideDepleted;
        private bool _isPlayerSideDepleted;
        private bool _canCheckForEndCondition;
        private bool _missionEndedMessageShown;
        private bool _victoryReactionsActivated;
        private bool _victoryReactionsActivatedForRetreating;
        private bool _scoreBoardOpenedOnceOnMissionEnd;

        public bool PlayerVictory
        {
            get
            {
                return this._isEnemySideRetreating || this._isEnemySideDepleted;
            }
        }

        public bool EnemyVictory
        {
            get
            {
                return this._isPlayerSideDepleted;
            }
        }

        private bool _notificationsDisabled { get; set; }

        public EnhancedEndMissionLogic(BattleConfigBase config)
        {
            _config = config;
        }

        public override bool MissionEnded(ref MissionResult missionResult)
        {
            bool flag = false;
            if (this._isEnemySideRetreating || this._isEnemySideDepleted)
            {
                missionResult = MissionResult.CreateSuccessful((IMission)this.Mission);
                flag = true;
            }
            else if (this._isPlayerSideDepleted)
            {
                missionResult = MissionResult.CreateDefeated((IMission)this.Mission);
                flag = true;
            }
            return flag;
        }

        public override void OnMissionTick(float dt)
        {
            if (_shouldCelebrate && this.Mission.IsMissionEnding)
            {
                if (this._notificationsDisabled)
                    this._scoreBoardOpenedOnceOnMissionEnd = true;
                if (this._missionEndedMessageShown && !this._scoreBoardOpenedOnceOnMissionEnd)
                {
                    if ((double)this._checkRetreatingTimer.ElapsedTime > 7.0)
                    {
                        this.CheckIsEnemySideRetreatingOrOneSideDepleted();
                        this._checkRetreatingTimer.Reset();
                        if (this.Mission.MissionResult != null && this.Mission.MissionResult.PlayerDefeated)
                        {
                            GameTexts.SetVariable("leave_key", Game.Current.GameTextManager.GetHotKeyGameText("CombatHotKeyCategory", 4));
                            InformationManager.AddQuickInformation(GameTexts.FindText("str_battle_lost_press_tab_to_view_results", (string)null), 0, (BasicCharacterObject)null, "");
                        }
                        else if (this.Mission.MissionResult != null && this.Mission.MissionResult.PlayerVictory)
                        {
                            if (this._isEnemySideDepleted)
                            {
                                GameTexts.SetVariable("leave_key", Game.Current.GameTextManager.GetHotKeyGameText("CombatHotKeyCategory", 4));
                                InformationManager.AddQuickInformation(GameTexts.FindText("str_battle_won_press_tab_to_view_results", (string)null), 0, (BasicCharacterObject)null, "");
                            }
                        }
                        else
                        {
                            GameTexts.SetVariable("leave_key", Game.Current.GameTextManager.GetHotKeyGameText("CombatHotKeyCategory", 4));
                            InformationManager.AddQuickInformation(GameTexts.FindText("str_battle_finished_press_tab_to_view_results", (string)null), 0, (BasicCharacterObject)null, "");
                        }
                    }
                }
                else if ((double)this._checkRetreatingTimer.ElapsedTime > 3.0 && !this._scoreBoardOpenedOnceOnMissionEnd)
                {
                    if (this.Mission.MissionResult != null && this.Mission.MissionResult.PlayerDefeated)
                        InformationManager.AddQuickInformation(GameTexts.FindText("str_battle_lost", (string)null), 0, (BasicCharacterObject)null, "");
                    else if (this.Mission.MissionResult != null && this.Mission.MissionResult.PlayerVictory)
                    {
                        if (this._isEnemySideDepleted)
                            InformationManager.AddQuickInformation(GameTexts.FindText("str_battle_won", (string)null), 0, (BasicCharacterObject)null, "");
                        else if (this._isEnemySideRetreating)
                            InformationManager.AddQuickInformation(GameTexts.FindText("str_enemies_are_fleeing_you_won", (string)null), 0, (BasicCharacterObject)null, "");
                    }
                    else
                        InformationManager.AddQuickInformation(GameTexts.FindText("str_battle_finished", (string)null), 0, (BasicCharacterObject)null, "");
                    this._missionEndedMessageShown = true;
                    this._checkRetreatingTimer.Reset();
                }
                if (this._victoryReactionsActivated)
                    return;
                AgentVictoryLogic missionBehaviour = this.Mission.GetMissionBehaviour<AgentVictoryLogic>();
                if (missionBehaviour == null)
                    return;
                this.CheckIsEnemySideRetreatingOrOneSideDepleted();
                if (this._isEnemySideDepleted)
                {
                    missionBehaviour.SetTimersOfVictoryReactions(this.Mission.PlayerTeam.Side);
                    this._victoryReactionsActivated = true;
                }
                else if (this._isPlayerSideDepleted)
                {
                    missionBehaviour.SetTimersOfVictoryReactions(this.Mission.PlayerEnemyTeam.Side);
                    this._victoryReactionsActivated = true;
                }
                else
                {
                    if (!this._isEnemySideRetreating || this._victoryReactionsActivatedForRetreating)
                        return;
                    missionBehaviour.SetTimersOfVictoryReactionsForRetreating(this.Mission.PlayerTeam.Side);
                    this._victoryReactionsActivatedForRetreating = true;
                }
            }
            else
            {
                if ((double)this._checkRetreatingTimer.ElapsedTime <= 1.0)
                    return;
                this.CheckIsEnemySideRetreatingOrOneSideDepleted();
                this._checkRetreatingTimer.Reset();
            }
        }

        private void CheckIsEnemySideRetreatingOrOneSideDepleted()
        {
            if (!this._canCheckForEndCondition)
            {
                this._canCheckForEndCondition = this.Mission.GetMissionBehaviour<SiegeDeploymentHandler>() == null;
            }
            else
            {
                BattleSideEnum side = this.Mission.PlayerTeam.Side;
                this._isPlayerSideDepleted = this.Mission.GetMemberCountOfSide(side) == 0;
                this._isEnemySideDepleted = this.Mission.GetMemberCountOfSide(side.GetOppositeSide()) == 0;
                if (this._isEnemySideDepleted || this._isPlayerSideDepleted || this.Mission.GetMissionBehaviour<HideoutPhasedMissionController>() != null)
                    return;
                bool flag = true;
                foreach (Team team in (ReadOnlyCollection<Team>)this.Mission.Teams)
                {
                    if (team.IsEnemyOf(this.Mission.PlayerTeam))
                    {
                        foreach (Agent activeAgent in team.ActiveAgents)
                        {
                            if (!activeAgent.IsRunningAway)
                            {
                                flag = false;
                                break;
                            }
                        }
                    }
                }
                if (!flag)
                    this._enemiesNotYetRetreatingTime = MissionTime.Now;
                if ((double)this._enemiesNotYetRetreatingTime.ElapsedSeconds <= 3.0)
                    return;
                this._isEnemySideRetreating = true;
            }
        }

        public BattleEndLogic.ExitResult TryExit()
        {
            if (GameNetwork.IsClientOrReplay || this.Mission.MainAgent != null && this.Mission.MainAgent.IsActive() && this.Mission.IsPlayerCloseToAnEnemy(5f) || !this.Mission.MissionEnded() && (this.PlayerVictory || this.EnemyVictory))
                return BattleEndLogic.ExitResult.False;
            if (!this.Mission.MissionEnded() && !this._isEnemySideRetreating)
                return BattleEndLogic.ExitResult.NeedsPlayerConfirmation;
            this.Mission.EndMission();
            return BattleEndLogic.ExitResult.True;
        }

        public override void OnBehaviourInitialize()
        {
            base.OnBehaviourInitialize();
            this._checkRetreatingTimer = new BasicTimer(MBCommon.TimeType.Mission);
            this._shouldCelebrate = _config.ShouldCelebrateVictory;
        }

        protected override void OnEndMission()
        {
            if (!this._isEnemySideRetreating)
                return;
            foreach (Agent activeAgent in this.Mission.PlayerEnemyTeam.ActiveAgents)
                activeAgent.Origin?.SetRouted();
        }

        public bool IsEnemySideRetreating
        {
            get
            {
                return this._isEnemySideRetreating;
            }
        }

        public void SetNotificationDisabled(bool value)
        {
            this._notificationsDisabled = value;
        }

        public enum ExitResult
        {
            False,
            NeedsPlayerConfirmation,
            True,
        }
    }
}
