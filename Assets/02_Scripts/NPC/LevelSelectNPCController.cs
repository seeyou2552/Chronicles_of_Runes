using _02_Scripts.UI;
using UnityEngine;

namespace _02_Scripts.NPC
{
    public class LevelSelectNPCController : NPCController
    {
        [SerializeField] LevelSelectUI levelSelectUI;

        void Awake()
        {
            if(levelSelectUI == null)levelSelectUI = UIManager.Instance.GetComponentInChildren<LevelSelectUI>();
        }

        public override void Interact()
        {
            if(levelSelectUI == null)levelSelectUI = UIManager.Instance.GetComponentInChildren<LevelSelectUI>();
            levelSelectUI.OnOffPanel();
        }
    }
}