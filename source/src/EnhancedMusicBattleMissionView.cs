using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using psai.net;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;

namespace EnhancedBattleTest
{
    class EnhancedMusicBattleMissionView : MissionView, IMusicHandler
    {
        private EnhancedFreeBattleMissionController _controller;
        private EnhancedMusicBattleMissionView.BattleState _battleState;
        private int[] _startingTroopCounts;
        private float _startingBattleRatio;
        private bool _isSiegeBattle;
        private bool _isPaganBattle;

        bool IMusicHandler.IsPausable
        {
            get
            {
                return false;
            }
        }

        private BattleSideEnum PlayerSide
        {
            get
            {
                Team playerTeam = Mission.Current.PlayerTeam;
                return playerTeam == null ? BattleSideEnum.None : playerTeam.Side;
            }
        }

        public EnhancedMusicBattleMissionView(bool isSiegeBattle)
        {
            this._isSiegeBattle = isSiegeBattle;
        }

        public override void OnBehaviourInitialize()
        {
            base.OnBehaviourInitialize();
            _controller = Mission.GetMissionBehaviour<EnhancedFreeBattleMissionController>();
            MBMusicManager.Current.DeactivateCurrentMode();
            MBMusicManager.Current.ActivateBattleMode();
            MBMusicManager.Current.OnBattleMusicHandlerInit((IMusicHandler)this);
        }

        public override void OnMissionScreenFinalize()
        {
            MBMusicManager.Current.DeactivateBattleMode();
            MBMusicManager.Current.OnBattleMusicHandlerFinalize();
        }

        private void CheckIntensityFall()
        {
            PsaiInfo psaiInfo = PsaiCore.Instance.GetPsaiInfo();
            if (psaiInfo.effectiveThemeId < 0)
                return;
            if (float.IsNaN(psaiInfo.currentIntensity))
            {
                MBMusicManager.Current.ChangeCurrentThemeIntensity(MusicParameters.MinIntensity);
            }
            else
            {
                if ((double)psaiInfo.currentIntensity >= (double)MusicParameters.MinIntensity)
                    return;
                MBMusicManager.Current.ChangeCurrentThemeIntensity(MusicParameters.MinIntensity - psaiInfo.currentIntensity);
            }
        }

