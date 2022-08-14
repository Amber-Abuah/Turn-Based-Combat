using System;

namespace RPGCharacterAnims.Actions
{
    /// <summary>
    /// SimpleActionHandler is used when you need a handler with an off and on state. Its only
    /// start condition is that it's not "on", and its only end condition is that its not "off",
    /// like a light switch. It has no context.
    /// </summary>
    public class SimpleActionHandler : BaseActionHandler<EmptyContext>
    {
        public SimpleActionHandler(Action onStart, Action onEnd)
        {
            this.AddStartListener(onStart);
            this.AddEndListener(onEnd);
        }

        public override bool CanStartAction(RPGCharacterController controller)
        { return !active; }

        public override bool CanEndAction(RPGCharacterController controller)
        { return active; }

        protected override void _StartAction(RPGCharacterController controller, EmptyContext context) { }

        protected override void _EndAction(RPGCharacterController controller) { }
    }
}