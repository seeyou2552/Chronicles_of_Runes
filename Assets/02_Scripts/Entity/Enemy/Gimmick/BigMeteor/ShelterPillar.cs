using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ShelterPillar : MonoBehaviour
{
    public SpriteRenderer sr;
    private Tilemap tilemap;

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    private void OnEnable()
    {
        EventBus.Subscribe(GimmickEvent.MeteorFall, DestroyPillar);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(GimmickEvent.MeteorFall, DestroyPillar);
    }

    public void DestroyPillar(object obj)
    {
        float alpha = sr.color.a;
        //float alpha = 1f;

        DoTweenExtensions.TweenFloat(alpha, 0f, 2f, (a) =>
        {
            // SpriteRenderer 투명도 조절
            if (sr != null)
            {
                Color c = sr.color;
                c.a = a;
                sr.color = c;
            }

            // Tilemap 투명도 조절
            if (tilemap != null)
            {
                Color c = tilemap.color;
                c.a = a;
                tilemap.color = c;
            }

        },
        () =>
        {
            Destroy(gameObject);
        });
    }
}
