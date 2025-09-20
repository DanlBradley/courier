using UnityEngine;

namespace Character.NPCs
{
    /// <summary>
    /// Keeps track of who the owner is and other summon-unique data.
    /// </summary>
    public class SummonManager : MonoBehaviour
    {
        public Transform owner;
        public float maxLeashDistance;

        public bool IsOutOfRange()
        {
            var dist = Vector3.Distance(transform.position, owner.transform.position);
            return dist > maxLeashDistance;
        }
    }
}