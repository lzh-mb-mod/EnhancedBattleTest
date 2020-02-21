using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class ClassInfo
    {
        public string classStringId;
        public int selectedFirstPerk;
        public int selectedSecondPerk;
    }
    public abstract class BattleConfigBase<T> where T : BattleConfigBase<T>
    {
        public string ConfigVersion { get; set; }

        public int playerSoldierCount, enemySoldierCount;

        public ClassInfo playerClass;
        public ClassInfo playerTroopClass;
        public ClassInfo enemyTroopClass;

        [XmlIgnore]
        public MultiplayerClassDivisions.MPHeroClass PlayerHeroClass
        {
            get => MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>(playerClass.classStringId);
            set => playerClass.classStringId = value.StringId;
        }
        [XmlIgnore]
        public MultiplayerClassDivisions.MPHeroClass PlayerTroopHeroClass
        {
            get => MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>(playerTroopClass.classStringId);
            set => playerTroopClass.classStringId = value.StringId;
        }
        [XmlIgnore]
        public MultiplayerClassDivisions.MPHeroClass EnemyTroopHeroClass
        {
            get => MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>(enemyTroopClass.classStringId);
            set => enemyTroopClass.classStringId = value.StringId;
        }


        public virtual bool Validate()
        {
            return this.playerSoldierCount >= 0
                   && this.enemySoldierCount >= 0
                   && PlayerHeroClass != null
                   && PlayerTroopHeroClass != null
                   && EnemyTroopHeroClass != null;
        }


        public abstract bool Serialize();

        public abstract bool Deserialize();

        public abstract void ReloadSavedConfig();

        public abstract void ResetToDefault();

        protected virtual void CopyFrom(T other)
        {
            ConfigVersion = other.ConfigVersion;
            this.playerSoldierCount = other.playerSoldierCount;
            this.enemySoldierCount = other.enemySoldierCount;
            if (other.playerClass != null)
                this.playerClass = other.playerClass;
            if (other.playerTroopClass != null)
                this.playerTroopClass = other.playerTroopClass;
            if (other.enemyTroopClass != null)
                this.enemyTroopClass = other.enemyTroopClass;

        }

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
}