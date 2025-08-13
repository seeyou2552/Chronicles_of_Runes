using UnityEngine;

[CreateAssetMenu(menuName = "Passive/Passive_SwiftMove")]
public class Passive_SwiftMove : PassiveBase
{
    [SerializeField] private float moveSpeed;
    public float MoveSpeed => moveSpeed;
    
    public override void PassiveEffect()
    {
        PlayerController.Instance.GetStatModifier().MoveSpeed += moveSpeed;
    }
}
