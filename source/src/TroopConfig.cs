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

        public TroopConfig(bool isMultiplayer, string id, int number, float femaleRatio = 0)
        {
            Character = CharacterConfig.Create(isMultiplayer, id, femaleRatio);
            Number = number;
        }
    }
}
