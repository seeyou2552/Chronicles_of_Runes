using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="ItemData/Potion")]
public class PotionData : ItemData
{
    //체력 회복량
    public float hpRecover;

    //마나 회복량
    public float mpRecover;
}
