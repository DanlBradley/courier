using UnityEngine;
using UnityEngine.Android;
using Utils;

namespace Character.AI.AIState.States
{
    /// <summary>
    /// A basic melee attack state. Can be activated while enemy is in engagement range.
    /// </summary>
    public class AttackAIState : AIState
    {
        private readonly AIBrain _aiBrain;
        private Transform _targetTransform;
        
        public AttackAIState(AIBrain brain)
        {
            _aiBrain = brain;
        }
        
        public override bool IsEligible()
        {
            if (_aiBrain.currentTarget is null) return false;
            if (_aiBrain.currentTarget.lastKnownDistance > _aiBrain.npcType.engagementDistance * 1.1f) return false;
            if (_aiBrain.attackCooldownRemaining > 0f) return false;
            return true;
        }

        public override void Initialize()
        {
            _targetTransform = _aiBrain.currentTargetTransform;
        }

        public override void Cleanup()
        {
            _aiBrain.npcManager.agent.isStopped = true;
        }

        public override void Update()
        {
            if (_aiBrain.currentTarget.lastKnownDistance <= _aiBrain.npcType.attackRange)
            {
                //Stop the agent
                _aiBrain.npcManager.agent.isStopped = true;
                LockOn();
                MainAttack();
            }
            else
            {
                _aiBrain.npcManager.NavigateToTarget(_targetTransform.position);
            }
        }
        
        private void LockOn()
        {
            var transform = _aiBrain.npcManager.transform;
            var targetPosition = _targetTransform.position;
            targetPosition.y = transform.position.y;
            transform.LookAt(targetPosition);
        }
        
        private void MainAttack()
        {
            // Debug.Log("Attacking!");
            //TODO: Handle Attack
            
            //make this based on npc type or a function or something
            _aiBrain.ResetAtkCooldown();
        }
    }
}
