using UnityEngine;

namespace Character.AI.AIState.States
{
    public class CombatIdleState : AIState
    {
        private readonly AIBrain _aiBrain;
        private Transform _targetTransform;
    
        public CombatIdleState(AIBrain brain)
        {
            _aiBrain = brain;
        }
    
        public override bool IsEligible()
        {
            if (_aiBrain.currentTarget is null) return false;
            if (_aiBrain.currentTarget.lastKnownDistance > _aiBrain.npcType.engagementDistance * 1.1f) return false;
            if (_aiBrain.attackCooldownRemaining <= 0f) return false;
            return true;
        }

        public override void Initialize()
        {
            _targetTransform = _aiBrain.currentTargetTransform;
        }

        public override void Cleanup()
        {
            
        }

        public override void Update()
        {
            //Right now just lock on to the player
            LockOn();
            
            //TODO: Do stuff like... change animation blend tree to "engagement zone" or something
        }
        
        
        private void LockOn()
        {
            var transform = _aiBrain.npcManager.transform;
            var targetPosition = _targetTransform.position;
            targetPosition.y = transform.position.y;
            transform.LookAt(targetPosition);
        }
    }
}