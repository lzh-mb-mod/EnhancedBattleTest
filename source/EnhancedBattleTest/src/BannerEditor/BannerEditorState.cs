using System;
using System.Linq;
using EnhancedBattleTest.Config;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace EnhancedBattleTest.BannerEditor
{
    public class BannerEditorState : GameState
    {
        public static PartyConfig Config;

        public static Action OnDone;

        public static BasicCharacterObject PreferredCharacter;

        public static BasicCharacterObject Character =>
            PreferredCharacter
            ?? Config.Heroes.Troops
                .Select(troop => troop?.Character?.CharacterObject)
                .FirstOrDefault(character => character != null)
            ?? Config.Troops.Troops
                .Select(troop => troop?.Character?.CharacterObject)
                .FirstOrDefault(character => character != null)
            ?? MBObjectManager.Instance.GetObject<CharacterObject>(character => true);

        public static Banner Banner
        {
            get
            {
                try
                {
                    return new Banner(Config.BannerKey);
                }
                catch
                {
                    return new Banner(PartyConfig.DefaultBannerKey);
                }
            }
        }

        public override bool IsMenuState => true;

        public static void Clear()
        {
            Config = null;
            OnDone = null;
            PreferredCharacter = null;
        }
    }
}
