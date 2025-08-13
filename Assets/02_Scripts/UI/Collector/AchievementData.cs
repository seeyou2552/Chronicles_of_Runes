using UnityEngine;

[CreateAssetMenu(menuName = "AchievementData/Achievement")]
public class AchievementData : ScriptableObject
{
    public Sprite icon;
    private string iconPath;
    [SerializeField] public string displayName;
    [SerializeField] public string description;
}
