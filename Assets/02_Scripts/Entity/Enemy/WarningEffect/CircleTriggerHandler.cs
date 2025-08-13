using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleTriggerHandler : MonoBehaviour
{
    public CircleWarningEffectController circleWarningEffectController;

    public LayerMask targetMask;

    private void Awake()
    {
        targetMask = LayerMask.GetMask("Player");
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & targetMask.value) != 0)
        {
            var damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                circleWarningEffectController.damageablesInCollision.Add(damageable);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            circleWarningEffectController.damageablesInCollision.Remove(damageable);
        }
    }
}
