using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace EnhancedBattleTest.Config
{
    public class BattleConfig
    {
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
            return new TeamConfig
            {
                PrimaryParty = new PartyConfig
                {
                    BannerKey =
                        "11.14.14.1536.1536.768.768.1.0.0.160.0.15.512.512.769.764.1.0.0",
                    UseCustomBanner = false,
                    IsInArmy = true,
                    HasGeneral = true,
                    Generals = new TroopGroupConfig(true)
                    {
                        Troops = new List<TroopConfig>
                        {
                            new TroopConfig("lord_4_6", 1),
                            new TroopConfig("lord_4_1", 1),
                            new TroopConfig("lord_4_25", 1)
                        }
                    },
                    Troops = new TroopGroupConfig
                    {
                        Troops = new List<TroopConfig>
                        {
                            new TroopConfig("vlandian_infantry", 40),
                            new TroopConfig("vlandian_billman", 40),
                            new TroopConfig("vlandian_sharpshooter", 30),
                            new TroopConfig("vlandian_militia_archer", 30),
                            new TroopConfig("vlandian_banner_knight", 30),
                            new TroopConfig("vlandian_champion", 30)
                        }
                    }
                },
                PlayerCharacter = CharacterConfig.Create("vlandian_infantry")
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
                    HasGeneral = true,
                    Generals = new TroopGroupConfig(true)
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
                var filePath = Path.Combine(SaveFolderPath(), "spconfig-v2.xml");
                using TextReader reader = new StreamReader(filePath);
                var result = (BattleConfig)serializer.Deserialize(reader);
                result.PlayerTeamConfig?.NormalizeAfterDeserialize();
                result.EnemyTeamConfig?.NormalizeAfterDeserialize();
                RemoveUnavailableCharacters(result.PlayerTeamConfig);
                RemoveUnavailableCharacters(result.EnemyTeamConfig);
                return result;
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
                    ?? team.PrimaryParty.Generals.Troops
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
            party.Generals.Troops.RemoveAll(IsCharacterUnavailable);
            if (party.Generals.Troops.Count == 0)
                party.HasGeneral = false;
            party.Troops.Troops.RemoveAll(IsCharacterUnavailable);
        }

        private static bool IsCharacterUnavailable(TroopConfig troop)
        {
            return troop?.Character?.CharacterObject == null;
        }

        public void Serialize()
        {
            try
            {
                EnsureSaveDirectory();
                var filePath = Path.Combine(SaveFolderPath(), "spconfig-v2.xml");
                using TextWriter writer = new StreamWriter(filePath);
                XmlSerializer serializer = new XmlSerializer(typeof(BattleConfig));
                serializer.Serialize(writer, this);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static string SaveFolderPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "Mount and Blade II Bannerlord", "Configs", "EnhancedBattleTest");

        }
        private void EnsureSaveDirectory()
        {
            Directory.CreateDirectory(SaveFolderPath());
        }
    }
}
