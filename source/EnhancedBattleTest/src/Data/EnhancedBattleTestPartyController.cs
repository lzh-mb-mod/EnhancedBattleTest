using EnhancedBattleTest.Config;
using EnhancedBattleTest.SinglePlayer.Config;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.TroopSuppliers;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace EnhancedBattleTest.Data
{
    public static class EnhancedBattleTestPartyController
    {
        public sealed class BattleContext
        {
            public MobileParty PlayerParty { get; }
            public MobileParty EnemyParty { get; }
            public MapEvent MapEvent { get; }
            public PartyGroupTroopSupplier[] TroopSuppliers { get; }
            public MapEventSide OriginalMainPartyMapEventSide { get; }
            public CharacterObject PlayerCharacter { get; }
            public IReadOnlyList<string> PlayerPriorityCharacterIds { get; }
            public IReadOnlyList<CharacterObject> TemporaryCharacters { get; }

            public BattleContext(
                MobileParty playerParty,
                MobileParty enemyParty,
                MapEvent mapEvent,
                PartyGroupTroopSupplier[] troopSuppliers,
                MapEventSide originalMainPartyMapEventSide,
                CharacterObject playerCharacter,
                IReadOnlyList<string> playerPriorityCharacterIds,
                IReadOnlyList<CharacterObject> temporaryCharacters)
            {
                PlayerParty = playerParty;
                EnemyParty = enemyParty;
                MapEvent = mapEvent;
                TroopSuppliers = troopSuppliers;
                OriginalMainPartyMapEventSide = originalMainPartyMapEventSide;
                PlayerCharacter = playerCharacter;
                PlayerPriorityCharacterIds = playerPriorityCharacterIds;
                TemporaryCharacters = temporaryCharacters;
            }
        }

        public static BattleContext Current { get; private set; }
        private static bool _isCleaningUp;

        public static BattleContext Create(BattleConfig config)
        {
            Cleanup();

            MobileParty playerParty = null;
            MobileParty enemyParty = null;
            MapEvent mapEvent = null;
            MapEventSide originalMainPartyMapEventSide =
                PartyBase.MainParty.MapEventSide;
            CharacterObject selectedPlayerCharacter =
                GetPlayerCharacter(config.PlayerTeamConfig);
            var temporaryCharacters = new List<CharacterObject>();
            try
            {
                playerParty = CreateParty(
                    new TextObject("{=sSJSTe5p}Player Party"),
                    config.PlayerTeamConfig,
                    true,
                    selectedPlayerCharacter,
                    temporaryCharacters,
                    out CharacterObject playerCharacter,
                    out List<string> playerPriorityCharacterIds);
                enemyParty = CreateParty(
                    new TextObject("{=0xC75dN6}Enemy Party"),
                    config.EnemyTeamConfig,
                    false,
                    null,
                    temporaryCharacters,
                    out _,
                    out _);

                BattleSideEnum playerSide = config.BattleTypeConfig.PlayerSide;
                PartyBase attacker = playerSide == BattleSideEnum.Attacker
                    ? playerParty.Party
                    : enemyParty.Party;
                PartyBase defender = playerSide == BattleSideEnum.Defender
                    ? playerParty.Party
                    : enemyParty.Party;

                mapEvent = FieldBattleEventComponent
                    .CreateFieldBattleEvent(attacker, defender)
                    .MapEvent;
                var suppliers = new PartyGroupTroopSupplier[2];
                var playerPriorityTroops = new FlattenedTroopRoster();
                if (playerCharacter != null)
                    playerPriorityTroops.Add(playerCharacter, 1);
                suppliers[(int)BattleSideEnum.Defender] =
                    new PartyGroupTroopSupplier(
                        mapEvent,
                        BattleSideEnum.Defender,
                        playerSide == BattleSideEnum.Defender ? playerPriorityTroops : null);
                suppliers[(int)BattleSideEnum.Attacker] =
                    new PartyGroupTroopSupplier(
                        mapEvent,
                        BattleSideEnum.Attacker,
                        playerSide == BattleSideEnum.Attacker ? playerPriorityTroops : null);

                PartyBase.MainParty.MapEventSide =
                    mapEvent.GetMapEventSide(playerSide);
                Current = new BattleContext(
                    playerParty,
                    enemyParty,
                    mapEvent,
                    suppliers,
                    originalMainPartyMapEventSide,
                    playerCharacter,
                    playerPriorityCharacterIds,
                    temporaryCharacters);
                return Current;
            }
            catch
            {
                RestoreMainPartyMapEventSide(
                    mapEvent,
                    originalMainPartyMapEventSide);
                PrepareMapEventForCleanup(mapEvent);
                mapEvent?.FinalizeEvent();
                DestroyParty(playerParty);
                DestroyParty(enemyParty);
                UnregisterCharacters(temporaryCharacters);
                throw;
            }
        }

        public static bool IsTestParty(PartyBase party)
        {
            return party != null
                   && Current != null
                   && (party == Current.PlayerParty.Party || party == Current.EnemyParty.Party);
        }

        public static void Cleanup()
        {
            if (_isCleaningUp)
                return;

            BattleContext context = Current;
            if (context == null)
                return;

            _isCleaningUp = true;
            try
            {
                RestoreMainPartyMapEventSide(
                    context.MapEvent,
                    context.OriginalMainPartyMapEventSide);
                PrepareMapEventForCleanup(context.MapEvent);
                context.MapEvent?.FinalizeEvent();
                DestroyParty(context.PlayerParty);
                DestroyParty(context.EnemyParty);
                UnregisterCharacters(context.TemporaryCharacters);
            }
            finally
            {
                Current = null;
                _isCleaningUp = false;
            }
        }

        private static MobileParty CreateParty(
            TextObject name,
            TeamConfig config,
            bool isPlayerParty,
            CharacterObject controlledCharacter,
            List<CharacterObject> temporaryCharacters,
            out CharacterObject primaryGeneral,
            out List<string> priorityCharacterIds)
        {
            BasicCultureObject culture = Utility.GetCulture(config);
            Hero owner = isPlayerParty ? Hero.MainHero : GetOwner(config, culture);
            Clan clan = owner?.Clan
                        ?? Clan.All.FirstOrDefault(candidate => candidate.Culture == culture)
                        ?? Clan.PlayerClan;
            TroopRoster roster = CreateRoster(
                config,
                controlledCharacter,
                temporaryCharacters,
                out primaryGeneral,
                out priorityCharacterIds);
            MobileParty party = CustomPartyComponent.CreateQuestParty(
                MobileParty.MainParty.Position2D,
                0f,
                null,
                name,
                clan,
                roster,
                TroopRoster.CreateDummyTroopRoster(),
                owner,
                avoidHostileActions: true);

            party.IsVisible = false;
            party.Ai.SetMoveModeHold();
            return party;
        }

        private static TroopRoster CreateRoster(
            TeamConfig config,
            CharacterObject controlledCharacter,
            List<CharacterObject> temporaryCharacters,
            out CharacterObject primaryGeneral,
            out List<string> priorityCharacterIds)
        {
            TroopRoster roster = TroopRoster.CreateDummyTroopRoster();
            var heroCopies = new Dictionary<CharacterObject, CharacterObject>();
            primaryGeneral = null;
            priorityCharacterIds = new List<string>();
            if (config.HasGeneral)
            {
                foreach (TroopConfig troop in config.Generals.Troops)
                {
                    if (troop.Character.CharacterObject is CharacterObject character)
                    {
                        bool isControlledCharacter =
                            primaryGeneral == null && character == controlledCharacter;
                        CharacterObject rosterCharacter = isControlledCharacter
                            ? CreateTemporaryCharacter(character, temporaryCharacters)
                            : GetRosterCharacter(
                                character,
                                heroCopies,
                                temporaryCharacters);
                        roster.AddToCounts(rosterCharacter, 1);
                        if (isControlledCharacter)
                            primaryGeneral = rosterCharacter;
                        priorityCharacterIds.Add(rosterCharacter.StringId);
                    }
                }
            }

            foreach (TroopGroupConfig group in config.TroopGroups)
            {
                foreach (TroopConfig troop in group.Troops)
                {
                    if (troop.Number > 0
                        && troop.Character.CharacterObject is CharacterObject character)
                    {
                        CharacterObject rosterCharacter = GetRosterCharacter(
                            character,
                            heroCopies,
                            temporaryCharacters);
                        roster.AddToCounts(rosterCharacter, troop.Number);
                        if (character.IsHero)
                            priorityCharacterIds.Add(rosterCharacter.StringId);
                    }
                }
            }

            return roster;
        }

        private static CharacterObject GetPlayerCharacter(TeamConfig config)
        {
            if (!config.HasGeneral)
                return null;

            return config.Generals.Troops
                .Select(troop => troop.Character.CharacterObject as CharacterObject)
                .FirstOrDefault(character => character != null);
        }

        private static CharacterObject GetRosterCharacter(
            CharacterObject character,
            Dictionary<CharacterObject, CharacterObject> heroCopies,
            List<CharacterObject> temporaryCharacters)
        {
            if (!character.IsHero)
                return character;
            if (heroCopies.TryGetValue(character, out CharacterObject copy))
                return copy;

            copy = CreateTemporaryCharacter(character, temporaryCharacters);
            heroCopies.Add(character, copy);
            return copy;
        }

        private static CharacterObject CreateTemporaryCharacter(
            CharacterObject character,
            List<CharacterObject> temporaryCharacters)
        {
            CharacterObject copy = CharacterObject.CreateFrom(character);
            temporaryCharacters.Add(copy);
            return copy;
        }

        private static Hero GetOwner(TeamConfig config, BasicCultureObject culture)
        {
            Hero selectedHero = config.HasGeneral
                ? config.Generals.Troops
                    .Select(troop => (troop.Character as SPCharacterConfig)?.ActualCharacterObject?.HeroObject)
                    .FirstOrDefault(hero => hero != null)
                : null;
            return selectedHero
                   ?? Hero.AllAliveHeroes.FirstOrDefault(hero => hero.Culture == culture)
                   ?? Hero.MainHero;
        }

        private static void DestroyParty(MobileParty party)
        {
            if (party?.IsActive == true)
                DestroyPartyAction.Apply(null, party);
        }

        private static void PrepareMapEventForCleanup(MapEvent mapEvent)
        {
            if (mapEvent == null)
                return;

            mapEvent.ResetBattleState();
            mapEvent.AttackerSide.Casualties = 0;
            mapEvent.DefenderSide.Casualties = 0;
        }

        private static void RestoreMainPartyMapEventSide(
            MapEvent mapEvent,
            MapEventSide originalMapEventSide)
        {
            if (PartyBase.MainParty?.MapEvent == mapEvent)
                PartyBase.MainParty.MapEventSide = originalMapEventSide;
        }

        private static void UnregisterCharacters(IEnumerable<CharacterObject> characters)
        {
            foreach (CharacterObject character in characters)
                MBObjectManager.Instance.UnregisterObject(character);
        }
    }
}
