using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "SlowDebuff", menuName = "Debuff/Slow")]
public class SlowDebuff : ScriptableObject
{
    [Header("Debuff 설정")]
    public float slowPercentage; // 슬로우 효과
    public float duration; // 지속시간
    public int maxOverlap; // 최대 중첩

    [Header("VFX")]
    public GameObject debuffEffect;

    public async UniTask ApllyDebuff(Collider2D collider, int overlap, CancellationToken token)
    {
        if (ObjectPoolManager.Instance.pools != null && ObjectPoolManager.Instance.pools.ContainsKey(debuffEffect.name))
        {

        }
        else  // 풀 없으면 생성
        {
            ObjectPoolManager.Instance.CreatePool(debuffEffect.name, debuffEffect, 1);
        }
        var effect = ObjectPoolManager.Instance.Get(debuffEffect.name);
        effect.GetComponent<DebuffEffect>().targer = collider.gameObject;
        effect.transform.position = collider.transform.position;

        if (overlap > maxOverlap) overlap = maxOverlap; // 최대 중첩 처리

        float speed = collider.GetComponent<EnemyController>().aiPath.maxSpeed;

        CancellationTokenRegistration registration = token.Register(() => // 취소 시 복원 등록
        {
            collider.GetComponent<EnemyController>().aiPath.maxSpeed = speed;
            ObjectPoolManager.Instance.Return(effect, debuffEffect.name);
        });

        if (collider.GetComponent<EnemyController>().aiPath.maxSpeed - speed * (slowPercentage * PlayerController.Instance.GetStatModifier().CurseOfShackleBonus() * overlap * 0.01f) <= 0) // 0 이하는 0으로
        {
            collider.GetComponent<EnemyController>().aiPath.maxSpeed = 0;
        }
        else
        {
            collider.GetComponent<EnemyController>().aiPath.maxSpeed -= speed * (slowPercentage * PlayerController.Instance.GetStatModifier().CurseOfShackleBonus() * overlap * 0.01f);
        }
        await UniTask.Delay((int)(duration * PlayerController.Instance.GetStatModifier().StickyMagicBonus()) * 1000, cancellationToken: token); // 디버프 해제

        ObjectPoolManager.Instance.Return(effect, debuffEffect.name);
        if (collider.GetComponent<EnemyController>()?.isDead == true)
        {
            registration.Dispose();
            ObjectPoolManager.Instance.Return(effect, debuffEffect.name);
            return; // 죽었을 때 처리
        }

        collider.GetComponent<EnemyController>().aiPath.maxSpeed = speed;
        registration.Dispose();
    }
}
