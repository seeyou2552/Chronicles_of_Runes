using UnityEngine;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

public interface IElementalRune
{
    ElementalType GetElemental();
}


[CreateAssetMenu(fileName = "DarkRune", menuName = "Rune/Dark")]
public class DarkRune : Rune, IElementalRune
{
    public ElementalType GetElemental(){
        return ElementalType.Dark;
    }
    
    public override void Apply(Skill skill)
    {
        if (enemy)
        {
            skill.EnemySet(() =>
            {
                skill.enemySkillAction.elemental = ElementalType.Dark;
            });
            return;
        }

        skill.UseSkillSet(() =>
        {
            skill.skillAction.elemental = ElementalType.Dark;

            if (skill.devidePool != null)
            {
                skill.skillActionDevide.elemental = ElementalType.Dark;
            }

        });
    }
}