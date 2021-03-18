namespace EnhancedBattleTest.Config
{
    public class TroopConfig
    {
        private CharacterConfig _character = CharacterConfig.Create(EnhancedBattleTestSubModule.IsMultiplayer);

        public CharacterConfig Character
        {
            get => _character;
            set => _character = value ?? _character;
        }
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

        public TroopConfig(TroopConfig other)
        {
            Character = other.Character.Clone();
            Number = other.Number;
        }
    }
}
