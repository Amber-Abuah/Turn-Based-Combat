using UnityEngine;

namespace RPGCharacterAnims.Actions
{
    public class SlowTime : BaseActionHandler<float>
    {
        public override bool CanStartAction(RPGCharacterController controller)
        { return !active; }

        public override bool CanEndAction(RPGCharacterController controller)
        { return active; }

        protected override void _StartAction(RPGCharacterController controller, float context)
        { Time.timeScale = context; }

        protected override void _EndAction(RPGCharacterController controller)
        { Time.timeScale = 1f; }
    }
}