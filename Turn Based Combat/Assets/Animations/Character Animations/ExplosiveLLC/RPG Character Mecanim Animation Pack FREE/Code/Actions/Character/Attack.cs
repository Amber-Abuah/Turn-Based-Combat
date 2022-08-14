using RPGCharacterAnims.Extensions;
using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims.Actions
{
	public class Attack:BaseActionHandler<AttackContext>
	{
		public override bool CanStartAction(RPGCharacterController controller)
		{ return !active && controller.canAction; }

		public override bool CanEndAction(RPGCharacterController controller)
		{ return active; }

		protected override void _StartAction(RPGCharacterController controller, AttackContext context)
		{
			var attackSide = Side.None;
			var attackNumber = context.number;
			var weaponNumber = controller.rightWeapon;
			var duration = 0f;

			if (context.Side == Side.Right && weaponNumber.Is2HandedWeapon()) { context.Side = Side.None; }

			switch (context.Side) {
				case Side.None:
					attackSide = context.Side;
					weaponNumber = controller.rightWeapon;
					break;
				case Side.Left:
					attackSide = context.Side;
					weaponNumber = controller.leftWeapon;
					break;
				case Side.Right:
					attackSide = context.Side;
					weaponNumber = controller.rightWeapon;
					break;
			}

			if (attackNumber == -1) {
				switch (context.type) {
					case "Attack":
						attackNumber = AnimationData.RandomAttackNumber(attackSide, weaponNumber);
						break;
					case "Special":
						attackNumber = 1;
						break;
				}
			}

			duration = AnimationData.AttackDuration(attackSide, weaponNumber, attackNumber);

			if (controller.isMoving) {
				controller.RunningAttack(
					attackSide,
					false,
					false,
					controller.hasTwoHandedWeapon
				);
				EndAction(controller);
			}
			else if (context.type == "Attack") {
				controller.Attack(
					attackNumber,
					attackSide,
					controller.leftWeapon,
					controller.rightWeapon,
					duration
				);
				EndAction(controller);
			}
		}

		protected override void _EndAction(RPGCharacterController controller)
		{
		}
	}
}