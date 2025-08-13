using UnityEngine;

[CreateAssetMenu(menuName = "Passive/Passive_FastMotion")]
public class Passive_FasterTime : PassiveBase
{
    public float clockUp;
    
    public override void PassiveEffect()
    {
        float temp = Time.timeScale;
        Time.timeScale = temp + clockUp; 
        if (Time.timeScale >= 2f)
        {
            Time.timeScale = 2f;
        }
    }
}
