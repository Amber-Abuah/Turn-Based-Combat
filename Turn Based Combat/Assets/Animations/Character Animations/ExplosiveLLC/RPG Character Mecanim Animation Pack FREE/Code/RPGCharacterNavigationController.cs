using RPGCharacterAnims.Lookups;
using UnityEngine;

namespace RPGCharacterAnims
{
    [RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
    [RequireComponent(typeof(RPGCharacterController))]
    public class RPGCharacterNavigationController : MonoBehaviour
    {
        // Components.
        [HideInInspector] public UnityEngine.AI.NavMeshAgent navMeshAgent;
        private RPGCharacterController rpgCharacterController;
        private RPGCharacterMovementController rpgCharacterMovementController;
        private Animator animator;

		// Variables.
        [HideInInspector] public bool isNavigating;
		public float moveSpeed = 7.0f;
		public float rotationSpeed = 1.0f;

        void Awake()
        {
            // In order for the navMeshAgent not to interfere with other movement, we want it to be
            // enabled ONLY when we are actually using it.
            navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            navMeshAgent.enabled = false;

            rpgCharacterController = GetComponent<RPGCharacterController>();
            rpgCharacterMovementController = GetComponent<RPGCharacterMovementController>();
            rpgCharacterController.SetHandler(HandlerTypes.Navigation, new Actions.Navigation(this));
		}

        void Start()
        {
            // Check if Animator exists, otherwise pause script.
            animator = rpgCharacterController.animator;
            if (animator != null) { return; }
            Debug.LogError("No Animator component found!");
            Debug.Break();
        }

		void Update()
		{
			if (isNavigating) {
				RotateTowardsMovementDir();

				// Nav mesh speed compared to RPGCharacterMovementController speed is 7-1.
				navMeshAgent.speed = moveSpeed * 7;
				if (navMeshAgent.velocity.sqrMagnitude > 0) {
					animator.SetBool(AnimationParameters.Moving, true);

					// Default run speed is 7 for navigation, so we divide by that.
					animator.SetFloat(AnimationParameters.VelocityZ, moveSpeed * (1 / (7 / navMeshAgent.speed)));
				}
				else { animator.SetFloat(AnimationParameters.VelocityZ, 0); }
			}

			// Disable the navMeshAgent once the character has reached its destination and set the animation speed to 0.
			if (isNavigating && !navMeshAgent.hasPath) {
				StopNavigating();
				animator.SetFloat(AnimationParameters.VelocityZ, 0f);
			}
		}

		/// <summary>
		/// Navigate to the destination using Unity's NavMeshAgent.
		/// </summary>
		/// <param name="destination">Point in world space to navigate to.</param>
		public void MeshNavToPoint(Vector3 destination)
        {
			navMeshAgent.enabled = true;
			navMeshAgent.SetDestination(destination);
			isNavigating = true;
			if (rpgCharacterMovementController != null) { rpgCharacterMovementController.enabled = false; }
		}

        /// <summary>
        /// Stop navigating to the current destination.
        /// </summary>
        public void StopNavigating()
        {
			isNavigating = false;
			navMeshAgent.enabled = false;
			if (rpgCharacterMovementController != null) { rpgCharacterMovementController.enabled = true; }
		}

        private void RotateTowardsMovementDir()
        {
			if (navMeshAgent.velocity.sqrMagnitude > 0.01f) {
				transform.rotation = Quaternion.Slerp(transform.rotation,
					Quaternion.LookRotation(navMeshAgent.velocity),
					Time.deltaTime * navMeshAgent.angularSpeed * rotationSpeed);

				// Keep X and Z rotation at 0.
				Quaternion q = transform.rotation;
				q.eulerAngles = new Vector3(0, q.eulerAngles.y, 0);
				transform.rotation = q;
			}
		}
    }
}