using UnityEngine;

[CreateAssetMenu(menuName = "Passive/Passive_Harmony")]
public class Passive_Harmony : PassiveBase
{
    public float harmonyBonus;
    
    public override void PassiveEffect()
    {
        PlayerController.Instance.GetStatModifier().SetHarmony(harmonyBonus);
    }
}
