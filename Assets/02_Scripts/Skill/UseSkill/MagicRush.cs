using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

public class MagicRush : MonoBehaviour, SkillAction
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

    Vector3 smoothedDirection;

    

    [Header("Controller")]
    [SerializeField] private SkillSoundController soundController;
    [SerializeField] private SkillVFXController vfxController;


    private CancellationTokenSource cts;

    public void Init(Vector3 pos)
    {
        onMiddle?.Invoke();
        transform.position = PlayerController.Instance.transform.position;

        GetComponent<Animator>().SetInteger("Elemental", (int)elemental);

        Vector3 playerPos = PlayerController.Instance.transform.position;

        // 마우스 방향 벡터
        Vector3 direction = (pos - playerPos).normalized;

        smoothedDirection = direction;

        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        Rush(cts.Token).Forget();
        // soundController.StartSound(elemental, objName); // 시작 사운드
    }

    public async UniTaskVoid Rush(CancellationToken token)
    {
        float timer = 0f;
        PlayerController.Instance.isInvincible = true;
        while (timer < duration)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;

            Vector3 playerPos = PlayerController.Instance.transform.position;
            Vector3 targetDirection = (mouseWorldPos - playerPos).normalized;

            smoothedDirection = Vector3.Lerp(smoothedDirection, targetDirection, Time.deltaTime * 2f);

            RaycastHit2D hit = Physics2D.Raycast(playerPos, smoothedDirection, 1.5f, LayerMask.GetMask("Obstacle"));
            if (hit.collider != null)
            {
                ObjectPoolManager.Instance.Return(this.gameObject, objName);
                SoundManager.StopAllSFX(); // 사운드 종료
                cts?.Cancel();
            }
            else
            {
                timer += Time.deltaTime;
                PlayerController.Instance.transform.position += smoothedDirection * speed * Time.deltaTime / 2;
                transform.position = PlayerController.Instance.transform.position;
            }
            await UniTask.Yield(token);
        }
        SoundManager.StopAllSFX(); // 사운드 종료
        onEnd?.Invoke();
        PlayerController.Instance.isInvincible = false;
        ObjectPoolManager.Instance.Return(this.gameObject, objName);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            soundController.HitSound(objName); // Hit Sound
            vfxController.GetVFX(elemental, transform.rotation, gameObject.transform.position);
            collision.GetComponent<DebuffController>().TakeDebuff(collision, elemental);
            collision.GetComponent<IDamageable>().OnDamage(damage);
            if (collision.TryGetComponent<DebuffController>(out DebuffController debuffController))
            {
                debuffController.TakeDebuff(collision, elemental);
            }
        }
    }

    public void EnemyInit()
    {

    }
}
