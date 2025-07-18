using System;
using System.Collections.Generic;
using System.Linq;
using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data.MissionData;
using EnhancedBattleTest.Multiplayer.Config;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace EnhancedBattleTest.Multiplayer.Data.MissionData
{
    public class MPCombatant : EnhancedBattleTestCombatant
    {
        private readonly List<MPSpawnableCharacter> _characters = new List<MPSpawnableCharacter>();
        private int _tacticLevel;
        public IEnumerable<MPSpawnableCharacter> MPCharacters => _characters.AsReadOnly();
        public override IEnumerable<BasicCharacterObject> Characters => MPCharacters.Select(character => character.Character);

        public override int NumberOfHealthyMembers => _characters.Count;

        public MPCombatant(BattleSideEnum side, int tacticLevel, BasicCultureObject culture,
            Tuple<uint, uint> primaryColorPair, Tuple<uint, uint> alternativeColorPair, Banner banner, bool isPlayerTeam)
            : base(GameTexts.FindText("str_ebt_side", side == BattleSideEnum.Attacker ? "Attacker" : "Defender"),
                side, culture, primaryColorPair, alternativeColorPair, banner, isPlayerTeam)
        {
            _tacticLevel = tacticLevel;
        }

        public MPCombatant(BattleSideEnum side, int tacticLevel, BasicCultureObject culture, Tuple<uint, uint> primaryColorPair, Banner banner, bool isPlayerTeam)
            : base(GameTexts.FindText("str_ebt_side", side == BattleSideEnum.Attacker ? "Attacker" : "Defender"),
                side, culture, primaryColorPair, new Tuple<uint, uint>(primaryColorPair.Item2, primaryColorPair.Item1), banner, isPlayerTeam)
        {
            _tacticLevel = tacticLevel;
        }

        public static MPCombatant CreateParty(BattleSideEnum side, BasicCultureObject culture,
            TeamConfig teamConfig, bool isPlayerTeam)
        {
            bool isAttacker = side == BattleSideEnum.Attacker;
            uint color1 = Utility.BackgroundColor(culture, isAttacker);
            uint color2 = Utility.ForegroundColor(culture, isAttacker);
            var combatant = new MPCombatant(side, teamConfig.TacticLevel, culture,
                new Tuple<uint, uint>(color1, color2), new Banner(culture.BannerKey, color1, color2), isPlayerTeam);
            if (teamConfig.HasGeneral)
            {
                bool hasPlayer = false;
                foreach (var generalTroop in teamConfig.Generals.Troops)
                {
                    if (generalTroop.Character is MPCharacterConfig general)
                    {
                        combatant.AddCharacter(
                            new MPSpawnableCharacter(general, (int)general.CharacterObject.DefaultFormationClass,
                                general.FemaleRatio > 0.5, !hasPlayer && isPlayerTeam), 1);
                        hasPlayer = isPlayerTeam;
                    }
                }
            }

            var queue = new PriorityQueue<float, MPSpawnableCharacter>(new GenericComparer<float>());

            for (int i = 0; i < teamConfig.TroopGroups.Length; i++)
            {
                var originalNumbers = new Dictionary<string, TroopCountPair>();
                var queuedNumbers = new Dictionary<string, TroopCountPair>();
                foreach (var troop in teamConfig.TroopGroups[i].Troops)
                {
                    if (troop.Character is MPCharacterConfig spCharacter)
                    {
                        var femaleCount = (int)(troop.Number * spCharacter.FemaleRatio + 0.49);
                        var maleCount = troop.Number - femaleCount;
                        if (originalNumbers.ContainsKey(spCharacter.CharacterId))
                        {
                            originalNumbers[spCharacter.CharacterId].FemaleCount += femaleCount;
                            originalNumbers[spCharacter.CharacterId].MaleCount += maleCount;
                        }
                        else
                        {
                            originalNumbers[spCharacter.CharacterId] = new TroopCountPair(femaleCount, maleCount);
                            queuedNumbers[spCharacter.CharacterId] = new TroopCountPair(0, 0);
                        }
                    }
                }
                foreach (var troop in teamConfig.TroopGroups[i].Troops)
                {
                    if (troop.Character is MPCharacterConfig spCharacter)
                    {
                        var femaleCount = (int)(troop.Number * spCharacter.FemaleRatio + 0.49);
                        var maleCount = troop.Number - femaleCount;
                        for (int j = 0; j < femaleCount; j++)
                        {
                            queue.Enqueue(
                                -(float)queuedNumbers[spCharacter.CharacterId].FemaleCount /
                                originalNumbers[spCharacter.CharacterId].FemaleCount,
                                new MPSpawnableCharacter(spCharacter, i, true));
                            ++queuedNumbers[spCharacter.CharacterId].FemaleCount;
                        }
                        for (int j = 0; j < maleCount; j++)
                        {
                            queue.Enqueue(
                                -(float)queuedNumbers[spCharacter.CharacterId].MaleCount /
                                originalNumbers[spCharacter.CharacterId].MaleCount,
                                new MPSpawnableCharacter(spCharacter, i, false));
                            ++queuedNumbers[spCharacter.CharacterId].MaleCount;
                        }
                    }
                }
            }

            while (!queue.IsEmpty)
            {
                combatant.AddCharacter(queue.DequeueValue(), 1);
            }

            return combatant;
        }

        public override int GetTacticsSkillAmount()
        {
            return _tacticLevel;
        }

        public void AddCharacter(MPSpawnableCharacter character, int number)
        {
            for (int index = 0; index < number; ++index)
                this._characters.Add(character);
            this.NumberOfAllMembers += number;
        }

        public void KillCharacter(MPSpawnableCharacter character)
        {
            this._characters.Remove(character);
        }
    }
}
