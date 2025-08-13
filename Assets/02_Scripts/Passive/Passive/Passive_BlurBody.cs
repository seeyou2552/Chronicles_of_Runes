using UnityEngine;

[CreateAssetMenu(menuName = "Passive/Passive_BlurBody")]
public class Passive_BlurBody : PassiveBase
{
    public float damageIncome;
    
    public override void PassiveEffect()
    {
        var statMod = PlayerController.Instance.GetStatModifier();
        statMod.DamageIncome *= damageIncome;
        
        if (statMod.DamageIncome <= 0.2f)
        {
            statMod.DamageIncome = 0.2f;
        }
    }
}
