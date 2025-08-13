using UnityEngine;
using System;
using System.Collections;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "flowRune", menuName = "Rune/Flow")]
public class flowRune : Rune
{
    public override void Apply(Skill skill)
    {
        float originDelay = skill.delay;
        
        skill.UseSkillSet(() =>
        {
            skill.delay = 0f;
            CoroutineRunner.Instance.RunCoroutine(RestorenDelay(skill, originDelay, skill.duration));
        });
        
        skill.StartSet(() =>
        {
            PlayerController.Instance.canControl = true;
        });

        skill.EndSet(() =>
        {
            skill.delay = originDelay;
        });
    }
    
    private IEnumerator RestorenDelay(Skill skill, float origin, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (skill != null)
        {
            // 단순 원복: 다른 버프 시스템과 섞인다면 "스택 기반" 설계를 고려
            skill.delay = origin;
        }
    }
}