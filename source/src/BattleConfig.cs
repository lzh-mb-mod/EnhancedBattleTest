using System;
using System.IO;
using System.Xml.Serialization;

namespace EnhancedBattleTest
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

        public BattleConfig(bool isMultiplayer)
        {
            PlayerTeamConfig = new TeamConfig(isMultiplayer)
            {
                BannerKey = "11.14.14.1536.1536.768.768.1.0.0.160.0.15.512.512.769.764.1.0.0"
            };
            EnemyTeamConfig = new TeamConfig(isMultiplayer)
            {
                BannerKey = "11.12.12.4345.4345.768.768.1.0.0.462.13.13.512.512.769.764.1.0.0"
            };
            if (!isMultiplayer)
            {
                PlayerTeamConfig.General = CharacterConfig.Create(false, "lord_4_6", 1);
                PlayerTeamConfig.HasGeneral = true;
                PlayerTeamConfig.Troops = new TroopGroupConfig
                {
                    Troops = new TroopConfig[8]
                    {
                        new TroopConfig(false, "vlandian_infantry", 40, 0),
                        new TroopConfig(false, "vlandian_billman", 40, 0),
                        new TroopConfig(false, "vlandian_sharpshooter", 30, 0),
                        new TroopConfig(false, "vlandian_militia_archer", 30, 0),
                        new TroopConfig(false, "vlandian_banner_knight", 30, 0),
                        new TroopConfig(false, "vlandian_champion", 0),
                        new TroopConfig(false, "lord_4_1", 0),
                        new TroopConfig(false, "lord_4_25", 0),
                    }
                };
                EnemyTeamConfig.General = CharacterConfig.Create(false, "lord_2_2", 1);
                EnemyTeamConfig.HasGeneral = true;
                EnemyTeamConfig.Troops = new TroopGroupConfig
                {
                    Troops = new TroopConfig[8]
                    {
                        new TroopConfig(false, "sturgian_veteran_warrior", 40, 0),
                        new TroopConfig(false, "sturgian_spearman", 40, 0),
                        new TroopConfig(false, "sturgian_militia_archer", 30, 0),
                        new TroopConfig(false, "sturgian_archer", 30, 0),
                        new TroopConfig(false, "druzhinnik", 30, 0),
                        new TroopConfig(false, "sturgian_horse_raider", 0),
                        new TroopConfig(false, "lord_2_4", 0),
                        new TroopConfig(false, "lord_2_111", 0),
                    }
                };
            }
        }

        public static BattleConfig Deserialize(bool isMultiplayer)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(BattleConfig));
                var filePath = Path.Combine(SaveFolderPath(), isMultiplayer ? "mpconfig.xml" : "spconfig.xml");
                using TextReader reader = new StreamReader(filePath);
                return (BattleConfig)serializer.Deserialize(reader);
            }
            catch
            {
                return new BattleConfig(isMultiplayer);
            }
        }

        public void Serialize(bool isMultiplayer)
        {
            try
            {
                EnsureSaveDirectory();
                var filePath = Path.Combine(SaveFolderPath(), isMultiplayer ? "mpconfig.xml" : "spconfig.xml");
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
