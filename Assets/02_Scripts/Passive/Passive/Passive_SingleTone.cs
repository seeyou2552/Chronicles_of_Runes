using UnityEngine;

[CreateAssetMenu(menuName = "Passive/Passive_SingleTone")]
public class Passive_SingleTone : PassiveBase
{
    public float singleToneBonus;
    
    public override void PassiveEffect()
    {
        PlayerController.Instance.GetStatModifier().SetSingleTone(singleToneBonus);
    }
}
