using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageFontManager : Singleton<DamageFontManager>
{
    public GameObject damageTextPrefab;
    public Canvas worldCanvas; //월드 스페이스 캔버스


    public void ShowDamage(Vector3 worldPosition, float value)
    {
        if (damageTextPrefab == null || worldCanvas == null)
        {
            return;
        }

        //풀링 해줘야하는데...
        GameObject obj = Instantiate(damageTextPrefab, worldCanvas.transform);
        obj.transform.position = worldPosition + Vector3.up * 1.5f;
        obj.GetComponent<DamageText>().SetText(value);
    }
}
