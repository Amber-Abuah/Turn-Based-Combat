using System.Collections;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Extensions;
using RPGCharacterAnims.Lookups;
using UnityEngine;

namespace RPGCharacterAnims
{
	public class RPGCharacterMovementController : SuperStateMachine
    {
        // Components.
        private SuperCharacterController superCharacterController;
        private RPGCharacterController rpgCharacterController;
        private Rigidbody rb;
        private Animator animator;
        private CapsuleCollider capCollider;

		/// <summary>
		/// Returns whether the character can face.
		/// </summary>
		public bool acquiringGround => superCharacterController.currentGround.IsGrounded(false, 0.01f);

		/// <summary>
		/// Returns whether the character can face.
		/// </summary>
		public bool maintainingGround => superCharacterController.currentGround.IsGrounded(true, 0.5f);

		[HideInInspector] public Vector3 lookDirection { get; private set; }

		[Header("Knockback")]
        /// <summary>
        /// Multiplies the amount of knockback force a character recieves when they get hit.
        /// </summary>
        public float knockbackMultiplier = 1f;

		[Header("Movement Multiplier")]
        /// <summary>
        /// Multiplies the speed of animation velocity.
        /// </summary>
        public float movementAnimationMultiplier = 1f;

        /// <summary>
        /// Vector3 movement velocity.
        /// </summary>
        [HideInInspector] public Vector3 currentVelocity;

        [Header("Movement")]
        /// <summary>
        /// Movement speed while walking and strafing.
        /// </summary>
        public float walkSpeed = .5f;

        /// <summary>
        /// Walking acceleration.
        /// </summary>
        public float walkAccel = 15f;

        /// <summary>
        /// Movement speed while running. (the default movement)
        /// </summary>
        public float runSpeed = 1f;

        /// <summary>
        /// Running acceleration.
        /// </summary>
        public float runAccel = 30f;

        /// <summary>
        /// Ground friction, slows the character to a stop.
        /// </summary>
        public float groundFriction = 120f;

        /// <summary>
        /// Speed of rotation when turning the character to face movement direction or target.
        /// </summary>
        public float rotationSpeed = 100f;

        /// <summary>
        /// Internal flag for when the character can jump.
        /// </summary>
        [HideInInspector] public bool canJump;

        /// <summary>
        /// Internal flag for if the player is holding the jump input. If this is released while
        /// the character is still ascending, the vertical speed is damped.
        /// </summary>
        [HideInInspector] public bool holdingJump;

        /// <summary>
        /// Internal flag for if the character can perform a double jump.
        /// </summary>
        [HideInInspector] public bool canDoubleJump = false;
        private bool doublejumped = false;

        [Header("Jumping")]
        /// <summary>
        /// Jumping speed while ascending.
        /// </summary>
        public float jumpSpeed = 12f;

        /// <summary>
        /// Gravity while ascending.
        /// </summary>
        public float jumpGravity = 24f;

        /// <summary>
        /// Double jump speed.
        /// </summary>
        public float doubleJumpSpeed = 8f;

        /// <summary>
        /// Horizontal speed while in the air.
        /// </summary>
        public float inAirSpeed = 8f;

        /// <summary>
        /// Horizontal acceleration while in the air.
        /// </summary>
        public float inAirAccel = 16f;

		/// <summary>
		/// Gravity while descending. Default is higher than ascending gravity (like a Mario jump).
		/// </summary>
		public float fallGravity = 32f;

		/// <summary>
		/// Allows control while character is falling.
		/// </summary>
		public bool fallingControl = false;

		[Header("Debug Options")]
		public bool debugMessages;

