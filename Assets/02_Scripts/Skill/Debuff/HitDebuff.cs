using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "HitDebuff", menuName = "Debuff/Hit")]
public class HitDebuff : ScriptableObject
{
    [Header("Debuff 설정")]
    public float damage; // 데미지
    public float duration; // 지속시간
    public int maxOverlap; // 최대 중첩
    public float delay; // 데미지 딜레이 (초)

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

        CancellationTokenRegistration registration = token.Register(() => // 취소 시 복원 등록
        {
            ObjectPoolManager.Instance.Return(effect, debuffEffect.name);
        });

        float currentTime = 0;
        while (currentTime < duration * PlayerController.Instance.GetStatModifier().StickyMagicBonus())
        {
            await UniTask.Delay((int)delay * 1000, cancellationToken: token);
            if (collider.GetComponent<EnemyController>()?.isDead == true)
            {
                ObjectPoolManager.Instance.Return(effect, debuffEffect.name);
                return; // 죽었을 때 처리
            }
            collider.GetComponent<IDamageable>()?.OnDamage(damage * PlayerController.Instance.GetStatModifier().CurseOfDestructionBonus() * overlap);
            currentTime += delay;
        }

        ObjectPoolManager.Instance.Return(effect, debuffEffect.name);
    }
}
