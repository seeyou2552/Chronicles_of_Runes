using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class MagicFloor : MonoBehaviour, SkillAction
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

    private CancellationTokenSource cts;

    public void Init(Vector3 pos)
    {
        onStart?.Invoke();
        onMiddle?.Invoke();

        // 마우스 방향 벡터 계산 및 정규화
        Vector2 direction = (pos - PlayerController.Instance.transform.position).normalized;

        // 방향 벡터로 각도 계산 (라디안 → 도 단위)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 오브젝트 회전 설정 (Z축 회전)
        transform.rotation = Quaternion.Euler(0, 0, angle);
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        // Rigidbody2D에 방향 * 속도로 속도 설정 → 발사
        rb.velocity = direction * speed;

        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        DelayExplore(cts.Token).Forget();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && !enemy)
        {
            onHit?.Invoke();

            if (!through)
            {
                Explore();
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle")) // 벽 적중
        {
            Explore();
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && enemy)
        {
            Explore();
        }
    }


    void Explore()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        transform.GetChild(0).gameObject.SetActive(true);
        FloorEffect floor = transform.GetChild(0).GetComponent<FloorEffect>();
        floor.damage = damage;
        floor.delay = 1f;

        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        DelayHide(cts.Token).Forget();
    }

    public async UniTaskVoid DelayExplore(CancellationToken token)
    {
            await UniTask.Delay((int)(duration * 1000), cancellationToken: token);
            if (this == null || gameObject == null) return;
            Explore();
    }

    public async UniTaskVoid DelayHide(CancellationToken token)
    {
        try
        {
            await UniTask.Delay((int)(duration * 1000), cancellationToken: token);
            if (this == null || gameObject == null) return;
            transform.GetChild(0).gameObject.SetActive(false);

            if (!enemy) ObjectPoolManager.Instance.Return(this.gameObject, objName);
            else if (enemy) ObjectPoolManager.Instance.Return(this.gameObject, "EnemyMagicFloor");
        }
        catch (OperationCanceledException)
        {
            return;
        }
    }

    public void EnemyInit()
    {
        FloorEffect floor = GetComponent<FloorEffect>();
        floor.damage = damage;
        floor.delay = 1f;
        floor.enemy = true;
    }
}
