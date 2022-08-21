using SandBox.GauntletUI.BannerEditor;
using System;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace EnhancedBattleTest.BannerEditor
{
    [GameStateScreen(typeof(BannerEditorState))]
    public class BannerEditorScreen : ScreenBase, IGameStateListener
    {
        private const int ViewOrderPriority = 15;
        private readonly BannerEditorView _bannerEditorLayer;
        private bool _oldGameStateManagerDisabledStatus;

        public BannerEditorScreen(BannerEditorState bannerEditorState)
        {
            LoadingWindow.EnableGlobalLoadingWindow();
            _bannerEditorLayer = new BannerEditorView(BannerEditorState.Character, BannerEditorState.Banner, OnDone,
                new TextObject("{=WiNRdfsm}Done"), OnCancel, new TextObject("{=3CpNUnVl}Cancel"));
            _bannerEditorLayer.DataSource.SetClanRelatedRules(true);
        }

        protected override void OnFrameTick(float dt)
        {
            base.OnFrameTick(dt);

            _bannerEditorLayer.OnTick(dt);

            if (Input.IsKeyReleased(InputKey.Escape))
                OnCancel();

            if (_bannerEditorLayer.SceneLayer.Input.IsHotKeyPressed("Copy") ||
                _bannerEditorLayer.GauntletLayer.Input.IsHotKeyPressed("Copy"))
            {
                try
                {
                    Input.SetClipboardText(_bannerEditorLayer.DataSource.BannerVM.Banner.Serialize());
                }
                catch (Exception e)
                {
                    Utility.DisplayMessage(e.ToString(), new Color(1, 0, 0));
                }
            }
            else if (_bannerEditorLayer.SceneLayer.Input.IsHotKeyPressed("Paste") ||
                     _bannerEditorLayer.GauntletLayer.Input.IsHotKeyPressed("Paste"))
            {
                try
                {
                    string text = Input.GetClipboardText();
                    _bannerEditorLayer.DataSource.BannerVM.BannerCode = text;
                    typeof(BannerEditorView).GetMethod("RefreshShieldAndCharacter",
                            BindingFlags.NonPublic | BindingFlags.Instance)
                        ?.Invoke(_bannerEditorLayer, new object[] { });
                }
                catch (Exception e)
                {
                    Utility.DisplayMessage(e.ToString(), new Color(1, 0, 0));
                }
            }
        }

        private void OnDone()
        {
            BannerEditorState.Config.BannerKey = _bannerEditorLayer.DataSource.BannerVM.Banner.Serialize();
            BannerEditorState.OnDone?.Invoke();
            TaleWorlds.Core.Game.Current.GameStateManager.PopState();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _oldGameStateManagerDisabledStatus = TaleWorlds.Core.Game.Current.GameStateManager.ActiveStateDisabledByUser;
            TaleWorlds.Core.Game.Current.GameStateManager.RegisterActiveStateDisableRequest(this);
        }

        protected override void OnFinalize()
        {
            base.OnFinalize();

            _bannerEditorLayer.OnFinalize();
            if (LoadingWindow.GetGlobalLoadingWindowState())
                LoadingWindow.DisableGlobalLoadingWindow();
            TaleWorlds.Core.Game.Current.GameStateManager.UnregisterActiveStateDisableRequest(this);
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            AddLayer(_bannerEditorLayer.GauntletLayer);
            AddLayer(_bannerEditorLayer.SceneLayer);
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();

            _bannerEditorLayer.OnDeactivate();
        }

        private void OnCancel()
        {
            TaleWorlds.Core.Game.Current.GameStateManager.PopState();
        }

        void IGameStateListener.OnActivate()
        {
        }

        void IGameStateListener.OnDeactivate()
        {
        }

        void IGameStateListener.OnInitialize()
        {
        }

        void IGameStateListener.OnFinalize()
        {
        }
    }
}
