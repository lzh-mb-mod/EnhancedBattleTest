using System;
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

    public abstract class BattleConfigBase
    {
        public string ConfigVersion { get; set; }


        public ClassInfo playerClass;
        private bool _useFreeCamera;
        public ClassInfo enemyClass;
        private bool _spawnEnemyCommander;
        public ClassInfo[] playerTroops;
        public ClassInfo[] enemyTroops;

        public bool disableDying;
        public bool changeCombatAI;
        public int combatAI;

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
        public bool UseFreeCamera
        {
            get => _useFreeCamera;
            set
            {
                _useFreeCamera = value;
                playerClass.troopCount = value ? 0 : 1;
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
            if (!UseFreeCamera)
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
            Utility.DisplayMessage("No config file found.\nCreate default config.");
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
        protected virtual void CopyFrom(T other)
        {
            ConfigVersion = other.ConfigVersion;
            if (other.playerClass != null)
                this.playerClass = other.playerClass;
            if (other.enemyClass != null)
                this.enemyClass = other.enemyClass;
            this.SpawnEnemyCommander = other.SpawnEnemyCommander;
            if (other.playerTroops != null)
                this.playerTroops = other.playerTroops;
            if (other.enemyTroops != null)
                this.enemyTroops = other.enemyTroops;
            this.UseFreeCamera = other.UseFreeCamera;
            this.disableDying = other.disableDying;
            this.changeCombatAI = other.changeCombatAI;
            this.combatAI = other.combatAI;
        }
    }
}