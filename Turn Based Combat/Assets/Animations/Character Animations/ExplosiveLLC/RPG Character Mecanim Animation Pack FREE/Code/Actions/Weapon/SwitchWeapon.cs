using RPGCharacterAnims.Extensions;
using RPGCharacterAnims.Lookups;
using UnityEngine;

namespace RPGCharacterAnims.Actions
{
	public class SwitchWeaponContext
    {
        public string type;
        public string side;

		public Weapon rightWeapon;
        public Weapon leftWeapon;

        public SwitchWeaponContext()
        {
            this.type = "Instant";
            this.side = "None";
            this.rightWeapon = Weapon.Unarmed;
            this.leftWeapon = Weapon.Unarmed;
        }

        public SwitchWeaponContext(string type, string side, Weapon rightWeapon = Weapon.Unarmed, Weapon leftWeapon = Weapon.Unarmed)
        {
            this.type = type;
            this.side = side;
            this.rightWeapon = rightWeapon;
            this.leftWeapon = leftWeapon;
        }

        public void LowercaseStrings()
        {
            type = type.ToLower();
            side = side.ToLower();
        }
    }

    public class SwitchWeapon : BaseActionHandler<SwitchWeaponContext>
    {
        public override bool CanStartAction(RPGCharacterController controller)
        { return !IsActive(); }

        public override bool CanEndAction(RPGCharacterController controller)
        { return IsActive(); }

        protected override void _StartAction(RPGCharacterController controller, SwitchWeaponContext context)
		{
			RPGCharacterWeaponController weaponController = controller.GetComponent<RPGCharacterWeaponController>();

			if (weaponController == null) {
				EndAction(controller);
				return;
			}

			context.LowercaseStrings();

			bool changeRight = false;
			bool sheathRight = false;
			bool unsheathRight = false;
			Weapon fromRightWeapon = controller.rightWeapon;
			Weapon toRightWeapon = context.rightWeapon;

			bool changeLeft = false;
			bool sheathLeft = false;
			bool unsheathLeft = false;
			Weapon fromLeftWeapon = controller.leftWeapon;
			Weapon toLeftWeapon = context.leftWeapon;

			AnimatorWeapon toAnimatorWeapon = 0;

			// Filter which side is changing.
			switch (context.side) {
				case "none":
				case "right":
					changeRight = true;
					if (toRightWeapon.Is2HandedWeapon() && !fromLeftWeapon.HasNoWeapon()) {
						changeLeft = true;
						toLeftWeapon = Weapon.Unarmed;
					}
					break;
				case "both":
					changeLeft = true;
					changeRight = true;
					break;
			}

			// Force Unarmed if sheathing weapons.
			if (context.type == "sheath") {
				if (context.side == "left" || context.side == "dual" || context.side == "both")
				{ toLeftWeapon = Weapon.Unarmed; }
				else { toLeftWeapon = fromLeftWeapon; }

				if (context.side == "none" || context.side == "right" || context.side == "dual" || context.side == "both")
				{ toRightWeapon = Weapon.Unarmed; }
				else { toRightWeapon = fromRightWeapon; }
			}

			toAnimatorWeapon = AnimationData.ConvertToAnimatorWeapon(controller.leftWeapon, controller.rightWeapon);

			// If switching weapons in Armed state or from Dpad.
			if (context.type == "switch") {
				sheathLeft = changeLeft && fromLeftWeapon != toLeftWeapon && !fromLeftWeapon.HasNoWeapon();
				sheathRight = changeRight && fromRightWeapon != toRightWeapon && !fromRightWeapon.HasNoWeapon();
				unsheathLeft = changeLeft && fromLeftWeapon != toLeftWeapon && !toLeftWeapon.HasNoWeapon();
				unsheathRight = changeRight && fromRightWeapon != toRightWeapon && !toRightWeapon.HasNoWeapon();
			}

			// Sheath weapons first if our starting weapon is different from our desired weapon and we're
			// not starting from an Unarmed position.
			if (context.type == "sheath" || context.type == "switch") {
				sheathLeft = changeLeft && fromLeftWeapon != toLeftWeapon && !fromLeftWeapon.HasNoWeapon();
				sheathRight = changeRight && fromRightWeapon != toRightWeapon && !fromRightWeapon.HasNoWeapon();
			}

			// Unsheath a weapon if our starting weapon is different from our desired weapon and we're
			// not ending on an Unarmed position.
			if (context.type == "unsheath" || context.type == "switch") {
				unsheathLeft = changeLeft && fromLeftWeapon != toLeftWeapon && !toLeftWeapon.HasNoWeapon();
				unsheathRight = changeRight && fromRightWeapon != toRightWeapon && !toRightWeapon.HasNoWeapon();
			}

			DebugSwitchWeapon(weaponController, context, changeRight, changeLeft, sheathRight, sheathLeft, unsheathRight, unsheathLeft,
				fromRightWeapon, toRightWeapon, fromLeftWeapon, toLeftWeapon);

			///
			/// Actually make changes to the weapon controller.
			///

			// If Instant Switch.
			if (context.type == "instant") {
				Debug.Log("Instant Switch");
				if (changeLeft && changeRight) { weaponController.InstantWeaponSwitch(toRightWeapon); }
				else if (changeLeft) { weaponController.InstantWeaponSwitch(toLeftWeapon); }
				else if (changeRight) { weaponController.InstantWeaponSwitch(toRightWeapon); }
			}
			// Non-instant weapon switch.
			else {

				// SHEATHING
				if (sheathRight) {
					Debug.Log("Sheath Right - fromRightAnim: "
						+ fromRightWeapon + " > " + "toRightAnim: " + toRightWeapon);
					weaponController.SheathWeapon(fromRightWeapon, toRightWeapon);
				}

				// UNSHEATHING
				if (unsheathRight) {
					Debug.Log("Unsheath Right:" + toRightWeapon);
					weaponController.UnsheathWeapon(toRightWeapon);
				}
			}

			EndSwitch(controller, weaponController, changeRight, toRightWeapon, changeLeft, toLeftWeapon);
		}

