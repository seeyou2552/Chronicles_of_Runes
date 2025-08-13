using UnityEngine;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "DevideRune", menuName = "Rune/Devide")]
public class DevideRune : Rune
{
    public bool create = false;

    public override void Apply(Skill skill)
    {
        skill.UseSkillSet(() =>
        {
            skill.DevideUse();
        });
    }
}

