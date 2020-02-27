using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace EnhancedBattleTest
{
    public class TrainingAgentApplyDamageModel : AgentApplyDamageModel
    {
        public override int CalculateDamage(
            BasicCharacterObject affectorCharacterBasic,
            BasicCharacterObject affectedCharacterBasic,
            MissionWeapon offHandItem,
            bool isHeadShot,
            bool isAffectedAgentMount,
            bool isAffectedAgentHuman,
            bool hasAffectorAgentMount,
            bool isAffectedAgentNull,
            bool isAffectorAgentHuman,
            AttackCollisionData collisionData,
            WeaponComponentData weapon)
        {
            return collisionData.InflictedDamage;
        }

        public override void CalculateEffects(Agent attackerAgent, ref bool crushedThrough)
        {
        }

        public override float CalculateMoraleEffects(
            Agent attackerAgent,
            Agent defenderAgent,
            int currentWeaponUsageIndex,
            int affectorWeaponKind)
        {
            return 0.0f;
        }

        public override float CalculateCouchedLanceDamage(
            BasicCharacterObject attackerCharacter,
            float baseDamage)
        {
            return 0.0f;
        }

        public override float CalculateShieldDamage(float baseDamage)
        {
            MissionMultiplayerFlagDomination missionBehaviour = Mission.Current.GetMissionBehaviour<MissionMultiplayerFlagDomination>();
            return missionBehaviour != null && missionBehaviour.GetMissionType() == MissionLobbyComponent.MultiplayerGameType.Captain ? baseDamage * 0.5f : baseDamage;
        }
    }
}
