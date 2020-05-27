using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public static class Utility
    {
        public static List<MPPerkObject> GetAllSelectedPerks(MultiplayerClassDivisions.MPHeroClass mpHeroClass,
            int[] selectedPerks)
        {
            List<MPPerkObject> selectedPerkList = new List<MPPerkObject>();
            for (int i = 0; i < selectedPerks.Length; ++i)
            {
                var perks = mpHeroClass.GetAllAvailablePerksForListIndex(i);
                if (perks.IsEmpty())
                    continue;
                selectedPerkList.Add(perks[selectedPerks[i]]);
            }

            return selectedPerkList;
        }

        public static IEnumerable<PerkEffect> SelectRandomPerkEffectsForPerks(MultiplayerClassDivisions.MPHeroClass mpHeroClass, bool isPlayer, PerkType perkType, int[] selectedPerks)
        {
            var selectedPerkList = GetAllSelectedPerks(mpHeroClass, selectedPerks);
            return MPPerkObject.SelectRandomPerkEffectsForPerks(isPlayer, perkType, selectedPerkList);
        }

        public static Equipment GetNewEquipmentsForPerks(MPCharacterConfig config, bool isHero)
        {
            BasicCharacterObject character = isHero ? config.HeroClass.HeroCharacter : config.HeroClass.TroopCharacter;
            Equipment equipment = isHero ? character.Equipment.Clone() : Equipment.GetRandomEquipmentElements(character, true, false, MBRandom.RandomInt());
            foreach (PerkEffect perkEffectsForPerk in SelectRandomPerkEffectsForPerks(config.HeroClass, isHero,
                PerkType.PerkAlternativeEquipment, new[]
                {
                    config.SelectedFirstPerk, config.SelectedSecondPerk
                }))
                equipment[perkEffectsForPerk.NewItemIndex] = perkEffectsForPerk.NewItem.EquipmentElement;
            return equipment;
        }
    }
}
