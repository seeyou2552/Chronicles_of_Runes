using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnVFX : MonoBehaviour
{
    public string objName;

    public void ReturnAnimationVFX()
    {
        gameObject.GetComponent<Animator>().SetInteger("Elemental", -1);
        ObjectPoolManager.Instance.Return(this.gameObject, objName);
    }

    public async UniTask ReturnParticleVFX()
    {
        await UniTask.Delay((int)GetComponent<ParticleSystem>().main.duration * 1000); // 재생 시간 만큼 대기
        gameObject.GetComponent<ParticleSystem>().Stop();
        ObjectPoolManager.Instance.Return(gameObject, objName);
    }
}
