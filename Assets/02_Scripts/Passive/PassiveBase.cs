using System;
using UnityEngine;

public abstract class PassiveBase : ScriptableObject
{
    [SerializeField] private Sprite icon;
    [SerializeField, TextArea] private String passiveName;
    [SerializeField, TextArea] private String description;

    public Sprite Icon => icon;
    public String Name => passiveName;
    public String Description => description;

    public abstract void PassiveEffect();
}
