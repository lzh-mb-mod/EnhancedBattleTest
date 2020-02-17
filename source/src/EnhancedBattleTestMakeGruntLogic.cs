using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.MountAndBlade;

namespace Modbed
{
    class EnhancedBattleTestMakeGruntLogic : MissionLogic
    {
        class Pair
        {
            public Formation formation;
            public float timer;
        }
        private List<Pair> _formations = new List<Pair>();

        public void AddFormation(Formation formation, float timer)
        {
            if (_formations.Exists(pair => pair.formation == formation))
                return;
            _formations.Add(new Pair{formation = formation, timer = timer});
        }

        public override void OnClearScene()
        {
            this._formations.Clear();
        }

        public override void OnMissionTick(float dt)
        {
            _formations.RemoveAll(pair =>
            {
                pair.timer -= dt;
                if (pair.timer < 0)
                {
                    foreach (var agent in pair.formation.Units)
                    {
                        if (agent == Mission.MainAgent)
                            continue;
                        agent.MakeVoice(SkinVoiceManager.HardCodedSkinVoiceTypes.Grunt, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
                    }

                    return true;
                }
                return false;
            });
        }
    }
}
