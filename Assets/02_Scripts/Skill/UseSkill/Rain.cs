using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using System.Linq;
using System.Collections;

public class Rain : MonoBehaviour
{

    public Action onHit;
    public Action onEnd;
    public float duration;
    public bool through;
    public bool homing;
    public ElementalType elemental;
    public float damage;
    public bool enemy;
    public GameObject caster;
    public string objName;

    public float speed;
    public float rotateSpeed = 200f;
    public float searchRadius = 200f;
    private Transform target;

    public SkillSoundController soundController;
    public SkillVFXController vfxController;

    public void Init()
    {
        GetComponent<Animator>().SetInteger("Elemental", (int)elemental);
        target = null;
        if(elemental == ElementalType.Electric) soundController.StartSound(elemental, objName);


        if (homing)
        {
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
        }

        StartCoroutine(Return());
    }

    void Update()
    {
        if (homing && target != null) // 호밍 적용
        {
            if (elemental != ElementalType.Electric)
            {
                Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
                float rotateAmount = Vector3.Cross(direction, transform.right).z;

                GetComponent<Rigidbody2D>().angularVelocity = -rotateAmount * rotateSpeed;
                GetComponent<Rigidbody2D>().velocity = transform.right * speed;
            }
            else // electric의 경우 호밍 효과 변경
            {
                gameObject.transform.position = target.transform.position;
            }

        }
        else if (elemental != ElementalType.Electric)
        {
            transform.position += Vector3.down * speed * Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && !enemy)
        {
            onHit?.Invoke();
            soundController.HitSound(elemental, objName); // Hit Sound
            vfxController.GetVFX(elemental, transform.rotation, gameObject.transform.position);
            collision.GetComponent<IDamageable>().OnDamage(damage);
            collision.GetComponent<DebuffController>()?.TakeDebuff(collision, elemental);
            GetComponent<Animator>().SetInteger("Elemental", -1);
            if (!through && elemental != ElementalType.Electric)
            {
                onEnd?.Invoke();
                target = null;
                ObjectPoolManager.Instance.Return(this.gameObject, objName);
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && enemy)
        {
            collision.GetComponent<IDamageable>().OnDamage(damage);
            ObjectPoolManager.Instance.Return(this.gameObject, "EnemyRain");
        }
    }

    public IEnumerator Return()
    {
        yield return YieldCache.WaitForSeconds(0.5f);

        if(elemental != ElementalType.Electric) vfxController.GetVFX(elemental, transform.rotation, gameObject.transform.position);
        if(elemental != ElementalType.Electric) soundController.HitSound(elemental, objName); // Hit Sound
        onEnd?.Invoke();
        target = null;
        GetComponent<Animator>().SetInteger("Elemental", -1);
        if (!enemy) ObjectPoolManager.Instance.Return(this.gameObject, objName);
        else if (enemy) ObjectPoolManager.Instance.Return(this.gameObject, "EnemyRain");
    }

}
