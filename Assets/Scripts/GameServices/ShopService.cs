using Interfaces;
using UnityEngine;
using Utils;

namespace GameServices
{
    public class ShopService : Service
    {
        
        public override void Initialize()
        {
            Logs.Log("Shop service initialized.", "GameServices");
        }
        public void OpenShop(string shopID) { Debug.Log("Opened the shop! :)  - haha lol jk not implemented"); }
    }
}
