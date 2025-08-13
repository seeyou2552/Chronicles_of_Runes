using UnityEngine;

[CreateAssetMenu(menuName = "Passive/Passive_CurseOfDestruction")]
public class Passive_CurseOfDestruction : PassiveBase
{
    public float curseOfDestructionBonus;
    
    public override void PassiveEffect()
    {
        PlayerController.Instance.GetStatModifier().SetCurseOfDestruction(curseOfDestructionBonus);
    }
}
