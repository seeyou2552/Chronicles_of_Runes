using UnityEngine;

public class SoundManagerTest : MonoBehaviour
{
    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Logger.Log("소리 출력");
            SoundManager.PlaySFX("SFX_Vefects_Anime_Stylized_Arrow_Shot_Cast");
        }
            
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SoundManager.StopAllSFX();
        }
        
    }
}