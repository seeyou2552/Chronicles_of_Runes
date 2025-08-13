using UnityEngine;
using UnityEngine.UI;
using System;

public class OverwriteAlert 
{
    //장착한 룬을 새로운 룬으로 덮어씌울 때 뜨는 경고 팝업 스크립트

    public static void Show(RuneData oldRune, RuneData newRune, Action onYes, Action onNo = null)
    {
        string msg = $"{oldRune.displayName} → {newRune.displayName}\n\n" +
            $"룬을 덮어씌우시겠습니까?\r\n기존 룬의 정보는 사라집니다.\r\n";
        StaticPopupManager.Instance.ShowConfirm(
            msg,
            () => { onYes?.Invoke(); },
            () => { onNo?.Invoke(); }
        );
    }

    public static void SkillShow(Action onYes, Action onNo = null)
    {
        string msg = $"레벨이 높은 스킬북을 낮은 스킬북으로 합성을 시도했습니다.\r\n" +
            $"정말 합성하시겠습니까?\r\n" +
            $"이 스킬북의 정보가 사라지며 레벨이 계승되지 않습니다.";
        StaticPopupManager.Instance.ShowConfirm (
            msg,
            () => { onYes?.Invoke(); },
            () => { onNo?.Invoke(); }
        );
    }
}
