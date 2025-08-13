using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomGimmickManager : Singleton<RoomGimmickManager>
{
    //연결된 방 기믹 중 랜덤으로 하나 골라서 내보내기 
    [Serializable]
    private struct GimmickEntry
    {
        public GimmickType type;
        public float weight;
        public Component prototype;
    }
    [SerializeField] private GimmickEntry[] gimmicks;
    private readonly Dictionary<Room, IRoomGimmick> active = new();
    private float totalWeight;

    protected override void Awake()
    {
        base.Awake();

        // 전체 가중치 계산
        totalWeight = 0f;
        foreach (var e in gimmicks)
            totalWeight += e.weight;
    }
    public void StartGimmick(Room room)
    {
        // 이미 활성화된 기믹이 있으면 무시
        if (active.ContainsKey(room))
            return;

        //랜덤으로 방 기믹 고르기
        float r = UnityEngine.Random.value * totalWeight;
        float cum = 0f;
        GimmickEntry? chosenEntry = null;

        foreach (var e in gimmicks)
        {
            cum += e.weight;
            if (r <= cum)
            {
                chosenEntry = e;
                break;
            }
        }

        // 선택된 게 없거나 None 타입이면 종료
        if (chosenEntry == null || chosenEntry.Value.weight <= 0f)
            return;

        //프로토타입 복제
        var protoComponent = chosenEntry.Value.prototype;
        if (protoComponent == null)
            return;

        var protoGO = protoComponent.gameObject;
        // room 하위에 위치(월드 좌표 그대로 유지)
        var instGO = Instantiate(protoGO, room.transform, worldPositionStays: false);
        instGO.name = protoGO.name;

        //IRoomGimmick 인터페이스로 꺼내서 시작
        if (instGO.TryGetComponent<IRoomGimmick>(out var gimmick))
        {
            gimmick.StartGimmick();
            active.Add(room, gimmick);
        }
        else
        {
            // 인터페이스 구현 안 되어 있으면 즉시 파괴
            Destroy(instGO);
        }
    }
    public void StopGimmick(Room room)
    {
        if (!active.TryGetValue(room, out var gimmick))
            return;

        // 기믹 중지
        gimmick.StopGimmick();

        // 해당 컴포넌트가 붙은 게임오브젝트 파괴
        Destroy((gimmick as Component)?.gameObject);

        //딕셔너리에서 제거
        active.Remove(room);
    }

}
