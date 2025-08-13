using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



public class Heal : MonoBehaviour, SkillAction
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

    public void Init(Vector3 pos)
    {
        onStart?.Invoke();

        onMiddle?.Invoke();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !enemy)
        {
            soundController.StartSound(objName); // 시작 사운드
            vfxController.GetVFX(elemental, transform.rotation, gameObject.transform.position);
            onHit?.Invoke();
            onEnd?.Invoke();
            PlayerController.Instance.statHandler.Heal(damage);
            ObjectPoolManager.Instance.Return(this.gameObject, objName);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && enemy)
        {
            ObjectPoolManager.Instance.Return(this.gameObject, "EnemyHeal");
        }
    }

    public void EnemyInit()
    {

    }
}