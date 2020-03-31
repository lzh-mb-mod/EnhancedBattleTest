using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public enum TacticOptionEnum
    {
        // field battle
        Charge,
        FullScaleAttack,
        DefensiveEngagement,
        DefensiveLine,
        DefensiveRing,
        HoldTheHill,
        HoldChokePoint,
        ArchersOnTheHill,
        RangedHarassmentOffensive,
        FrontalCavalryCharge,
        CoordinatedRetreat,

        // siege
        BreachWalls,
        DefendCastle,
    }

    public class TacticOptionHelper
    {
        public static Type GetTacticComponentType(TacticOptionEnum tacticOptionEnum)
        {
            switch (tacticOptionEnum)
            {
                case TacticOptionEnum.Charge:
                    return typeof(TacticCharge);
                case TacticOptionEnum.FullScaleAttack:
                    return typeof(TacticFullScaleAttack);
                case TacticOptionEnum.DefensiveEngagement:
                    return typeof(TacticDefensiveEngagement);
                case TacticOptionEnum.DefensiveLine:
                    return typeof(TacticDefensiveLine);
                case TacticOptionEnum.DefensiveRing:
                    return typeof(TacticDefensiveRing);
                case TacticOptionEnum.HoldTheHill:
                    return typeof(TacticHoldTheHill);
                case TacticOptionEnum.HoldChokePoint:
                    return typeof(TacticHoldChokePoint);
                case TacticOptionEnum.ArchersOnTheHill:
                    return typeof(TacticArchersOnTheHill);
                case TacticOptionEnum.RangedHarassmentOffensive:
                    return typeof(TacticRangedHarrassmentOffensive);
                case TacticOptionEnum.FrontalCavalryCharge:
                    return typeof(TacticFrontalCavalryCharge);
                case TacticOptionEnum.CoordinatedRetreat:
                    return typeof(TacticCoordinatedRetreat);
                case TacticOptionEnum.BreachWalls:
                    return typeof(TacticBreachWalls);
                case TacticOptionEnum.DefendCastle:
                    return typeof(TacticDefendCastle);
                default:
                    return null;
            }
        }

        public static void AddTacticComponent(Team team, TacticOptionEnum tacticOptionEnum, bool displayMessage = false)
        {
            if (displayMessage)
            {
                MBTextManager.SetTextVariable("CurrentTacticOption", GameTexts.FindText("str_tactic_option", tacticOptionEnum.ToString()));
                MBTextManager.SetTextVariable("CurrentSide", GameTexts.FindText("str_side", team.Side.ToString()));
                Utility.DisplayLocalizedText("str_add_tactic");
            }
            team.AddTacticOption((TacticComponent) Activator.CreateInstance(GetTacticComponentType(tacticOptionEnum),
                new Object[] {team}));

            team.ExpireAIQuerySystem();
            team.ResetTactic();
        }

        public static void RemoveTacticComponent(Team team, TacticOptionEnum tacticOptionEnum, bool displayMessage = false)
        {
            if (displayMessage)
            {
                MBTextManager.SetTextVariable("CurrentTacticOption", GameTexts.FindText("str_tactic_option", tacticOptionEnum.ToString()));
                MBTextManager.SetTextVariable("CurrentSide", GameTexts.FindText("str_side", team.Side.ToString()));
                Utility.DisplayLocalizedText("str_remove_tactic");
            }
            team.RemoveTacticOption(GetTacticComponentType(tacticOptionEnum));

            team.ExpireAIQuerySystem();
            team.ResetTactic();
        }
    }
}

