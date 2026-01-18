using UnityEngine;

/// <summary>
/// Controlla il completamento della sezione Health Demo:
/// 1. Player deve prendere danno (trigger DamageZone)
/// 2. Player deve raccogliere medikit per curarsi
/// </summary>
public class HealthDemoChecker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject medikitPrefab;
    [SerializeField] private Transform medikitSpawnPoint;

    [Header("Settings")]
    [SerializeField] private float damageThreshold = 20f; // Danno minimo da prendere

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private bool hasTakenDamage = false;
    private bool hasHealed = false;
    private bool sectionCompleted = false;

    private float playerInitialHealth = 0f;
    private float playerLowestHealth = 100f;
    private GameObject spawnedMedikit = null;

    void Start()
    {
        // Ottieni la salute iniziale del player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                playerInitialHealth = ph.currentHealth;
                playerLowestHealth = playerInitialHealth;
            }
        }

        Debug.Log("[HealthDemoChecker] Tracciamento salute iniziato");
    }

    void Update()
    {
        if (sectionCompleted) return;

        CheckDamage();
        CheckHealing();
        CheckCompletion();
    }

    void CheckDamage()
    {
        if (hasTakenDamage) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                float currentHealth = ph.currentHealth;

                // Aggiorna salute minima
                if (currentHealth < playerLowestHealth)
                {
                    playerLowestHealth = currentHealth;
                }

                // Controlla se ha preso abbastanza danno
                float damageTaken = playerInitialHealth - playerLowestHealth;
                if (damageTaken >= damageThreshold)
                {
                    hasTakenDamage = true;
                    Debug.Log("[HealthDemoChecker] Player ha preso danno sufficiente");

                    // Spawna il medikit
                    SpawnMedikit();
                }
            }
        }
    }

    void CheckHealing()
    {
        if (!hasTakenDamage || hasHealed) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                float currentHealth = ph.currentHealth;

                // Controlla se la salute è aumentata (ha raccolto medikit)
                if (currentHealth > playerLowestHealth + 5f) // Margine per evitare false positive
                {
                    hasHealed = true;
                    Debug.Log("[HealthDemoChecker] Player si è curato");
                }
            }
        }
    }

    void SpawnMedikit()
    {
        if (spawnedMedikit != null) return;

        if (medikitPrefab != null && medikitSpawnPoint != null)
        {
            spawnedMedikit = Instantiate(medikitPrefab, medikitSpawnPoint.position, Quaternion.identity);
            Debug.Log("[HealthDemoChecker] Medikit spawnato");
        }
        else
        {
            Debug.LogWarning("[HealthDemoChecker] Medikit prefab o spawn point non assegnati!");
        }
    }

    void CheckCompletion()
    {
        if (hasTakenDamage && hasHealed)
        {
            CompleteSection();
        }
    }

    void CompleteSection()
    {
        if (sectionCompleted) return;

        sectionCompleted = true;
        Debug.Log("[HealthDemoChecker] Sezione Health completata!");

        // Notifica il TutorialManager
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.CompleteSection("Health Demo");
        }
    }

    void OnGUI()
    {
        if (!showDebugInfo || sectionCompleted) return;

        GUILayout.BeginArea(new Rect(10, 100, 300, 100));
        GUILayout.Label("=== HEALTH DEMO ===");
        GUILayout.Label($"Damage taken: {(hasTakenDamage ? "✓" : "✗")} ({playerInitialHealth - playerLowestHealth:F0}/{damageThreshold})");
        GUILayout.Label($"Healed: {(hasHealed ? "✓" : "✗")}");
        GUILayout.EndArea();
    }
}
