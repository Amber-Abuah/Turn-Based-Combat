using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims.Actions
{
    public class Fall : MovementActionHandler<EmptyContext>
    {
        public Fall(RPGCharacterMovementController movement) : base(movement)
        {
        }

        public override bool CanStartAction(RPGCharacterController controller)
        { return !controller.maintainingGround; }

        protected override void _StartAction(RPGCharacterController controller, EmptyContext context)
        { movement.currentState = CharacterState.Fall; }

        public override bool IsActive()
        { return movement.currentState != null && (CharacterState)movement.currentState == CharacterState.Fall; }
    }
}