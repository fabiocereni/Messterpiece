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
    
    [Header("Respawn System")]
    [Tooltip("Riferimento al sistema di respawn")]
    public PlayerRespawn playerRespawn;

    [Header("Damage Feedback")]
    [Tooltip("Tempo di invincibilità dopo aver preso danno")]
    public float invincibilityDuration = 1.0f;
    
    [Header("UI")]
    [Tooltip("Mirino del player")]
    public GameObject crosshairUI;
    public DamageIndicator myDamageIndicator;

    private bool isInvincible = false;

    private GameObject lastAttacker = null;
    private bool isDead = false;

    private bool hasMatchStarted = false;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealthBar(currentHealth, maxHealth);
        }

        // Se esiste il MatchFlowManager → aspetta il warmup
        if (MatchFlowManager.Instance != null)
        {
            MatchFlowManager.Instance.OnWarmupComplete += OnMatchStarted;
        }
        else
        {
            // Nessun warmup → match già iniziato
            OnMatchStarted();
        }
    }


    /// <summary>
    /// Applica danno al player
    /// </summary>
    public void TakeDamage(float damage, Vector3 hitPoint, GameObject attacker = null)
    {
        // PROTEZIONE: Se è già morto o invincibile, ignora il danno
        if (isDead || isInvincible) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"[PlayerHealth] Player took {damage} damage. Health: {currentHealth}/{maxHealth}");

        // Aggiorna la UI
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealthBar(currentHealth, maxHealth);
        }
        
        // Crea la freccia (rossa) che indica la direzione del danno
        if (myDamageIndicator != null)
        {
            myDamageIndicator.DamageLocation = hitPoint;
            GameObject go = Instantiate(myDamageIndicator.gameObject, myDamageIndicator.transform.position, myDamageIndicator.transform.rotation, myDamageIndicator.transform.parent);
            go.SetActive(true);
        }
        
        if (attacker != null) lastAttacker = attacker;

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
    /// Player muore - ora usa respawn invece di game over
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("[PlayerHealth] Player DIED!");

        if (crosshairUI != null)
        {
            crosshairUI.SetActive(false);
        }

        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.RegisterKill(lastAttacker, transform.root.gameObject);
        }

        if (playerRespawn != null)
        {
            playerRespawn.RespawnPlayer(gameObject);
        }
        else
        {
            RestoreHealth(maxHealth);
        }
    }
    
    

    /// <summary>
    /// Getter per checking se morto (usa il flag isDead invece di solo currentHealth)
    /// </summary>
    public bool IsDead()
    {
        return isDead;
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
    
    /// <summary>
    /// Ripristina la salute del giocatore (usato dal respawn)
    /// </summary>
    public void RestoreHealth(float amount)
    {
        isDead = false;

        currentHealth = amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"[PlayerHealth] Salute ripristinata a {currentHealth}/{maxHealth}");

        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealthBar(currentHealth, maxHealth);
        }

        isInvincible = false;

        if (crosshairUI != null && hasMatchStarted)
        {
            crosshairUI.SetActive(true);
        }

    }
    
    private void OnMatchStarted()
    {
        hasMatchStarted = true;

        if (!isDead && crosshairUI != null)
        {
            crosshairUI.SetActive(true);
        }
    }
    
    private void OnDestroy()
    {
        if (MatchFlowManager.Instance != null)
        {
            MatchFlowManager.Instance.OnWarmupComplete -= OnMatchStarted;
        }
    }

    
    
}
