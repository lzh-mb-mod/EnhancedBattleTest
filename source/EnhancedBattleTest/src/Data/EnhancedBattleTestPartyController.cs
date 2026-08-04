using EnhancedBattleTest.Config;
using EnhancedBattleTest.SinglePlayer.Config;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
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
        internal sealed class PlayerIdentityState
        {
            public MobileParty OriginalMainParty { get; }
            public BasicCharacterObject OriginalPlayerTroop { get; }
            public Hero BattleHero { get; }
            public MobileParty OriginalBattleHeroParty { get; }
            public bool IsTemporaryHero { get; }
            public CharacterObject BattleCharacter => BattleHero.CharacterObject;

            public PlayerIdentityState(
                MobileParty originalMainParty,
                BasicCharacterObject originalPlayerTroop,
                Hero battleHero,
                MobileParty originalBattleHeroParty,
                bool isTemporaryHero)
            {
                OriginalMainParty = originalMainParty;
                OriginalPlayerTroop = originalPlayerTroop;
                BattleHero = battleHero;
                OriginalBattleHeroParty = originalBattleHeroParty;
                IsTemporaryHero = isTemporaryHero;
            }
        }

        private sealed class TemporaryPartyProfile
        {
            public Hero Owner { get; }
            public Hero Leader { get; }
            public Clan Clan { get; }
            public Banner Banner { get; }
            public Tuple<uint, uint> Colors { get; }

            public TemporaryPartyProfile(
                Hero owner,
                Hero leader,
                Clan clan,
                Banner banner,
                Tuple<uint, uint> colors)
            {
                Owner = owner;
                Leader = leader;
                Clan = clan;
                Banner = banner;
                Colors = colors;
            }
        }

        public sealed class BattleContext
        {
            public MobileParty PlayerParty { get; }
            public MobileParty EnemyParty { get; }
            public IReadOnlyList<MobileParty> PlayerParties { get; }
            public IReadOnlyList<MobileParty> EnemyParties { get; }
            public IReadOnlyList<MobileParty> OwnedTemporaryParties { get; }
            public MapEvent MapEvent { get; }
            public PartyGroupTroopSupplier[] TroopSuppliers { get; }
            public CharacterObject PlayerCharacter { get; }
            public IReadOnlyList<CharacterObject> PlayerSpawnPriorityCharacters { get; }
            public IReadOnlyList<string> PlayerPriorityCharacterIds { get; }
            public IReadOnlyDictionary<Hero, int> OriginalHeroHitPoints { get; }
            public EquipmentModifierType EquipmentModifierType { get; }
            internal PlayerIdentityState PlayerIdentity { get; }
            internal PlayerEncounter OriginalPlayerEncounter { get; }

            internal BattleContext(
                MobileParty playerParty,
                MobileParty enemyParty,
                IReadOnlyList<MobileParty> playerParties,
                IReadOnlyList<MobileParty> enemyParties,
                IReadOnlyList<MobileParty> ownedTemporaryParties,
                MapEvent mapEvent,
                PartyGroupTroopSupplier[] troopSuppliers,
                CharacterObject playerCharacter,
                IReadOnlyList<CharacterObject> playerSpawnPriorityCharacters,
                IReadOnlyList<string> playerPriorityCharacterIds,
                IReadOnlyDictionary<Hero, int> originalHeroHitPoints,
                EquipmentModifierType equipmentModifierType,
                PlayerIdentityState playerIdentity,
                PlayerEncounter originalPlayerEncounter)
            {
                PlayerParty = playerParty;
                EnemyParty = enemyParty;
                PlayerParties = playerParties;
                EnemyParties = enemyParties;
                OwnedTemporaryParties = ownedTemporaryParties;
                MapEvent = mapEvent;
                TroopSuppliers = troopSuppliers;
                PlayerCharacter = playerCharacter;
                PlayerSpawnPriorityCharacters = playerSpawnPriorityCharacters;
                PlayerPriorityCharacterIds = playerPriorityCharacterIds;
                OriginalHeroHitPoints = originalHeroHitPoints;
                EquipmentModifierType = equipmentModifierType;
                PlayerIdentity = playerIdentity;
                OriginalPlayerEncounter = originalPlayerEncounter;
            }
        }

        public static BattleContext Current { get; private set; }
        private static bool _isCleaningUp;
        private static bool _isCreatingTemporaryParty;
        private static readonly HashSet<MobileParty> TemporaryParties =
            new HashSet<MobileParty>();
        private static readonly HashSet<Hero> ParticipatingHeroes =
            new HashSet<Hero>();
        private static readonly Dictionary<MobileParty, TemporaryPartyProfile>
            TemporaryPartyProfiles =
                new Dictionary<MobileParty, TemporaryPartyProfile>();
        private static readonly Dictionary<MobileParty, bool>
            TemporaryPartyPlayerTeamMembership =
                new Dictionary<MobileParty, bool>();
        private static readonly Dictionary<MobileParty, int>
            TemporaryPartyTacticLevels =
                new Dictionary<MobileParty, int>();
        private static readonly Dictionary<
            MobileParty,
            Dictionary<BasicCharacterObject, float>>
            TemporaryPartyFemaleRatios =
                new Dictionary<
                    MobileParty,
                    Dictionary<BasicCharacterObject, float>>();
        private static readonly Dictionary<
            MobileParty,
            Dictionary<BasicCharacterObject, List<int>>>
            TemporaryPartyEquipmentSets =
                new Dictionary<
                    MobileParty,
                    Dictionary<BasicCharacterObject, List<int>>>();
        private static readonly PropertyInfo MainPartyProperty =
            AccessTools.Property(typeof(Campaign), nameof(Campaign.MainParty));
        private static readonly PropertyInfo PlayerEncounterProperty =
            AccessTools.Property(
                typeof(Campaign),
                nameof(Campaign.PlayerEncounter));
        private static readonly FieldInfo PlayerEncounterMapEventField =
            AccessTools.Field(typeof(PlayerEncounter), "_mapEvent");
        private static readonly FieldInfo HeroPartyField =
            AccessTools.Field(typeof(Hero), "_partyBelongedTo");
        private static readonly MethodInfo UnregisterDeadHeroMethod =
            AccessTools.Method(
                typeof(CampaignObjectManager),
                "UnregisterDeadHero");
        private static readonly PropertyInfo PartyGroupProperty =
            AccessTools.Property(
                typeof(PartyGroupTroopSupplier),
                "PartyGroup");
        private static readonly FieldInfo ReadyTroopsPriorityListField =
            AccessTools.Field(
                typeof(MapEventSide),
                "_readyTroopsPriorityList");

        public static BattleContext Create(BattleConfig config)
        {
            Cleanup();
            config.NormalizeCharacterGroups();

            MobileParty playerParty = null;
            MobileParty enemyParty = null;
            var playerParties = new List<MobileParty>();
            var enemyParties = new List<MobileParty>();
            var ownedTemporaryParties = new List<MobileParty>();
            MapEvent mapEvent = null;
            CharacterObject selectedPlayerCharacter =
                config.BattleTypeConfig.PlayerType == PlayerType.None
                    ? null
                    : GetPlayerCharacter(config.PlayerTeamConfig);
            PlayerIdentityState playerIdentity =
                CreatePlayerIdentity(selectedPlayerCharacter);
            PlayerEncounter originalPlayerEncounter =
                Campaign.Current.PlayerEncounter;
            Dictionary<Hero, int> originalHeroHitPoints = null;
            try
            {
                BattleSideEnum playerSide = config.BattleTypeConfig.PlayerSide;
                bool isPlayerAttacker = playerSide == BattleSideEnum.Attacker;
                BasicCharacterObject preferredPlayerPartyCharacter =
                    GetPreferredPlayerPartyCharacter(
                        config,
                        selectedPlayerCharacter);
                playerParty = CreateParty(
                    new TextObject("{=sSJSTe5p}Player Party"),
                    config.PlayerTeamConfig.PrimaryParty,
                    isPlayerAttacker,
                    selectedPlayerCharacter,
                    selectedPlayerCharacter != null,
                    true,
                    preferredPlayerPartyCharacter,
                    out CharacterObject playerCharacter,
                    out List<CharacterObject> playerSpawnPriorityCharacters,
                    out List<string> playerPriorityCharacterIds);
                playerParties.Add(playerParty);
                ownedTemporaryParties.Add(playerParty);
                if (selectedPlayerCharacter != null
                    && config.PlayerTeamConfig.PlayerCharacter
                        is SPCharacterConfig playerConfig
                    && playerConfig.OverrideGender)
                {
                    SetFemaleRatio(
                        playerParty,
                        playerIdentity.BattleCharacter,
                        playerConfig.FemaleRatio);
                }
                if (selectedPlayerCharacter != null
                    && config.PlayerTeamConfig.PlayerCharacter
                        is SPCharacterConfig playerEquipmentConfig)
                {
                    SetEquipmentSet(
                        playerParty,
                        playerIdentity.BattleCharacter,
                        playerEquipmentConfig.EquipmentSetIndex);
                }
                CreateAlliedParties(
                    config.PlayerTeamConfig.AlliedParties,
                    true,
                    isPlayerAttacker,
                    playerParties,
                    ownedTemporaryParties,
                    playerSpawnPriorityCharacters,
                    config.PlayerTeamConfig.PrimaryParty.IsInArmy);
                if (config.PlayerTeamConfig.OverrideTacticLevel)
                {
                    RegisterTacticLevel(
                        playerParties,
                        config.PlayerTeamConfig.TacticLevel);
                }

                var enemySpawnPriorityCharacters = new List<CharacterObject>();
                enemyParty = CreateParty(
                    new TextObject("{=0xC75dN6}Enemy Party"),
                    config.EnemyTeamConfig.PrimaryParty,
                    !isPlayerAttacker,
                    null,
                    false,
                    false,
                    null,
                    out _,
                    out enemySpawnPriorityCharacters,
                    out _);
                enemyParties.Add(enemyParty);
                ownedTemporaryParties.Add(enemyParty);
                CreateAlliedParties(
                    config.EnemyTeamConfig.AlliedParties,
                    false,
                    !isPlayerAttacker,
                    enemyParties,
                    ownedTemporaryParties,
                    enemySpawnPriorityCharacters,
                    false);
                if (config.EnemyTeamConfig.OverrideTacticLevel)
                {
                    RegisterTacticLevel(
                        enemyParties,
                        config.EnemyTeamConfig.TacticLevel);
                }
                originalHeroHitPoints = CaptureHeroHitPoints(ownedTemporaryParties);
                ParticipatingHeroes.UnionWith(originalHeroHitPoints.Keys);
                PrepareHeroesForBattle(originalHeroHitPoints.Keys);
                ApplyPlayerIdentity(playerIdentity, playerParty);

                PartyBase attacker = playerSide == BattleSideEnum.Attacker
                    ? playerParty.Party
                    : enemyParty.Party;
                PartyBase defender = playerSide == BattleSideEnum.Defender
                    ? playerParty.Party
                    : enemyParty.Party;

                mapEvent = FieldBattleEventComponent
                    .CreateFieldBattleEvent(attacker, defender)
                    .MapEvent;
                AttachAlliedParties(
                    mapEvent,
                    playerSide,
                    playerParties.Skip(1));
                AttachAlliedParties(
                    mapEvent,
                    playerSide == BattleSideEnum.Attacker
                        ? BattleSideEnum.Defender
                        : BattleSideEnum.Attacker,
                    enemyParties.Skip(1));
                ApplyPlayerEncounter(
                    mapEvent,
                    attacker,
                    defender);
                var suppliers = new PartyGroupTroopSupplier[2];
                FlattenedTroopRoster playerPriorityTroops =
                    CreatePriorityRoster(playerSpawnPriorityCharacters);
                FlattenedTroopRoster enemyPriorityTroops =
                    CreatePriorityRoster(enemySpawnPriorityCharacters);
                suppliers[(int)BattleSideEnum.Defender] =
                    new PartyGroupTroopSupplier(
                        mapEvent,
                        BattleSideEnum.Defender,
                        playerSide == BattleSideEnum.Defender
                            ? playerPriorityTroops
                            : enemyPriorityTroops);
                suppliers[(int)BattleSideEnum.Attacker] =
                    new PartyGroupTroopSupplier(
                        mapEvent,
                        BattleSideEnum.Attacker,
                        playerSide == BattleSideEnum.Attacker
                            ? playerPriorityTroops
                            : enemyPriorityTroops);
                if (config.BattleTypeConfig.BalanceTroopSpawnOrder)
                {
                    BalanceTroopSpawnOrder(
                        suppliers[(int)BattleSideEnum.Defender]);
                    BalanceTroopSpawnOrder(
                        suppliers[(int)BattleSideEnum.Attacker]);
                }

                Current = new BattleContext(
                    playerParty,
                    enemyParty,
                    playerParties,
                    enemyParties,
                    ownedTemporaryParties,
                    mapEvent,
                    suppliers,
                    playerCharacter,
                    playerSpawnPriorityCharacters,
                    playerPriorityCharacterIds,
                    originalHeroHitPoints,
                    config.BattleTypeConfig.EquipmentModifierType,
                    playerIdentity,
                    originalPlayerEncounter);
                EnhancedBattleTestSaveGuard.Disable();
                return Current;
            }
            catch
            {
                try
                {
                    try
                    {
                        RestorePlayerEncounter(originalPlayerEncounter);
                        PrepareMapEventForCleanup(mapEvent);
                        mapEvent?.FinalizeEvent();
                    }
                    finally
                    {
                        try
                        {
                            RestorePlayerIdentity(playerIdentity);
                        }
                        finally
                        {
                            try
                            {
                                DestroyParties(ownedTemporaryParties);
                            }
                            finally
                            {
                                DestroyTemporaryHero(playerIdentity);
                            }
                        }
                    }
                }
                finally
                {
                    try
                    {
                        RestoreHeroHitPoints(originalHeroHitPoints);
                    }
                    finally
                    {
                        ParticipatingHeroes.Clear();
                    }
                }
                throw;
            }
        }

        public static bool IsTestParty(PartyBase party)
        {
            return party != null
                   && party.MobileParty != null
                   && TemporaryParties.Contains(party.MobileParty);
        }

        public static bool IsParticipatingHero(Hero hero)
        {
            return hero != null && ParticipatingHeroes.Contains(hero);
        }

        public static bool ShouldIgnoreHeroPartyChange(MobileParty party)
        {
            return party != null
                   && (_isCreatingTemporaryParty
                       || TemporaryParties.Contains(party));
        }

        public static bool IsPartyInPlayerTeam(PartyBase party)
        {
            return party?.MobileParty != null
                   && TemporaryPartyPlayerTeamMembership.TryGetValue(
                       party.MobileParty,
                       out bool isInPlayerTeam)
                   && isInPlayerTeam;
        }

        public static bool TryGetTemporaryPartyAppearance(
            PartyBase party,
            out Banner banner,
            out Tuple<uint, uint> colors)
        {
            if (party?.MobileParty != null
                && TemporaryPartyProfiles.TryGetValue(
                    party.MobileParty,
                    out TemporaryPartyProfile profile))
            {
                banner = profile.Banner;
                colors = profile.Colors;
                return true;
            }

            banner = null;
            colors = null;
            return false;
        }

        public static bool TryGetFemaleRatio(
            PartyBase party,
            BasicCharacterObject character,
            out float femaleRatio)
        {
            femaleRatio = 0f;
            return party?.MobileParty != null
                   && character != null
                   && TemporaryPartyFemaleRatios.TryGetValue(
                       party.MobileParty,
                       out Dictionary<BasicCharacterObject, float> ratios)
                   && ratios.TryGetValue(character, out femaleRatio);
        }

        public static bool TryGetEquipmentSet(
            PartyBase party,
            BasicCharacterObject character,
            int seed,
            out Equipment equipment)
        {
            equipment = null;
            if (party?.MobileParty == null
                || !(character is CharacterObject characterObject)
                || !TemporaryPartyEquipmentSets.TryGetValue(
                    party.MobileParty,
                    out Dictionary<BasicCharacterObject, List<int>> sets)
                || !sets.TryGetValue(character, out List<int> indices)
                || indices.Count == 0)
                return false;

            int selectedIndex = indices[(int)((uint)seed % indices.Count)];
            IReadOnlyList<Equipment> equipmentSets =
                SPCharacterConfig.GetBattleEquipmentSets(characterObject);
            if (selectedIndex < 0 || selectedIndex >= equipmentSets.Count)
                return false;

            equipment = equipmentSets[selectedIndex];
            return true;
        }

        public static bool TryGetTacticLevel(
            PartyBase party,
            out int tacticLevel)
        {
            tacticLevel = 0;
            return party?.MobileParty != null
                   && TemporaryPartyTacticLevels.TryGetValue(
                      party.MobileParty,
                      out tacticLevel);
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
                try
                {
                    RestorePlayerEncounter(context.OriginalPlayerEncounter);
                    PrepareMapEventForCleanup(context.MapEvent);
                    context.MapEvent?.FinalizeEvent();
                }
                finally
                {
                    try
                    {
                        RestorePlayerIdentity(context.PlayerIdentity);
                    }
                    finally
                    {
                        try
                        {
                            DestroyParties(context.OwnedTemporaryParties);
                        }
                        finally
                        {
                            DestroyTemporaryHero(context.PlayerIdentity);
                        }
                    }
                }
            }
            finally
            {
                try
                {
                    RestoreHeroHitPoints(context.OriginalHeroHitPoints);
                }
                finally
                {
                    ParticipatingHeroes.Clear();
                    Current = null;
                    _isCleaningUp = false;
                }
            }
        }

        private static MobileParty CreateParty(
            TextObject name,
            PartyConfig config,
            bool isAttacker,
            CharacterObject controlledCharacter,
            bool addControlledCharacter,
            bool isInPlayerTeam,
            BasicCharacterObject preferredCharacter,
            out CharacterObject playerCharacter,
            out List<CharacterObject> spawnPriorityCharacters,
            out List<string> priorityCharacterIds)
        {
            TroopRoster roster = CreateRoster(
                config,
                controlledCharacter,
                addControlledCharacter,
                out playerCharacter,
                out spawnPriorityCharacters,
                out priorityCharacterIds);
            TemporaryPartyProfile profile = ResolvePartyProfile(
                config,
                isAttacker,
                preferredCharacter,
                roster);
            MobileParty party = null;
            _isCreatingTemporaryParty = true;
            try
            {
                party = CustomPartyComponent.CreateCustomPartyWithTroopRoster(
                    MobileParty.MainParty.Position,
                    0f,
                    null,
                    name,
                    profile.Clan,
                    roster,
                    TroopRoster.CreateDummyTroopRoster(),
                    profile.Owner,
                    avoidHostileActions: true);
                if (profile.Leader != null)
                    party.ChangePartyLeader(profile.Leader);
                TemporaryParties.Add(party);
                TemporaryPartyProfiles.Add(party, profile);
                TemporaryPartyPlayerTeamMembership.Add(
                    party,
                    isInPlayerTeam);
                RegisterFemaleRatios(party, config);
                RegisterEquipmentSets(party, config);
                party.IsVisible = false;
                party.SetMoveModeHold();
                return party;
            }
            catch
            {
                DestroyParty(party);
                throw;
            }
            finally
            {
                _isCreatingTemporaryParty = false;
            }
        }

        private static TroopRoster CreateRoster(
            PartyConfig config,
            CharacterObject controlledCharacter,
            bool addControlledCharacter,
            out CharacterObject playerCharacter,
            out List<CharacterObject> spawnPriorityCharacters,
            out List<string> priorityCharacterIds)
        {
            TroopRoster roster = TroopRoster.CreateDummyTroopRoster();
            playerCharacter = controlledCharacter;
            spawnPriorityCharacters = new List<CharacterObject>();
            priorityCharacterIds = new List<string>();
            if (config.HasHeroes)
            {
                foreach (TroopConfig troop in config.Heroes.Troops)
                {
                    if (troop.Character.CharacterObject is CharacterObject character)
                    {
                        roster.AddToCounts(character, 1);
                        if (!spawnPriorityCharacters.Contains(character))
                            spawnPriorityCharacters.Add(character);
                        priorityCharacterIds.Add(character.StringId);
                    }
                }
            }

            foreach (TroopConfig troop in config.Troops.Troops)
            {
                if (troop.Number > 0
                    && troop.Character.CharacterObject is CharacterObject character)
                {
                    roster.AddToCounts(character, troop.Number);
                    if (character.IsHero)
                        priorityCharacterIds.Add(character.StringId);
                }
            }

            if (addControlledCharacter && controlledCharacter != null)
            {
                if (!roster.Contains(controlledCharacter))
                    roster.AddToCounts(controlledCharacter, 1);
                spawnPriorityCharacters.Remove(controlledCharacter);
                spawnPriorityCharacters.Insert(0, controlledCharacter);
                if (!priorityCharacterIds.Contains(controlledCharacter.StringId))
                    priorityCharacterIds.Add(controlledCharacter.StringId);
            }

            return roster;
        }

        private static void BalanceTroopSpawnOrder(
            PartyGroupTroopSupplier supplier)
        {
            MapEventSide partyGroup =
                (MapEventSide)PartyGroupProperty.GetValue(supplier);
            var priorityList =
                (List<(
                    FlattenedTroopRosterElement,
                    MapEventParty,
                    float)>)ReadyTroopsPriorityListField.GetValue(partyGroup);
            if (priorityList == null || priorityList.Count < 2)
                return;

            var preserved =
                new List<(
                    FlattenedTroopRosterElement,
                    MapEventParty,
                    float)>();
            var queues = new Dictionary<
                BasicCharacterObject,
                Queue<(
                    FlattenedTroopRosterElement,
                    MapEventParty,
                    float)>>();
            var troopOrder = new List<BasicCharacterObject>();
            foreach (var priority in priorityList)
            {
                BasicCharacterObject troop = priority.Item1.Troop;
                if (troop.IsHero || priority.Item3 > 1f)
                {
                    preserved.Add(priority);
                    continue;
                }

                if (!queues.TryGetValue(troop, out var queue))
                {
                    queue = new Queue<(
                        FlattenedTroopRosterElement,
                        MapEventParty,
                        float)>();
                    queues.Add(troop, queue);
                    troopOrder.Add(troop);
                }
                queue.Enqueue(priority);
            }

            priorityList.Clear();
            priorityList.AddRange(preserved);
            var weights = troopOrder.ToDictionary(
                troop => troop,
                troop => queues[troop].Count);
            var currentWeights = troopOrder.ToDictionary(
                troop => troop,
                troop => 0);
            int totalWeight = weights.Values.Sum();
            while (totalWeight > 0)
            {
                foreach (BasicCharacterObject troop in troopOrder)
                    currentWeights[troop] += weights[troop];

                BasicCharacterObject selectedTroop = troopOrder
                    .Where(troop => queues[troop].Count > 0)
                    .OrderByDescending(troop => currentWeights[troop])
                    .ThenBy(troop => troopOrder.IndexOf(troop))
                    .First();
                priorityList.Add(queues[selectedTroop].Dequeue());
                currentWeights[selectedTroop] -= totalWeight;
                if (queues[selectedTroop].Count == 0)
                {
                    totalWeight -= weights[selectedTroop];
                    weights[selectedTroop] = 0;
                }
            }
        }

        private static CharacterObject GetPlayerCharacter(TeamConfig config)
        {
            return config.PlayerCharacter?.CharacterObject as CharacterObject;
        }

        private static Dictionary<Hero, int> CaptureHeroHitPoints(
            IEnumerable<MobileParty> parties)
        {
            return parties
                .Where(party => party != null)
                .SelectMany(party => party.MemberRoster.GetTroopRoster())
                .Select(element => element.Character.HeroObject)
                .Where(hero => hero != null)
                .Distinct()
                .ToDictionary(hero => hero, hero => hero.HitPoints);
        }

        private static void PrepareHeroesForBattle(IEnumerable<Hero> heroes)
        {
            foreach (Hero hero in heroes)
                hero.HitPoints = hero.MaxHitPoints;
        }

        private static FlattenedTroopRoster CreatePriorityRoster(
            IEnumerable<CharacterObject> characters)
        {
            var roster = new FlattenedTroopRoster();
            foreach (CharacterObject character in characters)
                roster.Add(character, 1);
            return roster;
        }

        private static void RestoreHeroHitPoints(
            IReadOnlyDictionary<Hero, int> originalHeroHitPoints)
        {
            if (originalHeroHitPoints == null)
                return;

            foreach (KeyValuePair<Hero, int> entry in originalHeroHitPoints)
                entry.Key.HitPoints = entry.Value;
        }

        private static TemporaryPartyProfile ResolvePartyProfile(
            PartyConfig config,
            bool isAttacker,
            BasicCharacterObject preferredCharacter,
            TroopRoster roster)
        {
            BasicCultureObject culture = Utility.GetCulture(config);
            Hero preferredHero =
                (preferredCharacter as CharacterObject)?.HeroObject;
            Hero configuredGeneral = config.HasHeroes
                ? config.Heroes.Troops
                    .Select(troop =>
                        troop?.Character?.CharacterObject as CharacterObject)
                    .Select(character => character?.HeroObject)
                    .FirstOrDefault(hero => hero != null)
                : null;
            Hero leader = GetLeader(
                roster,
                preferredHero,
                configuredGeneral);
            Hero owner = leader
                         ?? Hero.AllAliveHeroes.FirstOrDefault(
                             hero => hero.Culture == culture)
                         ?? Hero.MainHero;
            Clan clan = owner?.Clan
                        ?? Clan.All.FirstOrDefault(
                            candidate => candidate.Culture == culture)
                        ?? Clan.PlayerClan;
            config.ResolveAppearance(
                isAttacker,
                out Banner banner,
                out Tuple<uint, uint> colors,
                preferredCharacter);
            return new TemporaryPartyProfile(
                owner,
                leader,
                clan,
                banner,
                colors);
        }

        private static Hero GetLeader(
            TroopRoster roster,
            params Hero[] preferredLeaders)
        {
            Hero preferredLeader = preferredLeaders.FirstOrDefault(hero =>
                    hero != null
                    && roster.Contains(hero.CharacterObject));
            if (preferredLeader != null)
                return preferredLeader;

            return roster.GetTroopRoster()
                .Select(element => element.Character?.HeroObject)
                .FirstOrDefault(hero => hero != null);
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
                TemporaryPartyProfiles.Remove(party);
                TemporaryPartyPlayerTeamMembership.Remove(party);
                TemporaryPartyFemaleRatios.Remove(party);
                TemporaryPartyEquipmentSets.Remove(party);
                TemporaryPartyTacticLevels.Remove(party);
            }
        }

        private static void RegisterFemaleRatios(
            MobileParty party,
            PartyConfig config)
        {
            var weightedRatios =
                new Dictionary<BasicCharacterObject, Tuple<float, int>>();

            if (config.HasHeroes)
            {
                foreach (TroopConfig troop in config.Heroes.Troops)
                    AddFemaleRatio(weightedRatios, troop, 1);
            }

            foreach (TroopConfig troop in config.Troops.Troops)
                AddFemaleRatio(weightedRatios, troop, troop.Number);

            TemporaryPartyFemaleRatios[party] = weightedRatios.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.Item1 / pair.Value.Item2);
        }

        private static void RegisterEquipmentSets(
            MobileParty party,
            PartyConfig config)
        {
            var sets = new Dictionary<BasicCharacterObject, List<int>>();
            if (config.HasHeroes)
            {
                foreach (TroopConfig troop in config.Heroes.Troops)
                    AddEquipmentSet(sets, troop, 1);
            }

            foreach (TroopConfig troop in config.Troops.Troops)
                AddEquipmentSet(sets, troop, troop.Number);

            TemporaryPartyEquipmentSets[party] = sets;
        }

        private static void RegisterTacticLevel(
            IEnumerable<MobileParty> parties,
            int tacticLevel)
        {
            foreach (MobileParty party in parties)
                TemporaryPartyTacticLevels[party] = tacticLevel;
        }

        private static void AddFemaleRatio(
            IDictionary<BasicCharacterObject, Tuple<float, int>> ratios,
            TroopConfig troop,
            int count)
        {
            if (count <= 0
                || !(troop?.Character is SPCharacterConfig characterConfig)
                || !characterConfig.OverrideGender
                || characterConfig.CharacterObject == null)
                return;

            float ratio = Math.Max(
                0f,
                Math.Min(1f, characterConfig.FemaleRatio));
            ratios.TryGetValue(
                characterConfig.CharacterObject,
                out Tuple<float, int> current);
            ratios[characterConfig.CharacterObject] = new Tuple<float, int>(
                (current?.Item1 ?? 0f) + ratio * count,
                (current?.Item2 ?? 0) + count);
        }

        private static void AddEquipmentSet(
            IDictionary<BasicCharacterObject, List<int>> sets,
            TroopConfig troop,
            int count)
        {
            if (count <= 0
                || !(troop?.Character is SPCharacterConfig characterConfig)
                || characterConfig.CharacterObject == null)
                return;

            if (!sets.TryGetValue(
                    characterConfig.CharacterObject,
                    out List<int> indices))
            {
                indices = new List<int>();
                sets.Add(characterConfig.CharacterObject, indices);
            }

            for (int i = 0; i < count; i++)
                indices.Add(characterConfig.EquipmentSetIndex);
        }

        private static void SetEquipmentSet(
            MobileParty party,
            BasicCharacterObject character,
            int equipmentSetIndex)
        {
            if (party == null || character == null)
                return;

            if (!TemporaryPartyEquipmentSets.TryGetValue(
                    party,
                    out Dictionary<BasicCharacterObject, List<int>> sets))
            {
                sets = new Dictionary<BasicCharacterObject, List<int>>();
                TemporaryPartyEquipmentSets.Add(party, sets);
            }

            sets[character] = new List<int> { equipmentSetIndex };
        }

        private static void SetFemaleRatio(
            MobileParty party,
            BasicCharacterObject character,
            float femaleRatio)
        {
            if (party == null || character == null)
                return;

            if (!TemporaryPartyFemaleRatios.TryGetValue(
                    party,
                    out Dictionary<BasicCharacterObject, float> ratios))
            {
                ratios = new Dictionary<BasicCharacterObject, float>();
                TemporaryPartyFemaleRatios.Add(party, ratios);
            }

            ratios[character] = Math.Max(0f, Math.Min(1f, femaleRatio));
        }

        private static void CreateAlliedParties(
            IEnumerable<PartyConfig> configs,
            bool isPlayerSide,
            bool isAttacker,
            ICollection<MobileParty> sideParties,
            ICollection<MobileParty> ownedTemporaryParties,
            ICollection<CharacterObject> spawnPriorityCharacters,
            bool playerPrimaryPartyIsInArmy)
        {
            if (configs == null)
                return;

            int index = 1;
            foreach (PartyConfig config in configs.Where(candidate => candidate != null))
            {
                TextObject name = GameTexts.FindText(
                    isPlayerSide
                        ? "str_ebt_player_allied_party"
                        : "str_ebt_enemy_allied_party");
                name.SetTextVariable("INDEX", index++);
                MobileParty party = CreateParty(
                    name,
                    config,
                    isAttacker,
                    null,
                    false,
                    isPlayerSide
                    && playerPrimaryPartyIsInArmy
                    && config.IsInArmy,
                    null,
                    out _,
                    out List<CharacterObject> partySpawnPriorityCharacters,
                    out _);
                sideParties.Add(party);
                ownedTemporaryParties.Add(party);
                foreach (CharacterObject character in partySpawnPriorityCharacters)
                {
                    if (!spawnPriorityCharacters.Contains(character))
                        spawnPriorityCharacters.Add(character);
                }
            }
        }

        private static void AttachAlliedParties(
            MapEvent mapEvent,
            BattleSideEnum side,
            IEnumerable<MobileParty> alliedParties)
        {
            MapEventSide mapEventSide = mapEvent.GetMapEventSide(side);
            foreach (MobileParty party in alliedParties)
                party.Party.MapEventSide = mapEventSide;
        }

        private static BasicCharacterObject GetPreferredPlayerPartyCharacter(
            BattleConfig config,
            CharacterObject playerCharacter)
        {
            if (config.BattleTypeConfig.PlayerType == PlayerType.Commander)
                return playerCharacter;

            return config.PlayerTeamConfig.PrimaryParty
                       .GetFirstHeroCharacter(playerCharacter)
                   ?? playerCharacter;
        }

        private static void DestroyParties(
            IEnumerable<MobileParty> parties)
        {
            if (parties == null)
                return;

            Exception firstException = null;
            foreach (MobileParty party in parties.Reverse().ToList())
            {
                try
                {
                    DestroyParty(party);
                }
                catch (Exception exception)
                {
                    if (firstException == null)
                        firstException = exception;
                }
            }

            if (firstException != null)
                throw firstException;
        }

        private static void PrepareMapEventForCleanup(MapEvent mapEvent)
        {
            if (mapEvent == null)
                return;

            mapEvent.ResetBattleState();
            mapEvent.AttackerSide.TroopCasualties = 0;
            mapEvent.DefenderSide.TroopCasualties = 0;
        }

        private static PlayerIdentityState CreatePlayerIdentity(
            CharacterObject selectedCharacter)
        {
            CharacterObject character = selectedCharacter
                                        ?? Hero.MainHero.CharacterObject;
            Hero battleHero = character.HeroObject;
            bool isTemporaryHero = battleHero == null;
            if (isTemporaryHero)
            {
                battleHero = HeroCreator.CreateSpecialHero(
                    character,
                    age: (int)character.Age);
            }

            return new PlayerIdentityState(
                MobileParty.MainParty,
                Game.Current.PlayerTroop,
                battleHero,
                battleHero.PartyBelongedTo,
                isTemporaryHero);
        }

        private static void ApplyPlayerIdentity(
            PlayerIdentityState identity,
            MobileParty playerParty)
        {
            HeroPartyField.SetValue(identity.BattleHero, playerParty);
            Game.Current.PlayerTroop = identity.BattleCharacter;
            MainPartyProperty.SetValue(Campaign.Current, playerParty);
        }

        private static void RestorePlayerIdentity(PlayerIdentityState identity)
        {
            if (identity == null)
                return;

            MainPartyProperty.SetValue(
                Campaign.Current,
                identity.OriginalMainParty);
            Game.Current.PlayerTroop = identity.OriginalPlayerTroop;
            HeroPartyField.SetValue(
                identity.BattleHero,
                identity.OriginalBattleHeroParty);
        }

        private static void ApplyPlayerEncounter(
            MapEvent mapEvent,
            PartyBase attacker,
            PartyBase defender)
        {
            PlayerEncounter.Start();
            PlayerEncounter encounter = PlayerEncounter.Current;
            encounter.SetupFields(attacker, defender);
            PlayerEncounterMapEventField.SetValue(encounter, mapEvent);
        }

        private static void RestorePlayerEncounter(
            PlayerEncounter originalEncounter)
        {
            PlayerEncounterProperty.SetValue(
                Campaign.Current,
                originalEncounter);
        }

        private static void DestroyTemporaryHero(PlayerIdentityState identity)
        {
            if (identity?.IsTemporaryHero != true)
                return;

            Hero hero = identity.BattleHero;
            if (hero.IsAlive)
                DisableHeroAction.Apply(hero);
            UnregisterDeadHeroMethod.Invoke(
                Campaign.Current.CampaignObjectManager,
                new object[] { hero });
            Game.Current.ObjectManager.UnregisterObject(hero.CharacterObject);
        }

    }
}
