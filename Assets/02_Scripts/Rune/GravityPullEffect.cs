using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityPullEffect : MonoBehaviour
{
    public GameObject effectPrefab;

    public IEnumerator PullRoutine(Vector3 center, float radius, float pullForce, float duration, Action onComplete = null)
    {
        float elapsed = 0f;
        List<Rigidbody2D> enemies = new();
        
        GameObject effect = Instantiate(effectPrefab, center, Quaternion.identity);
        effect.transform.localScale = Vector3.one * radius / 4.5f;
        Destroy(effect, duration);

        Collider2D[] cols = Physics2D.OverlapCircleAll(center, radius, LayerMask.GetMask("Enemy"));

        foreach (var col in cols)
        {
            if (col.TryGetComponent<Rigidbody2D>(out var rb))
            {
                enemies.Add(rb);
            }
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            foreach (var rb in enemies)
            {
                if (rb == null) continue;

                float distance = Vector2.Distance(rb.position, center);
                
                float forceMultiplier = Mathf.Pow(distance / radius, 2f);
                float speed = pullForce * forceMultiplier;

                Vector2 dir = (center - (Vector3)rb.position).normalized;
                Vector2 newPos = Vector2.MoveTowards(rb.position, center, speed * Time.deltaTime);
                rb.MovePosition(newPos);
            }

            yield return null;
        }


        onComplete?.Invoke(); // 풀에 반환
    }
}
