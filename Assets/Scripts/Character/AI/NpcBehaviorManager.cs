using System;
using Character.AI.AIState;
using Systems;
using UnityEngine;
using UnityEngine.AI;

namespace Character.AI
{
    public class NpcBehaviorManager : MonoBehaviour
    {
        /// <summary>
        /// Animation controller driven
        /// </summary>
        public bool isPerformingAction;
        
        public NpcConfig npcType;
        [HideInInspector] public NavMeshAgent agent;
        private AIBrain aiBrain;

        [Header("--DEBUG SETTINGS--")] 
        public Transform currentTargetTransform;

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            aiBrain = new AIBrain(this, npcType);
        }

        protected void Update()
        {
            aiBrain?.Update();

            //Handle humanoid movement animations
            if (agent == null) return;
        }

        public void TakeDamage(Transform targetTransform, float tempDamage)
        {
            aiBrain.targets.UpdateThreatOrAddTarget(targetTransform, tempDamage);
            //TODO: Actually take damage from NPC health
        }

        private void CheckDeathCondition(float previousValue, float currentValue)
        {
            if (currentValue > 0f) return;
        }

        //TODO: Right now this is pretty static. Need to update so that it can take in values like whether the enemy is
        //TODO: sprinting, etc.
        private void HandleAgentAnimations()
        {
            Vector3 agentVelocity = agent.velocity;
            Vector3 forward = transform.forward;
            // characterAnimatorManager.UpdateAnimatorValues(
            //     agentVelocity.y * forward.y / 2f,
            //     agentVelocity.x * forward.x / 2f);
        }

        public void CheckNoiseLevel(Component sender, object data)
        {
            if (data is not float noiseVolume) return;
            if (sender is not NoiseSource noiseSource) return;
            if (noiseSource.transform == transform) return;
            if (noiseSource.transform == transform.root) return;
            float distance = Vector3.Distance(transform.position, noiseSource.transform.position);
            // Debug.Log($"Checked for noise volume of {noiseVolume} from {noiseSource.name} at a distance {distance}");
            aiBrain.AttemptToHearNoiseSource(noiseSource.transform, distance, noiseVolume);
        }

        public void NavigateToTarget(Vector3 position)
        {
            if (isPerformingAction)
            {
                //Stop moving the NavMeshAgent
                //agent.isStopped = true;
            }
            else
            {
                agent.isStopped = false;
                agent.updateRotation = true;
                agent.speed = npcType.navAgentData.movementSpeed;
                agent.angularSpeed = npcType.navAgentData.angularSpeed;
                agent.stoppingDistance = npcType.navAgentData.stoppingDistance;

                //Start moving the NavMeshAgent
                agent.SetDestination(position);
            }
        }
    }
}
