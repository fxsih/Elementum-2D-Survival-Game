using UnityEngine;
using EasyTransition;

public class PlayButtonHandler : MonoBehaviour
{
    public TransitionSettings transition; // drag CircleWipe here

    public void LoadGame()
    {
        TransitionManager.Instance().Transition("Elementum", transition, 0f);
    }
}