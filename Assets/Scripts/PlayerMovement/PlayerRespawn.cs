using UnityEngine;

/// <summary>
/// Sistema respawn migliorato per evitare game over alla morte del giocatore
/// Gestisce respawn, spawn points e effetti visivi
/// </summary>
public class PlayerRespawn : MonoBehaviour
{
    [Header("Spawn Points")]
    [Tooltip("Lista degli spawn point disponibili")]
    public Transform[] spawnPoints;
    
    [Header("Auto Spawn Points")]
    [Tooltip("Cerca automaticamente spawn point con questo tag")]
    public string spawnPointTag = "SpawnPoint";
    [Tooltip("Usa spawn point automatici se l'array è vuoto")]
    public bool useAutoSpawnPoints = true;
    
    [Header("Respawn Settings")]
    [Tooltip("Delay prima del respawn in secondi")]
    public float respawnDelay = 2f;
    
    [Tooltip("Salute dopo il respawn")]
    public float respawnHealth = 100f;
    
    [Tooltip("Munizioni dopo il respawn")]
    public int respawnAmmo = 30;
    
    [Header("Visual Effects")]
    [Tooltip("Effetto visivo durante il respawn")]
    public GameObject respawnEffect;
    
    [Tooltip("Suono del respawn")]
    public AudioClip respawnSound;
    
    [Tooltip("Camera shake durante respawn")]
    public bool useCameraShake = true;
    [Tooltip("Intensità camera shake")]
    public float shakeIntensity = 2f;
    [Tooltip("Durata camera shake")]
    public float shakeDuration = 0.5f;
    
    [Header("Debug")]
    [Tooltip("Mostra log di debug")]
    public bool showDebugLogs = true;
    
    // Componenti
    private AudioSource audioSource;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private PlayerLook playerLook;
    private Gun playerGun;
    private AmmoDisplay ammoDisplay;
    
    // Stato
    private bool isRespawning = false;
    private Transform originalSpawnPoint;
    
    private void Awake()
    {
        // Ottieni i componenti necessari
        GetRequiredComponents();
        
        // Salva lo spawn point originale
        if (transform != null)
        {
            originalSpawnPoint = transform;
        }
    }
    
    /// <summary>
    /// Ottiene i componenti necessari
    /// </summary>
    private void GetRequiredComponents()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerMovement = GetComponent<PlayerMovement>();
        playerLook = GetComponent<PlayerLook>();
        playerGun = GetComponent<Gun>();
        
        // Cerca AmmoDisplay nella scena
        ammoDisplay = FindObjectOfType<AmmoDisplay>();
        
        // Audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        if (showDebugLogs)
            Debug.Log("[PlayerRespawn] Componenti ottenuti");
    }
    
    /// <summary>
    /// Metodo principale per respawnare il giocatore
    /// </summary>
    /// <param name="player">GameObject del giocatore da respawnare</param>
    public void RespawnPlayer(GameObject player)
    {
        if (isRespawning)
        {
            if (showDebugLogs)
                Debug.LogWarning("[PlayerRespawn] Respawn già in corso");
            return;
        }
        
        if (showDebugLogs)
            Debug.Log("[PlayerRespawn] Inizio respawn del giocatore");
        
        StartCoroutine(RespawnCoroutine(player));
    }
    
    /// <summary>
    /// Coroutine per gestire il respawn con delay
    /// </summary>
    private System.Collections.IEnumerator RespawnCoroutine(GameObject player)
    {
        isRespawning = true;
        
        // Disabilita controlli giocatore
        DisablePlayerControls();
        
        // Aspetta il delay
        yield return new WaitForSeconds(respawnDelay);
        
        // Trova spawn point
        Transform spawnPoint = GetBestSpawnPoint();
        if (spawnPoint == null)
        {
            Debug.LogError("[PlayerRespawn] Nessuno spawn point disponibile!");
            isRespawning = false;
            yield break;
        }
        
        // Teletrasporta giocatore
        TeleportPlayerToSpawn(player, spawnPoint);
        
        // Ripristina salute
        RestorePlayerHealth();
        
        // Ripristina munizioni
        RestorePlayerAmmo();
        
        // Effetti visivi e sonori
        PlayRespawnEffects();
        
        // Camera shake
        if (useCameraShake)
        {
            TriggerCameraShake();
        }
        
        // Riabilita controlli
        EnablePlayerControls();
        
        isRespawning = false;
        
        if (showDebugLogs)
            Debug.Log($"[PlayerRespawn] Giocatore respawnato con successo a {spawnPoint.name}");
    }
    
    /// <summary>
    /// Trova il miglior spawn point disponibile
    /// </summary>
    private Transform GetBestSpawnPoint()
    {
        // Prima controlla se ci sono spawn point manuali
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // Scegli spawn point casuale
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Transform selectedSpawn = spawnPoints[randomIndex];
            
            if (showDebugLogs)
                Debug.Log($"[PlayerRespawn] Spawn point manuale selezionato: {selectedSpawn.name}");
            
            return selectedSpawn;
        }
        
        // Poi controlla spawn point automatici con tag
        if (useAutoSpawnPoints && !string.IsNullOrEmpty(spawnPointTag))
        {
            GameObject[] spawnObjects = GameObject.FindGameObjectsWithTag(spawnPointTag);
            if (spawnObjects.Length > 0)
            {
                int randomIndex = Random.Range(0, spawnObjects.Length);
                Transform selectedSpawn = spawnObjects[randomIndex].transform;
                
                if (showDebugLogs)
                    Debug.Log($"[PlayerRespawn] Spawn point automatico selezionato: {selectedSpawn.name}");
                
                return selectedSpawn;
            }
        }
        
        // Fallback alla posizione originale
        if (originalSpawnPoint != null)
        {
            if (showDebugLogs)
                Debug.Log("[PlayerRespawn] Uso spawn point originale");
            return originalSpawnPoint;
        }
        
        // Ultimo fallback: posizione corrente
        if (showDebugLogs)
            Debug.LogWarning("[PlayerRespawn] Nessuno spawn point trovato, uso posizione corrente");
        return transform;
    }
    
    /// <summary>
    /// Teletrasporta il giocatore allo spawn point
    /// </summary>
    private void TeleportPlayerToSpawn(GameObject player, Transform spawnPoint)
    {
        // Resetta velocity del rigidbody
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // Teletrasporta
        player.transform.position = spawnPoint.position;
        player.transform.rotation = spawnPoint.rotation;
    }
    
    /// <summary>
    /// Ripristina la salute del giocatore
    /// </summary>
    private void RestorePlayerHealth()
    {
        if (playerHealth != null)
        {
            playerHealth.RestoreHealth(respawnHealth);
            
            if (showDebugLogs)
                Debug.Log($"[PlayerRespawn] Salute ripristinata a {respawnHealth}");
        }
    }
    
    /// <summary>
    /// Ripristina le munizioni del giocatore
    /// </summary>
    private void RestorePlayerAmmo()
    {
        if (playerGun != null)
        {
            // Resetta direttamente le munizioni al massimo
            // Poiché Reload() è privato, impostiamo direttamente currentAmmo
            var currentAmmoField = typeof(Gun).GetField("currentAmmo", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (currentAmmoField != null)
            {
                var maxAmmoField = typeof(Gun).GetField("maxAmmo", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                
                if (maxAmmoField != null)
                {
                    int maxAmmo = (int)maxAmmoField.GetValue(playerGun);
                    currentAmmoField.SetValue(playerGun, maxAmmo);
                    
                    if (showDebugLogs)
                        Debug.Log($"[PlayerRespawn] Munizioni ripristinate a {maxAmmo}");
                }
            }
        }
        
        // L'AmmoDisplay si aggiorna automaticamente nel suo Update()
        // Non c'è bisogno di chiamare UpdateAmmoDisplay() perché non esiste
    }
    
    /// <summary>
    /// Disabilita i controlli del giocatore
    /// </summary>
    private void DisablePlayerControls()
    {
        if (playerMovement != null)
            playerMovement.enabled = false;
        
        if (playerLook != null)
            playerLook.enabled = false;
        
        if (playerGun != null)
            playerGun.enabled = false;
    }
    
    /// <summary>
    /// Riabilita i controlli del giocatore
    /// </summary>
    private void EnablePlayerControls()
    {
        if (playerMovement != null)
            playerMovement.enabled = true;
        
        if (playerLook != null)
            playerLook.enabled = true;
        
        if (playerGun != null)
            playerGun.enabled = true;
    }
    
    /// <summary>
    /// Riproduce effetti visivi e sonori del respawn
    /// </summary>
    private void PlayRespawnEffects()
    {
        // Effetto visivo
        if (respawnEffect != null)
        {
            GameObject effect = Instantiate(respawnEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f); // Distruggi dopo 3 secondi
        }
        
        // Suono
        if (respawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(respawnSound);
        }
    }
    
    /// <summary>
    /// Attiva camera shake
    /// </summary>
    private void TriggerCameraShake()
    {
        // Trova la camera principale
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Cerca un componente camera shake o aggiungilo dinamicamente
            var cameraShake = mainCamera.GetComponent("CameraShake");
            if (cameraShake != null)
            {
                // Usa reflection per chiamare il metodo Shake se disponibile
                var shakeMethod = cameraShake.GetType().GetMethod("Shake");
                if (shakeMethod != null)
                {
                    shakeMethod.Invoke(cameraShake, new object[] { shakeIntensity, shakeDuration });
                }
            }
            else
            {
                // Se non c'è CameraShake, usa il semplice shake posizione
                StartCoroutine(SimpleCameraShake(mainCamera.transform));
            }
        }
    }
    
    /// <summary>
    /// Camera shake semplice senza componente dedicato
    /// </summary>
    private System.Collections.IEnumerator SimpleCameraShake(Transform cameraTransform)
    {
        Vector3 originalPosition = cameraTransform.localPosition;
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;
            
            cameraTransform.localPosition = originalPosition + new Vector3(x, y, 0);
            
            yield return null;
        }
        
        cameraTransform.localPosition = originalPosition;
    }
    
    /// <summary>
    /// Aggiunge uno spawn point alla lista
    /// </summary>
    public void AddSpawnPoint(Transform spawnPoint)
    {
        if (spawnPoint == null) return;
        
        // Crea o espandi l'array
        if (spawnPoints == null)
        {
            spawnPoints = new Transform[1];
            spawnPoints[0] = spawnPoint;
        }
        else
        {
            System.Array.Resize(ref spawnPoints, spawnPoints.Length + 1);
            spawnPoints[spawnPoints.Length - 1] = spawnPoint;
        }
        
        if (showDebugLogs)
            Debug.Log($"[PlayerRespawn] Aggiunto spawn point: {spawnPoint.name}");
    }
    
    /// <summary>
    /// Rimuovi uno spawn point dalla lista
    /// </summary>
    public void RemoveSpawnPoint(Transform spawnPoint)
    {
        if (spawnPoints == null || spawnPoint == null) return;
        
        int index = System.Array.IndexOf(spawnPoints, spawnPoint);
        if (index >= 0)
        {
            // Sposta gli elementi per rimuovere l'elemento
            for (int i = index; i < spawnPoints.Length - 1; i++)
            {
                spawnPoints[i] = spawnPoints[i + 1];
            }
            
            System.Array.Resize(ref spawnPoints, spawnPoints.Length - 1);
            
            if (showDebugLogs)
                Debug.Log($"[PlayerRespawn] Rimosso spawn point: {spawnPoint.name}");
        }
    }
    
    /// <summary>
    /// Controlla se il respawn è in corso
    /// </summary>
    public bool IsRespawning()
    {
        return isRespawning;
    }
    
    // Metodi per debug
    [ContextMenu("Test Respawn")]
    public void DebugRespawn()
    {
        if (gameObject != null)
            RespawnPlayer(gameObject);
    }
    
    [ContextMenu("Log Spawn Points")]
    public void DebugLogSpawnPoints()
    {
        if (spawnPoints != null)
        {
            Debug.Log($"[PlayerRespawn] Spawn points: {spawnPoints.Length}");
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Debug.Log($"  {i + 1}. {spawnPoints[i].name} at {spawnPoints[i].position}");
            }
        }
        else
        {
            Debug.Log("[PlayerRespawn] Nessuno spawn point configurato");
        }
    }
}