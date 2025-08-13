using System.Collections.Generic;

[System.Serializable]
public class SkillSlotData
{
    public Skill skill;
    public List<Rune> runes;
    public float currentCoolTime;
    public InputIndex index;

    public SkillSlotData(Skill skill, List<Rune> runes)
    {
        this.skill = skill;
        this.runes = runes;
    }
}