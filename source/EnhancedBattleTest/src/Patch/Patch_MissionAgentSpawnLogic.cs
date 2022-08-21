using EnhancedBattleTest.Data.MissionData;
using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.Patch
{
    public class Patch_MissionAgentSpawnLogic
    {
        public static bool SpawnTroops_Prefix(int number, bool isReinforcement, bool enforceSpawningOnInitialPoint, ref int __result,
            IMissionTroopSupplier ____troopSupplier, List<IAgentOriginBase> ____preSuppliedTroops,
            bool ____spawnWithHorses, BattleSideEnum ____side, MBList<Formation> ____spawnedFormations)
        {
            if (number <= 0)
            {
                __result = 0;
                return false;
            }
            int formationTroopIndex = 0;
            List<IAgentOriginBase> list = new List<IAgentOriginBase>();
            int preSuppliedCount = Math.Min(____preSuppliedTroops.Count, number);
            if (preSuppliedCount > 0)
            {
                for (int index = 0; index < preSuppliedCount; ++index)
                    list.Add(____preSuppliedTroops[index]);
                ____preSuppliedTroops.RemoveRange(0, preSuppliedCount);
            }
            list.AddRange(____troopSupplier.SupplyTroops(number - preSuppliedCount));
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
                        if (formation != null && !formation.HasBeenPositioned)
                        {
                            formation.BeginSpawn(count, isMounted && ____spawnWithHorses);
                            Mission.Current.SpawnFormation(formation);
                            ____spawnedFormations.Add(formation);
                        }
                        agentOriginBase.SpawnTroop(____side, true, ____spawnWithHorses, isReinforcement,
                                enforceSpawningOnInitialPoint, count, formationTroopIndex, true, true,
                                false, null, null);
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
                    formation.GroupSpawnIndex = 0;
                }
            }
            __result = formationTroopIndex;
            return false;
        }
    }
}
