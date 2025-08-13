using UnityEngine;

[CreateAssetMenu(menuName = "Passive/Passive_ElementalOverCharge")]
public class Passive_ElementalOverCharge : PassiveBase
{
    public float elementalOverChargeBonus;
    
    public override void PassiveEffect()
    {
        PlayerController.Instance.GetStatModifier().SetElementalOverCharge(elementalOverChargeBonus);
    }
}