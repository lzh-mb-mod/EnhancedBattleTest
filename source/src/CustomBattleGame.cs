﻿// Decompiled with JetBrains decompiler
// Type: TaleWorlds.MountAndBlade.CustomGame
// Assembly: TaleWorlds.MountAndBlade, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D5D21862-28AB-45FC-8C12-16AF95A20751
// Assembly location: D:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord - Beta\bin\Win64_Shipping_Client\TaleWorlds.MountAndBlade.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class CustomBattleGame : TaleWorlds.Core.GameType
    {

        public static CustomBattleGame Current => Game.Current.GameType as CustomBattleGame;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Game currentGame = this.CurrentGame;
            currentGame.FirstInitialize();
            IGameStarter gameStarter = (IGameStarter)new BasicGameStarter();
            this.AddGameModels(gameStarter);
            this.GameManager.OnGameStart(this.CurrentGame, gameStarter);
            currentGame.SecondInitialize(gameStarter.Models);
            currentGame.CreateGameManager();
            this.GameManager.BeginGameStart(this.CurrentGame);
            this.CurrentGame.RegisterBasicTypes();
            this.CurrentGame.ThirdInitialize();
            currentGame.CreateObjects();
            currentGame.InitializeDefaultGameObjects();
            currentGame.LoadBasicFiles(false);
            this.ObjectManager.LoadXML("Items", (Type)null);
            this.ObjectManager.LoadXML("MPCharacters", (Type)null);
            this.ObjectManager.LoadXML("BasicCultures", (Type)null);
            this.ObjectManager.LoadXML("MPClassDivisions", (Type)null);
            this.ObjectManager.ClearEmptyObjects();
            MultiplayerClassDivisions.Initialize();
            currentGame.SetDefaultEquipments((IReadOnlyDictionary<string, Equipment>)new Dictionary<string, Equipment>());
            ModuleLogger.Writer.WriteLine(currentGame.BasicModels);
            ModuleLogger.Writer.Flush();
            if (currentGame.BasicModels.SkillList == null)
            {
                throw new Exception("haha");
            }
            currentGame.CreateLists();
            this.ObjectManager.ClearEmptyObjects();
            this.AddGameTexts();
            this.GameManager.OnCampaignStart(this.CurrentGame, (object)null);
            this.GameManager.OnAfterCampaignStart(this.CurrentGame);
            this.GameManager.OnGameInitializationFinished(this.CurrentGame);
        }

        protected override void DoLoadingForGameType(
            GameTypeLoadingStates gameTypeLoadingState,
            out GameTypeLoadingStates nextState)
        {
            // ModuleLogger.Writer.WriteLine("EnhancedBattleTestGame.DoLoadingForGameType {0}", gameTypeLoadingState);
            // ModuleLogger.Writer.Flush();
            nextState = GameTypeLoadingStates.None;
            switch (gameTypeLoadingState)
            {
                case GameTypeLoadingStates.InitializeFirstStep:
                    this.CurrentGame.Initialize();
                    nextState = GameTypeLoadingStates.WaitSecondStep;
                    break;
                case GameTypeLoadingStates.WaitSecondStep:
                    nextState = GameTypeLoadingStates.LoadVisualsThirdState;
                    break;
                case GameTypeLoadingStates.LoadVisualsThirdState:
                    nextState = GameTypeLoadingStates.PostInitializeFourthState;
                    break;
            }
        }

        public override void OnDestroy()
        {
        }

        private void AddGameModels(IGameStarter basicGameStarter)
        {
            basicGameStarter.AddModel(new MultiplayerAgentDecideKilledOrUnconsciousModel());
            basicGameStarter.AddModel(new CustomBattleAgentStatCalculateModel());
            basicGameStarter.AddModel(new CustomBattleApplyWeatherEffectsModel());
            basicGameStarter.AddModel(new MultiplayerAgentApplyDamageModel());
            basicGameStarter.AddModel(new DefaultRidingModel());
            basicGameStarter.AddModel(new DefaultStrikeMagnitudeModel());
            basicGameStarter.AddModel(new CustomBattleSkillList());
            basicGameStarter.AddModel(new CustomBattleMoraleModel());
        }

        protected override void OnRegisterTypes()
        {
            base.OnRegisterTypes();
            int num1 = (int)this.ObjectManager.RegisterType<BasicCharacterObject>("NPCCharacter", "MPCharacters", true);
            int num2 = (int)this.ObjectManager.RegisterType<BasicCultureObject>("Culture", "BasicCultures", true);
            int num3 = (int)this.ObjectManager.RegisterType<MultiplayerClassDivisions.MPHeroClass>("MPClassDivision", "MPClassDivisions", true);
        }

        private void AddGameTexts()
        {
            this.CurrentGame.GameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/multiplayer_strings.xml");
            this.CurrentGame.GameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/global_strings.xml");
            this.CurrentGame.GameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/module_strings.xml");
            // this.CurrentGame.GameTextManager.LoadGameTexts(BasePath.Name + "Modules/EnhancedBattleTest/ModuleData/module_strings.xml");
        }
    }

    class CustomBattleSkillList : SkillList
    {

        public override IEnumerable<SkillObject> GetSkillList()
        {
            yield return DefaultSkills.OneHanded;
            yield return DefaultSkills.TwoHanded;
            yield return DefaultSkills.Polearm;
            yield return DefaultSkills.Bow;
            yield return DefaultSkills.Crossbow;
            yield return DefaultSkills.Throwing;
            yield return DefaultSkills.Riding;
            yield return DefaultSkills.Athletics;
            yield return DefaultSkills.Tactics;
            yield return DefaultSkills.Scouting;
            yield return DefaultSkills.Roguery;
            yield return DefaultSkills.Crafting;
            yield return DefaultSkills.Charm;
            yield return DefaultSkills.Trade;
            yield return DefaultSkills.Leadership;
            yield return DefaultSkills.Steward;
            yield return DefaultSkills.Medicine;
            yield return DefaultSkills.Engineering;
        }
    }
}