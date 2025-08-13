using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorEffect : MonoBehaviour
{
    public float damage;
    public float delay;
    public bool enemy;
    public string objName;
    public ElementalType elemental;
    public SkillSoundController soundController;
    public SkillVFXController vfxController;

    private HashSet<Collider2D> floorTargets = new HashSet<Collider2D>();

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null) return;
        if (damage == 0) return;
        
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && !enemy)
        {
            collision.GetComponent<IDamageable>().OnDamage(damage);
            if (vfxController != null) vfxController.GetVFX(elemental, transform.rotation, collision.transform.position);
            collision.GetComponent<DebuffController>().TakeDebuff(collision, elemental);
            floorTargets.Add(collision); // 장판 지속
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && enemy)
        {
            collision.GetComponent<IDamageable>().OnDamage(damage);
            floorTargets.Add(collision);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && !enemy)
        {
            floorTargets.Remove(collision); // 장판 데미지 해제 
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && enemy)
        {
            floorTargets.Remove(collision); // 장판 데미지 해제 
        }
    }

    void OnEnable()
    {
        if (delay == 0) return;
        InvokeRepeating(nameof(TakeDamage), 0f, delay);
    }

    void OnDisable()
    {
        floorTargets.Clear();
        CancelInvoke(nameof(TakeDamage));
    }

    void TakeDamage()
    {
        if (floorTargets == null) return;
        if (damage == 0) return; 
        foreach (var target in floorTargets)
        {
            if (target != null)
            {
                if (!target.GetComponent<EnemyController>().isDead)
                {
                    target.GetComponent<IDamageable>().OnDamage(damage);
                    target.GetComponent<DebuffController>().TakeDebuff(target, elemental);
                    if (vfxController != null) vfxController.GetVFX(elemental, transform.rotation, target.transform.position);
                    if (soundController != null) soundController.HitSound(elemental, objName);
                }
            }
        }
    }
}
