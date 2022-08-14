using System;

namespace RPGCharacterAnims.Actions
{
    /// <summary>
    /// This is the core implementation of IActionHandler, and all action handlers inherit it some
    /// way or another. This abstract class takes care of starting and stopping, and handles the
    /// event listeners. It's generic: the type TContext is used so you can pass whatever type of
    /// contextual info (ints, strings, or a whole custom class) to your action handler.
    ///
    /// In order to implement an action handler with actual behavior you need to:
    ///
    /// 1. Pick a context type (or use EmptyContext) and inherit from this class e.g.:
    ///        public class MyActionHandler : BaseActionHandler<float>
    ///
    /// 2. Implement CanStartAction and CanEndAction, required by IActionHandler.
    ///
    /// 3. Implement _StartAction and _EndAction, required by BaseActionHandler, which wraps them.
    /// </summary>
    /// <typeparam name="TContext">Context type.</typeparam>
    public abstract class BaseActionHandler<TContext> : IActionHandler
    {
        /// <summary>
        /// Actual storage for if this action handler is active.
        /// </summary>
        public bool active;

        /// <summary>
        /// Event (with an empty delegate) to hold the event listeners to be called after this
        /// action handler starts.
        /// </summary>
        public event Action OnStart = delegate { };

        /// <summary>
        /// Event (with an empty delegate) to hold the event listeners to be called after this
        /// action handler ends.
        /// </summary>
        public event Action OnEnd = delegate { };

        /// <summary>
        /// Placeholder method, see IActionHandler#CanStartAction.
        /// </summary>
        public abstract bool CanStartAction(RPGCharacterController controller);

        /// <summary>
        /// Actually start the action handler. This method sets the active flag, calls the
        /// _StartAction method with object context cast into the type you want, and notifies event
        /// listeners. It won't start the action unless CanStartAction returns true.
        /// </summary>
        /// <param name="controller">RPGCharacterController instance.</param>
        /// <param name="context">Plain old context object.</param>
        public virtual void StartAction(RPGCharacterController controller, object context)
        {
            if (!CanStartAction(controller)) { return; }
            active = true;
            _StartAction(controller, (TContext)context);
            OnStart();
        }

        /// <summary>
        /// Add an event listener to OnStart. See IActionHandler#AddStartListener.
        /// </summary>
        public virtual void AddStartListener(Action callback)
        { OnStart += callback; }

        /// <summary>
        /// Remove an event listener from OnStart. See IActionHandler#RemoveStartListener.
        /// </summary>
        public virtual void RemoveStartListener(Action callback)
        { OnStart -= callback; }

        /// <summary>
        /// Returns true if the action handler is active.
        /// </summary>
        public virtual bool IsActive()
        { return active; }

        /// <summary>
        /// Placeholder for a method which actually modifies controller and starts animations.
        /// It is wrapped by StartAction.
        /// </summary>
        /// <param name="controller">RPGCharacterController instance.</param>
        /// <param name="context">TContext context object.</param>
        protected abstract void _StartAction(RPGCharacterController controller, TContext context);

        /// <summary>
        /// Placeholder method, see IActionHandler#CanEndAction.
        /// </summary>
        /// <returns></returns>
        public abstract bool CanEndAction(RPGCharacterController controller);

        /// <summary>
        /// Actually end the action handler. This method unsets the active flag, calls the
        /// _EndAction method, and notifies event listeners. It won't end the action unless
        /// CanEndAction returns true.
        /// </summary>
        /// <param name="controller">RPGCharacterController instance.</param>
        public virtual void EndAction(RPGCharacterController controller)
        {
            if (!CanEndAction(controller)) { return; }
            active = false;
            _EndAction(controller);
            OnEnd();
        }

        /// <summary>
        /// Add an event listener to OnEnd. See IActionHandler#AddEndListener.
        /// </summary>
        public virtual void AddEndListener(Action callback)
        { OnEnd += callback; }

        /// <summary>
        /// Remove an event listener from OnEnd. See IActionHandler#RemoveEndListener.
        /// </summary>
        public virtual void RemoveEndListener(Action callback)
        { OnEnd -= callback; }

        /// <summary>
        /// Placeholder for a method which actually modifies the controller and ends animations.
        /// It is wrapped by EndAction.
        /// </summary>
        /// <param name="controller">RPGCharacterController instance.</param>
        protected abstract void _EndAction(RPGCharacterController controller);
    }
}