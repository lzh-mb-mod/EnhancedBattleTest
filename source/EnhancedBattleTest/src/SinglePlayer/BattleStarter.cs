using System.Collections.Generic;
using System.Reflection;
using EnhancedBattleTest.Config;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.SinglePlayer
{
    public class BattleStarter
    {
        private static PartyBase _playerParty;
        private static PartyBase _enemyParty;
        private static List<PartyBase> _playerSideParties;
        private static List<PartyBase> _enemySideParties;
        private static MobileParty _oldMainParty;
        private static BasicCharacterObject _oldPlayerCharacter;
        public static bool IsEnhancedBattleTestBattle = false;
        public static void Start(BattleConfig config, string mapId)
        {
            var rec = CreateMissionInitializerRecord(config.BattleTypeConfig.PlayerSide == BattleSideEnum.Attacker);
            if (config.PlayerSideConfig.Teams[0].HasGeneral && config.PlayerSideConfig.Teams[0].Generals.Troops.Count > 0)
            {
                _oldPlayerCharacter = Game.Current.PlayerTroop;
                Game.Current.PlayerTroop = config.PlayerSideConfig.Teams[0].Generals.Troops[0].Character.CharacterObject;
            }
            _oldMainParty = Campaign.Current.MainParty;

            _playerParty = CreateParty(config.PlayerSideConfig.Teams[0], true, 0);
            _playerSideParties = new List<PartyBase>() { _playerParty };
            _enemyParty = CreateParty(config.EnemySideConfig.Teams[0], false, 0);
            _enemySideParties = new List<PartyBase>() { _enemyParty };
            typeof(Campaign).GetProperty("MainParty").SetValue(Campaign.Current, _playerParty.MobileParty);
            PlayerEncounter.RestartPlayerEncounter(
                config.BattleTypeConfig.PlayerSide == BattleSideEnum.Defender ? _playerParty : _enemyParty,
                config.BattleTypeConfig.PlayerSide == BattleSideEnum.Attacker ? _playerParty : _enemyParty);

            IsEnhancedBattleTestBattle = true;
            PlayerEncounter.StartBattle();
            for (int i = 1; i < config.PlayerSideConfig.Teams.Count; ++i)
            {
                _playerSideParties.Add(CreateParty(config.PlayerSideConfig.Teams[i], true, i));
                _playerSideParties[i].MobileParty.MapEventSide = _playerParty.MapEventSide;
            }
            for (int i = 1; i < config.EnemySideConfig.Teams.Count; ++i)
            {
                _enemySideParties.Add(CreateParty(config.EnemySideConfig.Teams[i], false, i));
                _enemySideParties[i].MobileParty.MapEventSide = _enemyParty.MapEventSide;
            }
            CampaignMission.OpenBattleMission(rec);
        }

        public static void MissionEnded()
        {
            IsEnhancedBattleTestBattle = false;
            for (int i = _enemySideParties.Count - 1; i > 0; --i)
            {
                (_enemySideParties[i].MobileParty.PartyComponent as EnhancedBattleTestPartyComponent).RecoverHeroes();
                _enemySideParties[i].MapEventSide = null;
                _enemySideParties[i].MobileParty?.RemoveParty();
            }
            for (int i = _playerSideParties.Count - 1; i > 0; --i)
            {
                (_playerSideParties[i].MobileParty.PartyComponent as EnhancedBattleTestPartyComponent).RecoverHeroes();
                _playerSideParties[i].MapEventSide = null;
                _playerSideParties[i].MobileParty?.RemoveParty();
            }
            (_enemyParty.MobileParty.PartyComponent as EnhancedBattleTestPartyComponent).RecoverHeroes();
            (_playerParty.MobileParty.PartyComponent as EnhancedBattleTestPartyComponent).RecoverHeroes();
            if (_oldPlayerCharacter != null)
            {
                Game.Current.PlayerTroop = _oldPlayerCharacter;
            }
            _oldPlayerCharacter = null;

            var mapEvent = MapEvent.PlayerMapEvent;
            _playerParty.MapEventSide = null;
            _enemyParty.MapEventSide = null;
            mapEvent.FinalizeEvent();
            List<MapEvent> mapEvents = (List<MapEvent>)typeof(MapEventManager)
                .GetField("_mapEvents", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(Campaign.Current.MapEventManager);
            mapEvents.Remove(mapEvent);
            PlayerEncounter.Finish();
            typeof(Campaign).GetProperty("MainParty").SetValue(Campaign.Current, _oldMainParty);
            _oldMainParty = null;
            _playerParty.MobileParty?.RemoveParty();
            _enemyParty.MobileParty?.RemoveParty();
            _playerSideParties.Clear();
            _enemySideParties.Clear();
            _playerParty = null;
            _enemyParty = null;
            Campaign.Current.MainParty.Party.Visuals?.SetMapIconAsDirty();
        }

        private static MissionInitializerRecord CreateMissionInitializerRecord(bool isAttacker)
        {
            MapPatchData mapPatchAtPosition = Campaign.Current.MapSceneWrapper.GetMapPatchAtPosition(MobileParty.MainParty.Position2D);
            MissionInitializerRecord rec = new MissionInitializerRecord(PlayerEncounter.GetBattleSceneForMapPatch(mapPatchAtPosition));
            rec.TerrainType = (int)Campaign.Current.MapSceneWrapper.GetFaceTerrainType(MobileParty.MainParty.CurrentNavigationFace);
            rec.DamageToPlayerMultiplier = Campaign.Current.Models.DifficultyModel.GetDamageToPlayerMultiplier();
            rec.DamageToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
            rec.DamageFromPlayerToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
            rec.NeedsRandomTerrain = false;
            rec.PlayingInCampaignMode = true;
            rec.RandomTerrainSeed = MBRandom.RandomInt(10000);
            rec.AtmosphereOnCampaign = Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(CampaignTime.Now, MobileParty.MainParty.GetLogicalPosition());
            rec.SceneHasMapPatch = true;
            rec.AtlasGroup = 2;
            rec.PatchCoordinates = mapPatchAtPosition.normalizedCoordinates;
            Vec2 direction = Campaign.Current.MainParty.Bearing;
            if (isAttacker)
            {
                direction.RotateCCW(MBMath.PI);
            }
            rec.PatchEncounterDir = direction;
            float num = Campaign.CurrentTime % 24f;
            if (Campaign.Current != null)
                rec.TimeOfDay = num;
            return rec;
        }

        private static PartyBase CreateParty(TeamConfig teamConfig, bool isPlayerSide, int index = 0)
        {
            return MobileParty.CreateParty("EnhancedBattleTestParty",
                new EnhancedBattleTestPartyComponent(GetPartyName(isPlayerSide, index), teamConfig)).Party;
        }

        public static TextObject GetPartyName(bool isPlayerSide, int index)
        {
            return GameTexts.FindText("str_ebt_party_name").SetTextVariable("PARTY_TEXT", isPlayerSide
                ? new TextObject("{=sSJSTe5p}Player Party")
                : new TextObject("{=0xC75dN6}Enemy Party")).SetTextVariable("INDEX", index);
        }
    }
}
