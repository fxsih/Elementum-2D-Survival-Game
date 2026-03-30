using UnityEngine;
using TMPro;
using System.Collections;


public class UIManager : MonoBehaviour
{
   public static UIManager Instance;
   public TextMeshProUGUI killText;
   Color normalColor = Color.white;
   Color flashColor = new Color(1f, 0.85f, 0.2f); //golden flash color

   void Start()
   {
        UpdateKillText(0);
        killText.color = normalColor;
   }
   void Awake()
   {
       
        Instance = this;
   }
   public void UpdateKillText(int count)
   {
        killText.text = "Kills : " + count;
        killText.transform.localScale = Vector3.one * 1.2f; // pop effect
        Invoke(nameof(ResetScale), 0.2f); // reset after 0.2s
        StopAllCoroutines();
        StartCoroutine(FlashEffect());
   }
    void ResetScale()
    {
          killText.transform.localScale = Vector3.one;
    }
    IEnumerator FlashEffect()
     {
          //instantly change to flash color
          killText.color = flashColor;
          float t=0f;
          float duration = 0.4f; // flash duration
          while (t < duration)
          {
               t += Time.deltaTime;
               //gradually transition back to normal color
               killText.color = Color.Lerp(flashColor, normalColor, t / duration);
               yield return null;
          }
          killText.color = normalColor; // ensure it ends on normal color
     }
}
