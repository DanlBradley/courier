using System.Collections.Generic;
using System.Linq;
using Character.AI.AIState.States;
using UnityEngine;
using Utils;

namespace Character.AI.AIState
{
    public class AIBrain
    {
        private AIStateType currentState;
        private readonly AIStateType[] aiStates;
        private readonly Dictionary<AIStateType, States.AIState> logics;
        public readonly NpcBehaviorManager npcManager;
        public readonly NpcConfig npcType;
        public TargetList targets;
        public Target currentTarget;
        public Transform currentTargetTransform;
        public Transform alertedTransform;
        //public float distanceFromTarget = 999f;
        private float targetMemory;
        //private float _alertedMemory = 0f;
        private readonly float noiseAttenuationFloor;
        public float attackCooldownRemaining = 1f;

        public AIBrain(NpcBehaviorManager npcManager, NpcConfig npcConfig)
        {
            this.npcManager = npcManager;
            npcType = npcConfig;
            targets = new TargetList(npcType, npcManager.transform);
            aiStates = npcType.aiStateTypes;
            targetMemory = npcType.memory;
            noiseAttenuationFloor = npcType.noiseAttenuationFloor;
            
            logics = new Dictionary<AIStateType, States.AIState>
            {
                [AIStateType.Idle] = new IdleAIState(this),
                [AIStateType.Follow] = new FollowAIState(this),
                [AIStateType.Attack] = new AttackAIState(this),
                [AIStateType.Chase] = new ChaseAIState(this),
                [AIStateType.CombatIdle] = new CombatIdleState(this),
                [AIStateType.Investigate] = new InvestigateAIState(this),
            };
            
            //Only keep states that exist in this npc type
            var tempLogics = new Dictionary<AIStateType, States.AIState>(logics);
            foreach (var item in tempLogics)
            {
                bool existsFlag = false;
                foreach (var state in aiStates)
                {
                    if (item.Key == state) existsFlag = true;
                }

                if (!existsFlag) logics.Remove(item.Key);
            }
            
            currentState = AIStateType.Idle;
        }

        public void Update()
        {
            if (Time.frameCount % 10 == 0)
            {
                targets.Update(AISenses.LookForTargets(npcManager.transform, npcType));
                currentTarget = targets.GetHighestThreatTarget();

                // Check if the current target's transform is not destroyed
                if (currentTarget?.TargetTransform != null && currentTarget.TargetTransform.gameObject != null)
                {
                    currentTargetTransform = currentTarget.TargetTransform;
                }
                else
                {
                    currentTargetTransform = null; // Clear if the target is destroyed
                }
            }

            if (attackCooldownRemaining > 0) attackCooldownRemaining -= Time.deltaTime;

            AIStateType newState = FindBestEligibleAIState();
            if (currentState != newState)
            {
                logics[currentState].Cleanup();
                logics[newState].Initialize();
            }
            currentState = newState;
            logics[currentState].Update();
            
        }

        
        public void AttemptToHearNoiseSource(Transform origin, float distance, float volume)
        {
            var perceivedVolume = GameMath.SoundAttenuation(volume, distance);
            if (perceivedVolume < noiseAttenuationFloor) return;
            alertedTransform = origin;
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        private AIStateType FindBestEligibleAIState()
        {
            // for now we assume the AI states are in order of appropriateness,
            // which may be nonsensical when there are more states
            foreach (var aiStateType in aiStates)
            {
                if (logics[aiStateType].IsEligible())
                {
                    return aiStateType;
                }
            }

            Debug.LogError("No AI states are valid!?!");
            return AIStateType.Idle;
        }

        /// <summary>
        /// Allows different stats to reset the internal timer preventing the enemy from attacking.
        /// </summary>
        public void ResetAtkCooldown()
        {
            var attackCooldown = npcType.attackCooldown;
            attackCooldownRemaining = GameMath.RandomGaussian(
                attackCooldown * 0.8f, 
                attackCooldown * 1.2f);
        }
    }

    /// TODO: Eventually rebuild this system so there are explicit "priorities" assigned to each state.
    /// <summary>
    /// A library of possible AI States. Ordered in terms of priority.
    /// </summary>
    public enum AIStateType
    {
        Attack,
        Beam,
        Chase,
        Investigate,
        // Patrol,
        // Torpor,
        // Wander,
        CombatIdle,
        Follow, //This is a summon-only state where the summon returns to a radius around the owner if idle and outside radius
        Idle,
    }
    
    //What should a target system look like? It should always be ordered from highest to lowest threat.
    //Anything with more than 0 threat gets attacked. New targets get 0 if friendly, 1 if aggressive.
    //Every time threat is updated, priority list should be re-ordered. This means threat should be updated in the
    //brain, where the target list  can be managed.
    public class TargetList
    {
        public List<Target> targets = new();
        private NpcConfig npcType;
        private Transform npcTransform;

