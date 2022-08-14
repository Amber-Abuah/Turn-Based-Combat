using UnityEngine;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims
{
    public class NoJumpSlime : MonoBehaviour
    {
        RPGCharacterController controller;
        IActionHandler oldJumpHandler;

        private void OnTriggerEnter(Collider collide)
        {
            controller = collide.gameObject.GetComponent<RPGCharacterController>();

            if (controller != null) {
                oldJumpHandler = controller.GetHandler(HandlerTypes.Jump);
                controller.SetHandler(HandlerTypes.Jump, new SimpleActionHandler(() => {
                    Debug.Log("Can't jump!");
                    controller.EndAction(HandlerTypes.Jump);
                }, () => { }));
            }
        }

        private void OnTriggerExit(Collider collide)
        {
            if (collide.gameObject == controller.gameObject) {
                controller.SetHandler(HandlerTypes.Jump, oldJumpHandler);
                controller = null;
                oldJumpHandler = null;
            }
        }
    }
}