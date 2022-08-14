using System;
using UnityEngine;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims
{
	[HelpURL("https://docs.unity3d.com/Manual/class-InputManager.html")]

	public class RPGCharacterInputController : MonoBehaviour
    {
        RPGCharacterController rpgCharacterController;

        // Inputs.
        private float inputHorizontal = 0;
        private float inputVertical = 0;
        private bool inputJump;
        private bool inputLightHit;
        private bool inputKnockdown;
        private bool inputAttackL;
        private bool inputAttackR;
        private float inputSwitchUpDown;
        private float inputAimBlock;
        private bool inputAiming;
        private bool inputFace;
        private float inputFacingHorizontal;
        private float inputFacingVertical;
        private bool inputRoll;

        // Variables.
        private Vector3 moveInput;
        private bool isJumpHeld;
        private float inputPauseTimeout = 0;
        private bool inputPaused = false;

        private void Awake()
        {
            rpgCharacterController = GetComponent<RPGCharacterController>();
        }

        private void Update()
        {
			// Pause input for other external input.
			if (inputPaused) {
				if (Time.time > inputPauseTimeout) { inputPaused = false; }
				else { return; }
			}

			if (!inputPaused) { Inputs(); }

            Moving();
			Jumping();
			Damage();
            SwitchWeapons();
            Strafing();
            Facing();
            Aiming();
            Rolling();
            Attacking();
        }

        /// <summary>
        /// Pause input for a number of seconds.
        /// </summary>
        /// <param name="timeout">The amount of time in seconds to ignore input.</param>
        public void PauseInput(float timeout)
        {
            inputPaused = true;
            inputPauseTimeout = Time.time + timeout;
        }

        /// <summary>
        /// Input abstraction for easier asset updates using outside control schemes.
        /// </summary>
        private void Inputs()
        {
	        try {
		        inputJump = Input.GetButtonDown("Jump");
		        isJumpHeld = Input.GetButton("Jump");
		        inputLightHit = Input.GetButtonDown("LightHit");
		        inputKnockdown = Input.GetButtonDown("Knockdown");
		        inputAttackL = Input.GetButtonDown("AttackL");
		        inputAttackR = Input.GetButtonDown("AttackR");
		        inputSwitchUpDown = Input.GetAxisRaw("SwitchUpDown");
		        inputAimBlock = Input.GetAxisRaw("Aim");
		        inputAiming = Input.GetButton("Aiming");
		        inputHorizontal = Input.GetAxisRaw("Horizontal");
		        inputVertical = Input.GetAxisRaw("Vertical");
		        inputFace = Input.GetMouseButton(1);
		        inputFacingHorizontal = Input.GetAxisRaw("FacingHorizontal");
		        inputFacingVertical = Input.GetAxisRaw("FacingVertical");
		        inputRoll = Input.GetButtonDown("L3");

		        // Slow time toggle.
		        if (rpgCharacterController.HandlerExists(HandlerTypes.SlowTime)) {
			        if (Input.GetKeyDown(KeyCode.T)) {
				        if (!rpgCharacterController.TryStartAction(HandlerTypes.SlowTime, 0.0125f))
						{ rpgCharacterController.TryEndAction(HandlerTypes.SlowTime); }
			        }
			        // Pause toggle.
			        if (Input.GetKeyDown(KeyCode.P)) {
				        if (!rpgCharacterController.TryStartAction(HandlerTypes.SlowTime, 0f))
						{ rpgCharacterController.TryEndAction(HandlerTypes.SlowTime); }
			        }
		        }
	        }
	        catch (Exception)
			{ Debug.LogError("Inputs not found! Please read Readme, or watch https://www.youtube.com/watch?v=ruufqlXrCzU"); }
        }

		public bool HasMoveInput() => moveInput.magnitude > 0.1f;

		public bool HasAimInput() => inputAiming || inputAimBlock < -0.1f;

		public bool HasFacingInput() => (inputFacingHorizontal < -0.05 || inputFacingHorizontal > 0.05) ||
				   (inputFacingVertical < -0.05 || inputFacingVertical > 0.05) ||
				   inputFace;

        public void Moving()
		{
			moveInput = new Vector3(inputHorizontal, inputVertical, 0f);

			// Filter the 0.1 threshold of HasMoveInput.
			if (HasMoveInput()) { rpgCharacterController.SetMoveInput(moveInput); }
			else { rpgCharacterController.SetMoveInput(Vector3.zero); }
		}

		private void Jumping()
		{
			if (!rpgCharacterController.HandlerExists(HandlerTypes.Jump)) { return; }

			// Set the input on the jump axis every frame.
			var jumpInput = isJumpHeld ? Vector3.up : Vector3.zero;
			rpgCharacterController.SetJumpInput(jumpInput);

			// If we pressed jump button this frame, jump.
			if (inputJump && rpgCharacterController.CanStartAction(HandlerTypes.Jump))
			{ rpgCharacterController.StartAction(HandlerTypes.Jump); }

			// Or doublejump if already jumped.
			else if (inputJump && rpgCharacterController.CanStartAction(HandlerTypes.DoubleJump))
			{ rpgCharacterController.StartAction(HandlerTypes.DoubleJump); }
		}

		public void Rolling()
        {
            if (!inputRoll ||
                !rpgCharacterController.HandlerExists(HandlerTypes.DiveRoll) ||
                !rpgCharacterController.CanStartAction(HandlerTypes.DiveRoll)) { return; }

			rpgCharacterController.StartAction(HandlerTypes.DiveRoll, 1);
        }

        private void Aiming()
        { Strafing(); }

        private void Strafing()
        {
			// Check to make sure Strafe Action exists.
			if (!rpgCharacterController.HandlerExists(HandlerTypes.Strafe) || !rpgCharacterController.canStrafe) { return; }

			if (inputAimBlock < -0.1f || inputAiming) { rpgCharacterController.TryStartAction(HandlerTypes.Strafe); }
			else { rpgCharacterController.TryEndAction(HandlerTypes.Strafe); }
        }

        private void Facing()
        {
			// Check to make sure Face Action exists.
			if (!rpgCharacterController.HandlerExists(HandlerTypes.Face)) { return; }
			if (!rpgCharacterController.canFace) { return; }

			if (HasFacingInput()) {
				if (inputFace) {

					// Get world position from mouse position on screen and convert to direction from character.
					var playerPlane = new Plane(Vector3.up, transform.position);
					var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					var hitdist = 0.0f;
					if (playerPlane.Raycast(ray, out hitdist)) {
						var targetPoint = ray.GetPoint(hitdist);
						var lookTarget = new Vector3(targetPoint.x - transform.position.x, transform.position.z - targetPoint.z, 0);
						rpgCharacterController.SetFaceInput(lookTarget);
					}
				}
				else { rpgCharacterController.SetFaceInput(new Vector3(inputFacingHorizontal, inputFacingVertical, 0)); }

				rpgCharacterController.TryStartAction(HandlerTypes.Face);
			}
			else { rpgCharacterController.TryEndAction(HandlerTypes.Face); }
        }

        private void Attacking()
        {
			// Check to make sure Attack Action exists.
			if (!rpgCharacterController.HandlerExists(HandlerTypes.Attack)) { return; }

			// Check to make character can Attack.
			if (!rpgCharacterController.CanStartAction(HandlerTypes.Attack)) { return; }

            if (inputAttackL)
			{ rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext(HandlerTypes.Attack, Side.Left)); }
			else if (inputAttackR)
			{ rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext(HandlerTypes.Attack, Side.Right)); }
        }

        private void Damage()
        {
			// Hit.
			if (rpgCharacterController.HandlerExists(HandlerTypes.GetHit)) {
				if (inputLightHit) { rpgCharacterController.StartAction(HandlerTypes.GetHit, new HitContext()); }
			}

			// Knockdown.
			if (rpgCharacterController.HandlerExists(HandlerTypes.Knockdown)) {
				if (inputKnockdown && rpgCharacterController.CanStartAction(HandlerTypes.Knockdown))
				{ rpgCharacterController.StartAction(HandlerTypes.Knockdown, new HitContext(( int )KnockdownType.Knockdown1, Vector3.back)); }
			}
        }

		/// <summary>
		/// Cycle weapons using directional pad input. Up and Down cycle forward and backward through
		/// the list of two handed weapons. Left cycles through the left hand weapons. Right cycles through
		/// the right hand weapons.
		/// </summary>
		private void SwitchWeapons()
		{
			// Check to make sure SwitchWeapon Action exists.
			if (!rpgCharacterController.HandlerExists(HandlerTypes.SwitchWeapon)) { return; }

			// Bail out if we can't switch weapons.
			if (!rpgCharacterController.CanStartAction(HandlerTypes.SwitchWeapon)) { return; }

			var doSwitch = false;
			var context = new SwitchWeaponContext();
			var weaponNumber = Weapon.Unarmed;

			// Cycle through 2Handed weapons if any input happens on the up-down axis.
			if (Mathf.Abs(inputSwitchUpDown) > 0.1f) {
				var twoHandedWeapons = new Weapon[] {
					Weapon.TwoHandSword
				};
				// If we're not wielding 2Handed weapon already, just switch to the first one in the list.
				if (System.Array.IndexOf(twoHandedWeapons, rpgCharacterController.rightWeapon) == -1)
				{ weaponNumber = twoHandedWeapons[0]; }

				// Otherwise, loop through them.
				else {
					var index = System.Array.IndexOf(twoHandedWeapons, rpgCharacterController.rightWeapon);
					if (inputSwitchUpDown < -0.1f) { index = (index - 1 + twoHandedWeapons.Length) % twoHandedWeapons.Length; }
					else if (inputSwitchUpDown > 0.1f) { index = (index + 1) % twoHandedWeapons.Length; }
					weaponNumber = twoHandedWeapons[index];
				}
				// Set up the context and flag that we actually want to perform the switch.
				doSwitch = true;
				context.type = HandlerTypes.Switch;
				context.side = "None";
				context.leftWeapon = Weapon.Unarmed;
				context.rightWeapon = weaponNumber;
			}

			// If we've received input, then "doSwitch" is true, and the context is filled out,
			// so start the SwitchWeapon action.
			if (doSwitch) { rpgCharacterController.StartAction(HandlerTypes.SwitchWeapon, context); }
		}
	}
}