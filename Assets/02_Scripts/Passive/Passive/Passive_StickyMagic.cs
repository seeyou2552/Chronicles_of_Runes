using UnityEngine;

[CreateAssetMenu(menuName = "Passive/Passive_StickyMagic")]
public class Passive_StickyMagic : PassiveBase
{
    public float stickyMagicBonus;
    
    public override void PassiveEffect()
    {
        PlayerController.Instance.GetStatModifier().SetStickyMagicBonus(stickyMagicBonus);
    }
}
