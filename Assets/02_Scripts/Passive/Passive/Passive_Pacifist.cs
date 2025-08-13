using UnityEngine;

[CreateAssetMenu(menuName = "Passive/Passive_Pacifist")]
public class Passive_Pacifist : PassiveBase
{
    public float healBonus;
    public float damageOutcome;
    
    public override void PassiveEffect()
    {
        PlayerController.Instance.GetStatModifier().SetPacifist(healBonus, damageOutcome);
    }
}
