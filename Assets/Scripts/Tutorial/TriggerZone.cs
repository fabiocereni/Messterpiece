using UnityEngine;
using TMPro;

/// <summary>
/// Zona trigger che mostra un messaggio UI quando il player entra.
/// Usato per dare istruzioni durante il tutorial.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class TriggerZone : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI hintText;

    [Header("Hint Message")]
    [SerializeField] [TextArea(2, 5)] private string hintMessage = "Hint message here...";

    [Header("Trigger Settings")]
    [SerializeField] private bool showOnlyOnce = true;
    [SerializeField] private bool hideOnExit = true;
    [SerializeField] private float displayDuration = 0f; // 0 = infinito

    private bool hasBeenTriggered = false;
    private bool playerInside = false;

    void Start()
    {
        // Assicurati che il collider sia impostato come trigger
        BoxCollider col = GetComponent<BoxCollider>();
        col.isTrigger = true;

        // Nascondi il testo all'inizio
        if (hintText != null)
        {
            hintText.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Verifica che sia il player
        if (other.CompareTag("Player") && !hasBeenTriggered)
        {
            playerInside = true;
            ShowHint();

            if (showOnlyOnce)
            {
                hasBeenTriggered = true;
            }

            // Se c'è una durata impostata, nascondi dopo quel tempo
            if (displayDuration > 0f)
            {
                Invoke(nameof(HideHint), displayDuration);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            if (hideOnExit)
            {
                HideHint();
            }
        }
    }

    void ShowHint()
    {
        if (hintText != null)
        {
            hintText.text = hintMessage;
            hintText.gameObject.SetActive(true);
        }
    }

    void HideHint()
    {
        if (hintText != null && !playerInside)
        {
            hintText.gameObject.SetActive(false);
        }
    }

    // Metodo pubblico per forzare il reset (utile per restart tutorial)
    public void ResetTrigger()
    {
        hasBeenTriggered = false;
        HideHint();
    }
}
