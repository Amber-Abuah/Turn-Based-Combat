using System.Collections;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Extensions;
using RPGCharacterAnims.Lookups;
using UnityEngine;

namespace RPGCharacterAnims
{
    public class RPGCharacterWeaponController : MonoBehaviour
    {
		// Components.
        private RPGCharacterController rpgCharacterController;
        private Animator animator;
        private CoroutineQueue coroQueue;

		[Header("Debug Options")]
		public bool debugWalkthrough = true;
		public bool debugSwitchWeaponContext = true;
		public bool debugDoWeaponSwitch = true;
		public bool debugWeaponVisibility = true;
		public bool debugSetAnimator = true;

		[HideInInspector] bool isWeaponSwitching = false;

		[Header("Weapon Models")]
        public GameObject twoHandSword;

        private void Awake()
        {
            coroQueue = new CoroutineQueue(1, StartCoroutine);
            rpgCharacterController = GetComponent<RPGCharacterController>();
            rpgCharacterController.SetHandler(HandlerTypes.SwitchWeapon, new SwitchWeapon());

            // Find the Animator component.
            animator = GetComponentInChildren<Animator>();

			// Character starts in Unarmed so hide all weapons.
            StartCoroutine(_HideAllWeapons(false, false));
        }

        private void Start()
        {
            // Listen for the animator's weapon switch event.
            var animatorEvents = animator.gameObject.GetComponent<RPGCharacterAnimatorEvents>();
            animatorEvents.OnWeaponSwitch.AddListener(WeaponSwitch);
		}

        /// <summary>
        /// Add a callback to the coroutine queue to be executed in sequence.
        /// </summary>
        /// <param name="callback">The action to call.</param>
        public void AddCallback(System.Action callback)
        { coroQueue.RunCallback(callback); }

        /// <summary>
        /// Queue a command to unsheath a weapon.
        /// </summary>
        /// <param name="weapon">Weapon to unsheath.</param>
        public void UnsheathWeapon(Weapon weapon)
        {
			if (debugWalkthrough) { Debug.Log("UnsheathWeapon:" + weapon); }
			coroQueue.Run(_UnSheathWeapon(weapon));
        }

        /// <summary>
        /// Async method to unsheath a weapon.
        /// </summary>
        /// <param name="weapon">Weapon to unsheath.</param>
        /// <returns>IEnumerator for use with StartCoroutine.</returns>
        private IEnumerator _UnSheathWeapon(Weapon weapon)
        {
			if (debugWalkthrough) { Debug.Log($"UnsheathWeapon - weapon:{weapon}"); }

            isWeaponSwitching = true;

			var currentAnimatorWeapon = ( AnimatorWeapon )animator.GetInteger(AnimationParameters.Weapon);
			var currentWeaponType = (Weapon) animator.GetInteger(AnimationParameters.Weapon);

            // Switching to 2Handed weapon.
            if (weapon.Is2HandedWeapon()) {
				if (debugWalkthrough) { Debug.Log($"Switching to 2Handed Weapon:{weapon}"); }

				// Switching from 2Handed weapon.
				if (currentWeaponType.Is2HandedWeapon()) {
					if (debugWalkthrough) { Debug.Log("Switching from 2Handed weapon."); }
					DoWeaponSwitch(( int )AnimatorWeapon.UNARMED, weapon, weapon.ToAnimatorWeapon(), Side.Unchanged, false);

					// Wait for WeaponSwitch() to happen then update Animator.
					yield return new WaitForSeconds(0.75f);
					SetWeaponWithDebug(weapon.ToAnimatorWeapon(), -2, currentWeaponType, Weapon.Unarmed, Side.Unchanged);
				}
				else {
                    DoWeaponSwitch(( int )AnimatorWeapon.UNARMED, weapon, weapon.ToAnimatorWeapon(), Side.Unchanged, false);

					// Wait for WeaponSwitch() to happen then update Animator.
					yield return new WaitForSeconds(0.75f);
					SetWeaponWithDebug(weapon.ToAnimatorWeapon(), -2, weapon, Weapon.Unarmed, Side.Unchanged);
				}
			}
        }

