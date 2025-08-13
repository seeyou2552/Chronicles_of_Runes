using UnityEngine;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "ThroughRune", menuName = "Rune/Through")]
public class ThroughRune : Rune
{
    public bool create = false;
    Action delayUse;


    public override void Apply(Skill skill)
    {
        skill.MiddleSet(() =>
        {
            skill.skillAction.through = true;

            if (skill.skillActionDevide == null) return;
            skill.skillActionDevide.through = true;
        });
    }




}

