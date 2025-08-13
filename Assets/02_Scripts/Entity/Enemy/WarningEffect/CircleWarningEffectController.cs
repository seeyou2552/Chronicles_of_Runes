using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class CircleWarningEffectController : MonoBehaviour, IPoolObject
{
    [SerializeField] private SpriteRenderer baseCircle;    // 연한 원
    [SerializeField] private SpriteRenderer warningCircle; // 진한 커지는 원

    private float maxRadius;
    private float duration;
    private Action onComplete;
    private string originalName;

    public List<IDamageable> damageablesInCollision = new();
    public Action<List<IDamageable>> OnDamage;

    public void Init(string name)
    {
        originalName = name;
    }

    private void Awake()
    {
        //SceneManager.sceneUnloaded = OnSceneUnloaded;
    }
    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        // 이벤트 지우기
        OnDamage = null;
    }

    public void OnSceneUnloaded(Scene scene)
    {
        ObjectPoolManager.Instance.Return(gameObject, originalName);
    }


    private void OnDisable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        damageablesInCollision.Clear();

    }

    public Tween Setup(float radius, float duration, LayerMask targetMask, Action onComplete = null)
    {
        this.maxRadius = radius;
        this.duration = duration;
        this.onComplete = onComplete;

        baseCircle.transform.localScale = Vector3.one * radius * 2f;
        baseCircle.color = new Color(0.7f, 0.6f, 0.9f, 0.2f); // 보라색 나중에 어디서 따로 바꾸는게 좋을지도

        warningCircle.transform.localScale = Vector3.zero;
        warningCircle.color = new Color(0.6f, 0.4f, 0.85f, 0.45f);

        gameObject.SetActive(true);

        Tween tween = warningCircle.transform.DOScale(Vector3.one * radius * 2f, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                onComplete?.Invoke();
                foreach (var temp in damageablesInCollision)
                {
                    Logger.Log(temp.ToString());
                }
                OnDamage?.Invoke(damageablesInCollision);

                ObjectPoolManager.Instance.Return(gameObject, originalName);
            })
            .OnKill(() =>
            {
                if (gameObject.activeSelf == true)
                {
                    FadeOut(0.7f);
                }

            });

        return tween;
    }

    private void FadeOut(float duration)
    {
        float warningStartAlpha = warningCircle.color.a;
        float baseStartAlpha = baseCircle.color.a;

        //warningCircle 페이드아웃
        DOTween.To(() => warningStartAlpha, alpha =>
        {
            var color = warningCircle.color;
            color.a = alpha;
            warningCircle.color = color;
        }, 0f, duration)
        .SetEase(Ease.OutCubic);

        //baseCircle 페이드아웃
        DOTween.To(() => baseStartAlpha, alpha =>
        {
            var color = baseCircle.color;
            color.a = alpha;
            baseCircle.color = color;
        }, 0f, duration)
        .SetEase(Ease.OutSine)
        .OnComplete(() =>
        {
            ObjectPoolManager.Instance.Return(gameObject, originalName);
        });
    }

    public IEnumerator DelayOneFrame(Action callback)
    {
        yield return null;
        callback?.Invoke();
    }
}