        public override void OnAgentRemoved(
          Agent affectedAgent,
          Agent affectorAgent,
          AgentState agentState,
          KillingBlow blow)
        {
            if (this._battleState == EnhancedMusicBattleMissionView.BattleState.Starting)
                return;
            bool flag1 = affectedAgent.IsMine || affectedAgent.RiderAgent != null && affectedAgent.RiderAgent.IsMine;
            Team team = affectedAgent.Team;
            // ISSUE: explicit non-virtual call
            BattleSideEnum battleSideEnum = team != null ? team.Side : BattleSideEnum.None;
            int num1;
            if (!flag1)
            {
                if (battleSideEnum != BattleSideEnum.None)
                {
                    Team playerTeam = Mission.Current.PlayerTeam;
                    // ISSUE: explicit non-virtual call
                    num1 = (playerTeam != null ? playerTeam.Side : BattleSideEnum.None) == battleSideEnum ? 1 : 0;
                }
                else
                    num1 = 0;
            }
            else
                num1 = 1;
            bool flag2 = num1 != 0;
            if (!this._isSiegeBattle && affectedAgent.IsHuman && (battleSideEnum != BattleSideEnum.None && this._battleState == EnhancedMusicBattleMissionView.BattleState.Started) && ((IEnumerable<int>)this._startingTroopCounts).Sum() >= MusicParameters.SmallBattleTreshold && (MissionTime.Now.ToSeconds > (double)MusicParameters.BattleTurnsOneSideCooldown))
            {
                int[] numArray = new int[2]
                {
                    this._controller.NumberOfActiveDefenderTroops,
                    this._controller.NumberOfActiveAttackerTroops
                };
                --numArray[(int)battleSideEnum];
                float num2 = (float)numArray[0] / (float)numArray[1];
                MusicTheme theme = MusicTheme.None;
                if ((double)num2 < (double)this._startingBattleRatio * (double)MusicParameters.BattleRatioTresholdOnIntensity)
                    theme = MBMusicManager.Current.GetBattleTurnsOneSideTheme(this.Mission.MusicCulture.GetCultureCode(), (uint)this.PlayerSide > 0U, this._isPaganBattle);
                else if ((double)num2 > (double)this._startingBattleRatio / (double)MusicParameters.BattleRatioTresholdOnIntensity)
                    theme = MBMusicManager.Current.GetBattleTurnsOneSideTheme(this.Mission.MusicCulture.GetCultureCode(), this.PlayerSide == BattleSideEnum.Defender, this._isPaganBattle);
                if (theme != MusicTheme.None)
                {
                    MBMusicManager.Current.StartTheme(theme, PsaiCore.Instance.GetCurrentIntensity(), false);
                    this._battleState = EnhancedMusicBattleMissionView.BattleState.TurnedOneSide;
                }
            }
            if (((!affectedAgent.IsHuman ? 0 : (affectedAgent.State != AgentState.Routed ? 1 : 0)) | (flag1 ? 1 : 0)) == 0)
                return;
            float deltaIntensity = flag2 ? MusicParameters.FriendlyTroopDeadEffectOnIntensity : MusicParameters.EnemyTroopDeadEffectOnIntensity;
            if (flag1)
                deltaIntensity *= MusicParameters.PlayerTroopDeadEffectMultiplierOnIntensity;
            MBMusicManager.Current.ChangeCurrentThemeIntensity(deltaIntensity);
        }

        private void CheckForStarting()
        {
            if (this._startingTroopCounts == null)
            {
                this._startingTroopCounts = new int[2]
                {
                    this._controller.NumberOfActiveDefenderTroops,
                    this._controller.NumberOfActiveAttackerTroops,
                };
                this._startingBattleRatio = (float)this._startingTroopCounts[0] / (float)this._startingTroopCounts[1];
            }
            Agent main = Agent.Main;
            Vec2 vec2 = main != null ? main.Position.AsVec2 : Vec2.Invalid;
            Team playerTeam = Mission.Current.PlayerTeam;
            bool flag1 = playerTeam != null && playerTeam.Formations.Any<Formation>();
            float num1 = float.MaxValue;
            if (flag1 || vec2.IsValid)
            {
                foreach (Formation formation1 in Mission.Current.PlayerEnemyTeam.Formations)
                {
                    float num2 = float.MaxValue;
                    if (!flag1 && vec2.IsValid)
                        num2 = vec2.DistanceSquared(formation1.CurrentPosition);
                    else if (flag1)
                    {
                        foreach (Formation formation2 in Mission.Current.PlayerTeam.Formations)
                        {
                            float num3 = formation2.CurrentPosition.DistanceSquared(formation1.CurrentPosition);
                            if ((double)num2 > (double)num3)
                                num2 = num3;
                        }
                    }
                    if ((double)num1 > (double)num2)
                        num1 = num2;
                }
            }
            int battleSize = ((IEnumerable<int>)this._startingTroopCounts).Sum();
            bool flag2 = false;
            if (battleSize < MusicParameters.SmallBattleTreshold)
            {
                if ((double)num1 < (double)MusicParameters.SmallBattleDistanceTreshold * (double)MusicParameters.SmallBattleDistanceTreshold)
                    flag2 = true;
            }
            else if (battleSize < MusicParameters.MediumBattleTreshold)
            {
                if ((double)num1 < (double)MusicParameters.MediumBattleDistanceTreshold * (double)MusicParameters.MediumBattleDistanceTreshold)
                    flag2 = true;
            }
            else if (battleSize < MusicParameters.LargeBattleTreshold)
            {
                if ((double)num1 < (double)MusicParameters.LargeBattleDistanceTreshold * (double)MusicParameters.LargeBattleDistanceTreshold)
                    flag2 = true;
            }
            else if ((double)num1 < (double)MusicParameters.MaxBattleDistanceTreshold * (double)MusicParameters.MaxBattleDistanceTreshold)
                flag2 = true;
            if (!flag2)
                return;
            float startIntensity = (float)((double)MusicParameters.DefaultStartIntensity + (double)((float)battleSize / 1000f) * (double)MusicParameters.BattleSizeEffectOnStartIntensity + ((double)MBRandom.RandomFloat - 0.5) * ((double)MusicParameters.RandomEffectMultiplierOnStartIntensity * 2.0));
            MBMusicManager.Current.StartTheme(this._isSiegeBattle ? MBMusicManager.Current.GetSiegeTheme(this.Mission.MusicCulture.GetCultureCode()) : MBMusicManager.Current.GetBattleTheme(this.Mission.MusicCulture.GetCultureCode(), battleSize, out this._isPaganBattle), startIntensity, false);
            this._battleState = EnhancedMusicBattleMissionView.BattleState.Started;
        }

