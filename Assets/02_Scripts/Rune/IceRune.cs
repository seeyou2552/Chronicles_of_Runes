using UnityEngine;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "IceRune", menuName = "Rune/Ice")]
public class IceRune : Rune,IElementalRune
{
    public ElementalType GetElemental(){
        return ElementalType.Ice;
    }
    
    public override void Apply(Skill skill)
    {
        if (enemy)
        {
            skill.EnemySet(() =>
            {
                skill.enemySkillAction.elemental = ElementalType.Ice;
            });
            return;
        }

        skill.UseSkillSet(() =>
        {
            skill.skillAction.elemental = ElementalType.Ice;

            if (skill.devidePool != null)
            {
                skill.skillActionDevide.elemental = ElementalType.Ice;
            }
        });
    }
}