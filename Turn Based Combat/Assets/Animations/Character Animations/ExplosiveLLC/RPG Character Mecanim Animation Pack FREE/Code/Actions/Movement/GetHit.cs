// Hit from front1 - 1
// Hit from front2 - 2
// Hit from back - 3
// Hit from left - 4
// Hit from right - 5

using RPGCharacterAnims.Extensions;
using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims.Actions
{
    public class GetHit : MovementActionHandler<HitContext>
    {
        public GetHit(RPGCharacterMovementController movement) : base(movement)
        {
        }

        public override bool CanStartAction(RPGCharacterController controller)
        { return !controller.isKnockback && !controller.isKnockdown; }

        protected override void _StartAction(RPGCharacterController controller, HitContext context)
        {
            var hitNumber = context.number;
            var direction = context.direction;
            var force = context.force;
            var variableForce = context.variableForce;

            if (hitNumber == -1) {
                hitNumber = (int)AnimationVariations.Hits.TakeRandom();
                direction = AnimationData.HitDirection((HitType)hitNumber);
                direction = controller.transform.rotation * direction;
            }
			else {
                if (context.relative) { direction = controller.transform.rotation * direction; }
            }

            controller.GetHit(hitNumber);
            movement.KnockbackForce(direction, force, variableForce);
        }
    }
}