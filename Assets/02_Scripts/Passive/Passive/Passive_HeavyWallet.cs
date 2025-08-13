using UnityEngine;

[CreateAssetMenu(menuName = "Passive/Passive_HeavyWallet")]
public class Passive_HeavyWallet : PassiveBase, IStackablePassive
{
    public float hpBonus;

    public override void PassiveEffect()
    {
        PlayerController.Instance.GetStatModifier().SetOnOffHeavyWallet(hpBonus);
    }
    
    public int Stack()
    {
        return PlayerController.Instance.GetStatModifier().GetHeavyWalletStack();
    }
}