		private void Awake()
        {
            rpgCharacterController = GetComponent<RPGCharacterController>();
            rpgCharacterController.SetHandler(HandlerTypes.AcquiringGround, new SimpleActionHandler(() => { }, () => { }));
            rpgCharacterController.SetHandler(HandlerTypes.MaintainingGround, new SimpleActionHandler(() => { }, () => { }));
            rpgCharacterController.SetHandler(HandlerTypes.DiveRoll, new DiveRoll(this));
            rpgCharacterController.SetHandler(HandlerTypes.DoubleJump, new DoubleJump(this));
            rpgCharacterController.SetHandler(HandlerTypes.Fall, new Fall(this));
            rpgCharacterController.SetHandler(HandlerTypes.GetHit, new GetHit(this));
            rpgCharacterController.SetHandler(HandlerTypes.Idle, new Idle(this));
            rpgCharacterController.SetHandler(HandlerTypes.Jump, new Jump(this));
            rpgCharacterController.SetHandler(HandlerTypes.Knockback, new Knockback(this));
            rpgCharacterController.SetHandler(HandlerTypes.Knockdown, new Knockdown(this));
            rpgCharacterController.SetHandler(HandlerTypes.Move, new Move(this));
		}

        private void Start()
        {
            // Get other RPG Character components.
            superCharacterController = GetComponent<SuperCharacterController>();

            // Check if Animator exists, otherwise pause script.
            animator = GetComponentInChildren<Animator>();
			if (!animator) {
				Debug.LogError("ERROR: THERE IS NO ANIMATOR COMPONENT ON CHILD OF CHARACTER.");
				Debug.Break();
			}
			// Setup Collider and Rigidbody for collisions.
			capCollider = GetComponent<CapsuleCollider>();
            rb = GetComponent<Rigidbody>();

            // Set restraints on startup if using Rigidbody.
            if (rb != null) { rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; }

            rpgCharacterController.OnLockMovement += LockMovement;
            rpgCharacterController.OnUnlockMovement += UnlockMovement;
            var animatorEvents = rpgCharacterController.GetAnimatorTarget().GetComponent<RPGCharacterAnimatorEvents>();
            animatorEvents.OnMove.AddListener(AnimatorMove);
        }

        #region Updates

        /*
		Update is normally run once on every frame update. We won't be using it in this case, since the SuperCharacterController
        component sends a callback Update called SuperUpdate. SuperUpdate is recieved by the SuperStateMachine, and then fires
        further callbacks depending on the state.

        If SuperCharacterController is disabled then we still want the SuperStateMachine to run, so we call SuperUpdate manually.
        */

        void Update()
        {
            if (!superCharacterController.enabled)
			{ gameObject.SendMessage("SuperUpdate", SendMessageOptions.DontRequireReceiver); }
        }

        protected override void EarlyGlobalSuperUpdate()
        {
	        if (acquiringGround) { rpgCharacterController.StartAction(HandlerTypes.AcquiringGround); }
			else { rpgCharacterController.EndAction(HandlerTypes.AcquiringGround); }

            if (maintainingGround) { rpgCharacterController.StartAction(HandlerTypes.MaintainingGround); }
			else { rpgCharacterController.EndAction(HandlerTypes.MaintainingGround); }
        }

		// Put any code in here you want to run AFTER the state's update function.
		// This is run regardless of what state you're in.
        protected override void LateGlobalSuperUpdate()
        {
            // If the movement controller itself is disabled, this shouldn't run.
            if (!enabled) { return; }

            // Move the player by our velocity every frame.
            transform.position += currentVelocity * superCharacterController.deltaTime;

            // If alive and is moving, set animator.
            if (rpgCharacterController.canMove) {
                if (currentVelocity.magnitude > 0f) {
                    animator.SetFloat(AnimationParameters.VelocityX, 0);
                    animator.SetFloat(AnimationParameters.VelocityZ, transform.InverseTransformDirection(currentVelocity).z * movementAnimationMultiplier);
                    animator.SetBool(AnimationParameters.Moving, true);
                }
				else {
                    animator.SetFloat(AnimationParameters.VelocityX, 0f);
                    animator.SetFloat(AnimationParameters.VelocityZ, 0f);
                    animator.SetBool(AnimationParameters.Moving, false);
                }
            }
			// Aiming.
			if (rpgCharacterController.isAiming || rpgCharacterController.isStrafing)
			{ RotateTowardsTarget(rpgCharacterController.aimInput); }

			// Facing.
			else if (rpgCharacterController.isFacing) { RotateTowardsDirection(rpgCharacterController.faceInput); }
			else if (rpgCharacterController.canMove) { RotateTowardsMovementDir(); }

            if (currentState == null && rpgCharacterController.CanStartAction(HandlerTypes.Idle))
			{ rpgCharacterController.StartAction(HandlerTypes.Idle); }

			// Update animator with local movement values.
			animator.SetFloat(AnimationParameters.VelocityX, transform.InverseTransformDirection(currentVelocity).x * movementAnimationMultiplier);
			animator.SetFloat(AnimationParameters.VelocityZ, transform.InverseTransformDirection(currentVelocity).z * movementAnimationMultiplier);
		}

