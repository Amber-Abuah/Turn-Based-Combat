using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims.Actions
{
    public class Idle : MovementActionHandler<EmptyContext>
    {
        public Idle(RPGCharacterMovementController movement) : base(movement)
        {
        }

        public override bool CanStartAction(RPGCharacterController controller)
        {
            if (controller.isMoving) { return controller.moveInput.magnitude < 0.2f; }
            return controller.maintainingGround || controller.acquiringGround;
        }

        protected override void _StartAction(RPGCharacterController controller, EmptyContext context)
        {
            movement.currentState = CharacterState.Idle;
        }

        public override bool IsActive()
        { return movement.currentState != null && (CharacterState)movement.currentState == CharacterState.Idle; }
    }
}