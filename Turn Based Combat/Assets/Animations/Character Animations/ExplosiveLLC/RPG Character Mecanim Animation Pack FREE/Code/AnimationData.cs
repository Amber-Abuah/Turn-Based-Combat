using RPGCharacterAnims.Extensions;
using RPGCharacterAnims.Lookups;
using UnityEngine;

namespace RPGCharacterAnims
{
	/// <summary>
	/// Static class which contains hardcoded animation constants and helper functions.
	/// </summary>
	public class AnimationData
	{
		/// <summary>
		/// Converts left and right-hand weapon numbers into the legacy weapon number usable by the
		/// animator's "Weapon" parameter.
		/// </summary>
		/// <param name="leftWeapon">Left-hand weapon.</param>
		/// <param name="rightWeapon">Right-hand weapon.</param>
		public static AnimatorWeapon ConvertToAnimatorWeapon(Weapon leftWeapon, Weapon rightWeapon)
		{
			// 2-handed weapon.
			if (rightWeapon.Is2HandedWeapon()) { return ( AnimatorWeapon )rightWeapon; }

			// Unarmed or Relax.
			if (rightWeapon.HasNoWeapon() && leftWeapon.HasNoWeapon()) { return ( AnimatorWeapon )rightWeapon; }

			// Armed.
			return AnimatorWeapon.UNARMED;
		}

		/// <summary>
		/// Returns the duration of an attack animation. Use side 0 (none) for two-handed weapons.
		/// </summary>
		/// <param name="attackSide">Side of the attack: 0- None, 1- Left, 2- Right, 3- Dual.</param>
		/// <param name="weapon">Weapon that's attacking.</param>
		/// <param name="attackNumber">Attack animation number.</param>
		/// <returns>Duration in seconds of attack animation.</returns>
		public static float AttackDuration(Side attackSide, Weapon weapon, int attackNumber)
		{
			var duration = 1f;

			switch (attackSide) {
				case Side.None:						// Unspecified (2-Handed Weapons)
					switch (weapon) {
						case Weapon.TwoHandSword:
							duration = 1.1f;
							break;
						default:
							Debug.LogError("RPG Character: no weapon number " + weapon + " for Side 0");
							break;
					}
					break;

				case Side.Left:						// Left Side
					switch (weapon) {
						case Weapon.Unarmed:
							duration = 0.75f;
							break;					// Unarmed  (1-3)
						default:
							Debug.LogError("RPG Character: no weapon number " + weapon + " for Side 1 (Left)");
							break;
					}
					break;
				case Side.Right:					// Right Side
					switch (weapon) {
						case Weapon.Unarmed:
							duration = 0.75f;
							break;					// Unarmed  (4-6)
						default:
							Debug.LogError("RPG Character: no weapon number " + weapon + " for Side 2 (Right)");
							break;
					}
					break;
			}

			return duration;
		}

		/// <summary>
		/// Returns the duration of the weapon sheath animation.
		/// </summary>
		/// <param name="attackSide">Side of the attack: 0- None, 1- Left, 2- Right, 3- Dual.</param>
		/// <param name="weaponNumber">Weapon being sheathed.</param>
		/// <returns>Duration in seconds of sheath animation.</returns>
		public static float SheathDuration(Side attackSide, Weapon weapon)
		{
			var duration = 1f;

			if (weapon.HasNoWeapon()) { duration = 0f; }
			else if (weapon.Is2HandedWeapon()) { duration = 1.2f; }
			else { duration = 1.05f; }

			return duration;
		}

		/// <summary>
		/// Returns a random attack number usable as the animator's Action parameter.
		/// </summary>
		/// <param name="sideType">Side of the attack: 0- None, 1- Left, 2- Right, 3- Dual.</param>
		/// <param name="weapon">Weapon attacking.</param>
		/// <returns>Attack animation number.</returns>
		public static int RandomAttackNumber(Side sideType, Weapon weapon)
		{
			switch (sideType) {
				case Side.None:
					switch (weapon) {
						case Weapon.TwoHandSword:
							return ( int )AnimationVariations.TwoHandedSwordAttacks.TakeRandom();
						default:
							Debug.LogError($"RPG Character: no weapon number {weapon} for Side 0");
							break;
					}
					break;

				case Side.Left:
					switch (weapon) {
						case Weapon.Unarmed:
							return ( int )AnimationVariations.UnarmedLeftAttacks.TakeRandom();
						default:
							Debug.LogError($"RPG Character: no weapon number {weapon} for Side 1 (Left)");
							break;
					}
					break;
				case Side.Right:
					switch (weapon) {
						case Weapon.Unarmed:
							return ( int )AnimationVariations.UnarmedRightAttacks.TakeRandom();
						default:
							Debug.LogError($"RPG Character: no weapon number {weapon} for Side 2 (Right)");
							break;
					}
					break;
			}

			return 1;
		}

		public static Vector3 HitDirection(HitType hitType)
		{
			switch (hitType) {
				case HitType.Back1:
					return Vector3.forward;
				case HitType.Left1:
					return Vector3.right;
				case HitType.Right1:
					return Vector3.left;
				case HitType.Forward1:
				case HitType.Forward2:
				default:
					return Vector3.back;
			}
		}

		public static Vector3 HitDirection(KnockbackType hitType)
		{
			switch (hitType) {
				case KnockbackType.Knockback1:
				case KnockbackType.Knockback2:
				default:
					return Vector3.back;
			}
		}

		public static Vector3 HitDirection(KnockdownType hitType)
		{ return Vector3.back; }
	}
}