		/// <summary>
		/// Queue a command to sheath the current weapon and switch to a new one.
		/// </summary>
		/// <param name="fromWeapon">Which weapon to sheath.</param>
		/// <param name="toWeapon">Target weapon if immediately unsheathing new weapon.</param>
		public void SheathWeapon(Weapon fromWeapon, Weapon toWeapon)
        {
			if (debugWalkthrough) { Debug.Log($"SheathWeapon - fromWeapon:{fromWeapon}  toWeapon:{toWeapon}"); }
            coroQueue.Run(_SheathWeapon(fromWeapon, toWeapon));
        }

        /// <summary>
        /// Async method to sheath the current weapon and switch to a new one.
        /// </summary>
        /// <param name="weaponToSheath">Which weapon to sheath.</param>
        /// <param name="weaponToUnsheath">Target weapon if immediately unsheathing a new weapon.</param>
        /// <returns>IEnumerator for use with StartCoroutine.</returns>
        public IEnumerator _SheathWeapon(Weapon weaponToSheath, Weapon weaponToUnsheath)
        {
			if (debugWalkthrough) { Debug.Log($"Sheath Weapon - weaponToSheath:{weaponToSheath}  weaponToUnsheath:{weaponToUnsheath}"); }

            var currentWeaponType = (Weapon) animator.GetInteger(AnimationParameters.Weapon);
			var currentAnimatorWeapon = ( AnimatorWeapon )animator.GetInteger(AnimationParameters.Weapon);

			// Reset for animation events.
			isWeaponSwitching = true;

            // Putting away a weapon.
            if (weaponToUnsheath.HasNoWeapon()) {
				if (debugWalkthrough) { Debug.Log("Putting away a weapon."); }

				// Have at least 1 weapon.
				if (rpgCharacterController.rightWeapon.HasEquippedWeapon()
					|| rpgCharacterController.leftWeapon.HasEquippedWeapon()) {

					if (debugWalkthrough) { Debug.Log("Sheath 2Handed weapon."); }
					DoWeaponSwitch(( int )weaponToUnsheath, weaponToSheath, currentAnimatorWeapon, Side.Unchanged, true);

					// Wait for WeaponSwitch() to happen then update Animator.
					yield return new WaitForSeconds(0.55f);
					SetWeaponWithDebug(weaponToUnsheath.ToAnimatorWeapon(), -2, Weapon.Unarmed, Weapon.Unarmed, Side.Unchanged);
				}
			}
        }

        /// <summary>
        /// Switch to the weapon number instantly.
        /// </summary>
        /// <param name="weapon">Weapon to switch to.</param>
        public void InstantWeaponSwitch(Weapon weapon)
        { coroQueue.Run(_InstantWeaponSwitch(weapon)); }

		/// <summary>
		/// Async method to instant weapon switch.
		/// </summary>
		/// <param name="weaponNumber">Weapon number to switch to.</param>
		/// <returns>IEnumerator for use with StartCoroutine.</returns>
		/// /// <summary>
		public IEnumerator _InstantWeaponSwitch(Weapon weapon)
		{
			if (debugWalkthrough) { Debug.Log($"_InstantWeaponSwitch:{weapon}"); }
			animator.SetAnimatorTrigger(AnimatorTrigger.InstantSwitchTrigger);
			rpgCharacterController.SetIKOff();
			StartCoroutine(_HideAllWeapons(false, false));

			// 2Handed.
			if (weapon.Is2HandedWeapon()) {
				if (debugWalkthrough) { Debug.Log($"InstantSwitch to 2HandedWeapon - weapon:{weapon}"); }

				// 2Handed weapons map directly to AnimatorWeapon.
				animator.SetInteger(AnimationParameters.Weapon, ( int )weapon);
				rpgCharacterController.rightWeapon = Weapon.Unarmed;
				rpgCharacterController.leftWeapon = Weapon.Unarmed;
				animator.SetInteger(AnimationParameters.LeftWeapon, 0);
				animator.SetInteger(AnimationParameters.RightWeapon, 0);
				StartCoroutine(_HideAllWeapons(false, false));
				StartCoroutine(_WeaponVisibility(weapon, true));

				if (weapon.IsIKWeapon())
				{ rpgCharacterController.SetIKOn(( Weapon )animator.GetInteger(AnimationParameters.Weapon)); }
			}
			// Switching to Unarmed or Relax which map directly.
			else {
				animator.SetInteger(AnimationParameters.Weapon, ( int )weapon);
				rpgCharacterController.rightWeapon = Weapon.Unarmed;
				rpgCharacterController.leftWeapon = Weapon.Unarmed;
				animator.SetInteger(AnimationParameters.LeftWeapon, 0);
				animator.SetInteger(AnimationParameters.RightWeapon, 0);
				animator.SetInteger(AnimationParameters.Side, 0);
				StartCoroutine(_HideAllWeapons(false, false));
			}
			yield return null;
		}

