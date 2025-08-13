using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "InvincibleRune", menuName = "Rune/Invincible")]
public class InvincibleRune : Rune
{
    public override void Apply(Skill skill)
    {
        skill.UseSkillSet(() =>
        {
            PlayerController.Instance.isInvincible = true;

            CoroutineRunner.Instance.RunCoroutine(
                RestoreInvincible(skill, Mathf.Max(skill.duration,0.1f))
            );
        });
    }

    private IEnumerator RestoreInvincible(Skill skill, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.isInvincible = false;
        }
    }
}