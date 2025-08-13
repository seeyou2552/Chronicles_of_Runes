using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class DebuffController : MonoBehaviour
{
    public Dictionary<ElementalType, int> debuffDict = new Dictionary<ElementalType, int>(); // Key : elemental  Value : 중첩 수
    private Dictionary<ElementalType, CancellationTokenSource> ctsDict = new Dictionary<ElementalType, CancellationTokenSource>(); // cancel 토큰 보관용
    private bool sturnIgnoring; // 스턴 무적
    public DebuffData debuffData;

    public void TakeDebuff(Collider2D collider, ElementalType elemental)
    {
        var token = TokenCheck(elemental);
        debuffDict[elemental] += 1;

        switch (elemental)
        {
            case ElementalType.Normal:
                break;
            case ElementalType.Fire:
                debuffData.fireDebuff.ApllyDebuff(collider, debuffDict[elemental], token)
                .ContinueWith(() =>
                {
                    if (!ctsDict[elemental].IsCancellationRequested) debuffDict[elemental] = 0; // 비동기 수행 정상적으로 끝나면 중첩 초기화
                })
                .Forget();
                break;
            case ElementalType.Water:
                debuffData.waterDebuff.ApllyDebuff(collider, debuffDict[elemental], token)
                .ContinueWith(() =>
                {
                    if (!ctsDict[elemental].IsCancellationRequested) debuffDict[elemental] = 0; // 비동기 수행 정상적으로 끝나면 중첩 초기화
                })
                .Forget();
                break;
            case ElementalType.Ice:
                debuffData.iceDebuff.ApllyDebuff(collider, debuffDict[elemental], token)
                .ContinueWith(() =>
                {
                    if (!ctsDict[elemental].IsCancellationRequested) debuffDict[elemental] = 0; // 비동기 수행 정상적으로 끝나면 중첩 초기화
                })
                .Forget();
                break;
            case ElementalType.Electric:
                debuffData.electricDebuff.ApllyDebuff(collider, debuffDict[elemental], token)
                .ContinueWith(() =>
                {
                    if (!ctsDict[elemental].IsCancellationRequested) debuffDict[elemental] = 0; // 비동기 수행 정상적으로 끝나면 중첩 초기화
                })
                .Forget();
                break;
            case ElementalType.Dark:
                debuffData.darkDebuff.ApllyDebuff(collider, debuffDict[elemental], token)
                .ContinueWith(() =>
                {
                    if (!ctsDict[elemental].IsCancellationRequested) debuffDict[elemental] = 0; // 비동기 수행 정상적으로 끝나면 중첩 초기화
                })
                .Forget();
                break;
            case ElementalType.Light:
                debuffData.lightDebuff.ApllyDebuff(collider, debuffDict[elemental], token)
                .ContinueWith(() =>
                {
                    if (!ctsDict[elemental].IsCancellationRequested) debuffDict[elemental] = 0; // 비동기 수행 정상적으로 끝나면 중첩 초기화
                })
                .Forget();
                break;
        }
    }

    public CancellationToken TokenCheck(ElementalType elemental) // debuffDict 중첩 및 토큰 확인
    {
        if (!debuffDict.ContainsKey(elemental)) // dict 정보 없을 시 추가
        {
            debuffDict.Add(elemental, 0);
            ctsDict.Add(elemental, null);
        }

        if (ctsDict[elemental] != null) // 기존 토큰 있다면 해제
        {
            ctsDict[elemental].Cancel();
            ctsDict[elemental].Dispose();

        }
        var cts = new CancellationTokenSource(); // 토큰 발급
        ctsDict[elemental] = cts;
        return cts.Token;
    }

    public void TakeSturn(Collider2D collider, float duration)
    {
        if (sturnIgnoring) return;
        else
        {
            sturnIgnoring = true;

            debuffData.sturnDebuff.ApllyDebuff(collider, duration)
            .ContinueWith(() =>
            {
                sturnIgnoring = false; // 비동기 수행 정상적으로 끝나면 스턴 무적 해제
            })
            .Forget();
        }
    }
}