		/// <summary>
		/// Updates the weapons in character controller through callback, syncs the weapon visibility, and then ends the action.
		/// </summary>
		/// <param name="controller"></param>
		/// <param name="weaponController">RPGCharacterWeaponController.</param>
		/// <param name="changeRight">If rightWeapon changed.</param>
		/// <param name="toRightWeapon">New rightWeapon number.</param>
		/// <param name="changeLeft">If leftWeapon changed.</param>
		/// <param name="toLeftWeapon">New leftWeapon number.</param>
		private void EndSwitch(RPGCharacterController controller, RPGCharacterWeaponController weaponController, bool changeRight, Weapon toRightWeapon, bool changeLeft, Weapon toLeftWeapon)
		{
			// This callback will update the weapons in character controller after all other
			// coroutines finish.
			weaponController.AddCallback(() => {
				if (changeLeft) { controller.leftWeapon = toLeftWeapon; }
				if (changeRight) { controller.rightWeapon = toRightWeapon; }

				// Turn off the isWeaponSwitching flag and sync weapon object visibility.
				weaponController.SyncWeaponVisibility();
				EndAction(controller);
			});
		}

		private static void DebugSwitchWeapon(RPGCharacterWeaponController weaponController, SwitchWeaponContext context, bool changeRight, bool changeLeft, bool sheathRight,
			bool sheathLeft, bool unsheathRight, bool unsheathLeft, Weapon fromRightWeapon, Weapon toRightWeapon, Weapon fromLeftWeapon,
			Weapon toLeftWeapon)
		{
			if (weaponController.debugSwitchWeaponContext) {
				Debug.Log("===SwitchWeaponContext===");
				Debug.Log($"leftWeapon:{context.leftWeapon}   rightWeapon:{context.rightWeapon}   " +
					$"side:{context.side}    type:{context.type}    " +
					$"changeLeft:{changeLeft}    changeRight:{changeRight}    sheathRight:{sheathRight}    sheathLeft:{sheathLeft}");
				Debug.Log($"fromRightWeapon:{fromRightWeapon}   toRightWeapon:{toRightWeapon}   " +
					$"fromLeftWeapon:{fromLeftWeapon}    toLeftWeapon:{toLeftWeapon}");
			}
		}

		protected override void _EndAction(RPGCharacterController controller)
        {
        }
    }
}