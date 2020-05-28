using System;
using System.IO;
using System.Xml.Serialization;

namespace EnhancedBattleTest
{
    public class BattleConfig
    {
        public TeamConfig PlayerTeamConfig = new TeamConfig();
        public TeamConfig EnemyTeamConfig = new TeamConfig();
        public BattleTypeConfig BattleTypeConfig = new BattleTypeConfig();
        public MapConfig MapConfig = new MapConfig();
        public SiegeMachineConfig SiegeMachineConfig = new SiegeMachineConfig();

        public BattleConfig()
        { }

        public BattleConfig(bool isMultiplayer)
        {
            PlayerTeamConfig = new TeamConfig(isMultiplayer);
            EnemyTeamConfig = new TeamConfig(isMultiplayer);
        }

        public static BattleConfig Deserialize(bool isMultiplayer)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(BattleConfig));
                var filePath = Path.Combine(SaveFolderPath(), isMultiplayer ? "mpconfig.xml" : "spconfig.xml");
                using TextReader reader = new StreamReader(filePath);
                return (BattleConfig) serializer.Deserialize(reader);
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
