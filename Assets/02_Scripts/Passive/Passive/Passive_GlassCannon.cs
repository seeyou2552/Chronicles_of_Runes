using UnityEngine;

[CreateAssetMenu(menuName = "Passive/Passive_GlassCannon")]
public class Passive_GlassCannon : PassiveBase
{
    public float damageIncome;
    public float damageOutcome;
    
    public override void PassiveEffect()
    {
        var statMod = PlayerController.Instance.GetStatModifier();
        
        statMod.DamageIncome *= damageIncome;
        statMod.DamageOutcome *= damageOutcome;
    }
}
