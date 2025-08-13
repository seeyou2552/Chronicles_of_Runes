using UnityEngine;
using DG.Tweening;
using System;

public class EnemySpawnEffect : MonoBehaviour
{
    public Transform pillar; // 기둥 오브젝트 (Scale.y를 0에서 1까지)
    public GameObject enemyPrefab;
    public float growTime = 0.3f;
    public float stayTime = 0.2f;
    public float shrinkTime = 0.3f;

    private Vector3 pillarScale;

    void Awake()
    {
        if (pillar == null) pillar = transform;
        pillarScale = pillar.localScale;
        pillar.localScale = new Vector3(pillarScale.x, 0f, pillarScale.z);
    }

    public void PlaySpawn(Vector3 spawnPos, Func<GameObject> callBack, Action<GameObject> endCallBack)
    {
        Sequence seq = DOTween.Sequence();

        GameObject spawnedEnemy = null;

        seq.Append(pillar.DOScaleY(pillarScale.y, growTime).SetEase(Ease.OutQuad));

        seq.AppendInterval(stayTime);
        seq.AppendCallback(() =>
        {
            // 몬스터 생성 (반환값 받아오기)
            spawnedEnemy = callBack?.Invoke();
        });

        seq.Append(pillar.DOScaleY(0f, shrinkTime).SetEase(Ease.InQuad))
           .OnComplete(() =>
           {
               endCallBack?.Invoke(spawnedEnemy);
               Destroy(gameObject);
           });
    }

    public void CircleSpawn(Vector3 spawnPos, Func<GameObject> callBack, Action<GameObject> endCallBack)
    {

        Sequence seq = DOTween.Sequence();

        GameObject spawnedEnemy = null;

        seq.Append(pillar.DOScale(pillarScale.y, growTime).SetEase(Ease.OutQuad)).OnComplete(() =>
        {
            
        }); ;

        seq.AppendInterval(stayTime);
        seq.AppendCallback(() =>
        {
            spawnedEnemy = callBack?.Invoke();
            Logger.Log("뭔데ㅋㅋ");
            
        });

        seq.Append(pillar.DOScale(0f, shrinkTime).SetEase(Ease.InQuad))
           .OnComplete(() =>
           {
               endCallBack?.Invoke(spawnedEnemy);
               ObjectPoolManager.Instance.Return(gameObject, "Spawner");
               Destroy(gameObject);
           });
    }

}
