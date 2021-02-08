using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data.MissionData;
using EnhancedBattleTest.Multiplayer.Data.MissionData;
using EnhancedBattleTest.SinglePlayer.Data.MissionData;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.Patch
{
    public class Patch_MissionAgentSpawnLogic
    {
        private static readonly FieldInfo HasBeenPositionedProperty =
            typeof(Formation).GetField("HasBeenPositioned", BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool SpawnTroops_Prefix(int number, bool isReinforcement, bool enforceSpawningOnInitialPoint, ref int __result,
            IMissionTroopSupplier ____troopSupplier,
            bool ____spawnWithHorses, BattleSideEnum ____side, MBList<Formation> ____spawnedFormations)
        {
            if (number <= 0)
            {
                __result = 0;
                return false;
            }
            int formationTroopIndex = 0;
            List<IAgentOriginBase> list = ____troopSupplier.SupplyTroops(number).ToList();
            for (int index = 0; index < 8; ++index)
            {
                var originToSpawn = new List<EnhancedBattleTestAgentOrigin>();
                EnhancedBattleTestAgentOrigin player = null;
                bool isMounted = false;
                FormationClass formationIndex = (FormationClass)index;
                foreach (IAgentOriginBase agentOriginBase in list)
                {
                    if (agentOriginBase is EnhancedBattleTestAgentOrigin agentOrigin &&
                        formationIndex == agentOrigin.FormationIndex)
                    {
                        if (agentOrigin.SpawnableCharacter.IsPlayer)
                        {
                            player = agentOrigin;
                        }
                        else
                        {
                            originToSpawn.Add(agentOrigin);
                        }

                        isMounted = isMounted || agentOrigin.Troop.HasMount();
                    }
                }

                if (player != null)
                {
                    originToSpawn.Add(player);
                }

                int count = originToSpawn.Count;
                if (count <= 0)
                    continue;

                foreach (EnhancedBattleTestAgentOrigin agentOriginBase in originToSpawn)
                {
                    try
                    {
                        FormationClass formationClass = agentOriginBase.SpawnableCharacter.FormationIndex;
                        var team = agentOriginBase.IsUnderPlayersCommand ? Mission.Current.PlayerTeam : Mission.Current.PlayerEnemyTeam;
                        Formation formation = team.GetFormation(formationClass);
                        if (formation != null && !(bool)HasBeenPositionedProperty.GetValue(formation))
                        {
                            formation.BeginSpawn(count, isMounted);
                            Mission.Current.SpawnFormation(formation, count, ____spawnWithHorses, isMounted, isReinforcement);
                            ____spawnedFormations.Add(formation);
                        }
                        agentOriginBase.SpawnTroop(____side, true, ____spawnWithHorses, isReinforcement,
                                enforceSpawningOnInitialPoint, count, formationTroopIndex, true, true,
                                false, null, new MatrixFrame?());
                        ++formationTroopIndex;
                    }
                    catch (Exception e)
                    {
                        Utility.DisplayMessage(e.ToString());
                    }
                }
            }
            if (formationTroopIndex > 0)
            {
                foreach (Team team in Mission.Current.Teams)
                    team.ExpireAIQuerySystem();
                Debug.Print(formationTroopIndex + " troops spawned on " + ____side + " side.", 0, Debug.DebugColor.DarkGreen, 64UL);
            }
            foreach (Team team in Mission.Current.Teams)
            {
                foreach (Formation formation in team.Formations)
                {
                    typeof(Formation).GetField("GroupSpawnIndex", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(formation, 0);
                }
            }
            __result = formationTroopIndex;
            return false;
        }
    }
}
