using UnityEngine;

[CreateAssetMenu(menuName = "Passive/Passive_MoneyShot")]
public class Passive_MoneyShot : PassiveBase
{
    public float moneyShotBonus;
    
    public override void PassiveEffect()
    {
        PlayerController.Instance.GetStatModifier().SetMoneyShot(moneyShotBonus);
    }
}
