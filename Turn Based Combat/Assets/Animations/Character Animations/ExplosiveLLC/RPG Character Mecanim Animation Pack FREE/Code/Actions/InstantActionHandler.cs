namespace RPGCharacterAnims.Actions
{
    /// <summary>
    /// InstantActionHandler is used when you need an action handler and it doesn't make sense that
    /// it would be in an "on" state. This is useful for animations which return the character to
    /// the same state after the animation plays.
    ///
    /// How this works under the hood: this class calls StartAction and then immediately calls
    /// EndAction. Some methods here are short-circuited: IsActive is always false, and you can
    /// always end the action, which does nothing.
    ///
    /// In order to implement an instant action handler you need to:
    ///
    /// 1. Pick a context type (or use EmptyContext) and inherit from this class e.g.:
    ///        public class MyInstantActionHandler : InstantActionHandler<float>
    ///
    /// 2. Implement CanStartAction, required by IActionHandler.
    ///
    /// 3. Implement _StartAction, required by BaseActionHandler.
    /// </summary>
    /// <typeparam name="TContext">Context type.</typeparam>
    public abstract class InstantActionHandler<TContext> : BaseActionHandler<TContext>
    {
        public override void StartAction(RPGCharacterController controller, object context)
        {
            base.StartAction(controller, context);
            base.EndAction(controller);
        }

        public override bool IsActive()
        { return false; }

        public override bool CanEndAction(RPGCharacterController controller)
        { return true; }

        protected override void _EndAction(RPGCharacterController controller) { }
    }
}