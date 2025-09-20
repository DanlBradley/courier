using UnityEngine;

namespace Character.AI.AIState.States
{
    public class InvestigateAIState : AIState
    {
        private readonly AIBrain aiBrain;
        private bool investigating;
    
        public InvestigateAIState(AIBrain brain)
        {
            aiBrain = brain;
        }
    
        public override bool IsEligible()
        {
            var isCloseEnoughToTarget = false;
            if (aiBrain.alertedTransform != null)
            {
                isCloseEnoughToTarget = Vector3.Distance(aiBrain.alertedTransform.position, aiBrain.npcManager.transform.position) <= aiBrain.npcType.attackRange;
            }
            return (aiBrain.alertedTransform is not null
                    && aiBrain.currentTarget is null 
                    && !isCloseEnoughToTarget);
        
        }

        public override void Initialize()
        {
        }

        public override void Cleanup()
        {
            investigating = false;
            aiBrain.alertedTransform = null;
        }

        public override void Update()
        {
            //Requirements: Move towards heard object. If within stopping distance, clear state and object
            //TODO: Make it so the enemy, upon hearing something that is "louder", turns their attention to that
            if (!investigating) MoveTowardsSource();
            if (aiBrain.npcManager.agent.remainingDistance <= aiBrain.npcType.attackRange)
            {
                aiBrain.alertedTransform = null;
            }
        }

        private void MoveTowardsSource()
        {
            investigating = true;
            aiBrain.npcManager.NavigateToTarget(aiBrain.alertedTransform.position);
            //Debug.Log("Distance from investigate point:" + Vector3.Distance(_aiBrain.alertedTransform.position, _aiBrain.npcManager.transform.position));
        }
    }
}