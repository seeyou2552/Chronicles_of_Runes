using System;
using UnityEngine;

public abstract class Rune : ScriptableObject
{
    public string runeName;
    public bool enemy;
    public abstract void Apply(Skill skill);
}