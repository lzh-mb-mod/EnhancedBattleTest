using System.Collections.Generic;

namespace EnhancedBattleTest.Config
{
    public class SiegeMachineConfig
    {
        public List<string> AttackerMeleeMachines = new List<string>();
        public List<string> AttackerRangedMachines = new List<string>();
        public List<string> DefenderMachines = new List<string>();

        public void NormalizeAfterDeserialize()
        {
            AttackerMeleeMachines =
                AttackerMeleeMachines ?? new List<string>();
            AttackerRangedMachines =
                AttackerRangedMachines ?? new List<string>();
            DefenderMachines = DefenderMachines ?? new List<string>();
        }
    }
}
