using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "SturnDebuff", menuName = "Debuff/Sturn")]
public class SturnDebuff : ScriptableObject
{
    [Header("Debuff 설정")]
    public float ignoring; // 스턴 받지 않는 시간
    
    public async UniTask ApllyDebuff(Collider2D collider, float duration)
    {
        if (collider.TryGetComponent<PlayerController>(out PlayerController player))
        {
            player.canControl = false;
        }
        else
        {
            collider.gameObject.GetComponent<EnemyController>().canControl = false;
        }

        await UniTask.Delay((int)duration * 1000);

        if (player != null)
        {
            player.canControl = true;
        }
        else
        {
            collider.gameObject.GetComponent<EnemyController>().canControl = true;
        }

        await UniTask.Delay((int)ignoring * 1000); // 디버프 안받는 용
    }
}