        private void CheckForEnding()
        {
            if (!Mission.Current.IsMissionEnding)
                return;
            if (Mission.Current.MissionResult != null)
            {
                MBMusicManager.Current.StartTheme(MBMusicManager.Current.GetBattleEndTheme(this.Mission.MusicCulture.GetCultureCode(), Mission.Current.MissionResult.PlayerVictory), PsaiCore.Instance.GetPsaiInfo().currentIntensity, true);
                this._battleState = EnhancedMusicBattleMissionView.BattleState.Ending;
            }
            else
            {
                MBMusicManager.Current.StartTheme(MusicTheme.BattleDefeat, PsaiCore.Instance.GetPsaiInfo().currentIntensity, true);
                this._battleState = EnhancedMusicBattleMissionView.BattleState.Ending;
            }
        }

        void IMusicHandler.OnUpdated(float dt)
        {
            if (this._battleState == EnhancedMusicBattleMissionView.BattleState.Starting)
            {
                if (this.Mission.MusicCulture == null && Mission.Current.GetMissionBehaviour<DeploymentHandler>() == null && this._controller.spawned)
                {
                    KeyValuePair<BasicCultureObject, int> keyValuePair = new KeyValuePair<BasicCultureObject, int>((BasicCultureObject)null, -1);
                    Dictionary<BasicCultureObject, int> dictionary = new Dictionary<BasicCultureObject, int>();
                    foreach (Team team in (ReadOnlyCollection<Team>)this.Mission.Teams)
                    {
                        foreach (Agent activeAgent in team.ActiveAgents)
                        {
                            if (activeAgent.Character.Culture.IsMainCulture)
                            {
                                if (!dictionary.ContainsKey(activeAgent.Character.Culture))
                                    dictionary.Add(activeAgent.Character.Culture, 0);
                                dictionary[activeAgent.Character.Culture]++;
                                if (dictionary[activeAgent.Character.Culture] > keyValuePair.Value)
                                    keyValuePair = new KeyValuePair<BasicCultureObject, int>(activeAgent.Character.Culture, dictionary[activeAgent.Character.Culture]);
                            }
                        }
                    }
                    if (keyValuePair.Key != null)
                        this.Mission.MusicCulture = keyValuePair.Key;
                    else
                        this.Mission.MusicCulture = Game.Current.PlayerTroop.Culture;
                }
                if (this.Mission.MusicCulture != null)
                    this.CheckForStarting();
            }
            if (this._battleState == EnhancedMusicBattleMissionView.BattleState.Started || this._battleState == EnhancedMusicBattleMissionView.BattleState.TurnedOneSide)
                this.CheckForEnding();
            this.CheckIntensityFall();
        }

        private enum BattleState
        {
            Starting,
            Started,
            TurnedOneSide,
            Ending,
        }
    }
}
