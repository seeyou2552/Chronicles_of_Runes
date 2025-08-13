using UnityEngine;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "DurationRune", menuName = "Rune/Duration")]
public class DurationRune : Rune
{
    public override void Apply(Skill skill)
    {
        skill.MiddleSet(() =>
        {
            if (skill.devidePool != null && skill.skillActionDevide.duration <= skill.skillAction.duration)
            {
                skill.skillActionDevide.duration += skill.duration;
            }
            else
            {
                skill.skillAction.duration += skill.duration; 
            } 
        });
    }
}

