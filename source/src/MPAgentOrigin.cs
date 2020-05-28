using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class MPAgentOrigin : IAgentOriginBase
    {
        private readonly UniqueTroopDescriptor _descriptor;
        private readonly MPTroopSupplier _troopSupplier;
        private bool _isRemoved;
        private readonly BattleSideEnum _side;

        public bool IsUnderPlayersCommand => _side == Mission.Current.PlayerTeam.Side;
        public uint FactionColor => BattleCombatant.BasicCulture.Color;
        public uint FactionColor2 => BattleCombatant.BasicCulture.Color2;
        public VirtualPlayer Peer => null;
        public IBattleCombatant BattleCombatant { get; }
        public int UniqueSeed => _descriptor.UniqueSeed;
        public int Seed => Troop.GetDefaultFaceSeed(Rank);

        public int Rank { get; private set; }
        public Banner Banner => BattleCombatant.Banner;
        public BasicCharacterObject Troop => MPCharacter.Character;

        public MPSpawnableCharacter MPCharacter { get; }

        public MPAgentOrigin(MPCombatant combatant, MPSpawnableCharacter character, MPTroopSupplier troopSupplier, BattleSideEnum side, int rank = -1, UniqueTroopDescriptor uniqueNo = default)
        {
            _troopSupplier = troopSupplier;
            _side = side;
            BattleCombatant = combatant;
            MPCharacter = character;
            _descriptor = !uniqueNo.IsValid ? new UniqueTroopDescriptor(Game.Current.NextUniqueTroopSeed) : uniqueNo; 
            Rank = rank == -1 ? MBRandom.RandomInt(10000) : rank;
        }

    public void SetWounded()
    {
      if (_isRemoved)
        return;
      _troopSupplier.OnTroopWounded();
      _isRemoved = true;
    }

        public void SetKilled()
        {
            if (_isRemoved)
                return;
            _troopSupplier.OnTroopKilled();
            _isRemoved = true;
        }

        public void SetRouted()
        {
            if (_isRemoved)
                return;
            _troopSupplier.OnTroopRouted();
            _isRemoved = true;
        }

        public void OnAgentRemoved(float agentHealth)
        {
        }

        public void OnScoreHit(BasicCharacterObject victim, int damage, bool isFatal, bool isTeamKill, int weaponKind,
            int currentWeaponUsageIndex)
        {
        }

        public void SetBanner(Banner banner)
        {
            throw new NotImplementedException();
        }
    }
}
