using UnityEngine;

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
}
