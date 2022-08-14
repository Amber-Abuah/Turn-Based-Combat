namespace RPGCharacterAnims.Lookups
{
	/// <summary>
	/// Enum to use with the "Weapon" parameter of the animator. To convert from a Weapon number,
	/// use WeaponExtensions.ToAnimatorWeapon.
	///
	/// Two-handed weapons have a 1:1 relationship with this enum, but all one-handed weapons use
	/// ARMED.
	/// </summary>
	public enum AnimatorWeapon
	{
        UNARMED = 0,
        TWOHANDSWORD = 1,
    }
}