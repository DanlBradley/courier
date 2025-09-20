using GameServices;
using UnityEngine;

namespace Character.Player
{
    public class FollowPlayerCamController : MonoBehaviour
    {
        private Transform _player;
        private readonly Vector3 _offset = new Vector3(0, 0, 10);
        private void Start()
        {
            _player = GameManager.Instance.GetPlayer().transform;
        }

        private void LateUpdate()
        {
            transform.position = _player.position - _offset;
        }
    }
}
