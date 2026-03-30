using UnityEngine;

public class PlayerGemInventory : MonoBehaviour
{
    public int carriedGems = 0;

    public void AddGems(int amount)
    {
        carriedGems += amount;

        // 🔥 update UI
        GemCounter.Instance.AddGems(amount);
    }

    public int DepositAll()
    {
        int amount = carriedGems;
        carriedGems = 0;

        // 🔥 reset UI
        GemCounter.Instance.ResetGems();

        return amount;
    }
}