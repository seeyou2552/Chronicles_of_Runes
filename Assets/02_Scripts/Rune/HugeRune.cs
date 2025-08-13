using UnityEngine;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DevideRune", menuName = "Rune/Huge")]
public class HugeRune : Rune
{
    public float scale = 2f;
    public override void Apply(Skill skill)
    {
        if (enemy)
        {
            skill.EnemySet(() =>
            {
                if (skill.enemyPool == null || skill.enemyPool.transform.localScale.x == (skill.obj.transform.localScale.x * scale)) return;
                skill.enemyPool.transform.localScale *= scale;
            });
            return;
        }

        skill.MiddleSet(() =>
        {
            if (skill.devidePool != null && skill.devidePool.transform.localScale == skill.pool.transform.localScale)
            {
                if (skill.devidePool.transform.localScale.x > skill.obj.transform.localScale.x) // 중복 적용 시 너무 커지지 않게 
                {
                    skill.devidePool.transform.localScale *= 1.2f;
                }
                else skill.devidePool.transform.localScale *= scale;
            }
            else
            {
                if (skill.pool.transform.localScale.x > skill.obj.transform.localScale.x) // 중복 적용 시 너무 커지지 않게 
                {
                    skill.pool.transform.localScale *= 1.2f;
                }
                else skill.pool.transform.localScale *= scale;
            } 
        });

    }
}