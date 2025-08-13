using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices.WindowsRuntime;

public class MagicSlash : MonoBehaviour, SkillAction
{
    public Action onStart { get; set; }
    public Action onMiddle { get; set; }
    public Action onHit { get; set; }
    public Action onEnd { get; set; }
    public Action onEnemy { get; set; }
    public bool through { get; set; }
    public bool homing { get; set; }
    public float duration { get; set; }
    public ElementalType elemental { get; set; }
    public float damage { get; set; }
    public bool enemy { get; set; }
    public GameObject caster { get; set; }
    public string objName { get; set; }
    public float speed { get; set; }

    [Header("Controller")]
    [SerializeField] private SkillSoundController soundController;
    [SerializeField] private SkillVFXController vfxController;

    [Header("ParticleSkill")]
    [SerializeField] private GameObject normalParticle;
    [SerializeField] private GameObject fireParticle;
    [SerializeField] private GameObject waterParticle;
    [SerializeField] private GameObject iceParticle;
    [SerializeField] private GameObject electricParticle;
    [SerializeField] private GameObject darkParticle;
    [SerializeField] private GameObject lightParticle;

    private GameObject obj = null; // elemental vfx 기억


    public void Init(Vector3 pos)
    {
        Vector3 dir = pos - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        onStart?.Invoke();
        onMiddle?.Invoke();

        var slashVFX = GetElementalVFX(); // vfx 생성 및 주입
        slashVFX.transform.rotation = Quaternion.Euler(angle-70, -90, 90);
        slashVFX.transform.position = transform.position;
        slashVFX.transform.localScale = transform.localScale;
        slashVFX.GetComponent<ParticleSystem>().Play();
        soundController.StartSound(elemental, objName); // 시작 
        
        StartCoroutine(Return(slashVFX));
    }

    private IEnumerator Return(GameObject returnObj)
    {
        yield return YieldCache.WaitForSeconds(0.8f);

        onEnd?.Invoke();
        ObjectPoolManager.Instance.Return(returnObj, obj.name); // 생성한 vfx 먼저 return
        ObjectPoolManager.Instance.Return(this.gameObject, objName);
    }

    private GameObject GetElementalVFX()
    {

        switch (elemental)
        {
            case ElementalType.Normal:
                obj = normalParticle;
                break;
            case ElementalType.Fire:
                obj = fireParticle;
                break;
            case ElementalType.Water:
                obj = waterParticle;
                break;
            case ElementalType.Ice:
                obj = iceParticle;
                break;
            case ElementalType.Electric:
                obj = electricParticle;
                break;
            case ElementalType.Dark:
                obj = darkParticle;
                break;
            case ElementalType.Light:
                obj = lightParticle; ;
                break;
        }

        if (ObjectPoolManager.Instance.pools != null && ObjectPoolManager.Instance.pools.ContainsKey(obj.name))
        {

        }
        else  // 풀 없으면 생성
        {
            ObjectPoolManager.Instance.CreatePool(obj.name, obj, 1);
        }

        return ObjectPoolManager.Instance.Get(obj.name);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && !enemy) // Enemy 적중
        {
            onHit?.Invoke();
            soundController.HitSound(elemental, objName); // Hit Sound
            // vfxController.GetVFX(elemental, transform.rotation, gameObject.transform.position);
            collision.GetComponent<IDamageable>().OnDamage(damage);
            if (collision.TryGetComponent<DebuffController>(out DebuffController debuffController))
            {
                debuffController.TakeDebuff(collision, elemental);
            }

        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && enemy) // Player 적중
        {

        }
    }


    public void EnemyInit()
    {

    }

}
