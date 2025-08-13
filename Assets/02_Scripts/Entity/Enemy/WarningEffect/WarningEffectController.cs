using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WarningEffectController : MonoBehaviour, IPoolObject
{
    [SerializeField] private LineRenderer lineRenderer;

    public List<IDamageable> damageablesInCollision = new();

    public Action<List<IDamageable>> OnDamage;
    public LayerMask targetMask;

    string originalName;

    public void Init(string name)
    {
        originalName = name;
    }

    private void Awake()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    private void OnEnable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        OnDamage = null;
    }

    public void OnSceneUnloaded(Scene scene)
    {
        ObjectPoolManager.Instance.Return(gameObject, originalName);
    }

    private void OnDisable()
    {
        damageablesInCollision.Clear();
        targetMask = default;

        // 라인렌더러나 콜라이더도 초기화 가능
        var lr = GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.positionCount = 0;
        }

        var col = GetComponent<EdgeCollider2D>();
        if (col != null)
        {
            col.points = new Vector2[0];
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & targetMask.value) != 0)
        {
            var damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageablesInCollision.Add(damageable);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageablesInCollision.Remove(damageable);
        }
    }
    /*
    private void OnDestroy()
    {   
        OnDamage?.Invoke(damageablesInCollision);
    }*/

    
    private void Start()
    {
        // 초기 알파값 0으로 설정
        SetLineAlpha(0f);

        // 알파값을 1까지 점차 증가
        
    }

    private void SetLineAlpha(float alpha)
    {
        Color start = lineRenderer.startColor;
        Color end = lineRenderer.endColor;

        start.a = alpha;
        end.a = alpha;

        lineRenderer.startColor = start;
        lineRenderer.endColor = end;
    }

    public Tween SetDuration(float duration, Action callBack)
    {
        Tween tween = DoTweenExtensions.TweenFloat(0f, 1f, duration, SetLineAlpha, () => 
        { callBack?.Invoke(); 
            OnDamage?.Invoke(damageablesInCollision); 
            ObjectPoolManager.Instance.Return(gameObject, originalName); 
        }).SetEase(Ease.InQuad).OnKill(() => 
        {
            OnDamage = null;
            FadeOut(0.3f);
        });

        return tween;
    }

    public void FadeOut(float duration)
    {
        //일단 임시
        Tween tween = DoTweenExtensions.TweenFloat(1f, 0f, duration, SetLineAlpha, () =>
        {
            ObjectPoolManager.Instance.Return(gameObject, originalName);
        }).SetEase(Ease.InQuad);
    }
}