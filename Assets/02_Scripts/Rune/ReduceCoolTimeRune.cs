using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "ReduceCoolTimeRune", menuName = "Rune/ReduceCoolTimeRune")]
public class ReduceCoolTimeRune : Rune
{
    [SerializeField] private float coolTimeScale = 0.8f;

    public override void Apply(Skill skill)
    {
        float capturedOrigin = skill.coolTime;

        skill.UseSkillSet(() =>
        {
            skill.coolTime *= coolTimeScale;

            CoroutineRunner.Instance.RunCoroutine(RestoreCoolTime(skill, capturedOrigin, skill.duration));
        });
    }

    private IEnumerator RestoreCoolTime(Skill skill, float origin, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (skill != null)
        {
            skill.coolTime = origin;
        }
    }
}