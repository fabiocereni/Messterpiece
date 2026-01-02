using UnityEngine;

/// <summary>
/// Target specifico per il tutorial che notifica il TargetCounter quando viene distrutto.
/// Implementa IDamagable per ricevere danno dai proiettili.
/// </summary>
public class TutorialTarget : MonoBehaviour, IDamagable
{
    [Header("Target Settings")]
    [SerializeField] private float health = 50f;

    [Header("References")]
    [SerializeField] private TargetCounter targetCounter;

    [Header("Visual Feedback (Optional)")]
    [SerializeField] private GameObject destroyEffect; // Opzionale: particle effect alla distruzione
    [SerializeField] private AudioClip destroySound;   // Opzionale: suono alla distruzione

    private bool isDestroyed = false;

    void Start()
    {
        // Se non è stato assegnato manualmente, cerca il TargetCounter nella scena
        if (targetCounter == null)
        {
            // Usa FindObjectOfType invece di FindFirstObjectByType per evitare memory leak
            targetCounter = FindAnyObjectByType<TargetCounter>();

            if (targetCounter == null)
            {
                Debug.LogWarning($"[TutorialTarget] {gameObject.name}: TargetCounter non trovato nella scena!");
            }
        }
    }

    public void Damage(float amount)
    {
        if (isDestroyed) return;

        health -= amount;

        if (health <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // Notifica il counter
        if (targetCounter != null)
        {
            targetCounter.OnTargetDestroyed();
        }

        // Spawn destroy effect (opzionale)
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }

        // Play sound (opzionale)
        if (destroySound != null)
        {
            AudioSource.PlayClipAtPoint(destroySound, transform.position);
        }

        // Distruggi il target
        Destroy(gameObject);
    }
}
