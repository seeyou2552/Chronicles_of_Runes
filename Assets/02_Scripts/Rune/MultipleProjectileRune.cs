using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MultipleProjectileRune", menuName = "Rune/MultipleProjectile")]
public class MultipleProjectileRune : Rune
{
    public GameObject projectileObj;
    public string projectileName = "MultipleProjectile";

    GameObject pool;

    public override void Apply(Skill skill)
    {
        skill.MiddleSet(() =>
        {
            if (ObjectPoolManager.Instance.pools != null && ObjectPoolManager.Instance.pools.ContainsKey(projectileName))
            {

            }
            else  // 풀 없으면 생성
            {
                ObjectPoolManager.Instance.CreatePool(projectileName, projectileObj, 1);
            }
            pool = ObjectPoolManager.Instance.Get(projectileName);
            pool.transform.position = PlayerController.Instance.transform.position;
        });
    }
}