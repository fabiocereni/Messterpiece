using UnityEngine;

/// <summary>
/// Componente aggiuntivo per i nemici nel tutorial.
/// Implementa IDamagable e notifica l'EnemyCounter quando muore.
/// Usa insieme a EnemyAI_NavMesh per il movimento.
/// </summary>
public class TutorialEnemy : MonoBehaviour, IDamagable
{
    [Header("Enemy Settings")]
    [SerializeField] private float health = 100f;

    [Header("References")]
    [SerializeField] private EnemyCounter enemyCounter;

    [Header("Visual Feedback (Optional)")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private AudioClip deathSound;

    private bool isDead = false;

    void Start()
    {
        // Se non assegnato, cerca l'EnemyCounter nella scena
        if (enemyCounter == null)
        {
            enemyCounter = FindFirstObjectByType<EnemyCounter>();

            if (enemyCounter == null)
            {
                Debug.LogWarning($"EnemyCounter non trovato! Il nemico {gameObject.name} non notificherà la morte.");
            }
        }
    }

    public void Damage(float amount)
    {
        if (isDead) return;

        health -= amount;

        if (health <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Notifica il counter
        if (enemyCounter != null)
        {
            enemyCounter.OnEnemyDefeated();
        }

        // Spawn death effect
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Play death sound
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        Debug.Log($"Nemico {gameObject.name} eliminato!");

        // Distruggi il nemico
        Destroy(gameObject);
    }
}
