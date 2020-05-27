namespace EnhancedBattleTest
{
    public class TroopConfig
    {
        public CharacterConfig Character;
        public int Number;

        public TroopConfig()
        { }

        public TroopConfig(bool isMultiplayer)
        {
            Character = CharacterConfig.Create(isMultiplayer);
        }
    }
}
