using System.Collections;
using UnityEngine;
using System;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using DG.Tweening;



public class StickyBall : MonoBehaviour, SkillAction
{
    public Action onStart { get; set; }
    public Action onMiddle { get; set; }
    public Action onHit { get; set; }
    public Action onEnd { get; set; }
    public Action onEnemy { get; set; }
    public bool through { get; set; }
    public bool homing { get; set; }
    public float duration { get; set; }
    public ElementalType elemental { get; set; }
    public float damage { get; set; }
    public bool enemy { get; set; }
    public GameObject caster { get; set; }
    public string objName { get; set; }
    public float speed { get; set; }

    [Header("Controller")]
    [SerializeField] private SkillSoundController soundController;
    [SerializeField] private SkillVFXController vfxController;

    private CancellationTokenSource cts;

    public void Init(Vector3 pos)
    {
        // 마우스 방향 벡터 계산 및 정규화
        Vector2 direction = (pos - PlayerController.Instance.transform.position).normalized;

        // 방향 벡터로 각도 계산 (라디안 → 도 단위)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 오브젝트 회전 설정 (Z축 회전)
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        // // Rigidbody2D에 방향 * 속도로 속도 설정 → 발사
        // rb.velocity = direction * speed;

        onMiddle?.Invoke();
        GetComponent<Animator>().SetInteger("Elemental", (int)elemental);

        soundController.StartSound(objName); // 시작 사운드

        FloorEffect floor = GetComponent<FloorEffect>(); // floor 설절
        floor.damage = damage;
        floor.delay = 0.2f;
        floor.elemental = elemental;
        floor.soundController = soundController;
        floor.vfxController = vfxController;

        StartCoroutine(Return(objName));
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && !enemy) // Enemy 적중
        {
            speed = 1;
            onHit?.Invoke();
            soundController.HitSound(elemental, objName); // Hit Sound
            vfxController.GetVFX(elemental, transform.rotation, gameObject.transform.position);
            collision.GetComponent<IDamageable>().OnDamage(damage);
            if (collision.TryGetComponent<DebuffController>(out DebuffController debuffController))
            {
                debuffController.TakeDebuff(collision, elemental);
            }
            if (!through)
            {
                speed = 0;
                cts?.Cancel();
                cts?.Dispose();
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && enemy) // Player 적중
        {
            collision.GetComponent<IDamageable>().OnDamage(damage);
            vfxController.GetVFX(elemental, transform.rotation, gameObject.transform.position);
            // soundController.HitElementalSound(elemental, objName); // Hit Sound
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && !enemy && through)
        {
            speed = 5;
        }
    }

    public IEnumerator Return(string name)
    {
        yield return YieldCache.WaitForSeconds(duration);
        GetComponent<SpriteRenderer>().sprite = null;
        onEnd?.Invoke();
        ObjectPoolManager.Instance.Return(this.gameObject, objName);
    }


    public void EnemyInit()
    {

    }

    private void Update()
    {
        transform.position += transform.right * speed * Time.deltaTime;
    }



}