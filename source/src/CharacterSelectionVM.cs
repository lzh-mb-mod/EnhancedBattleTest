using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class CharacterSelectionVM : ViewModel
    {
        CharacterSelectionParams _params;
        bool _inChange;

        private MBBindingList<NameVM> _cultures;
        private MBBindingList<NameVM> _groups;
        private MBBindingList<CharacterVM> _characters;
        private MBBindingList<PerkVM> _firstPerks;
        private MBBindingList<PerkVM> _secondPerks;

        public int SelectedCultureIndex { get; set; }

        public int SelectedGroupIndex { get; set; }

        public int SelectedCharacterIndex { get; set; }

        public int SelectedFirstPerkIndex { get; set; }

        public int SelectedSecondPerkIndex { get; set; }

        [DataSourceProperty]
        public MBBindingList<NameVM> Cultures
        {
            get => this._cultures;
            set
            {
                if (value == this._cultures)
                    return;
                this._cultures = value;
                this.OnPropertyChanged(nameof(Cultures));
            }
        }

        [DataSourceProperty]
        public MBBindingList<NameVM> Groups
        {
            get => this._groups;
            set
            {
                if (value == this._groups)
                    return;
                this._groups = value;
                this.OnPropertyChanged(nameof(Groups));
            }
        }
        [DataSourceProperty]
        public MBBindingList<CharacterVM> Characters
        {
            get => this._characters;
            set
            {
                if (value == this._characters)
                    return;
                this._characters = value;
                this.OnPropertyChanged(nameof(Characters));
            }
        }

        [DataSourceProperty]
        public MBBindingList<PerkVM> FirstPerks
        {
            get => this._firstPerks;
            set
            {
                if (value == this._firstPerks)
                    return;
                this._firstPerks = value;
                this.OnPropertyChanged(nameof(FirstPerks));
            }
        }
        [DataSourceProperty]
        public MBBindingList<PerkVM> SecondPerks
        {
            get => this._secondPerks;
            set
            {
                if (value == this._secondPerks)
                    return;
                this._secondPerks = value;
                this.OnPropertyChanged(nameof(SecondPerks));
            }
        }


        public CharacterSelectionVM(CharacterSelectionParams p)
            : base()
        {
            this._params = p;
            var selectedCharacter = p.selectedHeroClass;

            FillCultures();
            SelectedCultureIndex = Cultures.FindIndex(item => item.StringId == selectedCharacter.Culture.StringId);

            FillGroups();
            SelectedGroupIndex = Groups.FindIndex(item => item.StringId == selectedCharacter.ClassGroup.StringId);

            FillCharacters();
            SelectedCharacterIndex = Characters.FindIndex(item => item.character.StringId == selectedCharacter.StringId);

            FillFirstPerks();
            FillSecondPerks();
            this.SelectedFirstPerkIndex = FirstPerks.FindIndex(item => item.perkIndex == this._params.selectedFirstPerk);
            this.SelectedSecondPerkIndex = SecondPerks.FindIndex(item => item.perkIndex == this._params.selectedSecondPerk);
        }

        public void SelectedCultureChanged(ListPanel listPanel)
        {
            this._inChange = true;
            var index = listPanel.IntValue;

            SelectedCultureIndex = index;

            FillGroups();
            FillCharacters();
            FillFirstPerks();
            FillSecondPerks();

            this._inChange = false;
        }

        public void SelectedGroupChanged(ListPanel listPanel)
        {
            var index = listPanel.IntValue;
            if (index < 0 || this._inChange) return;

            this._inChange = true;
            SelectedGroupIndex = index;

            FillCharacters();
            FillFirstPerks();
            FillSecondPerks();

            this._inChange = false;
        }

        public void SelectedCharacterChanged(ListPanel listPanel)
        {
            var index = listPanel.IntValue;
            if (index < 0 || this._inChange) return;
            this._inChange = true;

            SelectedCharacterIndex = index;
            FillFirstPerks();
            FillSecondPerks();
            this._inChange = false;
        }

        public void SelectedFirstPerkChanged(ListPanel listPanel)
        {
            var index = listPanel.IntValue;
            if (index < 0 || this._inChange) return;
            this._inChange = true;
            SelectedFirstPerkIndex = index;
            this._inChange = false;
        }

        public void SelectedSecondPerkChanged(ListPanel listPanel)
        {
            var index = listPanel.IntValue;
            if (index < 0 || this._inChange) return;
            this._inChange = true;
            SelectedSecondPerkIndex = index;
            this._inChange = false;
        }


        public void Done()
        {
            _params.selectedHeroClass = Characters[this.SelectedCharacterIndex].character;
            _params.selectedFirstPerk = this.SelectedFirstPerkIndex;
            _params.selectedSecondPerk = this.SelectedSecondPerkIndex;
            this._params.selectAction(_params);
        }

        private void FillCultures()
        {
            Cultures = new MBBindingList<NameVM>();
            foreach (var culture in this._params.allMpHeroClassMap.Keys)
            {
                BasicCultureObject cultureObject = MBObjectManager.Instance.GetObject<BasicCultureObject>(culture);
                Cultures.Add(new NameVM { StringId = culture, Name = cultureObject.Name.ToString() });
            }

            SelectedCultureIndex = 0;
        }

        private void FillGroups()
        {
            var culture = Cultures[SelectedCultureIndex].StringId;
            if (Groups != null)
                Groups.Clear();
            else
                Groups = new MBBindingList<NameVM>();
            foreach (var groupId in this._params.allMpHeroClassMap[culture].Keys)
            {
                Groups.Add(new NameVM { StringId = groupId, Name = GameTexts.FindText("str_troop_type_name", groupId).ToString() });
            }

            SelectedGroupIndex = 0;
        }
        private void FillCharacters()
        {
            var list = this._params.allMpHeroClassMap[Cultures[SelectedCultureIndex].StringId][Groups[SelectedGroupIndex].StringId];
            if (Characters != null)
                Characters.Clear();
            else
                Characters = new MBBindingList<CharacterVM>();
            foreach (var character in list)
            {
                Characters.Add(new CharacterVM(character, this._params.isTroop));
            }

            SelectedCharacterIndex = 0;
        }

        private void FillFirstPerks()
        {
            var character = this.Characters[SelectedCharacterIndex].character;
            if (FirstPerks != null)
                FirstPerks.Clear();
            else
                FirstPerks = new MBBindingList<PerkVM>();
            int index = 0;
            foreach (var perk in character.GetAllAvailablePerksForListIndex(0))
            {
                FirstPerks.Add(new PerkVM { Name = perk.Name.ToString(), perkIndex = index++ });
            }

            SelectedFirstPerkIndex = 0;
        }

        private void FillSecondPerks()
        {
            var character = this.Characters[SelectedCharacterIndex].character;
            if (SecondPerks != null)
                SecondPerks.Clear();
            else
                SecondPerks = new MBBindingList<PerkVM>();
            int index = 0;
            foreach (var perk in character.GetAllAvailablePerksForListIndex(1))
            {
                SecondPerks.Add(new PerkVM { Name = perk.Name.ToString(), perkIndex = index++ });
            }
            
            SelectedSecondPerkIndex = 0;
        }
    }

    public class NameVM : ViewModel
    {
        public string StringId { get; set; }
        public string Name { get; set; }
    }

    public class CharacterVM : ViewModel
    {
        public MultiplayerClassDivisions.MPHeroClass character;
        public bool isTroop;
        public CharacterVM(MultiplayerClassDivisions.MPHeroClass character, bool isTroop)
        {
            this.character = character;
            this.isTroop = isTroop;
        }
        public string Name { get { return isTroop ? this.character.TroopName.ToString() : this.character.HeroName.ToString(); } }
    }

    public class PerkVM : ViewModel
    {
        public string Name { get; set; }
        public int perkIndex;
    }

    public class CharacterSelectionParams
    {
        public Dictionary<string, Dictionary<string, List<MultiplayerClassDivisions.MPHeroClass>>> allMpHeroClassMap;
        public bool isTroop;
        public MultiplayerClassDivisions.MPHeroClass selectedHeroClass;
        public int selectedFirstPerk;
        public int selectedSecondPerk;
        public Action<CharacterSelectionParams> selectAction;


        public static CharacterSelectionParams CharacterSelectionParamsFor(Dictionary<string, Dictionary<string, List<MultiplayerClassDivisions.MPHeroClass>>> allMpHeroClassMap, ClassInfo classInfo, bool isTroop, Action<CharacterSelectionParams> selectionAction)
        {
            return new CharacterSelectionParams
            {
                allMpHeroClassMap = allMpHeroClassMap,
                isTroop = isTroop,
                selectedHeroClass = MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>(classInfo.classStringId),
                selectedFirstPerk = classInfo.selectedFirstPerk,
                selectedSecondPerk = classInfo.selectedSecondPerk,
                selectAction = selectionAction,
            };
        }
    } 
}