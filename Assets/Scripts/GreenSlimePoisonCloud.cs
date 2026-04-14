using UnityEngine;

public class GreenSlimePoisonCloud : MonoBehaviour
{
    public GameObject poisonCloudPrefab;
    public float detectionRange = 2.0f; // 🔥 bigger than before

    [Header("Poison Death Audio")]
public AudioClip[] poisonDeathSounds;

[Range(0f,1f)]
public float poisonVolume = 0.8f;

[Header("Pitch Variation")]
public float poisonMinPitch = 0.9f;
public float poisonMaxPitch = 1.2f;

int lastPoisonIndex = -1;
bool hasPlayedPoison = false;

    Transform player;
    EnemyController enemy;

    bool triggered = false;

    void Start()
    {
        enemy = GetComponent<EnemyController>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    void Update()
    {
        if (enemy == null || enemy.IsDead) return;
        if (player == null) return;
        if (triggered) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= detectionRange)
        {
            TriggerPoison();
        }
    }

    void TriggerPoison()
    {
        triggered = true;
        PlayPoisonDeathSound();

        if (poisonCloudPrefab != null)
        {
            Instantiate(poisonCloudPrefab, transform.position, Quaternion.identity);
        }

        enemy.TakeDamage(50f, false);
    }

    void PlayPoisonDeathSound()
{
    if (hasPlayedPoison) return;
    if (poisonDeathSounds == null || poisonDeathSounds.Length == 0) return;
    if (AudioManager.Instance == null) return;

    hasPlayedPoison = true;

    int index;

    do
    {
        index = Random.Range(0, poisonDeathSounds.Length);
    }
    while (index == lastPoisonIndex && poisonDeathSounds.Length > 1);

    lastPoisonIndex = index;

    float pitch = Random.Range(poisonMinPitch, poisonMaxPitch);

    AudioSource source = AudioManager.Instance.sfxSource;

    float originalPitch = source.pitch;
    source.pitch = pitch;

    source.PlayOneShot(poisonDeathSounds[index], poisonVolume);

    source.pitch = originalPitch;
}
}