        #endregion

        #region States
        // Below are the state functions. Each one is called based on the name of the state, so when currentState = Idle,
        // we call Idle_EnterState. If currentState = Jump, we call Jump_SuperUpdate().

        private void Idle_EnterState()
        {
			if (debugMessages) { Debug.Log("Idle_EnterState"); }
			superCharacterController.EnableSlopeLimit();
            superCharacterController.EnableClamping();
            canJump = true;
            doublejumped = false;
            canDoubleJump = false;
        }

        // Run every frame character is in the idle state.
        private void Idle_SuperUpdate()
        {
			// Check if the character starts falling.
			if (rpgCharacterController.TryStartAction(HandlerTypes.Fall)) { return; }

			// Apply friction to slow to a halt.
			currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, groundFriction * superCharacterController.deltaTime);
			rpgCharacterController.TryStartAction(HandlerTypes.Move);
        }

        // Run every frame character is moving.
        private void Move_SuperUpdate()
        {
			// Check if the character starts falling.
			if (rpgCharacterController.TryStartAction(HandlerTypes.Fall)) { return; }

            // Set speed determined by movement type.
            if (rpgCharacterController.canMove) {
                var moveSpeed = runSpeed;
                var moveAccel = runAccel;

				if (rpgCharacterController.isStrafing) {
                    moveSpeed = walkSpeed;
                    moveAccel = walkAccel;
                }

                currentVelocity = Vector3.MoveTowards(currentVelocity,
					rpgCharacterController.cameraRelativeInput * moveSpeed,
					moveAccel * superCharacterController.deltaTime);
			}
			// If there is no movement detected, go into Idle.
            rpgCharacterController.TryStartAction(HandlerTypes.Idle);
        }

        private void Jump_EnterState()
        {
			if (debugMessages) { Debug.Log("Jump_EnterState"); }
			superCharacterController.DisableClamping();
            superCharacterController.DisableSlopeLimit();

            currentVelocity = new Vector3(currentVelocity.x, jumpSpeed, currentVelocity.z);
            animator.SetInteger(AnimationParameters.Jumping, 1);
            animator.SetAnimatorTrigger(AnimatorTrigger.JumpTrigger);
            canJump = false;
        }

        private void Jump_SuperUpdate()
        {
            holdingJump = rpgCharacterController.jumpInput.y != 0f;

            // Cap jump speed if we stop holding the jump button.
            if (!holdingJump && currentVelocity.y > (jumpSpeed / 4f)) {
	            var destination = new Vector3(currentVelocity.x, (jumpSpeed / 4f), currentVelocity.z);
                currentVelocity = Vector3.MoveTowards(currentVelocity, destination, fallGravity * superCharacterController.deltaTime);
            }

            var planarMoveDirection = Math3d.ProjectVectorOnPlane(superCharacterController.up, currentVelocity);
            var verticalMoveDirection = currentVelocity - planarMoveDirection;

            // Falling.
            if (currentVelocity.y < 0) {
                currentVelocity = planarMoveDirection;
                currentState = CharacterState.Fall;
                return;
            }

			planarMoveDirection = Vector3.MoveTowards(planarMoveDirection,
				rpgCharacterController.cameraRelativeInput * inAirSpeed,
				inAirAccel * superCharacterController.deltaTime);

            verticalMoveDirection -= superCharacterController.up * jumpGravity * superCharacterController.deltaTime;
            currentVelocity = planarMoveDirection + verticalMoveDirection;
        }

