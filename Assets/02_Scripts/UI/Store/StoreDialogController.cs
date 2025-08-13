using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StoreDialogController : MonoBehaviour
{
    [Header("대사 출력용 UI 텍스트")]
    public TextMeshProUGUI dialogText;

    [Header("구매창 대사 목록")]
    [TextArea] public List<string> purchaseLines = new List<string>();

    [Header("구매 확인 눌렀을 때 ")]
    [TextArea] public List<string> purchaseConfLines = new List<string>();

    [Header("판매창 대사 목록")]
    [TextArea] public List<string> sellLines = new List<string>();

    [Header("판매 확인 눌렀을 때")]
    [TextArea] public List<string> sellConfLines = new List<string>();

    //type에 맞춰 랜덤 대사 출력
    public void Show(DialogType type)
    {
        List<string> lines;
        switch (type)
        {
            case DialogType.Purchase:
                lines = purchaseLines;
                break;
            case DialogType.PurchaseConf:
                lines = purchaseConfLines;
                break;
            case DialogType.SellConf:
                lines = sellConfLines;
                break;
            default:
                lines = null;
                break;
        }

        if (lines == null || lines.Count == 0)
        {
            dialogText.text = "";
            return;
        }

        int idx = Random.Range(0, lines.Count);
        dialogText.text = lines[idx];
    }

    //외부에서 이 메서드 호출
    public string GetRandomLine(DialogType type)
    {
        List<string> lines;
        switch (type)
        {
            case DialogType.Purchase:
                lines = purchaseLines;
                break;
            case DialogType.PurchaseConf:
                lines = purchaseConfLines;
                break;
            case DialogType.SellConf:
                lines = sellConfLines;
                break;
            default:
                return "";
        }

        if (lines == null || lines.Count == 0)
            return "";

        return lines[Random.Range(0, lines.Count)];
    }
}
