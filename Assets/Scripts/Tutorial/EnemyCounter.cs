using UnityEngine;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// Gestisce il conteggio dei nemici eliminati nella Combat Arena.
/// Quando tutti i nemici sono morti, sblocca l'Exit Portal.
/// </summary>
public class EnemyCounter : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private int totalEnemies = 3;
    [SerializeField] private TextMeshProUGUI counterText;

    [Header("Completion Event")]
    [SerializeField] private GameObject objectToActivate; // Es: Exit Portal
    [SerializeField] private UnityEvent onAllEnemiesDefeated;

    private int enemiesDefeated = 0;

    void Start()
    {
        UpdateUI();

        // Assicurati che l'oggetto da attivare sia disattivato all'inizio
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(false);
        }
    }

    /// <summary>
    /// Chiamato quando un nemico viene ucciso.
    /// Puoi chiamare questo metodo dal script Enemy quando muore.
    /// </summary>
    public void OnEnemyDefeated()
    {
        enemiesDefeated++;
        UpdateUI();

        // Controlla se tutti i nemici sono stati sconfitti
        if (enemiesDefeated >= totalEnemies)
        {
            OnAllEnemiesCompleted();
        }
    }

    void UpdateUI()
    {
        if (counterText != null)
        {
            counterText.text = $"Nemici: {enemiesDefeated}/{totalEnemies}";
        }
    }

    void OnAllEnemiesCompleted()
    {
        if (counterText != null)
        {
            counterText.text = "Tutti i nemici sconfitti!\nEsci dal portale →";
        }

        // Attiva l'oggetto (es: sblocca Exit Portal)
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
        }

        // Invoca l'evento custom
        onAllEnemiesDefeated?.Invoke();

        Debug.Log("Combat Arena completata! Exit Portal sbloccato.");
    }

    /// <summary>
    /// Reset per riavviare il tutorial
    /// </summary>
    public void ResetCounter()
    {
        enemiesDefeated = 0;
        UpdateUI();

        if (objectToActivate != null)
        {
            objectToActivate.SetActive(false);
        }
    }
}
