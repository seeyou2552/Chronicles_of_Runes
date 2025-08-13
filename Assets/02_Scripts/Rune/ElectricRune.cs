using UnityEngine;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "ElectricRune", menuName = "Rune/Electric")]
public class ElectricRune : Rune,IElementalRune
{
    public ElementalType GetElemental(){
        return ElementalType.Electric;
    }
    
    public override void Apply(Skill skill)
    {
        if (enemy)
        {
            skill.EnemySet(() =>
            {
                skill.enemySkillAction.elemental = ElementalType.Electric;
            });
            return;
        }

        skill.UseSkillSet(() =>
        {
            skill.skillAction.elemental = ElementalType.Electric;

            if (skill.devidePool != null)
            {
                skill.skillActionDevide.elemental = ElementalType.Electric;
            }
        });
    }
}