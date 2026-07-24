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
            public IReadOnlyList<CharacterObject> PlayerSpawnPriorityCharacters { get; }
            public IReadOnlyList<string> PlayerPriorityCharacterIds { get; }
            public IReadOnlyDictionary<Hero, int> OriginalHeroHitPoints { get; }

            public BattleContext(
                MobileParty playerParty,
                MobileParty enemyParty,
                MapEvent mapEvent,
                PartyGroupTroopSupplier[] troopSuppliers,
                MapEventSide originalMainPartyMapEventSide,
                CharacterObject playerCharacter,
                IReadOnlyList<CharacterObject> playerSpawnPriorityCharacters,
                IReadOnlyList<string> playerPriorityCharacterIds,
                IReadOnlyDictionary<Hero, int> originalHeroHitPoints)
            {
                PlayerParty = playerParty;
                EnemyParty = enemyParty;
                MapEvent = mapEvent;
                TroopSuppliers = troopSuppliers;
                OriginalMainPartyMapEventSide = originalMainPartyMapEventSide;
                PlayerCharacter = playerCharacter;
                PlayerSpawnPriorityCharacters = playerSpawnPriorityCharacters;
                PlayerPriorityCharacterIds = playerPriorityCharacterIds;
                OriginalHeroHitPoints = originalHeroHitPoints;
            }
        }

        public static BattleContext Current { get; private set; }
        private static bool _isCleaningUp;
        private static bool _isCreatingTemporaryParty;
        private static readonly HashSet<MobileParty> TemporaryParties =
            new HashSet<MobileParty>();

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
            Dictionary<Hero, int> originalHeroHitPoints = null;
            try
            {
                playerParty = CreateParty(
                    new TextObject("{=sSJSTe5p}Player Party"),
                    config.PlayerTeamConfig,
                    true,
                    selectedPlayerCharacter,
                    out CharacterObject playerCharacter,
                    out List<CharacterObject> playerSpawnPriorityCharacters,
                    out List<string> playerPriorityCharacterIds);
                enemyParty = CreateParty(
                    new TextObject("{=0xC75dN6}Enemy Party"),
                    config.EnemyTeamConfig,
                    false,
                    null,
                    out _,
                    out _,
                    out _);
                originalHeroHitPoints = CaptureHeroHitPoints(
                    playerParty,
                    enemyParty);

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
                foreach (CharacterObject character in playerSpawnPriorityCharacters)
                    playerPriorityTroops.Add(character, 1);
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
                    playerSpawnPriorityCharacters,
                    playerPriorityCharacterIds,
                    originalHeroHitPoints);
                EnhancedBattleTestSaveGuard.Disable();
                return Current;
            }
            catch
            {
                try
                {
                    RestoreMainPartyMapEventSide(
                        mapEvent,
                        originalMainPartyMapEventSide);
                    PrepareMapEventForCleanup(mapEvent);
                    mapEvent?.FinalizeEvent();
                    DestroyParty(playerParty);
                    DestroyParty(enemyParty);
                }
                finally
                {
                    RestoreHeroHitPoints(originalHeroHitPoints);
                }
                throw;
            }
        }

        public static bool IsTestParty(PartyBase party)
        {
            return party != null
                   && Current != null
                   && (party == Current.PlayerParty.Party || party == Current.EnemyParty.Party);
        }

        public static bool IsParticipatingHero(Hero hero)
        {
            return hero != null
                   && Current?.OriginalHeroHitPoints.ContainsKey(hero) == true;
        }

        public static bool ShouldIgnoreHeroPartyChange(MobileParty party)
        {
            return party != null
                   && (_isCreatingTemporaryParty
                       || TemporaryParties.Contains(party));
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
            }
            finally
            {
                RestoreHeroHitPoints(context.OriginalHeroHitPoints);
                Current = null;
                _isCleaningUp = false;
            }
        }

        private static MobileParty CreateParty(
            TextObject name,
            TeamConfig config,
            bool isPlayerParty,
            CharacterObject controlledCharacter,
            out CharacterObject primaryGeneral,
            out List<CharacterObject> spawnPriorityCharacters,
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
                out primaryGeneral,
                out spawnPriorityCharacters,
                out priorityCharacterIds);
            MobileParty party;
            _isCreatingTemporaryParty = true;
            try
            {
                party = CustomPartyComponent.CreateQuestParty(
                    MobileParty.MainParty.Position2D,
                    0f,
                    null,
                    name,
                    clan,
                    roster,
                    TroopRoster.CreateDummyTroopRoster(),
                    owner,
                    avoidHostileActions: true);
                TemporaryParties.Add(party);
            }
            finally
            {
                _isCreatingTemporaryParty = false;
            }

            party.IsVisible = false;
            party.Ai.SetMoveModeHold();
            return party;
        }

        private static TroopRoster CreateRoster(
            TeamConfig config,
            CharacterObject controlledCharacter,
            out CharacterObject primaryGeneral,
            out List<CharacterObject> spawnPriorityCharacters,
            out List<string> priorityCharacterIds)
        {
            TroopRoster roster = TroopRoster.CreateDummyTroopRoster();
            primaryGeneral = null;
            spawnPriorityCharacters = new List<CharacterObject>();
            priorityCharacterIds = new List<string>();
            if (config.HasGeneral)
            {
                foreach (TroopConfig troop in config.Generals.Troops)
                {
                    if (troop.Character.CharacterObject is CharacterObject character)
                    {
                        bool isControlledCharacter =
                            primaryGeneral == null && character == controlledCharacter;
                        roster.AddToCounts(character, 1);
                        if (isControlledCharacter)
                            primaryGeneral = character;
                        if (isControlledCharacter
                            && !character.IsHero
                            && !spawnPriorityCharacters.Contains(character))
                        {
                            spawnPriorityCharacters.Add(character);
                        }
                        priorityCharacterIds.Add(character.StringId);
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
                        roster.AddToCounts(character, troop.Number);
                        if (character.IsHero)
                            priorityCharacterIds.Add(character.StringId);
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

        private static Dictionary<Hero, int> CaptureHeroHitPoints(
            params MobileParty[] parties)
        {
            return parties
                .Where(party => party != null)
                .SelectMany(party => party.MemberRoster.GetTroopRoster())
                .Select(element => element.Character.HeroObject)
                .Where(hero => hero != null)
                .Distinct()
                .ToDictionary(hero => hero, hero => hero.HitPoints);
        }

        private static void RestoreHeroHitPoints(
            IReadOnlyDictionary<Hero, int> originalHeroHitPoints)
        {
            if (originalHeroHitPoints == null)
                return;

            foreach (KeyValuePair<Hero, int> entry in originalHeroHitPoints)
                entry.Key.HitPoints = entry.Value;
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
            if (party == null)
                return;

            try
            {
                if (party.IsActive)
                    DestroyPartyAction.Apply(null, party);
            }
            finally
            {
                TemporaryParties.Remove(party);
            }
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

    }
}
