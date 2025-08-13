using UnityEngine;

[CreateAssetMenu(menuName = "Passive/Passive_CurseOfShackle")]
public class Passive_CurseOfShackle : PassiveBase
{
    public float curseOfShackleBonus;
    
    public override void PassiveEffect()
    {
        PlayerController.Instance.GetStatModifier().SetCurseOfShackle(curseOfShackleBonus);
    }
}
