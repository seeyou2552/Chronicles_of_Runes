using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

public class Burst : MonoBehaviour, SkillAction
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

    Vector3 casterPos; // 시전자 위치

    [Header("Controller")]
    [SerializeField] private SkillSoundController soundController;
    [SerializeField] private ParticleColorChange colorController;
    [SerializeField] private ParticleSystem particle;

    public void Init(Vector3 pos)
    {
        onStart?.Invoke();
        caster = PlayerController.Instance.gameObject;
        casterPos = PlayerController.Instance.transform.position;

        Bursting(pos);
        soundController.StartSound(objName);
    }

    private void Bursting(Vector3 pos)
    {
        onMiddle?.Invoke();
        colorController.SwitchColor(elemental);
        particle.Play();
        Vector3 dir = (pos - casterPos).normalized;

        RaycastHit2D hit = Physics2D.Raycast(casterPos, dir, speed, LayerMask.GetMask("Obstacle")); // 벽에 닿을 경우 벽까지 이동
        if (hit.collider != null)
        {

            Vector3 hitPoint = hit.point;
            hitPoint.z = 0f;

            Vector3 tempPos = (hitPoint - PlayerController.Instance.transform.position).normalized;

            float offset = 1.0f;
            caster.transform.position = hitPoint - tempPos * offset;
        }
        else caster.transform.position += dir * speed;

        transform.position = caster.transform.position;

        StartCoroutine(Return());
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && !enemy) // Enemy 적중
        {
            onHit?.Invoke();
            collision.GetComponent<IDamageable>().OnDamage(damage);
            if (collision.TryGetComponent<DebuffController>(out DebuffController debuffController))
            {
                debuffController.TakeDebuff(collision, elemental);
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && enemy) // Player 적중
        {
            collision.GetComponent<IDamageable>().OnDamage(damage);
        }
    }

    public IEnumerator Return()
    {
        yield return YieldCache.WaitForSeconds(GetComponent<ParticleSystem>().main.duration);
        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        onEnd?.Invoke();
        ObjectPoolManager.Instance.Return(this.gameObject, objName);
    }

    public void EnemyInit()
    {
        casterPos = caster.transform.position;
        // Teleporting();
    }


}
