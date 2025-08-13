using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HomingBeam : MonoBehaviour
{
    public Action onHit;
    public float duration;
    public bool through;
    public bool homing;
    public ElementalType elemental;
    public float damage;
    public float speed;
    public bool enemy;
    public GameObject caster;
    public string objName;
    public SkillSoundController soundController;
    public SkillVFXController vfxController;

    public Transform target;
    public float rotateSpeed = 200f;
    public float searchRadius = 200f;

    private SpriteRenderer sprite;

    public void Init()
    {
        if (sprite == null) sprite = GetComponent<SpriteRenderer>();
        ChangeColor();

        GameObject[] GetEnemiesByPhysics()
        {
            // 현재 오브젝트 위치를 중심으로 반경 searchRadius 내의 Collider2D를 레이어 마스크로 검색
            Collider2D[] hits = Physics2D.OverlapCircleAll(gameObject.transform.position, searchRadius, LayerMask.GetMask("Enemy"));
            return hits
                .Select(c => c.gameObject)
                .Distinct()  // 중복 제거
                .ToArray();
        }

        var enemies = GetEnemiesByPhysics();
        Logger.Log(enemies.Length.ToString());

        Transform closest = null;
        float minDist = Mathf.Infinity;
        Vector3 origin = PlayerController.Instance.transform.position;

        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(origin, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy.transform;
            }
        }
        target = closest;

        StartCoroutine(Return());
    }

    void Update()
    {
        if (target != null)
        {
            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
            float rotateAmount = Vector3.Cross(direction, transform.right).z;

            if (GetComponent<Rigidbody2D>() == null) return;
            GetComponent<Rigidbody2D>().angularVelocity = -rotateAmount * rotateSpeed;
            GetComponent<Rigidbody2D>().velocity = transform.right * speed;
        }
        else
        {
            transform.position += transform.right * speed * Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && !enemy)
        {
            onHit?.Invoke();
            soundController.HitSound(elemental, "MagicBeam"); // Hit Sound
            vfxController.GetVFX(elemental, transform.rotation, gameObject.transform.position);
            collision.GetComponent<IDamageable>().OnDamage(damage);
            if (collision.TryGetComponent<DebuffController>(out DebuffController debuffController))
            {
                debuffController.TakeDebuff(collision, elemental);
            }
            GetComponent<Animator>().SetInteger("Elemental", -1);
            if (!through)
            {
                target = null;
                ObjectPoolManager.Instance.Return(this.gameObject, objName);
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && enemy)
        {

        }
    }

    public IEnumerator Return()
    {
        yield return YieldCache.WaitForSeconds(duration);
        elemental = ElementalType.Normal;
        ObjectPoolManager.Instance.Return(this.gameObject, objName);
    }
    
    private void ChangeColor()
    {
        switch (elemental)
        {
            case ElementalType.Normal:
                sprite.color = new Color(1, 1, 1);
                break;
            case ElementalType.Fire:
                sprite.color = new Color(1, 0, 0);
                break;
            case ElementalType.Water:
                sprite.color = new Color(0, 0.5f, 1);
                break;
            case ElementalType.Ice:
                sprite.color = new Color(0, 1, 1);
                break;
            case ElementalType.Electric:
                sprite.color = new Color(1, 1, 0);
                break;
            case ElementalType.Dark:
                sprite.color = new Color(1, 0, 1);
                break;
            case ElementalType.Light:
                sprite.color = new Color(1, 0.8f, 0);
                break;
        }
    }
}
