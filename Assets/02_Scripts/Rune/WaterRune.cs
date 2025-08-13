using UnityEngine;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "WaterRune", menuName = "Rune/Water")]
public class WaterRune : Rune,IElementalRune
{
    public ElementalType GetElemental(){
        return ElementalType.Water;
    }
    
    public override void Apply(Skill skill)
    {
        if (enemy)
        {
            skill.EnemySet(() =>
            {
                skill.enemySkillAction.elemental = ElementalType.Water;
            });
            return;
        }

        skill.UseSkillSet(() =>
        {
            skill.skillAction.elemental = ElementalType.Water;

            if (skill.devidePool != null)
            {
                skill.skillActionDevide.elemental = ElementalType.Water;
            }
        });
    }
}