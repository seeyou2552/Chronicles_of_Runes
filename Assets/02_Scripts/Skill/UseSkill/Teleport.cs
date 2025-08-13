using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

public class Teleport : MonoBehaviour, SkillAction
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

    public float radius = 2f;
    private float currentAngle = 0f;
    Vector3 targetPos; // 목표 위치
    Vector3 casterPos; // 시전자 위치

    [Header("SoundController")]
    [SerializeField] private SkillSoundController soundController;

    private CancellationTokenSource cts;

    public void Init(Vector3 pos)
    {
        onStart?.Invoke();
        caster = PlayerController.Instance.gameObject;
        casterPos = PlayerController.Instance.transform.position;

        Teleporting(pos);
        soundController.StartSound(objName);
    }

    private void Teleporting(Vector3 pos)
    {
        onMiddle?.Invoke();
        Vector3 dir = (pos - casterPos).normalized;

        // 막대기 위치
        transform.position = casterPos;

        // 막대기를 마우스 방향으로 회전
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        Vector3 size = sr.bounds.size;             // Vector3(가로, 세로, 깊이)
        Vector3 axis = transform.right.normalized; // 막대기의 축 방향 벡터 (unit vector)

        // 축 방향 벡터의 절댓값을 size와 곱해서 막대기 축 길이 계산
        float length = Mathf.Abs(axis.x) * size.x + Mathf.Abs(axis.y) * size.y;

        // pivot이 왼쪽 끝 (0)일 때 → 오른쪽 끝 위치 계산
        Vector3 endPos = transform.position + transform.right * length;
        
        transform.position = endPos;

        RaycastHit2D hit = Physics2D.Raycast(casterPos, dir, length, LayerMask.GetMask("Obstacle")); // 벽에 닿을 경우 벽까지 이동
        if (hit.collider != null)
        {
            Vector3 hitPoint = hit.point;
            hitPoint.z = 0f;

            caster.transform.position = hitPoint;
        }
        else caster.transform.position = endPos;

        

        GetComponent<Animator>().SetInteger("Elemental", (int)elemental);

        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        Return(cts.Token).Forget();
    }

    public async UniTaskVoid Return(CancellationToken token)
    {
        onEnd?.Invoke();
        await UniTask.Delay((int)(duration * 1000), cancellationToken: token);
        if (this == null || gameObject == null) return;
        // GetComponent<SpriteRenderer>().sprite = null;

        elemental = ElementalType.Normal;
        ObjectPoolManager.Instance.Return(this.gameObject, objName);
    }

    public void EnemyInit()
    {
        casterPos = caster.transform.position;
        targetPos = PlayerController.Instance.transform.position;
        targetPos.z = 0f;
        // Teleporting();
    }


}
