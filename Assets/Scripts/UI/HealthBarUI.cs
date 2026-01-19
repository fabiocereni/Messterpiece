using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Il componente Slider che rappresenta la barra della vita")]
    public Slider healthSlider;

    [Tooltip("Il testo che mostra il nome del player")]
    public TextMeshProUGUI playerNameText;

    [Header("Settings")]
    [Tooltip("Il nome del player da visualizzare")]
    public string playerName = "Abuu";

    [Header("Animation")]
    [Tooltip("Velocità di animazione della barra")]
    public float animationSpeed = 5f;

    private float targetValue = 1f;

    void Start()
    {
        // Imposta il nome del player
        if (playerNameText != null)
        {
            playerNameText.text = playerName;
        }

        // Inizializza lo slider
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
        }
    }

    void Update()
    {
        // Anima la barra smoothly verso il valore target
        if (healthSlider != null)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetValue, Time.deltaTime * animationSpeed);
        }
    }

    // Aggiorna la barra della vita
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        // Calcola la percentuale di vita rimanente
        targetValue = Mathf.Clamp01(currentHealth / maxHealth);
    }
}
