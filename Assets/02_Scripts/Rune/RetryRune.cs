using UnityEngine;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

[CreateAssetMenu(fileName = "RetryRune", menuName = "Rune/Retry")]
public class RetryRune : Rune
{
    public bool create = false;
    Action delayUse;


    public override void Apply(Skill skill)
    {
        skill.UseSkillAsyncSet(async () =>
        {
            if (!create)
            {
                await DelayUse(skill);
            }
        });
    }

    private async UniTask DelayUse(Skill skill)
    {
        create = true;
        await UniTask.Delay(TimeSpan.FromSeconds(skill.delay));
        skill.Use();
        if(skill.coolTime > 5) await UniTask.Delay(3000);
        else await UniTask.Delay(TimeSpan.FromSeconds(skill.coolTime) - TimeSpan.FromSeconds(0.2));
        create = false;
    }

    // 스킬 사용하면 먼저들어있던 스킬0번 사용함 현상 보존 -해-

    // public override void Apply(Skill skill)
    // {
    //     delayUse = () => DelayUse(skill);

    //     skill.UseSkillSet(() =>
    //     {
    //         delayUse?.Invoke();
    //     });
    // }

    // public async void DelayUse(Skill skill)
    // {
    //     await Task.Delay(500);
    //     if (!create) // 반복 생성 차단
    //     {
    //         create = true;
    //         skill.Use();
    //     }
    //     else
    //     {
    //         create = false;
    //     }
    // }

}

