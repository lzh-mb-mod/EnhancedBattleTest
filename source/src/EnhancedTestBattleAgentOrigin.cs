using System;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class EnhancedTestBattleAgentOrigin : IAgentOriginBase
    {
        private readonly UniqueTroopDescriptor _descriptor;

        public CustomBattleCombatant CustomBattleCombatant { get; private set; }

        IBattleCombatant IAgentOriginBase.BattleCombatant => this.CustomBattleCombatant;

        public BasicCharacterObject Troop { get; private set; }

        public int Rank { get; private set; }

        public Banner Banner => this.CustomBattleCombatant.Banner;

        public bool IsUnderPlayersCommand { get; }

        public uint FactionColor => this.CustomBattleCombatant.BasicCulture.Color;

        public uint FactionColor2 => this.CustomBattleCombatant.BasicCulture.Color2;

        public int Seed => this.Troop.GetDefaultFaceSeed(this.Rank);

        public int UniqueSeed => this._descriptor.UniqueSeed;

        public bool IsCoopTroop => false;

        public VirtualPlayer Peer => (VirtualPlayer)null;

        public EnhancedTestBattleAgentOrigin(
          CustomBattleCombatant customBattleCombatant,
          BasicCharacterObject characterObject,
          bool isPlayerSide,
          int rank = -1,
          UniqueTroopDescriptor uniqueNo = default(UniqueTroopDescriptor))
        {
            this.CustomBattleCombatant = customBattleCombatant;
            this.Troop = characterObject;
            this._descriptor = !uniqueNo.IsValid ? new UniqueTroopDescriptor(Game.Current.NextUniqueTroopSeed) : uniqueNo;
            this.Rank = rank == -1 ? MBRandom.RandomInt(10000) : rank;
            this.IsUnderPlayersCommand = isPlayerSide;
        }

        public void SetWounded()
        {
        }

        public void SetKilled()
        {
        }

        public void SetRouted()
        {
        }

        public void OnAgentRemoved(float agentHealth)
        {
        }

        void IAgentOriginBase.OnScoreHit(
          BasicCharacterObject victim,
          int damage,
          bool isFatal,
          bool isTeamKill,
          int weaponKind,
          int currentWeaponUsageIndex)
        {
        }

        public void SetBanner(Banner banner)
        {
        }
    }
}
