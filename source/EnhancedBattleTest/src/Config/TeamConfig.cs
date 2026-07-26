using System.Collections.Generic;

namespace EnhancedBattleTest.Config
{
    public class TeamConfig
    {
        public PartyConfig PrimaryParty { get; set; } = new PartyConfig();
        public List<PartyConfig> AlliedParties { get; set; } = new List<PartyConfig>();
        public CharacterConfig PlayerCharacter { get; set; }
        public bool OverrideTacticLevel;
        public int TacticLevel;

        public string BannerKey
        {
            get => GetPrimaryParty().BannerKey;
            set
            {
                PartyConfig party = GetPrimaryParty();
                party.BannerKey = value;
            }
        }

        public TroopGroupConfig Generals
        {
            get => GetPrimaryParty().Generals;
            set => GetPrimaryParty().Generals = value;
        }

        public bool HasGeneral
        {
            get => GetPrimaryParty().HasGeneral;
            set => GetPrimaryParty().HasGeneral = value;
        }

        public TroopGroupConfig[] TroopGroups { get; set; }

        public bool ShouldSerializeBannerKey() => false;
        public bool ShouldSerializeGenerals() => false;
        public bool ShouldSerializeHasGeneral() => false;
        public bool ShouldSerializeTroopGroups() => false;

        public void NormalizeAfterDeserialize()
        {
            PrimaryParty = PrimaryParty ?? new PartyConfig();
            AlliedParties = AlliedParties ?? new List<PartyConfig>();
            PrimaryParty.Normalize();
            foreach (PartyConfig party in AlliedParties)
                party?.Normalize();

            if (TroopGroups != null)
            {
                foreach (TroopGroupConfig group in TroopGroups)
                {
                    if (group?.Troops != null)
                        PrimaryParty.Troops.Troops.AddRange(group.Troops);
                }
                TroopGroups = null;
            }
        }

        private PartyConfig GetPrimaryParty()
        {
            return PrimaryParty ?? (PrimaryParty = new PartyConfig());
        }
    }
}
