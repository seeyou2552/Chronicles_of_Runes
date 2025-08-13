using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using System.Linq;

public class Orbiter : MonoBehaviour
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

    public float radiusX;
    public float radiusY;
    public float speed;
    public float angleOffset;
    public float rotateSpeed = 200f;
    public float searchRadius = 200f;
    private Transform target;

    public SkillSoundController soundController;
    public SkillVFXController vfxController;


    private float angle;
    private CancellationTokenSource cts;

    public void Init()
    {
        GetComponent<Animator>().SetInteger("Elemental", (int)elemental);
        target = null;

        angle = 0f;
        angle += speed * 2f * Mathf.PI * Time.deltaTime;

        float totalAngle = angle + angleOffset;

        float x = radiusX * Mathf.Cos(totalAngle);
        float y = radiusY * Mathf.Sin(totalAngle);

        transform.position = caster.transform.position + new Vector3(x, y, 0f);


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

        cts = new CancellationTokenSource();

        Return(cts.Token).Forget();
    }

    void Update()
    {
        if (homing && target != null )
        {
            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
            float rotateAmount = Vector3.Cross(direction, transform.right).z;

            GetComponent<Rigidbody2D>().angularVelocity = -rotateAmount * rotateSpeed;
            GetComponent<Rigidbody2D>().velocity = transform.right * (speed * 10);
        }
        else
        {
            angle += speed * 2f * Mathf.PI * Time.deltaTime; // 초당 회전 각도 누적

            float totalAngle = angle + angleOffset;

            float x = radiusX * Mathf.Cos(totalAngle);
            float y = radiusY * Mathf.Sin(totalAngle);

            transform.position = caster.transform.position + new Vector3(x, y, 0f);
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
            if(collision.TryGetComponent<DebuffController>(out DebuffController debuffController))
            {
                debuffController.TakeDebuff(collision, elemental);
            }
            GetComponent<Animator>().SetInteger("Elemental", -1);
            if (!through)
            {
                onEnd?.Invoke();
                target = null;
                cts?.Cancel();
                ObjectPoolManager.Instance.Return(this.gameObject, objName);
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && enemy)
        {
            collision.GetComponent<IDamageable>().OnDamage((damage + PlayerController.Instance.AttackPower())*PlayerController.Instance.DamageOutcome());
            ObjectPoolManager.Instance.Return(this.gameObject, "EnemyOrbiter");
        }
    }

    public async UniTaskVoid Return(CancellationToken token)
    {
        await UniTask.Delay((int)(duration * 1000), cancellationToken: token);
        if (this == null || gameObject == null) return;
        onEnd?.Invoke();
        target = null;
        GetComponent<Animator>().SetInteger("Elemental", -1);
        if (!enemy) ObjectPoolManager.Instance.Return(this.gameObject, objName);
        else if (enemy) ObjectPoolManager.Instance.Return(this.gameObject, "EnemyOrbiter");
    }

}
