using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controller per la UI del menu opzioni
/// Collega slider e dropdown a GameSettings
/// </summary>
public class OptionsMenuUI : MonoBehaviour
{
    [Header("Riferimenti UI")]
    [Tooltip("Slider per il volume (0-1)")]
    public Slider volumeSlider;
    
    [Tooltip("Testo che mostra il valore del volume (es. '50%')")]
    public TextMeshProUGUI volumeValueText;
    
    [Tooltip("Dropdown per la difficoltà")]
    public TMP_Dropdown difficultyDropdown;
    
    [Tooltip("Pulsante per tornare indietro")]
    public Button backButton;
    
    [Header("Pannelli")]
    [Tooltip("Pannello delle opzioni (questo)")]
    public GameObject optionsPanel;
    
    [Tooltip("Pannello del menu principale")]
    public GameObject mainMenuPanel;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    private void Start()
    {
        // Assicurati che GameSettings esista
        EnsureGameSettingsExists();
        
        // Inizializza UI con valori correnti
        InitializeUI();
        
        // Configura listeners
        SetupListeners();
    }
    
    /// <summary>
    /// Crea GameSettings se non esiste
    /// </summary>
    private void EnsureGameSettingsExists()
    {
        if (GameSettings.Instance == null)
        {
            GameObject settingsObj = new GameObject("GameSettings");
            settingsObj.AddComponent<GameSettings>();
            
            if (showDebugLogs)
                Debug.Log("[OptionsMenuUI] GameSettings creato automaticamente");
        }
    }
    
    /// <summary>
    /// Inizializza la UI con i valori salvati
    /// </summary>
    private void InitializeUI()
    {
        // Volume slider
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = GameSettings.Instance.MasterVolume;
            UpdateVolumeText(volumeSlider.value);
        }
        
        // Difficulty dropdown
        if (difficultyDropdown != null)
        {
            // Pulisci e aggiungi opzioni
            difficultyDropdown.ClearOptions();
            difficultyDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Facile",
                "Normale",
                "Difficile"
            });
            
            // Imposta valore corrente
            difficultyDropdown.value = (int)GameSettings.Instance.CurrentDifficulty;
            difficultyDropdown.RefreshShownValue();
        }
        
        if (showDebugLogs)
            Debug.Log("[OptionsMenuUI] UI inizializzata");
    }
    
    /// <summary>
    /// Configura i listeners per gli eventi UI
    /// </summary>
    private void SetupListeners()
    {
        // Volume slider
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        
        // Difficulty dropdown
        if (difficultyDropdown != null)
        {
            difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
        }
        
        // Back button
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }
    }
    
    /// <summary>
    /// Chiamato quando il volume cambia
    /// </summary>
    private void OnVolumeChanged(float value)
    {
        GameSettings.Instance.MasterVolume = value;
        UpdateVolumeText(value);
        
        if (showDebugLogs)
            Debug.Log($"[OptionsMenuUI] Volume: {value:P0}");
    }
    
    /// <summary>
    /// Aggiorna il testo del volume
    /// </summary>
    private void UpdateVolumeText(float value)
    {
        if (volumeValueText != null)
        {
            volumeValueText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }
    
    /// <summary>
    /// Chiamato quando la difficoltà cambia
    /// </summary>
    private void OnDifficultyChanged(int index)
    {
        GameSettings.Instance.CurrentDifficulty = (GameSettings.Difficulty)index;
        
        if (showDebugLogs)
            Debug.Log($"[OptionsMenuUI] Difficoltà: {(GameSettings.Difficulty)index}");
    }
    
    /// <summary>
    /// Chiamato quando si preme "Indietro"
    /// </summary>
    public void OnBackClicked()
    {
        if (showDebugLogs)
            Debug.Log("[OptionsMenuUI] Ritorno al menu principale");
        
        // Nascondi opzioni
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
        
        // Mostra menu principale
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }
    
    /// <summary>
    /// Mostra il pannello opzioni (chiamato da MainMenuScript)
    /// </summary>
    public void ShowOptions()
    {
        // Assicurati che GameSettings esista
        EnsureGameSettingsExists();
        
        // Aggiorna UI con valori correnti
        InitializeUI();
        
        // Nascondi menu principale
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        
        // Mostra opzioni
        if (optionsPanel != null)
            optionsPanel.SetActive(true);
        
        if (showDebugLogs)
            Debug.Log("[OptionsMenuUI] Pannello opzioni mostrato");
    }
    
    private void OnDestroy()
    {
        // Rimuovi listeners
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        
        if (difficultyDropdown != null)
            difficultyDropdown.onValueChanged.RemoveListener(OnDifficultyChanged);
        
        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackClicked);
    }
}
