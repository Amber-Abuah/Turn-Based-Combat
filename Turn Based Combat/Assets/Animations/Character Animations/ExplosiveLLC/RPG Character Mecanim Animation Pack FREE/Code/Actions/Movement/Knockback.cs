using RPGCharacterAnims.Extensions;
using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims.Actions
{
    public class Knockback : MovementActionHandler<HitContext>
    {
        public Knockback(RPGCharacterMovementController movement) : base(movement)
        {
        }

        public override bool CanStartAction(RPGCharacterController controller)
        { return controller.canAction; }

        protected override void _StartAction(RPGCharacterController controller, HitContext context)
        {
            var hitNumber = context.number;
            var direction = context.direction;
            var force = context.force;
            var variableForce = context.variableForce;

            if (hitNumber == -1) {
                hitNumber = (int)AnimationVariations.Knockbacks.TakeRandom();
                direction = AnimationData.HitDirection((KnockbackType)hitNumber);
                direction = controller.transform.rotation * direction;
            }
			else {
                if (context.relative) { direction = controller.transform.rotation * direction; }
            }

            controller.Knockback((KnockbackType)hitNumber);
            movement.KnockbackForce(direction, force, variableForce);
            movement.currentState = CharacterState.Knockback;
        }

        public override bool IsActive()
        { return movement.currentState != null && (CharacterState)movement.currentState == CharacterState.Knockback; }
    }
}