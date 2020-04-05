using System;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class EnhancedFreeBattleAgentOrigin : IAgentOriginBase
    {
        private EnhancedTroopSupplier _troopSupplier;
        private bool _isRemoved;

        private readonly UniqueTroopDescriptor _descriptor;

        public CustomBattleCombatant CustomBattleCombatant { get; private set; }

        IBattleCombatant IAgentOriginBase.BattleCombatant => this.CustomBattleCombatant;

        public BasicCharacterObject Troop { get; private set; }

        public int Rank { get; private set; }

        public Banner Banner => this.CustomBattleCombatant.Banner;

        public bool IsUnderPlayersCommand { get; }

        public uint FactionColor => this.CustomBattleCombatant.BasicCulture.Color;

        public uint FactionColor2 => this.CustomBattleCombatant.BasicCulture.Color2;

        public int Seed => this.Troop.IsHero ? 0 : this.Troop.GetDefaultFaceSeed(this.Rank) % 2000;

        public int UniqueSeed => this.Troop.IsHero ? 0 : this._descriptor.UniqueSeed;

        public bool IsCoopTroop => false;

        public VirtualPlayer Peer => (VirtualPlayer)null;

        public EnhancedFreeBattleAgentOrigin(
          CustomBattleCombatant customBattleCombatant,
          EnhancedTroopSupplier troopSupplier,
          BasicCharacterObject characterObject,
          int rank = -1,
          UniqueTroopDescriptor uniqueNo = default(UniqueTroopDescriptor))
        {
            this.CustomBattleCombatant = customBattleCombatant;
            this._troopSupplier = troopSupplier;
            this.Troop = characterObject;
            this._descriptor = !uniqueNo.IsValid ? new UniqueTroopDescriptor(Game.Current.NextUniqueTroopSeed) : uniqueNo;
            this.Rank = rank == -1 ? MBRandom.RandomInt(10000) : rank;
            this.IsUnderPlayersCommand = Mission.Current.PlayerTeam.Side == customBattleCombatant.Side;
        }

        public void SetWounded()
        {
            if (this._isRemoved)
                return;
            this._troopSupplier?.OnTroopWounded();
            this._isRemoved = true;
        }

        public void SetKilled()
        {
            if (this._isRemoved)
                return;
            this._troopSupplier?.OnTroopKilled();
            this._isRemoved = true;
        }

        public void SetRouted()
        {
            if (this._isRemoved)
                return;
            this._troopSupplier?.OnTroopRouted();
            this._isRemoved = true;
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
