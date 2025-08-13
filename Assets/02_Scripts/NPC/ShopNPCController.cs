using UnityEngine;

namespace _02_Scripts.NPC
{
    public class ShopNPCController : NPCController
    {
        [SerializeField] StoreManager storeManager;

        private void Start()
        {
            if (storeManager == null)
            {
                storeManager = StoreManager.Instance;
            }
        }
        public override void Interact()
        {
            storeManager.OnOffStore();
        }
    }
}