using UnityEngine;

public class Nexus : MonoBehaviour
{
    [Header("UI")]
    public GameObject interactUI;           // "Press E"
    public GameObject noGemsText;           // "No gems to give!"
    public RectTransform interactUIRect;    // RectTransform of Press E

    [Header("Tracking")]
    public Transform nexusTop;              // Empty object above statue

   int playersInRange = 0;
PlayerGemInventory playerInRange;

    void Start()
    {
        if (interactUI != null) interactUI.SetActive(false);
        if (noGemsText != null) noGemsText.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
{
    PlayerGemInventory inv = other.GetComponent<PlayerGemInventory>();

    if (inv != null)
    {
        playersInRange++;
        playerInRange = inv;

        if (interactUI != null)
            interactUI.SetActive(true);
    }
}

    void OnTriggerExit2D(Collider2D other)
{
    if (other.GetComponent<PlayerGemInventory>() != null)
    {
        playersInRange--;

        if (playersInRange <= 0)
        {
            playersInRange = 0;
            playerInRange = null;

            if (interactUI != null)
                interactUI.SetActive(false);

            if (noGemsText != null)
                noGemsText.SetActive(false);
        }
    }
}

    void Update()
{
    if (playersInRange > 0)
    {
        if (interactUI != null && !interactUI.activeSelf)
            interactUI.SetActive(true);

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryDeposit();
        }
    }
}

    void LateUpdate()
    {
        // 🔥 Make UI follow statue
        if (playerInRange == null || interactUIRect == null || nexusTop == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(nexusTop.position);

        // 🔥 slight offset upward (optional)
        screenPos.y += 40f;

        interactUIRect.position = screenPos;
    }

    void TryDeposit()
{
    if (playerInRange.carriedGems <= 0)
    {
        StopAllCoroutines(); // 🔥 prevents stacking
        StartCoroutine(ShowNoGems());
        return;
    }

    int amount = playerInRange.DepositAll();
    GameManager.Instance.AddGems(amount);

    Debug.Log("Deposited: " + amount);
}

    System.Collections.IEnumerator ShowNoGems()
{
    if (noGemsText == null) yield break;

    noGemsText.SetActive(true);

    CanvasGroup cg = noGemsText.GetComponent<CanvasGroup>();

    if (cg == null)
        cg = noGemsText.AddComponent<CanvasGroup>();

    cg.alpha = 1f;

    // 🔥 stay visible
    yield return new WaitForSeconds(0.8f);

    // 🔥 fade out
    float t = 0f;
    float duration = 0.5f;

    while (t < duration)
    {
        t += Time.deltaTime;
        cg.alpha = Mathf.Lerp(1f, 0f, t / duration);
        yield return null;
    }

    cg.alpha = 0f;
    noGemsText.SetActive(false);
}


}