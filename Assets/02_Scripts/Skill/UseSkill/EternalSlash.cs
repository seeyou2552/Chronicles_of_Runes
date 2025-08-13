using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Cysharp.Threading.Tasks;
using System.Threading;

public class EternalSlash : MonoBehaviour, SkillAction
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

    [SerializeField] private string slashName; // slash 돌려쓰기 용
    [Header("Controller")]
    [SerializeField] private SkillSoundController soundController;
    [SerializeField] private SkillVFXController vfxController;


    
    private Vector3 tempScale;
    private Vector3 randomOffset;
    private Quaternion randomRotation;


    public void Init(Vector3 pos)
    {
        Debug.Log("썡썽");
        onStart?.Invoke();
        onMiddle?.Invoke();

        Vector3 dir = (pos - PlayerController.Instance.transform.position).normalized;

        transform.position = PlayerController.Instance.transform.position + dir * (transform.localScale.x / 2);

        FloorEffect floor = GetComponent<FloorEffect>(); // floor 설정
        floor.damage = damage;
        floor.delay = 0.1f;
        floor.elemental = elemental;
        // floor.soundController = soundController;
        // floor.vfxController = vfxController;
        floor.objName = objName;
    

        tempScale = transform.localScale;

        StartCoroutine(VFXPlay());
        StartCoroutine(Return());
    }

    private IEnumerator VFXPlay()
    {
        while (true)
        {
            randomOffset = new Vector3(
                    UnityEngine.Random.Range(-tempScale.x / 2, tempScale.x / 2),  // X축 랜덤
                    UnityEngine.Random.Range(-tempScale.y / 2, tempScale.y / 2),  // Y축 랜덤
                    0f
                );
            randomRotation = Quaternion.Euler(
                    0,
                    10,
                    UnityEngine.Random.Range(0, 360) // Z축 랜덤
                );
            vfxController.GetVFX(elemental, randomRotation, gameObject.transform.position + randomOffset);
            soundController.StartSound(elemental, slashName);

            yield return YieldCache.WaitForSeconds(0.1f);
        }
    }

    private IEnumerator Return()
    {
        PlayerController.Instance.canControl = false;
        yield return YieldCache.WaitForSeconds(duration);
        PlayerController.Instance.canControl = true;
        onEnd?.Invoke();
        ObjectPoolManager.Instance.Return(this.gameObject, objName);
    }

    public void EnemyInit()
    {

    }

}
