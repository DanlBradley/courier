using System;
using UnityEngine;
using Utils;

namespace GameServices
{
    public class TickService : Service
    {
        public static TickService Instance { get; private set; }
        public event Action OnTick;
        [SerializeField] private float tickInterval = 1f;
        private float _timeSinceLastTick;

        public override void Initialize()
        {
            if (Instance && Instance != this) { Destroy(gameObject); } else { Instance = this; }
            Logs.Log("Tick Service initialized.", "GameServices");
        }
        

        private void Update()
        {
            _timeSinceLastTick += Time.deltaTime;
            if (!(_timeSinceLastTick >= tickInterval)) return;
            _timeSinceLastTick -= tickInterval;
            OnTick?.Invoke();
        }

        public void SetTickInterval(float interval) { tickInterval = interval; }
    }
}
