using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace EnhancedBattleTest
{
    public class EnhancedBattleTestState : GameState
    {
        private const TerrainType DefaultTerrain = TerrainType.Plain;
        private const ForestDensity DefaultForestDensity = ForestDensity.None;

        public override bool IsMenuState => true;
        public List<SceneData> Scenes { get; private set; } = new List<SceneData>();

        public EnhancedBattleTestState()
        {
            InitializeScenes();
        }

        private void InitializeScenes()
        {
            var filePath = Path.Combine(EnhancedBattleTestSubModule.ModuleFolderPath,
                "ModuleData", "enhanced_battle_test_scenes.xml");
            XmlSerializer serializer = new XmlSerializer(typeof(List<SerializedSceneData>));
            try
            {
                using TextReader reader = new StreamReader(filePath);
                Scenes = ((List<SerializedSceneData>)serializer.Deserialize(reader))
                    .Select(data => new SceneData(data))
                    .ToList();
            }
            catch
            {
                Scenes = new List<SceneData> { new SceneData() };
            }
            //finally
            //{
            //    using TextWriter writer = new StreamWriter(filePath);
            //    serializer.Serialize(writer, _scenes.Select(data => new SerializedSceneData(data)).ToList());
            //}
        }
    }
}
