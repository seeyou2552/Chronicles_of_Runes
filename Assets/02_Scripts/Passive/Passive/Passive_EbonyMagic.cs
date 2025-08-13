using UnityEngine;

[CreateAssetMenu(menuName = "Passive/Passive_EbonyMagic")]
public class Passive_EbonyMagic : PassiveBase, IStackablePassive
{
    public float ebonyMagicBonus;
    public float ebonyMagicStartAtk;
    
    public override void PassiveEffect()
    {
        PlayerController.Instance.GetStatModifier().SetEbonyMagic(ebonyMagicBonus, ebonyMagicStartAtk);
    }

    public int Stack()
    {
        return PlayerController.Instance.GetStatModifier().GetEbonyMagicCount();
    }
}