        public TargetList(NpcConfig npcType, Transform npcTransform)
        {
            this.npcType = npcType;
            this.npcTransform = npcTransform;
        }

        public void Update(List<Transform> updatedTargetTransforms)
        {
            // Remove destroyed transforms from updatedTargetTransforms
            updatedTargetTransforms = updatedTargetTransforms.Where(t => t != null && t.gameObject != null).ToList();

            foreach (var transform in updatedTargetTransforms.Where(transform => !IsTransformInList(transform)))
            {
                if (transform != null) // Ensure the transform is not null
                {
                    var newTarget = new Target(transform, npcType.enemyTypes, npcTransform);
                    targets.Add(newTarget);
                }
            }

            List<Target> toRemove = new List<Target>();
            foreach (var target in targets)
            {
                if (target.TargetTransform == null || target.TargetTransform.gameObject == null)
                {
                    toRemove.Add(target);
                    continue; // Skip this target as it's transform is destroyed
                }

                bool targetInLos = updatedTargetTransforms.Any(transform => transform == target.TargetTransform);
                target.inLos = targetInLos;
                target.lastSeen = targetInLos ? 0f : target.lastSeen + Time.deltaTime;

                if (target.lastSeen > npcType.alertedMemory)
                {
                    toRemove.Add(target);
                }
                else
                {
                    target.UpdateDistance();
                }
            }

            // Clean up targets to remove
            foreach (var target in toRemove)
            {
                targets.Remove(target);
            }
        }

        
        //Updates the threat of a specific acquired target, and then re-orders the target list to reflect priority
        //How to identify which target to update threat? First check if the target is in the target list.
        //If not, add to list and add appropriate threat.
        public void UpdateThreatOrAddTarget(Transform targetTransform, float threatUpdateAmount)
        {
            if (IsTransformInList(targetTransform))
            {
                targets[GetTargetIndexFromTransform(targetTransform)].UpdateThreat(threatUpdateAmount);
            }
            else
            {
                var newTarget = new Target(
                    targetTransform, 
                    npcType.enemyTypes,
                    npcTransform);
                targets.Add(newTarget);
                targets.Last().UpdateThreat(threatUpdateAmount);
            }
        }

        /// <summary>
        /// Used to identify highest priority target.
        /// </summary>
        /// <returns></returns>
        public Target GetHighestThreatTarget()
        {
            if (targets == null)
            {
                Debug.LogError("Target list is null.");
                return null;
            }

            List<Target> enemyTargets = new(targets);
            foreach (var target in enemyTargets.Where(target => target.GetThreat() <= 0f).ToList())
            {
                enemyTargets.Remove(target);
            }

            return enemyTargets.Count > 0 ? 
                enemyTargets.OrderByDescending(item => item.GetThreat()).First() : null;
        }


        /// <summary>
        /// Check if transform is in the list and return the index of that target if it is
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        private bool IsTransformInList(Transform transform)
        {
            return targets.Any(target => target.CompareTarget(transform));
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private int GetTargetIndexFromTransform(Transform transform)
        {
            if (!IsTransformInList(transform))
            {
                Debug.LogError("Error: Transform is not currently saved in the list of targets.");
            }
            for (var i = 0; i < targets.Count; i++)
            {
                if (targets[i].CompareTarget(transform))
                {
                    return i;
                }
            }
            Debug.LogError("Error: Transform is not currently saved in the list of targets.");
            return -1;
        }
    }
    
    public class Target
    {
        private float threat;
        private string name;
        private string type;
        public Transform TargetTransform { get; private set; }
        public Transform NpcTransform { get; private set; }
        public float lastSeen = 0;
        public float lastKnownDistance;
        public bool inLos = true;
        
        public Target(Transform targetTransform, string[] enemyTypes, Transform npcTransform)
        {
            TargetTransform = targetTransform;
            NpcTransform = npcTransform;
            name = TargetTransform.name;
            type = TargetTransform.tag;
            threat = 0f;
            UpdateDistance();
            //Check if target is an enemy and give it 1 threat
            foreach (var unused in enemyTypes.Where(type => type == this.type))
            {
                threat = 1f;
            }
        }

        public float GetThreat()
        {
            return threat;
        }

        public void UpdateThreat(float updateAmount)
        {
            threat += updateAmount;
        }

        public void UpdateDistance()
        {
            lastKnownDistance = AISenses.DistanceFromTarget(TargetTransform, NpcTransform);
        }

        /// <summary>
        /// Return true if same target, false otherwise
        /// </summary>
        /// <returns></returns>
        public bool CompareTarget(Transform otherTarget)
        {
            return TargetTransform == otherTarget;
        }
    }
}
