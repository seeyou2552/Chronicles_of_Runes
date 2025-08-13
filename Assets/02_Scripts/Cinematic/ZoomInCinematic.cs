using Cinemachine;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomInCinematic : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;

    private void OnEnable()
    {
        EventBus.Subscribe(EventType.BossSpawnCinematic, BossSpawnDirection);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(EventType.BossSpawnCinematic, BossSpawnDirection);
    }

    private void Awake()
    {
        virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
    }


    public async void BossSpawnDirection(object obj)
    {
        virtualCamera.Priority = 30;

        await UniTask.Delay(3000);

        virtualCamera.Priority = 0;

    }
}
