using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace EnhancedBattleTest
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
