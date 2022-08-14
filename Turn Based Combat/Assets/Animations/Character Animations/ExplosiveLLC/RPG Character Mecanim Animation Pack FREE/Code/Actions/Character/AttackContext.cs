using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims.Actions
{
    public class AttackContext
    {
        public string type;
        public Side Side;
        public int number;

        public AttackContext(string type, Side side, int number = -1)
        {
            this.type = type;
            Side = side;
            this.number = number;
        }
    }
}