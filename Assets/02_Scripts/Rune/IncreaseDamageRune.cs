using UnityEngine;

public class IncreaseDamageRune : Rune
{
    [SerializeField] float additionalDamage;
    public override void Apply(Skill skill)
    {
        skill.StartSet(() =>
        {
            Logger.Log($"룬 데미지+{additionalDamage}");
            skill.damage += additionalDamage;
        });
        
        skill.EndSet(() =>
        {
            Logger.Log($"[룬] 데미지 -{additionalDamage}");
            skill.damage -= additionalDamage;
        });

    }
}