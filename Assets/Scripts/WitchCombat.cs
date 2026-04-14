using UnityEngine;
using System.Collections;

public class WitchCombat : MonoBehaviour
{
    SpriteRenderer sr;
    Animator anim;
    Rigidbody2D rb;
    EnemyController enemy;

    Color originalColor;

    public float knockbackForce = 4f;

    [Header("Witch Death Audio")]
public AudioClip[] deathSounds;

[Range(0f,1f)]
public float deathVolume = 0.9f;

public float deathMinPitch = 0.9f;
public float deathMaxPitch = 1.1f;

int lastDeathIndex = -1;
bool hasPlayedDeath = false;

void Start()
{
    anim = GetComponent<Animator>();
    sr = GetComponent<SpriteRenderer>();
    rb = GetComponent<Rigidbody2D>();
    enemy = GetComponent<EnemyController>();

    if (sr != null)
        originalColor = sr.color;

    Debug.Log("WitchCombat initialized");
}

void Update()
{
    if (Input.GetKeyDown(KeyCode.K))
    {
        Debug.Log("K PRESSED");
        anim.Play("Death", 0, 0f);
        Debug.Log(anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
    }

    if (Input.GetKeyDown(KeyCode.H))
    {
        Debug.Log("H PRESSED");
        StartCoroutine(Flash());
    }
}

    public void OnHit(Vector2 hitDir)
    {
        if (enemy == null) return;

        StartCoroutine(Flash());

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.linearVelocity = hitDir * knockbackForce;
        }
    }

  IEnumerator Flash()
{
    if (sr == null) yield break;

    sr.color = Color.red; // ✅ clean flash (no glow)

    yield return new WaitForSeconds(0.08f);

    sr.color = originalColor;
}

    public void OnDeathEffects()
{
    // ONLY extra effects — NO animation control

    PlayDeathSound();
    GetComponent<WitchShooter>().enabled = false;

    var ai = GetComponent<Pathfinding.AIPath>();
    if (ai != null) ai.canMove = false;

    if (rb != null)
        rb.linearVelocity = Vector2.zero;

    Collider2D col = GetComponent<Collider2D>();
    if (col != null)
        col.enabled = false;
}

public void OnWitchDeathAnimationEnd()
{
    StartCoroutine(FadeOutAndDestroy());
}

IEnumerator FadeOutAndDestroy()
{
    SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

    float duration = 0.5f;
    float time = 0f;

    while (time < duration)
    {
        float alpha = Mathf.Lerp(1f, 0f, time / duration);

        foreach (var sr in renderers)
        {
            Color c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, alpha);
        }

        time += Time.deltaTime;
        yield return null;
    }

    Destroy(gameObject);
}

void PlayDeathSound()
{
    if (hasPlayedDeath) return;
    if (deathSounds == null || deathSounds.Length == 0) return;
    if (AudioManager.Instance == null) return;

    hasPlayedDeath = true;

    int index;

    do
    {
        index = Random.Range(0, deathSounds.Length);
    }
    while (index == lastDeathIndex && deathSounds.Length > 1);

    lastDeathIndex = index;

    float pitch = Random.Range(deathMinPitch, deathMaxPitch);

    AudioSource source = AudioManager.Instance.sfxSource;

    float originalPitch = source.pitch;
    source.pitch = pitch;

    source.PlayOneShot(deathSounds[index], deathVolume);

    source.pitch = originalPitch;
}
}