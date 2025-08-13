using UnityEngine;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

[CreateAssetMenu(fileName = "WaveRune", menuName = "Rune/Wave")]
public class WaveRune : Rune
{
    [SerializeField] private GameObject wavePrefeb;
    [SerializeField] private float damage;
    [SerializeField] private float speed;

    public override void Apply(Skill skill)
    {
        skill.UseSkillSet(() =>
        {
            if (ObjectPoolManager.Instance.pools != null && ObjectPoolManager.Instance.pools.ContainsKey(wavePrefeb.name))
            {

            }
            else  // 풀 없으면 생성
            {
                ObjectPoolManager.Instance.CreatePool(wavePrefeb.name, wavePrefeb, 1);
            }
            var wave = ObjectPoolManager.Instance.Get(wavePrefeb.name);

            

            // 포지션 설정
            wave.transform.position = PlayerController.Instance.transform.position;

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;

            Vector3 dir = (mouseWorldPos - wave.transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // 주입
            Wave waveComp = wave.GetComponent<Wave>();
            waveComp.damage = damage + skill.damage;
            waveComp.speed = speed;
            waveComp.objName = wavePrefeb.name;
            waveComp.targetPos = dir;
            wave.transform.rotation = Quaternion.Euler(0, 0, angle - 90);
            wave.SetActive(true);
        });
    }
}

