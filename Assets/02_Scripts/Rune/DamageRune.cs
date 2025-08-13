using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageRune", menuName = "Rune/DamageRune")]
public class DamageRune : Rune
{
    [SerializeField] private float damageScale = 2f;

    public override void Apply(Skill skill)
    {
        // 호출 시점의 원래 데미지를 "지역 변수"로 캡처
        float capturedOrigin = skill.damage;

        skill.UseSkillSet(() =>
        {
            skill.damage *= damageScale;

            // duration 이후 원복 (스킬이 제거되었거나 변경되었을 수도 있으니 코루틴 쪽에서 체크)
            CoroutineRunner.Instance.RunCoroutine(RestoreDamage(skill, capturedOrigin, skill.duration));
        });
    }

    private IEnumerator RestoreDamage(Skill skill, float origin, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (skill != null)
        {
            // 단순 원복: 다른 버프 시스템과 섞인다면 "스택 기반" 설계를 고려
            skill.damage = origin;
        }
    }
}