		/// <summary>
		/// Performs the weapon switch by setting Animator parameters then triggering Sheath or Unsheath animation.
		/// </summary>
		/// <param name="weaponSwitch">AnimatorWeapon switching from.</param>
		/// <param name="weapon">The weapon switching to.</param>
		/// <param name="animatorWeapon">Weapon enum switching to.</param>
		/// <param name="side">Weapon side. -1=Leave existing, 1=Left, 2=Right, 3=Dual</param>
		/// <param name="sheath">"sheath" or "unsheath".</param>
		private void DoWeaponSwitch(int weaponSwitch, Weapon weapon, AnimatorWeapon animatorWeapon, Side side, bool sheath)
		{
			if (debugDoWeaponSwitch)
			{ Debug.Log($"DO WEAPON SWITCH - weaponSwitch:{weaponSwitch}  weapon:{weapon}  animatorWeapon:{animatorWeapon}  side:{side}  sheath:{sheath}"); }

			// Lock character movement for switch unless has moving sheath/unsheath anims.
			rpgCharacterController.Lock(!rpgCharacterController.isMoving, true, true, 0f, 1f);

			// Set WeaponSwitch and Weapon.
			animator.SetInteger(AnimationParameters.WeaponSwitch, weaponSwitch);
			animator.SetInteger(AnimationParameters.Weapon, ( int )animatorWeapon);

			// Set LeftRight if applicable.
			if (side != Side.Unchanged) { animator.SetInteger(AnimationParameters.Side, ( int )side); }

			// Sheath.
			if (sheath) {
				animator.SetAnimatorTrigger(AnimatorTrigger.WeaponSheathTrigger);
				StartCoroutine(_WeaponVisibility(weapon, false));

				// If using IKHands, trigger IK blend.
				if (rpgCharacterController.ikHands != null)
				{ rpgCharacterController.ikHands.BlendIK(false, 0f, 0.2f, weapon); }

			}
			// Unsheath.
			else {
				animator.SetAnimatorTrigger(AnimatorTrigger.WeaponUnsheathTrigger);
				StartCoroutine(_WeaponVisibility(weapon, true));

				// If using IKHands and IK weapon, trigger IK blend.
				if (rpgCharacterController.ikHands != null && weapon.IsIKWeapon())
				{ rpgCharacterController.ikHands.BlendIK(true, 0.75f, 1, weapon); }
			}
		}

		/// <summary>
		/// Sets the animation state for weapons with debug option.
		/// </summary>
		/// <param name="animatorWeapon">Animator weapon number. Use AnimationData's AnimatorWeapon enum.</param>
		/// <param name="weaponSwitch">Weapon switch. -2 leaves parameter unchanged.</param>
		/// <param name="leftWeapon">Left weapon number. Use Weapon.cs enum.</param>
		/// <param name="rightWeapon">Right weapon number. Use Weapon.cs enum.</param>
		/// <param name="weaponSide">Weapon side: 0-None, 1-Left, 2-Right, 3-Dual.</param>
		private void SetWeaponWithDebug(AnimatorWeapon animatorWeapon, int weaponSwitch, Weapon leftWeapon, Weapon rightWeapon, Side weaponSide)
		{
			if (debugSetAnimator) { Debug.Log($"SET ANIMATOR - Weapon:{animatorWeapon}  WeaponSwitch:{weaponSwitch}  Lweapon:{leftWeapon}  Rweapon:{rightWeapon}  Weaponside:{weaponSide}"); }

			animator.SetWeapons(animatorWeapon, weaponSwitch, leftWeapon, rightWeapon, weaponSide);
		}

