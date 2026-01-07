using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Vita massima del player")]
    public float maxHealth = 150f;

    [Tooltip("Vita corrente")]
    public float currentHealth;

    [Header("UI Reference")]
    [Tooltip("Riferimento alla barra della vita UI")]
    public HealthBarUI healthBarUI;

    [Header("Damage Feedback")]
    [Tooltip("Tempo di invincibilità dopo aver preso danno")]
    public float invincibilityDuration = 1.0f;

    private bool isInvincible = false;

    void Start()
    {
        currentHealth = maxHealth;

        // Aggiorna la UI all'inizio
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    /// <summary>
    /// Applica danno al player
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"[PlayerHealth] Player took {damage} damage. Health: {currentHealth}/{maxHealth}");

        // Aggiorna la UI
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealthBar(currentHealth, maxHealth);
        }

        // Attiva invincibilità temporanea
        StartCoroutine(InvincibilityCoroutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Cura il player
    /// </summary>
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"[PlayerHealth] Player healed {amount}. Health: {currentHealth}/{maxHealth}");

        // Aggiorna la UI
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    /// <summary>
    /// Coroutine per gestire l'invincibilità temporanea
    /// </summary>
    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    /// <summary>
    /// Player muore
    /// </summary>
    private void Die()
    {
        Debug.Log($"[PlayerHealth] Player DIED!");

        // Qui puoi aggiungere logica per il game over
        // Per ora stampiamo solo un messaggio
        // TODO: Implementare logica di respawn o game over
    }

    /// <summary>
    /// Getter per checking se morto
    /// </summary>
    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    /// <summary>
    /// Getter health corrente
    /// </summary>
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// Getter health massima
    /// </summary>
    public float GetMaxHealth()
    {
        return maxHealth;
    }
}
