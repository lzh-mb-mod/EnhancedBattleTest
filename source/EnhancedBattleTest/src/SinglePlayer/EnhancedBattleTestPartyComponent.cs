using System.Collections.Generic;
using System.Linq;
using EnhancedBattleTest.Config;
using EnhancedBattleTest.SinglePlayer.Config;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.SinglePlayer
{
    public class EnhancedBattleTestPartyComponent : PartyComponent
    {

        public override Hero PartyOwner { get; }
        public override TextObject Name { get; }
        public override Settlement HomeSettlement { get; }

        public override Hero Leader { get; }

        private readonly TeamConfig _config;

        public EnhancedBattleTestPartyComponent(TextObject name, TeamConfig config)
        {
            PartyOwner = null;
            Name = name;
            HomeSettlement = null;
            if (config.HasGeneral && config.Generals.Troops.Count > 0)
            {
                Leader = (config.Generals.Troops[0].Character as SPCharacterConfig).ActualCharacterObject.HeroObject;
            }

            _config = config;
        }

        public void RemoveHeroes()
        {
            var heroes = MobileParty.MemberRoster.GetTroopRoster().Where(r => r.Character.IsHero).ToList();
            foreach (var hero in heroes)
            {
                MobileParty.AddElementToMemberRoster(hero.Character, -1);
            }
        }

        public int? GetTacticLevel()
        {
            return _config.CustomTacticLevel ? _config.TacticLevel : (int?)null;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (_config.HasGeneral)
            {
                foreach (var generalsTroop in _config.Generals.Troops)
                {
                    var spTroopConfig = generalsTroop.Character as SPCharacterConfig;
                    AddMember(spTroopConfig.ActualCharacterObject, 1);
                }
            }
            foreach (var troopConfig in _config.Troops.Troops)
            {
                var spTroopConfig = troopConfig.Character as SPCharacterConfig;
                AddMember(spTroopConfig.ActualCharacterObject, troopConfig.Number);
            }
        }

        private void AddMember(CharacterObject character, int number)
        {
            if (character.IsHero)
            {
                BattleStarter.RegisterHero(character.HeroObject);
                if (character.HeroObject.PartyBelongedTo != MobileParty)
                {
                    MobileParty.AddElementToMemberRoster(character, 1);
                }
            }
            else
            {
                MobileParty.AddElementToMemberRoster(character, number);
            }
        }
    }
}
