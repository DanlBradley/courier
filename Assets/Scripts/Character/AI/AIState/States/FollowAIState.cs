using Character.NPCs;
using UnityEngine;

namespace Character.AI.AIState.States
{
    /// <summary>
    /// Follows owner transform if outside of radius & combat
    /// </summary>
    public class FollowAIState : AIState
    {
        private readonly AIBrain _aiBrain;
        private SummonManager _sm;
        private bool _following;
    
        public FollowAIState(AIBrain brain)
        {
            _aiBrain = brain;
        }
        
        public override bool IsEligible()
        {
            if (_following)
            {
                Debug.Log("Still approaching target. Continue following.");
                return true;
            }
            if (!_aiBrain.npcManager.transform.TryGetComponent<SummonManager>(out var sm))
            {
                return false;
            }
            _sm = sm;

            return _sm.IsOutOfRange();
        }

        public override void Initialize()
        {
            Debug.Log("Initialized follow state");
        }

        public override void Cleanup()
        {
            //Stop following the target!
            _aiBrain.npcManager.agent.isStopped = true;
            _following = false;
            _sm = null;
            Debug.Log("Cleaning up follow state");
        }

        public override void Update()
        {
            //Idk check for issues like agent can't get to target etc.?
            //if within 1m of target, stop following
            
            
            //start following the target!
            _aiBrain.npcManager.NavigateToTarget(_sm.owner.position + _sm.owner.forward);
            _following = true;
            
            if (_aiBrain.npcManager.agent.remainingDistance < 2f)
            {
                _following = false;
            }
        }
    }
}