using UnityEngine;

// Gestisce le impostazioni di gioco (volume, difficoltà)
// Singleton persistente che salva le impostazioni in PlayerPrefs
public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }
    
    // Chiavi per PlayerPrefs
    private const string VOLUME_KEY = "MasterVolume";
    private const string DIFFICULTY_KEY = "Difficulty";
    
    // Valori di default
    private const float DEFAULT_VOLUME = 1f;
    private const int DEFAULT_DIFFICULTY = 1; // Normale
    
    // Enum per difficoltà
    public enum Difficulty
    {
        Facile = 0,
        Normale = 1,
        Difficile = 2
    }
    
    // Valori correnti
    private float masterVolume;
    private Difficulty currentDifficulty;
    
    // Proprietà pubbliche
    public float MasterVolume
    {
        get => masterVolume;
        set
        {
            masterVolume = Mathf.Clamp01(value);
            ApplyVolume();
            SaveSettings();
        }
    }
    
    public Difficulty CurrentDifficulty
    {
        get => currentDifficulty;
        set
        {
            currentDifficulty = value;
            SaveSettings();
            Debug.Log($"[GameSettings] Difficoltà impostata: {currentDifficulty}");
        }
    }
    
    // Moltiplicatori per difficoltà (usali nei tuoi script di gioco)
    public float EnemyDamageMultiplier
    {
        get
        {
            switch (currentDifficulty)
            {
                case Difficulty.Facile: return 0.5f;
                case Difficulty.Normale: return 1f;
                case Difficulty.Difficile: return 1.5f;
                default: return 1f;
            }
        }
    }
    
    public float EnemyHealthMultiplier
    {
        get
        {
            switch (currentDifficulty)
            {
                case Difficulty.Facile: return 0.75f;
                case Difficulty.Normale: return 1f;
                case Difficulty.Difficile: return 1.5f;
                default: return 1f;
            }
        }
    }
    
    public float PlayerDamageMultiplier
    {
        get
        {
            switch (currentDifficulty)
            {
                case Difficulty.Facile: return 1.5f;
                case Difficulty.Normale: return 1f;
                case Difficulty.Difficile: return 0.75f;
                default: return 1f;
            }
        }
    }
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Carica impostazioni salvate
        LoadSettings();
        
        Debug.Log($"[GameSettings] Inizializzato - Volume: {masterVolume}, Difficoltà: {currentDifficulty}");
    }
    
    // Carica le impostazioni da PlayerPrefs
    private void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(VOLUME_KEY, DEFAULT_VOLUME);
        currentDifficulty = (Difficulty)PlayerPrefs.GetInt(DIFFICULTY_KEY, DEFAULT_DIFFICULTY);
        
        // Applica volume all'avvio
        ApplyVolume();
    }
    
    // Salva le impostazioni in PlayerPrefs
    private void SaveSettings()
    {
        PlayerPrefs.SetFloat(VOLUME_KEY, masterVolume);
        PlayerPrefs.SetInt(DIFFICULTY_KEY, (int)currentDifficulty);
        PlayerPrefs.Save();
    }
    
    // Applica il volume al sistema audio
    private void ApplyVolume()
    {
        AudioListener.volume = masterVolume;
    }
    
    // Resetta tutte le impostazioni ai valori di default
    public void ResetToDefaults()
    {
        MasterVolume = DEFAULT_VOLUME;
        CurrentDifficulty = (Difficulty)DEFAULT_DIFFICULTY;
        Debug.Log("[GameSettings] Impostazioni resettate ai valori di default");
    }
}
