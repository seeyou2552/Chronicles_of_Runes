using UnityEngine;
using UnityEngine.SceneManagement;

namespace _02_Scripts.NPC
{
    public class CaveNPCController : NPCController
    {
        public override void Interact()
        {
            if (GameManager.Instance.state == GameState.Tutorial)
            {
                StaticNoticeManager.Instance.ShowMainNotice("이곳이 어딘지 알지못하니, 돌아가서 마법사에게 말을 걸어보자.");
            }
            else
            {
                GameManager.Instance.ChangeState(GameState.Dungeon);
            }
            
        }
    }
}