using System.Collections.Generic;
using Character.AI.AIState;
using UnityEngine;

namespace Character.AI
{
    public static class AISenses
    {
        //Task: Give vision tools to an AI
        //Requirements: Make available for the AIBrain so every tick any state
        //knows whether there is a target, and how far away it is
        
        /// <summary>
        /// Broadcast search of all targets in vicinity. NPCs keep record of both friendlies and enemies far various
        /// porpoises.
        /// </summary>
        /// <param name="npcTransform"></param>
        /// <param name="npcType"></param>
        /// <returns></returns>
        public static List<Transform> LookForTargets(Transform npcTransform, NpcConfig npcType)
        {
            var colliders = Physics.OverlapSphere(
                npcTransform.position, 
                npcType.baseVisionDistance, 
                npcType.targetableLayer);
            var acquiredTargets = new List<Transform>();
            foreach (var collider in colliders)
            {
                if (collider.transform == npcTransform) continue;
                // Debug.Log($"Identified a target that is NOT this npc: {collider.transform.name}");
                Vector3 targetRelativeVec = collider.transform.position - npcTransform.position;
                float angleToTarget = Vector3.Angle(targetRelativeVec, npcTransform.forward);
                if (angleToTarget > npcType.maxFOV) continue;
                // Debug.Log("Identified a target that is in field of view");
                
                //Step 3. Check if the target is in LOS: if anything is in the way, the target is NOT visible.
                //bool targetInLOS = false;
                RaycastHit[] raycastHits = Physics.RaycastAll(npcTransform.position, 
                    collider.transform.position - npcTransform.position,
                    npcType.baseVisionDistance);
                
                if (raycastHits.Length <= 0) continue;
                // Debug.Log($"Identified a target that is within range: {collider.transform.name}");
                
                //This is safe for now because the player will always be at the top of the scene hierarchy
                //TODO: Find another way to safely and efficiently access the player from any body part.
                //TODO: Maybe load in the player immediately for each body part?
                acquiredTargets.Add(collider.transform);
            }
            // Debug.Log("Acquired targets in lookfortargets func: " + acquiredTargets.Count);
            return acquiredTargets;
        }

        public static bool IsTargetInRange(Transform targetTransform, Transform npcTransform, NpcConfig npcType)
        {
            if (targetTransform is null) return false;
            var distanceFromTarget = Vector3.Distance(
                targetTransform.position, 
                npcTransform.position);
            return distanceFromTarget < npcType.baseVisionDistance;
        }

        public static bool IsTargetInLos(Transform targetTransform, Transform npcTransform, NpcConfig npcType)
        {
            //First check if target is in FOV. If not, fail!
            var position = npcTransform.position;
            var targetRelativeVec = targetTransform.position - position;
            var angleToTarget = Vector3.Angle(targetRelativeVec, npcTransform.forward);
            if (angleToTarget > npcType.maxFOV) return false;
            //Debug.Log("Target is in FOV. Checking if player can be detected...");
            //Okay NPC can see target based on FOV. Now what? Check if anything is in the way. If the first thing is a
            //player, it's a hit! If not, the target isn't in LOS.
            var originOffset = position + Vector3.up * 1;
            var targetOffset = targetTransform.position + Vector3.up * 1;
            var hits = Physics.RaycastAll(originOffset, 
                targetOffset - originOffset,
                npcType.baseVisionDistance);
            //Sorts each ray by distance
            System.Array.Sort(hits,
                (a, b) =>
                    (a.distance.CompareTo(b.distance)));
            foreach (var hit in hits)
            {
                //Debug.Log("Hit: " + hit.transform.name);
            }
            //Debug.Log($"Is target in LOS? {hits.Length > 0 && hits[0].transform.CompareTag("Player")}");
            return hits.Length > 0 && hits[0].transform.CompareTag("Player");
        }

        public static float DistanceFromTarget(Transform targetTransform, Transform npcTransform)
        {
            return Vector3.Distance(targetTransform.position, npcTransform.position);
        }

        public static Transform ListenForPlayer(Transform npcTransform, NpcConfig npcType)
        {
            //A noise "source" should find nearby npcManagers that can hear and assign "lastHeardTarget"
            //For now, this can be empty
            return null;
        }
    }
}