using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.View.Missions;

namespace Modbed
{
    public class EnhancedBattleTestView : MissionView
    {
        private GauntletLayer _gauntletLayer;
        private EnhancedBattleTestVM _dataSource;
        private CharacterSelectionView _selectionView;

        public EnhancedBattleTestView(CharacterSelectionView selectionView)
        {
            this._selectionView = selectionView;
            this.ViewOrderPriorty = 22;
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            Open();
        }
        public override void OnMissionScreenFinalize()
        {
            Close();
            base.OnMissionScreenFinalize();
        }
        public override bool OnEscape()
        {
            base.OnEscape();
            this.Close();
            return true;
        }

        public void Open()
        {
            this._dataSource = new EnhancedBattleTestVM(_selectionView, (param) =>
            {
                MissionState.OpenNew(
                    "EnhancedBattleTestBattle",
                    new MissionInitializerRecord(param.SceneName),
                    missionController => new MissionBehaviour[] {
                        new EnhancedBattleTestMissionController(param),
                        // new BattleTeam1MissionController(),
                        // new TaleWorlds.MountAndBlade.Source.Missions.SimpleMountedPlayerMissionController(),
                        new AgentBattleAILogic(),
                        new AgentVictoryLogic(),
                        new FieldBattleController(),
                        new MissionOptionsComponent(),
                        new EnhancedBattleTestMakeGruntLogic(),
                        // new MissionBoundaryPlacer(),
                    }
                );
                //this._missionController.BattleTestParams = param;
                //this._missionController.AddTeams();
                //this._orderView.EnhancedBattleInitialize();
                //this._missionController.SpawnEnemyTeamAgents();
                //this.OnClose();
            }, (param) =>
            {
                this.Mission.EndMission();
            });

            this._gauntletLayer = new GauntletLayer(this.ViewOrderPriorty, "GauntletLayer");
            this._gauntletLayer.LoadMovie(nameof(EnhancedBattleTestView), this._dataSource);
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, TaleWorlds.Library.InputUsageMask.All);
            this.MissionScreen.AddLayer(this._gauntletLayer);
        }

        public void Close()
        {
            if (this._gauntletLayer != null)
            {
                this._gauntletLayer.InputRestrictions.ResetInputRestrictions();
                this.MissionScreen.RemoveLayer(_gauntletLayer);
                this._gauntletLayer = null;
            }
            if (this._dataSource != null)
            {
                this._dataSource.OnFinalize();
                this._dataSource = null;
            }
        }
    }
}