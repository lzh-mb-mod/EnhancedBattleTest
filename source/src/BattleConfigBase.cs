using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class ClassInfo
    {
        public string classStringId;
        public int selectedFirstPerk;
        public int selectedSecondPerk;
        public int troopCount;
    }

    public enum BattleType
    {
        FieldBattle,
        SiegeBattle
    }

    public enum AIEnableType
    {
        None,
        EnemyOnly,
        PlayerOnly,
        Both
    }

    public class TacticOptionInfo
    {
        public TacticOptionEnum tacticOption { get; set; } = TacticOptionEnum.Charge;
        public bool isEnabled = true;
    }

    public abstract class BattleConfigBase
    {
        public string ConfigVersion { get; set; }

        [XmlIgnore]
        public BattleType battleType;

        public ClassInfo playerClass;
        private bool _spawnPlayer;
        public ClassInfo enemyClass;
        private bool _spawnEnemyCommander;
        public ClassInfo[] playerTroops;
        public ClassInfo[] enemyTroops;

        public TacticOptionInfo[] attackerTacticOptions;
        public TacticOptionInfo[] defenderTacticOptions;

        public AIEnableType aiEnableType = AIEnableType.EnemyOnly;

        public bool disableDying;

        public bool noAgentLabel = false;

        public bool changeCombatAI;
        public int combatAI;

        public void ToPreviousAIEnableType()
        {
            switch (aiEnableType)
            {
                case AIEnableType.None:
                    aiEnableType = AIEnableType.Both;
                    break;
                case AIEnableType.EnemyOnly:
                    aiEnableType = AIEnableType.None;
                    break;
                case AIEnableType.PlayerOnly:
                    aiEnableType = AIEnableType.EnemyOnly;
                    break;
                case AIEnableType.Both:
                    aiEnableType = AIEnableType.PlayerOnly;
                    break;
            }
        }

        public void ToNextAIEnableType()
        {
            switch (aiEnableType)
            {
                case AIEnableType.None:
                    aiEnableType = AIEnableType.EnemyOnly;
                    break;
                case AIEnableType.EnemyOnly:
                    aiEnableType = AIEnableType.PlayerOnly;
                    break;
                case AIEnableType.PlayerOnly:
                    aiEnableType = AIEnableType.Both;
                    break;
                case AIEnableType.Both:
                    aiEnableType = AIEnableType.None;
                    break;
            }
        }

        [XmlIgnore]
        public MultiplayerClassDivisions.MPHeroClass PlayerHeroClass
        {
            get => MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>(playerClass.classStringId);
            set => playerClass.classStringId = value.StringId;
        }
        [XmlIgnore]
        public MultiplayerClassDivisions.MPHeroClass EnemyHeroClass
        {
            get => MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>(enemyClass.classStringId);
            set => enemyClass.classStringId = value.StringId;
        }

        public void SetPlayerTroopHeroClass(int i, MultiplayerClassDivisions.MPHeroClass heroClass)
        {
            playerTroops[i].classStringId = heroClass.StringId;
        }

        public MultiplayerClassDivisions.MPHeroClass GetPlayerTroopHeroClass(int i)
        {
            return MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>(playerTroops[i]
                .classStringId);
        }
        public bool SpawnPlayer
        {
            get => _spawnPlayer;
            set
            {
                _spawnPlayer = value;
                playerClass.troopCount = value ? 1 : 0;
            }
        }

        public void SetEnemyTroopHeroClass(int i, MultiplayerClassDivisions.MPHeroClass heroClass)
        {
            enemyTroops[i].classStringId = heroClass.StringId;
        }

        public MultiplayerClassDivisions.MPHeroClass GetEnemyTroopHeroClass(int i)
        {
            return MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>(enemyTroops[i]
                .classStringId);
        }
        public bool SpawnEnemyCommander
        {
            get => _spawnEnemyCommander;
            set
            {
                _spawnEnemyCommander = value;
                enemyClass.troopCount = value ? 1 : 0;
            }
        }

        public BasicCultureObject GetPlayerTeamCulture()
        {
            if (SpawnPlayer)
                return PlayerHeroClass.Culture;
            for (int i = 0; i < 3; ++i)
            {
                if (playerTroops[i].troopCount != 0)
                    return GetPlayerTroopHeroClass(i).Culture;
            }

            return MultiplayerClassDivisions.AvailableCultures.FirstOrDefault();
        }

        public BasicCultureObject GetEnemyTeamCulture()
        {
            if (SpawnEnemyCommander)
                return EnemyHeroClass.Culture;
            for (int i = 0; i < 3; ++i)
            {
                if (enemyTroops[i].troopCount != 0)
                    return GetEnemyTroopHeroClass(i).Culture;
            }

            return MultiplayerClassDivisions.AvailableCultures.FirstOrDefault();
        }

        protected BattleConfigBase(BattleType t)
        {
            this.battleType = t;
            switch (t)
            {
                case BattleType.FieldBattle:
                    attackerTacticOptions = new[]
                    {
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.Charge},
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.FullScaleAttack},
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.DefensiveEngagement},
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.DefensiveLine},
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.DefensiveRing},
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.HoldTheHill},
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.HoldChokePoint},
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.ArchersOnTheHill},
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.RangedHarassmentOffensive},
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.FrontalCavalryCharge},
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.CoordinatedRetreat},
                    };
                    defenderTacticOptions = new[]
                    {
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.Charge},
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.FullScaleAttack},
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.DefensiveEngagement},
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.DefensiveLine},
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.DefensiveRing},
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.HoldTheHill},
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.HoldChokePoint},
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.ArchersOnTheHill},
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.RangedHarassmentOffensive},
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.FrontalCavalryCharge},
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.CoordinatedRetreat},
                    }; ;
                    break;
                case BattleType.SiegeBattle:
                    attackerTacticOptions = new[]
                    {
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.Charge},
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.BreachWalls},
                    };
                    defenderTacticOptions = new[]
                    {
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.Charge},
                        new TacticOptionInfo{tacticOption = TacticOptionEnum.DefendCastle},
                    };
                    break;
            }
        }

        public virtual bool Validate()
        {
            return PlayerHeroClass != null && enemyClass != null
                                           && playerTroops.All(classInfo =>
                                               MBObjectManager.Instance
                                                   .GetObject<MultiplayerClassDivisions.MPHeroClass>(
                                                       classInfo.classStringId) != null && classInfo.troopCount >= 0 &&
                                               classInfo.troopCount <= 5000 &&
                                               classInfo.selectedFirstPerk >= 0 && classInfo.selectedFirstPerk <= 2 &&
                                               classInfo.selectedSecondPerk >= 0 && classInfo.selectedSecondPerk <= 2)
                                           && enemyTroops.All(classInfo =>
                                               MBObjectManager.Instance
                                                   .GetObject<MultiplayerClassDivisions.MPHeroClass>(
                                                       classInfo.classStringId) != null && classInfo.troopCount >= 0 &&
                                               classInfo.troopCount <= 5000 &&
                                               classInfo.selectedFirstPerk >= 0 && classInfo.selectedFirstPerk <= 2 &&
                                               classInfo.selectedSecondPerk >= 0 && classInfo.selectedSecondPerk <= 2)
                                           && combatAI >= 0 && combatAI <= 100;
        }


        public abstract bool Serialize();

        public abstract bool Deserialize();

        public abstract void ReloadSavedConfig();

        public abstract void ResetToDefault();

        protected void EnsureSaveDirectory()
        {
            Directory.CreateDirectory(SavePath);
        }

        protected void SyncWithSave()
        {
            if (File.Exists(SaveName) && Deserialize())
            {
                RemoveOldConfig();
                return;
            }

            MoveOldConfig();
            if (File.Exists(SaveName) && Deserialize())
                return;
            Utility.DisplayMessage("Create default config.");
            ResetToDefault();
            Serialize();
        }

        private void RemoveOldConfig()
        {
            foreach (var oldName in OldNames)
            {
                if (File.Exists(oldName))
                {
                    Utility.DisplayMessage($"Found old config file: \"{oldName}\".");
                    Utility.DisplayMessage("Delete the old config file.");
                    File.Delete(oldName);
                }
            }
        }

        private void MoveOldConfig()
        {
            string firstOldName = OldNames.FirstOrDefault(File.Exists);
            if (firstOldName != null && !firstOldName.IsEmpty())
            {
                Utility.DisplayMessage($"Found old config file: \"{firstOldName}\".");
                Utility.DisplayMessage("Rename old config file to new name...");
                File.Move(firstOldName, SaveName);
            }
            RemoveOldConfig();
        }


        private static string ApplicationName = "Mount and Blade II Bannerlord";
        private static string ModuleName = "EnhancedBattleTest";

        protected static string SavePath => Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\" +
                                            ApplicationName + "\\Configs\\" + ModuleName + "\\";

        protected abstract string SaveName { get; }
        protected abstract string[] OldNames { get; }
    }

    public abstract class BattleConfigBase<T> : BattleConfigBase where T : BattleConfigBase<T>
    {
        protected BattleConfigBase(BattleType t)
            : base(t)
        { }
        protected virtual void CopyFrom(T other)
        {
            ConfigVersion = other.ConfigVersion;
            if (other.playerClass != null)
                this.playerClass = other.playerClass;
            this.SpawnPlayer = other.SpawnPlayer;
            if (other.enemyClass != null)
                this.enemyClass = other.enemyClass;
            this.SpawnEnemyCommander = other.SpawnEnemyCommander;
            if (other.playerTroops != null)
                this.playerTroops = other.playerTroops;
            if (other.enemyTroops != null)
                this.enemyTroops = other.enemyTroops;
            this.attackerTacticOptions = other.attackerTacticOptions;
            this.defenderTacticOptions = other.defenderTacticOptions;
            this.aiEnableType = other.aiEnableType;
            this.disableDying = other.disableDying;
            this.noAgentLabel = other.noAgentLabel;
            this.changeCombatAI = other.changeCombatAI;
            this.combatAI = other.combatAI;
        }
    }
}