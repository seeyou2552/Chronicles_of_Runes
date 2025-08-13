using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebuffEffect : MonoBehaviour
{
    public GameObject targer;
    void OnDisable()
    {
        targer = null;
    }
    void Update()
    {
        if (targer != null)
        {
            transform.position = targer.transform.position;
        }
    }
}
