using System;
using System.Threading;
using UnityEngine;

public class MagicRevolve : MonoBehaviour, SkillAction
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

    public GameObject orbiterObj;
    public int count = 3;

    public float radiusX = 3f;
    public float radiusY = 2f;
    public float rotationsPerSecond = 1f;
    private GameObject[] spheres;

    [Header("Controller")]
    [SerializeField] private SkillSoundController soundController;
    [SerializeField] private SkillVFXController vfxController;


    private CancellationTokenSource cts;
    private int remaining; 

    public void Init(Vector3 pos)
    {
        onStart?.Invoke();
        onMiddle?.Invoke();
        
        caster = PlayerController.Instance.gameObject;
        remaining = count;
        CreateOrbiter();
        soundController.StartSound(objName); // 시작 사운드
    }

    void FixedUpdate()
    {
        this.transform.position = PlayerController.Instance.transform.position;
    }

    private void CreateOrbiter()
    {
        if (ObjectPoolManager.Instance.pools != null && ObjectPoolManager.Instance.pools.ContainsKey(orbiterObj.name))
        {

        }
        else  // 풀 없으면 생성
        {
            ObjectPoolManager.Instance.CreatePool(orbiterObj.name, orbiterObj, 3);
        }

        spheres = new GameObject[count];

        for (int i = 0; i < count; i++)
        {
            spheres[i] = ObjectPoolManager.Instance.Get(orbiterObj.name);

            float offset = (2 * Mathf.PI / count) * i;

            Orbiter orbit = spheres[i].GetComponent<Orbiter>(); // orbiter에 설정 주입
            orbit.soundController = soundController;
            orbit.vfxController = vfxController;
            orbit.radiusX = radiusX;
            orbit.radiusY = radiusY;
            orbit.speed = rotationsPerSecond;
            orbit.angleOffset = offset;
            orbit.onHit = onHit;
            orbit.onEnd = onEnd;
            orbit.objName = orbiterObj.name;
            orbit.duration = duration;
            orbit.through = through;
            orbit.elemental = elemental;
            orbit.homing = homing;
            orbit.damage = damage;
            orbit.transform.localScale = transform.localScale;
            orbit.transform.position = caster.transform.position;
            orbit.caster = caster;
            
            orbit.onEnd = () =>
            {
                onEnd?.Invoke();
                if (--remaining == 0)
                {
                    if (!enemy)
                    {
                        ObjectPoolManager.Instance.Return(this.gameObject, objName);
                    }
                    else if (enemy)
                    {
                        ObjectPoolManager.Instance.Return(this.gameObject, "EnemyMagicRevolve");
                    }
                }
            };
            orbit.Init();
        }
    }
    public void EnemyInit()
    {
        CreateOrbiter();
    }
}
