using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonSpriteSwap : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler
{
    public Image targetImage;
    public Sprite normalSprite;
    public Sprite pressedSprite;

    public RectTransform textTransform;
    public Vector2 pressedOffset = new Vector2(0, -5);

    private Vector2 originalTextPos;

    void Start()
    {
        originalTextPos = textTransform.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetImage.sprite = pressedSprite;
        textTransform.anchoredPosition = originalTextPos + pressedOffset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetImage.sprite = normalSprite;
        textTransform.anchoredPosition = originalTextPos;
    }
}