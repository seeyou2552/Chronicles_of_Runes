using System.Collections;
using UnityEngine;

public class GasCloudGimmick : MonoBehaviour, IRoomGimmick
{
    [Header("독가스 데미지")]
    [SerializeField] float damagePerTick = 4f;

    [Header("데미지 적용 간격 (초)")]
    [SerializeField] float tickInterval = 1f;

    [Header("이펙트")]
    [SerializeField] ParticleSystem gasVFX;

    Coroutine tickRoutine;

    public void StartGimmick()
    {
        StaticNoticeManager.Instance.ShowSideNotice("독가스가 퍼지기 시작합니다", 4f);
        // VFX 켜기
        // 이미 돌고 있지 않다면 코루틴 시작
        if (tickRoutine == null)
            tickRoutine = StartCoroutine(ApplyTickDamage());
    }
    public void StopGimmick()
    {
        // VFX 끄기
        // 코루틴 정지
        if (tickRoutine != null)
        {
            StopCoroutine(tickRoutine);
            tickRoutine = null;
        }
    }

    IEnumerator ApplyTickDamage()
    {
        // 매 tick마다 PlayerController.Instance 에 데미지 호출
        while (true)
        {
            yield return YieldCache.WaitForSeconds(tickInterval);
            var player = PlayerController.Instance as IDamageable;
            if (player != null)
                player.OnDamage(damagePerTick);
        }
    }
}
