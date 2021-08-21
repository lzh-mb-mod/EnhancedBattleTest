using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.Data.MissionData
{
    public abstract class EnhancedBattleTestAgentOrigin : IAgentOriginBase
    {
        private readonly UniqueTroopDescriptor _descriptor;
        private readonly IEnhancedBattleTestTroopSupplier _troopSupplier;
        private bool _isRemoved;
        private readonly BattleSideEnum _side;

        public bool IsUnderPlayersCommand => _side == Mission.Current.PlayerTeam.Side;
        public virtual uint FactionColor => BattleCombatant.BasicCulture.Color;
        public virtual uint FactionColor2 => BattleCombatant.BasicCulture.Color2;
        public VirtualPlayer Peer => null;
        public IBattleCombatant BattleCombatant { get; }
        public int UniqueSeed => _descriptor.UniqueSeed;
        public int Seed => Troop.GetDefaultFaceSeed(Rank);

        public int Rank { get; }
        public virtual Banner Banner => BattleCombatant.Banner;
        public abstract BasicCharacterObject Troop { get; }

        public abstract ISpawnableCharacter SpawnableCharacter { get; }

        public abstract FormationClass FormationIndex { get; }

        protected EnhancedBattleTestAgentOrigin(IBattleCombatant combatant, IEnhancedBattleTestTroopSupplier troopSupplier, BattleSideEnum side, int rank = -1, UniqueTroopDescriptor uniqueNo = default)
        {
            _troopSupplier = troopSupplier;
            _side = side;
            BattleCombatant = combatant;
            _descriptor = !uniqueNo.IsValid ? new UniqueTroopDescriptor(TaleWorlds.Core.Game.Current.NextUniqueTroopSeed) : uniqueNo;
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


        // for Bannerlord e1.5.5
        public void OnScoreHit(BasicCharacterObject victim, BasicCharacterObject formationCaptain,
            int damage, bool isFatal, bool isTeamKill,
            WeaponComponentData attackerWeapon)
        {
        }

        public void SetBanner(Banner banner)
        {
        }

        public abstract Agent SpawnTroop(
            BattleSideEnum side,
            bool hasFormation,
            bool spawnWithHorse,
            bool isReinforcement,
            bool enforceSpawningOnInitialPoint,
            int formationTroopCount,
            int formationTroopIndex,
            bool isAlarmed,
            bool wieldInitialWeapons,
            bool forceDismounted,
            Vec3? initialPosition,
            Vec2? initialDirection,
            string specialActionSet = null);
    }
}