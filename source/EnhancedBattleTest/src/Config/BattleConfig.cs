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

        public static BattleConfig Deserialize()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(BattleConfig));
                var filePath = Path.Combine(
                    SaveFolderPath(),
                    CurrentConfigFileName);
                using TextReader reader = new StreamReader(filePath);
                return Normalize((BattleConfig)serializer.Deserialize(reader));
            }

            catch
            {
                var result = CreateDefault();
                result.Serialize();
                return result;
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
                    team.PrimaryParty.Troops.Troops
                    .Select(troop => troop?.Character)
                    .FirstOrDefault(character => character?.CharacterObject != null)
                    ?? team.PrimaryParty.Heroes.Troops
                        .Select(troop => troop?.Character)
                        .FirstOrDefault(character => character?.CharacterObject != null);
                team.PlayerCharacter =
                    defaultCharacter?.Clone() ?? CharacterConfig.Create();
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

        public void Serialize()
        {
            SerializeTo(Path.Combine(SaveFolderPath(), CurrentConfigFileName));
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

        public static IReadOnlyList<string> GetSavedConfigurationNames()
        {
            string folderPath = NamedConfigFolderPath();
            if (!Directory.Exists(folderPath))
                return Array.Empty<string>();

            return Directory.GetFiles(folderPath, "*.xml")
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(name => name)
                .ToList();
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
            try
            {
                Directory.CreateDirectory(
                    Path.GetDirectoryName(filePath)
                    ?? SaveFolderPath());
                using TextWriter writer = new StreamWriter(filePath);
                XmlSerializer serializer = new XmlSerializer(typeof(BattleConfig));
                serializer.Serialize(writer, this);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        private static BattleConfig Normalize(BattleConfig result)
        {
            result.PlayerTeamConfig?.NormalizeAfterDeserialize();
            result.EnemyTeamConfig?.NormalizeAfterDeserialize();
            RemoveUnavailableCharacters(result.PlayerTeamConfig);
            RemoveUnavailableCharacters(result.EnemyTeamConfig);
            result.NormalizeCharacterGroups();
            return result;
        }

        public void NormalizeCharacterGroups()
        {
            NormalizeCharacterGroups(PlayerTeamConfig);
            NormalizeCharacterGroups(EnemyTeamConfig);
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
