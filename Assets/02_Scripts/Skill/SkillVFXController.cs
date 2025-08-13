using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SkillVFXController : MonoBehaviour
{
    [Header("Particle VFX")]
    [SerializeField] private GameObject normalParticl;
    [SerializeField] private GameObject fireParticl;
    [SerializeField] private GameObject waterParticl;
    [SerializeField] private GameObject iceParticl;
    [SerializeField] private GameObject electricParticl;
    [SerializeField] private GameObject darkParticl;
    [SerializeField] private GameObject lightParticl;

    [Header("Animation VFX")]
    [SerializeField] private GameObject animVFX;

    private GameObject vfx;

    public void GetVFX(ElementalType elemental, Quaternion angle, Vector3 hit)
    {
        switch (elemental)
        {
            case ElementalType.Normal:
                if (normalParticl == null) // particle 없으면 애니메이션 실행
                {
                    AnimationVFX(elemental, angle, hit).Forget();
                }
                else ParticleVFX(normalParticl, angle, hit).Forget();
                break;
            case ElementalType.Fire:
                if (fireParticl == null) // particle 없으면 애니메이션 실행
                {
                    AnimationVFX(elemental, angle, hit).Forget();
                }
                else ParticleVFX(fireParticl,angle, hit).Forget();
                break;
            case ElementalType.Water:
                if (waterParticl == null) // particle 없으면 애니메이션 실행
                {
                    AnimationVFX(elemental, angle, hit).Forget();
                }
                else ParticleVFX(waterParticl, angle, hit).Forget();
                break;
            case ElementalType.Ice:
                if (iceParticl == null) // particle 없으면 애니메이션 실행
                {
                    AnimationVFX(elemental, angle, hit).Forget();
                }
                else ParticleVFX(iceParticl, angle, hit).Forget();
                break;
            case ElementalType.Electric:
                if (electricParticl == null) // particle 없으면 애니메이션 실행
                {
                    AnimationVFX(elemental, angle, hit).Forget();
                }
                else ParticleVFX(electricParticl, angle, hit).Forget();
                break;
            case ElementalType.Dark:
                if (darkParticl == null) // particle 없으면 애니메이션 실행
                {
                    AnimationVFX(elemental, angle, hit).Forget();
                }
                else ParticleVFX(darkParticl, angle, hit).Forget();
                break;
            case ElementalType.Light:
                if (lightParticl == null) // particle 없으면 애니메이션 실행
                {
                    AnimationVFX(elemental, angle, hit).Forget();
                }
                else ParticleVFX(lightParticl, angle, hit).Forget();
                break;
        }
    }

    private async UniTask AnimationVFX(ElementalType elemental, Quaternion angle, Vector3 hit)
    {
        if (ObjectPoolManager.Instance.pools != null && ObjectPoolManager.Instance.pools.ContainsKey(animVFX.name))
        {

        }
        else  // 풀 없으면 생성
        {
            ObjectPoolManager.Instance.CreatePool(animVFX.name, animVFX, 1);
        }
        vfx = ObjectPoolManager.Instance.Get(animVFX.name);
        vfx.transform.position = hit;
        vfx.transform.rotation = angle; // 회전 적용

        var vfxComp = vfx.GetComponent<ReturnVFX>(); // 애니메이션 후 리턴 설정
        vfxComp.objName = animVFX.name;

        vfx.GetComponent<Animator>().SetInteger("Elemental", (int)elemental); // 애니메이션 실행
    }

    

    private async UniTask ParticleVFX(GameObject particleVFX, Quaternion angle,  Vector3 hit)
    {
        if (ObjectPoolManager.Instance.pools != null && ObjectPoolManager.Instance.pools.ContainsKey(particleVFX.name))
        {

        }
        else  // 풀 없으면 생성
        {
            ObjectPoolManager.Instance.CreatePool(particleVFX.name, particleVFX, 1);
        }
        vfx = ObjectPoolManager.Instance.Get(particleVFX.name);
        vfx.transform.position = hit;
        vfx.transform.rotation = angle;

        var vfxSystem = vfx.GetComponent<ParticleSystem>();
        var vfxModule = vfxSystem.main;

        var vfxComp = vfx.GetComponent<ReturnVFX>(); // 리턴 설정
        vfxComp.objName = particleVFX.name;

        vfxSystem.Play(); // 파티클 실행 
        vfxComp.ReturnParticleVFX().Forget(); // 리턴 비동기 실행
    }
}
