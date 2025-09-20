namespace Character.AI.AIState.States
{
    /// <summary>
    /// An aware state where the NPC can "see" and "hear", but has not yet identified any targets.
    /// </summary>
    public class IdleAIState : AIState
    {
        private readonly AIBrain _aiBrain;
        
        public IdleAIState(AIBrain brain)
        {
            _aiBrain = brain;
        }
        
        public override bool IsEligible()
        {
            return _aiBrain.currentTarget is null;
        }

        public override void Initialize()
        {
            return;
        }

        public override void Cleanup()
        {
            return;
        }

        public override void Update()
        {
            return;
        }

    }
}