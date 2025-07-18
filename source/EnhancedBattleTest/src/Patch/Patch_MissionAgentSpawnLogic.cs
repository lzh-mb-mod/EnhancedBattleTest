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
        public static bool SpawnTroops_Prefix(Object __instance, int number, bool isReinforcement, ref int __result,
            IMissionTroopSupplier ____troopSupplier, List<IAgentOriginBase> ____reservedTroops, List<(Team team, List<IAgentOriginBase> origins)> ____troopOriginsToSpawnPerTeam,
            bool ____spawnWithHorses, BattleSideEnum ____side, MBList<Formation> ____spawnedFormations,
            Dictionary<IAgentOriginBase, int> ____reinforcementTroopFormationAssignments,
            (int currentTroopIndex, int troopCount)[] ____reinforcementSpawnedUnitCountPerFormation,
            BannerBearerLogic ____bannerBearerLogic,
            int ____numSpawnedTroops)
        {
            if (number <= 0)
            {
                __result = 0;
                return false;
            }
            List<IAgentOriginBase> agentOriginBaseList1 = new List<IAgentOriginBase>();
            int reservedCount = Math.Min(____reservedTroops.Count, number);
            if (reservedCount > 0)
            {
                for (int index = 0; index < reservedCount; ++index)
                    agentOriginBaseList1.Add(____reservedTroops[index]);
                ____reservedTroops.RemoveRange(0, reservedCount);
            }
            int numberToAllocate = number - reservedCount;
            agentOriginBaseList1.AddRange(____troopSupplier.SupplyTroops(numberToAllocate));
            bool isPlayerSide = Mission.Current.PlayerTeam != null && ____side == Mission.Current.PlayerTeam.Side;
            if (____troopOriginsToSpawnPerTeam == null)
            {
                ____troopOriginsToSpawnPerTeam = new List<(Team, List<IAgentOriginBase>)>();
                foreach (Team team in (List<Team>)Mission.Current.Teams)
                {
                    bool flag = team.Side == Mission.Current.PlayerTeam.Side;
                    if (isPlayerSide & flag || !isPlayerSide && !flag)
                        ____troopOriginsToSpawnPerTeam.Add((team, new List<IAgentOriginBase>()));
                }
            }
            else
            {
                foreach ((Team team, List<IAgentOriginBase> origins) tuple in ____troopOriginsToSpawnPerTeam)
                    tuple.origins.Clear();
            }
            int num1 = 0;
            foreach (IAgentOriginBase troopOrigin in agentOriginBaseList1)
            {
                Team agentTeam = Mission.GetAgentTeam(troopOrigin, isPlayerSide);
                foreach ((Team team, List<IAgentOriginBase> origins) tuple in ____troopOriginsToSpawnPerTeam)
                {
                    if (agentTeam == tuple.team)
                    {
                        ++num1;
                        tuple.origins.Add(troopOrigin);
                    }
                }
            }
            int num2 = 0;
            List<IAgentOriginBase> agentOriginBaseList2 = new List<IAgentOriginBase>();
            foreach ((Team team, List<IAgentOriginBase> origins) tuple in ____troopOriginsToSpawnPerTeam)
            {
                if (!tuple.origins.IsEmpty<IAgentOriginBase>())
                {
                    int num3 = 0;
                    int mountedTroopCount = 0;
                    int footTroopCount = 0;
                    List<(IAgentOriginBase, int)> valueTupleList;
                    if (isReinforcement)
                    {
                        valueTupleList = new List<(IAgentOriginBase, int)>();
                        foreach (IAgentOriginBase key in tuple.origins)
                        {
                            int num4;
                            ____reinforcementTroopFormationAssignments.TryGetValue(key, out num4);
                            valueTupleList.Add((key, num4));
                        }
                    }
                    else
                        valueTupleList = MissionGameModels.Current.BattleSpawnModel.GetInitialSpawnAssignments(____side, tuple.origins);
                    for (int index = 0; index < 8; ++index)
                    {
                        agentOriginBaseList2.Clear();
                        IAgentOriginBase player = (IAgentOriginBase)null;
                        FormationClass formationIndex = (FormationClass)index;
                        foreach ((IAgentOriginBase agentOriginBase2, int num5) in valueTupleList)
                        {
                            if (index == num5)
                            {
                                if (agentOriginBase2 is EnhancedBattleTestAgentOrigin agentOrigin &&
                                    formationIndex == agentOrigin.FormationIndex)
                                {
                                    if (agentOrigin.SpawnableCharacter.IsPlayer)
                                    {
                                        player = agentOrigin;
                                    }
                                    else
                                    {
                                        if (agentOriginBase2.Troop.HasMount())
                                            ++mountedTroopCount;
                                        else
                                            ++footTroopCount;
                                        agentOriginBaseList2.Add(agentOriginBase2);
                                    }
                                }
                                if (agentOriginBase2.Troop == Game.Current.PlayerTroop)
                                {
                                    player = agentOriginBase2;
                                }
                                else
                                {
                                    if (agentOriginBase2.Troop.HasMount())
                                        ++mountedTroopCount;
                                    else
                                        ++footTroopCount;
                                    agentOriginBaseList2.Add(agentOriginBase2);
                                }
                            }
                        }
                            if (player != null)
                        {
                            if (player.Troop.HasMount())
                                ++mountedTroopCount;
                            else
                                ++footTroopCount;
                            agentOriginBaseList2.Add(player);
                        }
                        int count2 = agentOriginBaseList2.Count;
                        if (count2 > 0)
                        {
                            bool isMounted = ____spawnWithHorses && MissionDeploymentPlan.HasSignificantMountedTroops(footTroopCount, mountedTroopCount);
                            int formationTroopIndex = 0;
                            int num6 = count2;
                            var missionSideType = typeof(MissionAgentSpawnLogic).GetNestedType("MissionSide", BindingFlags.NonPublic);
                            if ((bool)missionSideType.GetProperty("ReinforcementSpawnActive").GetValue(__instance))
                            {
                                formationTroopIndex = ____reinforcementSpawnedUnitCountPerFormation[index].currentTroopIndex;
                                num6 = ____reinforcementSpawnedUnitCountPerFormation[index].troopCount;
                            }
                            Formation formation = tuple.team.GetFormation((FormationClass)index);
                            if (!formation.HasBeenPositioned)
                            {
                                formation.BeginSpawn(num6, isMounted);
                                Mission.Current.SetFormationPositioningFromDeploymentPlan(formation);
                                ____spawnedFormations.Add(formation);
                            }
                            foreach (IAgentOriginBase troopOrigin in agentOriginBaseList2)
                            {
                                if (!troopOrigin.Troop.IsHero && ____bannerBearerLogic != null && Mission.Current.Mode != MissionMode.Deployment && ____bannerBearerLogic.GetMissingBannerCount(formation) > 0)
                                    ____bannerBearerLogic.SpawnBannerBearer(troopOrigin, isPlayerSide, formation, ____spawnWithHorses, isReinforcement, num6, formationTroopIndex, true, true, false, new Vec3?(), new Vec2?(), useTroopClassForSpawn: Mission.Current.IsSallyOutBattle);
                                else
                                    Mission.Current.SpawnTroop(troopOrigin, isPlayerSide, true, ____spawnWithHorses, isReinforcement, num6, formationTroopIndex, true, true, false, new Vec3?(), new Vec2?(), formationIndex: formation.FormationIndex, useTroopClassForSpawn: Mission.Current.IsSallyOutBattle);
                                ++____numSpawnedTroops;
                                ++formationTroopIndex;
                                ++num3;
                            }
                            if ((bool)missionSideType.GetProperty("ReinforcementSpawnActive").GetValue(__instance))
                                ____reinforcementSpawnedUnitCountPerFormation[index].currentTroopIndex = formationTroopIndex;
                        }
                    }
                    if (num3 > 0)
                        tuple.team.QuerySystem.Expire();
                    num2 += num3;
                    foreach (Formation formation in (List<Formation>)tuple.team.FormationsIncludingEmpty)
                    {
                        if (formation.CountOfUnits > 0 && formation.IsSpawning)
                            formation.EndSpawn();
                    }
                }
            }
            __result = num2;
            return false;
        }
    }
}
