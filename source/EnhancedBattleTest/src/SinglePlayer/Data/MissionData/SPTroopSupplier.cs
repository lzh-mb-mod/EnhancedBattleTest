using EnhancedBattleTest.Data.MissionData;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Diamond.Ranked;

namespace EnhancedBattleTest.SinglePlayer.Data.MissionData
{
    public class SPTroopSupplier : IEnhancedBattleTestTroopSupplier
    {
        private readonly SPCombatant _combatant;
        private PriorityQueue<float, SPSpawnableCharacter> _characters;
        private List<SPAgentOrigin> _allAgentOrigins;
        private int _numAllocated;
        private int _numWounded;
        private int _numKilled;
        private int _numRouted;
        public int NumActiveTroops => _numAllocated - (_numWounded + _numKilled + _numRouted);

        public int NumRemovedTroops => _numWounded + _numKilled + _numRouted;

        public int NumTroopsNotSupplied => _characters.Count - _numAllocated;

        public bool AnyTroopRemainsToBeSupplied { get; private set; } = true;

        public SPTroopSupplier(IEnhancedBattleTestCombatant combatant)
        {
            _combatant = combatant as SPCombatant;
            ArrangePriorities();
            AllocateAllTroops();
        }

        private void ArrangePriorities()
        {
            _characters = new PriorityQueue<float, SPSpawnableCharacter>(new GenericComparer<float>());
            int[] originalNumbers = new int[8];
            for (int i = 0; i < 8; i++)
                originalNumbers[i] = _combatant.SPCharacters.Count(character => character.FormationIndex == (FormationClass)i);
            int[] queuedNumbers = new int[8];
            int num = 2048;
            foreach (var character in _combatant.SPCharacters)
            {
                int formationIndex = (int)character.FormationIndex;
                _characters.Enqueue(
                    character.IsHero || character.IsPlayer ? num-- : -(float) queuedNumbers[formationIndex] / originalNumbers[formationIndex],
                    character);
                ++queuedNumbers[formationIndex];
            }
        }

        private void AllocateAllTroops()
        {
            _allAgentOrigins = new List<SPAgentOrigin>();
            int rank = 0;
            while (!_characters.IsEmpty())
            {
                UniqueTroopDescriptor uniqueNo = new UniqueTroopDescriptor(TaleWorlds.Core.Game.Current.NextUniqueTroopSeed);
                var origin = new SPAgentOrigin(_combatant, _characters.DequeueValue(), this, _combatant.Side, rank, uniqueNo);
                ++rank;
                _allAgentOrigins.Add(origin);
            }
        }

        public IEnumerable<IAgentOriginBase> SupplyTroops(
            int numberToAllocate)
        {
            if (numberToAllocate <= 0 || _numAllocated >= _allAgentOrigins.Count)
            {
                AnyTroopRemainsToBeSupplied = false;
                return Enumerable.Empty<IAgentOriginBase>();
            }
            if (numberToAllocate >= _allAgentOrigins.Count - _numAllocated)
            {
                numberToAllocate = _allAgentOrigins.Count - _numAllocated;
                AnyTroopRemainsToBeSupplied = false;
            }
            var result = _allAgentOrigins.Skip(_numAllocated).Take(numberToAllocate);
            _numAllocated += numberToAllocate;
            return result;
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
            return _allAgentOrigins;
        }

        public BasicCharacterObject GetGeneralCharacter()
        {
            return _combatant.General;

        }

        public int GetNumberOfPlayerControllableTroops()
        {
            return _combatant.IsPlayerTeam ? _combatant.NumberOfHealthyMembers : 0;
        }
    }
}
