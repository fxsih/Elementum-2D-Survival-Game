using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public Transform[] spawnPoints;
    public Transform player;

    void Start()
    {
        if (spawnPoints.Length == 0) return;

        int index = Random.Range(0, spawnPoints.Length);

        player.position = spawnPoints[index].position;
    }
}