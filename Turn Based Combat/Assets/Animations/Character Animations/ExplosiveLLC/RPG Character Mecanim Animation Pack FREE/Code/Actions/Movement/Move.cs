using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims.Actions
{
    public class Move : MovementActionHandler<EmptyContext>
    {
        public Move(RPGCharacterMovementController movement) : base(movement)
        {
        }

        public override bool CanStartAction(RPGCharacterController controller)
        {
			return controller.canMove
				&& controller.moveInput.sqrMagnitude > 0.1f
				&& controller.maintainingGround;
        }

        protected override void _StartAction(RPGCharacterController controller, EmptyContext context)
        { movement.currentState = CharacterState.Move; }

        public override bool IsActive()
        { return movement.currentState != null && (CharacterState)movement.currentState == CharacterState.Move; }
    }
}