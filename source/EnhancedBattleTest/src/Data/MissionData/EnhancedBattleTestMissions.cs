using EnhancedBattleTest.Config;
using EnhancedBattleTest.Multiplayer.Data.MissionData;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.Data.MissionData
{
    [MissionManager]
    public static class EnhancedBattleTestMissions
    {
        //private static AtmosphereInfo CreateAtmosphereInfoForMission(string seasonString = "", int timeOfDay = 6)
        //{
        //    Dictionary<int, string> strArray = new Dictionary<int, string>
        //    {
        //        {6, "TOD_06_00_SemiCloudy"},
        //        {12, "TOD_12_00_SemiCloudy"},
        //        {15, "TOD_04_00_SemiCloudy"},
        //        {18, "TOD_03_00_SemiCloudy"},
        //        {22, "TOD_01_00_SemiCloudy"}
        //    };
        //    string str = "field_battle";
        //    strArray.TryGetValue(timeOfDay, out str);
        //    Dictionary<string, int> dictionary = new Dictionary<string, int>
        //    {
        //        {"spring", 0}, {"summer", 1}, {"fall", 2}, {"winter", 3}
        //    };
        //    dictionary.TryGetValue(seasonString, out var num);
        //    return new AtmosphereInfo
        //    {
        //        AtmosphereName = str,
        //        TimeInfo = new TimeInformation { Season = num, TimeOfDay = timeOfDay }
        //    };
        //}

        public static Mission OpenMission(BattleConfig config, string mapName)
        {
            //TODO: implement in multiplayer mode
            if (EnhancedBattleTestSubModule.IsMultiplayer)
            {
                return OpenMultiplayerMission(config, mapName);
            }

            return null;
        }

        public static Mission OpenMultiplayerMission(BattleConfig config, string map)
        {
            var playerCulture = Utility.GetCulture(config.PlayerSideConfig.Teams[0]);
            var playerSide = config.BattleTypeConfig.PlayerSide;

            var enemyCulture = Utility.GetCulture(config.EnemySideConfig.Teams[0]);
            var enemySide = playerSide.GetOppositeSide();
            MPCombatant[] parties = new MPCombatant[2]
            {
                MPCombatant.CreateParty(playerSide, playerCulture, config.PlayerSideConfig.Teams[0], true),
                MPCombatant.CreateParty(enemySide, enemyCulture, config.EnemySideConfig.Teams[0], false)
            };

            return OpenMission(parties[0], parties[1], config, map);
        }

        public static Mission OpenMission(IEnhancedBattleTestCombatant playerParty,
            IEnhancedBattleTestCombatant enemyParty, BattleConfig config, string map)
        {
            if (config.BattleTypeConfig.BattleType == BattleType.Siege)
            {
                var attackerSiegeWeaponCount = GetSiegeWeaponCount(config.SiegeMachineConfig.AttackerMeleeMachines)
                    .Union(GetSiegeWeaponCount(config.SiegeMachineConfig.AttackerRangedMachines))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
                var defenderSiegeWeaponCount = GetSiegeWeaponCount(config.SiegeMachineConfig.DefenderMachines);

                int breachedWallCount = config.MapConfig.BreachedWallCount;
                var hitPointPercentages = new float[2];

                switch (breachedWallCount)
                {
                    case 0:
                        hitPointPercentages[0] = 1;
                        hitPointPercentages[1] = 1;
                        break;
                    case 1:
                        int i = MBRandom.RandomInt(2);
                        hitPointPercentages[i] = 0;
                        hitPointPercentages[1 - i] = 1;
                        break;
                    default:
                        hitPointPercentages[0] = 0;
                        hitPointPercentages[1] = 0;
                        break;
                }

                return OpenEnhancedBattleTestSiege(map, config, playerParty, enemyParty, hitPointPercentages,
                    attackerSiegeWeaponCount, defenderSiegeWeaponCount, false, false, config.MapConfig.TimeOfDay);
            }

            return OpenEnhancedBattleTestField(map, config, playerParty, enemyParty, config.MapConfig.TimeOfDay);
        }


        [MissionMethod]
        public static Mission OpenEnhancedBattleTestSiege(
            string scene,
            BattleConfig config,
            IEnhancedBattleTestCombatant playerParty,
            IEnhancedBattleTestCombatant enemyParty,
            float[] wallHitPointPercentages,
            Dictionary<SiegeEngineType, int> siegeWeaponsCountOfAttackers,
            Dictionary<SiegeEngineType, int> siegeWeaponsCountOfDefenders,
            bool isSallyOut = false,
            bool isReliefForceAttack = false,
            int timeOfDay = 6)
        {
            return null;
        }


        [MissionMethod]
        public static Mission OpenEnhancedBattleTestField(
            string scene,
            BattleConfig config,
            IEnhancedBattleTestCombatant playerParty,
            IEnhancedBattleTestCombatant enemyParty,
            int timeOfDay = 12)
        {
            return null;
        }

        private static IEnhancedBattleTestTroopSupplier CreateTroopSupplier(IEnhancedBattleTestCombatant combatant, bool isMultiplayer)
        {
            if (isMultiplayer)
            {
                return new MPTroopSupplier(combatant);
            }

            return null;
        }

        private static Dictionary<SiegeEngineType, int> GetSiegeWeaponCount(List<string> siegeWeaponIds)
        {
            Dictionary<SiegeEngineType, int> siegeWeaponsCount = new Dictionary<SiegeEngineType, int>();
            foreach (var attackerMeleeMachine in siegeWeaponIds)
            {
                var siegeWeapon = Utility.GetSiegeEngineType(attackerMeleeMachine);
                if (siegeWeapon == null)
                    continue;
                if (!siegeWeaponsCount.ContainsKey(siegeWeapon))
                    siegeWeaponsCount.Add(siegeWeapon, 0);
                siegeWeaponsCount[siegeWeapon]++;
            }

            return siegeWeaponsCount;
        }
        private static Type GetSiegeWeaponType(SiegeEngineType siegeWeaponType)
        {
            if (siegeWeaponType == DefaultSiegeEngineTypes.Ladder)
                return typeof(SiegeLadder);
            if (siegeWeaponType == DefaultSiegeEngineTypes.Ballista)
                return typeof(Ballista);
            if (siegeWeaponType == DefaultSiegeEngineTypes.FireBallista)
                return typeof(FireBallista);
            if (siegeWeaponType == DefaultSiegeEngineTypes.Ram || siegeWeaponType == DefaultSiegeEngineTypes.ImprovedRam)
                return typeof(BatteringRam);
            if (siegeWeaponType == DefaultSiegeEngineTypes.SiegeTower)
                return typeof(SiegeTower);
            if (siegeWeaponType == DefaultSiegeEngineTypes.Onager || siegeWeaponType == DefaultSiegeEngineTypes.Catapult)
                return typeof(Mangonel);
            if (siegeWeaponType == DefaultSiegeEngineTypes.FireOnager || siegeWeaponType == DefaultSiegeEngineTypes.FireCatapult)
                return typeof(FireMangonel);
            return siegeWeaponType == DefaultSiegeEngineTypes.Trebuchet || siegeWeaponType == DefaultSiegeEngineTypes.Bricole ? typeof(Trebuchet) : null;
        }

        private static Dictionary<Type, int> GetSiegeWeaponTypes(
            Dictionary<SiegeEngineType, int> values)
        {
            Dictionary<Type, int> dictionary = new Dictionary<Type, int>();
            foreach (KeyValuePair<SiegeEngineType, int> keyValuePair in values)
                dictionary.Add(GetSiegeWeaponType(keyValuePair.Key), keyValuePair.Value);
            return dictionary;
        }
    }
}
