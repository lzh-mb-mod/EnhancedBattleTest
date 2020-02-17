using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Core;

namespace Modbed
{
    class EnhancedBattleTestUtility
    {
        public static void DisplayMessage(string msg)
        {
            InformationManager.DisplayMessage(new InformationMessage(new TaleWorlds.Localization.TextObject(msg, null).ToString()));
        }
    }
}
