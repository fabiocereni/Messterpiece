using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controller unificato per il menu opzioni
/// Gestisce Volume, Difficoltà, Sensibilità e navigazione pannelli
/// </summary>
public class OptionsMenuController : MonoBehaviour
{
    [Header("Audio")]
    [Tooltip("AudioMixer per controllo volume (opzionale - se vuoto usa AudioListener)")]
    public AudioMixer audioMixer;
    
    [Tooltip("Slider per il volume (0-1)")]
    public Slider volumeSlider;
    
    [Tooltip("Testo che mostra il valore del volume")]
    public TextMeshProUGUI volumeText;

    [Header("Difficoltà")]
    [Tooltip("Dropdown per la difficoltà")]
    public TMP_Dropdown difficultyDropdown;

    [Header("Sensibilità Mouse")]
    [Tooltip("Slider per la sensibilità")]
    public Slider sensitivitySlider;
    
    [Tooltip("Testo che mostra il valore della sensibilità")]
    public TextMeshProUGUI sensitivityText;

    [Header("Navigazione Pannelli")]
    [Tooltip("Pulsante per tornare indietro")]
    public Button backButton;
    
    [Tooltip("Pannello delle opzioni (questo)")]
    public GameObject optionsPanel;
    
    [Tooltip("Pannello del menu principale")]
    public GameObject mainMenuPanel;

    [Header("Debug")]
    public bool showDebugLogs = true;

    void Start()
    {
        EnsureGameSettingsExists();
        InitializeUI();
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
                Debug.Log("[OptionsMenu] GameSettings creato automaticamente");
        }
    }

    /// <summary>
    /// Inizializza la UI con i valori salvati
    /// </summary>
    private void InitializeUI()
    {
        // Volume
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = GameSettings.Instance.MasterVolume;
            UpdateVolumeText(volumeSlider.value);
        }

        // Difficoltà
        if (difficultyDropdown != null)
        {
            difficultyDropdown.ClearOptions();
            difficultyDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Facile",
                "Normale", 
                "Difficile"
            });
            difficultyDropdown.value = (int)GameSettings.Instance.CurrentDifficulty;
            difficultyDropdown.RefreshShownValue();
        }

        // Sensibilità
        if (sensitivitySlider != null)
        {
            float savedSens = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
            sensitivitySlider.value = savedSens;
            UpdateSensitivityText(savedSens);
        }

        if (showDebugLogs)
            Debug.Log("[OptionsMenu] UI inizializzata");
    }

    /// <summary>
    /// Configura i listeners per gli eventi UI
    /// </summary>
    private void SetupListeners()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(SetVolume);

        if (difficultyDropdown != null)
            difficultyDropdown.onValueChanged.AddListener(SetDifficulty);

        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.AddListener(SetSensitivity);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
    }

    // ==================== VOLUME ====================
    
    public void SetVolume(float volume)
    {
        // Salva in GameSettings (usa AudioListener.volume internamente)
        GameSettings.Instance.MasterVolume = volume;
        
        // Se c'è un AudioMixer, usa anche quello (in dB)
        if (audioMixer != null)
        {
            float volumeDB = volume > 0.0001f 
                ? Mathf.Log10(volume) * 20f 
                : -80f;
            audioMixer.SetFloat("MasterVolume", volumeDB);
        }

        UpdateVolumeText(volume);

        if (showDebugLogs)
            Debug.Log($"[OptionsMenu] Volume: {volume:P0}");
    }

    private void UpdateVolumeText(float value)
    {
        if (volumeText != null)
            volumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
    }

    // ==================== DIFFICOLTÀ ====================
    
    public void SetDifficulty(int difficultyIndex)
    {
        GameSettings.Instance.CurrentDifficulty = (GameSettings.Difficulty)difficultyIndex;

        if (showDebugLogs)
            Debug.Log($"[OptionsMenu] Difficoltà: {(GameSettings.Difficulty)difficultyIndex}");
    }

    // ==================== SENSIBILITÀ ====================
    
    public void SetSensitivity(float sens)
    {
        PlayerPrefs.SetFloat("Sensitivity", sens);
        PlayerPrefs.Save();
        UpdateSensitivityText(sens);

        if (showDebugLogs)
            Debug.Log($"[OptionsMenu] Sensibilità: {sens}");
    }

    private void UpdateSensitivityText(float value)
    {
        if (sensitivityText != null)
            sensitivityText.text = value.ToString("F0");
    }

    // ==================== NAVIGAZIONE ====================
    
    public void OnBackClicked()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (showDebugLogs)
            Debug.Log("[OptionsMenu] Ritorno al menu principale");
    }

    /// <summary>
    /// Mostra il pannello opzioni (chiamato da MainMenuScript)
    /// </summary>
    public void ShowOptions()
    {
        EnsureGameSettingsExists();
        InitializeUI();

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(true);

        if (showDebugLogs)
            Debug.Log("[OptionsMenu] Pannello opzioni mostrato");
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(SetVolume);

        if (difficultyDropdown != null)
            difficultyDropdown.onValueChanged.RemoveListener(SetDifficulty);

        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.RemoveListener(SetSensitivity);

        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackClicked);
    }
}
