using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public AudioClip hoverSound;
    public AudioClip clickSound;

    public void OnPointerEnter(PointerEventData eventData)
{
    if (AudioManager.Instance == null) return;

    AudioManager.Instance.PlaySFX(hoverSound);
}

    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance.sfxSource.pitch = Random.Range(0.95f, 1.05f);
AudioManager.Instance.PlaySFX(clickSound);
AudioManager.Instance.sfxSource.pitch = 1f;
    }
}