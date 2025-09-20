using UnityEngine;

namespace Character.AI.AIState.States
{
    /// <summary>
    /// The NPC is closing their distance until they're within engagement range
    /// </summary>
    public class ChaseAIState : AIState
    {
        private readonly AIBrain _aiBrain;
        private Transform _targetTransform;
    
        public ChaseAIState(AIBrain brain)
        {
            _aiBrain = brain;
        }
    
        public override bool IsEligible()
        {
            return _aiBrain.currentTarget is not null &&
                   _aiBrain.currentTarget.lastKnownDistance > _aiBrain.npcType.engagementDistance;
        }

        public override void Initialize()
        {
            _targetTransform = _aiBrain.currentTargetTransform;
        }

        public override void Cleanup()
        {
            //Stop moving the navMeshAgent
            _aiBrain.npcManager.agent.isStopped = true;
        }

        public override void Update()
        {
            _aiBrain.npcManager.NavigateToTarget(_targetTransform.position);
        }
    }
}