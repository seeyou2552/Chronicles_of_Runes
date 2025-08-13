using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarSafeZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            PlayerController.Instance.isInvincible = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            PlayerController.Instance.isInvincible = false;
        }
    }

    private void OnDestroy()
    {
        PlayerController.Instance.GetComponent<Collider2D>().enabled = true;
    }
}
