using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace EnhancedBattleTest.Config
{
    public class BattleConfig
    {
        private const string CurrentConfigFileName = "spconfig-v3.xml";
        private const string NamedConfigFolderName = "Configurations";

        public static BattleConfig Instance;

        public TeamConfig PlayerTeamConfig = new TeamConfig();
        public TeamConfig EnemyTeamConfig = new TeamConfig();
        public BattleTypeConfig BattleTypeConfig = new BattleTypeConfig();
        public MapConfig MapConfig = new MapConfig();
        public SiegeMachineConfig SiegeMachineConfig = new SiegeMachineConfig();

        public BattleConfig()
        { }

        public static BattleConfig CreateDefault()
        {
            return new BattleConfig
            {
                PlayerTeamConfig = CreatePlayerTeam(),
                EnemyTeamConfig = CreateEnemyTeam()
            };
        }

        public void Reset()
        {
            BattleConfig defaults = CreateDefault();
            PlayerTeamConfig = defaults.PlayerTeamConfig;
            EnemyTeamConfig = defaults.EnemyTeamConfig;
            BattleTypeConfig = defaults.BattleTypeConfig;
            MapConfig = defaults.MapConfig;
            SiegeMachineConfig = defaults.SiegeMachineConfig;
        }

        private static TeamConfig CreatePlayerTeam()
        {
            string playerCharacterId =
                Hero.MainHero?.CharacterObject?.StringId;
            return new TeamConfig
            {
                PrimaryParty = new PartyConfig
                {
                    UseCustomBanner = false,
                    IsInArmy = true,
                    HasHeroes = false,
                    Heroes = new TroopGroupConfig(),
                    Troops = new TroopGroupConfig()
                },
                PlayerCharacter = string.IsNullOrEmpty(playerCharacterId)
                    ? CharacterConfig.Create()
                    : CharacterConfig.Create(playerCharacterId)
            };
        }

        private static TeamConfig CreateEnemyTeam()
        {
            return new TeamConfig
            {
                PrimaryParty = new PartyConfig
                {
                    BannerKey =
                        "11.12.12.4345.4345.768.768.1.0.0.462.13.13.512.512.769.764.1.0.0",
                    UseCustomBanner = false,
                    IsInArmy = true,
                    HasHeroes = true,
                    Heroes = new TroopGroupConfig(true)
                    {
                        Troops = new List<TroopConfig>
                        {
                            new TroopConfig("lord_2_2", 1),
                            new TroopConfig("lord_2_4", 1),
                            new TroopConfig("lord_2_111", 1)
                        }
                    },
                    Troops = new TroopGroupConfig
                    {
                        Troops = new List<TroopConfig>
                        {
                            new TroopConfig("sturgian_veteran_warrior", 40),
                            new TroopConfig("sturgian_spearman", 40),
                            new TroopConfig("sturgian_militia_archer", 30),
                            new TroopConfig("sturgian_archer", 30),
                            new TroopConfig("druzhinnik", 30),
                            new TroopConfig("sturgian_horse_raider", 30)
                        }
                    }
                },
                PlayerCharacter =
                    CharacterConfig.Create("sturgian_veteran_warrior")
            };
        }

        public static BattleConfig Deserialize(out bool recoveredFromError)
        {
            string filePath = Path.Combine(
                SaveFolderPath(),
                CurrentConfigFileName);
            if (!File.Exists(filePath))
            {
                recoveredFromError = false;
                return CreateDefault();
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(BattleConfig));
                using TextReader reader = new StreamReader(filePath);
                BattleConfig result = Normalize(
                    (BattleConfig)serializer.Deserialize(reader));
                recoveredFromError = false;
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                recoveredFromError = true;
                return CreateDefault();
            }
        }

        private static void RemoveUnavailableCharacters(TeamConfig team)
        {
            if (team == null)
                return;

            RemoveUnavailableCharacters(team.PrimaryParty);
            if (team.PlayerCharacter?.CharacterObject == null)
            {
                CharacterConfig defaultCharacter =
                    GetParties(team)
                        .SelectMany(party => party.Troops.Troops)
                        .Select(troop => troop?.Character)
                        .FirstOrDefault(character =>
                            character?.CharacterObject != null)
                    ?? GetParties(team)
                        .SelectMany(party => party.Heroes.Troops)
                        .Select(troop => troop?.Character)
                        .FirstOrDefault(character =>
                            character?.CharacterObject != null);
                CharacterObject fallbackCharacter =
                    Game.Current?.ObjectManager
                        .GetObjectTypeList<BasicCharacterObject>()
                        .OfType<CharacterObject>()
                        .FirstOrDefault(character =>
                            !character.IsHero
                            && !character.IsTemplate
                            && !character.IsChildTemplate);
                team.PlayerCharacter =
                    defaultCharacter?.Clone()
                    ?? (fallbackCharacter == null
                        ? CharacterConfig.Create()
                        : CharacterConfig.Create(fallbackCharacter.StringId));
            }
            team.AlliedParties?.RemoveAll(party => party == null);
            if (team.AlliedParties == null)
                return;
            foreach (PartyConfig party in team.AlliedParties)
                RemoveUnavailableCharacters(party);
        }

        private static void RemoveUnavailableCharacters(PartyConfig party)
        {
            if (party == null)
                return;

            party.Normalize();
            party.Heroes.Troops.RemoveAll(IsCharacterUnavailable);
            if (party.Heroes.Troops.Count == 0)
                party.HasHeroes = false;
            party.Troops.Troops.RemoveAll(IsCharacterUnavailable);
        }

        private static bool IsCharacterUnavailable(TroopConfig troop)
        {
            return troop?.Character?.CharacterObject == null;
        }

        public bool Serialize()
        {
            return SerializeTo(
                Path.Combine(SaveFolderPath(), CurrentConfigFileName));
        }

        public bool Serialize(string configurationName)
        {
            return SerializeTo(GetNamedConfigPath(configurationName));
        }

        public static bool TryDeserialize(
            string configurationName,
            out BattleConfig config)
        {
            try
            {
                XmlSerializer serializer =
                    new XmlSerializer(typeof(BattleConfig));
                using TextReader reader =
                    new StreamReader(GetNamedConfigPath(configurationName));
                config = Normalize(
                    (BattleConfig)serializer.Deserialize(reader));
                return true;
            }
            catch
            {
                config = null;
                return false;
            }
        }

        public static bool TryGetSavedConfigurationNames(
            out IReadOnlyList<string> names)
        {
            try
            {
                string folderPath = NamedConfigFolderPath();
                if (!Directory.Exists(folderPath))
                {
                    names = Array.Empty<string>();
                }
                else
                {
                    names = Directory.GetFiles(folderPath, "*.xml")
                        .Select(Path.GetFileNameWithoutExtension)
                        .OrderBy(name => name)
                        .ToList();
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                names = Array.Empty<string>();
                return false;
            }
        }

        public static Tuple<bool, string> ValidateConfigurationName(
            string configurationName)
        {
            if (string.IsNullOrWhiteSpace(configurationName))
                return Tuple.Create(
                    false,
                    GameTexts.FindText(
                        "str_ebt_configuration_name_required").ToString());
            if (configurationName.IndexOfAny(
                    Path.GetInvalidFileNameChars()) >= 0)
                return Tuple.Create(
                    false,
                    GameTexts.FindText(
                        "str_ebt_configuration_name_invalid").ToString());

            return Tuple.Create(true, string.Empty);
        }

        private bool SerializeTo(string filePath)
        {
            string temporaryPath = null;
            try
            {
                string folderPath =
                    Path.GetDirectoryName(filePath) ?? SaveFolderPath();
                Directory.CreateDirectory(folderPath);
                temporaryPath = Path.Combine(
                    folderPath,
                    Path.GetRandomFileName());
                using (TextWriter writer = new StreamWriter(temporaryPath))
                {
                    XmlSerializer serializer =
                        new XmlSerializer(typeof(BattleConfig));
                    serializer.Serialize(writer, this);
                }

                if (File.Exists(filePath))
                    File.Replace(temporaryPath, filePath, null);
                else
                    File.Move(temporaryPath, filePath);
                temporaryPath = null;
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            finally
            {
                if (temporaryPath != null)
                {
                    try
                    {
                        File.Delete(temporaryPath);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        private static BattleConfig Normalize(BattleConfig result)
        {
            if (result == null)
                throw new InvalidDataException(
                    "The battle configuration is empty.");

            result.PlayerTeamConfig =
                result.PlayerTeamConfig ?? new TeamConfig();
            result.EnemyTeamConfig =
                result.EnemyTeamConfig ?? new TeamConfig();
            result.BattleTypeConfig =
                result.BattleTypeConfig ?? new BattleTypeConfig();
            result.MapConfig = result.MapConfig ?? new MapConfig();
            result.SiegeMachineConfig =
                result.SiegeMachineConfig ?? new SiegeMachineConfig();
            result.PlayerTeamConfig.NormalizeAfterDeserialize();
            result.EnemyTeamConfig.NormalizeAfterDeserialize();
            result.SiegeMachineConfig.NormalizeAfterDeserialize();
            NormalizeBattleType(result.BattleTypeConfig);
            RemoveUnavailableCharacters(result.PlayerTeamConfig);
            RemoveUnavailableCharacters(result.EnemyTeamConfig);
            result.NormalizeCharacterGroups();
            return result;
        }

        private static void NormalizeBattleType(BattleTypeConfig config)
        {
            if (!Enum.IsDefined(typeof(BattleType), config.BattleType))
                config.BattleType = BattleType.Battle;
            if (!Enum.IsDefined(typeof(PlayerType), config.PlayerType))
                config.PlayerType = PlayerType.Commander;
            if (config.PlayerSide != BattleSideEnum.Attacker
                && config.PlayerSide != BattleSideEnum.Defender)
            {
                config.PlayerSide = BattleSideEnum.Attacker;
            }
            if (!Enum.IsDefined(
                    typeof(EquipmentModifierType),
                    config.EquipmentModifierType))
            {
                config.EquipmentModifierType = EquipmentModifierType.Random;
            }
        }

        private static IEnumerable<PartyConfig> GetParties(TeamConfig team)
        {
            if (team?.PrimaryParty != null)
                yield return team.PrimaryParty;
            if (team?.AlliedParties == null)
                yield break;

            foreach (PartyConfig party in team.AlliedParties)
            {
                if (party != null)
                    yield return party;
            }
        }

        public void NormalizeCharacterGroups()
        {
            NormalizeCharacterGroups(PlayerTeamConfig);
            NormalizeCharacterGroups(EnemyTeamConfig);
            HashSet<BasicCharacterObject> playerHeroes =
                BattleTypeConfig.PlayerType == PlayerType.None
                    ? new HashSet<BasicCharacterObject>()
                    : new[]
                        {
                            PlayerTeamConfig?.PlayerCharacter?.CharacterObject,
                            EnemyTeamConfig?.PlayerCharacter?.CharacterObject
                        }
                        .OfType<CharacterObject>()
                        .Where(character => character.IsHero)
                        .Cast<BasicCharacterObject>()
                        .ToHashSet();
            RemovePlayerCharactersFromHeroes(
                PlayerTeamConfig,
                playerHeroes);
            RemovePlayerCharactersFromHeroes(
                EnemyTeamConfig,
                playerHeroes);
        }

        private static void NormalizeCharacterGroups(TeamConfig team)
        {
            if (team == null)
                return;

            NormalizeCharacterGroups(team.PrimaryParty);
            if (team.AlliedParties == null)
                return;

            foreach (PartyConfig party in team.AlliedParties)
                NormalizeCharacterGroups(party);
        }

        private static void NormalizeCharacterGroups(PartyConfig party)
        {
            if (party == null)
                return;

            party.Normalize();
            bool hadEnabledHeroes = party.HasHeroes;
            List<TroopConfig> heroesInTroops = party.Troops.Troops
                .Where(IsHero)
                .ToList();
            List<TroopConfig> nonHeroesInHeroes = party.Heroes.Troops
                .Where(IsNonHero)
                .ToList();

            party.Troops.Troops.RemoveAll(IsHero);
            party.Heroes.Troops.RemoveAll(IsNonHero);
            party.Heroes.Troops.AddRange(heroesInTroops);
            party.Troops.Troops.AddRange(nonHeroesInHeroes);
            party.HasHeroes =
                party.Heroes.Troops.Count > 0
                && (hadEnabledHeroes || heroesInTroops.Count > 0);
        }

        private static bool IsHero(TroopConfig troop)
        {
            return troop?.Character?.CharacterObject is CharacterObject character
                   && character.IsHero;
        }

        private static bool IsNonHero(TroopConfig troop)
        {
            return troop?.Character?.CharacterObject is CharacterObject character
                   && !character.IsHero;
        }

        private static void RemovePlayerCharactersFromHeroes(
            TeamConfig team,
            HashSet<BasicCharacterObject> playerHeroes)
        {
            if (team == null || playerHeroes.Count == 0)
                return;

            RemoveHeroes(team.PrimaryParty, playerHeroes);
            if (team.AlliedParties == null)
                return;

            foreach (PartyConfig party in team.AlliedParties)
                RemoveHeroes(party, playerHeroes);
        }

        private static void RemoveHeroes(
            PartyConfig party,
            HashSet<BasicCharacterObject> playerHeroes)
        {
            if (party == null)
                return;

            party.Heroes.Troops.RemoveAll(troop =>
                playerHeroes.Contains(
                    troop?.Character?.CharacterObject));
            if (party.Heroes.Troops.Count == 0)
                party.HasHeroes = false;
        }

        private static string GetNamedConfigPath(string configurationName)
        {
            Tuple<bool, string> validation =
                ValidateConfigurationName(configurationName);
            if (!validation.Item1)
                throw new ArgumentException(
                    validation.Item2,
                    nameof(configurationName));

            return Path.Combine(
                NamedConfigFolderPath(),
                configurationName.Trim() + ".xml");
        }

        private static string NamedConfigFolderPath()
        {
            return Path.Combine(SaveFolderPath(), NamedConfigFolderName);
        }

        private static string SaveFolderPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "Mount and Blade II Bannerlord", "Configs", "EnhancedBattleTest");

        }
    }
}
