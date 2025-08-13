using UnityEngine;

[CreateAssetMenu(menuName = "Passive/Passive_DefensiveMovement")]
public class Passive_DefensiveMovement : PassiveBase
{
    [SerializeField] private float damageIncome;
    [SerializeField] private float damageOutcome;
    
    
    public override void PassiveEffect()
    {
        var statMod = PlayerController.Instance.GetStatModifier();
        
        statMod.DamageIncome = Mathf.Max(0.2f, statMod.DamageIncome * damageIncome);
        statMod.DamageOutcome = Mathf.Max(0.2f, statMod.DamageOutcome * damageOutcome);
    }
}
