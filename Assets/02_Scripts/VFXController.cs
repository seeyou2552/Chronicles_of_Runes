using Cysharp.Threading.Tasks;
using Pathfinding;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class VFXController : MonoBehaviour , IPoolObject
{
    public bool isAttack = false;
    public float damage;
    public LayerMask targetMask;
    private string originalName;


    public float activeTimerOffset = 0.3f;
    [SerializeField] private float activeTimer;

    public float durationTimerOffset = 0.3f;
    [SerializeField] private float durationTimer;
    bool endDurationFlag = false;

    public bool attackFlag = false;
    public bool isColliderFlag = true;


    Collider2D cd;
    private void Awake()
    {
        cd = GetComponent<Collider2D>();
        cd.enabled = false;
    }

    private void Update()
    {
        activeTimer -= Time.deltaTime;


        if (isColliderFlag == true)
        {
            if (activeTimer < 0)
            {
                cd.enabled = true;
                attackFlag = true;
                isColliderFlag = false;
            }
        }

        else
        {
            durationTimer -= Time.deltaTime;
            if (endDurationFlag == false)
            {
                if (durationTimer < 0)
                {
                    endDurationFlag = true;
                    cd.enabled = false;
                }
            }
        }

    }
    public void Init(string name)
    {
        originalName = name;
    }
    
    private void OnEnable()
    {
        isColliderFlag = true;
        attackFlag = false;
        activeTimer = activeTimerOffset;
        Invoke(nameof(Return), 1.5f);
    }

    public void Return()
    {
        endDurationFlag = false;
        cd.enabled = false;
        ObjectPoolManager.Instance.Return(gameObject, originalName);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isAttack == false)
        {
            return;
        }

        else if (((1 << collision.gameObject.layer) & targetMask.value) != 0)
        {
            if (collision.TryGetComponent<IDamageable>(out var target))
            {
                target.OnDamage(damage);
                cd.enabled = false;
            }
            
        
        }
    }

}
