using UnityEngine;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "LightRune", menuName = "Rune/Light")]
public class LightRune : Rune,IElementalRune
{
    public ElementalType GetElemental(){
        return ElementalType.Light;
    }
    
    public override void Apply(Skill skill)
    {
        if (enemy)
        {
            skill.EnemySet(() =>
            {
                skill.enemySkillAction.elemental = ElementalType.Light;
            });
            return;
        }
        
        skill.UseSkillSet(() =>
        {
            skill.skillAction.elemental = ElementalType.Light;

            if (skill.devidePool != null)
            {
                skill.skillActionDevide.elemental = ElementalType.Light;
            }
        });
    }
}