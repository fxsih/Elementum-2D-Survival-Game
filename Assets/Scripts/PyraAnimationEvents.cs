using UnityEngine;

public class PyraAnimationEvents : MonoBehaviour
{
    PlayerController playerController;

    void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    public void SpawnAttack1VFX()
    {
        if (playerController != null)
            playerController.SpawnAttack1VFX();
    }

   public void SpawnAttack2VFX_Left()
{
    if (playerController != null)
        playerController.SpawnAttack2VFX_Left();
}

public void SpawnAttack2VFX_Right()
{
    if (playerController != null)
        playerController.SpawnAttack2VFX_Right();
}
}