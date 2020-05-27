using System;
using System.Collections.Generic;
using System.IO;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class EnhancedBattleTestSubModule : MBSubModuleBase
    {
        public static EnhancedBattleTestSubModule Instance { get; private set; }

        public static string ModuleFolderName = "EnhancedBattleTest";

        public static string ModuleFolderPath = Path.Combine(BasePath.Name, "Modules", ModuleFolderName);

        public static bool IsMultiplayer;

        public CharacterSelectionLayer CharacterSelectionLayer;

        public event Action<CharacterSelectionData> OnSelectCharacter;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            EnhancedBattleTestSubModule.Instance = this;
            Module.CurrentModule.AddInitialStateOption(new InitialStateOption("EBTMultiplayerTest",
                new TextObject("{=EnhancedBattleTest_multiplayerbattleoption}Multiplayer Battle Test"), 3,
                () =>
                {
                    IsMultiplayer = true;
                    MBGameManager.StartNewGame(new EnhancedBattleTestGameManager<EnhancedBattleTestMultiplayerGame>());
                }, false));
        }

        protected override void OnSubModuleUnloaded()
        {
            EnhancedBattleTestSubModule.Instance = (EnhancedBattleTestSubModule)null;
            base.OnSubModuleUnloaded();
        }

        public override void OnGameInitializationFinished(Game game)
        {
            base.OnGameInitializationFinished(game);

            if (game.GameType is EnhancedBattleTestMultiplayerGame || game.GameType is EnhancedBattleTestSingleplayerGame)
            {
                var collection = new MPCharacterCollection();
                collection.Initialize();
                CharacterSelectionLayer = new CharacterSelectionLayer();
                CharacterSelectionLayer.Initialize(collection, IsMultiplayer);
            }
        }

        public override void OnGameEnd(Game game)
        {
            base.OnGameEnd(game);

            if (game.GameType is EnhancedBattleTestMultiplayerGame || game.GameType is EnhancedBattleTestSingleplayerGame)
                CharacterSelectionLayer.OnFinalize();
        }

        public void SelectCharacter(CharacterSelectionData data)
        {
            OnSelectCharacter?.Invoke(data);
        }
    }
}