using UnityEngine;
using Interfaces;

namespace Combat
{
    [System.Serializable]
    public struct AttackInfo
    {
        public IWeapon weapon;
        public int comboIndex;
        public float chargeAmount;
        public Transform attackOrigin;
        public LayerMask targetLayers;
        
        public AttackInfo(IWeapon weapon, Transform attackOrigin, LayerMask targetLayers)
        {
            this.weapon = weapon;
            this.attackOrigin = attackOrigin;
            this.targetLayers = targetLayers;
            this.comboIndex = 0;
            this.chargeAmount = 0f;
        }
    }
}