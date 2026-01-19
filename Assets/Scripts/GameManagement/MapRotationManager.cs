using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Gestisce la rotazione e selezione delle mappe per il "Gioca Ancora"
// Supporta selezione casuale o sequenziale delle mappe
public class MapRotationManager : MonoBehaviour
{
    public static MapRotationManager Instance { get; private set; }
    
    [Header("Mappe Giocabili")]
    [Tooltip("Lista delle scene delle mappe giocabili")]
    public string[] playableMaps = {
        "PlayerMovementTestScene",  // Mappa di default
        "Level1",
        "Level2", 
        "Level3"
    };
    
    [Header("Impostazioni Selezione")]
    [Tooltip("Usa selezione casuale delle mappe")]
    public bool useRandomSelection = true;
    
    [Tooltip("Evita di giocare la stessa mappa due volte di fila")]
    public bool avoidSameMapTwice = true;
    
    [Header("Debug")]
    [Tooltip("Mostra log di debug per selezione mappe")]
    public bool showDebugLogs = true;
    
    // Stato interno
    private string lastPlayedMap = "";
    private int sequentialIndex = 0;
    private List<string> availableMaps = new List<string>();
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Persistere tra le scene
        DontDestroyOnLoad(gameObject);
        
        // Inizializza lista mappe disponibili
        RefreshAvailableMaps();
        
        if (showDebugLogs)
            Debug.Log($"[MapRotationManager] Inizializzato con {playableMaps.Length} mappe");
    }
    
    // Ottiene la prossima mappa da giocare
    public string GetNextMap()
    {
        if (playableMaps.Length == 0)
        {
            Debug.LogError("[MapRotationManager] Nessuna mappa configurata!");
            return "";
        }
        
        if (playableMaps.Length == 1)
        {
            if (showDebugLogs)
                Debug.Log("[MapRotationManager] Una sola mappa disponibile, ritorno: " + playableMaps[0]);
            return playableMaps[0];
        }
        
        string nextMap;
        
        if (useRandomSelection)
        {
            nextMap = GetRandomMap();
        }
        else
        {
            nextMap = GetSequentialMap();
        }
        
        // Aggiorna l'ultima mappa giocata
        lastPlayedMap = nextMap;
        
        if (showDebugLogs)
            Debug.Log($"[MapRotationManager] Prossima mappa selezionata: {nextMap}");
        
        return nextMap;
    }
    
    // Seleziona una mappa casuale evitando la stessa mappa se richiesto
    private string GetRandomMap()
    {
        // Filtra la mappa se dobbiamo evitare la stessa
        List<string> candidateMaps = new List<string>();
        
        foreach (string map in playableMaps)
        {
            if (!avoidSameMapTwice || map != lastPlayedMap)
            {
                candidateMaps.Add(map);
            }
        }
        
        // Se tutte le mappe sono state escluse (es. solo una mappa), rimuovi il filtro
        if (candidateMaps.Count == 0)
        {
            if (showDebugLogs)
                Debug.LogWarning("[MapRotationManager] Tutte le mappe escluse, rimuovo filtro 'avoidSameMapTwice'");
            
            candidateMaps = new List<string>(playableMaps);
        }
        
        // Seleziona mappa casuale
        int randomIndex = Random.Range(0, candidateMaps.Count);
        return candidateMaps[randomIndex];
    }
    
    // Seleziona la prossima mappa in ordine sequenziale
    private string GetSequentialMap()
    {
        string nextMap = playableMaps[sequentialIndex];
        
        // Avanza l'indice e torna all'inizio se necessario
        sequentialIndex = (sequentialIndex + 1) % playableMaps.Length;
        
        return nextMap;
    }
    
    // Resetta lo stato di rotazione mappe
    public void ResetRotation()
    {
        lastPlayedMap = "";
        sequentialIndex = 0;
        RefreshAvailableMaps();
        
        if (showDebugLogs)
            Debug.Log("[MapRotationManager] Rotazione mappe resettata");
    }
    
    // Aggiorna la lista delle mappe disponibili
    private void RefreshAvailableMaps()
    {
        availableMaps = new List<string>(playableMaps);
        
        if (showDebugLogs)
            Debug.Log($"[MapRotationManager] Mappe disponibili: {string.Join(", ", availableMaps)}");
    }
    
    // Verifica se una mappa è valida e giocabile
    public bool IsValidMap(string mapName)
    {
        return playableMaps.Contains(mapName);
    }
    
    // Ottiene tutte le mappe giocabili
    public string[] GetAllMaps()
    {
        return (string[])playableMaps.Clone();
    }
    
    // Imposta la mappa corrente (usato per sincronizzazione)
    public void SetCurrentMap(string mapName)
    {
        if (IsValidMap(mapName))
        {
            lastPlayedMap = mapName;
            
            if (showDebugLogs)
                Debug.Log($"[MapRotationManager] Mappa corrente impostata: {mapName}");
        }
        else
        {
            Debug.LogWarning($"[MapRotationManager] Mappa non valida: {mapName}");
        }
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    // Metodi per debug da Inspector
    [ContextMenu("Test Get Next Map")]
    public void DebugGetNextMap()
    {
        string nextMap = GetNextMap();
        Debug.Log($"[MapRotationManager] Test - Prossima mappa: {nextMap}");
    }
    
    [ContextMenu("Reset Rotation")]
    public void DebugResetRotation()
    {
        ResetRotation();
    }
}