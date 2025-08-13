using System;
using UnityEngine;

// 공통 아이템 데이터

[Serializable]
public class ItemData : ScriptableObject
{
    public int id;
    public string displayName;    // 이름
    public Sprite icon;           // 아이콘
    public string description;  //설명
    public ItemType itemType;     // 종류
    public int price;             //가격
}