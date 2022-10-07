using EnhancedBattleTest.Data.MissionData;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace EnhancedBattleTest.Multiplayer.Data.MissionData
{
    public class MPTroopSupplier : IEnhancedBattleTestTroopSupplier
    {
        private readonly MPCombatant _combatant;
        private PriorityQueue<float, MPSpawnableCharacter> _characters;
        private int _numAllocated;
        private int _numWounded;
        private int _numKilled;
        private int _numRouted;
        public BasicCharacterObject GetGeneralCharacter()
        {
            throw new System.NotImplementedException();
        }

        public int GetNumberOfPlayerControllableTroops()
        {
            // TODO
            throw new System.NotImplementedException();
        }

        public int NumActiveTroops => _numAllocated - (_numWounded + _numKilled + _numRouted);

        public int NumRemovedTroops => _numWounded + _numKilled + _numRouted;

        public int NumTroopsNotSupplied => _characters.Count - _numAllocated;

        public bool AnyTroopRemainsToBeSupplied { get; private set; } = true;

        public MPTroopSupplier(IEnhancedBattleTestCombatant combatant)
        {
            _combatant = combatant as MPCombatant;
            ArrangePriorities();
        }
        private void ArrangePriorities()
        {
            _characters = new PriorityQueue<float, MPSpawnableCharacter>(new GenericComparer<float>());
            int[] originalNumbers = new int[8];
            for (int i = 0; i < 8; i++)
                originalNumbers[i] = _combatant.MPCharacters.Count(character => character.FormationIndex == (FormationClass)i);
            int[] queuedNumbers = new int[8];
            int num = 2048;
            foreach (var character in _combatant.MPCharacters)
            {
                int formationIndex = (int)character.FormationIndex;
                _characters.Enqueue(
                    character.IsHero ? num-- : -(float)queuedNumbers[formationIndex] / originalNumbers[formationIndex],
                    character);
                ++queuedNumbers[formationIndex];
            }
        }

        public IEnumerable<IAgentOriginBase> SupplyTroops(
            int numberToAllocate)
        {
            List<MPSpawnableCharacter> characterList = AllocateTroops(numberToAllocate);
            MPAgentOrigin[] battleAgentOriginArray = new MPAgentOrigin[characterList.Count];
            _numAllocated += characterList.Count;
            for (int rank = 0; rank < battleAgentOriginArray.Length; ++rank)
            {
                UniqueTroopDescriptor uniqueNo = new UniqueTroopDescriptor(TaleWorlds.Core.Game.Current.NextUniqueTroopSeed);
                battleAgentOriginArray[rank] = new MPAgentOrigin(_combatant,
                    characterList[rank], this,
                    _combatant.Side, rank, uniqueNo);
            }
            if (battleAgentOriginArray.Length < numberToAllocate)
                AnyTroopRemainsToBeSupplied = false;
            return battleAgentOriginArray;
        }

        private List<MPSpawnableCharacter> AllocateTroops(int numberToAllocate)
        {
            if (numberToAllocate > _characters.Count)
                numberToAllocate = _characters.Count;
            List<MPSpawnableCharacter> basicCharacterObjectList = new List<MPSpawnableCharacter>();
            for (int index = 0; index < numberToAllocate; ++index)
                basicCharacterObjectList.Add(_characters.DequeueValue());
            return basicCharacterObjectList;
        }
        public void OnTroopWounded()
        {
            ++_numWounded;
        }

        public void OnTroopKilled()
        {
            ++_numKilled;
        }

        public void OnTroopRouted()
        {
            ++_numRouted;
        }

        public IEnumerable<IAgentOriginBase> GetAllTroops()
        {
            throw new System.NotImplementedException();
        }
    }
}
