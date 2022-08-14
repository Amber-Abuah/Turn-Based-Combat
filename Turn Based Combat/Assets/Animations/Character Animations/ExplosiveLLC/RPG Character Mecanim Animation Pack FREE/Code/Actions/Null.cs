namespace RPGCharacterAnims.Actions
{
    public class Null : InstantActionHandler<EmptyContext>
    {
        public override bool CanStartAction(RPGCharacterController controller)
        { return false; }

        protected override void _StartAction(RPGCharacterController controller, EmptyContext context)
        {
        }
    }
}