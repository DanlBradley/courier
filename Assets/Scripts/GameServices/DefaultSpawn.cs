using UnityEngine;

namespace GameServices
{
    public class DefaultSpawn : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private string spawnId = "default";
        [SerializeField] private bool isDefaultSpawn = true;
        
        private void Awake()
        {
            if (isDefaultSpawn)
            {
                gameObject.tag = "DefaultSpawn";
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = isDefaultSpawn ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawRay(transform.position, transform.forward * 2f);
        }
        
        public string GetSpawnId()
        {
            return spawnId;
        }
        
        public void SetAsDefaultSpawn()
        {
            isDefaultSpawn = true;
            gameObject.tag = "DefaultSpawn";
        }
    }
}