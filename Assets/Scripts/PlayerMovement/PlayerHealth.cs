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

        // Se esiste il MatchFlowManager, aspetta il warmup
        if (MatchFlowManager.Instance != null)
        {
            MatchFlowManager.Instance.OnWarmupComplete += OnMatchStarted;
        }
        else
        {
            // Nessun warmup, match già iniziato
            OnMatchStarted();
        }
    }


    // applica danno al player
    public void TakeDamage(float damage, Vector3 hitPoint, GameObject attacker = null)
    {
        // se è già morto o invincibile, ignora il danno
        if (isDead || isInvincible) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // assicura che non scenda sotto 0 la vita

        // aggiorna la UI
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealthBar(currentHealth, maxHealth);
        }
        
        // creo la freccia che indica la direzione del danno
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

    // cura il player
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // aggiorna la UI
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    // Coroutine per gestire l'invincibilità temporanea
    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    // Player muore - respawna
    private void Die()
    {
        if (isDead) return;
        isDead = true;

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

    // Controlla se il giocatore è morto
    public bool IsDead()
    {
        return isDead;
    }

    // Getter health corrente
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    // Getter health massima
    public float GetMaxHealth()
    {
        return maxHealth;
    }
    
    // Ripristina la salute del giocatore (usato dal respawn)
    public void RestoreHealth(float amount)
    {
        isDead = false;

        currentHealth = amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

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
        hasMatchStarted = true; // Il match è iniziato

        // se sono vivo, mostra il mirino
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
