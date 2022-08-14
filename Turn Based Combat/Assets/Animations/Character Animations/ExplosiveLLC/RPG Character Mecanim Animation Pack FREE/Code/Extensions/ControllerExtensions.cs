using UnityEngine;

namespace RPGCharacterAnims.Extensions
{
    public static class ControllerExtensions
    {
        public static void DebugController(this RPGCharacterController controller)
        {
            Debug.Log("CONTROLLER SETTINGS---------------------------");
            Debug.Log("AnimationSpeed: " + controller.animationSpeed);
            Debug.Log("canAction: " + controller.canAction);
            Debug.Log("canFace: " + controller.canFace);
            Debug.Log("canMove: " + controller.canMove);
            Debug.Log("canStrafe: " + controller.canStrafe);
            Debug.Log("acquiringGround: " + controller.acquiringGround);
            Debug.Log("maintainingGround: " + controller.maintainingGround);
			Debug.Log("isAttacking: " + controller.isAttacking);
            Debug.Log("isFacing: " + controller.isFacing);
            Debug.Log("isFalling: " + controller.isFalling);
            Debug.Log("isIdle: " + controller.isIdle);
            Debug.Log("isMoving: " + controller.isMoving);
            Debug.Log("isNavigating: " + controller.isNavigating);
            Debug.Log("isRolling: " + controller.isRolling);
            Debug.Log("isKnockback: " + controller.isKnockback);
            Debug.Log("isKnockdown: " + controller.isKnockdown);
            Debug.Log("isStrafing: " + controller.isStrafing);
            Debug.Log("moveInput: " + controller.moveInput);
            Debug.Log("aimInput: " + controller.aimInput);
            Debug.Log("jumpInput: " + controller.jumpInput);
            Debug.Log("cameraRelativeInput: " + controller.cameraRelativeInput);
            Debug.Log("rightWeapon: " + controller.rightWeapon);
            Debug.Log("hasTwoHandedWeapon: " + controller.hasTwoHandedWeapon);
        }
    }
}