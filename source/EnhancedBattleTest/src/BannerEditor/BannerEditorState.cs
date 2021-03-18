using System;
using EnhancedBattleTest.Config;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace EnhancedBattleTest.BannerEditor
{
    public class BannerEditorState : GameState
    {
        public static TeamConfig Config;

        public static Action OnDone;

        public static BasicCharacterObject Character => Config.Generals.Troops?[0].Character.CharacterObject ??
                                                        MBObjectManager.Instance.GetObject<CharacterObject>(character =>
                                                            true);

        public static Banner Banner => Config.Banner;

        public override bool IsMenuState => true;


    }
}
