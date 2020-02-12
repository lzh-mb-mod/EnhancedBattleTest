using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TaleWorlds.Core;
using TaleWorlds.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Modbed
{
    public class CharacterSelectionVM : ViewModel
    {
        CharacterSelectionParams _params;
        bool _inChange;

        private MBBindingList<NameVM> _cultures;
        private MBBindingList<NameVM> _groups;
        private MBBindingList<CharacterVM> _characters;
        private MBBindingList<PerkVM> _perks;

        public int SelectedCultureIndex { get; set; }

        public int SelectedGroupIndex { get; set; }

        public int SelectedCharacterIndex { get; set; }

        public int SelectedPerkIndex { get; set; }

        [DataSourceProperty]
        public MBBindingList<NameVM> Cultures
        {
            get
            {
                return this._cultures;
            }
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
            get
            {
                return this._groups;
            }
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
            get
            {
                return this._characters;
            }
            set
            {
                if (value == this._characters)
                    return;
                this._characters = value;
                this.OnPropertyChanged(nameof(Characters));
            }
        }

        [DataSourceProperty]
        public MBBindingList<PerkVM> Perks
        {
            get => this._perks;
            set
            {
                if (value == this._perks)
                    return;
                this._perks = value;
                this.OnPropertyChanged(nameof(Perks));
            }
        }
        

        public CharacterSelectionVM(CharacterSelectionParams p)
            : base()
        {
            ModuleLogger.Log("begin character selection vm construction");
            this._params = p;
            var selectedCharacter = p.selectedHeroClass;

            FillCultures();
            SelectedCultureIndex = Cultures.FindIndex(item => item.StringId == selectedCharacter.Culture.StringId);

            FillGroups();
            SelectedGroupIndex = Groups.FindIndex(item => item.StringId == selectedCharacter.ClassGroup.StringId);

            FillCharacters();
            SelectedCharacterIndex = Characters.FindIndex(item => item.character.StringId == selectedCharacter.StringId);

            FillPerks();
            this.SelectedPerkIndex = Perks.FindIndex(item => item.perkIndex == this._params.selectedPerk);

            ModuleLogger.Log("end character selection vm construction");
        }

        public void SelectedCultureChanged(ListPanel listPanel)
        {
            this._inChange = true;
            var index = listPanel.IntValue;
            ModuleLogger.Log("SelectedCultureChanged {0}", index);

            SelectedCultureIndex = index;

            FillGroups();
            FillCharacters();
            FillPerks();

            this._inChange = false;
        }

        public void SelectedGroupChanged(ListPanel listPanel)
        {
            var index = listPanel.IntValue;
            if (index < 0 || this._inChange) return;
            ModuleLogger.Log("SelectedGroupChanged {0} {1}", index, Groups.Count);

            this._inChange = true;
            SelectedGroupIndex = index;

            FillCharacters();
            FillPerks();

            this._inChange = false;
        }

        public void SelectedCharacterChanged(ListPanel listPanel)
        {
            var index = listPanel.IntValue;
            if (index < 0 || this._inChange) return;
            ModuleLogger.Log("SelectedCharacterChanged {0}", index);
            this._inChange = true;

            SelectedCharacterIndex = index;
            FillPerks();
            this._inChange = false;
        }

        public void SelectedPerkChanged(ListPanel listPanel)
        {
            var index = listPanel.IntValue;
            if (index < 0 || this._inChange) return;
            ModuleLogger.Log("SelectedPerkChanged {0}", index);
            this._inChange = true;
            SelectedPerkIndex = index;
            this._inChange = false;
        }

        public void Done()
        {
            var character = Characters[this.SelectedCharacterIndex].character;
            this._params.selectAction(character, this.SelectedPerkIndex);
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
                Groups.Add(new NameVM { StringId = groupId, Name = groupId });
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

        private void FillPerks()
        {
            var character = this.Characters[SelectedCharacterIndex].character;
            if (Perks != null)
                Perks.Clear();
            else
                Perks = new MBBindingList<PerkVM>();
            int index = 0;
            foreach (var perk in character.GetAllAvailablePerksForListIndex(1))
            {
                Perks.Add(new PerkVM { Name = perk.Name.ToString(), perkIndex = index++ });
            }

            this.SelectedPerkIndex = 0;
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
        public int selectedPerk;
        public System.Action<MultiplayerClassDivisions.MPHeroClass, int> selectAction;
    } 
}