using UnityEngine;

namespace RPGCharacterAnims.Actions
{
    public class Navigation : BaseActionHandler<Vector3>
    {
        RPGCharacterNavigationController navigation;

        public Navigation(RPGCharacterNavigationController navigation)
        { this.navigation = navigation; }

        public override bool CanStartAction(RPGCharacterController controller)
        { return navigation != null && controller.canMove; }

        public override bool CanEndAction(RPGCharacterController controller)
        { return navigation != null && navigation.isNavigating; }

        protected override void _StartAction(RPGCharacterController controller, Vector3 context)
        { navigation.MeshNavToPoint(context); }

        public override bool IsActive()
        { return navigation.isNavigating; }

        protected override void _EndAction(RPGCharacterController controller)
        { navigation.StopNavigating(); }
    }
}