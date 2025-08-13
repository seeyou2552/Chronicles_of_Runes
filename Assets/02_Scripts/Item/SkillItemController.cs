using System.Collections.Generic;
using UnityEngine;

public class SkillItemController : MonoBehaviour
{
    //스킬 아이템 프리팹에 붙는 스크립트
    //룬이 장착되면 여기에 추가되어 스킬 슬롯을 옮겨다녀도 반영이 됨;
    public int level;
    public List<RuneData> attachedRunes = new List<RuneData>();
}
