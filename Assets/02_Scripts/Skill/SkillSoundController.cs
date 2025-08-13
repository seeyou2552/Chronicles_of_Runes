using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSoundController : MonoBehaviour
{
    private Dictionary<int, string> useSound = new();
    private Dictionary<int, string> hitSound = new();

    public void StartSound(ElementalType elemental, string name)
    {
        if (!useSound.ContainsKey((int)elemental))
        {
            useSound.Add((int)elemental, elemental.ToString() + name);
        }

        SoundManager.PlaySFX(useSound[(int)elemental]);
    }

    public void StartSound(string name)
    {
        if (!useSound.ContainsKey(-1))
        {
            useSound.Add(-1, name);
        }

        SoundManager.PlaySFX(useSound[-1]);
    }

    public void HitSound(ElementalType elemental, string name)
    {
        if (!hitSound.ContainsKey((int)elemental))
        {
            hitSound.Add((int)elemental, "Hit" + elemental.ToString() + name);
        }

        SoundManager.PlaySFX(hitSound[(int)elemental]);
    }

    public void HitSound(string name)
    {
        if (!hitSound.ContainsKey(-1))
        {
            hitSound.Add(-1, "Hit" + name);
        }

        SoundManager.PlaySFX(hitSound[-1]);
    }

}