        private void DoubleJump_EnterState()
        {
			if (debugMessages) { Debug.Log("DoubleJump_EnterState"); }
			currentVelocity = new Vector3(currentVelocity.x, doubleJumpSpeed, currentVelocity.z);
            canDoubleJump = false;
            doublejumped = true;
            animator.SetInteger(AnimationParameters.Jumping, 3);
            animator.SetAnimatorTrigger(AnimatorTrigger.JumpTrigger);
        }

		/// <summary>
		/// DoubleJump uses the same SuperUpdate as Jump.
		/// </summary>
        private void DoubleJump_SuperUpdate()
        { Jump_SuperUpdate(); }

        private void Fall_EnterState()
        {
			if (debugMessages) { Debug.Log("Fall_EnterState"); }
			if (!doublejumped) { canDoubleJump = true; }
            superCharacterController.DisableClamping();
            superCharacterController.DisableSlopeLimit();
            canJump = false;
            animator.SetInteger(AnimationParameters.Jumping, 2);
            animator.SetAnimatorTrigger(AnimatorTrigger.JumpTrigger);
        }

        private void Fall_SuperUpdate()
        {
            if (rpgCharacterController.CanStartAction(HandlerTypes.Idle)) {
                currentVelocity = Math3d.ProjectVectorOnPlane(superCharacterController.up, currentVelocity);
                rpgCharacterController.StartAction(HandlerTypes.Idle);
                return;
            }

			// If FallingControl is enabled.
			if (fallingControl) {
				var planarMoveDirection = Math3d.ProjectVectorOnPlane(superCharacterController.up, currentVelocity);
				var verticalMoveDirection = currentVelocity - planarMoveDirection;

				planarMoveDirection = Vector3.MoveTowards(planarMoveDirection,
					rpgCharacterController.cameraRelativeInput * inAirSpeed,
					inAirAccel * superCharacterController.deltaTime);

				verticalMoveDirection -= superCharacterController.up * fallGravity * superCharacterController.deltaTime;
				currentVelocity = planarMoveDirection + verticalMoveDirection;
			}
			else { currentVelocity -= superCharacterController.up * fallGravity * superCharacterController.deltaTime; }
		}

		private void Fall_ExitState()
		{
			if (debugMessages) { Debug.Log("Fall_ExitState"); }
			animator.SetInteger(AnimationParameters.Jumping, 0);
			animator.SetAnimatorTrigger(AnimatorTrigger.JumpTrigger);
		}

        private void DiveRoll_EnterState()
        {
			if (debugMessages) { Debug.Log("DiveRoll_EnterState"); }
			rpgCharacterController.OnUnlockMovement += IdleOnceAfterMoveUnlock;
		}

		private void DiveRoll_SuperUpdate()
		{
			if (rpgCharacterController.CanStartAction(HandlerTypes.Idle)) {
				currentVelocity = Math3d.ProjectVectorOnPlane(superCharacterController.up, currentVelocity);
				rpgCharacterController.StartAction(HandlerTypes.Idle);
				return;
			}
			currentVelocity -= superCharacterController.up * (fallGravity / 2) * superCharacterController.deltaTime;
		}

        private void Knockback_EnterState()
        {
			if (debugMessages) { Debug.Log("Knockback_EnterState"); }
			rpgCharacterController.OnUnlockMovement += IdleOnceAfterMoveUnlock;
		}

        private void Knockdown_EnterState()
        {
			if (debugMessages) { Debug.Log("Knockdown_EnterState"); }
			rpgCharacterController.OnUnlockMovement += IdleOnceAfterMoveUnlock;
		}

		#endregion

        private void RotateTowardsMovementDir()
        {
            var movementVector = new Vector3(currentVelocity.x, 0, currentVelocity.z);
            if (movementVector.magnitude > 0.01f) {
                transform.rotation = Quaternion.Slerp(transform.rotation,
					Quaternion.LookRotation(movementVector),
					Time.deltaTime * rotationSpeed);
            }
        }

