using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class SpawnPlayerLogic : MissionLogic
    {
        private CustomBattleCombatant _playerParty;
        private EnhancedTroopSupplier _troopSupplier;
        private BasicCharacterObject _playerCharacter;
        private bool _withHorse;

        public SpawnPlayerLogic(CustomBattleCombatant playerParty, EnhancedTroopSupplier troopSupplier, BasicCharacterObject player, bool withHorse)
        {
            _playerParty = playerParty;
            _troopSupplier = troopSupplier;
            _playerCharacter = player;
            _withHorse = withHorse;
        }

        public override void AfterStart()
        {
            base.AfterStart();
            var player = Mission.Current.SpawnTroop(
                Utility.CreateOrigin(_playerParty, _playerCharacter, -1, _troopSupplier), true, true, _withHorse, false,
                true, 1, 0, true, true);
            player.Controller = Agent.ControllerType.Player;
        }
    }
}