		/// <summary>
		/// Callback to use with Animator's WeaponSwitch event.
		/// </summary>
		public void WeaponSwitch()
        {
            if (isWeaponSwitching) { isWeaponSwitching = false; }
        }

        /// <summary>
        /// Helper method used by other weapon visibility methods to safely set a weapon object's visibility.
        /// This will work even if the object is not set in the component parameters.
        /// </summary>
        /// <param name="weaponObject">Weapon to update.</param>
        /// <param name="visibility">Visibility status.</param>
        public void SafeSetVisibility(GameObject weaponObject, bool visibility)
        {
            if (weaponObject != null) { weaponObject.SetActive(visibility); }
        }

        /// <summary>
        /// Hide all weapon objects and set the animator and the character controller to the unarmed state.
        /// </summary>
        public void HideAllWeapons()
        { StartCoroutine(_HideAllWeapons(false, true)); }

        /// <summary>
        /// Async method to all weapon objects and set the animator and the character controller to the unarmed state.
        /// </summary>
        /// <param name="timed">Whether to wait until a period of time to hide the weapon.</param>
        /// <param name="resetToUnarmed">Whether to reset the animator and the character controller to the unarmed state.</param>
        /// <returns>IEnumerator for use with StartCoroutine.</returns>
        public IEnumerator _HideAllWeapons(bool timed, bool resetToUnarmed)
        {
            if (timed) { while (!isWeaponSwitching) { yield return null; } }

            // Reset to Unarmed.
            if (resetToUnarmed) {
                animator.SetInteger(AnimationParameters.Weapon, 0);
                rpgCharacterController.rightWeapon = Weapon.Unarmed;
                rpgCharacterController.leftWeapon = Weapon.Unarmed;
                StartCoroutine(_WeaponVisibility(rpgCharacterController.leftWeapon, false));
                animator.SetInteger(AnimationParameters.RightWeapon, 0);
                animator.SetInteger(AnimationParameters.LeftWeapon, 0);
                animator.SetSide(Side.None);
            }
            SafeSetVisibility(twoHandSword, false);
        }

        /// <summary>
        /// Set a single weapon's visibility.
        /// </summary>
        /// <param name="weaponNumber">Weapon object to set.</param>
        /// <param name="visible">Whether to set it visible or not.</param>
        /// <param name="dual">Whether to update the same weapon in the other hand as well.</param>
        public IEnumerator _WeaponVisibility(Weapon weaponNumber, bool visible)
        {
			if (debugWeaponVisibility) { Debug.Log($"WeaponVisibility:{weaponNumber}   Visible:{visible}"); }

			while (isWeaponSwitching) { yield return null; }
            var weaponType = (Weapon)weaponNumber;
			switch (weaponType) { case Weapon.TwoHandSword: SafeSetVisibility(twoHandSword, visible); break; }
            yield return null;
        }

        /// <summary>
        /// Sync weapon object visibility to the current weapons in the RPGCharacterController.
        /// </summary>
        public void SyncWeaponVisibility()
        { coroQueue.Run(_SyncWeaponVisibility()); }

        /// <summary>
        /// Async method to sync weapon object visiblity to the current weapons in RPGCharacterController.
        /// This will wait for weapon switching to finish. If your aim is to force this update, call WeaponSwitch
        /// first. This will stop the _HideAllWeapons and _WeaponVisibility coroutines.
        /// </summary>
        /// <returns>IEnumerator for use with.</returns>
        private IEnumerator _SyncWeaponVisibility()
        {
            while (isWeaponSwitching && !(rpgCharacterController.canAction && rpgCharacterController.canMove))
			{ yield return null; }

            StopCoroutine(nameof(_HideAllWeapons));
            StopCoroutine(nameof(_WeaponVisibility));

            SafeSetVisibility(twoHandSword, false);

            var rightWeaponType = (Weapon)rpgCharacterController.rightWeapon;
            switch (rightWeaponType) { case Weapon.TwoHandSword: SafeSetVisibility(twoHandSword, true); break; }
        }
    }
}