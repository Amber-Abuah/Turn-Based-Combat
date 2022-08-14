using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims.Extensions
{
	public static class WeaponExtensions
	{
		/// <summary>
		/// Checks if the weapon is a 2 Handed weapon.
		/// </summary>
		/// <param name="weapon">Weapon value to check.</param>
		/// <returns>True if 2 Handed, false if not.</returns>
		public static bool Is2HandedWeapon(this Weapon weapon)
		{ return weapon == Weapon.TwoHandSword; }

		/// <summary>
		/// Checks if the weapon is equipped, i.e not Relaxing, or Unarmed.
		/// </summary>
		/// <param name="weapon">Weapon value to check.</param>
		/// <returns>True or false.</returns>
		public static bool HasEquippedWeapon(this Weapon weapon)
		{ return weapon != Weapon.Unarmed; }

		/// <summary>
		/// Checks if the weapon is empty, i.e Relaxing, or Unarmed.
		/// </summary>
		/// <param name="weapon">Weapon value to check.</param>
		/// <returns>True or false.</returns>
		public static bool HasNoWeapon(this Weapon weapon)
		{ return weapon == Weapon.Unarmed; }

		/// <summary>
		/// Returns true if the weapon number can use IKHands.
		/// </summary>
		/// <param name="weapon">Weapon to test.</param>
		public static bool IsIKWeapon(this Weapon weapon)
		{ return weapon == Weapon.TwoHandSword; }

		/// <summary>
		/// This converts the Weapon into AnimatorWeapon, which is used in the Animator component to determine the
		/// proper state to set the character into, because all 1 Handed weapons use the ARMED state. 2 Handed weapons,
		/// Unarmed, and Relax map directly from Weapon to AnimatorWeapon.
		/// </summary>
		/// <param name="weapon">Weapon to convert.</param>
		/// <returns></returns>
		public static AnimatorWeapon ToAnimatorWeapon(this Weapon weapon)
		{
			if (weapon == Weapon.Unarmed || weapon == Weapon.TwoHandSword)
			{ return ( AnimatorWeapon )weapon; }

			return AnimatorWeapon.UNARMED;
		}

		/// <summary>
		/// Checks if the animator weapon is a 2 Handed weapon.
		/// </summary>
		/// <param name="weapon">Weapon value to check.</param>
		/// <returns>True if 1 Handed, false if not.</returns>
		public static bool Is2HandedAnimWeapon(this AnimatorWeapon weapon)
		{ return weapon == AnimatorWeapon.TWOHANDSWORD; }

		/// <summary>
		/// Checks if the animator weapon is Unarmed or Relaxed.
		/// </summary>
		/// <param name="weapon">Weapon value to check.</param>
		/// <returns>True if 1 Handed, false if not.</returns>
		public static bool HasNoAnimWeapon(this AnimatorWeapon weapon)
		{ return weapon == AnimatorWeapon.UNARMED; }
	}
}