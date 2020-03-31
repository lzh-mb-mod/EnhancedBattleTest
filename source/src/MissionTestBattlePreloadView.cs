using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;

namespace EnhancedBattleTest
{
    public class MissionFreeBattlePreloadView : MissionView
    {
        private EnhancedFreeBattleConfig _config;

        public MissionFreeBattlePreloadView(EnhancedFreeBattleConfig config)
        {
            _config = config;
        }
        public override void OnPreMissionTick(float dt)
        {
            List<BasicCharacterObject> characters = new List<BasicCharacterObject>()
            {
                _config.PlayerHeroClass.HeroCharacter,
                _config.EnemyHeroClass.HeroCharacter,
                _config.GetPlayerTroopHeroClass(0).TroopCharacter,
                _config.GetPlayerTroopHeroClass(1).TroopCharacter,
                _config.GetPlayerTroopHeroClass(2).TroopCharacter,
                _config.GetEnemyTroopHeroClass(0).TroopCharacter,
                _config.GetEnemyTroopHeroClass(1).TroopCharacter,
                _config.GetEnemyTroopHeroClass(2).TroopCharacter,
            };
            MissionPreloadHelper.PreloadCharacters(characters);
            this.Mission.RemoveMissionBehaviour(this);
        }
    }
}
