using System.Collections.Generic;
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
        private enum MemberState
        {
            Original,
            Leader,
            Prisoner
        }
        private readonly Dictionary<Hero, KeyValuePair<PartyBase, MemberState>> _originalParties = new Dictionary<Hero, KeyValuePair<PartyBase, MemberState>>();
        private readonly Dictionary<Hero, Settlement> _originalSettlements = new Dictionary<Hero, Settlement>();
        private readonly Dictionary<Hero, int> _originalHitPoints = new Dictionary<Hero, int>();

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

        public void RecoverHeroes()
        {
            foreach (var originalHitPoint in _originalHitPoints)
            {
                MobileParty.AddElementToMemberRoster(originalHitPoint.Key.CharacterObject, -1);
                originalHitPoint.Key.HitPoints = originalHitPoint.Value;
            }
            foreach (var pair in _originalParties)
            {
                if (pair.Value.Value == MemberState.Leader)
                {
                    pair.Value.Key.MobileParty.ChangePartyLeader(pair.Key);
                }
                /* switch (pair.Value.Value)
                {
                    case MemberState.Leader:
                        pair.Value.Key.AddElementToMemberRoster(pair.Key.CharacterObject, 0);
                        pair.Value.Key.MobileParty.ChangePartyLeader(pair.Key);
                        break;
                    case MemberState.Prisoner:
                        pair.Value.Key.AddPrisoner(pair.Key.CharacterObject, 0);
                        break;
                    case MemberState.Original:
                        pair.Value.Key.AddElementToMemberRoster(pair.Key.CharacterObject, 0);
                        break;
                }*/
            }

            foreach (var pair in _originalSettlements)
            {
                pair.Key.StayingInSettlement = pair.Value;
            }
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
                if (!_originalHitPoints.ContainsKey(character.HeroObject))
                {
                    _originalHitPoints[character.HeroObject] = character.HeroObject.HitPoints;
                    character.HeroObject.HitPoints = character.HeroObject.MaxHitPoints;
                }
                if (!_originalSettlements.ContainsKey(character.HeroObject) && !_originalParties.ContainsKey(character.HeroObject))
                {
                    if (character.HeroObject.StayingInSettlement != null)
                    {
                        _originalSettlements[character.HeroObject] = character.HeroObject.StayingInSettlement;
                    }
                    else if (character.HeroObject.PartyBelongedTo != null)
                    {
                        _originalParties[character.HeroObject] = new KeyValuePair<PartyBase, MemberState>(
                            character.HeroObject.PartyBelongedTo.Party,
                            character.HeroObject.PartyBelongedTo.LeaderHero == character.HeroObject ? MemberState.Leader: MemberState.Original);
                    }
                    else if (character.HeroObject.PartyBelongedToAsPrisoner != null)
                    {
                        _originalParties[character.HeroObject] = new KeyValuePair<PartyBase, MemberState>(
                            character.HeroObject.PartyBelongedToAsPrisoner, MemberState.Prisoner);
                    }
                }
            }
            MobileParty.AddElementToMemberRoster(character, number);
        }
    }
}
