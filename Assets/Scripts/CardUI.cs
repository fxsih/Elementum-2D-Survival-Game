using UnityEngine;
using UnityEngine.EventSystems;

public class CardUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    ISelectHandler,
    IDeselectHandler
{
    public float normalScale = 1f;
    public float selectedScale = 1.25f;
    public float scaleSpeed = 12f;

    Vector3 targetScale;

    void Start()
    {
        targetScale = Vector3.one * normalScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.unscaledDeltaTime * scaleSpeed
        );
    }

    // 🖱️ Mouse hover → SAME as keyboard selection
  public void OnPointerEnter(PointerEventData eventData)
{
    // 🔥 HARD BLOCK mouse when keyboard active
    if (UpgradeManager.Instance.usingKeyboard)
    {
        eventData.pointerEnter = null;
        return;
    }

    EventSystem.current.SetSelectedGameObject(gameObject);
}

public void OnPointerExit(PointerEventData eventData)
{
    if (UpgradeManager.Instance.usingKeyboard) return;

    if (EventSystem.current.currentSelectedGameObject == gameObject)
    {
        EventSystem.current.SetSelectedGameObject(null);
    }
}

public void OnSelect(BaseEventData eventData)
{
    targetScale = Vector3.one * selectedScale;
}

public void OnDeselect(BaseEventData eventData)
{
    targetScale = Vector3.one * normalScale;
}

    public UpgradeData GetUpgrade()
{
    UpgradeCard card = GetComponent<UpgradeCard>();
    return card != null ? card.GetUpgrade() : null;
}

public void ForceDeselect()
{
    targetScale = Vector3.one * normalScale;
}

public void ResetCard()
{
    targetScale = Vector3.one * normalScale;
    transform.localScale = targetScale;
}
}