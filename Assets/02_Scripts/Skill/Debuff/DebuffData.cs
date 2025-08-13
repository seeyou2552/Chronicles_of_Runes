using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[CreateAssetMenu(fileName = "DebuffData", menuName = "Debuff/DebuffData")]
public class DebuffData : ScriptableObject
{
    [Header("Sturn")]
    public SturnDebuff sturnDebuff;
    
    [Header("Fire")]
    public HitDebuff fireDebuff;

    [Header("Water")]
    public SlowDebuff waterDebuff;

    [Header("Ice")]
    public SlowDebuff iceDebuff;

    [Header("Electric")]
    public HitDebuff electricDebuff;

    [Header("Dark")]
    public HitDebuff darkDebuff;

    [Header("Light")]
    public SlowDebuff lightDebuff;


}   
