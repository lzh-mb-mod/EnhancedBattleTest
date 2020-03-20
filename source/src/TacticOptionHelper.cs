using System;
using System.Collections.Generic;
using System.Text;
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
                Utility.DisplayMessage("Add tactic " + tacticOptionEnum.ToString() + " for " + team.Side.ToString());
            team.AddTacticOption((TacticComponent) Activator.CreateInstance(GetTacticComponentType(tacticOptionEnum),
                new Object[] {team}));

            team.ExpireAIQuerySystem();
            team.ResetTactic();
        }

        public static void RemoveTacticComponent(Team team, TacticOptionEnum tacticOptionEnum, bool displayMessage = false)
        {
            if (displayMessage) 
                Utility.DisplayMessage("Remove tactic " + tacticOptionEnum.ToString() + " for " + team.Side.ToString());
            team.RemoveTacticOption(GetTacticComponentType(tacticOptionEnum));

            team.ExpireAIQuerySystem();
            team.ResetTactic();
        }
    }
}

