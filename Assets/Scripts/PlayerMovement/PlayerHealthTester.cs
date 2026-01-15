using UnityEngine;

/// <summary>
/// Script di test per provare la barra della vita del player
/// Premi T per prendere danno, Y per curarsi
/// DA RIMUOVERE prima del build finale!
/// </summary>
public class PlayerHealthTester : MonoBehaviour
{
    [Header("Test Settings")]
    [Tooltip("Riferimento al componente PlayerHealth")]
    public PlayerHealth playerHealth;

    [Tooltip("Quantità di danno da applicare quando si preme T")]
    public float testDamageAmount = 20f;

    [Tooltip("Quantità di cura da applicare quando si preme Y")]
    public float testHealAmount = 30f;

    void Start()
    {
        // Se non è stato assegnato, prova a trovarlo sullo stesso GameObject
        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }

        if (playerHealth == null)
        {
            Debug.LogError("[PlayerHealthTester] PlayerHealth non trovato! Assegna il riferimento nell'Inspector.");
        }
    }

    /*
    void Update()
    {
        if (playerHealth == null) return;

        // Premi T per prendere danno
        if (Input.GetKeyDown(KeyCode.T))
        {
            playerHealth.TakeDamage(testDamageAmount);
            Debug.Log($"[TEST] Player ha preso {testDamageAmount} danno. Vita: {playerHealth.GetCurrentHealth()}/{playerHealth.GetMaxHealth()}");
        }

        // Premi Y per curarsi
        if (Input.GetKeyDown(KeyCode.Y))
        {
            playerHealth.Heal(testHealAmount);
            Debug.Log($"[TEST] Player curato di {testHealAmount}. Vita: {playerHealth.GetCurrentHealth()}/{playerHealth.GetMaxHealth()}");
        }

        // Premi K per uccidere istantaneamente (test morte)
        if (Input.GetKeyDown(KeyCode.K))
        {
            playerHealth.TakeDamage(playerHealth.GetCurrentHealth());
            Debug.Log($"[TEST] Player ucciso! Vita: {playerHealth.GetCurrentHealth()}/{playerHealth.GetMaxHealth()}");
        }

        // Premi H per curare completamente
        if (Input.GetKeyDown(KeyCode.H))
        {
            playerHealth.Heal(playerHealth.GetMaxHealth());
            Debug.Log($"[TEST] Player curato completamente! Vita: {playerHealth.GetCurrentHealth()}/{playerHealth.GetMaxHealth()}");
        }
    }
    */
}
