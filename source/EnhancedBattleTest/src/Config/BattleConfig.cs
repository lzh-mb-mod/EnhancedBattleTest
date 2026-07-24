using System;
using System.Collections.Generic;
using System.IO;
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
                BannerKey =
                    "11.14.14.1536.1536.768.768.1.0.0.160.0.15.512.512.769.764.1.0.0",
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
                TroopGroups = new[]
                {
                    new TroopGroupConfig
                    {
                        Troops = new List<TroopConfig>
                        {
                            new TroopConfig("vlandian_infantry", 40),
                            new TroopConfig("vlandian_billman", 40)
                        }
                    },
                    new TroopGroupConfig
                    {
                        Troops = new List<TroopConfig>
                        {
                            new TroopConfig("vlandian_sharpshooter", 30),
                            new TroopConfig("vlandian_militia_archer", 30)
                        }
                    },
                    new TroopGroupConfig
                    {
                        Troops = new List<TroopConfig>
                        {
                            new TroopConfig("vlandian_banner_knight", 30),
                            new TroopConfig("vlandian_champion", 30)
                        }
                    },
                    new TroopGroupConfig(),
                    new TroopGroupConfig(),
                    new TroopGroupConfig(),
                    new TroopGroupConfig(),
                    new TroopGroupConfig()
                }
            };
        }

        private static TeamConfig CreateEnemyTeam()
        {
            return new TeamConfig
            {
                BannerKey =
                    "11.12.12.4345.4345.768.768.1.0.0.462.13.13.512.512.769.764.1.0.0",
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
                TroopGroups = new[]
                {
                    new TroopGroupConfig
                    {
                        Troops = new List<TroopConfig>
                        {
                            new TroopConfig("sturgian_veteran_warrior", 40),
                            new TroopConfig("sturgian_spearman", 40)
                        }
                    },
                    new TroopGroupConfig
                    {
                        Troops = new List<TroopConfig>
                        {
                            new TroopConfig("sturgian_militia_archer", 30),
                            new TroopConfig("sturgian_archer", 30)
                        }
                    },
                    new TroopGroupConfig
                    {
                        Troops = new List<TroopConfig>
                        {
                            new TroopConfig("druzhinnik", 30),
                            new TroopConfig("sturgian_horse_raider", 30)
                        }
                    },
                    new TroopGroupConfig(),
                    new TroopGroupConfig(),
                    new TroopGroupConfig(),
                    new TroopGroupConfig(),
                    new TroopGroupConfig()
                }
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

            team.Generals?.Troops?.RemoveAll(IsCharacterUnavailable);
            if (team.Generals?.Troops == null
                || team.Generals.Troops.Count == 0)
                team.HasGeneral = false;

            if (team.TroopGroups == null)
                return;
            foreach (TroopGroupConfig group in team.TroopGroups)
                group?.Troops?.RemoveAll(IsCharacterUnavailable);
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
