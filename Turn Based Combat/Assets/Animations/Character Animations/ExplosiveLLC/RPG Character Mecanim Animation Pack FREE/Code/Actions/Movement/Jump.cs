using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims.Actions
{
    public class Jump : MovementActionHandler<EmptyContext>
    {
        public Jump(RPGCharacterMovementController movement) : base(movement)
        {
        }

        public override bool CanStartAction(RPGCharacterController controller)
        { return (movement.canJump || movement.canDoubleJump) && controller.maintainingGround && controller.canAction; }

        protected override void _StartAction(RPGCharacterController controller, EmptyContext context)
        { movement.currentState = CharacterState.Jump; }

        public override bool IsActive()
        { return movement.currentState != null && (CharacterState)movement.currentState == CharacterState.Jump; }
    }
}