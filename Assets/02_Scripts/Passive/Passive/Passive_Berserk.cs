using UnityEngine;

[CreateAssetMenu(menuName = "Passive/Passive_Berserk")]
public class Passive_Berserk : PassiveBase
{
    [SerializeField] private float damageIncome;
    [SerializeField] private float damageOutcome;
    
    
    public override void PassiveEffect()
    {
        var statMod = PlayerController.Instance.GetStatModifier();
        
        statMod.DamageIncome = statMod.DamageIncome * damageIncome;
        statMod.DamageOutcome = statMod.DamageOutcome * damageOutcome;
    }
}
