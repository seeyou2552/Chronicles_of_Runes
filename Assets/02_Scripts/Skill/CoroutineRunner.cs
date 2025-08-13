using UnityEngine;
using System.Collections;

public class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner instance;
    public static CoroutineRunner Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("[CoroutineRunner]");
                instance = go.AddComponent<CoroutineRunner>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    public void RunCoroutine(IEnumerator routine)
    {
        StartCoroutine(routine);
    }
}