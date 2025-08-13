using UnityEngine;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "FireRune", menuName = "Rune/Fire")]
public class FireRune : Rune,IElementalRune
{
    public ElementalType GetElemental(){
        return ElementalType.Fire;
    }
    
    public override void Apply(Skill skill)
    {
        if (enemy)
        {
            skill.EnemySet(() =>
            {
                skill.enemySkillAction.elemental = ElementalType.Fire;
            });
            return;
        }

        skill.UseSkillSet(() =>
        {
            skill.skillAction.elemental = ElementalType.Fire;

            if (skill.devidePool != null)
            {
                skill.skillActionDevide.elemental = ElementalType.Fire;
            }
        });
    }
}