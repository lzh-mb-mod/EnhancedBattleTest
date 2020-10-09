using System;
using EnhancedBattleTest.Config;
using TaleWorlds.Core;

namespace EnhancedBattleTest.BannerEditor
{
    public class BannerEditorState : GameState
    {
        public static TeamConfig Config;

        public static Action OnDone;

        public static BasicCharacterObject Character => Config.General.CharacterObject;

        public static Banner Banner => Config.Banner;

        public override bool IsMenuState => true;


    }
}
