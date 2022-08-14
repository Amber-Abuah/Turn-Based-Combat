using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RPGCharacterAnims
{
	/// <summary>
	/// Basic Camera Controller with Follow, Rotate, and Zoom functionality.
	/// Can be used with either Legacy Input or Input System inputs.
	/// </summary>
	public class CameraController:MonoBehaviour
	{
		public GameObject cameraTarget;
		public float cameraTargetOffsetY;
		private Vector3 cameraTargetOffset;
		public float rotateSpeed = 2.0f;
		private float rotate;
		public float height = 6.0f;
		public float distance = 5.0f;
		public float zoomAmount = 0.1f;
		public float smoothing = 2.0f;
		private Vector3 offset;
		private bool following = true;
		private Vector3 lastPosition;

		// Inputs.
		private bool inputFollow;
		private bool inputRotateR;
		private bool inputRotateL;
		private bool inputMouseScrollUp;
		private bool inputMouseScrollDown;

		private void Start()
		{
			// Try to find Player if not set in Inspector.
			if (cameraTarget == null) { cameraTarget = GameObject.FindWithTag("Player"); }

			if (!cameraTarget) { Debug.LogError("No target selected for Camera."); }
			else { SetStartPosition(); }
		}

		/// <summary>
		/// Sets the initial starting position for the camera.
		/// </summary>
		private void SetStartPosition()
		{
			offset = new Vector3(cameraTarget.transform.position.x,
				cameraTarget.transform.position.y + height,
				cameraTarget.transform.position.z - distance);

			lastPosition = new Vector3(cameraTarget.transform.position.x,
				cameraTarget.transform.position.y + height,
				cameraTarget.transform.position.z - distance);

			distance = 1;
			height = 1;
		}

		/// <summary>
		/// Sets the inputs depending on whether the Input System is used or the Legacy Inputs.
		/// </summary>
		private void Inputs()
		{
			#if ENABLE_INPUT_SYSTEM
			inputFollow = Keyboard.current.fKey.isPressed;
			inputRotateL = Keyboard.current.qKey.isPressed;
			inputRotateR = Keyboard.current.eKey.isPressed;
			inputMouseScrollUp = Mouse.current.scroll.ReadValue().y > 0f;
			inputMouseScrollDown = Mouse.current.scroll.ReadValue().y < 0f;
			#else
			inputFollow = Input.GetKeyDown(KeyCode.F);
			inputRotateL = Input.GetKey(KeyCode.Q);
			inputRotateR = Input.GetKey(KeyCode.E);
			inputMouseScrollUp = Input.mouseScrollDelta.y == 1;
			inputMouseScrollDown = Input.mouseScrollDelta.y == -1;
			#endif
		}

		private void Update()
		{
			if (!cameraTarget) { return; }

			Inputs();

			// Follow cam.
			if (inputFollow) {
				if (following) { following = false; }
				else { following = true; }
			}
			if (following) { CameraFollow(); }
			else { transform.position = lastPosition; }

			// Rotate cam.
			if (inputRotateL) { rotate = -1; }
			else if (inputRotateR) { rotate = 1; }
			else { rotate = 0; }

			// Mouse zoom.
			if (inputMouseScrollUp) { distance += zoomAmount; height += zoomAmount; }
			else if (inputMouseScrollDown) { distance -= zoomAmount; height -= zoomAmount; }

			// Set cameraTargetOffset as cameraTarget + cameraTargetOffsetY.
			cameraTargetOffset = cameraTarget.transform.position + new Vector3(0, cameraTargetOffsetY, 0);

			// Smoothly look at cameraTargetOffset.
			transform.rotation = Quaternion.Slerp(transform.rotation,
				Quaternion.LookRotation(cameraTargetOffset - transform.position),
				Time.deltaTime * smoothing);
		}

		private void CameraFollow()
		{
			offset = Quaternion.AngleAxis(rotate * rotateSpeed, Vector3.up) * offset;

			transform.position = new Vector3(Mathf.Lerp(lastPosition.x, cameraTarget.transform.position.x + offset.x, smoothing * Time.deltaTime),
				Mathf.Lerp(lastPosition.y, cameraTarget.transform.position.y + offset.y * height, smoothing * Time.deltaTime),
				Mathf.Lerp(lastPosition.z, cameraTarget.transform.position.z + offset.z * distance, smoothing * Time.deltaTime));
		}

		private void LateUpdate()
		{ lastPosition = transform.position; }
	}
}