        private void RotateTowardsTarget(Vector3 targetPosition)
        {
			if (debugMessages) { Debug.Log($"RotateTowardsTarget: {targetPosition}"); }
			var lookTarget = new Vector3(targetPosition.x - transform.position.x, 0, targetPosition.z - transform.position.z);
			if (lookTarget != Vector3.zero) {
				var targetRotation = Quaternion.LookRotation(lookTarget);
				transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
			}
        }

		private void RotateTowardsDirection(Vector3 direction)
		{
			if (debugMessages) { Debug.Log($"RotateTowardsDirection: {direction}"); }
			var lookDirection = new Vector3(direction.x, 0, -direction.y);
			var lookRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
			transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
		}

		/// <summary>
		/// Exert a knockback force on the character. Used by the GetHit, Knockdown, and Knockback
		/// actions.
		/// </summary>
		/// <param name="knockDirection">Vector3 direction knock the character.</param>
		/// <param name="knockBackAmount">Amount to knock back.</param>
		/// <param name="variableAmount">Random variance in knockback.</param>
		public void KnockbackForce(Vector3 knockDirection, float knockBackAmount, float variableAmount)
        { StartCoroutine(_KnockbackForce(knockDirection, knockBackAmount, variableAmount)); }

        private IEnumerator _KnockbackForce(Vector3 knockDirection, float knockBackAmount, float variableAmount)
        {
            if (rb == null) { yield break; }

            var startTime = Time.time;
            var elapsed = 0f;

            rb.isKinematic = false;

            while (elapsed < .1f) {
                rb.AddForce(knockDirection
					* ((knockBackAmount + Random.Range(-variableAmount, variableAmount))
					* knockbackMultiplier * 10), ForceMode.Impulse);
                elapsed = Time.time - startTime;
                yield return null;
            }

            rb.isKinematic = true;
        }

        /// <summary>
        /// Event listener for when RPGCharacterController.OnLockMovement is called.
        /// </summary>
        public void LockMovement()
        {
            currentVelocity = new Vector3(0, 0, 0);
            animator.SetBool(AnimationParameters.Moving, false);
            animator.applyRootMotion = true;
        }

        /// <summary>
        /// Event listener for when RPGCharacterController.OnUnlockMovement is called.
        /// </summary>
        public void UnlockMovement()
        { animator.applyRootMotion = false; }

        /// <summary>
        /// Event listener for when RPGCharacterAnimatorEvents.OnMove is called.
        /// </summary>
        /// <param name="deltaPosition">Change in position.</param>
        /// <param name="rootRotation">Change in rotation.</param>
        public void AnimatorMove(Vector3 deltaPosition, Quaternion rootRotation)
        {
            transform.position += deltaPosition;
            transform.rotation = rootRotation;
        }

        /// <summary>
        /// Event listener to return to the Idle state once movement is unlocked, which executes
        /// once. Use with the RPGCharacterController.OnUnlockMovement event.
        ///
        /// e.g.: rpgCharacterController.OnUnlockMovement += IdleOnceAfterMoveUnlock;
        /// </summary>
        public void IdleOnceAfterMoveUnlock()
        {
            rpgCharacterController.StartAction(HandlerTypes.Idle);
            rpgCharacterController.OnUnlockMovement -= IdleOnceAfterMoveUnlock;
        }

        /// <summary>
        /// Event listener to instant switch once movement is unlocked, which executes only
        /// once. Use with the RPGCharacterController.OnUnlockMovement event. This is used by
        /// the Crawl->Crouch transition to get back into crouching.
        ///
        /// e.g.: rpgCharacterController.OnUnlockMovement += InstantSwitchOnceAfterMoveUnlock;
        /// </summary>
        public void InstantSwitchOnceAfterMoveUnlock()
        {
            animator.SetAnimatorTrigger(AnimatorTrigger.InstantSwitchTrigger);
            rpgCharacterController.OnUnlockMovement -= InstantSwitchOnceAfterMoveUnlock;
        }
    }
}