using TMPro;
using UnityEngine;
using DG.Tweening;

public class DamageText : MonoBehaviour
{
    public TextMeshProUGUI Bg;
    public TextMeshProUGUI Text;

    [Header("Motion Settings")]
    public float lifeTime = 1.2f;
    public float upwardForce = 2.5f;
    public float horizontalForce = 4.5f;
    public float gravity = -6f;

    [Header("Scale Settings")]
    public float startScale = 1.3f;
    public float endScale = 0.3f;
    public float scaleShrinkTime = 1f;

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;

    private float timer;
    private Vector3 velocity;

    void Start()
    {
        timer = 0f;

        float x = Random.Range(-horizontalForce, horizontalForce);
        velocity = new Vector3(x, upwardForce, 0f);

        transform.localScale = Vector3.zero;
        transform.DOScale(startScale, 0.15f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            // 이후 자연스럽게 줄어들도록
            transform.DOScale(endScale, scaleShrinkTime).SetEase(Ease.InQuad);
        });

        //페이드 아웃
        Text.DOFade(0f, fadeDuration).SetDelay(lifeTime - fadeDuration);
        Bg.DOFade(0f, fadeDuration).SetDelay(lifeTime - fadeDuration);
    }

    public void SetText(float value)
    {
        string str = value.ToString("F1");
        Text.text = str;
        Bg.text = str;

        SetAlpha(1f); // 알파 초기화
    }

    private void Update()
    {
        timer += Time.deltaTime;

        //포물선 이동
        velocity += Vector3.up * gravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;

        transform.forward = Camera.main.transform.forward;

        if (timer >= lifeTime)
            Destroy(gameObject);
    }

    void SetAlpha(float alpha)
    {
        var c1 = Text.color;
        c1.a = alpha;
        Text.color = c1;

        var c2 = Bg.color;
        c2.a = alpha;
        Bg.color = c2;
    }
}
