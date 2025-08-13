using _02_Scripts.NPC;
using UnityEngine;

public class PortalNPCController : NPCController
{
    public override void Interact()
    {
        GameObject player = PlayerController.Instance.gameObject;
        if (player != null)
        {
            //player.transform.position = new Vector3(0f, 0f, 0f);
        }
        GameManager.Instance.ChangeState(GameState.Boss);
    }
}