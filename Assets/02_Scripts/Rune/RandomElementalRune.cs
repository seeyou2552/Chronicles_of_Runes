using UnityEngine;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

[CreateAssetMenu(fileName = "RandomElementalRune", menuName = "Rune/RandomElemental")]
public class RandomElementalRune : Rune
{
    public bool divide;
    public override void Apply(Skill skill)
    {
        var elementalRange = Enum.GetValues(typeof(ElementalType));

        if (enemy)
        {
            skill.EnemySet(() =>
            {
                ElementalType type = (ElementalType)elementalRange.GetValue(UnityEngine.Random.Range(0, elementalRange.Length));
                skill.enemySkillAction.elemental = type;
            });
            return;
        }

        skill.UseSkillSet(() =>
        {
            ElementalType type = (ElementalType)elementalRange.GetValue(UnityEngine.Random.Range(0, elementalRange.Length));
            skill.skillAction.elemental = type;
            if (skill.skillActionDevide != null) skill.skillActionDevide.elemental = type;
        });
    }
}