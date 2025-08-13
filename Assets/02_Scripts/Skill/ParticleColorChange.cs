using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleColorChange : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] particles;

    [SerializeField] private Color normalColor;
    [SerializeField] private Color fireColor;
    [SerializeField] private Color waterColor;
    [SerializeField] private Color iceColor;
    [SerializeField] private Color electricColor;
    [SerializeField] private Color darkColor;
    [SerializeField] private Color lightColor;
    private Color curColor;

    public void SwitchColor(ElementalType elemental)
    {
        switch (elemental)
        {
            case ElementalType.Normal:
                curColor = normalColor;
                break;
            case ElementalType.Fire:
                curColor = fireColor;
                break;
            case ElementalType.Water:
                curColor = waterColor;
                break;
            case ElementalType.Ice:
                curColor = iceColor;
                break;
            case ElementalType.Electric:
                curColor = electricColor;
                break;
            case ElementalType.Dark:
                curColor = darkColor;
                break;
            case ElementalType.Light:
                curColor = lightColor;
                break;
        }

        foreach (var particle in particles)
        {
            var mainM = particle.main;
            mainM.startColor = curColor; 
        }
         
    }
}
