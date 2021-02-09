using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using EnhancedBattleTest.Data;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.CustomBattle;

namespace EnhancedBattleTest.GameMode
{
    public class EnhancedBattleTestState : GameState
    {
        private const TerrainType DefaultTerrain = TerrainType.Plain;
        private const ForestDensity DefaultForestDensity = ForestDensity.None;

        public override bool IsMusicMenuState => true;
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
                var customGame = new CustomGame();
                customGame.LoadCustomBattleScenes(BasePath.Name +
                                                  "Modules/CustomBattle/ModuleData/custom_battle_scenes.xml");
                Scenes.AddRange(customGame.CustomBattleScenes.Select(data => new SceneData(data)));
                GameSceneDataManager.Instance.LoadSPBattleScenes(BasePath.Name + "Modules/Sandbox/ModuleData/sp_battle_scenes.xml");
                Scenes.AddRange(GameSceneDataManager.Instance.SingleplayerBattleScenes.Select(data => new SceneData(data)));
                Scenes = Scenes.Distinct(new SceneDataComparer()).ToList();
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
