using UnityEngine;
using System.Collections;

public class HitStop : MonoBehaviour
{
    public static HitStop Instance;

    void Awake()
    {
        Instance = this;
    }

    public void DoHitStop(float duration)
    {
        StartCoroutine(HitStopCoroutine(duration));
    }

    IEnumerator HitStopCoroutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }
}