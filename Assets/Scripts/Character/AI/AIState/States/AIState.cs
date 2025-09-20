namespace Character.AI.AIState.States
{
    /// <summary>
    /// Base class for all AIStates
    /// </summary>
    public abstract class AIState
    {
        /// <summary>
        /// Indicates whether this state thinks it can become/continue to be the active state.
        /// </summary>
        public abstract bool IsEligible();

        /// <summary>
        /// Called once each time this state becomes the active state.
        /// (This will only happen if IsEligible() has returned true for this state)
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Called once this state is no longer active. Used to reset / update anything that needs to happen as the
        /// NPC leaves this state.
        /// </summary>
        public abstract void Cleanup();

        /// <summary>
        /// Called once per frame while this is the active state. Initialize() will have
        /// already been called prior to Update() being called
        /// </summary>
        public abstract void Update();

    }
}