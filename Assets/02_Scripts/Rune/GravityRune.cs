using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GravityRune", menuName = "Rune/GravityRune")]
public class GravityRune : Rune
{
    [SerializeField] private float radius; // 8
    [SerializeField] private float pullForce; // 100
    [SerializeField] private float duration; // 1
    [SerializeField] private GameObject gravityPrefeb;

    public override void Apply(Skill skill)
    {
        // 풀 존재 여부 확인 후 없으면 생성
        if (!ObjectPoolManager.Instance.pools.ContainsKey(gravityPrefeb.name))
        {
            ObjectPoolManager.Instance.CreatePool(gravityPrefeb.name, gravityPrefeb, 5);
        }

        skill.EndSet(() =>
        {
            Vector3 skillPosition = skill.pool.transform.position;

            GameObject gravityPullObject = ObjectPoolManager.Instance.Get(gravityPrefeb.name);
            if (gravityPullObject == null)
            {
                Logger.Log($"[GravityRune] '{gravityPrefeb.name}' 풀에서 오브젝트를 가져오지 못했습니다.");
                return;
            }

            gravityPullObject.transform.position = skillPosition;

            if (gravityPullObject.TryGetComponent<GravityPullEffect>(out var effect))
            {
                CoroutineRunner.Instance.RunCoroutine(effect.PullRoutine(skillPosition, radius, pullForce, duration, () =>
                {
                    ObjectPoolManager.Instance.Return(gravityPullObject, gravityPrefeb.name);
                }));
            }
        });
    }

}