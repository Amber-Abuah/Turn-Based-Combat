using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Extensions;
using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims
{
	/// <summary>
    /// RPGCharacterController is the main entry point for triggering animations and holds all the
    /// state related to a character. It is the core component of this package–no other controller
    /// will run without it.
    /// </summary>
    public class RPGCharacterController : MonoBehaviour
    {
	    /// <summary>
        /// Event called when actions are locked by an animation.
        /// </summary>
        public event System.Action OnLockActions = delegate { };

        /// <summary>
        /// Event called when actions are unlocked at the end of an animation.
        /// </summary>
        public event System.Action OnUnlockActions = delegate { };

        /// <summary>
        /// Event called when movement is locked by an animation.
        /// </summary>
        public event System.Action OnLockMovement = delegate { };

        /// <summary>
        /// Event called when movement is unlocked at the end of an animation.
        /// </summary>
        public event System.Action OnUnlockMovement = delegate { };

        /// <summary>
        /// Unity Animator component.
        /// </summary>
        [HideInInspector] public Animator animator;

        /// <summary>
        /// Animation speed control. Doesn't affect lock timing.
        /// </summary>
        public float animationSpeed = 1;

		/// <summary>
		/// IKHands component.
		/// </summary>
		[HideInInspector] public IKHands ikHands;

		/// <summary>
		/// Target for Aiming/Strafing.
		/// </summary>
		public Transform target;

		/// <summary>
		/// Returns whether the character can take actions.
		/// </summary>
		public bool canAction => _canAction && !isNavigating;
		private bool _canAction;

        /// <summary>
        /// Returns whether the character can face.
        /// </summary>
        public bool canFace => _canFace;
        private bool _canFace = true;

        /// <summary>
        /// Returns whether the character can move.
        /// </summary>
        public bool canMove => _canMove;
        private bool _canMove;

        /// <summary>
        /// Returns whether the character can strafe.
        /// </summary>
        public bool canStrafe => _canStrafe;
        private bool _canStrafe = true;

        /// <summary>
        /// Returns whether the AcquiringGround action is active, signifying that the character is
        /// landing on the ground. AcquiringGround is added by RPGCharacterMovementController.
        /// </summary>
		public bool acquiringGround => TryGetHandlerActive(HandlerTypes.AcquiringGround);

        /// <summary>
		/// Returns whether the Aim action is active.
		/// </summary>
		public bool isAiming => TryGetHandlerActive(HandlerTypes.Aim);

        /// <summary>
		/// Returns whether the Attack action is active.
		/// </summary>
		public bool isAttacking => _isAttacking;
		private bool _isAttacking;

        /// <summary>
		/// Returns whether the Facing action is active.
		/// </summary>
		public bool isFacing => TryGetHandlerActive(HandlerTypes.Face);

        /// <summary>
		/// Returns whether the Fall action is active. Fall is added by
		/// RPGCharacterMovementController.
		/// </summary>
		public bool isFalling => TryGetHandlerActive(HandlerTypes.Fall);

        /// <summary>
		/// Returns whether the Idle action is active. Idle is added by
		/// RPGCharacterMovementController.
		/// </summary>
		public bool isIdle => TryGetHandlerActive(HandlerTypes.Idle);

        /// <summary>
		/// Returns whether the Move action is active. Idle is added by
		/// RPGCharacterMovementController.
		/// </summary>
		public bool isMoving => TryGetHandlerActive(HandlerTypes.Move);

        /// <summary>
		/// Returns whether the Navigation action is active. Navigation is added by
		/// RPGCharacterNavigationController.
		/// </summary>
		public bool isNavigating => TryGetHandlerActive(HandlerTypes.Navigation);

		/// <summary>
		/// Returns whether the Roll action is active. Roll is added by
		/// RPGCharacterMovementController.
		/// </summary>
		public bool isRolling => TryGetHandlerActive(HandlerTypes.Roll);

        /// <summary>
		/// Returns whether the Roll action is active. Roll is added by
		/// RPGCharacterMovementController.
		/// </summary>
		public bool isKnockback => TryGetHandlerActive(HandlerTypes.Knockback);

        /// <summary>
		/// Returns whether the Roll action is active. Roll is added by
		/// RPGCharacterMovementController.
		/// </summary>
		public bool isKnockdown => TryGetHandlerActive(HandlerTypes.Knockdown);

        /// <summary>
		/// Returns whether the Strafe action is active.
		/// </summary>
		public bool isStrafing => TryGetHandlerActive(HandlerTypes.Strafe);

        /// <summary>
        /// Returns whether the MaintainingGround action is active, signifying that the character
        /// is on the ground. MaintainingGround is added by RPGCharacterMovementController. If the
        /// action does not exist, this defaults to true.
        /// </summary>
        public bool maintainingGround => TryGetHandlerActive(HandlerTypes.MaintainingGround);

        /// <summary>
        /// Vector3 for move input. Use SetMoveInput to change this.
        /// </summary>
        public Vector3 moveInput => _moveInput;
        private Vector3 _moveInput;

        /// <summary>
        /// Vector3 for aim input. Use SetAimInput to change this.
        /// </summary>
        public Vector3 aimInput => _aimInput;
        private Vector3 _aimInput;

        /// <summary>
        /// Vector3 for facing. Use SetFaceInput to change this.
        /// </summary>
        public Vector3 faceInput => _faceInput;
        private Vector3 _faceInput;

        /// <summary>
        /// Vector3 for jump input. Use SetJumpInput to change this.
        /// </summary>
        public Vector3 jumpInput => _jumpInput;
        private Vector3 _jumpInput;

        /// <summary>
        /// Camera relative input in the XZ plane. This is calculated when SetMoveInput is called.
        /// </summary>
        public Vector3 cameraRelativeInput => _cameraRelativeInput;
        private Vector3 _cameraRelativeInput;

		/// <summary>
		/// Integer weapon number for the right hand. See the Weapon enum in AnimationData.cs for a
		/// full list.
		/// </summary>
		[HideInInspector] public Weapon rightWeapon = Weapon.Unarmed;

		/// <summary>
		/// Integer weapon number for the left hand. See the Weapon enum in AnimationData.cs for a
		/// full list.
		/// </summary>
		[HideInInspector] public Weapon leftWeapon = Weapon.Unarmed;

		/// <summary>
		/// Returns whether the character is holding a two-handed weapon. Two-handed weapons are
		/// "held" in the right hand.
		/// </summary>
		public bool hasTwoHandedWeapon => rightWeapon.Is2HandedWeapon();

		/// <summary>
		/// Returns whether the character is in Unarmed or Relax state.
		/// </summary>
		public bool hasNoWeapon => rightWeapon.HasNoWeapon() && leftWeapon.HasNoWeapon();

		private Dictionary<string, IActionHandler> actionHandlers = new Dictionary<string, IActionHandler>();

		#region Initialization

        private void Awake()
        {
            // Setup Animator, add AnimationEvents script.
            animator = GetComponentInChildren<Animator>();

            if (!animator) {
                Debug.LogError("ERROR: THERE IS NO ANIMATOR COMPONENT ON CHILD OF CHARACTER.");
                Debug.Break();
            }

            animator.gameObject.AddComponent<RPGCharacterAnimatorEvents>();
            animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            animator.SetInteger(AnimationParameters.Weapon, 0);
            animator.SetInteger(AnimationParameters.WeaponSwitch, 0);

			// Setup IKhands if used.
            ikHands = GetComponentInChildren<IKHands>();

            SetHandler(HandlerTypes.Attack, new Attack());
            SetHandler(HandlerTypes.Face, new SimpleActionHandler(StartFace, EndFace));
            SetHandler(HandlerTypes.Null, new Null());
            SetHandler(HandlerTypes.SlowTime, new SlowTime());
            SetHandler(HandlerTypes.Strafe, new SimpleActionHandler(StartStrafe, EndStrafe));

            // Unlock actions and movement.
            Unlock(true, true);

			// Set Aim Input.
			SetAimInput(target.transform.position);
		}

		#endregion

		#region Actions

		/// <summary>
		/// Set an action handler.
		/// </summary>
		/// <param name="action">Name of the action.</param>
		/// <param name="handler">The handler associated with this action.</param>
		public void SetHandler(string action, IActionHandler handler)
        { actionHandlers[action] = handler; }

        /// <summary>
        /// Get an action handler by name. If it doesn't exist, return the Null handler.
        /// </summary>
        /// <param name="action">Name of the action.</param>
        public IActionHandler GetHandler(string action)
        {
            if (HandlerExists(action)) { return actionHandlers[action]; }
            Debug.LogError("RPGCharacterController: No handler for action \"" + action + "\"");
            return actionHandlers[HandlerTypes.Null];
        }

        /// <summary>
        /// Check if a handler exists.
        /// </summary>
        /// <param name="action">Name of the action.</param>
        /// <returns>Whether or not that action exists on this controller.</returns>
        public bool HandlerExists(string action)
        { return actionHandlers.ContainsKey(action); }

        public bool TryGetHandlerActive(string action)
		{ return HandlerExists(action) && IsActive(action); }

        /// <summary>
        /// Check if an action is active.
        /// </summary>
        /// <param name="action">Name of the action.</param>
        /// <returns>Whether the action is active. If the action does not exist, returns false.</returns>
        public bool IsActive(string action)
        { return GetHandler(action).IsActive(); }

        /// <summary>
        /// Check if an action can be started.
        /// </summary>
        /// <param name="action">Name of the action.</param>
        /// <returns>Whether the action can be started. If the action does not exist, returns false.</returns>
        public bool CanStartAction(string action)
        { return GetHandler(action).CanStartAction(this); }

        public bool TryStartAction(string action, object context = null)
        {
	        if (!CanStartAction(action)) { return false; }

	        if (context == null) { StartAction(action); }
	        else { StartAction(action, context);}

	        return true;
        }

        public bool TryEndAction(string action)
        {
	        if (!CanEndAction(action)) { return false; }
	        EndAction(action);
	        return true;
        }

        /// <summary>
        /// Check if an action can be ended.
        /// </summary>
        /// <param name="action">Name of the action.</param>
        /// <returns>Whether the action can be ended. If the action does not exist, returns false.</returns>
        public bool CanEndAction(string action)
        { return GetHandler(action).CanEndAction(this); }

        /// <summary>
        /// Start the action with the specified context. If the action does not exist, there is no effect.
        /// </summary>
        /// <param name="action">Name of the action.</param>
        /// <param name="context">Contextual object used by this action. Leave blank if none is required.</param>
        public void StartAction(string action, object context = null)
        { GetHandler(action).StartAction(this, context); }

        /// <summary>
        /// End the action. If the action does not exist, there is no effect.
        /// </summary>
        /// <param name="action">Name of the action.</param>
        public void EndAction(string action)
        { GetHandler(action).EndAction(this); }

        #endregion

        #region Updates

        private void LateUpdate()
        {
            // Update Animator animation speed.
            animator.SetFloat(AnimationParameters.AnimationSpeed, animationSpeed);
        }

        #endregion

        #region Input

        /// <summary>
        /// Set move input. This method expects the x-axis to be left-right input and the
        /// y-axis to be up-down input.
        ///
        /// The z-axis is ignored, but the type is a Vector3 in case you wish to use the z-axis.
        ///
        /// This method computes CameraRelativeInput using the x and y axis of the move input
        /// and the main camera, producing a normalized Vector3 in the XZ plane.
        /// </summary>
        /// <param name="_moveInput">Vector3 move input</param>
        public void SetMoveInput(Vector3 _moveInput)
        {
            this._moveInput = _moveInput;

            // Forward vector relative to the camera along the x-z plane.
            var forward = Camera.main.transform.TransformDirection(Vector3.forward);
            forward.y = 0;
            forward = forward.normalized;

            // Right vector relative to the camera always orthogonal to the forward vector.
            var right = new Vector3(forward.z, 0, -forward.x);
            var relativeVelocity = _moveInput.x * right + _moveInput.y * forward;

            // Reduce input for diagonal movement.
            if (relativeVelocity.magnitude > 1) { relativeVelocity.Normalize(); }

            _cameraRelativeInput = relativeVelocity;
        }

        /// <summary>
        /// Set facing input. This is a position in world space of the object that the character
        /// is facing towards.
        /// </summary>
        /// <param name="_faceInput">Vector3 face input.</param>
        public void SetFaceInput(Vector3 _faceInput)
        { this._faceInput = _faceInput; }

        /// <summary>
        /// Set aim input. This is a position in world space of the object that the character
        /// is aiming at, so that you can easily lock on to a moving target.
        /// </summary>
        /// <param name="_aimInput">Vector3 aim input.</param>
        public void SetAimInput(Vector3 _aimInput)
        { this._aimInput = _aimInput; }

        /// <summary>
        /// Set jump input. Use this with Vector3.up and Vector3.down (y-axis).
        ///
        /// The X and Z axes are  ignored, but the type is a Vector3 in case you wish to
        /// use the X and Z axes for other actions.
        /// </summary>
        /// <param name="_jumpInput">Vector3 jump input.</param>
        public void SetJumpInput(Vector3 _jumpInput)
        { this._jumpInput = _jumpInput; }

        #endregion

        #region Movement

        /// <summary>
        /// Dive Roll.
        ///
        /// Use the "DiveRoll" action for a friendly interface.
        /// </summary>
        /// <param name="rollType">1- Forward.</param>
        public void DiveRoll(DiveRollType rollType)
        {
	        animator.TriggerDiveRoll(rollType);
            Lock(true, true, true, 0, 1f);
			SetIKPause(1.05f);
        }

        /// <summary>
        /// Knockback in the specified direction.
        ///
        /// Use the "Knockback" action for a friendly interface. Forwards only for Unarmed state.
        /// </summary>
        /// <param name="direction">1- Backwards, 2- Backward version2.</param>
        public void Knockback(KnockbackType direction)
        {
	        animator.TriggerKnockback(direction);
			switch (direction) {
				case KnockbackType.Knockback1: SetIKPause(1.125f);
					Lock(true, true, true, 0, 1f);
					break;
				case KnockbackType.Knockback2: SetIKPause(1f);
					Lock(true, true, true, 0, 0.8f);
					break;
			}
        }

        /// <summary>
        /// Knockdown in the specified direction. Currently only backwards.
        ///
        /// Use the "Knockdown" action for a friendly interface.
        /// </summary>
        /// <param name="direction">1- Backwards.</param>
        public void Knockdown(KnockdownType direction)
        {
	        animator.TriggerKnockdown(direction);
            Lock(true, true, true, 0, 5.25f);
			SetIKPause(5.25f);
		}

		#endregion

		#region Combat

        /// <summary>
        /// Trigger an attack animation.
        ///
        /// Use the "Attack" action for a friendly interface.
        /// </summary>
        /// <param name="attackNumber">Animation number to play. See AnimationData.RandomAttackNumber for details.</param>
        /// <param name="attackSide">Side of the attack: 0- None, 1- Left, 2- Right, 3- Dual.</param>
        /// <param name="leftWeapon">Left side weapon. See Weapon enum in AnimationData.cs.</param>
        /// <param name="rightWeapon">Right-hand weapon. See Weapon enum in AnimationData.cs.</param>
        /// <param name="duration">Duration in seconds that animation is locked.</param>
        public void Attack(int attackNumber, Side attackSide, Weapon leftWeapon, Weapon rightWeapon, float duration)
        {
	        animator.SetSide(attackSide);
			_isAttacking = true;
            Lock(true, true, true, 0, duration);

			// Trigger the animation.
			var attackTriggerType = AnimatorTrigger.AttackTrigger;
			animator.SetActionTrigger(attackTriggerType, attackNumber);
		}

        /// <summary>
        /// Trigger the running attack animation.
        ///
        /// Use the "Attack" action for a friendly interface.
        /// </summary>
        /// <param name="side">Side of the attack: 0- None, 1- Left, 2- Right, 3- Dual.</param>
        /// <param name="leftWeapon">Whether to attack on the left side.</param>
        /// <param name="rightWeapon">Whether to attack on the right side.</param>
        /// <param name="twoHandedWeapon">If wielding a two-handed weapon.</param>
        public void RunningAttack(Side side, bool leftWeapon, bool rightWeapon, bool twoHandedWeapon)
        {
			if (side == Side.Right && rightWeapon) { animator.SetActionTrigger(AnimatorTrigger.AttackTrigger, 4); }
			else if (hasNoWeapon) {
				animator.SetSide(side);
				animator.SetActionTrigger(AnimatorTrigger.AttackTrigger, 1);
			}
        }

        /// <summary>
        /// Run left and right while still facing a target.
        ///
        /// Use the "Face" action for a friendly interface.
        /// </summary>
        public void StartFace()
        {
        }

        /// <summary>
        /// Stop facing.
        ///
        /// Use the "Face" action for a friendly interface.
        /// </summary>
        public void EndFace()
        {
        }

        /// <summary>
        /// Strafe left and right while still facing a target.
        ///
        /// Use the "Strafe" action for a friendly interface.
        /// </summary>
        public void StartStrafe()
        {
        }

        /// <summary>
        /// Stop strafing.
        ///
        /// Use the "Strafe" action for a friendly interface.
        /// </summary>
        public void EndStrafe()
        {
        }

        /// <summary>
        /// Get hit with an attack.
        ///
        /// Use the "GetHit" action for a friendly interface.
        /// </summary>
        public void GetHit(int hitNumber)
        {
            animator.TriggerGettingHit(hitNumber);
			Lock(true, true, true, 0.1f, 0.4f);
			SetIKPause(0.6f);
		}

        #endregion

        #region Misc

        /// <summary>
        /// Gets the object with the animator on it. Useful if that object is a child of this one.
        /// </summary>
        /// <returns>GameObject to which the animator is attached.</returns>
        public GameObject GetAnimatorTarget()
        { return animator.gameObject; }

		/// <summary>
		/// Returns the current animation length of the given animation layer.
		/// </summary>
		/// <param name="animationlayer">The animation layer being checked.</param>
		/// <returns>Float time of the currently played animation on animationlayer.</returns>
		private float CurrentAnimationLength(int animationlayer)
		{ return animator.GetCurrentAnimatorClipInfo(animationlayer).Length; }

        /// <summary>
        /// Lock character movement and/or action, on a delay for a set time.
        /// </summary>
        /// <param name="lockMovement">If set to <c>true</c> lock movement.</param>
        /// <param name="lockAction">If set to <c>true</c> lock action.</param>
        /// <param name="timed">If set to <c>true</c> timed.</param>
        /// <param name="delayTime">Delay time.</param>
        /// <param name="lockTime">Lock time.</param>
        public void Lock(bool lockMovement, bool lockAction, bool timed, float delayTime, float lockTime)
        {
            StopCoroutine("_Lock");
            StartCoroutine(_Lock(lockMovement, lockAction, timed, delayTime, lockTime));
        }

        private IEnumerator _Lock(bool lockMovement, bool lockAction, bool timed, float delayTime, float lockTime)
        {
            if (delayTime > 0) { yield return new WaitForSeconds(delayTime); }

            if (lockMovement) {
                _canMove = false;
                OnLockMovement();
            }
            if (lockAction) {
                _canAction = false;
                OnLockActions();
            }
            if (timed) {
                if (lockTime > 0) { yield return new WaitForSeconds(lockTime); }
                Unlock(lockMovement, lockAction);
            }
        }

        /// <summary>
        /// Let character move and act again.
        /// </summary>
        /// <param name="movement">Unlock movement if true.</param>
        /// <param name="actions">Unlock actions if true.</param>
        public void Unlock(bool movement, bool actions)
        {
            if (movement) {
                _canMove = true;
                OnUnlockMovement();
            }

			if (!actions) { return; }

            _canAction = true;
			if (_isAttacking) { _isAttacking = false; }
            OnUnlockActions();
        }

		/// <summary>
		/// Turns IK to 0 instantly.
		/// </summary>
		public void SetIKOff()
		{
			if (ikHands == null) return;
			ikHands.leftHandPositionWeight = 0;
			ikHands.leftHandRotationWeight = 0;
		}

		/// <summary>
		/// Turns IK to 1 instantly.
		/// </summary>
		public void SetIKOn(Weapon weapon)
		{
			if (ikHands != null) { ikHands.BlendIK(true, 0, 0, weapon); }
		}

		/// <summary>
		/// Pauses IK while character uses Left Hand during an animation.
		/// </summary>
		public void SetIKPause(float pauseTime)
		{
			if (ikHands != null && ikHands.isUsed) { ikHands.SetIKPause(pauseTime); }
		}

		#endregion
	}
}