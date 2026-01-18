using UnityEngine;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// Gestisce il conteggio dei target distrutti nello shooting range.
/// Quando tutti i target sono distrutti, sblocca la porta/attiva un evento.
/// </summary>
public class TargetCounter : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private int totalTargets = 6;
    [SerializeField] private TextMeshProUGUI counterText;

    [Header("Completion Event")]
    [SerializeField] private GameObject objectToActivate; // Es: porta, barrier, etc.
    [SerializeField] private UnityEvent onAllTargetsDestroyed;

    private int targetsDestroyed = 0;

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
    /// Chiamato quando un target viene distrutto.
    /// Puoi chiamare questo metodo dal Target script.
    /// </summary>
    public void OnTargetDestroyed()
    {
        targetsDestroyed++;
        UpdateUI();

        // Controlla se tutti i target sono stati distrutti
        if (targetsDestroyed >= totalTargets)
        {
            OnAllTargetsCompleted();
        }
    }

    void UpdateUI()
    {
        if (counterText != null)
        {
            counterText.text = $"Targets: {targetsDestroyed}/{totalTargets}";
        }
    }

    void OnAllTargetsCompleted()
    {
        if (counterText != null)
        {
            counterText.text = "All targets destroyed! Proceed →";
        }

        // Attiva l'oggetto (es: apri la porta)
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
        }

        // Invoca l'evento custom
        onAllTargetsDestroyed?.Invoke();

        // Notifica il TutorialManager
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.CompleteSection("Shooting");
        }

        Debug.Log("Tutorial Shooting Range completato!");
    }

    /// <summary>
    /// Reset per riavviare il tutorial
    /// </summary>
    public void ResetCounter()
    {
        targetsDestroyed = 0;
        UpdateUI();

        if (objectToActivate != null)
        {
            objectToActivate.SetActive(false);
        }
    }
}
