using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningEffect : Singleton<WarningEffect>
{
    public GameObject lineRendererPrefab;
    public GameObject CircleEffectPrefab;
    private LineRenderer[] lineRenderers;

    public (GameObject go, Tween tween) CreateLines(Vector3[] shotPositions, Vector3 dir, int cnt, float lineLength, float lineWidth, float delay, LayerMask targetMask, Action callBack = null)
    {
        lineRenderers = new LineRenderer[cnt];
        GameObject go = null;
        Tween tween = null; 

        bool isCallBack = true;
        for (int i = 0; i < cnt; i++)
        {
            go = ObjectPoolManager.Instance.Get(lineRendererPrefab.name);
            go.transform.SetParent(transform);

            //타겟 설정
            go.GetComponent<WarningEffectController>().targetMask = targetMask;

            LineRenderer lr = go.GetComponent<LineRenderer>();
            lr.useWorldSpace = true; // 꼭 명시적으로 설정

            // 이펙트 시간 설정
            var controller = go.GetComponent<WarningEffectController>();
            if (controller != null)
                if (isCallBack)
                {
                    tween = controller.SetDuration(delay, callBack);
                    isCallBack = false;
                }
                else
                {
                    tween = controller.SetDuration(delay, null);
                }

            Vector3 startPos = shotPositions[i];
            Vector3 endPos = startPos + dir * lineLength;

            // Z축 제거 (2D 충돌 계산에는 필요 없음)
            startPos.z = 0f;
            endPos.z = 0f;

            // 라인 렌더링
            lr.positionCount = 2;
            lr.SetPosition(0, startPos);
            lr.SetPosition(1, endPos);
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;

            // EdgeCollider2D 설정
            EdgeCollider2D edgeCollider = go.GetComponent<EdgeCollider2D>();
            if (edgeCollider != null)
            {
                // 끝부분 둥글게 안 하고 각지게
                edgeCollider.edgeRadius = 0f;
                edgeCollider.isTrigger = true;
                edgeCollider.edgeRadius = lineWidth * 0.5f;

                // 로컬 좌표로 변환
                Vector2[] edgePoints = new Vector2[2];
                edgePoints[0] = go.transform.InverseTransformPoint(startPos);
                edgePoints[1] = go.transform.InverseTransformPoint(endPos);
                edgeCollider.points = edgePoints;
            }

            lineRenderers[i] = lr;
        }
        return (go, tween);
    }

    public (GameObject go, Tween tween) CreateCircle(Vector3 position, float radius, float duration, LayerMask targetMask, Action callBack = null)
    {
        GameObject warning = ObjectPoolManager.Instance.Get("CircleWarningEffect");
        warning.transform.SetParent(transform);
        warning.transform.position = position;

        var controller = warning.GetComponent<CircleWarningEffectController>();

        Tween tween = controller.Setup(radius, duration, targetMask, callBack);


        return (warning, tween);
    }

    //일단 안씀
    public void ClearLines()
    {
        if (lineRenderers != null)
        {
            foreach (var lr in lineRenderers)
            {
                if (lr != null)
                    Destroy(lr.gameObject);
            }
        }
    }
}
