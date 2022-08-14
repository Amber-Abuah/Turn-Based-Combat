namespace RPGCharacterAnims.Actions
{
    /// <summary>
    /// MovementActionHandler is a special type of action handler meant for use with
    /// RPGCharacterMovementController. You pass the movement controller to your action handler
    /// when it is created (see RPGCharacterMovementController#Awake) and in this way the
    /// handler can use the movement controller without actually adding it to
    /// RPGCharacterController. This keeps the character controller independent of
    /// RPGMovementController.
    ///
    /// The movement controller can only be in one of its movement states at any given time
    /// (Move, Jump, Swim, etc.), and the start/end logic is mostly taken care of in the movement
    /// controller itself.
    /// </summary>
    /// <typeparam name="TContext">Context type.</typeparam>
    public abstract class MovementActionHandler<TContext> : InstantActionHandler<TContext>
    {
        protected RPGCharacterMovementController movement;

        public MovementActionHandler(RPGCharacterMovementController movement)
        { this.movement = movement; }
    }
}